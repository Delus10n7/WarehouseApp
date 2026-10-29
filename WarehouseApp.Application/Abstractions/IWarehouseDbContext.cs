using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Abstractions;

/// <summary>
/// Интерфейс, представляющий контекст базы данных для работы со складскими сущностями.
/// </summary>
public interface IWarehouseDbContext
{
    /// <summary>
    /// Набор сущностей товаров.
    /// </summary>
    IQueryable<Product> Products { get; }

    /// <summary>
    /// Набор сущностей категорий.
    /// </summary>
    IQueryable<Category> Categories { get; }

    /// <summary>
    /// Набор сущностей единиц измерения.
    /// </summary>
    IQueryable<UnitOfMeasure> UnitsOfMeasure { get; }

    /// <summary>
    /// Набор сущностей контрагентов.
    /// </summary>
    IQueryable<Counterparty> Counterparties { get; }

    /// <summary>
    /// Набор сущностей складских документов.
    /// </summary>
    IQueryable<WareDocument> Documents { get; }

    /// <summary>
    /// Набор сущностей позиций (строк) складских документов.
    /// </summary>
    IQueryable<WareDocumentItem> DocumentItems { get; }


    /// <summary>
    /// Добавляет сущность в контекст (будет сохранена при SaveChangesAsync).
    /// </summary>
    void Add<T>(T entity) where T : class;

    /// <summary>
    /// Помечает сущность как изменённую (для UPDATE).
    /// </summary>
    void Update<T>(T entity) where T : class;

    /// <summary>
    /// Помечает сущность как удалённую (для DELETE).
    /// </summary>
    void Remove<T>(T entity) where T : class;

    /// <summary>
    /// Асинхронно сохраняет все изменения, отслеживаемые в контексте, в базе данных.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Количество записей, затронутых операцией сохранения.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
