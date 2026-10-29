using System.Text.Json.Serialization;

namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет контрагента (поставщика или покупателя).
/// </summary>
public class Counterparty
{
    /// Уникальный идентификатор контрагента.
    public int Id { get; set; }

    /// Наименование организации или ФИО контрагента.
    public string Name { get; set; } = string.Empty;

    /// Контактный телефон (необязательное поле).
    public string? Phone { get; set; }

    /// Электронная почта (необязательное поле).
    public string? Email { get; set; }

    /// Коллекция документов, связанных с данным контрагентом.
    [JsonIgnore]
    public ICollection<WareDocument> Documents { get; set; } = new List<WareDocument>();
}
