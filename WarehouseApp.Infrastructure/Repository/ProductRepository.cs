using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;

namespace WarehouseApp.Infrastructure.Repository;

/// <summary>
/// Реализация репозитория для работы с товарами в базе данных.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория товаров.
    /// </summary>
    /// <param name="contextFactory">Фабрика для создания экземпляров <see cref="WarehouseDbContext"/>.</param>
    public ProductRepository(IDbContextFactory<WarehouseDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Products.Include(p => p.Category).Include(p => p.UnitOfMeasure).ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.Products.Add(product);
        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        context.Products.Update(product);
        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        var p = await context.Products.FindAsync(new object[] { id }, ct);
        if (p is not null)
        {
            context.Products.Remove(p);
            await context.SaveChangesAsync(ct);
        }
    }

    /// <inheritdoc />
    public async Task<List<Product>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.UnitOfMeasure)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
