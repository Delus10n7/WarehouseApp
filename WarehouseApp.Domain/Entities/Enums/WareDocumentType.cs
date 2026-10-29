namespace WarehouseApp.Domain.Entities.Enums;

/// <summary>
/// Определяет тип складского документа.
/// </summary>
public enum WareDocumentType
{
    /// <summary>Поступление товара на склад (приходная накладная).</summary>
    Receipt = 1,

    /// <summary>Продажа / отгрузка товара со склада (расходная накладная).</summary>
    Sale = 2
}
