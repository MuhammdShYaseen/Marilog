using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Entities.NotifiEntities;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using AuditEntry = Marilog.Domain.Entities.UsersLog.AuditLog;

namespace Marilog.Application.Services.ApplicationServices.NotifiServices
{
    /// <summary>
    /// Turns new AuditLog rows into notifications for the users who follow the record,
    /// or a record it belongs to (a payment → its document and its SWIFT).
    /// Run every minute by AuditNotificationJob.
    /// </summary>
    public class AuditNotificationService
    {
        private const int BatchSize = 500;
        private const int MaxPhrases = 3;
        private const int MaxFields = 4;

        // Never shown to the user
        private static readonly HashSet<string> HiddenFields = new(StringComparer.OrdinalIgnoreCase)
        {
            "Id", "Guid", "IsDeleted", "CreatedAt", "UpdatedAt"
        };

        private readonly IAuditLogRepository _audit;
        private readonly INotificationRepository _notifications;
        private readonly IRepository<Payment> _payments;
        private readonly IRepository<DocumentAdjustment> _adjustments;
        private readonly IRepository<Document> _documents;
        private readonly IRepository<SwiftBankFee> _swiftFees;
        private readonly IRepository<CrewPayrollDisbursement> _disbursements;
        private readonly IRepository<CrewPayroll> _payrolls;
        private readonly IRepository<CharterTerms> _charterTerms;
        private readonly IRepository<InboxEntryLink> _inboxLinks;
        private readonly IRepository<Vessel> _vessels;
        private readonly IRepository<Person> _persons;
        private readonly IRepository<Comment> _comments;
        private readonly IRepository<StoredFile> _files;

        public AuditNotificationService(
            IAuditLogRepository audit,
            INotificationRepository notifications,
            IRepository<Payment> payments,
            IRepository<DocumentAdjustment> adjustments,
            IRepository<Document> documents,
            IRepository<SwiftBankFee> swiftFees,
            IRepository<CrewPayrollDisbursement> disbursements,
            IRepository<CrewPayroll> payrolls,
            IRepository<CharterTerms> charterTerms,
            IRepository<InboxEntryLink> inboxLinks,
            IRepository<Vessel> vessels,
            IRepository<Person> persons,
            IRepository<Comment> comments,
            IRepository<StoredFile> files)
        {
            _audit = audit;
            _notifications = notifications;
            _payments = payments;
            _adjustments = adjustments;
            _documents = documents;
            _swiftFees = swiftFees;
            _disbursements = disbursements;
            _payrolls = payrolls;
            _charterTerms = charterTerms;
            _inboxLinks = inboxLinks;
            _vessels = vessels;
            _persons = persons;
            _comments = comments;
            _files = files;
        }

        // ==== Public ==========================================================================

        /// <summary>
        /// Where the job starts after the app starts: 10 minutes back.
        /// Anything already sent is skipped by its DedupKey.
        /// </summary>
        public async Task<long> GetStartCursorAsync(CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddMinutes(-10);
            return await _audit.Query()
                .Where(l => l.OccurredAt < cutoff)
                .MaxAsync(l => (long?)l.Id, ct) ?? 0;
        }

