using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Api.Controllers;

/// <summary>
/// Контроллер для управления товарами.
/// Предоставляет API-эндпоинты для получения, создания и обновления записей.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;

    /// <summary>
    /// Инициализирует контроллер товаров.
    /// </summary>
    /// <param name="repository">Репозиторий для доступа к данным контрагентов.</param>
    public ProductsController(IProductRepository repository) => _repository = repository;

    /// <summary>
    /// Получает список всех товаров с включением связанных данных (категории и ед. измерения).
    /// </summary>
    /// <returns>Коллекция объектов <see cref="Product"/>.</returns>
    /// <response code="200">Успешно возвращает список контрагентов (может быть пустым).</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _repository.GetAllWithDetailsAsync();
        return Ok(products);
    }

    /// <summary>
    /// Создает новый товар.
    /// </summary>
    /// <param name="product">Объект товара, передаваемый в теле запроса (JSON).</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="201">Товар успешно создан.</response>
    /// <response code="400">Переданы некорректные данные.</response>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Product product)
    {
        await _repository.AddAsync(product);
        return Created();
    }

    /// <summary>
    /// Обновляет данные существующего товара.
    /// </summary>
    /// <param name="id">Идентификатор товара, которого нужно обновить.</param>
    /// <param name="product">Обновленные данные товара.</param>
    /// <returns>Результат выполнения операции.</returns>
    /// <response code="204">Данные успешно обновлены (нет содержимого для возврата).</response>
    /// <response code="400">Идентификатор в URL не совпадает с идентификатором в теле запроса.</response>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id != product.Id) return BadRequest();
        await _repository.UpdateAsync(product);
        return NoContent();
    }
}
