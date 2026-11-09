using Microsoft.EntityFrameworkCore;
using WarehouseApp.Infrastructure;

namespace WarehouseApp.Tests.TestHelpers;

/// <summary>
/// Тестовая реализация <see cref="IDbContextFactory{TContext}"/> для SQLite in-memory.
/// </summary>
/// <remarks>
/// Репозитории принимают <see cref="IDbContextFactory{TContext}"/> в конструкторе,
/// поэтому для тестов нужно создать фабрику вручную, а не через DI.
/// </remarks>
public class TestDbContextFactory : IDbContextFactory<WarehouseDbContext>
{
    /// <summary>
    /// Параметры контекста (строка подключения к SQLite in-memory).
    /// </summary>
    private readonly DbContextOptions<WarehouseDbContext> _options;

    /// <summary>
    /// Инициализирует фабрику с заданными параметрами контекста.
    /// </summary>
    /// <param name="options">Параметры контекста БД.</param>
    public TestDbContextFactory(DbContextOptions<WarehouseDbContext> options)
    {
        _options = options;
    }

    /// <summary>
    /// Создаёт новый экземпляр <see cref="WarehouseDbContext"/>.
    /// </summary>
    /// <returns>Новый контекст БД.</returns>
    public WarehouseDbContext CreateDbContext() => new(_options);

    /// <summary>
    /// Асинхронная версия — возвращает уже готовую задачу.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены (не используется).</param>
    /// <returns>Задача с новым контекстом БД.</returns>
    public Task<WarehouseDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}
