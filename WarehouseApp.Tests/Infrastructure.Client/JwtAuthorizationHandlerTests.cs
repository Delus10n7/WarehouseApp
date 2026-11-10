using System.Net;
using System.Text;
using FluentAssertions;
using WarehouseApp.Infrastructure.Client.Services;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure.Client;

/// <summary>
/// Unit-тесты <see cref="JwtAuthorizationHandler"/>.
/// </summary>
/// <remarks>
/// Проверяют:
/// <list type="bullet">
///   <item><description>что обработчик запрашивает токен у <see cref="AuthClient"/> и добавляет
///   заголовок <c>Authorization: Bearer {token}</c> к исходящему запросу;</description></item>
///   <item><description>что токен кэшируется — при нескольких запросах <see cref="AuthClient"/>
///   вызывается только один раз;</description></item>
///   <item><description>что запрос доходит до внутреннего обработчика без изменений URL и метода;</description></item>
///   <item><description>что ошибка получения токена пробрасывается наружу.</description></item>
/// </list>
/// </remarks>
public class JwtAuthorizationHandlerTests
{
    /// <summary>
    /// Создаёт <see cref="AuthClient"/> с подменным HTTP-обработчиком, который
    /// считает количество обращений к эндпоинту токена и всегда возвращает валидный токен.
    /// </summary>
    /// <param name="tokenRequestCount">Счётчик обращений (по ссылке).</param>
    /// <param name="tokenValue">Значение токена, которое должен вернуть сервер.</param>
    /// <returns>Готовый <see cref="AuthClient"/>.</returns>
    private static AuthClient CreateAuthClient(
        out TokenRequestCounter counter, string tokenValue = "test-token-123")
    {
        // Замыкание-счётчик для проверки кэширования
        var localCounter = new TokenRequestCounter();
        counter = localCounter;

        var tokenHandler = new MockHttpMessageHandler(_ =>
        {
            localCounter.Increment();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    $"{{\"token\":\"{tokenValue}\"}}",
                    Encoding.UTF8,
                    "application/json")
            };
        });

        var http = new HttpClient(tokenHandler) { BaseAddress = new Uri("http://localhost/") };
        return new AuthClient(http);
    }

    /// <summary>
    /// Проверяет, что <see cref="JwtAuthorizationHandler.SendAsync"/> добавляет
    /// заголовок <c>Authorization: Bearer {token}</c> к запросу.
    /// </summary>
    [Fact]
    public async Task SendAsync_Adds_Bearer_Header()
    {
        // Arrange — токен-эндпоинт и «внутренний» эндпоинт
        var authClient = CreateAuthClient(out _);
        var innerHandler = new MockHttpMessageHandler("[]", HttpStatusCode.OK);

        var jwtHandler = new JwtAuthorizationHandler(authClient)
        {
            InnerHandler = innerHandler
        };
        var http = new HttpClient(jwtHandler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        await http.GetAsync("api/Counterparties");

        // Assert — заголовок передан во внутренний обработчик
        innerHandler.LastRequest.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
        innerHandler.LastRequest!.Headers.Authorization!.Scheme.Should().Be("Bearer");
        innerHandler.LastRequest!.Headers.Authorization!.Parameter.Should().Be("test-token-123");
    }

    /// <summary>
    /// Проверяет, что токен запрашивается у <see cref="AuthClient"/> только <b>один раз</b>
    /// и кэшируется для последующих запросов.
    /// </summary>
    [Fact]
    public async Task SendAsync_Caches_Token_Between_Requests()
    {
        // Arrange
        var authClient = CreateAuthClient(out var tokenCounter);
        var innerHandler = new MockHttpMessageHandler("[]", HttpStatusCode.OK);

        var jwtHandler = new JwtAuthorizationHandler(authClient)
        {
            InnerHandler = innerHandler
        };
        var http = new HttpClient(jwtHandler) { BaseAddress = new Uri("http://localhost/") };

        // Act — три запроса подряд
        await http.GetAsync("api/Counterparties");
        await http.GetAsync("api/Products");
        await http.GetAsync("api/Lookups/categories");

        // Assert — токен запрошен только один раз
        tokenCounter.Count.Should().Be(1);
    }

    /// <summary>
    /// Проверяет, что URL и HTTP-метод исходного запроса не меняются —
    /// обработчик только добавляет заголовок, а не пересобирает запрос.
    /// </summary>
    [Fact]
    public async Task SendAsync_Preserves_Original_Method_And_Uri()
    {
        // Arrange
        var authClient = CreateAuthClient(out _);
        var innerHandler = new MockHttpMessageHandler("{}", HttpStatusCode.OK);

        var jwtHandler = new JwtAuthorizationHandler(authClient)
        {
            InnerHandler = innerHandler
        };
        var http = new HttpClient(jwtHandler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        await http.PostAsync("api/Products", new StringContent("{}", Encoding.UTF8, "application/json"));

        // Assert
        innerHandler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        innerHandler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Products");
    }

    /// <summary>
    /// Проверяет, что заголовок <c>Authorization</c> ставится на каждый запрос
    /// даже при кэшированном токене (повторные запросы тоже должны быть авторизованы).
    /// </summary>
    [Fact]
    public async Task SendAsync_Adds_Header_On_Every_Request()
    {
        // Arrange
        var authClient = CreateAuthClient(out _);
        var innerHandler = new MockHttpMessageHandler("[]", HttpStatusCode.OK);

        var jwtHandler = new JwtAuthorizationHandler(authClient)
        {
            InnerHandler = innerHandler
        };
        var http = new HttpClient(jwtHandler) { BaseAddress = new Uri("http://localhost/") };

        // Act + Assert — проверяем, что заголовок есть после каждого запроса
        await http.GetAsync("api/Counterparties");
        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();

        await http.GetAsync("api/Products");
        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();

        await http.GetAsync("api/Lookups/categories");
        innerHandler.LastRequest!.Headers.Authorization.Should().NotBeNull();
    }

    /// <summary>
    /// Проверяет, что если <see cref="AuthClient"/> не смог получить токен,
    /// исключение пробрасывается наружу и запрос к защищённому эндпоинту не уходит.
    /// </summary>
    [Fact]
    public async Task SendAsync_Propagates_Token_Fetch_Exception()
    {
        // Arrange — токен-эндпоинт возвращает 500
        var tokenHandler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var tokenHttp = new HttpClient(tokenHandler) { BaseAddress = new Uri("http://localhost/") };
        var authClient = new AuthClient(tokenHttp);

        var innerHandler = new MockHttpMessageHandler("[]", HttpStatusCode.OK);
        var jwtHandler = new JwtAuthorizationHandler(authClient)
        {
            InnerHandler = innerHandler
        };
        var http = new HttpClient(jwtHandler) { BaseAddress = new Uri("http://localhost/") };

        // Act
        var act = async () => await http.GetAsync("api/Counterparties");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();

        // Внутренний обработчик НЕ должен был получить запрос
        innerHandler.LastRequest.Should().BeNull();
    }

    /// <summary>
    /// Вспомогательный счётчик для проверки кэширования токена.
    /// Используется в <see cref="SendAsync_Caches_Token_Between_Requests"/>.
    /// </summary>
    public sealed class TokenRequestCounter
    {
        /// <summary>Текущее значение счётчика.</summary>
        public int Count { get; private set; }

        /// <summary>Увеличивает счётчик на единицу.</summary>
        public void Increment() => Count++;
    }
}
