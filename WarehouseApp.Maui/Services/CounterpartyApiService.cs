using System.Net.Http.Json;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Maui.Services;

public class CounterpartyApiService : ICounterpartyApiService
{
    private readonly HttpClient _httpClient;

    // HttpClient будет внедряться через DI
    public CounterpartyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default)
    {
        // Выполняем GET-запрос к api/counterparties
        var result = await _httpClient.GetFromJsonAsync<List<Counterparty>>("api/counterparties", ct);
        return result ?? new List<Counterparty>();
    }

    public async Task<Counterparty?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Выполняем GET-запрос к api/counterparties/{id}
        return await _httpClient.GetFromJsonAsync<Counterparty>($"api/counterparties/{id}", ct);
    }

    public async Task AddAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        // Выполняем POST-запрос
        var response = await _httpClient.PostAsJsonAsync("api/counterparties", counterparty, ct);
        response.EnsureSuccessStatusCode(); // Выбросит исключение, если ответ не 200 OK
    }

    public async Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        // Выполняем PUT-запрос
        var response = await _httpClient.PutAsJsonAsync($"api/counterparties/{counterparty.Id}", counterparty, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        // Выполняем DELETE-запрос
        var response = await _httpClient.DeleteAsync($"api/counterparties/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}