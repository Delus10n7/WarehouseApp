using FluentAssertions;
using WarehouseApp.Application.Dtos;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Tests.Application;

/// <summary>
/// Тесты для extension-методов маппинга <see cref="Mappings"/>.
/// </summary>
/// <remarks>
/// Покрывают только <b>нетривиальную</b> логику маппинга:
/// <list type="bullet">
///   <item><description>нормализацию опциональных полей (<c>"   " → null</c>);</description></item>
///   <item><description>обрезку пробелов (<c>Trim</c>);</description></item>
///   <item><description>разворачивание навигаций в плоские поля (<c>CategoryName</c>);</description></item>
///   <item><description>инициализацию полей, недоступных клиенту (<c>Quantity = 0</c>);</description></item>
///   <item><description>защиту от перезаписи служебных полей (<c>Id</c>, <c>Quantity</c>, <c>Price</c>).</description></item>
/// </list>
/// Тривиальные маппинги (простое копирование 2 полей для Category / UnitOfMeasure)
/// не тестируются отдельно — они неявно покрыты в тестах контроллеров
/// (<c>CreateCategory_Persists_And_Returns_Dto</c>, <c>CreateUnit_Persists_And_Returns_Dto</c>).
/// </remarks>
public class MappingsTests
{
    // ---------- Counterparty ----------

    /// <summary>
    /// Проверяет, что <c>Counterparty.ToDto()</c> копирует все 4 поля (включая опциональные).
    /// </summary>
    /// <remarks>
    /// Тест важен, потому что <see cref="Counterparty"/> содержит опциональные поля
    /// (<c>Phone</c>, <c>Email</c>), и их корректное копирование — часть контракта.
    /// </remarks>
    [Fact]
    public void Counterparty_ToDto_Should_Copy_All_Fields()
    {
        // Arrange — готовим сущность со всеми заполненными полями
        var entity = new Counterparty
        {
            Id = 5,
            Name = "ООО Ромашка",
            Phone = "+79990000000",
            Email = "test@example.com"
        };

        // Act
        var dto = entity.ToDto();

        // Assert — каждое поле скопировано
        dto.Id.Should().Be(5);
        dto.Name.Should().Be("ООО Ромашка");
        dto.Phone.Should().Be("+79990000000");
        dto.Email.Should().Be("test@example.com");
    }

    /// <summary>
    /// Проверяет, что <c>CounterpartyRequestDto.ToEntity()</c> обрезает пробелы
    /// и нормализует пустые опциональные строки в <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Нормализация критична: в БД <c>NULL</c> и <c>""</c> — разные значения.
    /// Без нормализации в БД попали бы пустые строки, что усложняет фильтрацию.
    /// </remarks>
    [Fact]
    public void Counterparty_ToEntity_Should_Trim_And_Normalize()
    {
        // Arrange — специально даём пробелы и пустые строки
        var dto = new CounterpartyRequestDto
        {
            Name = "  ООО Тест  ",
            Phone = "   ",                              // только пробелы → должно стать null
            Email = "  mail@test.ru  "                  // пробелы по краям → обрезать
        };

        // Act
        var entity = dto.ToEntity();

        // Assert
        entity.Name.Should().Be("ООО Тест");            // Trim
        entity.Phone.Should().BeNull();                 // "" → null
        entity.Email.Should().Be("mail@test.ru");       // Trim
    }

    /// <summary>
    /// Проверяет, что <c>CounterpartyRequestDto.ApplyTo()</c> обновляет поля существующей сущности,
    /// но <b>не трогает</b> <c>Id</c>.
    /// </summary>
    /// <remarks>
    /// Критично для безопасности: если клиент пришлёт DTO с поддельным <c>Id</c>,
    /// <c>ApplyTo</c> не должен его применить — иначе возможен over-posting.
    /// </remarks>
    [Fact]
    public void Counterparty_ApplyTo_Should_Update_Fields_But_Not_Id()
    {
        // Arrange
        var entity = new Counterparty { Id = 10, Name = "Старое", Phone = "111" };
        var dto = new CounterpartyRequestDto { Name = "Новое", Phone = null, Email = "new@test" };

        // Act
        dto.ApplyTo(entity);

        // Assert
        entity.Id.Should().Be(10);                      // Id НЕ изменился
        entity.Name.Should().Be("Новое");
        entity.Phone.Should().BeNull();                 // было "111", стало null
        entity.Email.Should().Be("new@test");
    }

