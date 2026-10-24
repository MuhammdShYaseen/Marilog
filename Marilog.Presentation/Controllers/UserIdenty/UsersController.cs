using Marilog.Contracts.DTOs.Requests.UserIdenty;
using Marilog.Contracts.DTOs.Responses;
using Marilog.Contracts.Interfaces.Services.UserIdenty;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Kernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marilog.Presentation.Controllers.UserIdenty
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = AuthPolicies.SuperAdmin)]
    public sealed class UsersController(IUserManagementService userService, IRoleManagementService roleService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken ct)
            => Ok(await userService.GetAllAsync(ct));

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken ct)
            => ToActionResult(await userService.CreateAsync(request, ct));

        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
            => ToActionResult(await userService.ActivateAsync(id, ct));

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
            => ToActionResult(await userService.DeactivateAsync(id, ct));

        [HttpPost("{id:guid}/grant-super-admin")]
        public async Task<IActionResult> GrantSuperAdmin(Guid id, CancellationToken ct)
            => ToActionResult(await userService.GrantSuperAdminAsync(id, ct));

        [HttpPost("{id:guid}/revoke-super-admin")]
        public async Task<IActionResult> RevokeSuperAdmin(Guid id, CancellationToken ct)
            => ToActionResult(await userService.RevokeSuperAdminAsync(id, ct));

        [HttpPost("{id:guid}/reset-password")]
        public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordRequest request, CancellationToken ct)
            => ToActionResult(await userService.ResetPasswordAsync(id, request, ct));


        [HttpPut("{id:guid}/roles")]
        public async Task<IActionResult> SetRoles(Guid id, [FromBody] IReadOnlyList<int> roleIds, CancellationToken ct)
        => await roleService.SetUserRolesAsync(id, roleIds, ct) switch
        {
            RoleManagementResult.Success => NoContent(),
            RoleManagementResult.NotFound => Problem(
                detail: "User not found.",
                statusCode: StatusCodes.Status404NotFound),
            RoleManagementResult.InvalidRole => Problem(
                detail: "One or more roles do not exist.",
                statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(
                detail: "The operation failed.",
                statusCode: StatusCodes.Status400BadRequest)
    };

        private IActionResult ToActionResult(UserManagementResult result) => result switch
        {
            UserManagementResult.Success => NoContent(),
            UserManagementResult.NotFound => Problem(
                detail: "User not found.",
                statusCode: StatusCodes.Status404NotFound),
            UserManagementResult.UserNameTaken => Problem(
                detail: "This username is already taken.",
                statusCode: StatusCodes.Status409Conflict),
            UserManagementResult.InvalidPassword => Problem(
                detail: "The password does not meet the password requirements.",
                statusCode: StatusCodes.Status400BadRequest),
            UserManagementResult.LastSuperAdmin => Problem(
                detail: "This is the last active super admin. Grant another user super admin first.",
                statusCode: StatusCodes.Status409Conflict),
            UserManagementResult.CannotModifySelf => Problem(
                detail: "You cannot do this to your own account.",
                statusCode: StatusCodes.Status400BadRequest),
            _ => Problem(
                detail: "The operation failed.",
                statusCode: StatusCodes.Status400BadRequest)
        };
    }
}