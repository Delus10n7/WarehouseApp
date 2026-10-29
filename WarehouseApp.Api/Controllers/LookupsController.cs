using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для работы со справочниками (категории и единицы измерения).
/// Предоставляет REST-эндпоинты для получения и создания записей.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly IWarehouseDbContext _context;

    /// <summary>
    /// Инициализирует контроллер справочников.
    /// </summary>
    /// <param name="context">Абстракция контекста БД (из Application).</param>
    public LookupsController(IWarehouseDbContext context) => _context = context;

    /// <summary>
    /// Получает список всех категорий.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Коллекция объектов <see cref="CategoryDto"/>.</returns>
    /// <response code="200">Успешно возвращает список категорий (может быть пустым).</response>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(CancellationToken ct)
    {
        var entities = await _context.Categories.AsNoTracking().ToListAsync(ct);
        return Ok(entities.Select(e => e.ToDto()).ToList());
    }

    /// <summary>
    /// Создает новую категорию.
    /// </summary>
    /// <param name="dto">Данные для создания категории.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный объект <see cref="CategoryDto"/> с присвоенным идентификатором.</returns>
    /// <response code="200">Категория успешно создана (возвращается 200 для совместимости с клиентом).</response>
    /// <response code="400">Поле 'Name' обязательно и не может быть пустым.</response>
    [HttpPost("categories")]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CategoryCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Поле 'Name' обязательно." });

        var entity = dto.ToEntity();
        _context.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Ok(entity.ToDto());
    }

    /// <summary>
    /// Получает список всех единиц измерения.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Коллекция объектов <see cref="UnitOfMeasureDto"/>.</returns>
    /// <response code="200">Успешно возвращает список единиц измерения (может быть пустым).</response>
    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<UnitOfMeasureDto>>> GetUnits(CancellationToken ct)
    {
        var entities = await _context.UnitsOfMeasure.AsNoTracking().ToListAsync(ct);
        return Ok(entities.Select(e => e.ToDto()).ToList());
    }

    /// <summary>
    /// Создает новую единицу измерения.
    /// </summary>
    /// <param name="dto">Данные для создания единицы измерения.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный объект <see cref="UnitOfMeasureDto"/> с присвоенным идентификатором.</returns>
    /// <response code="200">Единица измерения успешно создана (возвращается 200 для совместимости с клиентом).</response>
    /// <response code="400">Поле 'Name' обязательно и не может быть пустым.</response>
    [HttpPost("units")]
    public async Task<ActionResult<UnitOfMeasureDto>> CreateUnit(
        [FromBody] UnitOfMeasureCreateDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Поле 'Name' обязательно." });

        var entity = dto.ToEntity();
        _context.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Ok(entity.ToDto());
    }
}
