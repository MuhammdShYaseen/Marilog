using Marilog.Application.Interfaces.UserIdentity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Marilog.Infrastructure.Services.UserIdenty
{
    public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

        public Guid? UserId =>
            Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        public string? UserName => Principal?.Identity?.Name;
    }
}
