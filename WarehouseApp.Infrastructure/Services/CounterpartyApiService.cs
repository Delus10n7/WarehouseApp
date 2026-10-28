using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using WarehouseApp.Application.Services;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Services;

/// <summary>
/// Сервис для выполнения HTTP-запросов к API контрагентов.
/// </summary>
public class CounterpartyApiService : ICounterpartyApiService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CounterpartyApiService"/>.
    /// </summary>
    /// <param name="httpClient">Настроенный экземпляр HttpClient.</param>
    public CounterpartyApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default)
    {
        // Выполняем GET-запрос к api/counterparties
        var result = await _httpClient.GetFromJsonAsync<List<Counterparty>>("api/counterparties", ct);
        return result ?? new List<Counterparty>();
    }

    /// <inheritdoc />
    public async Task<Counterparty?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        // Выполняем GET-запрос к api/counterparties/{id}
        return await _httpClient.GetFromJsonAsync<Counterparty>($"api/counterparties/{id}", ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        // Выполняем POST-запрос
        var response = await _httpClient.PostAsJsonAsync("api/counterparties", counterparty, ct);
        response.EnsureSuccessStatusCode(); // Выбросит исключение, если ответ не 200 OK
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        // Выполняем PUT-запрос
        var response = await _httpClient.PutAsJsonAsync($"api/counterparties/{counterparty.Id}", counterparty, ct);
        response.EnsureSuccessStatusCode();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        // Выполняем DELETE-запрос
        var response = await _httpClient.DeleteAsync($"api/counterparties/{id}", ct);
        response.EnsureSuccessStatusCode();
    }
}
