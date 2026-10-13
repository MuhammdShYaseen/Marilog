using Marilog.Application.Interfaces.Services.Laytime;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Kernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices
{
    public class LaytimeCalculationService : ILaytimeCalculationService
    {
        private readonly IRepository<LaytimeCalculation> _calculationRepo;
        private readonly ILaytimeHelpper _laytimeHelpper;
        private readonly ILaytimeEngine _engine;
        public LaytimeCalculationService(IRepository<LaytimeCalculation> calculationRepo, ILaytimeHelpper helpper, ILaytimeEngine engine)
        {
            _calculationRepo = calculationRepo;
            _laytimeHelpper = helpper;
            _engine = engine;
        }
        public async Task<LaytimeCalculationResponse> CreateCalculationAsync(
            CreateLaytimeCalculationRequest request,
            CancellationToken cancellationToken = default)
        {
            var calculation = LaytimeCalculation.Create(
                request.VoyageId,
                request.ContractId,
                request.PortId,
                request.OperationType,
                request.CargoQuantityMt);

            await _calculationRepo.AddAsync(calculation, cancellationToken);
            await _calculationRepo.SaveChangesAsync(cancellationToken);

            return _laytimeHelpper.MapCalculationResponse(calculation);
        }

        public async Task<LaytimeCalculationResponse?> GetCalculationAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _laytimeHelpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            return _laytimeHelpper.MapCalculationResponse(calculation);
        }

        public async Task<IReadOnlyList<LaytimeCalculationSummaryResponse>> GetCalculationsByVoyageAsync(
            int voyageId,
            CancellationToken cancellationToken = default)
        {
            var calculations = await _calculationRepo
                .Query()
                .Where(x => x.VoyageId == voyageId)
                .ToListAsync(cancellationToken);

            return calculations
                .Select(_laytimeHelpper.MapCalculationSummaryResponse)
                .ToList();
        }

        public async Task<LaytimeResultResponse> ComputeAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _laytimeHelpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            if (calculation.SofEvents.Count < 2)
                throw new InvalidOperationException(
                    "At least 2 SOF events are required to compute Laytime.");

            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(
                calculation.ContractId, cancellationToken);

            if (!charterTerms.LaytimeTerms.IsReadyForCalculation())
                throw new InvalidOperationException(
                    "Charter Terms are not ready for calculation. Ensure Loading/Discharging and Demurrage are configured.");

            var operationTerms = _laytimeHelpper.GetOperationTerms(charterTerms, calculation.OperationType);
            var ruleOptions = charterTerms.LaytimeTerms.RuleOptions;
            // LT-08: reversible laytime — one pool shared by the ports of this voyage
            decimal? allowedDaysOverride = null;
            var despatchRate = charterTerms.LaytimeTerms.Despatch?.RateUsdPerDay ?? 0;

            if (ruleOptions.TimeReversible)
            {
                allowedDaysOverride = await GetReversibleAllowedDaysAsync(calculation, charterTerms, cancellationToken);

                // Despatch is settled once, at discharge
                if (calculation.OperationType == OperationType.Loading)
                    despatchRate = 0;
            }
            // بناء Segments — مع تطبيق CalendarType (SHEX/PWWD) و NoticeHours و RuleOptions
            calculation.ClearSegments();
            var build = _engine.BuildSegments(
                        calculation,
                        operationTerms,
                        ruleOptions,
                        charterTerms.LaytimeTerms.Demurrage.OnceOnDemurrageAlwaysOnDemurrage,
                        allowedDaysOverride);

            foreach (var segment in build.Segments)
                calculation.AddSegment(segment);

            var result = _engine.Calculate(
                        calculation,
                        build,
                        charterTerms.LaytimeTerms.Demurrage.RateUsdPerDay,
                        despatchRate,
                        charterTerms.LaytimeTerms.Despatch?.Basis ?? DespatchBasis.OnWorkingTimeSaved);

            var setResult = calculation.SetResult(result, charterTerms.TermsVersion, build.LaytimeExpiredAt);
            if (setResult.IsFailure)
                throw new InvalidOperationException(setResult.Error);

            await _calculationRepo.SaveChangesAsync(cancellationToken);

            return _laytimeHelpper.MapResultResponse(result) with { LaytimeExpiredAt = build.LaytimeExpiredAt };
        }

        public async Task<LaytimeResultResponse> RecomputeAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _laytimeHelpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var recomputeResult = calculation.Recompute();
            if (recomputeResult.IsFailure)
                throw new InvalidOperationException(recomputeResult.Error);

            await _calculationRepo.SaveChangesAsync(cancellationToken);

            return await ComputeAsync(calculationId, cancellationToken);
        }

        public async Task FinalizeAsync(int calculationId, CancellationToken cancellationToken = default)
        {
            var calculation = await _laytimeHelpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: false, cancellationToken);

            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(
                calculation.ContractId, cancellationToken);

            var result = calculation.FinalizeCalculation(charterTerms.TermsVersion);
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error);

            await _calculationRepo.SaveChangesAsync(cancellationToken);
        }


        /// <summary>
        /// LT-08: allowed time left in the reversible pool for this calculation —
        /// (loading allowed + discharging allowed) minus the time used at earlier ports
        /// of the same voyage under the same contract. Earlier ports must be finalized.
        /// </summary>
        private async Task<decimal> GetReversibleAllowedDaysAsync(
            LaytimeCalculation calculation,
            CharterTerms charterTerms,
            CancellationToken cancellationToken)
        {
            var loading = charterTerms.LaytimeTerms.Loading
                ?? throw new InvalidOperationException("Reversible laytime needs both Loading and Discharging terms.");
            var discharging = charterTerms.LaytimeTerms.Discharging
                ?? throw new InvalidOperationException("Reversible laytime needs both Loading and Discharging terms.");

            var quantity = charterTerms.CargoQuantityMt > 0 ? charterTerms.CargoQuantityMt : calculation.CargoQuantityMt;
            var pooledDays = loading.AllowedDays(quantity) + discharging.AllowedDays(quantity);

            var isDischarging = calculation.OperationType == OperationType.Discharging;
            var operationType = calculation.OperationType;
            var firstEventTime = calculation.SofEvents.Min(e => e.EventTime);

            var earlier = await _calculationRepo.Query()
                .Where(c => c.VoyageId == calculation.VoyageId
                         && c.ContractId == calculation.ContractId
                         && c.Id != calculation.Id)
                .Where(c => (isDischarging && c.OperationType == OperationType.Loading)
                         || (c.OperationType == operationType && c.LaytimeCommencedAt < firstEventTime))
                .ToListAsync(cancellationToken);

            if (earlier.Any(c => c.Status != LaytimeStatus.Finalized || c.Result is null))
                throw new InvalidOperationException(
                    "Reversible laytime: finalize the earlier port calculations of this voyage first.");

            var usedBefore = earlier.Sum(c => c.Result!.UsedDays);
            return Math.Max(0, pooledDays - usedBefore);
        }

    }
}