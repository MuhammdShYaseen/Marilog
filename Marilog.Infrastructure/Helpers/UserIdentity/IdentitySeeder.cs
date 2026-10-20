using Marilog.Infrastructure.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Marilog.Infrastructure.Helpers.UserIdentity
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services, IConfiguration configuration)
        {
            var userManager = services.GetRequiredService<UserManager<User>>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(IdentitySeeder));

            if (await userManager.Users.AnyAsync())
                return;

            var userName = configuration["AdminSeed:UserName"];
            var fullName = configuration["AdminSeed:FullName"];
            var password = configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("No users exist and AdminSeed is not configured. No admin was created.");
                return;
            }

            var admin = User.Create(userName, string.IsNullOrWhiteSpace(fullName) ? "Administrator" : fullName);
            admin.GrantSuperAdmin();

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Failed to create admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));

            logger.LogInformation("Initial admin {UserName} created.", userName);
        }
    }
}
