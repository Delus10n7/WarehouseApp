using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

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
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно добавляет новый товар в базу данных.
    /// </summary>
    /// <param name="product">Объект товара для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task AddAsync(Product product, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно обновляет данные существующего товара.
    /// </summary>
    /// <param name="product">Объект товара с обновленными данными.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task UpdateAsync(Product product, CancellationToken ct = default);
}
