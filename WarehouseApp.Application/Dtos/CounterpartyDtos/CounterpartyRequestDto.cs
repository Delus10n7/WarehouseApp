namespace WarehouseApp.Application.Dtos.CounterpartyDtos;

/// <summary>
/// DTO для создания или обновления контрагента.
/// </summary>
public class CounterpartyRequestDto
{
    /// <summary>Наименование организации или ФИО контрагента.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Контактный телефон (необязательное поле).</summary>
    public string? Phone { get; set; }

    /// <summary>Электронная почта (необязательное поле).</summary>
    public string? Email { get; set; }
}
