using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure;
using WarehouseApp.Infrastructure.Repository;

namespace WarehouseApp.Tests.Infrastructure;

/// <summary>
/// Тесты для <see cref="CounterpartyRepository"/>. 
/// Используют реальную базу данных SQLite в оперативной памяти (in-memory).
/// </summary>
public class CounterpartyRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;
    private readonly CounterpartyRepository _repository;

    /// <summary>
    /// Инициализирует тестовое окружение: создает подключение к SQLite в памяти и настраивает схему БД.
    /// </summary>
    public CounterpartyRepositoryTests()
    {
        // Создаем соединение с БД в памяти. Держим его открытым, чтобы БД не удалилась.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_connection)
            .Options;

        _contextFactory = new TestDbContextFactory(options);

        // Создаем схему БД (таблицы) на основе наших конфигураций EF Core
        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        _repository = new CounterpartyRepository(_contextFactory);
    }

    /// <summary>
    /// Проверяет, что метод AddAsync корректно сохраняет контрагента в базу данных.
    /// </summary>
    [Fact]
    public async Task AddAsync_Should_Insert_Into_Database()
    {
        // Arrange
        var counterparty = new Counterparty { Name = "Тестовый Контрагент", Phone = "123" };

        // Act
        await _repository.AddAsync(counterparty);
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Тестовый Контрагент");
    }

    /// <summary>
    /// Проверяет, что метод DeleteAsync корректно удаляет контрагента из базы данных.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Should_Remove_From_Database()
    {
        // Arrange
        var counterparty = new Counterparty { Name = "Для удаления" };
        await _repository.AddAsync(counterparty);
        var all = await _repository.GetAllAsync();
        int idToDelete = all[0].Id;

        // Act
        await _repository.DeleteAsync(idToDelete);
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Освобождает ресурсы и закрывает соединение с базой данных после каждого теста.
    /// </summary>
    public void Dispose()
    {
        _connection.Dispose();
    }
}

/// <summary>
/// Вспомогательная реализация фабрики контекста БД для тестов.
/// </summary>
public class TestDbContextFactory : IDbContextFactory<WarehouseDbContext>
{
    private readonly DbContextOptions<WarehouseDbContext> _options;

    public TestDbContextFactory(DbContextOptions<WarehouseDbContext> options) => _options = options;

    public WarehouseDbContext CreateDbContext() => new WarehouseDbContext(_options);
}
