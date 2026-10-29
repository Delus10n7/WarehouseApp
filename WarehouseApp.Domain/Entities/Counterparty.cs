namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет контрагента (поставщика или покупателя).
/// </summary>
public class Counterparty
{
    /// <summary>
    /// Уникальный идентификатор контрагента (первичный ключ).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Наименование организации или ФИО контрагента.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Контактный телефон
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Электронная почта
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Коллекция документов, связанных с данным контрагентом (обратная навигация one-to-many).
    /// </summary>
    public ICollection<WareDocument> Documents { get; set; } = new List<WareDocument>();
}
