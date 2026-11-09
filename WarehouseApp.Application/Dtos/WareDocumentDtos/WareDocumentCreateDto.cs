using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для создания складского документа.
/// </summary>
public class WareDocumentCreateDto
{
    /// <summary>Номер документа.</summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>Дата создания или проведения документа.</summary>
    public DateTime Date { get; set; }

    /// <summary>Тип документа (поступление или продажа).</summary>
    public WareDocumentType Type { get; set; }

    /// <summary>Идентификатор контрагента.</summary>
    public int CounterpartyId { get; set; }

    /// <summary>Коллекция строк документа.</summary>
    public List<WareDocumentItemCreateDto> Items { get; set; } = new();
}
