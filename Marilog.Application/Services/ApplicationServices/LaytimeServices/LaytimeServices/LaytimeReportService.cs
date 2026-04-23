using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Application.Services.ApplicationServices.LaytimeServices.LaytimeServices
{
    public class LaytimeReportService : ILaytimeReportService
    {
        private readonly ILaytimeReportCreator _creator;
        private readonly ILaytimeHelpper _helpper;
        private readonly IRepository<LaytimeCalculation> _calculationRepo;
        public LaytimeReportService(ILaytimeReportCreator creator, ILaytimeHelpper helpper, IRepository<LaytimeCalculation> calculationRepo)
        {
            _creator = creator;
            _helpper = helpper;
            _calculationRepo = calculationRepo;
        }
        public async Task<byte[]> GenerateTimeSheetExcelAsync(
            int calculationId,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var charterTerms = await _helpper.GetCharterTermsOrThrowAsync(
                calculation.ContractId, cancellationToken);

            return await _creator.GenerateTimeSheetExcelAsync(
                calculation, charterTerms, cancellationToken);
        }

        public async Task<byte[]> GenerateSummaryReportAsync(
            int calculationId,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var charterTerms = await _helpper.GetCharterTermsOrThrowAsync(
                calculation.ContractId, cancellationToken);

            return await _creator.GenerateSummaryReportAsync(
                calculation, charterTerms, format, cancellationToken);
        }

        public async Task<byte[]> GenerateDetailedReportAsync(
            int calculationId,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var charterTerms = await _helpper.GetCharterTermsOrThrowAsync(
                calculation.ContractId, cancellationToken);

            return await _creator.GenerateDetailedReportAsync(
                calculation, charterTerms, format, cancellationToken);
        }

        public async Task<byte[]> GenerateDelayReportAsync(
            int calculationId,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            var calculation = await _helpper.GetCalculationOrThrowAsync(
                calculationId, withIncludes: true, cancellationToken);

            var charterTerms = await _helpper.GetCharterTermsOrThrowAsync(
                calculation.ContractId, cancellationToken);

            return await _creator.GenerateDelayReportAsync(
                calculation, charterTerms, format, cancellationToken);
        }

        public async Task<byte[]> GenerateContractLaytimeReportAsync(
            int contractId,
            ReportFormat format,
            CancellationToken cancellationToken = default)
        {
            var calculations = await _calculationRepo
                .Query()
                .Include(x => x.SofEvents)
                .Include(x => x.Segments)
                .Include(x => x.Exceptions)
                .Where(x => x.ContractId == contractId)
                .ToListAsync(cancellationToken);

            if (calculations.Count == 0)
                throw new KeyNotFoundException(
                    $"No calculations found for ContractId {contractId}.");

            var charterTerms = await _helpper.GetCharterTermsOrThrowAsync(contractId, cancellationToken);

            return await _creator.GenerateContractLaytimeReportAsync(
                calculations, charterTerms, format, cancellationToken);
        }
    }
}
