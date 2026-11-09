using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure;
using WarehouseApp.Infrastructure.Repository;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure;

/// <summary>
/// Integration-тесты <see cref="ProductRepository"/> на SQLite in-memory.
/// </summary>
public class ProductRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ProductRepository _repository;

    /// <summary>
    /// Настраивает SQLite in-memory, создаёт схему и репозиторий.
    /// </summary>
    public ProductRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_connection)
            .Options;

        var factory = new TestDbContextFactory(options);

        using var context = factory.CreateDbContext();
        context.Database.EnsureCreated();

        _repository = new ProductRepository(factory);
    }

    /// <summary>
    /// Проверяет, что <c>GetAllWithDetailsAsync</c> подгружает навигации
    /// Category и UnitOfMeasure через Include.
    /// </summary>
    [Fact]
    public async Task GetAllWithDetailsAsync_Should_Include_Navigations()
    {
        // Arrange — создаём категорию, единицу и товар
        using (var context = new TestDbContextFactory(
            new DbContextOptionsBuilder<WarehouseDbContext>().UseSqlite(_connection).Options)
            .CreateDbContext())
        {
            var cat = new Category { Name = "Кат" };
            var unit = new UnitOfMeasure { Name = "шт" };
            context.Categories.Add(cat);
            context.UnitsOfMeasure.Add(unit);
            await context.SaveChangesAsync();

            context.Products.Add(new Product
            {
                Name = "Товар",
                Article = "T-1",
                CategoryId = cat.Id,
                UnitOfMeasureId = unit.Id
            });
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _repository.GetAllWithDetailsAsync();

        // Assert — навигации должны быть не null
        result.Should().HaveCount(1);
        result[0].Category.Should().NotBeNull();
        result[0].Category!.Name.Should().Be("Кат");
        result[0].UnitOfMeasure.Should().NotBeNull();
        result[0].UnitOfMeasure!.Name.Should().Be("шт");
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
    /// Проверяет, что <c>AddAsync</c> вставляет запись и присваивает Id.
    /// </summary>
    [Fact]
    public async Task AddAsync_Should_Insert_And_Return_With_Id()
    {
        // Arrange — нужны Category и UnitOfMeasure (FK Restrict)
        using var context = new TestDbContextFactory(
            new DbContextOptionsBuilder<WarehouseDbContext>().UseSqlite(_connection).Options)
            .CreateDbContext();
        var cat = new Category { Name = "Кат" };
        var unit = new UnitOfMeasure { Name = "шт" };
        context.AddRange(cat, unit);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Name = "Товар",
            Article = "T-1",
            CategoryId = cat.Id,
            UnitOfMeasureId = unit.Id,
            Quantity = 5m
        };

        // Act
        var returned = await _repository.AddAsync(product);

        // Assert
        returned.Id.Should().BeGreaterThan(0);
        (await _repository.GetAllAsync()).Should().HaveCount(1);
    }

    /// <summary>
    /// Проверяет, что <c>UpdateAsync</c> обновляет поля Product.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Should_Modify_Fields()
    {
        // Arrange — создаём зависимости и товар
        using var context = new TestDbContextFactory(
            new DbContextOptionsBuilder<WarehouseDbContext>().UseSqlite(_connection).Options)
            .CreateDbContext();
        var cat = new Category { Name = "Кат" };
        var unit = new UnitOfMeasure { Name = "шт" };
        context.AddRange(cat, unit);
        await context.SaveChangesAsync();

        var product = await _repository.AddAsync(new Product
        {
            Name = "Старое",
            Article = "OLD",
            CategoryId = cat.Id,
            UnitOfMeasureId = unit.Id
        });

        // Act
        product.Name = "Новое";
        product.Quantity = 42m;
        await _repository.UpdateAsync(product);

        // Assert
        var fetched = await _repository.GetByIdAsync(product.Id);
        fetched!.Name.Should().Be("Новое");
        fetched.Quantity.Should().Be(42m);
    }

    /// <summary>
    /// Проверяет, что <c>DeleteAsync</c> удаляет запись и возвращает true.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Returns_True_When_Deleted()
    {
        using var context = new TestDbContextFactory(
            new DbContextOptionsBuilder<WarehouseDbContext>().UseSqlite(_connection).Options)
            .CreateDbContext();
        var cat = new Category { Name = "Кат" };
        var unit = new UnitOfMeasure { Name = "шт" };
        context.AddRange(cat, unit);
        await context.SaveChangesAsync();

        var product = await _repository.AddAsync(new Product
        {
            Name = "Удалить",
            Article = "D-1",
            CategoryId = cat.Id,
            UnitOfMeasureId = unit.Id
        });

        var deleted = await _repository.DeleteAsync(product.Id);

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

    /// <summary>Освобождает соединение.</summary>
    public void Dispose() => _connection.Dispose();
}
