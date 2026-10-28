using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure;
using WarehouseApp.Infrastructure.Repository;

namespace WarehouseApp.Tests.Infrastructure;

/// <summary>
/// Тесты для <see cref="ProductRepository"/>. 
/// Используют SQLite в оперативной памяти для проверки запросов EF Core.
/// </summary>
public class ProductRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_connection)
            .Options;

        _contextFactory = new TestDbContextFactory(options);

        using var context = _contextFactory.CreateDbContext();
        context.Database.EnsureCreated();

        _repository = new ProductRepository(_contextFactory);
    }

    /// <summary>
    /// Проверяет, что метод GetAllWithDetailsAsync возвращает товары с подгруженными категориями и единицами измерения.
    /// </summary>
    [Fact]
    public async Task GetAllWithDetailsAsync_Should_Include_Related_Entities()
    {
        // Arrange
        using (var context = _contextFactory.CreateDbContext())
        {
            var category = new Category { Name = "Электроника" };
            var unit = new UnitOfMeasure { Name = "шт" };
            context.Categories.Add(category);
            context.UnitsOfMeasure.Add(unit);
            await context.SaveChangesAsync();

            context.Products.Add(new Product
            {
                Name = "Телефон",
                Article = "TEL-01",
                CategoryId = category.Id,
                UnitOfMeasureId = unit.Id
            });
            await context.SaveChangesAsync();
        }

        // Act
        var result = await _repository.GetAllWithDetailsAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Category.Should().NotBeNull();
        result[0].Category!.Name.Should().Be("Электроника");
        result[0].UnitOfMeasure.Should().NotBeNull();
    }

    public void Dispose() => _connection.Dispose();
}
