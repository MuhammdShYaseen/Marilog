using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.UserIdenty;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.UserIdenty
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await authService.LoginAsync(request);

            return result switch
            {
                LoginResult.Success => NoContent(),
                LoginResult.LockedOut => Problem(
                    detail: "Account temporarily locked. Try again later.",
                    statusCode: StatusCodes.Status401Unauthorized),
                _ => Problem(
                    detail: "Invalid username or password.",
                    statusCode: StatusCodes.Status401Unauthorized)
            };
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await authService.LogoutAsync();
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<CurrentUserResponse>> Me()
        {
            var user = await authService.GetCurrentUserAsync();
            return user is null ? Unauthorized() : Ok(user);
        }
    }
}
