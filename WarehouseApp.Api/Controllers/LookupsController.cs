using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для работы со справочниками (категории и единицы измерения).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly WarehouseDbContext _context;

    /// <summary>
    /// Инициализирует контроллер справочников.
    /// </summary>
    public LookupsController(WarehouseDbContext context) => _context = context;

    /// <summary>
    /// Получает список всех категорий.
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        => Ok(await _context.Categories.AsNoTracking().ToListAsync());

    /// <summary>
    /// Создает новую категорию.
    /// </summary>
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
    [HttpGet("units")]
    public async Task<ActionResult<IEnumerable<UnitOfMeasure>>> GetUnits()
        => Ok(await _context.UnitsOfMeasure.AsNoTracking().ToListAsync());

    /// <summary>
    /// Создает новую единицу измерения.
    /// </summary>
    [HttpPost("units")]
    public async Task<ActionResult> CreateUnit([FromBody] UnitOfMeasure unit)
    {
        _context.UnitsOfMeasure.Add(unit);
        await _context.SaveChangesAsync();
        return Ok(unit);
    }
}
