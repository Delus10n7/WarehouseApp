using Microsoft.EntityFrameworkCore;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Repository;

/// <summary>
/// Реализация <see cref="ICounterpartyRepository"/> на EF Core.
/// </summary>
public class CounterpartyRepository : ICounterpartyRepository
{
    private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория контрагентов.
    /// </summary>
    /// <param name="contextFactory">Фабрика контекстов БД (scoped через DI).</param>
    public CounterpartyRepository(IDbContextFactory<WarehouseDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Counterparty>> GetAllAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Counterparties.AsNoTracking().ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Counterparty?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Counterparties.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<Counterparty> AddAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.Counterparties.Add(counterparty);
        await context.SaveChangesAsync(ct);
        return counterparty;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        context.Counterparties.Attach(counterparty);
        context.Entry(counterparty).State = EntityState.Modified;
        context.Entry(counterparty).Collection(c => c.Documents).IsModified = false;

        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var entity = await context.Counterparties.FindAsync(new object[] { id }, ct);
        if (entity is null) return false;

        context.Counterparties.Remove(entity);
        await context.SaveChangesAsync(ct);
        return true;
    }
}
