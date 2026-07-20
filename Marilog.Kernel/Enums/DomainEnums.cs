namespace Marilog.Kernel.Enums
{
    public enum PersonCertificateType
    {
        Other = 0,

        // ===== STCW Certificates =====
        StcwBasicSafetyTraining,            // BST
        StcwAdvancedFireFighting,
        StcwMedicalFirstAid,
        StcwMedicalCare,
        StcwProficiencyInSurvivalCraft,     // PSCRB
        StcwSecurityAwareness,
        StcwDesignatedSecurityDuties,
        StcwCertificateOfCompetency,        // COC - حسب الرتبة (Officer/Master/Engineer)
        StcwCertificateOfProficiency,       // COP
        StcwWatchkeeping,
        StcwCrowdManagement,                // للسفن الراكبين
        StcwTankerFamiliarization,          // Oil/Chemical/Gas tanker endorsement

        // ===== Medical =====
        MedicalFitnessCertificate,          // ENG1 أو ما يعادلها
        YellowFeverVaccinationCertificate,
        Covid19VaccinationCertificate,

        // ===== Identity & Travel Documents =====
        Passport,
        SeamanBook,                         // Seafarer's Identity Document (SID)
        Visa,
        NationalId,
        UsSeamanVisa,                       // C1/D Visa - شائع لبعض الموانئ

        // ===== Employment / Labour =====
        SeafarerEmploymentAgreement,        // SEA - MLC 2006
        DischargeBook,
        AntiBullyingHarassmentTraining,

        // ===== Endorsements / Flag-specific =====
        GmdssOperatorCertificate,
        FlagStateEndorsement,               // Endorsement attesting COC recognition

        // ===== Other Training =====
        SecurityTrainingCertificate,        // ISPS-related for individuals
        DrugAndAlcoholTestCertificate
    }

    public enum VesselCertificateType
    {
        Other = 0,

        // ===== Statutory Certificates (SOLAS / MARPOL / Load Line) =====
        SafetyManagementCertificate,        // SMC - ISM Code
        DocumentOfCompliance,               // DOC - ISM Code (company-level, لكن بيتربط بالباخرة)
        InternationalShipSecurityCertificate, // ISSC - ISPS Code
        InternationalLoadLineCertificate,   // ILLC
        InternationalTonnageCertificate,    // ITC 1969
        SafetyConstructionCertificate,      // SOLAS Ch. II-1/II-2
        SafetyEquipmentCertificate,         // SOLAS Ch. III
        SafetyRadioCertificate,             // SOLAS Ch. IV
        CargoShipSafetyCertificate,         // مجمع (لو مطبق بدل الثلاثة فوق)
        PassengerShipSafetyCertificate,

        // MARPOL Annexes
        InternationalOilPollutionPreventionCertificate, // IOPP - Annex I
        InternationalSewagePollutionPreventionCertificate, // ISPP - Annex IV
        InternationalAirPollutionPreventionCertificate, // IAPP - Annex VI
        InternationalEnergyEfficiencyCertificate,       // IEEC - Annex VI
        BallastWaterManagementCertificate,              // BWM Convention

        // Labour / Crew
        MaritimeLabourCertificate,          // MLC 2006
        DeclarationOfMaritimeLabourCompliance, // DMLC

        // ===== Class Certificates =====
        ClassCertificate,                   // Certificate of Class
        HullClassCertificate,
        MachineryClassCertificate,
        ContinuousSurveyReport,

        // ===== Flag State =====
        CertificateOfRegistry,              // شهادة التسجيل
        FlagStateAuthorizationCertificate,

        // ===== Port State Control / Inspection =====
        PortStateControlInspectionReport,

        // ===== Insurance / Financial =====
        CivilLiabilityCertificate,          // CLC - Oil Pollution
        BunkerConventionCertificate,        // Bunker Oil Pollution
        P_and_I_CertificateOfEntry,         // P&I Club

        // ===== Radio / Communication =====
        ShipStationLicence,

        // ===== Other Operational =====
        MinimumSafeManningDocument,
        GarbageManagementPlanCertificate,
        OilRecordBookCertificate
    }
    public enum PhoneType { Office, Mobile, Fax, WhatsApp }
    public enum EmailRole { General, Operations, Accounts, Legal, Technical }
    public enum BlType
    {
        Straight,      // Non-negotiable, named consignee
        OrderBl,       // Negotiable, transferable by endorsement
        BearerBl       // Negotiable, no named consignee
    }
    public enum ContractRole
    {
        Owner,      // المالك
        Charterer,  // المستأجر
        Agent,      // الوكيل
        Supplier,   // المورد
        Guarantor,  // الضامن
    }
    public enum CrewContractStatus
    {
        Valied,
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
        
        CrewContract = 1,
        CrewPayroll = 2,
        Document = 3,
        EmailAttachment = 4,
        SwiftTransfer = 5,
        Person = 6,
        Vessel = 7,
        AContract = 8,
        Bank = 9,
        Company = 10,
        PersonCertificate = 11,
        NON = 0
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
