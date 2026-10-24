using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace WarehouseApp.Domain.Entities;

public class WareDocumentItem
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public WareDocument? Document { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
