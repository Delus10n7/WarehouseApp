using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для создания складского документа.
/// </summary>
public class WareDocumentCreateDto
{
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public WareDocumentType Type { get; set; }
    public int CounterpartyId { get; set; }

    public List<WareDocumentItemCreateDto> Items { get; set; } = new();
}
