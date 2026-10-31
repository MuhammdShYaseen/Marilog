using Marilog.Application.Interfaces.Services.Domain;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.SystemServices
{
    public class DocumentNumberService : IDocumentNumberService
    {
        private const int MaxAttempts = 3;

        private readonly IRepository<IssuedNumber> _repo;

        public DocumentNumberService(IRepository<IssuedNumber> repo)
        {
            _repo = repo;
        }

        public async Task<string> GenerateAsync(EntityType entityType, Guid entityId, string code, string? description, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Code is required.");

            var prefix = Normalize(code);
            var year = DateTime.UtcNow.Year;
            

            for (var attempt = 1; ; attempt++)
            {
                var max = await _repo.Query()
                    .Where(x => x.Code == prefix && x.Year == year)
                    .MaxAsync(x => (int?)x.Sequence, ct);

                var sequence = (max ?? 0) + 1;
                var number = Format(prefix, sequence, year);

                var record = IssuedNumber.Create(entityType, entityId, prefix, year,
                    sequence, number, description);

                await _repo.AddAsync(record, ct);

                try
                {
                    await _repo.SaveChangesAsync(ct);
                    return number;
                }
                catch (DbUpdateException) when (attempt < MaxAttempts)
                {
                    // Same number taken concurrently — drop it and try the next one
                    _repo.HardDelete(record);
                }
            }
        }

        public async Task<string> CreateStartAsync(EntityType entityType, Guid entityId, string code, int year, int start, string? description, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidOperationException("Code is required.");

            if (start <= 0)
                throw new InvalidOperationException("Start must be greater than zero.");

            var prefix = Normalize(code);

            var started = await _repo.Query()
                .AnyAsync(x => x.Code == prefix && x.Year == year, ct);

            if (started)
                throw new InvalidOperationException($"Numbering for {prefix} in {year} has already started.");

            var number = Format(prefix, start, year);

            await _repo.AddAsync(IssuedNumber.Create(entityType, entityId, prefix, year,
                start, number, description), ct);
            await _repo.SaveChangesAsync(ct);

            return number;
        }

        public async Task<string> UpdateStartAsync(Guid id, int start, string? description,
            CancellationToken ct = default)
        {
            if (start <= 0)
                throw new InvalidOperationException("Start must be greater than zero.");

            var record = await _repo.Query().FirstOrDefaultAsync(x => x.Guid == id, ct)
                ?? throw new InvalidOperationException($"Issued number {id} not found.");

            var count = await _repo.Query()
                .CountAsync(x => x.Code == record.Code && x.Year == record.Year, ct);

            if (count > 1)
                throw new InvalidOperationException(
                    $"Start of {record.Code} in {record.Year} is locked — numbers have already been issued after it.");

            var number = Format(record.Code, start, record.Year);

            record.ChangeSequence(start, number, description);
            _repo.Update(record);
            await _repo.SaveChangesAsync(ct);

            return number;
        }

        private static string Normalize(string code) => code.Trim().ToUpperInvariant();

        private static string Format(string prefix, int sequence, int year)
            => $"{prefix}-{sequence:D4}-{year}";
    }
}