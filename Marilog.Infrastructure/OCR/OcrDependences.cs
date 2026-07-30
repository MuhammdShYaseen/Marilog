using Marilog.Application.Interfaces.OCR;
using Marilog.Infrastructure.BackgroundServices;
using Marilog.Infrastructure.OCR.Implementations;
using Marilog.Infrastructure.OCR.Interfaces;
using Marilog.Infrastructure.OCR.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marilog.Infrastructure.OCR
{
    public static class OcrDependences
    {
        public static IServiceCollection AddOcr(this IServiceCollection services, IConfiguration configuration)
        {
            //-----CompressService--------------
            services.AddSingleton<IPdfCompressionService, GhostscriptPdfCompressionService>();
            services.AddSingleton<IPdfDirectTextExtractor, PdfDirectTextExtractor>();
            services.AddSingleton<ISearchablePdfService, OcrMyPdfService>();

            #pragma warning disable CA1416 // known deployment targets are Windows/Linux/macOS
            services.AddSingleton<IThumbnailGenerator, PdfThumbnailGenerator>();
            #pragma warning restore CA1416

            // ── OCR Queue ──
            services.AddSingleton<OcrQueue>();

            // ── Background Worker ──
            services.AddHostedService<OcrWorker>();
            var _basePath = configuration["FileStorage:BasePath"]
               ?? throw new InvalidOperationException("FileStorage:BasePath not configured.");

            services.AddScoped<ICallBackService, CallBackService>();

            services.AddScoped<IOcrStarter>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<OcrStarter>>();
                var queue = sp.GetRequiredService<OcrQueue>();
                return new OcrStarter(_basePath, logger, queue);
            });
            return services;
        }
    }
}
