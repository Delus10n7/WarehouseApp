using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Interfaces.Abstractions
{
    public interface IWarehouseDbContext
    {
        DbSet<Product> Products { get; }
        DbSet<Category> Categories { get; }
        DbSet<UnitOfMeasure> UnitsOfMeasure { get; }
        DbSet<Counterparty> Counterparties { get; }
        DbSet<WareDocument> Documents { get; }
        DbSet<WareDocumentItem> DocumentItems { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
