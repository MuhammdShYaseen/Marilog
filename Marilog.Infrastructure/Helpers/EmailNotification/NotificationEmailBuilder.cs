using Marilog.Domain.Entities.NotifiEntities;
using System.Net;
using System.Text;

namespace Marilog.Infrastructure.Helpers.EmailNotification
{
    // The daily email: a user's new notifications, grouped by record, each group with an Open link.
    public static class NotificationEmailBuilder
    {
        internal static string Build(string userName, IReadOnlyList<Notification> items, string? appUrl)
        {
            var sb = new StringBuilder();

            sb.Append("""
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1.0">
                    <title>Marilog notifications</title>
                </head>
                <body style="margin:0;background:#f4f6f8;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;">
                <div style="max-width:680px;margin:0 auto;padding:24px 12px;">
                <div style="background:#ffffff;border-radius:10px;padding:24px;">
                """);

            var name = string.IsNullOrWhiteSpace(userName) ? "" : $" {E(userName)}";
            sb.Append($"""<h2 style="margin:0 0 6px;color:#0b2a4a;">Hello{name},</h2>""");
            sb.Append($"""<p style="margin:0 0 20px;color:#4b5563;">You have {items.Count} new notification{(items.Count == 1 ? "" : "s")} in Marilog.</p>""");

            foreach (var record in items.GroupBy(n => (n.EntityType, n.EntityId)))
            {
                sb.Append("""<div style="border:1px solid #e5e7eb;border-radius:8px;padding:12px 16px;margin-bottom:12px;">""");
                sb.Append("""<ul style="margin:0;padding-left:18px;color:#374151;">""");

                foreach (var n in record.OrderBy(n => n.Id))
                    sb.Append($"""<li style="margin:4px 0;">{E(n.Text)} <span style="color:#9ca3af;font-size:12px;">· {n.CreatedAt:dd MMM HH:mm} UTC</span></li>""");

                sb.Append("</ul>");

                if (!string.IsNullOrWhiteSpace(appUrl))
                {
                    var link = $"{appUrl.TrimEnd('/')}/open/{record.Key.EntityType}/{record.Key.EntityId}";
                    sb.Append($"""<div style="text-align:right;margin-top:8px;"><a href="{E(link)}" style="color:#0d9488;font-weight:600;text-decoration:none;">Open</a></div>""");
                }

                sb.Append("</div>");
            }

            sb.Append("""
                </div>
                <p style="text-align:center;color:#9ca3af;font-size:12px;margin-top:16px;">You get this email because you follow these records in Marilog.</p>
                </div>
                </body>
                </html>
                """);

            return sb.ToString();
        }

        private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");
    }
}