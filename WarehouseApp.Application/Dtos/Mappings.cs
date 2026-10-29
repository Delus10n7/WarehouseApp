using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Dtos;

/// <summary>
/// Extension-методы для маппинга доменных сущностей в DTO и обратно.
/// </summary>
public static class Mappings
{
    // ---------- Counterparty ----------

    /// <summary>
    /// Преобразует сущность <see cref="Counterparty"/> в DTO для чтения.
    /// </summary>
    public static CounterpartyDto ToDto(this Counterparty e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Phone = e.Phone,
        Email = e.Email
    };

    /// <summary>
    /// Создаёт новую сущность <see cref="Counterparty"/> из DTO запроса.
    /// </summary>
    public static Counterparty ToEntity(this CounterpartyRequestDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Phone = Normalize(dto.Phone),
        Email = Normalize(dto.Email)
    };

    /// <summary>
    /// Применяет данные из DTO к существующей сущности (для update).
    /// </summary>
    public static void ApplyTo(this CounterpartyRequestDto dto, Counterparty entity)
    {
        entity.Name = dto.Name.Trim();
        entity.Phone = Normalize(dto.Phone);
        entity.Email = Normalize(dto.Email);
    }

    // ---------- Category ----------

    /// <summary>Преобразует <see cref="Category"/> в DTO.</summary>
    public static CategoryDto ToDto(this Category e) => new() { Id = e.Id, Name = e.Name };

    /// <summary>Создаёт <see cref="Category"/> из DTO создания.</summary>
    public static Category ToEntity(this CategoryCreateDto dto) => new() { Name = dto.Name.Trim() };

    // ---------- UnitOfMeasure ----------

    /// <summary>Преобразует <see cref="UnitOfMeasure"/> в DTO.</summary>
    public static UnitOfMeasureDto ToDto(this UnitOfMeasure e) => new() { Id = e.Id, Name = e.Name };

    /// <summary>Создаёт <see cref="UnitOfMeasure"/> из DTO создания.</summary>
    public static UnitOfMeasure ToEntity(this UnitOfMeasureCreateDto dto) => new() { Name = dto.Name.Trim() };

    // ---------- Product ----------

    /// <summary>
    /// Преобразует <see cref="Product"/> в DTO.
    /// </summary>
    public static ProductDto ToDto(this Product e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Article = e.Article,
        CategoryId = e.CategoryId,
        CategoryName = e.Category?.Name,
        UnitOfMeasureId = e.UnitOfMeasureId,
        UnitOfMeasureName = e.UnitOfMeasure?.Name,
        Quantity = e.Quantity,
        PurchasePrice = e.PurchasePrice,
        SalePrice = e.SalePrice
    };

    /// <summary>
    /// Создаёт <see cref="Product"/> из DTO запроса.
    /// </summary>
    public static Product ToEntity(this ProductRequestDto dto) => new()
    {
        Name = dto.Name.Trim(),
        Article = dto.Article.Trim(),
        CategoryId = dto.CategoryId,
        UnitOfMeasureId = dto.UnitOfMeasureId,
        Quantity = 0m,
        PurchasePrice = 0m,
        SalePrice = 0m
    };

    /// <summary>
    /// Применяет данные из DTO к существующему <see cref="Product"/>.
    /// </summary>
    public static void ApplyTo(this ProductRequestDto dto, Product entity)
    {
        entity.Name = dto.Name.Trim();
        entity.Article = dto.Article.Trim();
        entity.CategoryId = dto.CategoryId;
        entity.UnitOfMeasureId = dto.UnitOfMeasureId;
    }

    // ---------- WareDocument ----------

    /// <summary>
    /// Преобразует <see cref="WareDocument"/> в DTO для чтения.
    /// </summary>
    public static WareDocumentDto ToDto(this WareDocument e) => new()
    {
        Id = e.Id,
        Number = e.Number,
        Date = e.Date,
        Type = e.Type,
        CounterpartyId = e.CounterpartyId,
        CounterpartyName = e.Counterparty?.Name,
        TotalAmount = e.TotalAmount,
        Items = e.Items.Select(i => i.ToDto()).ToList()
    };

    /// <summary>
    /// Преобразует <see cref="WareDocumentItem"/> в DTO для чтения.
    /// </summary>
    public static WareDocumentItemDto ToDto(this WareDocumentItem e) => new()
    {
        Id = e.Id,
        ProductId = e.ProductId,
        ProductName = e.Product?.Name,
        Quantity = e.Quantity,
        Price = e.Price
    };

    /// <summary>
    /// Нормализует опциональную строку: пустая/пробельная → <c>null</c>, иначе — <c>Trim()</c>.
    /// </summary>
    private static string? Normalize(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