    // ---------- Product ----------

    /// <summary>
    /// Проверяет маппинг Product → DTO, включая разворачивание навигаций
    /// в плоские поля <c>CategoryName</c> / <c>UnitOfMeasureName</c>.
    /// </summary>
    /// <remarks>
    /// Разворачивание навигаций — ключевая логика: клиенту не нужно тащить
    /// весь объект <c>Category</c>, достаточно строки с именем.
    /// </remarks>
    [Fact]
    public void Product_ToDto_Should_Map_All_Fields_And_Flatten_Navigations()
    {
        // Arrange
        var entity = new Product
        {
            Id = 42,
            Name = "Телефон",
            Article = "TEL-01",
            CategoryId = 3,
            Category = new Category { Id = 3, Name = "Электроника" },
            UnitOfMeasureId = 1,
            UnitOfMeasure = new UnitOfMeasure { Id = 1, Name = "шт" },
            Quantity = 5.5m,
            PurchasePrice = 1000m,
            SalePrice = 1500m
        };

        // Act
        var dto = entity.ToDto();

        // Assert — все скалярные поля + плоские имена
        dto.Id.Should().Be(42);
        dto.Name.Should().Be("Телефон");
        dto.Article.Should().Be("TEL-01");
        dto.CategoryId.Should().Be(3);
        dto.CategoryName.Should().Be("Электроника");    // развёрнуто из Category.Name
        dto.UnitOfMeasureId.Should().Be(1);
        dto.UnitOfMeasureName.Should().Be("шт");        // развёрнуто из UnitOfMeasure.Name
        dto.Quantity.Should().Be(5.5m);
        dto.PurchasePrice.Should().Be(1000m);
        dto.SalePrice.Should().Be(1500m);
    }

    /// <summary>
    /// Проверяет, что если навигации не подгружены (<c>Category = null</c>),
    /// плоское поле <c>CategoryName</c> будет <c>null</c> (а не упадёт с NRE).
    /// </summary>
    /// <remarks>
    /// Это реальный сценарий: если забыть <c>Include(p =&gt; p.Category)</c> в репозитории,
    /// навигация останется <c>null</c>. Маппинг не должен падать — просто вернуть <c>null</c>.
    /// </remarks>
    [Fact]
    public void Product_ToDto_With_Null_Navigations_Should_Return_Null_Names()
    {
        var entity = new Product { Id = 1, Category = null, UnitOfMeasure = null };
        var dto = entity.ToDto();
        dto.CategoryName.Should().BeNull();
        dto.UnitOfMeasureName.Should().BeNull();
    }

    /// <summary>
    /// Проверяет, что <c>ProductRequestDto.ToEntity()</c> инициализирует
    /// <c>Quantity</c>, <c>PurchasePrice</c>, <c>SalePrice</c> нулями.
    /// </summary>
    /// <remarks>
    /// Эти поля не приходят от клиента при создании — они меняются только через документы.
    /// Инициализация нулём гарантирует, что новый товар создаётся с нулевым остатком.
    /// </remarks>
    [Fact]
    public void Product_ToEntity_Should_Initialize_Quantity_And_Prices_To_Zero()
    {
        var dto = new ProductRequestDto
        {
            Name = "  Товар  ",
            Article = "  A-01  ",
            CategoryId = 1,
            UnitOfMeasureId = 2
        };

        var entity = dto.ToEntity();

        // Строковые поля обрезаны
        entity.Name.Should().Be("Товар");
        entity.Article.Should().Be("A-01");
        // FK скопированы
        entity.CategoryId.Should().Be(1);
        entity.UnitOfMeasureId.Should().Be(2);
        // Цены и остаток — нули (не приходят от клиента)
        entity.Quantity.Should().Be(0m);
        entity.PurchasePrice.Should().Be(0m);
        entity.SalePrice.Should().Be(0m);
    }

