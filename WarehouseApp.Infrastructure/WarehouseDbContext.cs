using Microsoft.EntityFrameworkCore;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure;

/// <summary>
/// Контекст базы данных Entity Framework Core для складского приложения.
/// Реализует <see cref="IWarehouseDbContext"/> для использования в слое Application/Api.
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

    // ---------- Явная реализация IWarehouseDbContext ----------
    // DbSet<T> реализует IQueryable<T>, но компилятор не приводит типы автоматически.

    IQueryable<Product> IWarehouseDbContext.Products => Products;
    IQueryable<Category> IWarehouseDbContext.Categories => Categories;
    IQueryable<UnitOfMeasure> IWarehouseDbContext.UnitsOfMeasure => UnitsOfMeasure;
    IQueryable<Counterparty> IWarehouseDbContext.Counterparties => Counterparties;
    IQueryable<WareDocument> IWarehouseDbContext.Documents => Documents;
    IQueryable<WareDocumentItem> IWarehouseDbContext.DocumentItems => DocumentItems;

    void IWarehouseDbContext.Add<T>(T entity) => Set<T>().Add(entity);
    void IWarehouseDbContext.Update<T>(T entity) => Set<T>().Update(entity);
    void IWarehouseDbContext.Remove<T>(T entity) => Set<T>().Remove(entity);

    /// <summary>
    /// Настраивает модель базы данных, применяя все <c>IEntityTypeConfiguration</c>
    /// из текущей сборки (папка Configurations).
    /// </summary>
    /// <param name="modelBuilder">Построитель модели EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
