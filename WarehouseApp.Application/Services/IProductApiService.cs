using WarehouseApp.Application.Dtos.ProductDtos;

namespace WarehouseApp.Application.Services;

/// <summary>
/// Интерфейс сервиса для работы с API товаров.
/// </summary>
public interface IProductApiService
{
    /// <summary>
    /// Асинхронно получает список всех товаров.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список товаров.</returns>
    Task<List<ProductDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно добавляет новый товар в базу данных.
    /// </summary>
    /// <param name="product">Объект товара для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный товар.</returns>
    Task<ProductDto> AddAsync(ProductRequestDto product, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно обновляет данные существующего товара.
    /// </summary>
    /// <param name="id">Идентификатор товара для обновления.</param>
    /// <param name="product">Объект товара с обновленными данными.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task UpdateAsync(int id, ProductRequestDto product, CancellationToken ct = default);
}
