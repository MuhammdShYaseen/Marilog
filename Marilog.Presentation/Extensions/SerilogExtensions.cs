using Serilog;
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
                var logPath = ctx.Configuration["Logging:FilePath"]
                              ?? "Logs/marilog-.log";

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
                        path: logPath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Information)

                    // ── Separate error log ────────────────────────────────────────
                    .WriteTo.File(
                        path: "Logs/errors-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 90,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Error);
            });
        }
    }
}
