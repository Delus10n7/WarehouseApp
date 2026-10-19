using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Interfaces.Abstractions;

namespace WarehouseApp.Infrastructure.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;

        public ProductRepository(IDbContextFactory<WarehouseDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            return await context.Products.Include(p => p.Category).Include(p => p.UnitOfMeasure).ToListAsync(ct);
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            return await context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task AddAsync(Product product, CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            context.Products.Add(product);
            await context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(ct);
            context.Products.Update(product);
            await context.SaveChangesAsync(ct);
        }

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
    }
}
