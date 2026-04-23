using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
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

        public async Task<LaytimeCalculationResponse> GetCalculationAsync(
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

            // بناء Segments
            calculation.ClearSegments();
            var segments = _engine.BuildSegments(calculation);
            foreach (var segment in segments)
                calculation.AddSegment(segment);

            // حساب النتيجة
            var result = _engine.Calculate(
                calculation,
                operationTerms.RateMtPerDay,
                charterTerms.LaytimeTerms.Demurrage.RateUsdPerDay,
                charterTerms.LaytimeTerms.Despatch?.RateUsdPerDay ?? 0);

            var setResult = calculation.SetResult(result);
            if (setResult.IsFailure)
                throw new InvalidOperationException(setResult.Error);

            await _calculationRepo.SaveChangesAsync(cancellationToken);

            return _laytimeHelpper.MapResultResponse(result);
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

        public async Task FinalizeAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _laytimeHelpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: false, cancellationToken);

            var result = calculation.FinalizeCalculation();
            if (result.IsFailure)
                throw new InvalidOperationException(result.Error);

            await _calculationRepo.SaveChangesAsync(cancellationToken);
        }

    }
}
