using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;
using System.Text;

namespace Marilog.Infrastructure.Helpers.EmailNotification
{
    public static class NotificationEmailBuilder
    {
        internal static string Build(NotificationDataResponse data)
        {
            ArgumentNullException.ThrowIfNull(data);

            var builder = new StringBuilder();

            builder.Append("""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Daily Notifications</title>
                <style>
                        @media only screen and (max-width: 600px) {
                    .email-outer {
                        padding: 16px 8px !important;
                    }
                    .email-card {
                        padding: 16px !important;
                    }
                    .email-title {
                        font-size: 20px !important;
                    }
                    .company-header {
                        flex-direction: column !important;
                        align-items: flex-start !important;
                    }
                    .responsive-table thead {
                        display: none;
                    }
                    .responsive-table,
                    .responsive-table tbody,
                    .responsive-table tr,
                    .responsive-table td {
                        display: block;
                        width: 100%;
                    }
                    .responsive-table tr {
                        border: 1px solid #eee;
                        border-radius: 8px;
                        margin-bottom: 10px;
                        padding: 10px 14px;
                    }
                    .responsive-table td {
                        text-align: left !important;
                        padding: 6px 0 !important;
                        border-bottom: none !important;
                    }
                    .responsive-table td:before {
                        content: attr(data-label);
                        display: block;
                        font-size: 11px;
                        text-transform: uppercase;
                        letter-spacing: .03em;
                        color: #999;
                        font-weight: 600;
                        margin-bottom: 2px;
                    }
                    .responsive-table td[data-label="Amount"] {
                        font-weight: bold;
                        color: #1a56b0;
                        font-size: 15px;
                    }
                }
                </style>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f6f8;font-family:Arial,Helvetica,sans-serif;color:#333;">
                <div class="email-outer" style="max-width:900px;margin:0 auto;padding:30px 20px;">
                    <div class="email-card" style="background:#ffffff;border-radius:8px;padding:30px;box-shadow:0 1px 4px rgba(0,0,0,0.08);">

                        <h1 class="email-title" style="margin:0 0 8px;font-size:24px;">
                            Daily Notifications
                        </h1>

                        <p style="margin:0 0 30px;color:#666;font-size:14px;">
                            The following items require your attention.
                        </p>
            """);

            if (data.UnpaidDocuments.Count > 0)
            {
                AppendUnpaidDocuments(builder, data.UnpaidDocuments);
            }

            if (data.ExpiringVesselCertificates.Count > 0)
            {
                AppendVesselCertificates(
                    builder,
                    "Expiring Vessel Certificates",
                    data.ExpiringVesselCertificates);
            }

            if (data.ExpiredVesselCertificates.Count > 0)
            {
                AppendVesselCertificates(
                    builder,
                    "Expired Vessel Certificates",
                    data.ExpiredVesselCertificates);
            }

            if (data.ExpiringPersonCertificates.Count > 0)
            {
                AppendPersonCertificates(
                    builder,
                    "Expiring Person Certificates",
                    data.ExpiringPersonCertificates);
            }

            if (data.ExpiredPersonCertificates.Count > 0)
            {
                AppendPersonCertificates(
                    builder,
                    "Expired Person Certificates",
                    data.ExpiredPersonCertificates);
            }

            builder.Append("""
                    </div>
                </div>
            </body>
            </html>
            """);

            return builder.ToString();
        }

        private static void AppendUnpaidDocuments(StringBuilder builder, IReadOnlyList<DocumentResponse> documents)
        {
            var filteredDocs = documents
                .Where(d => d.Side != FinancialSide.None)
                .ToList();

            if (filteredDocs.Count == 0)
            {
                return;
            }

            builder.Append("""
            <h2 style="font-size:18px;margin:0 0 16px;color:#1a1a1a;">
                Unpaid Documents
            </h2>
            """);

            // Group by company (supplier), oldest -> newest within each group
            var companyGroups = filteredDocs
                        .GroupBy(d => string.IsNullOrWhiteSpace(d.SupplierName) ? "Unknown Supplier" : d.SupplierName)
                        .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

            foreach (var companyGroup in companyGroups)
            {
                var orderedDocs = companyGroup
                    .OrderBy(d => d.DocDate)
                    .ToList();

                AppendCompanyGroup(builder, companyGroup.Key, orderedDocs);
            }
        }

