namespace WarehouseApp.Application.Dtos.CounterpartyDtos;

/// <summary>
/// DTO для создания или обновления контрагента.
/// </summary>
public class CounterpartyRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
