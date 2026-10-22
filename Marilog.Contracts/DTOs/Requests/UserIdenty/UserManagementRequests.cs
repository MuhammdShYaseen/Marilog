

namespace Marilog.Contracts.DTOs.Requests.UserIdenty
{
      public sealed record CreateUserRequest(string UserName, string FullName, string? Email, string Password);
      public sealed record ResetPasswordRequest(string NewPassword);
    
}
