using System.Net.Http.Json;    // ReadFromJsonAsync
using System.Text.Json;        // JsonSerializerOptions (неявно)

namespace WarehouseApp.Infrastructure.Client.Services;

/// <summary>
/// HTTP-клиент для получения JWT-токена с сервера.
/// </summary>
public class AuthClient
{
    /// <summary>Настроенный <see cref="HttpClient"/> с базовым адресом API.</summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует клиент.
    /// </summary>
    /// <param name="httpClient">
    /// HttpClient с базовым адресом API. Регистрируется через
    /// <c>AddHttpClient&lt;AuthClient&gt;()</c> в <c>DependencyInjection</c>.
    /// </param>
    public AuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Запрашивает новый JWT-токен у сервера.
    /// </summary>
    /// <param name="ct">Токен отмены (позволяет прервать HTTP-запрос).</param>
    /// <returns>Строка с подписанным JWT.</returns>
    /// <exception cref="HttpRequestException">
    /// Если сервер вернул 4xx/5xx.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Если сервер вернул 200, но тело ответа пустое или не содержит <c>token</c>.
    /// </exception>
    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        // POST без тела — эндпоинт не требует параметров.
        var response = await _httpClient.PostAsync("api/auth/token", null, ct);

        // Бросает HttpRequestException, если статус не 2xx.
        response.EnsureSuccessStatusCode();

        // Читаем JSON: { "token": "eyJhbGciOi..." }
        var result = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);

        return result?.Token
            ?? throw new InvalidOperationException("Сервер вернул пустой токен.");
    }

    /// <summary>
    /// DTO ответа от <c>POST /api/auth/token</c>.
    /// </summary>
    /// <param name="Token">Подписанный JWT.</param>
    private sealed record TokenResponse(string Token);
}
