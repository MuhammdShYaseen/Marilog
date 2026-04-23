using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Domain.Entities.LaytimeEntities;
using Marilog.Domain.ValueObjects.Laytime;
using Marilog.Kernel.Enums;

namespace Marilog.Application.Interfaces.Services
{
    public interface ILaytimeHelpper
    {

        Task<CharterTerms> GetCharterTermsOrThrowAsync(int contractId, CancellationToken cancellationToken);

        Task<LaytimeCalculation> GetCalculationOrThrowAsync(int calculationId, bool withIncludes, CancellationToken cancellationToken);

        CargoOperationTerms GetOperationTerms(CharterTerms charterTerms, OperationType operationType);

        CargoOperationTerms MapCargoOperation(CargoOperationTermsRequest r);

        DemurrageTerms MapDemurrage(DemurrageTermsRequest r);

        DespatchTerms MapDespatch(DespatchTermsRequest r);

        LaytimeRuleOptions MapRuleOptions(LaytimeRuleOptionsRequest r);

        CharterTermsResponse MapCharterTermsResponse(CharterTerms ct);

        CargoOperationTermsResponse MapCargoOpResponse(CargoOperationTerms t);

        DemurrageTermsResponse MapDemurrageResponse(DemurrageTerms d);

        DespatchTermsResponse MapDespatchResponse(DespatchTerms d);

        LaytimeRuleOptionsResponse MapRuleOptionsResponse(LaytimeRuleOptions r);

        LaytimeCalculationResponse MapCalculationResponse(LaytimeCalculation c);

        LaytimeCalculationSummaryResponse MapCalculationSummaryResponse(LaytimeCalculation c);

        LaytimeResultResponse MapResultResponse(LaytimeResult r);

        SofEventResponse MapSofEventResponse(SofEvent e);

        LaytimeSegmentResponse MapSegmentResponse(LaytimeSegment s);

        LaytimeExceptionResponse MapExceptionResponse(LaytimeException e);


    }
}
