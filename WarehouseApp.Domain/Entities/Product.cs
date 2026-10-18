using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;      // Артикул
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int UnitOfMeasureId { get; set; }
        public UnitOfMeasure? UnitOfMeasure { get; set; }
        public int Quantity { get; set; }                        // Остаток
        public decimal PurchasePrice { get; set; }               // Цена закупки
        public decimal SalePrice { get; set; }                   // Цена продажи
    }
}