        /// <summary>Handles the logs after <paramref name="afterId"/> and returns the new cursor.</summary>
        public async Task<long> ProcessAsync(long afterId, CancellationToken ct = default)
        {
            var logs = await _audit.Query()
                .AsNoTracking()
                .Where(l => l.Id > afterId && l.UserId != null && l.Operation != AuditOperation.Purge)
                .OrderBy(l => l.Id)
                .Take(BatchSize)
                .ToListAsync(ct);

            if (logs.Count == 0) return afterId;
            var cursor = logs[^1].Id;

            if (!await _notifications.Watches().AnyAsync(ct)) return cursor;

            // Each log touches its own record (matched by Guid) and the records it belongs to
            var parentsByLog = new Dictionary<long, List<(EntityType Type, int Id)>>();
            foreach (var log in logs)
                parentsByLog[log.Id] = await ParentsAsync(log, ct);

            var guids = logs.Select(l => l.EntityId).Distinct().ToList();
            var parentIds = parentsByLog.Values.SelectMany(p => p).Select(p => p.Id).Distinct().ToList();

            var watches = await _notifications.Watches()
                .AsNoTracking()
                .Where(w => guids.Contains(w.EntityGuid) || parentIds.Contains(w.EntityId))
                .ToListAsync(ct);

            if (watches.Count == 0) return cursor;

            // One notification per follower, followed record and person who made the changes
            var groups = new Dictionary<(int WatchId, string User), Group>();

            foreach (var log in logs)
            {
                var parents = parentsByLog[log.Id];

                foreach (var watch in watches)
                {
                    var onRecord = watch.EntityGuid == log.EntityId;
                    var onParent = parents.Contains((watch.EntityType, watch.EntityId));
                    if (!onRecord && !onParent) continue;

                    if (watch.UserId == log.UserId) continue;          // your own change
                    if (log.OccurredAt < watch.CreatedAt) continue;    // before you followed

                    var phrase = onRecord && log.EntityType == watch.EntityType
                        ? OwnPhrase(log)
                        : ChildPhrase(log);

                    var user = string.IsNullOrWhiteSpace(log.UserName) ? "Someone" : log.UserName;

                    if (!groups.TryGetValue((watch.Id, user), out var group))
                        groups[(watch.Id, user)] = group = new Group(watch, user);

                    group.LastLogId = log.Id;
                    if (!group.Phrases.Contains(phrase)) group.Phrases.Add(phrase);
                }
            }

            if (groups.Count == 0) return cursor;

            var created = groups.Values
                .Select(g => Notification.Create(
                    g.Watch.UserId,
                    BuildText(g),
                    g.Watch.EntityType,
                    g.Watch.EntityId,
                    $"audit:{g.LastLogId}:{(int)g.Watch.EntityType}:{g.Watch.EntityId}"))
                .ToList();

            // Skip what was already sent (the first run after a restart looks back 10 minutes)
            var keys = created.Select(n => n.DedupKey).ToList();
            var userIds = created.Select(n => n.UserId).Distinct().ToList();
            var existing = await _notifications.Notifications()
                .Where(n => userIds.Contains(n.UserId) && keys.Contains(n.DedupKey))
                .Select(n => new { n.UserId, n.DedupKey })
                .ToListAsync(ct);
            var sent = existing.Select(e => (e.UserId, e.DedupKey)).ToHashSet();

            foreach (var n in created.Where(n => !sent.Contains((n.UserId, n.DedupKey))))
                _notifications.Add(n);

            await _notifications.SaveChangesAsync(ct);
            return cursor;
        }

        // ==== Relations =======================================================================

        // The followable records a log belongs to. Deleted rows included: a removed payment still notifies.
        private async Task<List<(EntityType Type, int Id)>> ParentsAsync(AuditEntry log, CancellationToken ct)
        {
            var g = log.EntityId;
            var parents = new List<(EntityType Type, int Id)>();

            switch (log.EntityType)
            {
                case EntityType.Payment:
                    var payment = await _payments.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g)
                        .Select(x => new { x.DocumentId, x.SwiftTransferId })
                        .FirstOrDefaultAsync(ct);
                    if (payment is null) break;
                    parents.Add((EntityType.Document, payment.DocumentId));
                    if (payment.SwiftTransferId is int swiftId) parents.Add((EntityType.SwiftTransfer, swiftId));
                    break;

                case EntityType.DocumentAdjustment:
                    await AddAsync(parents, EntityType.Document, _adjustments.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g).Select(x => (int?)x.DocumentId), ct);
                    break;

                case EntityType.Document:   // a sub-document → its parent document
                    await AddAsync(parents, EntityType.Document, _documents.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g).Select(x => x.ParentDocumentId), ct);
                    break;

