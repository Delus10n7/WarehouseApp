using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WarehouseApp.Api.Controllers;
using WarehouseApp.Api.Services;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Unit-тесты <see cref="AuthController"/>.
/// </summary>
/// <remarks>
/// Проверяют:
/// <list type="bullet">
///   <item><description>что <c>POST /api/auth/token</c> возвращает 200 OK;</description></item>
///   <item><description>что в ответе объект <see cref="TokenResponse"/> с токеном от <see cref="ITokenService"/>;</description></item>
///   <item><description>что сервис вызывается ровно один раз;</description></item>
///   <item><description>что исключение из сервиса не перехватывается (пробрасывается выше —
///   глобальный exception handler вернёт 500).</description></item>
/// </list>
/// </remarks>
public class AuthControllerTests
{
    /// <summary>Мок сервиса генерации токенов.</summary>
    private readonly ITokenService _tokenServiceMock;

    /// <summary>Тестируемый контроллер.</summary>
    private readonly AuthController _controller;

    /// <summary>Инициализирует мок и контроллер.</summary>
    public AuthControllerTests()
    {
        _tokenServiceMock = Substitute.For<ITokenService>();
        _controller = new AuthController(_tokenServiceMock);
    }

    /// <summary>
    /// Проверяет, что <c>GetToken</c> возвращает 200 OK.
    /// </summary>
    [Fact]
    public void GetToken_Returns_Ok_Result()
    {
        // Arrange
        _tokenServiceMock.GenerateToken().Returns("test-token");

        // Act
        var result = _controller.GetToken();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    /// <summary>
    /// Проверяет, что в ответе объект <see cref="TokenResponse"/> с токеном,
    /// который вернул <see cref="ITokenService"/>.
    /// </summary>
    [Fact]
    public void GetToken_Returns_TokenResponse_With_Service_Token()
    {
        // Arrange
        const string expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.sig";
        _tokenServiceMock.GenerateToken().Returns(expectedToken);

        // Act
        var result = _controller.GetToken();

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<TokenResponse>().Subject;
        response.Token.Should().Be(expectedToken);
    }

    /// <summary>
    /// Проверяет, что <see cref="ITokenService.GenerateToken"/> вызывается ровно один раз.
    /// </summary>
    [Fact]
    public void GetToken_Calls_TokenService_Once()
    {
        // Arrange
        _tokenServiceMock.GenerateToken().Returns("token");

        // Act
        _controller.GetToken();

        // Assert
        _tokenServiceMock.Received(1).GenerateToken();
    }

    /// <summary>
    /// Проверяет, что исключение из <see cref="ITokenService.GenerateToken"/>
    /// пробрасывается наверх (его должен перехватить глобальный exception handler).
    /// </summary>
    [Fact]
    public void GetToken_Propagates_Service_Exception()
    {
        // Arrange — имитируем ошибку конфигурации
        _tokenServiceMock.GenerateToken()
            .Returns<string>(_ => throw new InvalidOperationException("Jwt:Key is not configured."));

        // Act
        var act = () => _controller.GetToken();

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Jwt:Key*");
    }
}
