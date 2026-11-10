using System.Net.Http.Headers;    // AuthenticationHeaderValue

namespace WarehouseApp.Infrastructure.Client.Services;

/// <summary>
/// <see cref="DelegatingHandler"/>, добавляющий JWT-токен в заголовок
/// <c>Authorization</c> каждого исходящего запроса.
/// </summary>
public class JwtAuthorizationHandler : DelegatingHandler
{
    /// <summary>Клиент для получения токена.</summary>
    private readonly AuthClient _authClient;

    /// <summary>Кэш токена. <c>null</c>, пока токен не запрошен.</summary>
    private string? _cachedToken;

    /// <summary>
    /// Инициализирует обработчик.
    /// </summary>
    /// <param name="authClient">Клиент для получения токена (из DI).</param>
    public JwtAuthorizationHandler(AuthClient authClient)
    {
        _authClient = authClient;
    }

    /// <summary>
    /// Добавляет заголовок <c>Authorization: Bearer {token}</c> и отправляет запрос.
    /// </summary>
    /// <param name="request">Исходящий HTTP-запрос.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>HTTP-ответ от сервера.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Ленивая инициализация: запрашиваем токен только при первом вызове.
        // AuthClient использует ОТДЕЛЬНЫЙ HttpClient (без этого обработчика),
        // поэтому рекурсии нет.
        _cachedToken ??= await _authClient.GetTokenAsync(cancellationToken);

        // Подставляем заголовок. AuthenticationHeaderValue сам добавит "Bearer ".
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _cachedToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
