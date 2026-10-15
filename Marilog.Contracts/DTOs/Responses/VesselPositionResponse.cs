

namespace Marilog.Contracts.DTOs.Responses
{
    public sealed record VesselPositionResponse(
     int Imo,
     long Mmsi,
     string? Name,
     string? CallSign,
     string? ShipType,
     string? AisType,
     string? Flag,
     int? YearBuilt,
     VesselDimensions Dimensions,
     VesselPositionInfo Position,
     VesselVoyageInfo Voyage,
     IReadOnlyList<VesselHistoryEntry> History,
     VesselUtilization? Utilization,
     string? PhotoUrl);

    public sealed record VesselDimensions(
        double? LengthOverall,      // m
        double? Beam,               // m
        int? GrossTonnage,
        int? Deadweight);           // t

    public sealed record VesselPositionInfo(
        double Latitude,            // rounded to whole degrees by the free source (~±55 km)
        double Longitude,
        double Course,
        double Speed,               // knots
        string? Area,               // e.g. "Indian Coast"
        string? NavigationStatus,
        double? CurrentDraught,     // m
        DateTime? LastReportUtc);

    public sealed record VesselVoyageInfo(
        PortReference? Destination,
        string? Eta,                // raw source text, no year / time zone
        PortReference? LastPort,
        DateTime? LastPortDepartureUtc);

    public sealed record PortReference(
        string Name,
        string? Country,
        string? PortCode);          // VesselFinder's own code (e.g. JOAQB002) — not a UN/LOCODE

    public sealed record VesselHistoryEntry(
        DateOnly Date,              // month precision
        string? VesselName,
        string? Flag);

    public sealed record VesselUtilization(
        int Year,
        double? SpeedAverage,
        double? SpeedMax,
        int? PortCalls,
        IReadOnlyList<PortReference> Ports);
}
