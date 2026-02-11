using northguan_nsa_vue_app.Server.Services;
using System.Security.Claims;

namespace northguan_nsa_vue_app.Server.Middleware
{
    public class PasetoAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPasetoService _pasetoService;
        private readonly ILogger<PasetoAuthenticationMiddleware> _logger;

        public PasetoAuthenticationMiddleware(
            RequestDelegate next, 
            IPasetoService pasetoService,
            ILogger<PasetoAuthenticationMiddleware> logger)
        {
            _next = next;
            _pasetoService = pasetoService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = ExtractTokenFromRequest(context);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var principal = _pasetoService.ValidateToken(token);
                    if (principal != null)
                    {
                        context.User = principal;
                        _logger.LogDebug("PASETO token validated successfully for user: {UserId}", 
                            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid PASETO token provided");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating PASETO token");
                }
            }

            await _next(context);
        }

        private string? ExtractTokenFromRequest(HttpContext context)
        {
            // 從Authorization header中提取token
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader["Bearer ".Length..].Trim();
            }

            // 也可以從查詢參數中提取token（可選）
            var tokenFromQuery = context.Request.Query["token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(tokenFromQuery))
            {
                return tokenFromQuery;
            }

            return null;
        }
    }

    public static class PasetoAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UsePasetoAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PasetoAuthenticationMiddleware>();
        }
    }
}