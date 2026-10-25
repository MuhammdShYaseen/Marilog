
using Marilog.Contracts.Common;
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
        private readonly IHostEnvironment _env;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
            {
                Log.Debug("Request cancelled by client | Path: {Path}", context.Request.Path);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    Log.Error(ex, "Exception after response started | Path: {Path}", context.Request.Path);
                    throw;
                }
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var errorId = Guid.NewGuid().ToString("N")[..8];

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

            var isDevelopment = _env.IsDevelopment();
            var isClientError = (int)statusCode < 500;

            var response = new ApiErrorResponse
            {
                Success = false,
                Message = isClientError ? GetUserMessage(ex) : $"An unexpected error occurred. Ref: {errorId}",
                ErrorCode = statusCode.ToString(),
                DeveloperMessage = isDevelopment ? ex.ToString() : null,
                ErrorSummary = isDevelopment ? safeErrorDetails : null,
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

        private static string GetUserMessage(Exception ex) => ex switch
        {
            ArgumentException { ParamName: not null } ae
                => ae.Message.Replace($" (Parameter '{ae.ParamName}')", string.Empty),
            _ => ex.Message
        };
    }
}
