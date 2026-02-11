using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using northguan_nsa_vue_app.Server.Services;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace northguan_nsa_vue_app.Server.Authentication
{
    public class PasetoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IPasetoService _pasetoService;

        public PasetoAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IPasetoService pasetoService)
            : base(options, logger, encoder)
        {
            _pasetoService = pasetoService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var token = ExtractTokenFromRequest();

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            try
            {
                var principal = _pasetoService.ValidateToken(token);
                if (principal == null)
                {
                    return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
                }

                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error validating PASETO token");
                return Task.FromResult(AuthenticateResult.Fail("Token validation failed"));
            }
        }

        private string? ExtractTokenFromRequest()
        {
            // 從Authorization header中提取token
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return authHeader["Bearer ".Length..].Trim();
            }

            // 也可以從查詢參數中提取token（可選）
            var tokenFromQuery = Request.Query["token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(tokenFromQuery))
            {
                return tokenFromQuery;
            }

            return null;
        }
    }
}