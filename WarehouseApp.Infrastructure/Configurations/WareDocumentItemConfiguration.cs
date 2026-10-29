using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations;

/// <summary>
/// Конфигурация <see cref="WareDocumentItem"/>: точности, каскад по документу,
/// запрет каскада по товару.
/// </summary>
public class WareDocumentItemConfiguration : IEntityTypeConfiguration<WareDocumentItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WareDocumentItem> builder)
    {
        builder.Property(i => i.Quantity).HasPrecision(18, 3);
        builder.Property(i => i.Price).HasPrecision(18, 2);

        builder.HasOne(i => i.Document)
               .WithMany(d => d.Items)
               .HasForeignKey(i => i.DocumentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Product)
               .WithMany()
               .HasForeignKey(i => i.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
