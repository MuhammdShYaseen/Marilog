using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;

namespace Marilog.Shared.UI.Pages.Laytime
{
    // Plain mutable holders used for two-way binding inside the setup/edit forms —
    // never sent over the wire directly; ToRequest() builds the real DTOs.

    public sealed class CargoOperationFormValues
    {
        public decimal RateMtPerDay { get; set; } = 1000;
        public LaytimeCalendarType CalendarType { get; set; }
        public int NoticeHours { get; set; } = 12;
        public bool IsReversible { get; set; }

        public bool IsValid => RateMtPerDay > 0 && NoticeHours >= 0;

        public CargoOperationTermsRequest ToRequest(OperationType operationType) =>
            new(operationType, RateMtPerDay, CalendarType, NoticeHours, IsReversible);

        public static CargoOperationFormValues FromResponse(CargoOperationTermsResponse r) => new()
        {
            RateMtPerDay = r.RateMtPerDay,
            CalendarType = r.CalendarType,
            NoticeHours = r.NoticeHours,
            IsReversible = r.IsReversible
        };
    }

    public sealed class LaytimeRuleOptionsFormValues
    {
        public bool DraftSurveyCounts { get; set; }
        public bool HolidaysIncluded { get; set; }
        public bool WeekendIncluded { get; set; } = true;
        public DayOfWeek WeekendDay1 { get; set; } = DayOfWeek.Friday;
        public DayOfWeek WeekendDay2 { get; set; } = DayOfWeek.Saturday;
        public bool SundaysIncluded { get; set; } = true;
        public bool TimeReversible { get; set; }
        public bool AllowShiftingTime { get; set; }

        public LaytimeRuleOptionsRequest ToRequest() =>
            new(DraftSurveyCounts, HolidaysIncluded, WeekendIncluded, WeekendDay1, WeekendDay2,
                SundaysIncluded, TimeReversible, AllowShiftingTime);

        public static LaytimeRuleOptionsFormValues FromResponse(LaytimeRuleOptionsResponse r) => new()
        {
            DraftSurveyCounts = r.DraftSurveyCounts,
            HolidaysIncluded = r.HolidaysIncluded,
            WeekendIncluded = r.WeekendIncluded,
            WeekendDay1 = r.WeekendDay1 ?? DayOfWeek.Friday,
            WeekendDay2 = r.WeekendDay2 ?? DayOfWeek.Saturday,
            TimeReversible = r.TimeReversible,
            AllowShiftingTime = r.AllowShiftingTime
        };
    }
}

namespace Marilog.Shared.UI.Components.Laytime
{
    // Client-side only — distinguishes which report endpoint to call; not a server enum.
    public enum LaytimeReportType
    {
        Summary,
        Detailed,
        Delays,
        TimeSheetExcel
    }
}
