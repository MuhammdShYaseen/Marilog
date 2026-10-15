using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.VesselDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/vessel-certificates")]
    public class VesselCertificatesController : ControllerBase
    {
        private readonly IVesselCertificateService _service;

        public VesselCertificatesController(IVesselCertificateService service) => _service = service;

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<CertificateSummaryResponse>>> GetSummary(
            [FromQuery] int? vesselId, [FromQuery] bool includeInactiveVessels, CancellationToken ct)
        {
            var result = await _service.GetSummaryAsync(vesselId, includeInactiveVessels, ct);
            return Ok(ApiResponse<CertificateSummaryResponse>.Ok(result));
        }


        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CertificateResponse>>>> GetPaged([FromQuery] PagedRequest paging, [FromQuery] VesselCertificateFilter filter, CancellationToken ct)
        {
            var result = await _service.GetPagedAsync(paging, filter, ct);
            return Ok(ApiResponse<PagedResponse<CertificateResponse>>.Ok(result));
        }
    }
}