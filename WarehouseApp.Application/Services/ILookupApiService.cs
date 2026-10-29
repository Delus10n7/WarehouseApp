using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;

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
    Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно добавляет новую категорию в базу данных.
    /// </summary>
    /// <param name="category">Объект категория для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task<CategoryDto> AddCategoryAsync(CategoryCreateDto category, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно получает список всех единиц измерения.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список единиц измерения.</returns>
    Task<List<UnitOfMeasureDto>> GetUnitsAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно добавляет новую единицу измерения в базу данных.
    /// </summary>
    /// <param name="unit">Объект единица измерения для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task<UnitOfMeasureDto> AddUnitAsync(UnitOfMeasureCreateDto unit, CancellationToken ct = default);
}
