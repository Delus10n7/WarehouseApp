using System.Net;
using System.Text;
using FluentAssertions;
using WarehouseApp.Infrastructure.Client.Services;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure.Client;

/// <summary>
/// Unit-тесты <see cref="AuthClient"/>.
/// </summary>
/// <remarks>
/// Проверяют:
/// <list type="bullet">
///   <item><description>что запрос идёт методом POST на <c>api/auth/token</c>;</description></item>
///   <item><description>что токен корректно извлекается из JSON-ответа;</description></item>
///   <item><description>что 4xx/5xx → <see cref="HttpRequestException"/>;</description></item>
///   <item><description>что пустой/невалидный ответ → <see cref="InvalidOperationException"/>.</description></item>
/// </list>
/// </remarks>
public class AuthClientTests
{
    /// <summary>
    /// Создаёт <see cref="AuthClient"/> с подменным HTTP-обработчиком.
    /// </summary>
    /// <param name="handler">Обработчик, возвращающий подготовленный ответ.</param>
    /// <returns>Готовый <see cref="AuthClient"/>.</returns>
    private static AuthClient CreateClient(HttpMessageHandler handler)
    {
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new AuthClient(http);
    }

    /// <summary>
    /// Проверяет, что запрос уходит методом POST на <c>api/auth/token</c>.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_Sends_Post_To_Correct_Url()
    {
        // Arrange
        const string json = "{\"token\":\"abc\"}";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var client = CreateClient(handler);

        // Act
        await client.GetTokenAsync();

        // Assert
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/auth/token");
    }

    /// <summary>
    /// Проверяет, что токен извлекается из JSON-поля <c>token</c>.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_Returns_Token_From_Json()
    {
        // Arrange
        const string json = "{\"token\":\"eyJhbGciOi.abc.xyz\"}";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var client = CreateClient(handler);

        // Act
        var token = await client.GetTokenAsync();

        // Assert
        token.Should().Be("eyJhbGciOi.abc.xyz");
    }

    /// <summary>
    /// Проверяет, что при 401 (Unauthorized) выбрасывается <see cref="HttpRequestException"/>.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_Throws_On_Unauthorized()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("", HttpStatusCode.Unauthorized);
        var client = CreateClient(handler);

        // Act
        var act = async () => await client.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    /// <summary>
    /// Проверяет, что при 500 (Internal Server Error) выбрасывается <see cref="HttpRequestException"/>.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_Throws_On_Server_Error()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var client = CreateClient(handler);

        // Act
        var act = async () => await client.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    /// <summary>
    /// Проверяет, что пустой JSON-объект (<c>{}</c>) — нет поля <c>token</c> —
    /// приводит к <see cref="InvalidOperationException"/> с понятным сообщением.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_Throws_When_Token_Field_Missing()
    {
        // Arrange — валидный JSON, но без поля token
        var handler = new MockHttpMessageHandler("{}", HttpStatusCode.OK);
        var client = CreateClient(handler);

        // Act
        var act = async () => await client.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*пустой токен*");
    }

    /// <summary>
    /// Проверяет, что тело <c>null</c> (буквально строка "null") — 
    /// <c>ReadFromJsonAsync</c> вернёт <c>null</c> — приводит к <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_Throws_When_Body_Is_Null_Literal()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("null", HttpStatusCode.OK);
        var client = CreateClient(handler);

        // Act
        var act = async () => await client.GetTokenAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
