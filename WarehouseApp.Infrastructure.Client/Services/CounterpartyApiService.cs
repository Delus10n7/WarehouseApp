using System.Net;
using System.Net.Http.Json;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Infrastructure.Client.Services;

/// <summary>
/// HTTP-клиент для API контрагентов. Реализует <see cref="ICounterpartyApiService"/>.
/// </summary>
public class CounterpartyApiService : ICounterpartyApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CounterpartyApiService"/>.
    /// </summary>
    /// <param name="httpClient">Типизированный HttpClient с заданным BaseAddress.</param>
    public CounterpartyApiService(HttpClient httpClient) => _httpClient = httpClient;

    /// <inheritdoc />
    public async Task<List<CounterpartyDto>> GetAllAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<CounterpartyDto>>("api/counterparties", ct);
        return result ?? new List<CounterpartyDto>();
    }

    /// <inheritdoc />
    public async Task<CounterpartyDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/counterparties/{id}", ct);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CounterpartyDto>(ct);
    }

    /// <inheritdoc />
    public async Task<CounterpartyDto> AddAsync(CounterpartyRequestDto counterparty, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync("api/counterparties", counterparty, ct);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<CounterpartyDto>(ct);
        return created ?? throw new InvalidOperationException("Server returned empty response on create.");
    }

    /// <inheritdoc />
    public async Task UpdateAsync(int id, CounterpartyRequestDto counterparty, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/counterparties/{id}", counterparty, ct);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/counterparties/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
