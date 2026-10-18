using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace WarehouseApp.Domain.Entities
{
    public class Counterparty
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public ICollection<WareDocument> Documents { get; set; } = new List<WareDocument>();
    }
}
