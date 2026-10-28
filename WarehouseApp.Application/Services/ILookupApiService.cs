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
    Task<List<Category>> GetCategoriesAsync();
    Task<Category> AddCategoryAsync(Category category);
    Task<List<UnitOfMeasure>> GetUnitsAsync();
    Task<UnitOfMeasure> AddUnitAsync(UnitOfMeasure unit);
}
