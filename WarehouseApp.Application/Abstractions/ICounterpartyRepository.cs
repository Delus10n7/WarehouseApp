using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Abstractions;

public interface ICounterpartyRepository
{
    Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Counterparty counterparty, CancellationToken ct = default);
    Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
