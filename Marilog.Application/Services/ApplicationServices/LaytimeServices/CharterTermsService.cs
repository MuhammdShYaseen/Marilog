using Marilog.Application.Interfaces.Services;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.Interfaces.Repositories;
using Marilog.Domain.ValueObjects.Laytime;
using Microsoft.EntityFrameworkCore;


namespace Marilog.Application.Services.ApplicationServices.LaytimeServices
{
    public class CharterTermsService : ICharterTermsService
    {
        private readonly IRepository<CharterTerms> _charterTermsRepo;
        private readonly ILaytimeHelpper _laytimeHelpper;
        public CharterTermsService(IRepository<CharterTerms> repository, ILaytimeHelpper laytimeHelpper)
        {
            _charterTermsRepo = repository;
            _laytimeHelpper = laytimeHelpper;
        }
        public async Task<CharterTermsResponse> InitializeCharterTermsAsync(
             InitializeCharterTermsRequest request,
             CancellationToken cancellationToken = default)
        {
            var existing = await _charterTermsRepo
                .Query()
                .FirstOrDefaultAsync(x => x.ContractId == request.ContractId, cancellationToken);

            if (existing is not null)
                throw new InvalidOperationException(
                    $"CharterTerms already initialized for ContractId {request.ContractId}.");

            var laytimeTerms = LaytimeTerms.Create(
                demurrage: _laytimeHelpper.MapDemurrage(request.Demurrage),
                ruleOptions: _laytimeHelpper.MapRuleOptions(request.RuleOptions),
                loading: request.Loading is not null ? _laytimeHelpper.MapCargoOperation(request.Loading) : null,
                discharging: request.Discharging is not null ? _laytimeHelpper.MapCargoOperation(request.Discharging) : null,
                despatch: request.Despatch is not null ? _laytimeHelpper.MapDespatch(request.Despatch) : null);

            var charterTerms = CharterTerms.Create(
                request.ContractId,
                request.CargoQuantityMt,
                laytimeTerms);

            await _charterTermsRepo.AddAsync(charterTerms, cancellationToken);
            await _charterTermsRepo.SaveChangesAsync(cancellationToken);

            return _laytimeHelpper.MapCharterTermsResponse(charterTerms);
        }

        public async Task UpdateLoadingTermsAsync(
            int contractId,
            CargoOperationTermsRequest request,
            CancellationToken cancellationToken = default)
        {
            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(contractId, cancellationToken);
            charterTerms.UpdateLoadingTerms(_laytimeHelpper.MapCargoOperation(request));
            await _charterTermsRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateDischargingTermsAsync(
            int contractId,
            CargoOperationTermsRequest request,
            CancellationToken cancellationToken = default)
        {
            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(contractId, cancellationToken);
            charterTerms.UpdateDischargingTerms(_laytimeHelpper.MapCargoOperation(request));
            await _charterTermsRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateDemurrageTermsAsync(
            int contractId,
            DemurrageTermsRequest request,
            CancellationToken cancellationToken = default)
        {
            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(contractId, cancellationToken);
            charterTerms.UpdateDemurrage(_laytimeHelpper.MapDemurrage(request));
            await _charterTermsRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateDespatchTermsAsync(
            int contractId,
            DespatchTermsRequest request,
            CancellationToken cancellationToken = default)
        {
            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(contractId, cancellationToken);
            charterTerms.UpdateDespatch(_laytimeHelpper.MapDespatch(request));
            await _charterTermsRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRuleOptionsAsync(
            int contractId,
            LaytimeRuleOptionsRequest request,
            CancellationToken cancellationToken = default)
        {
            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(contractId, cancellationToken);
            charterTerms.UpdateRuleOptions(_laytimeHelpper.MapRuleOptions(request));
            await _charterTermsRepo.SaveChangesAsync(cancellationToken);
        }

        public async Task<CharterTermsResponse> GetCharterTermsAsync(
            int contractId,
            CancellationToken cancellationToken = default)
        {
            var charterTerms = await _laytimeHelpper.GetCharterTermsOrThrowAsync(contractId, cancellationToken);
            return _laytimeHelpper.MapCharterTermsResponse(charterTerms);
        }
    }
}
