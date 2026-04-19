
namespace Marilog.Contracts.Enums
{
    //LayTime Enums
    public enum OperationType
    {
        Loading = 1,
        Discharging = 2
    }

    public enum LaytimeStatus
    {
        Draft = 1,
        Computed = 2,
        Finalized = 3
    }

    public enum LaytimeImpactType
    {
        FullCount = 1,
        NoCount = 2,
        ProRata = 3
    }

    /// <summary>
    /// نوع التقويم المستخدم في احتساب الـ Laytime — مصطلحات Charter Party الرسمية.
    /// SHINC  = Sundays and Holidays Included
    /// SHEX   = Sundays and Holidays Excluded
    /// SSHINC = Saturdays, Sundays and Holidays Included
    /// </summary>
    public enum LaytimeCalendarType
    {
        SHINC = 1,
        SHEX = 2,
        SSHINC = 3,
        WeatherWorkingDay = 4
    }

    public enum DespatchBasis
    {
        OnWorkingTimeSaved = 1,
        OnAllTimeSaved = 2
    }

    public enum LaytimeExceptionType
    {
        Rain = 1,
        CraneBreakdown = 2,
        SurveyDelay = 3,
        Holiday = 4,
        MechanicalFailure = 5,
        Strike = 6,
        PortCongestion = 7,
        Custom = 8,
        Other = 9
    }

    /// <summary>
    /// أنواع أحداث Statement of Facts.
    /// الترتيب العددي لا يعكس تسلسلاً إجبارياً — الترتيب يحدده EventTime.
    /// </summary>
    public enum SofEventType
    {
        NorTendered = 1,
        NorAccepted = 2,
        LaytimeCommenced = 3,   // بدء احتساب الـ Laytime الفعلي
        PilotOnBoard = 4,
        VesselBerthed = 5,
        GangwayDown = 6,
        HatchesOpen = 7,
        LoadingCommenced = 8,
        LoadingCompleted = 9,
        DischargingCommenced = 10,
        DischargingCompleted = 11,
        LaytimeCompleted = 12,  // نهاية احتساب الـ Laytime
        DraftSurveyStart = 13,
        DraftSurveyEnd = 14,
        CraneBreakdownStart = 15,
        CraneBreakdownEnd = 16,
        RainStart = 17,
        RainEnd = 18,
        ShiftingStart = 19,
        ShiftingEnd = 20,
        HatchesClosed = 21,
        VesselUnberthed = 22,
        Custom = 99
    }

    //Email Enums
    public enum EmailDirection { Outbound, Inbound }
    public enum EmailStatus { Draft, Sent, Received, Failed }
    public enum ParticipantRole { From, To, Cc }
    public enum ParticipantType { Company, Vessel }


    //Payroll Enums
    public enum PayrollStatus { Draft, Approved, PartiallyPaid, FullyPaid, Cancelled }
    public enum PaymentMethod
    {
        CashOnBoard,   // نقداً على ظهر الباخرة   — VoyageId مطلوب
        CashAtOffice,  // نقداً في مكتب الشركة    — City + Country + بيانات المستلم
        BankTransfer   // حوالة بنكية              — SwiftTransferId مطلوب
    }
    public enum DisbursementStatus { Pending, Confirmed, Cancelled }

    //Rank Enums
    public enum Department { DECK, ENGINE, CATERING }

    //Voyage Enums
    public enum VoyageStatus { PLANNED, UNDERWAY, COMPLETED, CANCELLED }

    public class DomainEnums
    {
    }
}
