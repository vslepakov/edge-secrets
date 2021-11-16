using Microsoft.Extensions.Primitives;

namespace SecretDeliveryApp;

internal class APIKeyCheckMiddleware
{
    public const string API_KEY_HEADER_NAME = "X-API-KEY";
    private readonly RequestDelegate _next;

    public APIKeyCheckMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out StringValues value))
        {
            var apiKey = value;
            var validApiKey = Environment.GetEnvironmentVariable(API_KEY_HEADER_NAME);

            if (validApiKey != null && apiKey == validApiKey)
            {
                await _next(httpContext);
                return;
            }
        }

        httpContext.Response.StatusCode = 403;
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class APIKeyCheckMiddlewareExtensions
{
    public static IApplicationBuilder UseAPIKeyCheckMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<APIKeyCheckMiddleware>();
    }
}
