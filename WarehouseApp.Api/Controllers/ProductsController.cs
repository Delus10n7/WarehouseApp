using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos;
using WarehouseApp.Application.Dtos.ProductDtos;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для управления товарами.
/// Предоставляет REST-эндпоинты для получения, создания, обновления и удаления записей.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    /// <summary>
    /// Инициализирует контроллер товаров.
    /// </summary>
    /// <param name="repository">Репозиторий для доступа к данным товаров (из слоя Application).</param>
    public ProductsController(IProductRepository repository) => _repository = repository;

    /// <summary>
    /// Получает список всех товаров с подгруженными категориями и единицами измерения.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Коллекция объектов <see cref="ProductDto"/>.</returns>
    /// <response code="200">Успешно возвращает список товаров (может быть пустым).</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll(CancellationToken ct)
    {
        var entities = await _repository.GetAllWithDetailsAsync(ct);
        return Ok(entities.Select(e => e.ToDto()).ToList());
    }

    /// <summary>
    /// Получает товар по идентификатору (с подгруженными категорией и единицей измерения).
    /// </summary>
    /// <param name="id">Идентификатор товара.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект <see cref="ProductDto"/>.</returns>
    /// <response code="200">Успешно возвращает товар.</response>
    /// <response code="404">Товар с указанным идентификатором не найден.</response>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
    {
        var entity = await _repository.GetByIdAsync(id, ct);
        if (entity is null) return NotFound();
        return Ok(entity.ToDto());
    }

    /// <summary>
    /// Создает новый товар.
    /// </summary>
    /// <param name="dto">Данные для создания товара.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный объект <see cref="ProductDto"/> с присвоенным идентификатором и подгруженными навигациями.</returns>
    /// <response code="201">Товар успешно создан. Заголовок Location содержит ссылку на GetById.</response>
    /// <response code="400">Поля 'Name' и 'Article' обязательны.</response>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] ProductRequestDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Article))
            return BadRequest(new { message = "Поля 'Name' и 'Article' обязательны." });

        var entity = dto.ToEntity();
        await _repository.AddAsync(entity, ct);

        // После создания догружаем навигации, чтобы DTO содержал CategoryName/UnitOfMeasureName.
        var created = await _repository.GetByIdAsync(entity.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, created!.ToDto());
    }

    /// <summary>
    /// Обновляет данные существующего товара.
    /// </summary>
    /// <param name="id">Идентификатор товара, которого нужно обновить.</param>
    /// <param name="dto">Обновленные данные товара.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="204">Данные успешно обновлены (нет тела ответа).</response>
    /// <response code="404">Товар с указанным идентификатором не найден.</response>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] ProductRequestDto dto, CancellationToken ct)
    {
        var existing = await _repository.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        dto.ApplyTo(existing);
        await _repository.UpdateAsync(existing, ct);
        return NoContent();
    }

    /// <summary>
    /// Удаляет товар.
    /// </summary>
    /// <param name="id">Идентификатор товара, которого нужно удалить.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="204">Товар успешно удалён.</response>
    /// <response code="404">Товар с указанным идентификатором не найден.</response>
    /// <remarks>
    /// Удаление товара, который используется в строках документов,
    /// приведёт к <c>DbUpdateException</c> (FK с <c>DeleteBehavior.Restrict</c>) → 500.
    /// </remarks>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _repository.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
