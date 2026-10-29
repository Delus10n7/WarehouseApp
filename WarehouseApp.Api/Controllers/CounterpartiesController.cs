using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos;
using WarehouseApp.Application.Dtos.CounterpartyDtos;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для управления контрагентами.
/// Предоставляет REST-эндпоинты для получения, создания, обновления и удаления записей.
/// </summary>
[ApiController]
[Route("api/[controller]")] // URL /api/Counterparties
public class CounterpartiesController : ControllerBase
{
    private readonly ICounterpartyRepository _repository;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера контрагентов.
    /// </summary>
    /// <param name="repository">Репозиторий для доступа к данным контрагентов (из слоя Application).</param>
    public CounterpartiesController(ICounterpartyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Получает список всех контрагентов.
    /// </summary>
    /// <param name="ct">Токен отмены операции (прокидывается из HTTP-запроса).</param>
    /// <returns>Коллекция объектов <see cref="CounterpartyDto"/>.</returns>
    /// <response code="200">Успешно возвращает список контрагентов (может быть пустым).</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CounterpartyDto>>> GetAll(CancellationToken ct)
    {
        var entities = await _repository.GetAllAsync(ct);
        var dtos = entities.Select(e => e.ToDto()).ToList();
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
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CounterpartyDto>> GetById(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();
        return Ok(entity.ToDto());
    }

    /// <summary>
    /// Создает нового контрагента.
    /// </summary>
    /// <param name="dto">Данные для создания контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный объект <see cref="CounterpartyDto"/> с присвоенным идентификатором.</returns>
    /// <response code="201">Контрагент успешно создан. Заголовок Location содержит ссылку на GetById.</response>
    /// <response code="400">Поле 'Name' обязательно и не может быть пустым.</response>
    [HttpPost]
    public async Task<ActionResult<CounterpartyDto>> Create(
        [FromBody] CounterpartyRequestDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Поле 'Name' обязательно." });

        var entity = dto.ToEntity();
        await _repository.AddAsync(entity, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    /// <summary>
    /// Обновляет данные существующего контрагента.
    /// </summary>
    /// <param name="id">Идентификатор контрагента, которого нужно обновить.</param>
    /// <param name="dto">Обновленные данные контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="204">Данные успешно обновлены (нет тела ответа).</response>
    /// <response code="404">Контрагент с указанным идентификатором не найден.</response>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] CounterpartyRequestDto dto, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        dto.ApplyTo(existing);
        await _repository.UpdateAsync(existing, ct);
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
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _repository.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
