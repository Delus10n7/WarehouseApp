namespace WarehouseApp.Api.Services;

/// <summary>
/// Сервис для генерации JWT-токенов.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Генерирует новый JWT-токен.
    /// </summary>
    /// <returns>Строка с подписанным JWT.</returns>
    /// <exception cref="InvalidOperationException">
    /// Если в конфигурации не заданы <c>Jwt:Key</c>, <c>Jwt:Issuer</c>
    /// или <c>Jwt:Audience</c>.
    /// </exception>
    string GenerateToken();
}
