using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.ValueObjects.Laytime;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices
{
    public class LaytimeHelpper : ILaytimeHelpper
    {
        private readonly IRepository<CharterTerms> _charterTermsRepo;
        private readonly IRepository<LaytimeCalculation> _calculationRepo;
        public LaytimeHelpper(IRepository<LaytimeCalculation> calculationRepo, IRepository<CharterTerms> charterTermsRepo)
        {
            _calculationRepo = calculationRepo;
            _charterTermsRepo = charterTermsRepo;
        }
        // ═══════════════════════════════════════════════════════════════════
        //  — Data Access Helpers
        // ═══════════════════════════════════════════════════════════════════

        public async Task<CharterTerms> GetCharterTermsOrThrowAsync(
            int contractId,
            CancellationToken cancellationToken)
        {
            return await _charterTermsRepo
                .Query()
                .FirstOrDefaultAsync(x => x.ContractId == contractId, cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"CharterTerms not found for ContractId {contractId}.");
        }

        public async Task<LaytimeCalculation> GetCalculationOrThrowAsync(
            int calculationId,
            bool withIncludes,
            CancellationToken cancellationToken)
        {
            var query = _calculationRepo.Query();

            if (withIncludes)
            {
                query = query
                    .Include(x => x.SofEvents)
                    .Include(x => x.Segments)
                    .Include(x => x.Exceptions);
            }

            return await query
                .FirstOrDefaultAsync(x => x.Id == calculationId, cancellationToken)
                ?? throw new KeyNotFoundException(
                    $"LaytimeCalculation {calculationId} not found.");
        }

        public CargoOperationTerms GetOperationTerms(
            CharterTerms charterTerms,
            OperationType operationType)
        {
            var terms = operationType == OperationType.Loading
                ? charterTerms.LaytimeTerms.Loading
                : charterTerms.LaytimeTerms.Discharging;

            return terms ?? throw new InvalidOperationException(
                $"No {operationType} terms configured in CharterTerms.");
        }

        // ═══════════════════════════════════════════════════════════════════
        //  — Domain → Request Mappers
        // ═══════════════════════════════════════════════════════════════════

        public CargoOperationTerms MapCargoOperation(CargoOperationTermsRequest r) =>
            CargoOperationTerms.Create(
                r.OperationType,
                r.RateMtPerDay,
                r.CalendarType,
                r.NoticeHours,
                r.IsReversible);

        public DemurrageTerms MapDemurrage(DemurrageTermsRequest r) =>
            DemurrageTerms.Create(
                r.RateUsdPerDay,
                r.OnceOnDemurrageAlwaysOnDemurrage);

        public DespatchTerms MapDespatch(DespatchTermsRequest r) =>
            DespatchTerms.Create(r.RateUsdPerDay, r.Basis);

        public LaytimeRuleOptions MapRuleOptions(LaytimeRuleOptionsRequest r) =>
            LaytimeRuleOptions.Create(
                r.DraftSurveyCounts,
                r.HolidaysIncluded,
                r.SundaysIncluded,
                r.TimeReversible,
                r.AllowShiftingTime);

        // ═══════════════════════════════════════════════════════════════════
        //  — Domain → Response Mappers
        // ═══════════════════════════════════════════════════════════════════

        public CharterTermsResponse MapCharterTermsResponse(CharterTerms ct) =>
            new(
                ct.Id,
                ct.ContractId,
                ct.CargoQuantityMt,
                ct.LaytimeTerms.Loading is not null ? MapCargoOpResponse(ct.LaytimeTerms.Loading) : null,
                ct.LaytimeTerms.Discharging is not null ? MapCargoOpResponse(ct.LaytimeTerms.Discharging) : null,
                MapDemurrageResponse(ct.LaytimeTerms.Demurrage),
                ct.LaytimeTerms.Despatch is not null ? MapDespatchResponse(ct.LaytimeTerms.Despatch) : null,
                MapRuleOptionsResponse(ct.LaytimeTerms.RuleOptions));

        public CargoOperationTermsResponse MapCargoOpResponse(CargoOperationTerms t) =>
            new(t.OperationType, t.RateMtPerDay, t.CalendarType,
                t.NoticeHours, t.IsReversible, t.IsWeatherWorkingDay);

        public DemurrageTermsResponse MapDemurrageResponse(DemurrageTerms d) =>
            new(d.RateUsdPerDay, d.OnceOnDemurrageAlwaysOnDemurrage);

        public DespatchTermsResponse MapDespatchResponse(DespatchTerms d) =>
            new(d.RateUsdPerDay, d.Basis);

        public LaytimeRuleOptionsResponse MapRuleOptionsResponse(LaytimeRuleOptions r) =>
            new(r.DraftSurveyCounts, r.HolidaysIncluded, r.SundaysIncluded,
                r.TimeReversible, r.AllowShiftingTime);

        public LaytimeCalculationResponse MapCalculationResponse(LaytimeCalculation c) =>
            new(
                c.Id,
                c.VoyageId,
                c.ContractId,
                c.PortId,
                c.OperationType,
                c.CargoQuantityMt,
                c.Status,
                c.LaytimeCommencedAt,
                c.LaytimeCompletedAt,
                c.Result is not null ? MapResultResponse(c.Result) : null);

        public LaytimeCalculationSummaryResponse MapCalculationSummaryResponse(
            LaytimeCalculation c) =>
            new(
                c.Id,
                c.VoyageId,
                c.OperationType,
                c.Status,
                c.Result?.AllowedDays,
                c.Result?.UsedDays,
                c.Result?.DemurrageAmount,
                c.Result?.DespatchAmount,
                c.Result?.IsDemurrage);

        public LaytimeResultResponse MapResultResponse(LaytimeResult r) =>
            new(r.AllowedDays, r.UsedDays, r.BalanceDays,
                r.DemurrageAmount, r.DespatchAmount, r.IsDemurrage);

        public SofEventResponse MapSofEventResponse(SofEvent e) =>
            new(e.Id, e.LaytimeCalculationId, e.EventTime,
                e.EventType, e.ImpactType, e.Factor, e.Description);

        public LaytimeSegmentResponse MapSegmentResponse(LaytimeSegment s) =>
            new(
                s.Id,
                s.From,
                s.To,
                s.ImpactType,
                s.Factor,
                LaytimeEngine.FormatDuration(s.Duration),
                LaytimeEngine.FormatDuration(s.CountedDuration),
                s.Reason);

        public LaytimeExceptionResponse MapExceptionResponse(LaytimeException e) =>
            new(
                e.Id,
                e.From,
                e.To,
                e.ExceptionType,
                e.Factor,
                LaytimeEngine.FormatDuration(e.Duration),
                e.Notes,
                e.LinkedSofEventId);
    }
}
