using System.ComponentModel.DataAnnotations;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.SystemServices;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.Weather
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherController : ControllerBase
    {
        private readonly IWeatherService _service;

        public WeatherController(IWeatherService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<ApiResponse<WeatherResponse>>> Get(
            [FromQuery, Required, Range(-90, 90)] double? lat,
            [FromQuery, Required, Range(-180, 180)] double? lon,
            CancellationToken ct)
        {
            var result = await _service.GetAtAsync(lat!.Value, lon!.Value, ct);
            return result is null ? NotFound() : Ok(ApiResponse<WeatherResponse>.Ok(result));
        }
    }
}