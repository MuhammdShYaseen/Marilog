using Marilog.OCR.Worker.Extensions;

namespace Marilog.OCR.Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            });

            builder.Services.AddSearchablePdfGenerator(opts =>
            {
                // opts.TessDataPath = @"C:\MyApp\tessdata";
            });

            builder.Services.AddHostedService<Worker>();

            IHost host = builder.Build();

            await host.RunAsync();
        }
    }
}
