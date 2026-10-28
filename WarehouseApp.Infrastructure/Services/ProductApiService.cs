using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using WarehouseApp.Application.Services;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Services;

/// <summary>
/// Сервис для выполнения HTTP-запросов к API товаров.
/// </summary>
public class ProductApiService : IProductApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProductApiService"/>.
    /// </summary>
    public ProductApiService(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<Product>>("api/products", ct);
        return result ?? new List<Product>();
    }

    /// <inheritdoc />
    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products", product, ct);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/products/{product.Id}", product, ct);
        response.EnsureSuccessStatusCode();
    }
}
