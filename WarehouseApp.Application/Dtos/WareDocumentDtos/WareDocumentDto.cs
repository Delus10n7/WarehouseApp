using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для чтения складского документа.
/// </summary>
public class WareDocumentDto
{
    /// <summary>Уникальный идентификатор документа.</summary>
    public int Id { get; set; }

    /// <summary>Номер документа.</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Дата создания или проведения документа.</summary>
    public DateTime Date { get; set; }

    /// <summary>Тип документа.</summary>
    public WareDocumentType Type { get; set; }

    /// <summary>Идентификатор контрагента.</summary>
    public int CounterpartyId { get; set; }

    /// <summary>Название контрагента (плоское поле).</summary>
    public string? CounterpartyName { get; set; }

    /// <summary>Общая сумма по документу.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Коллекция строк документа.</summary>
    public List<WareDocumentItemDto> Items { get; set; } = new();
}
