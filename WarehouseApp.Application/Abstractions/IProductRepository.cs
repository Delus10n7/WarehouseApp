using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Abstractions;

/// <summary>
/// Интерфейс репозитория для управления сущностями <see cref="Product"/> (товарами).
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Асинхронно получает список всех товаров.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список товаров.</returns>
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно получает товар по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор товара.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект товара или null, если товар не найден.</returns>
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);

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

    /// <summary>
    /// Асинхронно удаляет товар из базы данных по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор товара.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
