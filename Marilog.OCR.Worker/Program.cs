using Marilog.OCR.Worker;
using Marilog.OCR.Worker.Abstractions;
using Marilog.OCR.Worker.Extensions;
using Marilog.OCR.Worker.Filters;
using Marilog.OCR.Worker.Infrastructure;
using Marilog.OCR.Worker.Options;
using Marilog.OCR.Worker.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Marilog.OCR.Worker;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<InternalApiKeyFilter>();
        // ── Logging ──
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Services.Configure<UrlsOptions>(builder.Configuration.GetSection("Urls"));
        // ── OCR Pipeline ──
        builder.Services.AddSearchablePdfGenerator(opts =>
        {
            // opts.TessDataPath = @"C:\MyApp\tessdata";
        });

        //-----CompressService--------------
        builder.Services.AddSingleton<IPdfCompressionService, GhostscriptPdfCompressionService>();
        builder.Services.AddSingleton<IPdfDirectTextExtractor, PdfDirectTextExtractor>();

        #pragma warning disable CA1416 // known deployment targets are Windows/Linux/macOS
        builder.Services.AddSingleton<IThumbnailGenerator, PdfThumbnailGenerator>();
        #pragma warning restore CA1416

        // ── OCR Queue ──
        builder.Services.AddSingleton<OcrQueue>();

        // ── Background Worker ──
        builder.Services.AddHostedService<Worker>();
        var _basePath = builder.Configuration["FileStorage:BasePath"]
           ?? throw new InvalidOperationException("FileStorage:BasePath not configured.");
        builder.Services.AddHttpClient<ICallBackService, CallBackService>((sp, client) =>
        {
            var urls = sp.GetRequiredService<IOptions<UrlsOptions>>().Value;
            
            var apiKey = builder.Configuration["InternalApiKey"]
                ?? throw new InvalidOperationException("InternalApiKey is not configured.");
            client.BaseAddress = new Uri(urls.Presentation);
            client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);
        });

        var app = builder.Build();

        // ── Health Check ──
        app.MapGet("/health", () => Results.Ok(new
        {
            Status = "Healthy",
            Service = "Marilog.OCR.Worker",
            Time = DateTime.UtcNow
        }));

        // ── OCR Request ──
        app.MapPost("/api/ocr/process", async (OcrRequest request, OcrQueue queue, ILogger<Program> logger, CancellationToken ct) =>
        {
            

            if (string.IsNullOrWhiteSpace(request.FilePath))
                return Results.BadRequest("FilePath is required");

            request.FilePath = Path.Combine(_basePath, request.FilePath);

            if (!File.Exists(request.FilePath))
                return Results.NotFound($"File not found: {request.FilePath}");

            logger.LogInformation( "OCR request received | DocumentId: {Id} | File: {File}", request.DocumentId, Path.GetFileName(request.FilePath) );

            await queue.EnqueueAsync(request, ct);

            return Results.Accepted(value: new
            {
                Message = "OCR job queued",
                DocumentId = request.DocumentId
            });
        }).AddEndpointFilter<InternalApiKeyFilter>();

        await app.RunAsync();
    }
}