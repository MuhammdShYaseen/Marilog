namespace Marilog.OCR.Worker.Filters
{

    // InternalApiKeyFilter.cs — في OCR Worker
    public sealed class InternalApiKeyFilter : IEndpointFilter
    {
        private readonly string _apiKey;

        public InternalApiKeyFilter(IConfiguration configuration)
        {
            _apiKey = configuration["InternalApiKey"]
                ?? throw new InvalidOperationException("InternalApiKey is not configured.");
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("X-Internal-Api-Key", out var key) || key != _apiKey)
                return Results.Unauthorized();

            return await next(context);
        }
    }
}
