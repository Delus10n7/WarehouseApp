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
        private readonly WarehouseDbContext _context;

        public ProductRepository(WarehouseDbContext context) => _context = context;

        public Task<List<Product>> GetAllAsync(CancellationToken ct = default) =>
            _context.Products.Include(p => p.Category).Include(p => p.UnitOfMeasure).ToListAsync(ct);

        public Task<Product?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _context.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

        public async Task AddAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Product product, CancellationToken ct = default)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            var p = await _context.Products.FindAsync(new object[] { id }, ct);
            if (p is not null)
            {
                _context.Products.Remove(p);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
