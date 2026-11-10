using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace WarehouseApp.Api.Services;

/// <summary>
/// Реализация <see cref="ITokenService"/> на алгоритме HMAC SHA-256.
/// </summary>
public class TokenService : ITokenService
{
    /// <summary>Конфигурация приложения (для доступа к секции <c>Jwt</c>).</summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Инициализирует сервис с конфигурацией.
    /// </summary>
    /// <param name="configuration">Конфигурация приложения.</param>
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public string GenerateToken()
    {
        // 1. Читаем обязательные параметры. Если чего-то нет — падаем с понятным сообщением.
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");
        var jwtIssuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
        var jwtAudience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

        // 2. Создаём ключ подписи (HMAC SHA-256 требует ≥ 32 байт ключа).
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        // 3. Креденшелы для подписи — алгоритм HMAC SHA-256.
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 4. Минимальный набор claims:
        //    - sub  — фиксированный идентификатор клиента (ролей и пользователей нет);
        //    - jti  — уникальный идентификатор токена (для аудита и потенциального blacklist).
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "warehouse-client"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // 5. Срок жизни токена (по умолчанию 24 часа).
        var expiresHours = _configuration.GetValue<int>("Jwt:ExpiresHours", 24);

        // 6. Собираем токен.
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiresHours),
            signingCredentials: credentials);

        // 7. Сериализуем в строку формата xxx.yyy.zzz.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
