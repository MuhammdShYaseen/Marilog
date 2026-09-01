// Direct, hand-curated display labels for the real Laytime enums —
// no reflection. Grouped where it improves usability (SofEventType has 22
// members, so the dropdown groups them under subheaders).

using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Pages.Laytime
{
    public static class LaytimeEnumLabels
    {
        public static string Label(OperationType value) => value switch
        {
            OperationType.Loading => "Loading",
            OperationType.Discharging => "Discharging",
            _ => value.ToString()
        };

        public static string Label(LaytimeImpactType value) => value switch
        {
            LaytimeImpactType.FullCount => "Full Count",
            LaytimeImpactType.NoCount => "No Count",
            LaytimeImpactType.ProRata => "Pro Rata",
            _ => value.ToString()
        };

        public static string Label(LaytimeCalendarType value) => value switch
        {
            LaytimeCalendarType.SHINC => "SHINC — Sundays & Holidays Included",
            LaytimeCalendarType.SHEX => "SHEX — Sundays & Holidays Excluded",
            LaytimeCalendarType.SSHINC => "SSHINC — Saturdays, Sundays & Holidays Included",
            LaytimeCalendarType.WeatherWorkingDay => "Weather Working Day",
            _ => value.ToString()
        };

        public static string Label(DespatchBasis value) => value switch
        {
            DespatchBasis.OnWorkingTimeSaved => "On Working Time Saved",
            DespatchBasis.OnAllTimeSaved => "On All Time Saved",
            _ => value.ToString()
        };

        public static string Label(LaytimeExceptionType value) => value switch
        {
            LaytimeExceptionType.Rain => "Rain",
            LaytimeExceptionType.CraneBreakdown => "Crane Breakdown",
            LaytimeExceptionType.SurveyDelay => "Survey Delay",
            LaytimeExceptionType.Holiday => "Holiday",
            LaytimeExceptionType.MechanicalFailure => "Mechanical Failure",
            LaytimeExceptionType.Strike => "Strike",
            LaytimeExceptionType.PortCongestion => "Port Congestion",
            LaytimeExceptionType.Custom => "Custom",
            LaytimeExceptionType.Other => "Other",
            _ => value.ToString()
        };

        public static string Label(SofEventType value) => value switch
        {
            SofEventType.NorTendered => "NOR Tendered",
            SofEventType.NorAccepted => "NOR Accepted",
            SofEventType.LaytimeCommenced => "Laytime Commenced",
            SofEventType.PilotOnBoard => "Pilot On Board",
            SofEventType.VesselBerthed => "Vessel Berthed",
            SofEventType.GangwayDown => "Gangway Down",
            SofEventType.HatchesOpen => "Hatches Open",
            SofEventType.LoadingCommenced => "Loading Commenced",
            SofEventType.LoadingCompleted => "Loading Completed",
            SofEventType.DischargingCommenced => "Discharging Commenced",
            SofEventType.DischargingCompleted => "Discharging Completed",
            SofEventType.LaytimeCompleted => "Laytime Completed",
            SofEventType.DraftSurveyStart => "Draft Survey Start",
            SofEventType.DraftSurveyEnd => "Draft Survey End",
            SofEventType.CraneBreakdownStart => "Crane Breakdown Start",
            SofEventType.CraneBreakdownEnd => "Crane Breakdown End",
            SofEventType.RainStart => "Rain Start",
            SofEventType.RainEnd => "Rain End",
            SofEventType.ShiftingStart => "Shifting Start",
            SofEventType.ShiftingEnd => "Shifting End",
            SofEventType.HatchesClosed => "Hatches Closed",
            SofEventType.VesselUnberthed => "Vessel Unberthed",
            SofEventType.Custom => "Custom",
            _ => value.ToString()
        };

        public static string Label(LaytimeStatus value) => value switch
        {
            LaytimeStatus.Draft => "Draft",
            LaytimeStatus.Computed => "Computed",
            LaytimeStatus.Finalized => "Finalized",
            _ => value.ToString()
        };

        // ── SOF Event grouping for the dropdown ────────────────────────────
        public static readonly (string GroupLabel, SofEventType[] Events)[] SofEventGroups =
        {
            ("Arrival & Notice", new[] { SofEventType.NorTendered, SofEventType.NorAccepted, SofEventType.PilotOnBoard }),
            ("Berthing", new[] { SofEventType.VesselBerthed, SofEventType.GangwayDown, SofEventType.HatchesOpen, SofEventType.HatchesClosed, SofEventType.VesselUnberthed }),
            ("Laytime Period", new[] { SofEventType.LaytimeCommenced, SofEventType.LaytimeCompleted }),
            ("Cargo Operations", new[] { SofEventType.LoadingCommenced, SofEventType.LoadingCompleted, SofEventType.DischargingCommenced, SofEventType.DischargingCompleted }),
            ("Draft Survey", new[] { SofEventType.DraftSurveyStart, SofEventType.DraftSurveyEnd }),
            ("Delays", new[] { SofEventType.CraneBreakdownStart, SofEventType.CraneBreakdownEnd, SofEventType.RainStart, SofEventType.RainEnd, SofEventType.ShiftingStart, SofEventType.ShiftingEnd }),
            ("Other", new[] { SofEventType.Custom }),
        };
    }
}
