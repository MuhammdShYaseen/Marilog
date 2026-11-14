using Marilog.Domain.Entities.NotifiEntities;
using Marilog.Domain.Entities.SystemEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.NotifiServices
{
    /// <summary>
    /// Once a day: the due date of followed documents, and the expiry of the certificates
    /// of followed vessels and persons. Each stage is sent once; a changed date starts again.
    /// </summary>
    public class DueNotificationService
    {
        private const int DocumentDueSoonDays = 7;
        private const int CertificateCloseDays = 30;
        private const int CertificateSoonDays = 90;

        private readonly INotificationRepository _notifications;
        private readonly IRepository<Document> _documents;
        private readonly IRepository<Vessel> _vessels;
        private readonly IRepository<Person> _persons;

        public DueNotificationService(
            INotificationRepository notifications,
            IRepository<Document> documents,
            IRepository<Vessel> vessels,
            IRepository<Person> persons)
        {
            _notifications = notifications;
            _documents = documents;
            _vessels = vessels;
            _persons = persons;
        }

        /// <summary>Creates the notifications due today and returns how many were added.</summary>
        public async Task<int> CreateAsync(DateOnly today, CancellationToken ct = default)
        {
            var watches = await _notifications.Watches()
                .AsNoTracking()
                .Where(w => w.EntityType == EntityType.Document
                         || w.EntityType == EntityType.Vessel
                         || w.EntityType == EntityType.Person)
                .ToListAsync(ct);

            if (watches.Count == 0) return 0;

            var created = new List<Notification>();
            await AddDocumentsAsync(watches, today, created, ct);
            await AddCertificatesAsync(watches, today, created, ct);

            if (created.Count == 0) return 0;

            // Skip stages already sent
            var keys = created.Select(n => n.DedupKey).Distinct().ToList();
            var userIds = created.Select(n => n.UserId).Distinct().ToList();
            var existing = await _notifications.Notifications()
                .Where(n => userIds.Contains(n.UserId) && keys.Contains(n.DedupKey))
                .Select(n => new { n.UserId, n.DedupKey })
                .ToListAsync(ct);
            var sent = existing.Select(e => (e.UserId, e.DedupKey)).ToHashSet();

            var added = 0;
            foreach (var n in created)
            {
                if (!sent.Add((n.UserId, n.DedupKey))) continue;
                _notifications.Add(n);
                added++;
            }

            if (added > 0) await _notifications.SaveChangesAsync(ct);
            return added;
        }

        // ==== Documents: 7 days before, on the day, overdue ===================================

        private async Task AddDocumentsAsync(List<Watch> watches, DateOnly today, List<Notification> created, CancellationToken ct)
        {
            var docWatches = watches.Where(w => w.EntityType == EntityType.Document).ToList();
            if (docWatches.Count == 0) return;

            var ids = docWatches.Select(w => w.EntityId).Distinct().ToList();
            var lastDay = today.AddDays(DocumentDueSoonDays);

            var documents = await _documents.Query()
                .AsNoTracking()
                .Where(d => ids.Contains(d.Id) && d.IsActive && d.DocDue != null && d.DocDue <= lastDay)
                .Select(d => new
                {
                    d.Id,
                    Due = d.DocDue!.Value,
                    d.Currency.CurrencyCode,
                    Balance = d.TotalAmount
                              + (d.Adjustments.Sum(a => (decimal?)a.Amount) ?? 0m)
                              - d.Payments.Sum(p => p.PaidAmount)
                })
                .ToListAsync(ct);

            foreach (var d in documents.Where(d => d.Balance > 0))
            {
                var days = d.Due.DayNumber - today.DayNumber;
                var (stage, phrase) = days switch
                {
                    < 0 => ("overdue", $"overdue since {d.Due:dd MMM yyyy}"),
                    0 => ("today", "due today"),
                    _ => ("soon", $"due in {Days(days)} ({d.Due:dd MMM yyyy})")
                };

                foreach (var w in docWatches.Where(w => w.EntityId == d.Id))
                    created.Add(Notification.Create(
                        w.UserId,
                        $"{w.Title}: {phrase}, balance {d.Balance:N2} {d.CurrencyCode}",
                        EntityType.Document,
                        d.Id,
                        $"due:doc:{d.Id}:{stage}:{d.Due:yyyyMMdd}"));
            }
        }

        // ==== Certificates: 90 days, 30 days, expired ========================================

        private async Task AddCertificatesAsync(List<Watch> watches, DateOnly today, List<Notification> created, CancellationToken ct)
        {
            var lastDay = today.AddDays(CertificateSoonDays);

            var vesselIds = Ids(watches, EntityType.Vessel);
            if (vesselIds.Count > 0)
            {
                var rows = await _vessels.Query()
                    .AsNoTracking()
                    .Where(v => vesselIds.Contains(v.Id))
                    .SelectMany(v => v.Certificates
                        .Where(c => c.Certificate.ExpiryDate != null && c.Certificate.ExpiryDate <= lastDay)
                        .Select(c => new CertificateRow
                        {
                            OwnerId = v.Id,
                            CertificateId = c.Id,
                            Name = c.Certificate.CertificateName,
                            Expiry = c.Certificate.ExpiryDate!.Value
                        }))
                    .ToListAsync(ct);

                AddCertificates(watches, EntityType.Vessel, "vcert", rows, today, created);
            }

            var personIds = Ids(watches, EntityType.Person);
            if (personIds.Count > 0)
            {
                var rows = await _persons.Query()
                    .AsNoTracking()
                    .Where(p => personIds.Contains(p.Id))
                    .SelectMany(p => p.Certificates
                        .Where(c => c.Certificate.ExpiryDate != null && c.Certificate.ExpiryDate <= lastDay)
                        .Select(c => new CertificateRow
                        {
                            OwnerId = p.Id,
                            CertificateId = c.Id,
                            Name = c.Certificate.CertificateName,
                            Expiry = c.Certificate.ExpiryDate!.Value
                        }))
                    .ToListAsync(ct);

                AddCertificates(watches, EntityType.Person, "pcert", rows, today, created);
            }
        }

        private static void AddCertificates(List<Watch> watches, EntityType ownerType, string keyPrefix,
            List<CertificateRow> rows, DateOnly today, List<Notification> created)
        {
            foreach (var c in rows)
            {
                var days = c.Expiry.DayNumber - today.DayNumber;
                var (stage, phrase) = days switch
                {
                    < 0 => ("expired", $"expired on {c.Expiry:dd MMM yyyy}"),
                    0 => ("30", "expires today"),
                    <= CertificateCloseDays => ("30", $"expires in {Days(days)} ({c.Expiry:dd MMM yyyy})"),
                    _ => ("90", $"expires in {Days(days)} ({c.Expiry:dd MMM yyyy})")
                };

                foreach (var w in watches.Where(w => w.EntityType == ownerType && w.EntityId == c.OwnerId))
                    created.Add(Notification.Create(
                        w.UserId,
                        $"{w.Title} — {c.Name}: {phrase}",
                        ownerType,
                        c.OwnerId,
                        $"due:{keyPrefix}:{c.CertificateId}:{stage}:{c.Expiry:yyyyMMdd}"));
            }
        }

        private static List<int> Ids(List<Watch> watches, EntityType type)
            => watches.Where(w => w.EntityType == type).Select(w => w.EntityId).Distinct().ToList();

        private static string Days(int days) => days == 1 ? "1 day" : $"{days} days";

        private sealed class CertificateRow
        {
            public int OwnerId { get; init; }
            public int CertificateId { get; init; }
            public string Name { get; init; } = "";
            public DateOnly Expiry { get; init; }
        }
    }
}