                case EntityType.SwiftBankFee:
                    await AddAsync(parents, EntityType.SwiftTransfer, _swiftFees.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g).Select(x => (int?)x.SwiftTransferId), ct);
                    break;

                case EntityType.CrewPayrollDisbursement:
                    await AddAsync(parents, EntityType.CrewPayroll, _disbursements.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g).Select(x => (int?)x.PayrollId), ct);
                    break;

                case EntityType.CrewPayroll:
                    await AddAsync(parents, EntityType.CrewContract, _payrolls.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g).Select(x => (int?)x.ContractId), ct);
                    break;

                case EntityType.CharterTerms:
                    await AddAsync(parents, EntityType.AContract, _charterTerms.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g).Select(x => (int?)x.ContractId), ct);
                    break;

                case EntityType.InboxEntryLink:
                    await AddAsync(parents, EntityType.InboxEntry, _inboxLinks.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g).Select(x => (int?)x.InboxEntryId), ct);
                    break;

                case EntityType.VesselCertificate:
                    await AddAsync(parents, EntityType.Vessel, _vessels.Query().IgnoreQueryFilters()
                        .SelectMany(v => v.Certificates, (v, c) => new { v.Id, c.Guid })
                        .Where(x => x.Guid == g).Select(x => (int?)x.Id), ct);
                    break;

                case EntityType.PersonCertificate:
                    await AddAsync(parents, EntityType.Person, _persons.Query().IgnoreQueryFilters()
                        .SelectMany(p => p.Certificates, (p, c) => new { p.Id, c.Guid })
                        .Where(x => x.Guid == g).Select(x => (int?)x.Id), ct);
                    break;

                case EntityType.Comment:
                    var comment = await _comments.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g)
                        .Select(x => new { x.EntityType, x.EntityId })
                        .FirstOrDefaultAsync(ct);
                    if (comment?.EntityId is int commentOwnerId) parents.Add((comment.EntityType, commentOwnerId));
                    break;

                case EntityType.StoredFile:
                    var file = await _files.Query().IgnoreQueryFilters()
                        .Where(x => x.Guid == g)
                        .Select(x => new { x.EntityType, x.EntityId })
                        .FirstOrDefaultAsync(ct);
                    if (file?.EntityId is int fileOwnerId) parents.Add((file.EntityType, fileOwnerId));
                    break;
            }

            return parents;
        }

        private static async Task AddAsync(List<(EntityType Type, int Id)> parents, EntityType type,
            IQueryable<int?> ids, CancellationToken ct)
        {
            if (await ids.FirstOrDefaultAsync(ct) is int id) parents.Add((type, id));
        }

        // ==== Text ============================================================================

        // "INV-55: Ahmad added a payment, edited Due date (+1 more)"
        private static string BuildText(Group g)
        {
            var more = g.Phrases.Count - MaxPhrases;
            return $"{g.Watch.Title}: {g.User} {string.Join(", ", g.Phrases.Take(MaxPhrases))}"
                   + (more > 0 ? $" (+{more} more)" : "");
        }

        // A change to the followed record itself
        private static string OwnPhrase(AuditEntry log) => log.Operation switch
        {
            AuditOperation.Create => "created it",
            AuditOperation.Delete => "deleted it",
            _ => ChangedFields(log) is { Count: > 0 } fields
                ? "edited " + string.Join(", ", fields.Take(MaxFields)) + (fields.Count > MaxFields ? "…" : "")
                : "edited it"
        };

        // A change to something that belongs to it
        private static string ChildPhrase(AuditEntry log)
        {
            if (log.EntityType == EntityType.Comment && log.Operation == AuditOperation.Create) return "commented";

            var verb = log.Operation switch
            {
                AuditOperation.Create => "added",
                AuditOperation.Delete => "removed",
                _ => "edited"
            };
            return $"{verb} {ChildName(log.EntityType)}";
        }

        private static string ChildName(EntityType type) => type switch
        {
            EntityType.Payment => "a payment",
            EntityType.DocumentAdjustment => "an adjustment",
            EntityType.Document => "a sub-document",
            EntityType.DocumentItem => "an item",
            EntityType.SwiftBankFee => "a bank fee",
            EntityType.CrewPayrollDisbursement => "a disbursement",
            EntityType.CrewPayroll => "a payroll",
            EntityType.CharterTerms => "the charter terms",
            EntityType.InboxEntryLink => "a linked record",
            EntityType.VesselCertificate or EntityType.PersonCertificate => "a certificate",
            EntityType.Comment => "a comment",
            EntityType.StoredFile => "a file",
            EntityType.VoyageStop => "a stop",
            EntityType.VoyageCargo => "cargo",
            EntityType.VoyageContract => "a contract link",
            _ => "a " + Humanize(type.ToString()).ToLowerInvariant()
        };

        // The changed fields, in plain words, from the log's JSON
        private static List<string> ChangedFields(AuditEntry log)
        {
            var json = log.DataAfter ?? log.DataBefore;
            if (string.IsNullOrWhiteSpace(json)) return [];

            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Object) return [];

                var typeName = log.EntityType.ToString();
                return doc.RootElement.EnumerateObject()
                    .Select(p => FieldName(p.Name, typeName))
                    .OfType<string>()
                    .Distinct()
                    .ToList();
            }
            catch (JsonException)
            {
                return [];
            }
        }

        // "DueDate" → "Due date", "SupplierId" → "Supplier", "IsActive" → "Active"; technical → null
        private static string? FieldName(string name, string typeName)
        {
            if (HiddenFields.Contains(name)) return null;

            if (name.Length > 2 && name.StartsWith("Is", StringComparison.Ordinal) && char.IsUpper(name[2]))
                name = name[2..];

            if (name.EndsWith("Id", StringComparison.Ordinal) || name.EndsWith("ID", StringComparison.Ordinal))
                name = name[..^2];

            // Empty, or the parent's own key on an owned value (CompanyId on a company's bank account)
            if (name.Length == 0 || name.Equals(typeName, StringComparison.OrdinalIgnoreCase)) return null;

            return Humanize(name);
        }

        private static string Humanize(string name)
        {
            var words = Regex.Replace(name, "(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
            return char.ToUpperInvariant(words[0]) + words[1..].ToLowerInvariant();
        }

        private sealed class Group(Watch watch, string user)
        {
            public Watch Watch { get; } = watch;
            public string User { get; } = user;
            public long LastLogId { get; set; }
            public List<string> Phrases { get; } = [];
        }
    }
}