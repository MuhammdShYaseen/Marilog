using Marilog.Kernel.Enums;

using System;
using System.Collections.Generic;
using System.Text;

namespace Marilog.Contracts.DTOs.Responses
{
    // ═══════════════════════════════════════════════════════════════════════
    // Charter Terms — Response
    // ═══════════════════════════════════════════════════════════════════════

    public record CharterTermsResponse(
        int Id,
        int ContractId,
        decimal CargoQuantityMt,
        CargoOperationTermsResponse? Loading,
        CargoOperationTermsResponse? Discharging,
        DemurrageTermsResponse Demurrage,
        DespatchTermsResponse? Despatch,
        LaytimeRuleOptionsResponse RuleOptions);

    public record CargoOperationTermsResponse(
        OperationType OperationType,
        decimal RateMtPerDay,
        LaytimeCalendarType CalendarType,
        int NoticeHours,
        bool IsReversible,
        bool IsWeatherWorkingDay);

    public record DemurrageTermsResponse(
        decimal RateUsdPerDay,
        bool OnceOnDemurrageAlwaysOnDemurrage);

    public record DespatchTermsResponse(
        decimal RateUsdPerDay,
        DespatchBasis Basis);

    public record LaytimeRuleOptionsResponse(
        bool DraftSurveyCounts,
        bool HolidaysIncluded,
        bool SundaysIncluded,
        bool TimeReversible,
        bool AllowShiftingTime);

    // ═══════════════════════════════════════════════════════════════════════
    // Laytime Calculation — Response
    // ═══════════════════════════════════════════════════════════════════════

    public record LaytimeCalculationResponse(
        int Id,
        int VoyageId,
        int ContractId,
        int PortId,
        OperationType OperationType,
        decimal CargoQuantityMt,
        LaytimeStatus Status,
        DateTime? LaytimeCommencedAt,
        DateTime? LaytimeCompletedAt,
        LaytimeResultResponse? Result);

    public record LaytimeCalculationSummaryResponse(
        int Id,
        int VoyageId,
        OperationType OperationType,
        LaytimeStatus Status,
        decimal? AllowedDays,
        decimal? UsedDays,
        decimal? DemurrageAmount,
        decimal? DespatchAmount,
        bool? IsDemurrage);

    public record LaytimeResultResponse(
        decimal AllowedDays,
        decimal UsedDays,
        decimal BalanceDays,
        decimal DemurrageAmount,
        decimal DespatchAmount,
        bool IsDemurrage);


    // ═══════════════════════════════════════════════════════════════════════
    // SofEvent — Response
    // ═══════════════════════════════════════════════════════════════════════
    public record SofEventResponse(
        int Id,
        int LaytimeCalculationId,
        DateTime EventTime,
        SofEventType EventType,
        LaytimeImpactType ImpactType,
        decimal Factor,
        string? Description);


    // ═══════════════════════════════════════════════════════════════════════
    // Laytime Segments — Response
    // ═══════════════════════════════════════════════════════════════════════

    public record LaytimeSegmentResponse(
        int Id,
        DateTime From,
        DateTime To,
        LaytimeImpactType ImpactType,
        decimal Factor,
        string DurationDisplay,        // "7h 15m"
        string CountedDurationDisplay, // "7h 15m" أو "0h 00m"
        string? Reason);
    // ═══════════════════════════════════════════════════════════════════════
    // Exceptions —  Response
    // ═══════════════════════════════════════════════════════════════════════

    public record LaytimeExceptionResponse(
        int Id,
        DateTime From,
        DateTime To,
        LaytimeExceptionType ExceptionType,
        decimal Factor,
        string DurationDisplay,
        string? Notes,
        int? LinkedSofEventId);
}
