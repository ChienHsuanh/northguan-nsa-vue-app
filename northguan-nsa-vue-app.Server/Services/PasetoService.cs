using System.Security.Claims;
using System.Text.Json;
using northguan_nsa_vue_app.Server.Models;
using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;

namespace northguan_nsa_vue_app.Server.Services
{
    public interface IPasetoService
    {
        string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
        ClaimsPrincipal? ValidateToken(string token);
        bool IsTokenExpired(string token);
    }

    public class PasetoService : IPasetoService
    {
        private readonly IConfiguration _configuration;
        private readonly PasetoSymmetricKey _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public PasetoService(IConfiguration configuration)
        {
            _configuration = configuration;

            // 從配置中獲取PASETO設定
            var pasetoSettings = _configuration.GetSection("PasetoSettings");
            var secretKeyString = pasetoSettings["SecretKey"] ??
                                throw new InvalidOperationException("PASETO SecretKey is not configured");

            // 生成PASETO v4.local 密鑰
            var keyBytes = Convert.FromHexString(secretKeyString.Length >= 64 ? secretKeyString[..64] : secretKeyString.PadRight(64, '0'));
            _secretKey = new PasetoSymmetricKey(keyBytes, new Paseto.Protocol.Version4());

            _issuer = pasetoSettings["Issuer"] ?? "https://localhost";
            _audience = pasetoSettings["Audience"] ?? "https://localhost";
            _expirationMinutes = pasetoSettings.GetValue<int>("ExpirationInMinutes", 60);
        }

        public string GenerateToken(ApplicationUser user, IEnumerable<string> roles)
        {
            var now = DateTimeOffset.Now;
            var expiration = now.AddMinutes(_expirationMinutes);

            var token = new PasetoBuilder()
                .Use(ProtocolVersion.V4, Purpose.Local)
                .WithKey(_secretKey)
                .Subject(user.Id)
                .AddClaim("name", user.Name)
                .AddClaim("email", user.Email ?? "")
                .AddClaim("username", user.UserName ?? "")
                .AddClaim("isReadOnly", user.ReadOnly.ToString())
                .AddClaim("roles", string.Join(",", roles))
                .Issuer(_issuer)
                .Audience(_audience)
                .IssuedAt(now)
                .NotBefore(now)
                .Expiration(expiration)
                .Encode();

            return token;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var result = new PasetoBuilder()
                    .Use(ProtocolVersion.V4, Purpose.Local)
                    .WithKey(_secretKey)
                    .Decode(token);

                if (result == null || !result.IsValid)
                    return null;

                // 解析payload為JSON
                var payloadJson = result.Paseto.RawPayload;
                var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);

                if (payload == null)
                    return null;

                // 檢查過期時間
                if (payload.TryGetValue("exp", out var expElement))
                {
                    DateTimeOffset expiration;
                    if (expElement.ValueKind == JsonValueKind.String)
                    {
                        if (!DateTimeOffset.TryParse(expElement.GetString(), out expiration))
                            return null;
                    }
                    else if (expElement.ValueKind == JsonValueKind.Number)
                    {
                        var exp = expElement.GetInt64();
                        expiration = DateTimeOffset.FromUnixTimeSeconds(exp);
                    }
                    else
                    {
                        return null;
                    }

                    if (expiration <= DateTimeOffset.Now)
                        return null;
                }

                // 檢查生效時間
                if (payload.TryGetValue("nbf", out var nbfElement))
                {
                    DateTimeOffset notBefore;
                    if (nbfElement.ValueKind == JsonValueKind.String)
                    {
                        if (!DateTimeOffset.TryParse(nbfElement.GetString(), out notBefore))
                            return null;
                        notBefore.ToUniversalTime();
                    }
                    else if (nbfElement.ValueKind == JsonValueKind.Number)
                    {
                        var nbf = nbfElement.GetInt64();
                        notBefore = DateTimeOffset.FromUnixTimeSeconds(nbf);
                    }
                    else
                    {
                        return null;
                    }

                    if (notBefore > DateTimeOffset.Now)
                        return null;
                }

                // 驗證發行者和受眾
                if (payload.TryGetValue("iss", out var issElement) && issElement.GetString() != _issuer)
                    return null;

                if (payload.TryGetValue("aud", out var audElement) && audElement.GetString() != _audience)
                    return null;

                // 構建Claims
                var claims = new List<Claim>();

                if (payload.TryGetValue("sub", out var subElement))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, subElement.GetString() ?? ""));

                if (payload.TryGetValue("name", out var nameElement))
                    claims.Add(new Claim(ClaimTypes.Name, nameElement.GetString() ?? ""));

                if (payload.TryGetValue("email", out var emailElement))
                    claims.Add(new Claim(ClaimTypes.Email, emailElement.GetString() ?? ""));

                if (payload.TryGetValue("username", out var usernameElement))
                    claims.Add(new Claim("username", usernameElement.GetString() ?? ""));

                if (payload.TryGetValue("isReadOnly", out var readOnlyElement))
                    claims.Add(new Claim("isReadOnly", readOnlyElement.GetString() ?? "false"));

                // 添加角色Claims
                if (payload.TryGetValue("roles", out var rolesElement))
                {
                    var rolesString = rolesElement.GetString();
                    if (!string.IsNullOrEmpty(rolesString))
                    {
                        var rolesList = rolesString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                        foreach (var role in rolesList)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
                        }
                    }
                }

                var identity = new ClaimsIdentity(claims, "PASETO");
                return new ClaimsPrincipal(identity);
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var result = new PasetoBuilder()
                    .Use(ProtocolVersion.V4, Purpose.Local)
                    .WithKey(_secretKey)
                    .Decode(token);

                if (result == null || !result.IsValid)
                    return true;

                // 解析payload為JSON
                var payloadJson = result.Paseto.RawPayload;
                var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);

                if (payload == null)
                    return true;

                if (payload.TryGetValue("exp", out var expElement))
                {
                    var exp = expElement.GetInt64();
                    var expiration = DateTimeOffset.FromUnixTimeSeconds(exp);
                    return expiration <= DateTimeOffset.Now;
                }

                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}