using Microsoft.EntityFrameworkCore;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Repository;

/// <summary>
/// Реализация <see cref="IProductRepository"/> на EF Core.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория товаров.
    /// </summary>
    /// <param name="contextFactory">Фабрика контекстов БД (scoped через DI).</param>
    public ProductRepository(IDbContextFactory<WarehouseDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.UnitOfMeasure)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.UnitOfMeasure)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product product, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.Products.Add(product);
        await context.SaveChangesAsync(ct);
        return product;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        context.Products.Attach(product);
        context.Entry(product).State = EntityState.Modified;

        context.Entry(product).Reference(p => p.Category).IsModified = false;
        context.Entry(product).Reference(p => p.UnitOfMeasure).IsModified = false;

        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var entity = await context.Products.FindAsync(new object[] { id }, ct);
        if (entity is null) return false;

        context.Products.Remove(entity);
        await context.SaveChangesAsync(ct);
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.UnitOfMeasure)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
