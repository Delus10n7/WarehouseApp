using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;

namespace WarehouseApp.Infrastructure;

/// <summary>
/// Контекст базы данных Entity Framework Core для складского приложения.
/// Предоставляет доступ к таблицам и управляет отслеживанием изменений.
/// </summary>
public class WarehouseDbContext : DbContext, IWarehouseDbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="WarehouseDbContext"/>.
    /// </summary>
    /// <param name="options">Параметры конфигурации контекста базы данных.</param>
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options) { }

    /// <summary>Таблица товаров.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Таблица категорий.</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>Таблица единиц измерения.</summary>
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    /// <summary>Таблица контрагентов.</summary>
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();

    /// <summary>Таблица складских документов.</summary>
    public DbSet<WareDocument> Documents => Set<WareDocument>();

    /// <summary>Таблица позиций (строк) документов.</summary>
    public DbSet<WareDocumentItem> DocumentItems => Set<WareDocumentItem>();

    /// <summary>
    /// Настраивает модель базы данных при её создании, применяя конфигурации из текущей сборки.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Автоматически применяет все классы, реализующие IEntityTypeConfiguration из этой сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
