using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для управления складскими документами (накладными).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IWareDocumentRepository _repository;

    /// <summary>
    /// Инициализирует контроллер документов.
    /// </summary>
    /// <param name="repository">Репозиторий для доступа к данным документов.</param>
    public DocumentsController(IWareDocumentRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Получает список всех документов с подгруженными контрагентами и товарами.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Коллекция объектов <see cref="WareDocumentDto"/>.</returns>
    /// <response code="200">Успешно возвращает список документов.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WareDocumentDto>>> GetAll(CancellationToken ct)
    {
        var entities = await _repository.GetAllWithDetailsAsync(ct);
        return Ok(entities.Select(e => e.ToDto()).ToList());
    }

    /// <summary>
    /// Получает документ по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект <see cref="WareDocumentDto"/>.</returns>
    /// <response code="200">Документ найден.</response>
    /// <response code="404">Документ не найден.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WareDocumentDto>> GetById(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();
        return Ok(entity.ToDto());
    }

    /// <summary>
    /// Создаёт новый документ и обновляет остатки товаров.
    /// </summary>
    /// <param name="dto">Данные для создания документа.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный документ.</returns>
    /// <response code="201">Документ успешно создан.</response>
    /// <response code="400">Некорректные данные (пустые поля, нет товаров).</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WareDocumentDto>> Create(
        [FromBody] WareDocumentCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Number) || dto.CounterpartyId == 0 || dto.Items.Count == 0)
            return BadRequest(new { message = "Заполните номер, контрагента и добавьте товар." });

        try
        {
            var entity = dto.ToEntity();
            await _repository.AddAsync(entity, ct);

            var created = await _repository.GetByIdAsync(entity.Id, ct);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, created!.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            // Перехватываем ошибку нехватки товара
            return BadRequest(new ProblemDetails
            {
                Title = "Недостаточно товара",
                Detail = ex.Message,
                Status = 400
            });
        }
    }

    /// <summary>
    /// Обновляет данные существующего документа и корректирует остатки.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <param name="dto">Обновлённые данные.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат операции.</returns>
    /// <response code="204">Документ обновлён.</response>
    /// <response code="404">Документ не найден.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id, [FromBody] WareDocumentCreateDto dto, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        try
        {
            dto.ApplyTo(existing);
            await _repository.UpdateAsync(existing, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Удаляет документ и отменяет его влияние на остатки товаров.
    /// </summary>
    /// <param name="id">Идентификатор документа.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат операции.</returns>
    /// <response code="204">Документ удалён.</response>
    /// <response code="404">Документ не найден.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _repository.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
