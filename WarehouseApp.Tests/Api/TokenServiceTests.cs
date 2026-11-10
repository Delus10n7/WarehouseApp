using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WarehouseApp.Api.Services;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Unit-тесты <see cref="TokenService"/>.
/// </summary>
/// <remarks>
/// Проверяют:
/// <list type="bullet">
///   <item><description>что токен генерируется и содержит корректные claims (sub, jti);</description></item>
///   <item><description>что issuer, audience, срок жизни соответствуют конфигурации;</description></item>
///   <item><description>что подпись валидна (токен проходит проверку с тем же ключом);</description></item>
///   <item><description>что отсутствие обязательных параметров в конфигурации приводит к
///   <see cref="InvalidOperationException"/>.</description></item>
/// </list>
/// </remarks>
public class TokenServiceTests
{
    /// <summary>Валидный ключ (56 байт — с запасом больше минимума для HS256 в 32 байта).</summary>
    private const string ValidKey = "THIS_IS_A_VERY_LONG_SECRET_KEY_FOR_JWT_SIGNING_1234567890";

    /// <summary>Издатель токена (claim iss).</summary>
    private const string ValidIssuer = "WarehouseApp";

    /// <summary>Получатель токена (claim aud).</summary>
    private const string ValidAudience = "WarehouseAppClient";

