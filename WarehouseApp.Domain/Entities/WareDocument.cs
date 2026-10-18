using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Domain.Entities
{
    public enum WareDocumentType
    {
        Receipt = 1,   // Поступление
        Sale = 2       // Продажа
    }

    public class WareDocument
    {
        public int Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public WareDocumentType Type { get; set; }
        public int CounterpartyId { get; set; }
        public Counterparty? Counterparty { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<WareDocumentItem> Items { get; set; } = new List<WareDocumentItem>();
    }
}
