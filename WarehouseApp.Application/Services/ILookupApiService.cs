using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Services;

/// <summary>
/// Интерфейс сервиса для работы со справочниками (категориями и единицами измерения).
/// </summary>
public interface ILookupApiService
{
    /// <summary>
    /// Асинхронно получает список всех категорий.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список категорий.</returns>
    Task<List<Category>> GetCategoriesAsync();

    /// <summary>
    /// Асинхронно добавляет новую категорию в базу данных.
    /// </summary>
    /// <param name="category">Объект категория для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task<Category> AddCategoryAsync(Category category);

    /// <summary>
    /// Асинхронно получает список всех единиц измерения.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список единиц измерения.</returns>
    Task<List<UnitOfMeasure>> GetUnitsAsync();

    /// <summary>
    /// Асинхронно добавляет новую единицу измерения в базу данных.
    /// </summary>
    /// <param name="unit">Объект единица измерения для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task<UnitOfMeasure> AddUnitAsync(UnitOfMeasure unit);
}
