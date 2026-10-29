using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations;

/// <summary>
/// Конфигурация <see cref="Product"/>: длины, точности, уникальный артикул,
/// связи с категорией и единицей измерения (обе — Restrict).
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Article).HasMaxLength(50).IsRequired();

        builder.Property(p => p.Quantity).HasPrecision(18, 3);
        builder.Property(p => p.PurchasePrice).HasPrecision(18, 2);
        builder.Property(p => p.SalePrice).HasPrecision(18, 2);

        builder.HasIndex(p => p.Article).IsUnique();

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.UnitOfMeasure)
               .WithMany()
               .HasForeignKey(p => p.UnitOfMeasureId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
