using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.PersonDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.DTOs.Responses.Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/person-certificates")]
    public class PersonCertificatesController : ControllerBase
    {
        private readonly IPersonCertificateService _service;

        public PersonCertificatesController(IPersonCertificateService service) => _service = service;

        [HttpGet("summary")]
        public async Task<ActionResult<ApiResponse<CertificateSummaryResponse>>> GetSummary(
            [FromQuery] int? personId, [FromQuery] bool includeInactivePersons, CancellationToken ct)
        {
            var result = await _service.GetSummaryAsync(personId, includeInactivePersons, ct);
            return Ok(ApiResponse<CertificateSummaryResponse>.Ok(result));
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<CertificateResponse>>>> GetPaged([FromQuery] PagedRequest paging, [FromQuery] PersonCertificateFilter filter, CancellationToken ct)
        {
            var result = await _service.GetPagedAsync(paging, filter, ct);
            return Ok(ApiResponse<PagedResponse<CertificateResponse>>.Ok(result));
        }


        [HttpGet("names")]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<string>>>> GetNames(CancellationToken ct)
        {
            var result = await _service.GetNamesAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<string>>.Ok(result));
        }
    }
}