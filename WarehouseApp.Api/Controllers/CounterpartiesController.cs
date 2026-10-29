using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для управления контрагентами.
/// Предоставляет API-эндпоинты для получения, создания и обновления записей.
/// </summary>
[ApiController]
[Route("api/[controller]")] // URL /api/Counterparties
public class CounterpartiesController : ControllerBase
{
    private readonly ICounterpartyRepository _repository;

    /// <summary>
    /// Инициализирует новый экземпляр контроллера контрагентов.
    /// </summary>
    /// <param name="repository">Репозиторий для доступа к данным контрагентов.</param>
    public CounterpartiesController(ICounterpartyRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Получает список всех контрагентов.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="Counterparty"/>.</returns>
    /// <response code="200">Успешно возвращает список контрагентов (может быть пустым).</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Counterparty>>> GetAll()
    {
        var counterparties = await _repository.GetAllAsync();
        return Ok(counterparties);
    }

    /// <summary>
    /// Создает нового контрагента.
    /// </summary>
    /// <param name="counterparty">Объект контрагента, передаваемый в теле запроса (JSON).</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="201">Контрагент успешно создан.</response>
    /// <response code="400">Переданы некорректные данные.</response>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Counterparty counterparty)
    {
        await _repository.AddAsync(counterparty);
        return Created();
    }

    /// <summary>
    /// Обновляет данные существующего контрагента.
    /// </summary>
    /// <param name="id">Идентификатор контрагента, которого нужно обновить.</param>
    /// <param name="counterparty">Обновленные данные контрагента.</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="204">Данные успешно обновлены (нет содержимого для возврата).</response>
    /// <response code="400">Идентификатор в URL не совпадает с идентификатором в теле запроса.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Counterparty counterparty)
    {
        if (id != counterparty.Id) return BadRequest();
        await _repository.UpdateAsync(counterparty);
        return NoContent();
    }
}

