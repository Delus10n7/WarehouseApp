using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Domain.Entities.Enums;
using WarehouseApp.Infrastructure;
using WarehouseApp.Infrastructure.Repository;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure;

/// <summary>
/// Integration-тесты <see cref="WareDocumentRepository"/> на SQLite in-memory.
/// Проверяют бизнес-логику списания и пополнения остатков товаров.
/// </summary>
public class WareDocumentRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WareDocumentRepository _repository;
    private readonly TestDbContextFactory _factory;
    private int _productId;
    private int _counterpartyId;

    /// <summary>
    /// Настраивает окружение: создает БД, схему, репозиторий и заполняет базовые справочники.
    /// </summary>
    public WareDocumentRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_connection)
            .Options;

        _factory = new TestDbContextFactory(options);

        using var context = _factory.CreateDbContext();
        context.Database.EnsureCreated();

        // Seed: Категория, Единица, Товар (остаток 10), Контрагент
        var cat = new Category { Name = "Кат" };
        var unit = new UnitOfMeasure { Name = "шт" };
        context.AddRange(cat, unit);
        context.SaveChanges();

        var product = new Product
        {
            Name = "Товар",
            Article = "T-1",
            CategoryId = cat.Id,
            UnitOfMeasureId = unit.Id,
            Quantity = 10m
        };
        var cp = new Counterparty { Name = "Контрагент" };
        context.AddRange(product, cp);
        context.SaveChanges();

        _productId = product.Id;
        _counterpartyId = cp.Id;

        _repository = new WareDocumentRepository(_factory);
    }

    /// <summary>
    /// Проверяет, что создание приходной накладной (Receipt) увеличивает остаток товара.
    /// </summary>
    [Fact]
    public async Task AddAsync_Receipt_Increases_Stock()
    {
        var document = CreateDocument(WareDocumentType.Receipt, 5m);

        await _repository.AddAsync(document);

        using var context = _factory.CreateDbContext();
        var product = await context.Products.FindAsync(_productId);
        product!.Quantity.Should().Be(15m); // 10 + 5
    }

    /// <summary>
    /// Проверяет, что создание расходной накладной (Sale) уменьшает остаток товара.
    /// </summary>
    [Fact]
    public async Task AddAsync_Sale_Decreases_Stock()
    {
        var document = CreateDocument(WareDocumentType.Sale, 3m);

        await _repository.AddAsync(document);

        using var context = _factory.CreateDbContext();
        var product = await context.Products.FindAsync(_productId);
        product!.Quantity.Should().Be(7m); // 10 - 3
    }

    /// <summary>
    /// Проверяет, что при попытке продать больше, чем есть на складе, 
    /// выбрасывается исключение и остатки не меняются.
    /// </summary>
    [Fact]
    public async Task AddAsync_Sale_With_Insufficient_Stock_Throws_Exception()
    {
        var document = CreateDocument(WareDocumentType.Sale, 15m);

        var act = async () => await _repository.AddAsync(document);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Недостаточно товара*Доступно: 10*запрошено: 15*");

        using var context = _factory.CreateDbContext();
        var product = await context.Products.FindAsync(_productId);
        product!.Quantity.Should().Be(10m); // Остаток не изменился
    }

    /// <summary>
    /// Проверяет, что удаление документа корректно отменяет его влияние на остатки.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Reverses_Stock_Impact()
    {
        // Arrange: создаем приход на 5 (остаток 15)
        var document = CreateDocument(WareDocumentType.Receipt, 5m);
        await _repository.AddAsync(document);

        // Act: удаляем документ
        var deleted = await _repository.DeleteAsync(document.Id);

        // Assert
        deleted.Should().BeTrue();
        using var context = _factory.CreateDbContext();
        var product = await context.Products.FindAsync(_productId);
        product!.Quantity.Should().Be(10m); // Вернулось к исходному
    }

    /// <summary>
    /// Проверяет, что при редактировании документа сначала отменяется старое влияние на остатки, 
    /// а затем применяется новое.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Correctly_Reverses_And_Applies_New_Stock_Impact()
    {
        // Arrange: создаем приход на 5 (остаток 15)
        var document = CreateDocument(WareDocumentType.Receipt, 5m);
        await _repository.AddAsync(document);

        // Act: меняем приход на 8 и тип на Продажу
        document.Type = WareDocumentType.Sale;
        document.Items.First().Quantity = 8m;

        await _repository.UpdateAsync(document);

        // Assert
        using var context = _factory.CreateDbContext();
        var product = await context.Products.FindAsync(_productId);

        // Логика: 10 (исходный) + 5 (старый приход) - 5 (отмена) - 8 (новая продажа) = 2
        product!.Quantity.Should().Be(2m);
    }

    /// <summary>
    /// Вспомогательный метод для создания тестового документа.
    /// </summary>
    private WareDocument CreateDocument(WareDocumentType type, decimal quantity)
    {
        return new WareDocument
        {
            Number = "DOC-1",
            Date = DateTime.UtcNow,
            Type = type,
            CounterpartyId = _counterpartyId,
            TotalAmount = quantity * 100m,
            Items = new List<WareDocumentItem>
            {
                new()
                {
                    ProductId = _productId,
                    Quantity = quantity,
                    Price = 100m
                }
            }
        };
    }

    public void Dispose() => _connection.Dispose();
}
