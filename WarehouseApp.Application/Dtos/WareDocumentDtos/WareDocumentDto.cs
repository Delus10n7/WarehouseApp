using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Application.Dtos.WareDocumentDtos;

/// <summary>
/// DTO для чтения складского документа.
/// </summary>
public class WareDocumentDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public WareDocumentType Type { get; set; }

    public int CounterpartyId { get; set; }
    public string? CounterpartyName { get; set; }

    public decimal TotalAmount { get; set; }

    public List<WareDocumentItemDto> Items { get; set; } = new();
}
