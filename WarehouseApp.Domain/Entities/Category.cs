using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет категорию товаров (например, "Электроника", "Продукты").
/// </summary>
public class Category
{
    /// Уникальный идентификатор категории.
    public int Id { get; set; }

    /// Название категории.
    public string Name { get; set; } = string.Empty;

    /// Коллекция товаров, относящихся к данной категории.
    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
