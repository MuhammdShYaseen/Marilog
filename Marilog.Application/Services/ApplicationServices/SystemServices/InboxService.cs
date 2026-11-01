using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Reports.InboxEntryReport;
using Marilog.Contracts.DTOs.Requests.InboxEntryDTOs;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class InboxService : IInboxService
    {
        private const string Prefix = "IN";
        private const int MaxAttempts = 3;
        private readonly IRepository<InboxEntry> _entries;
        private readonly IRepository<InboxEntryLink> _links;

        public InboxService(IRepository<InboxEntry> entries, IRepository<InboxEntryLink> links)
        {
            _entries = entries;
            _links = links;
        }

        //==== Commands ==========================================================================================
        public async Task<string> CreateAsync(DateOnly receivedDate, InboxSource source, List<int> vesselIds, string description, int? sequence = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidOperationException("Description is required.");

            var year = receivedDate.Year;

            if (sequence is not null)
            {
                if (sequence <= 0)
                    throw new InvalidOperationException("Start must be greater than zero.");

                if (await _entries.Query().AnyAsync(x => x.Year == year && x.Sequence == sequence, ct))
                     throw new InvalidOperationException($"{Format(sequence.Value, year)} is already used.");
            }

            for (var attempt = 1; ; attempt++)
            {
                var next = sequence
                     ?? (await _entries.Query()
                            .Where(x => x.Year == year)
                            .MaxAsync(x => (int?)x.Sequence, ct) ?? 0) + 1;

                var number = Format(next, year);
                var entry = InboxEntry.Create(year, next, number, receivedDate,
                     source, vesselIds, description.Trim());

                await _entries.AddAsync(entry, ct);

                try
                {
                    await _entries.SaveChangesAsync(ct);
                    return number;
                }
                catch (DbUpdateException) when (attempt < MaxAttempts && sequence is null)
                {
                    _entries.HardDelete(entry);
                }
            }
        }


        public async Task<List<string>> CreateRangeAsync(List<CreateInboxEntryItem> items, bool keepInputOrder = false, CancellationToken ct = default)
        {
            if (items is null || items.Count == 0)
                throw new InvalidOperationException("At least one entry is required.");

            if (items.Any(x => string.IsNullOrWhiteSpace(x.Description)))
                throw new InvalidOperationException("Description is required for every entry.");

            if (items.Any(x => x.Sequence is <= 0))
                throw new InvalidOperationException("Manual number must be greater than zero.");

            var manual = items
                .Where(x => x.Sequence is not null)
                .Select(x => (Year: x.ReceivedDate.Year, Sequence: x.Sequence!.Value))
                .ToList();

            var repeated = manual
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => Format(g.Key.Sequence, g.Key.Year))
                .ToList();

            if (repeated.Count > 0)
                throw new InvalidOperationException($"Repeated in the batch: {string.Join(", ", repeated)}.");

            var manualSet = manual.ToHashSet();
            var manualSequences = manual.Select(x => x.Sequence).Distinct().ToList();

            var indexed = items.Select((item, index) => (item, index));
            var ordered = keepInputOrder
                ? indexed.ToList()
                : indexed.OrderBy(x => x.item.ReceivedDate).ToList();

            var years = items.Select(x => x.ReceivedDate.Year).Distinct().ToList();

            for (var attempt = 1; ; attempt++)
            {
                if (manual.Count > 0)
                {
                    var taken = await _entries.Query()
                        .Where(x => years.Contains(x.Year) && manualSequences.Contains(x.Sequence))
                        .Select(x => new { x.Year, x.Sequence })
                        .ToListAsync(ct);

                    var conflicts = taken
                        .Where(t => manualSet.Contains((t.Year, t.Sequence)))
                        .Select(t => Format(t.Sequence, t.Year))
                        .ToList();

                    if (conflicts.Count > 0)
                        throw new InvalidOperationException($"Already used: {string.Join(", ", conflicts)}.");
                }

                var lastByYear = await _entries.Query()
                    .Where(x => years.Contains(x.Year))
                    .GroupBy(x => x.Year)
                    .Select(g => new { Year = g.Key, Max = g.Max(x => x.Sequence) })
                    .ToDictionaryAsync(x => x.Year, x => x.Max, ct);

                // Auto numbers start after the highest of existing and manual numbers
                foreach (var (year, sequence) in manual)
                    lastByYear[year] = Math.Max(lastByYear.GetValueOrDefault(year), sequence);

                var numbers = new string[items.Count];
                var added = new List<InboxEntry>(items.Count);

                foreach (var (item, index) in ordered)
                {
                    var year = item.ReceivedDate.Year;
                    int sequence;

                    if (item.Sequence is not null)
                    {
                        sequence = item.Sequence.Value;
                    }
                    else
                    {
                        sequence = lastByYear.GetValueOrDefault(year) + 1;
                        lastByYear[year] = sequence;
                    }

                    var number = Format(sequence, year);
                    var entry = InboxEntry.Create(year, sequence, number, item.ReceivedDate,
                        item.Source, item.VesselIds ?? [], item.Description.Trim());

                    await _entries.AddAsync(entry, ct);
                    added.Add(entry);
                    numbers[index] = number;
                }

                try
                {
                    await _entries.SaveChangesAsync(ct);
                    return numbers.ToList();
                }
                catch (DbUpdateException) when (attempt < MaxAttempts)
                {
                    foreach (var entry in added)
                        _entries.HardDelete(entry);
                }
            }
        }

        public async Task UpdateAsync(int id, DateOnly receivedDate, InboxSource source, List<int> vesselIds, string description, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new InvalidOperationException("Description is required.");

            var entry = await GetAsync(id, ct);

            if (receivedDate.Year != entry.Year)
                throw new InvalidOperationException(
                    $"Received date must stay in {entry.Year} — {entry.Number} belongs to that year.");

            entry.Update(receivedDate, source, vesselIds, description.Trim());
            _entries.Update(entry);
            await _entries.SaveChangesAsync(ct);
        }
        public async Task<string> ChangeStartAsync(int id, int sequence, CancellationToken ct = default)
        {
            if (sequence <= 0)
                throw new InvalidOperationException("Start must be greater than zero.");

            var entry = await GetAsync(id, ct);

            var count = await _entries.Query().CountAsync(x => x.Year == entry.Year, ct);
            if (count > 1)
                throw new InvalidOperationException(
                    $"Start of {entry.Year} is locked — entries have already been added after it.");

            var number = Format(sequence, entry.Year);
            entry.ChangeSequence(sequence, number);
            _entries.Update(entry);
            await _entries.SaveChangesAsync(ct);

            return number;
        }
        public async Task LinkAsync(int id, EntityType entityType, int entityId,
            CancellationToken ct = default)
        {
            var entry = await GetAsync(id, ct);

            var exists = await _links.Query().AnyAsync(x => x.InboxEntryId == id
                && x.EntityType == entityType && x.EntityId == entityId, ct);

            if (!exists)
                await _links.AddAsync(InboxEntryLink.Create(id, entityType, entityId), ct);

            entry.MarkProcessed();
            _entries.Update(entry);
            await _entries.SaveChangesAsync(ct);
        }

        public async Task UnlinkAsync(int linkId, CancellationToken ct = default)
        {
            var link = await _links.Query().FirstOrDefaultAsync(x => x.Id == linkId, ct)
                ?? throw new InvalidOperationException($"Link {linkId} not found.");

            _links.HardDelete(link);

            var hasOtherLinks = await _links
                .Query()
                .AnyAsync(x => x.InboxEntryId == link.InboxEntryId && x.Id != linkId, ct);

            if (!hasOtherLinks)
            {
                var entry = await GetAsync(link.InboxEntryId, ct);
                entry.Reopen();
                _entries.Update(entry);
            }

            await _links.SaveChangesAsync(ct);
        }

        /// <summary>For entries that need no record (SMS, confirmations…).</summary>
        public async Task MarkProcessedAsync(int id, CancellationToken ct = default)
        {
            var entry = await GetAsync(id, ct);
            entry.MarkProcessed();
            _entries.Update(entry);
            await _entries.SaveChangesAsync(ct);
        }

        /// <summary>The number stays reserved — never reused.</summary>
        public async Task CancelAsync(int id, CancellationToken ct = default)
        {
            var entry = await GetAsync(id, ct);
            entry.Cancel();
            _entries.Update(entry);
            await _entries.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var entry = await GetAsync(id, ct);

            // Links from other entries pointing to this one (e.g. "Paid 466")
            var pointingHere = await _links.Query()
                .Where(x => x.EntityType == EntityType.InboxEntry && x.EntityId == id)
                .ToListAsync(ct);

            foreach (var link in pointingHere)
                _links.HardDelete(link);

            _entries.HardDelete(entry);
            await _entries.SaveChangesAsync(ct);
        }

        //==== Quares ==========================================================================================
        public async Task<PagedResponse<InboxEntryResponse>> GetFilteredAsync(InboxEntryFilter filter, CancellationToken ct = default)
        {
            var query = ApplySearch(_entries.Query(), filter.SearchTerm);

            if (filter.Year is not null)
                query = query.Where(x => x.Year == filter.Year);

            if (filter.From is not null)
                query = query.Where(x => x.ReceivedDate >= filter.From);

            if (filter.To is not null)
                query = query.Where(x => x.ReceivedDate <= filter.To);

            if (filter.Source is not null)
                query = query.Where(x => x.Source == filter.Source);

            if (filter.VesselId is not null)
            {
                var vesselId = filter.VesselId.Value;
                query = query.Where(x => x.VesselIds.Contains(vesselId));
            }

            if (filter.IsProcessed is not null)
                query = query.Where(x => x.IsProcessed == filter.IsProcessed);

            if (filter.IsCancelled is not null)
                query = query.Where(x => x.IsCancelled == filter.IsCancelled);

            if (filter.LinkedEntityType is not null)
            {
                var links = _links.Query();
                var type = filter.LinkedEntityType.Value;
                var entityId = filter.LinkedEntityId;

                query = query.Where(x => links.Any(l => l.InboxEntryId == x.Id
                    && l.EntityType == type
                    && (entityId == null || l.EntityId == entityId)));
            }
            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Sequence)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(ToResponse)
                .ToListAsync(ct);

            return new PagedResponse<InboxEntryResponse>
            {
                Items = await AttachLinksAsync(items, ct),
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<PagedResponse<InboxEntryResponse>> GetPagedAsync(PagedRequest request,
            CancellationToken ct = default)
        {
            var query = ApplySearch(_entries.Query(), request.SearchTerm);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Sequence)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ToResponse)
                .ToListAsync(ct);

            return new PagedResponse<InboxEntryResponse>
            {
                Items = await AttachLinksAsync(items, ct),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }


        //==== Privates ==========================================================================================
        private static readonly Expression<Func<InboxEntry, InboxEntryResponse>> ToResponse =
            x => new InboxEntryResponse(x.Id, x.Year, x.Sequence, x.Number, x.ReceivedDate,
                x.Source, x.VesselIds, x.Description, x.IsProcessed, x.IsCancelled);
        private static IQueryable<InboxEntry> ApplySearch(IQueryable<InboxEntry> query, string? searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return query;

            var term = searchTerm.Trim();
            return query.Where(x => x.Number.Contains(term) || x.Description.Contains(term));
        }

        private async Task<List<InboxEntryResponse>> AttachLinksAsync(List<InboxEntryResponse> items,
            CancellationToken ct)
        {
            if (items.Count == 0) return items;

            var ids = items.Select(x => x.Id).ToList();

            var links = await _links.Query()
                .Where(x => ids.Contains(x.InboxEntryId))
                .Select(x => new
                {
                    x.InboxEntryId,
                    Link = new InboxEntryLinkResponse(x.Id, x.EntityType, x.EntityId)
                })
                .ToListAsync(ct);

            var byEntry = links.ToLookup(x => x.InboxEntryId, x => x.Link);

            return items.Select(x => x with { Links = byEntry[x.Id].ToList() }).ToList();
        }

        private async Task<InboxEntry> GetAsync(int id, CancellationToken ct)
            => await _entries.Query().FirstOrDefaultAsync(x => x.Id == id, ct)
               ?? throw new InvalidOperationException($"Inbox entry {id} not found.");

        private static string Format(int sequence, int year) => $"{Prefix}-{sequence:D4}-{year}";
    }
}