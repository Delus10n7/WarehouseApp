using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using WarehouseApp.Application.Services;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Services;

/// <summary>
/// Сервис для выполнения HTTP-запросов к API справочников.
/// </summary>
public class LookupApiService : ILookupApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="LookupApiService"/>.
    /// </summary>
    public LookupApiService(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<List<Category>> GetCategoriesAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<Category>>("api/lookups/categories");
        return result ?? new List<Category>();
    }

    /// <inheritdoc />
    public async Task<Category> AddCategoryAsync(Category category)
    {
        var response = await _httpClient.PostAsJsonAsync("api/lookups/categories", category);
        response.EnsureSuccessStatusCode();

        var createdCategory = await response.Content.ReadFromJsonAsync<Category>();
        return createdCategory ?? category;
    }

    /// <inheritdoc />
    public async Task<List<UnitOfMeasure>> GetUnitsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<List<UnitOfMeasure>>("api/lookups/units");
        return result ?? new List<UnitOfMeasure>();
    }

    /// <inheritdoc />
    public async Task<UnitOfMeasure> AddUnitAsync(UnitOfMeasure unit)
    {
        var response = await _httpClient.PostAsJsonAsync("api/lookups/units", unit);
        response.EnsureSuccessStatusCode();

        var createdUnit = await response.Content.ReadFromJsonAsync<UnitOfMeasure>();
        return createdUnit ?? unit;
    }
}
