using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет складской документ (приходную или расходную накладную).
/// </summary>
public class WareDocument
{
    /// Уникальный идентификатор документа.
    public int Id { get; set; }

    /// Номер документа.
    public string Number { get; set; } = string.Empty;

    /// Дата создания или проведения документа.
    public DateTime Date { get; set; }

    /// Тип документа (поступление или продажа).
    public WareDocumentType Type { get; set; }

    /// Идентификатор контрагента, связанного с документом.
    public int CounterpartyId { get; set; }

    /// Навигационное свойство контрагента.
    public Counterparty? Counterparty { get; set; }

    /// Общая сумма по документу.
    public decimal TotalAmount { get; set; }

    /// Коллекция позиций (строк) документа.
    public ICollection<WareDocumentItem> Items { get; set; } = new List<WareDocumentItem>();
}
