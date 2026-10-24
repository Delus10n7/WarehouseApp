using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;

namespace WarehouseApp.Infrastructure.Repository;

public class CounterpartyRepository : ICounterpartyRepository
{
    private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;

    public CounterpartyRepository(IDbContextFactory<WarehouseDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Counterparties.AsNoTracking().ToListAsync(ct);
    }

    public async Task AddAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.Counterparties.Add(counterparty);
        await context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.Counterparties.Update(counterparty);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var counterparty = await context.Counterparties.FindAsync(new object[] { id }, ct);
        if (counterparty != null)
        {
            context.Counterparties.Remove(counterparty);
            await context.SaveChangesAsync(ct);
        }
    }
}
