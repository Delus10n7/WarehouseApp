using Microsoft.EntityFrameworkCore;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Infrastructure.Repository;

/// <summary>
/// Реализация <see cref="IWareDocumentRepository"/> на EF Core.
/// Управляет корректировкой остатков товаров при создании/редактировании/удалении.
/// </summary>
public class WareDocumentRepository : IWareDocumentRepository
{
    private readonly IDbContextFactory<WarehouseDbContext> _contextFactory;

    /// <summary>
    /// Инициализирует новый экземпляр репозитория документов.
    /// </summary>
    /// <param name="contextFactory">Фабрика контекстов БД.</param>
    public WareDocumentRepository(IDbContextFactory<WarehouseDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WareDocument>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Documents
            .Include(d => d.Counterparty)
            .Include(d => d.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .OrderByDescending(d => d.Date)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<WareDocument?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);
        return await context.Documents
            .Include(d => d.Counterparty)
            .Include(d => d.Items)
                .ThenInclude(i => i.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    /// <inheritdoc />
    public async Task<WareDocument> AddAsync(WareDocument document, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        context.Documents.Add(document);
        await ApplyStockChangesAsync(context, document, isReversal: false, ct);
        await context.SaveChangesAsync(ct);

        return document;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(WareDocument document, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var existing = await context.Documents
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == document.Id, ct);

        if (existing == null) return;

        // Отменяем влияние старого документа на остатки
        await ApplyStockChangesAsync(context, existing, isReversal: true, ct);

        existing.Number = document.Number;
        existing.Date = document.Date;
        existing.Type = document.Type;
        existing.CounterpartyId = document.CounterpartyId;
        existing.TotalAmount = document.TotalAmount;

        existing.Items.Clear();
        foreach (var item in document.Items)
        {
            existing.Items.Add(new WareDocumentItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            });
        }

        // Применяем влияние нового документа на остатки (с проверкой)
        await ApplyStockChangesAsync(context, existing, isReversal: false, ct);

        await context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(ct);

        var document = await context.Documents
            .Include(d => d.Items)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (document == null) return false;

        // Отменяем влияние документа на остатки (остатки возвращаются на склад)
        await ApplyStockChangesAsync(context, document, isReversal: true, ct);

        context.Documents.Remove(document);
        await context.SaveChangesAsync(ct);
        return true;
    }

    /// <summary>
    /// Изменяет остатки товаров на складе в зависимости от типа документа.
    /// </summary>
    /// <param name="context">Контекст БД.</param>
    /// <param name="document">Документ, влияние которого применяется.</param>
    /// <param name="isReversal">Если <c>true</c>, влияние отменяется (обратная операция).</param>
    /// <param name="ct">Токен отмены.</param>
    /// <exception cref="InvalidOperationException">Выбрасывается, если для продажи не хватает товара.</exception>
    private static async Task ApplyStockChangesAsync(
        WarehouseDbContext context,
        WareDocument document,
        bool isReversal,
        CancellationToken ct)
    {
        int multiplier = isReversal ? -1 : 1;
        int typeMultiplier = document.Type == WareDocumentType.Receipt ? 1 : -1;

        int finalMultiplier = multiplier * typeMultiplier;

        var productIds = document.Items.Select(i => i.ProductId).ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var item in document.Items)
        {
            if (products.TryGetValue(item.ProductId, out var product))
            {
                // ПРОВЕРКА: Если это продажа (и мы не отменяем документ), проверяем остаток
                if (document.Type == WareDocumentType.Sale && !isReversal)
                {
                    if (product.Quantity < item.Quantity)
                    {
                        // Форматируем числа с помощью "0.###" (убирает лишние нули)
                        var availableStr = product.Quantity.ToString("0.###");
                        var requestedStr = item.Quantity.ToString("0.###");

                        throw new InvalidOperationException(
                            $"Недостаточно товара «{product.Name}» на складе. Доступно: {availableStr}, запрошено: {requestedStr}.");
                    }
                }

                product.Quantity += (item.Quantity * finalMultiplier);
            }
        }
    }
}
