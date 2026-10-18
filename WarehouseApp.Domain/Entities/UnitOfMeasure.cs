using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Domain.Entities
{
    public class UnitOfMeasure
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;  // шт, кг, л, упак
    }
}
