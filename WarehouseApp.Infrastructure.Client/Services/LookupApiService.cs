using System.Net.Http.Json;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Infrastructure.Client.Services;

/// <summary>
/// HTTP-клиент для API справочников. Реализует <see cref="ILookupApiService"/>.
/// </summary>
public class LookupApiService : ILookupApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="LookupApiService"/>.
    /// </summary>
    /// <param name="httpClient">Типизированный HttpClient с заданным BaseAddress.</param>
    public LookupApiService(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/lookups/categories", ct);
        return result ?? new List<CategoryDto>();
    }

    /// <inheritdoc />
    public async Task<CategoryDto> AddCategoryAsync(CategoryCreateDto category, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/lookups/categories", category, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CategoryDto>(ct);
        return created ?? throw new InvalidOperationException("Server returned empty response on create.");
    }

    /// <inheritdoc />
    public async Task<List<UnitOfMeasureDto>> GetUnitsAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<UnitOfMeasureDto>>("api/lookups/units", ct);
        return result ?? new List<UnitOfMeasureDto>();
    }

    /// <inheritdoc />
    public async Task<UnitOfMeasureDto> AddUnitAsync(UnitOfMeasureCreateDto unit, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/lookups/units", unit, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<UnitOfMeasureDto>(ct);
        return created ?? throw new InvalidOperationException("Server returned empty response on create.");
    }
}
