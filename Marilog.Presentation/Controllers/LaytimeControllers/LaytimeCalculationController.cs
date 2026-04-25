using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.LayTimeDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.CharterLaytimeServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.LaytimeControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LaytimeCalculationController : ControllerBase
    {
        private readonly ILaytimeCalculationService _service;

        public LaytimeCalculationController(ILaytimeCalculationService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ApiResponse<LaytimeCalculationResponse>> Create(CreateLaytimeCalculationRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateCalculationAsync(request, cancellationToken);
            return ApiResponse<LaytimeCalculationResponse>.Ok(result);
        }

        [HttpGet("{calculationId:int}")]
        public async Task<ApiResponse<LaytimeCalculationResponse>> Get(
            int calculationId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetCalculationAsync(calculationId, cancellationToken);
            return ApiResponse<LaytimeCalculationResponse>.Ok(result);
        }

        [HttpGet("voyage/{voyageId:int}")]
        public async Task<ApiResponse<IReadOnlyList<LaytimeCalculationSummaryResponse>>> GetByVoyage(int voyageId, CancellationToken cancellationToken)
        {
            var result = await _service.GetCalculationsByVoyageAsync(voyageId, cancellationToken);
            return ApiResponse<IReadOnlyList<LaytimeCalculationSummaryResponse>>.Ok(result);
        }

        [HttpPost("{calculationId:int}/compute")]
        public async Task<ApiResponse<LaytimeResultResponse>> Compute(int calculationId, CancellationToken cancellationToken)
        {
            var result = await _service.ComputeAsync(calculationId, cancellationToken);
            return ApiResponse<LaytimeResultResponse>.Ok(result);
        }

        [HttpPost("{calculationId:int}/recompute")]
        public async Task<ApiResponse<LaytimeResultResponse>> Recompute(int calculationId, CancellationToken cancellationToken)
        {
            var result = await _service.RecomputeAsync(calculationId, cancellationToken);
            return ApiResponse<LaytimeResultResponse>.Ok(result);
        }

        [HttpPost("{calculationId:int}/finalize")]
        public async Task<ApiResponse<object>> Finalize(int calculationId, CancellationToken cancellationToken)
        {
            await _service.FinalizeAsync(calculationId, cancellationToken);
            return ApiResponse<object>.Ok("Finalized");
        }
    }
}
