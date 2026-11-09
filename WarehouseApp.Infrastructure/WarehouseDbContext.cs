using Microsoft.EntityFrameworkCore;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure;

/// <summary>
/// Контекст базы данных Entity Framework Core для складского приложения.
/// </summary>
public class WarehouseDbContext : DbContext, IWarehouseDbContext
{
    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="WarehouseDbContext"/>.
    /// </summary>
    /// <param name="options">
    /// Параметры конфигурации контекста: строка подключения, провайдер БД, политики
    /// повторных попыток и т.д. Передаются через <see cref="DbContextOptionsBuilder{TContext}"/>.
    /// </param>
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options)
        : base(options) { }                       // Пробрасываем options в базовый DbContext


    // DbSet-свойства.

    /// <summary>
    /// Таблица товаров. <see cref="DbSet{TEntity}"/> — коллекция, привязанная к таблице БД.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// Таблица категорий.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Таблица единиц измерения.
    /// </summary>
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();

    /// <summary>
    /// Таблица контрагентов.
    /// </summary>
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();

    /// <summary>
    /// Таблица складских документов.
    /// </summary>
    public DbSet<WareDocument> Documents => Set<WareDocument>();

    /// <summary>
    /// Таблица позиций (строк) документов.
    /// </summary>
    public DbSet<WareDocumentItem> DocumentItems => Set<WareDocumentItem>();


    // Явная реализация IWarehouseDbContext.

    /// <summary>Реализация <see cref="IWarehouseDbContext.Products"/> — как IQueryable.</summary>
    IQueryable<Product> IWarehouseDbContext.Products => Products;

    /// <summary>Реализация <see cref="IWarehouseDbContext.Categories"/> — как IQueryable.</summary>
    IQueryable<Category> IWarehouseDbContext.Categories => Categories;

    /// <summary>Реализация <see cref="IWarehouseDbContext.UnitsOfMeasure"/> — как IQueryable.</summary>
    IQueryable<UnitOfMeasure> IWarehouseDbContext.UnitsOfMeasure => UnitsOfMeasure;

    /// <summary>Реализация <see cref="IWarehouseDbContext.Counterparties"/> — как IQueryable.</summary>
    IQueryable<Counterparty> IWarehouseDbContext.Counterparties => Counterparties;

    /// <summary>Реализация <see cref="IWarehouseDbContext.Documents"/> — как IQueryable.</summary>
    IQueryable<WareDocument> IWarehouseDbContext.Documents => Documents;

    /// <summary>Реализация <see cref="IWarehouseDbContext.DocumentItems"/> — как IQueryable.</summary>
    IQueryable<WareDocumentItem> IWarehouseDbContext.DocumentItems => DocumentItems;

    /// <summary>
    /// Реализация <see cref="IWarehouseDbContext.Add{T}"/>:
    /// добавляет сущность в ChangeTracker со статусом Added.
    /// </summary>
    /// <typeparam name="T">Тип сущности.</typeparam>
    /// <param name="entity">Экземпляр для добавления.</param>
    void IWarehouseDbContext.Add<T>(T entity) => Set<T>().Add(entity);

    /// <summary>
    /// Реализация <see cref="IWarehouseDbContext.Update{T}"/>:
    /// помечает сущность как Modified.
    /// </summary>
    /// <typeparam name="T">Тип сущности.</typeparam>
    /// <param name="entity">Экземпляр для обновления.</param>
    void IWarehouseDbContext.Update<T>(T entity) => Set<T>().Update(entity);

    /// <summary>
    /// Реализация <see cref="IWarehouseDbContext.Remove{T}"/>:
    /// помечает сущность как Deleted.
    /// </summary>
    /// <typeparam name="T">Тип сущности.</typeparam>
    /// <param name="entity">Экземпляр для удаления.</param>
    void IWarehouseDbContext.Remove<T>(T entity) => Set<T>().Remove(entity);

    /// <summary>
    /// Настраивает модель БД при её создании.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Находим все IEntityTypeConfiguration в сборке Infrastructure и применяем
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WarehouseDbContext).Assembly);

        // Вызов базовой реализации — на случай, если EF добавит что-то своё
        base.OnModelCreating(modelBuilder);
    }
}
