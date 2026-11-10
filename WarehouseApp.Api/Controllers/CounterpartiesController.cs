using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos;
using WarehouseApp.Application.Dtos.CounterpartyDtos;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для управления контрагентами.
/// Предоставляет REST-эндпоинты для получения, создания, обновления и удаления записей.
/// </summary>
[ApiController]                              // Включает автоматическую валидацию модели и связывание параметров
[Route("api/[controller]")]                  // Маршрут: /api/Counterparties (имя контроллера без "Controller")
[Authorize]
public class CounterpartiesController : ControllerBase
{
    /// <summary>
    /// Репозиторий для доступа к данным контрагентов (из слоя Application).
    /// </summary>
    private readonly ICounterpartyRepository _repository;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера контрагентов.
    /// </summary>
    /// <param name="repository">Репозиторий для доступа к данным контрагентов.</param>
    public CounterpartiesController(ICounterpartyRepository repository)
    {
        _repository = repository;            // Сохраняем зависимость для дальнейшего использования
    }

    /// <summary>
    /// Получает список всех контрагентов.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Коллекция объектов <see cref="CounterpartyDto"/>.</returns>
    /// <response code="200">Успешно возвращает список контрагентов (может быть пустым).</response>
    [HttpGet]                                // HTTP GET /api/Counterparties
    [ProducesResponseType(StatusCodes.Status200OK)]  // Документируем возможный ответ 200
    public async Task<ActionResult<IEnumerable<CounterpartyDto>>> GetAll(CancellationToken ct)
    {
        // Асинхронно запрашиваем все сущности из репозитория
        var entities = await _repository.GetAllAsync(ct);

        // Преобразуем каждую сущность в DTO с помощью extension-метода ToDto
        var dtos = entities.Select(e => e.ToDto()).ToList();

        // Возвращаем 200 OK с коллекцией DTO
        return Ok(dtos);
    }

    /// <summary>
    /// Получает контрагента по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект <see cref="CounterpartyDto"/>.</returns>
    /// <response code="200">Успешно возвращает контрагента.</response>
    /// <response code="404">Контрагент с указанным идентификатором не найден.</response>
    [HttpGet("{id:int}")]                    // HTTP GET /api/Counterparties/{id}
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CounterpartyDto>> GetById(int id, CancellationToken ct)
    {
        // Пытаемся найти сущность по id
        var entity = await _repository.GetByIdAsync(id, ct);

        // Если не нашли — возвращаем 404 Not Found
        if (entity is null) return NotFound();

        // Иначе — 200 OK с DTO
        return Ok(entity.ToDto());
    }

    /// <summary>
    /// Создаёт нового контрагента.
    /// </summary>
    /// <param name="dto">Данные для создания контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный объект <see cref="CounterpartyDto"/> с присвоенным идентификатором.</returns>
    /// <response code="201">Контрагент успешно создан. Заголовок Location содержит ссылку на GetById.</response>
    /// <response code="400">Поле 'Name' обязательно и не может быть пустым.</response>
    [HttpPost]                               // HTTP POST /api/Counterparties
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CounterpartyDto>> Create(
        [FromBody] CounterpartyRequestDto dto, CancellationToken ct)
    {
        // Простейшая валидация: Name не должно быть пустым
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Поле 'Name' обязательно." });

        // Преобразуем DTO в сущность (ToEntity обрезает пробелы, нормализует опциональные поля)
        var entity = dto.ToEntity();

        // Сохраняем сущность в БД через репозиторий
        await _repository.AddAsync(entity, ct);

        // Возвращаем 201 Created, Location: /api/Counterparties/{id}, тело — DTO созданной сущности
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    /// <summary>
    /// Обновляет данные существующего контрагента.
    /// </summary>
    /// <param name="id">Идентификатор контрагента, которого нужно обновить.</param>
    /// <param name="dto">Обновлённые данные контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="204">Данные успешно обновлены (нет тела ответа).</response>
    /// <response code="404">Контрагент с указанным идентификатором не найден.</response>
    [HttpPut("{id:int}")]                    // HTTP PUT /api/Counterparties/{id}
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] CounterpartyRequestDto dto, CancellationToken ct)
    {
        // Ищем существующую сущность
        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        // Применяем изменения из DTO к сущности (ApplyTo меняет поля, не трогая Id и навигации)
        dto.ApplyTo(existing);

        // Сохраняем изменения
        await _repository.UpdateAsync(existing, ct);

        // 204 No Content — успешно, без тела
        return NoContent();
    }

    /// <summary>
    /// Удаляет контрагента по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор контрагента, которого нужно удалить.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="204">Данные успешно удалены (нет тела ответа).</response>
    /// <response code="404">Контрагент с указанным идентификатором не найден.</response>
    [HttpDelete("{id:int}")]                 // HTTP DELETE /api/Counterparties/{id}
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        // Пытаемся удалить; DeleteAsync возвращает true, если запись была найдена и удалена
        var deleted = await _repository.DeleteAsync(id, ct);

        // Если не нашли — 404, иначе 204
        return deleted ? NoContent() : NotFound();
    }
}
