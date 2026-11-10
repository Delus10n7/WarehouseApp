using Microsoft.AspNetCore.Authorization;   // [AllowAnonymous] — эндпоинт без токена
using Microsoft.AspNetCore.Mvc;             // ControllerBase, ActionResult, [ApiController]
using WarehouseApp.Api.Services;            // ITokenService

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер выдачи JWT-токенов.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    /// <summary>Сервис генерации JWT.</summary>
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Инициализирует контроллер.
    /// </summary>
    /// <param name="tokenService">Сервис генерации токенов (из DI).</param>
    public AuthController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    /// <summary>
    /// Выдаёт новый JWT-токен.
    /// </summary>
    /// <returns>Объект <see cref="TokenResponse"/> с подписанным JWT.</returns>
    /// <response code="200">Токен успешно сгенерирован.</response>
    /// <response code="500">В конфигурации не заданы параметры <c>Jwt</c>.</response>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public ActionResult<TokenResponse> GetToken()
    {
        var token = _tokenService.GenerateToken();
        return Ok(new TokenResponse(token));
    }
}

/// <summary>
/// DTO ответа с JWT-токеном.
/// </summary>
/// <param name="Token">Строка с подписанным JWT.</param>
public record TokenResponse(string Token);