        private static void AppendCompanyGroup(
            StringBuilder builder,
            string companyName,
            IReadOnlyList<DocumentResponse> documents)
        {
            // Sum per currency separately - documents for the same supplier can be in different currencies
            var totalsByCurrency = documents
                .GroupBy(d => d.CurrencyCode)
                .Select(g => new { Currency = g.Key, Total = g.Sum(d => d.RemainingBalance) })
                .OrderBy(t => t.Currency, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var totalsBadgesBuilder = new StringBuilder();
            foreach (var t in totalsByCurrency)
            {
                totalsBadgesBuilder.Append($"""
                <span style="display:inline-block;background:#eaf1fb;color:#1a56b0;font-weight:bold;font-size:13px;padding:5px 12px;border-radius:20px;margin-left:6px;white-space:nowrap;">
                    {t.Total:N2} {HtmlEncode(t.Currency)}
                </span>
                """);
            }

            builder.Append($"""
            <div style="border:1px solid #e2e5e9;border-left:4px solid #2c7be5;border-radius:8px;margin-bottom:18px;overflow:hidden;">

                <div class="company-header" style="background:#f8fafc;padding:12px 16px;display:flex;justify-content:space-between;align-items:center;flex-wrap:wrap;gap:8px;border-bottom:1px solid #e2e5e9;">
                    <span style="font-size:15px;font-weight:bold;color:#1a1a1a;">
                        {HtmlEncode(companyName)}
                    </span>
                    <span>
                        {totalsBadgesBuilder}
                    </span>
                </div>

                <table class="responsive-table" style="width:100%;border-collapse:collapse;font-size:14px;">
                    <thead>
                        <tr>
                            <th style="text-align:left;padding:10px 16px;border-bottom:1px solid #eee;color:#666;font-weight:600;">Document</th>
                            <th style="text-align:left;padding:10px 16px;border-bottom:1px solid #eee;color:#666;font-weight:600;">Vessel</th>
                            <th style="text-align:left;padding:10px 16px;border-bottom:1px solid #eee;color:#666;font-weight:600;">Description</th>
                            <th style="text-align:right;padding:10px 16px;border-bottom:1px solid #eee;color:#666;font-weight:600;">Amount</th>
                            <th style="text-align:left;padding:10px 16px;border-bottom:1px solid #eee;color:#666;font-weight:600;">Doc Date</th>
                        </tr>
                    </thead>
                    <tbody>
            """);

            foreach (var document in documents)
            {
                builder.Append($"""
                        <tr>
                            <td data-label="Document" style="padding:10px 16px;border-bottom:1px solid #f2f2f2;">
                                {HtmlEncode(document.DocNumber)}
                            </td>
                            <td data-label="Vessel" style="padding:10px 16px;border-bottom:1px solid #f2f2f2;">
                                {HtmlEncode(document.VesselName)}
                            </td>
                            <td data-label="Description" style="padding:10px 16px;border-bottom:1px solid #f2f2f2;">
                                {HtmlEncode(document.DocTypeName + " " + document.Reference)}
                            </td>
                            <td data-label="Amount" style="padding:10px 16px;border-bottom:1px solid #f2f2f2;text-align:right;">
                                {document.RemainingBalance:N2} {HtmlEncode(document.CurrencyCode)}
                            </td>
                            <td data-label="Doc Date" style="padding:10px 16px;border-bottom:1px solid #f2f2f2;">
                                {FormatDate(document.DocDate.ToDateTime(TimeOnly.MinValue))}
                            </td>
                        </tr>
                """);
            }

            builder.Append("""
                    </tbody>
                </table>
            </div>
            """);
        }

        private static void AppendVesselCertificates(
            StringBuilder builder,
            string title,
            IReadOnlyList<CertificateResponse> certificates)
        {
            builder.Append($"""
            <h2 style="font-size:18px;margin:0 0 12px;">
                {HtmlEncode(title)}
            </h2>

            <table class="responsive-table" style="width:100%;border-collapse:collapse;margin-bottom:30px;font-size:14px;">
                <thead>
                    <tr>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Vessel</th>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Certificate</th>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Expiry Date</th>
                    </tr>
                </thead>
                <tbody>
            """);

            foreach (var certificate in certificates)
            {
                builder.Append($"""
                <tr>
                    <td data-label="Vessel" style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.VName)}
                    </td>
                    <td data-label="Certificate" style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.CertificateName)}
                    </td>
                    <td data-label="Expiry Date" style="padding:10px;border-bottom:1px solid #eee;">
                        {certificate.ExpiryDate:dd MMM yyyy}
                    </td>
                </tr>
                """);
            }

            builder.Append("""
                </tbody>
            </table>
            """);
        }

        private static void AppendPersonCertificates(StringBuilder builder, string title, IReadOnlyList<CertificateResponse> certificates)
        {
            builder.Append($"""
            <h2 style="font-size:18px;margin:0 0 12px;">
                {HtmlEncode(title)}
            </h2>

            <table class="responsive-table" style="width:100%;border-collapse:collapse;margin-bottom:30px;font-size:14px;">
                <thead>
                    <tr>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Person</th>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Certificate</th>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Expiry Date</th>
                    </tr>
                </thead>
                <tbody>
            """);

            foreach (var certificate in certificates)
            {
                builder.Append($"""
                <tr>
                    <td data-label="Person" style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.PName)}
                    </td>
                    <td data-label="Certificate" style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.CertificateName)}
                    </td>
                    <td data-label="Expiry Date" style="padding:10px;border-bottom:1px solid #eee;">
                        {certificate.ExpiryDate:dd MMM yyyy}
                    </td>
                </tr>
                """);
            }

            builder.Append("""
                </tbody>
            </table>
            """);
        }

        private static string HtmlEncode(string? value)
        {
            return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string FormatDate(DateTime? date)
        {
            return date?.ToString("dd MMM yyyy") ?? "-";
        }
    }
}