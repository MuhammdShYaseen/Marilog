using Marilog.Domain.Entities.SystemEntities;
using Marilog.Kernel.Enums;

namespace Marilog.Application.Extensions
{
    internal static class DocumentQueryExtensions
    {
        // Same rule as DocumentResponse.PaymentStatus:
        // Paid = net <= paid, PartiallyPaid = 0 < paid < net, Unpaid = paid <= 0 and paid < net.
        // null = all documents.
        internal static IQueryable<Document> WherePaymentStatus(
            this IQueryable<Document> query, DocumentPaymentStatus? status)
        {
            return status switch
            {
                DocumentPaymentStatus.Paid => query.Where(x =>
                    x.TotalAmount + (x.Adjustments.Sum(a => (decimal?)a.Amount) ?? 0m)
                    <= (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m)),

                DocumentPaymentStatus.PartiallyPaid => query.Where(x =>
                    (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m) > 0m &&
                    (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m)
                    < x.TotalAmount + (x.Adjustments.Sum(a => (decimal?)a.Amount) ?? 0m)),

                DocumentPaymentStatus.Unpaid => query.Where(x =>
                    (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m) <= 0m &&
                    (x.Payments.Sum(p => (decimal?)p.PaidAmount) ?? 0m)
                    < x.TotalAmount + (x.Adjustments.Sum(a => (decimal?)a.Amount) ?? 0m)),

                _ => query
            };
        }

        // Multi-select filters: an empty list means "no filter".
        internal static IQueryable<Document> WhereSupplierIn(this IQueryable<Document> query, List<int> ids) =>
            ids.Count == 0 ? query : query.Where(x => x.SupplierId != null && ids.Contains(x.SupplierId.Value));

        internal static IQueryable<Document> WhereBuyerIn(this IQueryable<Document> query, List<int> ids) =>
            ids.Count == 0 ? query : query.Where(x => x.BuyerId != null && ids.Contains(x.BuyerId.Value));

        internal static IQueryable<Document> WhereDocTypeIn(this IQueryable<Document> query, List<int> ids) =>
            ids.Count == 0 ? query : query.Where(x => ids.Contains(x.DocTypeId));
    }
}