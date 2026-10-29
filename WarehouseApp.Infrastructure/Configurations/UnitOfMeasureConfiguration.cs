using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations;

/// <summary>
/// Конфигурация <see cref="UnitOfMeasure"/>: длина и уникальность имени.
/// </summary>
public class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.Property(u => u.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.Name).IsUnique();
    }
}
