using System.Net.Http.Json;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Infrastructure.Client.Services;

/// <summary>
/// HTTP-клиент для API товаров. Реализует <see cref="IProductApiService"/>.
/// </summary>
public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ProductApiService"/>.
    /// </summary>
    /// <param name="httpClient">Типизированный HttpClient с заданным BaseAddress.</param>
    public ProductApiService(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<List<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<ProductDto>>("api/products", ct);
        return result ?? new List<ProductDto>();
    }

    /// <inheritdoc />
    public async Task<ProductDto> AddAsync(ProductRequestDto product, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products", product, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ProductDto>(ct);
        return created ?? throw new InvalidOperationException("Server returned empty response on create.");
    }

    /// <inheritdoc />
    public async Task UpdateAsync(int id, ProductRequestDto product, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/products/{id}", product, ct);
        response.EnsureSuccessStatusCode();
    }
}
