using Marilog.Application.Interfaces.Email;
using Marilog.Contracts.Common;
using Marilog.Contracts.DTOs.Requests.EmailDTOs;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.EmailServices;
using Marilog.Contracts.Options;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Marilog.Presentation.Controllers.SystemControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailAccountsController : ControllerBase
    {
        private readonly IEmailAccountService _service;
        private readonly IGoogleOAuthTokenService _googleOAuthService;
        private readonly GoogleOAuthOptions _googleOptions;
        private readonly UrlsOptions _UrlsOptions;
        public EmailAccountsController(IEmailAccountService service, IGoogleOAuthTokenService googleOAuthService, IOptions<GoogleOAuthOptions> options, IOptions<UrlsOptions> urlsOptions)
        {
            _service = service;
            _googleOAuthService = googleOAuthService;
            _googleOptions = options.Value;
            _UrlsOptions = urlsOptions.Value;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EmailAccountResponse>> GetById(int id, CancellationToken ct)
        {
            var account = await _service.GetByIdAsync(id, ct);
            return account is null ? NotFound() : Ok(ApiResponse<EmailAccountResponse>.Ok(account));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<EmailAccountResponse>>> GetAll(CancellationToken ct)
        {
            var accounts = await _service.GetAllAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<EmailAccountResponse>>.Ok(accounts));
        }

        [HttpPost]
        public async Task<ActionResult<EmailAccountResponse>> Create([FromBody] CreateEmailAccountRequest request, CancellationToken ct)
        {
            var account = await _service.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = account.Id }, ApiResponse<EmailAccountResponse>.Ok(account));
        }

        [HttpPatch("{id:int}/rename")]
        public async Task<IActionResult> Rename(int id, [FromBody] RenameEmailAccountRequest request, CancellationToken ct)
        {
            await _service.RenameAsync(id, request.DisplayName, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/config")]
        public async Task<IActionResult> UpdateConfig(
            int id,
            [FromBody] UpdateEmailAccountConfigRequest request,
            CancellationToken ct)
        {
            await _service.UpdateConfigAsync(id, request.Config, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }


        [HttpGet("google/connect")]
        [AllowAnonymous]
        public IActionResult ConnectGoogle()
        {
            var state = Guid.NewGuid().ToString("N");

            var url = _googleOAuthService.GetAuthorizationUrl(state);

            return Redirect(url);
        }

        [HttpGet("google/callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string code,  string state, CancellationToken ct)
        {
            var token = await _googleOAuthService.ExchangeCodeForTokenAsync(code, ct);
            var user = await _googleOAuthService.GetUserInfoAsync(token.AccessToken, ct);
            var config = new Dictionary<string, string>
            {
                 ["clientId"] = _googleOptions.ClientID!,

                 ["refreshToken"] = token.RefreshToken ?? "",

                 ["accessToken"] = token.AccessToken,

                 ["expiresAt"] = DateTime.UtcNow.AddSeconds(token.ExpiresIn).ToString("O")
            };

            var request = new CreateEmailAccountRequest
            {
                ProviderType = EmailProviderType.Gmail,

                EmailAddress = user.Email,

                DisplayName = user.Name,

                Config = config
            };
            await _service.CreateAsync(request, ct);
            return Redirect(_UrlsOptions.Frontend + "email/emailaccounts");
        }
    }
}
