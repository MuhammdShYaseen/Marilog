using Marilog.Kernel.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Requests.LayTimeDTOs
{
    public enum ReportFormat
    {
        Pdf = 1,
        Excel = 2
    }
    // ═══════════════════════════════════════════════════════════════════════
    // Charter Terms — Requests
    // ═══════════════════════════════════════════════════════════════════════

    public record InitializeCharterTermsRequest(
        int ContractId,
        decimal CargoQuantityMt,
        CargoOperationTermsRequest? Loading,
        CargoOperationTermsRequest? Discharging,
        DemurrageTermsRequest Demurrage,
        DespatchTermsRequest? Despatch,
        LaytimeRuleOptionsRequest RuleOptions);

    public record CargoOperationTermsRequest(
        OperationType OperationType,
        decimal RateMtPerDay,
        LaytimeCalendarType CalendarType,
        int NoticeHours,
        bool IsReversible = false);

    public record DemurrageTermsRequest(
        decimal RateUsdPerDay,
        bool OnceOnDemurrageAlwaysOnDemurrage = false);

    public record DespatchTermsRequest(
        decimal RateUsdPerDay,
        DespatchBasis Basis);

    public record LaytimeRuleOptionsRequest(
        bool DraftSurveyCounts,
        bool HolidaysIncluded,
        bool SundaysIncluded,
        bool TimeReversible,
        bool AllowShiftingTime);

    // ═══════════════════════════════════════════════════════════════════════
    // Laytime Calculation — Requests
    // ═══════════════════════════════════════════════════════════════════════

    public record CreateLaytimeCalculationRequest(
        int VoyageId,
        int ContractId,
        int PortId,
        OperationType OperationType,
        decimal CargoQuantityMt);


    // ═══════════════════════════════════════════════════════════════════════
    // SOF Events — Requests 
    // ═══════════════════════════════════════════════════════════════════════

    public record AddSofEventRequest(
        DateTime EventTime,
        SofEventType EventType,
        LaytimeImpactType ImpactType,
        decimal Factor = 1.0m,
        string? Description = null);

    public record UpdateSofEventImpactRequest(
        LaytimeImpactType ImpactType,
        decimal Factor);


    // ═══════════════════════════════════════════════════════════════════════
    // Exceptions — Requests 
    // ═══════════════════════════════════════════════════════════════════════

    public record AddLaytimeExceptionRequest(
        DateTime From,
        DateTime To,
        LaytimeExceptionType ExceptionType,
        decimal Factor,
        string? Notes = null,
        int? LinkedSofEventId = null);

    public record UpdateLaytimeExceptionRequest(
        decimal Factor,
        string? Notes);
}
