using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для работы со справочниками (категории и единицы измерения).
/// Предоставляет API-эндпоинты для получения, создания и обновления записей.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly WarehouseDbContext _context;

    /// <summary>
    /// Инициализирует контроллер справочников.
    /// </summary>
    /// <param name="context">Контекст БД для получения данных.</param>
    public LookupsController(WarehouseDbContext context) => _context = context;

    /// <summary>
    /// Получает список всех категорий.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="Category"/>.</returns>
    /// <response code="200">Успешно возвращает список категорий (может быть пустым).</response>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        => Ok(await _context.Categories.AsNoTracking().ToListAsync());

    /// <summary>
    /// Создает новую категорию.
    /// </summary>
    /// <param name="category">Объект категория, передаваемый в теле запроса (JSON).</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="200">Категория успешно создана.</response>
    /// <response code="400">Переданы некорректные данные.</response>
    [HttpPost("categories")]
    public async Task<ActionResult> CreateCategory([FromBody] Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return Ok(category);
    }

    /// <summary>
    /// Получает список всех единиц измерения.
    /// </summary>
    /// <returns>Коллекция объектов <see cref="UnitOfMeasure"/>.</returns>
    /// <response code="200">Успешно возвращает список единиц измерения (может быть пустым).</response>
    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<UnitOfMeasure>>> GetUnits()
        => Ok(await _context.UnitsOfMeasure.AsNoTracking().ToListAsync());

    /// <summary>
    /// Создает новую единицу измерения.
    /// </summary>
    /// <param name="unit">Объект единица измерения, передаваемый в теле запроса (JSON).</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="200">Единица измерения успешно создана.</response>
    /// <response code="400">Переданы некорректные данные.</response>
    [HttpPost("units")]
    public async Task<ActionResult> CreateUnit([FromBody] UnitOfMeasure unit)
    {
        _context.UnitsOfMeasure.Add(unit);
        await _context.SaveChangesAsync();
        return Ok(unit);
    }
}
