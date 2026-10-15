

using HtmlAgilityPack;
using Marilog.Application.Interfaces.VesselPosition;
using Marilog.Contracts.DTOs.Responses;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Marilog.Infrastructure.Services.VesselPositions
{
    internal sealed partial class VesselFinderPositionProvider(HttpClient http, ILogger<VesselFinderPositionProvider> logger) : IVesselPositionProvider
    {
        [GeneratedRegex(@"\s+")]
        private static partial Regex WhitespaceRegex();

        [GeneratedRegex(@"\d[\d,]*(\.\d+)?")]
        private static partial Regex NumberRegex();

        [GeneratedRegex(@"is\s+at\s+(.+?)\s+reported", RegexOptions.Singleline)]
        private static partial Regex AreaRegex();

        [GeneratedRegex(@"ATD:\s*(.+?)\s+UTC")]
        private static partial Regex AtdRegex();

        public async Task<VesselPositionResponse?> GetByImoAsync(int imo, CancellationToken ct = default)
        {
            using var res = await http.GetAsync($"vessels/details/{imo}", ct);
            if (res.StatusCode == HttpStatusCode.NotFound)
                return null;

            res.EnsureSuccessStatusCode();

            var doc = new HtmlDocument();
            doc.LoadHtml(await res.Content.ReadAsStringAsync(ct));

            var djson = ReadDjson(doc);
            if (djson?.Lat is null || djson.Lon is null)
            {
                logger.LogWarning("VesselFinder: no position data for IMO {Imo} (page structure may have changed)", imo);
                return null;
            }

            var fields = ReadLabeledFields(doc);
            var destinationBlock = FindInfoBlock(doc, "Destination");
            var lastPortBlock = FindInfoBlock(doc, "Last Port");

            return new VesselPositionResponse(
                imo,
                djson.Mmsi,
                Text(doc.DocumentNode, "//h1[@class='title']"),
                fields.GetValueOrDefault("Callsign"),
                fields.GetValueOrDefault("Ship Type"),
                fields.GetValueOrDefault("AIS Type"),
                fields.GetValueOrDefault("Flag"),
                ParseInt(fields.GetValueOrDefault("Year of Build")),
                new VesselDimensions(
                    ParseDouble(fields.GetValueOrDefault("Length Overall (m)")),
                    ParseDouble(fields.GetValueOrDefault("Beam (m)")),
                    ParseInt(fields.GetValueOrDefault("Gross Tonnage")),
                    ParseInt(fields.GetValueOrDefault("Deadweight (t)"))),
                new VesselPositionInfo(
                    djson.Lat.Value,
                    djson.Lon.Value,
                    djson.Cog ?? 0,
                    djson.Sog ?? 0,
                    ReadArea(doc),
                    fields.GetValueOrDefault("Navigation Status"),
                    ParseDouble(fields.GetValueOrDefault("Current draught")),
                    ReadLastReport(doc)),
                new VesselVoyageInfo(
                    ReadPort(destinationBlock),
                    ReadEta(destinationBlock),
                    ReadPort(lastPortBlock),
                    ReadDeparture(lastPortBlock)),
                ReadHistory(doc),
                await GetUtilizationAsync(imo, ct),
                NullIfEmpty(doc.DocumentNode
                    .SelectSingleNode("//img[contains(@class,'main-photo')]")
                    ?.GetAttributeValue("src", "")));
        }

        // ---------- page sections ----------

        private static DjsonPayload? ReadDjson(HtmlDocument doc)
        {
            var raw = doc.GetElementbyId("djson")?.GetAttributeValue("data-json", "");
            if (string.IsNullOrEmpty(raw))
                return null;

            try
            {
                return JsonSerializer.Deserialize<DjsonPayload>(WebUtility.HtmlDecode(raw));
            }
            catch (JsonException)
            {
                return null; // caller logs the warning
            }
        }

        // Label/value cells from the top info table (n3/v3) and the particulars tables (tpc1/tpc2).
        // Premium-locked cells contain only an icon, so they come out empty and are skipped.
        private static Dictionary<string, string> ReadLabeledFields(HtmlDocument doc)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var labels = doc.DocumentNode.SelectNodes("//td[contains(@class,'n3') or contains(@class,'tpc1')]");
            if (labels is null)
                return fields;

            foreach (var label in labels)
            {
                var key = Clean(label.InnerText);
                var value = Clean(label.SelectSingleNode("following-sibling::td[1]")?.InnerText);
                if (key is not null && value is not null)
                    fields.TryAdd(key, value);
            }

            return fields;
        }

        private static string? ReadArea(HtmlDocument doc)
        {
            var text = Text(doc.DocumentNode, "//p[contains(@class,'text2')]");
            var m = text is null ? null : AreaRegex().Match(text);
            return m is { Success: true } ? m.Groups[1].Value.Trim() : null;
        }

        private static DateTime? ReadLastReport(HtmlDocument doc)
        {
            var title = doc.GetElementbyId("lastrep")
                ?.SelectSingleNode(".//*[@data-title]")
                ?.GetAttributeValue("data-title", "");

            return DateTime.TryParseExact(
                title, "MMM d, yyyy HH:mm 'UTC'", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt)
                ? dt : null;
        }

        private static HtmlNode? FindInfoBlock(HtmlDocument doc, string label)
            => doc.DocumentNode.SelectSingleNode($"//div[@class='vilabel' and normalize-space()='{label}']/..");

        private static PortReference? ReadPort(HtmlNode? block)
        {
            var link = block?.SelectSingleNode("a[contains(@class,'_npNa')]");
            var name = Clean(link?.InnerText);
            if (link is null || name is null)
                return null;

            var country = NullIfEmpty(block!
                .SelectSingleNode("preceding-sibling::div[contains(@class,'flag-icon')]")
                ?.GetAttributeValue("title", ""));

            if (country is not null && name.EndsWith($", {country}", StringComparison.OrdinalIgnoreCase))
                name = name[..^(country.Length + 2)];

            var href = link.GetAttributeValue("href", "");
            var code = href.StartsWith("/ports/") ? href["/ports/".Length..] : null;

            return new PortReference(name, country, code);
        }

        private static string? ReadEta(HtmlNode? block)
        {
            var text = Text(block, ".//span[contains(@class,'_mcol12')]");
            return text?.StartsWith("ETA:") == true ? NullIfEmpty(text["ETA:".Length..].Trim()) : null;
        }

        // Source gives "ATD: Oct 1, 23:59 UTC" without a year: assume the current year,
        // and step back one year if that would put the departure in the future.
        private static DateTime? ReadDeparture(HtmlNode? block)
        {
            var text = Text(block, ".//div[contains(@class,'_value')]");
            var m = text is null ? null : AtdRegex().Match(text);
            if (m is not { Success: true })
                return null;

            if (!DateTime.TryParseExact(
                    m.Groups[1].Value, "MMM d, HH:mm", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
                return null;

            return dt > DateTime.UtcNow ? dt.AddYears(-1) : dt;
        }

        // Each history row records one change (name, flag, or a premium-locked owner/manager).
        // Rows with no visible name or flag are skipped.
        private static IReadOnlyList<VesselHistoryEntry> ReadHistory(HtmlDocument doc)
        {
            var rows = doc.DocumentNode.SelectNodes("//div[contains(@class,'vhrow')]");
            if (rows is null)
                return [];

            var history = new List<VesselHistoryEntry>();
            foreach (var row in rows)
            {
                var period = Text(row, ".//div[contains(@class,'vhc1')]//div[@class='vhv']");
                var name = Text(row, ".//div[contains(@class,'vhc3')]//div[@class='vhv']");
                var flag = NullIfEmpty(row
                    .SelectSingleNode(".//div[contains(@class,'vhc2')]//img")
                    ?.GetAttributeValue("title", ""));

                if (name is null && flag is null)
                    continue;

                if (DateOnly.TryParseExact(period, "yyyy MMM", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var date))
                    history.Add(new VesselHistoryEntry(date, name, flag));
            }

            return history;
        }

        // ---------- utilization endpoint ----------

        private async Task<VesselUtilization?> GetUtilizationAsync(int imo, CancellationToken ct)
        {
            // The page itself requested the previous year (2025) when checked in Oct 2026.
            var year = DateTime.UtcNow.Year - 1;

            using var req = new HttpRequestMessage(HttpMethod.Get, $"api/pub/ship/vu/{imo}/{year}");
            req.Headers.Referrer = new Uri(http.BaseAddress!, $"vessels/details/{imo}");

            try
            {
                using var res = await http.SendAsync(req, ct);
                if (!res.IsSuccessStatusCode)
                    return null;

                var vu = await res.Content.ReadFromJsonAsync<VuPayload>(ct);
                if (vu is null)
                    return null;

                var ports = vu.Ports?
                    .Select(p => new PortReference(p.Name, p.Country, p.Locode))
                    .ToList() ?? [];

                return new VesselUtilization(year, vu.SpeedAverage, vu.SpeedMax, vu.PortCalls, ports);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException
                                       || (ex is TaskCanceledException && !ct.IsCancellationRequested))
            {
                logger.LogWarning(ex, "VesselFinder: utilization unavailable for IMO {Imo}", imo);
                return null;
            }
        }

        // ---------- helpers ----------

        private static string? Text(HtmlNode? root, string xpath)
            => Clean(root?.SelectSingleNode(xpath)?.InnerText);

        private static string? Clean(string? s)
        {
            if (s is null)
                return null;

            var v = WhitespaceRegex().Replace(WebUtility.HtmlDecode(s), " ").Trim();
            return v.Length == 0 || v == "-" ? null : v;
        }

        private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

        private static double? ParseDouble(string? s)
        {
            var m = s is null ? null : NumberRegex().Match(s);
            return m is { Success: true }
                   && double.TryParse(m.Value.Replace(",", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                ? v : null;
        }

        private static int? ParseInt(string? s) => ParseDouble(s) is { } d ? (int)d : null;

        // ---------- source payloads ----------

        private sealed record DjsonPayload(
            [property: JsonPropertyName("mmsi")] long Mmsi,
            [property: JsonPropertyName("ship_lat")] double? Lat,
            [property: JsonPropertyName("ship_lon")] double? Lon,
            [property: JsonPropertyName("ship_cog")] double? Cog,
            [property: JsonPropertyName("ship_sog")] double? Sog);

        private sealed record VuPayload(
            [property: JsonPropertyName("speed_average")] double? SpeedAverage,
            [property: JsonPropertyName("speed_max")] double? SpeedMax,
            [property: JsonPropertyName("pc")] int? PortCalls,   // meaning inferred, not documented
            [property: JsonPropertyName("ports")] List<VuPort>? Ports);

        private sealed record VuPort(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("country")] string? Country,
            [property: JsonPropertyName("locode")] string? Locode);
    }
}