    /// <summary>
    /// Собирает <see cref="IConfiguration"/> из словаря «ключ-значение» в памяти.
    /// </summary>
    /// <param name="values">Словарь конфигурации.</param>
    /// <returns>Готовая конфигурация.</returns>
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    /// <summary>
    /// Собирает «валидную» конфигурацию (все обязательные поля + срок жизни).
    /// </summary>
    /// <param name="expiresHours">Срок жизни токена в часах.</param>
    /// <returns>Готовая конфигурация.</returns>
    private static IConfiguration BuildValidConfiguration(int expiresHours = 24)
    {
        return BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = ValidKey,
            ["Jwt:Issuer"] = ValidIssuer,
            ["Jwt:Audience"] = ValidAudience,
            ["Jwt:ExpiresHours"] = expiresHours.ToString()
        });
    }

    /// <summary>
    /// Проверяет, что <c>GenerateToken()</c> возвращает непустую строку формата JWT (xxx.yyy.zzz).
    /// </summary>
    [Fact]
    public void GenerateToken_Returns_NonEmpty_WellFormed_Token()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration());

        // Act
        var token = service.GenerateToken();

        // Assert — три части, разделённые точками
        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3);
    }

    /// <summary>
    /// Проверяет, что в токене есть claim <c>sub = "warehouse-client"</c>.
    /// </summary>
    [Fact]
    public void GenerateToken_Contains_Sub_Claim()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration());
        var handler = new JwtSecurityTokenHandler();

        // Act
        var token = service.GenerateToken();
        var jwt = handler.ReadJwtToken(token);

        // Assert — sub зафиксирован, т.к. пользователей нет
        jwt.Claims.Should().ContainSingle(c =>
            c.Type == JwtRegisteredClaimNames.Sub && c.Value == "warehouse-client");
    }

    /// <summary>
    /// Проверяет, что в токене есть уникальный claim <c>jti</c> (валидный GUID).
    /// </summary>
    [Fact]
    public void GenerateToken_Contains_Jti_Claim_As_Guid()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration());
        var handler = new JwtSecurityTokenHandler();

        // Act
        var token = service.GenerateToken();
        var jwt = handler.ReadJwtToken(token);
        var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

        // Assert
        jti.Should().NotBeNull();
        Guid.TryParse(jti!.Value, out _).Should().BeTrue();
    }

    /// <summary>
    /// Проверяет, что <c>jti</c> уникален между двумя вызовами <c>GenerateToken()</c>.
    /// </summary>
    [Fact]
    public void GenerateToken_Jti_Is_Unique_Between_Calls()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration());
        var handler = new JwtSecurityTokenHandler();

        // Act
        var jwt1 = handler.ReadJwtToken(service.GenerateToken());
        var jwt2 = handler.ReadJwtToken(service.GenerateToken());

        var jti1 = jwt1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwt2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        // Assert
        jti1.Should().NotBe(jti2);
    }

    /// <summary>
    /// Проверяет, что issuer и audience в токене соответствуют конфигурации.
    /// </summary>
    [Fact]
    public void GenerateToken_Contains_Correct_Issuer_And_Audience()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration());
        var handler = new JwtSecurityTokenHandler();

        // Act
        var jwt = handler.ReadJwtToken(service.GenerateToken());

        // Assert
        jwt.Issuer.Should().Be(ValidIssuer);
        jwt.Audiences.Should().ContainSingle().Which.Should().Be(ValidAudience);
    }

    /// <summary>
    /// Проверяет, что срок жизни соответствует <c>Jwt:ExpiresHours</c> (24 часа по умолчанию).
    /// </summary>
    [Fact]
    public void GenerateToken_Expires_In_Configured_Hours()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration(expiresHours: 24));
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;

        // Act
        var jwt = handler.ReadJwtToken(service.GenerateToken());

        // Assert — exp ≈ now + 24 часа (допуск 1 минута на выполнение теста)
        jwt.ValidTo.Should().BeCloseTo(now.AddHours(24), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Проверяет, что нестандартный <c>Jwt:ExpiresHours</c> (например, 1 час) учитывается.
    /// </summary>
    [Fact]
    public void GenerateToken_Respects_Custom_ExpiresHours()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration(expiresHours: 1));
        var handler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;

        // Act
        var jwt = handler.ReadJwtToken(service.GenerateToken());

        // Assert
        jwt.ValidTo.Should().BeCloseTo(now.AddHours(1), TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Проверяет, что токен подписан корректно: с тем же ключом он успешно
    /// проходит валидацию через <see cref="JwtSecurityTokenHandler.ValidateToken"/>.
    /// </summary>
    [Fact]
    public void GenerateToken_Signature_Is_Valid_For_Configured_Key()
    {
        // Arrange
        var service = new TokenService(BuildValidConfiguration());
        var handler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,       // проверяем именно подпись, не срок
            ValidateIssuerSigningKey = true,
            ValidIssuer = ValidIssuer,
            ValidAudience = ValidAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ValidKey))
        };

        // Act
        var token = service.GenerateToken();
        var action = () => handler.ValidateToken(token, validationParameters, out _);

        // Assert
        action.Should().NotThrow();
    }

    /// <summary>
    /// Проверяет, что отсутствие <c>Jwt:Key</c> приводит к <see cref="InvalidOperationException"/>
    /// с упоминанием параметра в сообщении.
    /// </summary>
    [Fact]
    public void GenerateToken_Throws_When_Key_Missing()
    {
        // Arrange
        var config = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = ValidIssuer,
            ["Jwt:Audience"] = ValidAudience
            // Key отсутствует
        });
        var service = new TokenService(config);

        // Act
        var act = () => service.GenerateToken();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Jwt:Key*");
    }

    /// <summary>
    /// Проверяет, что отсутствие <c>Jwt:Issuer</c> приводит к <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void GenerateToken_Throws_When_Issuer_Missing()
    {
        // Arrange
        var config = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = ValidKey,
            ["Jwt:Audience"] = ValidAudience
            // Issuer отсутствует
        });
        var service = new TokenService(config);

        // Act
        var act = () => service.GenerateToken();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Jwt:Issuer*");
    }

    /// <summary>
    /// Проверяет, что отсутствие <c>Jwt:Audience</c> приводит к <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void GenerateToken_Throws_When_Audience_Missing()
    {
        // Arrange
        var config = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = ValidKey,
            ["Jwt:Issuer"] = ValidIssuer
            // Audience отсутствует
        });
        var service = new TokenService(config);

        // Act
        var act = () => service.GenerateToken();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Jwt:Audience*");
    }
}
