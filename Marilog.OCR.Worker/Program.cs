using Marilog.OCR.Worker;
using Marilog.OCR.Worker.Extensions;
using Marilog.OCR.Worker.Options;
using Microsoft.Extensions.Logging;

namespace Marilog.OCR.Worker;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ── Logging ──
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Services.Configure<UrlsOptions>(builder.Configuration.GetSection("Urls"));
        // ── OCR Pipeline ──
        builder.Services.AddSearchablePdfGenerator(opts =>
        {
            // opts.TessDataPath = @"C:\MyApp\tessdata";
        });

        // ── OCR Queue ──
        builder.Services.AddSingleton<OcrQueue>();

        // ── Background Worker ──
        builder.Services.AddHostedService<Worker>();

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
            if (request.DocumentId <= 0)
                return Results.BadRequest("Invalid DocumentId");

            if (string.IsNullOrWhiteSpace(request.FilePath))
                return Results.BadRequest("FilePath is required");

            if (!File.Exists(request.FilePath))
                return Results.NotFound($"File not found: {request.FilePath}");

            logger.LogInformation( "OCR request received | DocumentId: {Id} | File: {File}", request.DocumentId, Path.GetFileName(request.FilePath) );

            await queue.EnqueueAsync(request, ct);

            return Results.Accepted(value: new
            {
                Message = "OCR job queued",
                DocumentId = request.DocumentId
            });
        });

        await app.RunAsync();
    }
}