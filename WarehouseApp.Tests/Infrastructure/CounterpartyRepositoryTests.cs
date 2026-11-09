using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure;
using WarehouseApp.Infrastructure.Repository;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure;

/// <summary>
/// Integration-тесты <see cref="CounterpartyRepository"/> на SQLite in-memory.
/// </summary>
/// <remarks>
/// Используется реальная БД (SQLite в памяти), а не моки — так проверяется
/// корректность EF Core-запросов (WHERE, INSERT, UPDATE, DELETE).
/// </remarks>
public class CounterpartyRepositoryTests : IDisposable
{
    /// <summary>SQLite-соединение в памяти. Держим открытым, пока жив класс.</summary>
    private readonly SqliteConnection _connection;

    /// <summary>Репозиторий, который тестируем.</summary>
    private readonly CounterpartyRepository _repository;

    /// <summary>
    /// Настраивает SQLite in-memory, создаёт схему и репозиторий.
    /// </summary>
    public CounterpartyRepositoryTests()
    {
        // SQLite in-memory живёт, пока открыто соединение
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_connection)
            .Options;

        var factory = new TestDbContextFactory(options);

        // Создаём схему по конфигурациям EF (без миграций — быстро для тестов)
        using var context = factory.CreateDbContext();
        context.Database.EnsureCreated();

        _repository = new CounterpartyRepository(factory);
    }

    /// <summary>
    /// Проверяет, что <c>GetAllAsync</c> на пустой БД возвращает пустой список.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_On_Empty_Db_Returns_Empty_List()
    {
        var result = await _repository.GetAllAsync();
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Проверяет, что <c>AddAsync</c> вставляет запись и возвращает сущность с присвоенным Id.
    /// </summary>
    [Fact]
    public async Task AddAsync_Should_Insert_And_Return_Entity_With_Id()
    {
        var entity = new Counterparty { Name = "Тест", Phone = "123" };

        var returned = await _repository.AddAsync(entity);

        returned.Id.Should().BeGreaterThan(0);
        returned.Name.Should().Be("Тест");

        var all = await _repository.GetAllAsync();
        all.Should().HaveCount(1);
        all[0].Phone.Should().Be("123");
    }

    /// <summary>
    /// Проверяет, что <c>GetByIdAsync</c> возвращает null для несуществующего Id.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Returns_Null_When_Not_Found()
    {
        var result = await _repository.GetByIdAsync(999);
        result.Should().BeNull();
    }

    /// <summary>
    /// Проверяет, что <c>GetByIdAsync</c> возвращает сущность для существующего Id.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Returns_Entity_When_Found()
    {
        var added = await _repository.AddAsync(new Counterparty { Name = "Найти меня" });

        var result = await _repository.GetByIdAsync(added.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Найти меня");
    }

    /// <summary>
    /// Проверяет, что <c>UpdateAsync</c> изменяет поля существующей записи.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Modify_Existing_Entity()
    {
        var entity = await _repository.AddAsync(new Counterparty { Name = "Старое", Phone = "111" });

        entity.Name = "Новое";
        entity.Phone = "222";
        await _repository.UpdateAsync(entity);

        var fetched = await _repository.GetByIdAsync(entity.Id);
        fetched!.Name.Should().Be("Новое");
        fetched.Phone.Should().Be("222");
    }

    /// <summary>
    /// Проверяет, что <c>DeleteAsync</c> возвращает true при успешном удалении.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Returns_True_When_Deleted()
    {
        var entity = await _repository.AddAsync(new Counterparty { Name = "Удалить" });

        var deleted = await _repository.DeleteAsync(entity.Id);

        deleted.Should().BeTrue();
        (await _repository.GetAllAsync()).Should().BeEmpty();
    }

    /// <summary>
    /// Проверяет, что <c>DeleteAsync</c> возвращает false для несуществующего Id.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Returns_False_When_Not_Found()
    {
        var deleted = await _repository.DeleteAsync(999);
        deleted.Should().BeFalse();
    }

    /// <summary>
    /// Освобождает SQLite-соединение после каждого теста.
    /// </summary>
    public void Dispose() => _connection.Dispose();
}
