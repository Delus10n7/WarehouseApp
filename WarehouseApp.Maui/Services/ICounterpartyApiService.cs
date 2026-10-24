using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Maui.Services;

public interface ICounterpartyApiService
{
    Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default);
    Task<Counterparty?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Counterparty counterparty, CancellationToken ct = default);
    Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

