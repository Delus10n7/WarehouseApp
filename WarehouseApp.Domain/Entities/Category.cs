namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет категорию товаров (например, "Электроника", "Продукты").
/// </summary>
public class Category
{
    /// <summary>
    /// Уникальный идентификатор категории (первичный ключ).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название категории. Обязательное поле, не может быть пустым.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Коллекция товаров, относящихся к данной категории (обратная навигация one-to-many).
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
