namespace Marilog.Presentation.Controllers.Attributes
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public sealed class InternalApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string HeaderName = "X-Internal-Key";

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext
                .RequestServices
                .GetRequiredService<IConfiguration>();

            var expectedKey = configuration["InternalServices:OcrApiKey"];

            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            if (!string.Equals(expectedKey, providedKey, StringComparison.Ordinal))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }
}
