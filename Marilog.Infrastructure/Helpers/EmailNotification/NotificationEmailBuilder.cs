

using Marilog.Contracts.DTOs.EmailNotificationDTOs;
using Marilog.Contracts.DTOs.Responses;
using Microsoft.VisualBasic;
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
            </head>
            <body style="margin:0;padding:0;background-color:#f4f6f8;font-family:Arial,Helvetica,sans-serif;color:#333;">
                <div style="max-width:900px;margin:0 auto;padding:30px 20px;">
                    <div style="background:#ffffff;border-radius:8px;padding:30px;box-shadow:0 1px 4px rgba(0,0,0,0.08);">

                        <h1 style="margin:0 0 8px;font-size:24px;">
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

        private static void AppendUnpaidDocuments(
            StringBuilder builder,
            IReadOnlyList<DocumentResponse> documents)
        {
            builder.Append("""
            <h2 style="font-size:18px;margin:0 0 12px;">
                Unpaid Documents
            </h2>

            <table style="width:100%;border-collapse:collapse;margin-bottom:30px;font-size:14px;">
                <thead>
                    <tr>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Document</th>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Description</th>
                        <th style="text-align:right;padding:10px;border-bottom:2px solid #ddd;">Amount</th>
                        <th style="text-align:left;padding:10px;border-bottom:2px solid #ddd;">Due Date</th>
                    </tr>
                </thead>
                <tbody>
            """);

            foreach (var document in documents)
            {
                builder.Append($"""
                <tr>
                    <td style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(document.DocNumber)}
                    </td>
                    <td style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(document.Reference)}
                    </td>
                    <td style="padding:10px;border-bottom:1px solid #eee;text-align:right;">
                        {document.TotalAmount:N2} {HtmlEncode(document.CurrencyCode)}
                    </td>
                    <td style="padding:10px;border-bottom:1px solid #eee;">
                        {FormatDate(document.DocDate.ToDateTime(TimeOnly.MinValue).AddDays(30))}
                    </td>
                </tr>
                """);
            }

            builder.Append("""
                </tbody>
            </table>
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

            <table style="width:100%;border-collapse:collapse;margin-bottom:30px;font-size:14px;">
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
                    <td style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.VName)}
                    </td>
                    <td style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.CertificateName)}
                    </td>
                    <td style="padding:10px;border-bottom:1px solid #eee;">
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

            <table style="width:100%;border-collapse:collapse;margin-bottom:30px;font-size:14px;">
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
                    <td style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.PName)}
                    </td>
                    <td style="padding:10px;border-bottom:1px solid #eee;">
                        {HtmlEncode(certificate.CertificateName)}
                    </td>
                    <td style="padding:10px;border-bottom:1px solid #eee;">
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