    /// <summary>
    /// Проверяет, что <c>ApplyTo</c> для Product обновляет <b>только</b> редактируемые поля
    /// и не трогает <c>Quantity</c> / <c>PurchasePrice</c> / <c>SalePrice</c>.
    /// </summary>
    /// <remarks>
    /// Защита от over-posting: клиент не должен иметь возможности изменить
    /// остаток или цены через <c>PUT /api/Products/{id}</c>. Это делается только через документы.
    /// </remarks>
    [Fact]
    public void Product_ApplyTo_Should_Not_Modify_Quantity_And_Prices()
    {
        // Arrange — сущность с "финансовыми" значениями
        var entity = new Product
        {
            Id = 1,
            Name = "Старое",
            Article = "OLD",
            CategoryId = 1,
            UnitOfMeasureId = 1,
            Quantity = 10m,
            PurchasePrice = 500m,
            SalePrice = 800m
        };

        var dto = new ProductRequestDto
        {
            Name = "Новое",
            Article = "NEW",
            CategoryId = 2,
            UnitOfMeasureId = 3
        };

        // Act
        dto.ApplyTo(entity);

        // Assert — редактируемые поля обновились
        entity.Name.Should().Be("Новое");
        entity.Article.Should().Be("NEW");
        entity.CategoryId.Should().Be(2);
        entity.UnitOfMeasureId.Should().Be(3);

        // Assert — "финансовые" поля НЕ изменились (защита от over-posting)
        entity.Quantity.Should().Be(10m);
        entity.PurchasePrice.Should().Be(500m);
        entity.SalePrice.Should().Be(800m);
    }

    // ---------- WareDocument ----------

    /// <summary>
    /// Проверяет маппинг WareDocument → DTO, включая маппинг вложенной коллекции <c>Items</c>.
    /// </summary>
    /// <remarks>
    /// Сложный маппинг: проверяет вложенный <c>ToDto()</c> для каждого item'а,
    /// плюс вычисляемое свойство <c>Total</c> (Quantity * Price).
    /// </remarks>
    [Fact]
    public void WareDocument_ToDto_Should_Map_Items_And_Flat_Fields()
    {
        // Arrange — документ с одной строкой
        var entity = new WareDocument
        {
            Id = 5,
            Number = "DOC-1",
            Date = new DateTime(2024, 1, 15),
            Type = WareDocumentType.Receipt,
            CounterpartyId = 2,
            Counterparty = new Counterparty { Name = "Поставщик" },
            TotalAmount = 1500m,
            Items = new List<WareDocumentItem>
            {
                new()
                {
                    Id = 10,
                    ProductId = 1,
                    Product = new Product { Name = "Товар" },
                    Quantity = 3m,
                    Price = 500m
                }
            }
        };

        // Act
        var dto = entity.ToDto();

        // Assert — все поля документа
        dto.Id.Should().Be(5);
        dto.Number.Should().Be("DOC-1");
        dto.Type.Should().Be(WareDocumentType.Receipt);
        dto.CounterpartyId.Should().Be(2);
        dto.CounterpartyName.Should().Be("Поставщик");          // плоское поле из навигации
        dto.TotalAmount.Should().Be(1500m);

        // Assert — вложенные items
        dto.Items.Should().HaveCount(1);
        dto.Items[0].ProductName.Should().Be("Товар");          // плоское поле
        dto.Items[0].Total.Should().Be(1500m);                  // вычисляемое: Quantity * Price
    }
}
