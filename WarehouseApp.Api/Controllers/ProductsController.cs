using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для управления товарами.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    /// <summary>
    /// Инициализирует контроллер товаров.
    /// </summary>
    public ProductsController(IProductRepository repository) => _repository = repository;

    /// <summary>
    /// Получает список всех товаров с включением связанных данных (категории и ед. измерения).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _repository.GetAllWithDetailsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Создает новый товар.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Product product)
    {
        await _repository.AddAsync(product);
        return Ok();
    }

    /// <summary>
    /// Обновляет данные существующего товара.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        await _repository.UpdateAsync(product);
        return NoContent();
    }
}
