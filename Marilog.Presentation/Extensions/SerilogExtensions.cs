using Serilog;
using Serilog.Core;
using Serilog.Events;
namespace Marilog.Presentation.Extensions
{
    public static class SerilogExtensions
    {
        public static void ConfigureSerilog(this IHostBuilder host)
        {
            host.UseSerilog((ctx, services, config) =>
            {
                var appName = ctx.HostingEnvironment.ApplicationName;
                var env = ctx.HostingEnvironment.EnvironmentName;
                var logDir = ctx.Configuration["LogFiles:Directory"] ?? "Logs";

                config
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", appName)
                    .Enrich.WithProperty("Environment", env)

                    // ── Console (dev only) ────────────────────────────────────────
                    .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Information)

                    // ── Rolling file — one file per day, keep 30 days ─────────────
                    .WriteTo.File(
                       path: Path.Combine(logDir, "marilog-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Information)

                    // ── Separate error log ────────────────────────────────────────
                    .WriteTo.File(
                        path: Path.Combine(logDir, "errors-.log"),
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 90,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Error);
            });
        }
    }
}
