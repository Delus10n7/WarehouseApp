using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Domain.Entities;

/// <summary>
/// Представляет единицу измерения товара (шт, кг, л, упак и т.д.).
/// </summary>
public class UnitOfMeasure
{
    /// Уникальный идентификатор единицы измерения.
    public int Id { get; set; }

    /// Название или сокращение единицы измерения.
    public string Name { get; set; } = string.Empty;
}
