using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;

namespace WarehouseApp.Infrastructure
{
    public class WarehouseDbContext : DbContext, IWarehouseDbContext
    {
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
            : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
        public DbSet<Counterparty> Counterparties => Set<Counterparty>();
        public DbSet<WareDocument> Documents => Set<WareDocument>();
        public DbSet<WareDocumentItem> DocumentItems => Set<WareDocumentItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}
