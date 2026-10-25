
using Marilog.Application;
using Marilog.Contracts.Options;
using Marilog.Infrastructure;
using Marilog.Infrastructure.DataAccess.ContextDb;
using Marilog.Infrastructure.Helpers.UserIdentity;
using Marilog.Infrastructure.Models.Identity;
using Marilog.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
namespace Marilog.Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.ConfigureSerilog(); 
            builder.Services
                .AddInfrastructure(builder.Configuration)
                .AddApplication()
                .AddApiServices();
            builder.Services
                .AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddIdentityCookies();

            builder.Services
                .AddIdentityCore<User>(options =>
                {
                    options.Password.RequiredLength = 4;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                })
                .AddEntityFrameworkStores<MarilogContext>()
                .AddSignInManager();
            builder.Services.Configure<SecurityStampValidatorOptions>(o => o.ValidationInterval = TimeSpan.FromMinutes(10));
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "Marilog.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
            });

            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build())
                .AddPolicy(AuthPolicies.SuperAdmin, p => p
                .RequireAuthenticatedUser()
                .AddRequirements(new SuperAdminRequirement()));

            builder.Services.AddScoped<IAuthorizationHandler, SuperAdminHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            builder.Services.AddScoped<UserPermissionReader>();
            var dataProtection = builder.Services.AddDataProtection()
                    .SetApplicationName("Marilog");

            var keysPath = builder.Configuration["DataProtection:KeysPath"];
            if (!string.IsNullOrWhiteSpace(keysPath))
                dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
            builder.Services.AddControllers(options => options.Conventions.Add(new PermissionConvention()));
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowClient", policy =>
                {
                    if (builder.Environment.IsDevelopment())
                    {
                        // Development: اقبل أي origin
                        policy
                            .SetIsOriginAllowed(_ => true)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                    else
                    {
                        // Production: محدد
                        var urls = builder.Configuration
                            .GetSection("Urls")
                            .Get<UrlsOptions>()
                            ?? throw new InvalidOperationException("Urls configuration is missing.");

                        var apiKeys = builder.Configuration
                                .GetSection("InternalApiKeys")
                                .Get<InternalApiKeysOptions>()
                                ?? throw new InvalidOperationException("InternalApiKeys configuration is missing.");


                        policy
                            .WithOrigins(urls.Frontend.TrimEnd('/'))
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                });
            });

            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MarilogContext>();
                AuditEntityTypes.ValidateAll(db);
                db.Database.Migrate();
                IdentitySeeder.SeedAdminAsync(scope.ServiceProvider, builder.Configuration).GetAwaiter().GetResult();


            }
            app.UseCors("AllowClient");
            app.UseErrorHandler();
            app.UseSerilogRequestLogging(opts =>
            {
                opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000}ms";
            });
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
