namespace Marilog.Kernel.Enums
{
    public enum PhoneType { Office, Mobile, Fax, WhatsApp }
    public enum EmailRole { General, Operations, Accounts, Legal, Technical }
    public enum BlType
    {
        Straight,      // Non-negotiable, named consignee
        OrderBl,       // Negotiable, transferable by endorsement
        BearerBl       // Negotiable, no named consignee
    }

    public enum CrewContractStatus
    {
        Active,
        Expiring,
        Expired
    }

    public enum BlIssuanceType
    {
        Master,        // MBL - issued by the actual carrier/vessel owner
        House          // HBL - issued by freight forwarder / NVOCC / agent
    }

    public enum FreightTerms
    {
        Prepaid,       // Shipper pays freight
        Collect,       // Consignee pays at destination
        ThirdParty     // Third party pays
    }
    public enum EntityType
    {
        AContract = 0,
        CrewContract = 1,
        CrewPayroll = 2,
        Document = 3,
        EmailAttachment = 4,
        SwiftTransfer = 5,
        Person = 6,
        Vessel = 7,
        NON = 8
    }
    public enum JobStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
    public enum AiProviderType
    {
        OpenAI = 1,
        Anthropic = 2,
        AzureOpenAI = 3,
        GoogleGemini = 4,
        Ollama = 5
    }
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
        CashOnBoard,   // نقداً على ظهر الباخرة 
        CashAtOffice,  // نقداً في مكتب الشركة 
        BankTransfer,
        Swift,
        Cash,
        Cheque,
        Other
    }
    public enum DisbursementStatus { Pending, Confirmed, Cancelled }

    //Rank Enums
    public enum Department { DECK, ENGINE, CATERING }

    //Voyage Enums
    public enum VoyageStatus { PLANNED, UNDERWAY, COMPLETED, CANCELLED }
    public enum FinancialSide
    {
        None = 0,
        Revenue = 1, //مدخول
        Expense = 2  //مصروفات
    }

    public class DomainEnums
    {
    }
}
