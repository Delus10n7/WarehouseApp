using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет складской документ (приходную или расходную накладную).
/// </summary>
public class WareDocument
{
    /// <summary>
    /// Уникальный идентификатор документа (первичный ключ).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Номер документа (например, "ПН-00123/2024").
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания или проведения документа.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Тип документа: поступление (Receipt) или продажа (Sale).
    /// </summary>
    public WareDocumentType Type { get; set; }

    /// <summary>
    /// Идентификатор контрагента, связанного с документом (внешний ключ).
    /// </summary>
    public int CounterpartyId { get; set; }

    /// <summary>
    /// Навигационное свойство контрагента.
    /// </summary>
    public Counterparty? Counterparty { get; set; }

    /// <summary>
    /// Общая сумма по документу.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Коллекция позиций (строк) документа (обратная навигация one-to-many).
    /// </summary>
    public ICollection<WareDocumentItem> Items { get; set; } = new List<WareDocumentItem>();
}
