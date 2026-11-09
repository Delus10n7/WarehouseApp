namespace WarehouseApp.Application.Dtos.ProductDtos;

/// <summary>
/// DTO для создания или обновления товара.
/// </summary>
public class ProductRequestDto
{
    /// <summary>Наименование товара.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Артикул (уникальный код товара).</summary>
    public string Article { get; set; } = string.Empty;

    /// <summary>Идентификатор категории.</summary>
    public int CategoryId { get; set; }

    /// <summary>Идентификатор единицы измерения.</summary>
    public int UnitOfMeasureId { get; set; }
}
