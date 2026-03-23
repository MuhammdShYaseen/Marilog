using Marilog.Presentation.Common;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Marilog.Presentation.Middlewares.ErrorHandler
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType ??= "application/json";

            var errorId = Guid.NewGuid().ToString();

            var statusCode = ex switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                ArgumentException => HttpStatusCode.BadRequest,
                KeyNotFoundException => HttpStatusCode.NotFound,
                BadHttpRequestException => HttpStatusCode.BadRequest,
                InvalidOperationException => HttpStatusCode.BadRequest,
                HubException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            var safeErrorDetails = BuildSafeErrorDetails(ex);
            Log.Error(ex, "Exception handled | ErrorId: {ErrorId} | Path: {Path} | User: {User} | Summary: {@Summary}",
                                                     errorId,context.Request.Path, context.User?.Identity?.Name ?? "Anonymous", safeErrorDetails);

           #if DEBUG
            var developerMessage = ex.ToString();
           #else
            string? developerMessage = null;
           #endif

            var response = new ApiErrorResponse
            {
                Success = false,
                Message = "An unexpected error occurred.",
                ErrorCode = statusCode.ToString(),
                DeveloperMessage = developerMessage,
                ErrorSummary = safeErrorDetails,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private static SafeErrorDetails BuildSafeErrorDetails(Exception ex)
        {
            var stackFrame = new StackTrace(ex, true)
                                .GetFrames()?
                                .FirstOrDefault(f => f.GetFileLineNumber() > 0);

            return new SafeErrorDetails
            {
                ExceptionType = ex.GetType().Name,
                Message = "An internal error occurred.",
                Location = stackFrame is not null ? $"{stackFrame.GetMethod()?.DeclaringType?.Name}.{stackFrame.GetMethod()?.Name}" : "Unknown"
            };
        }
    }
}
