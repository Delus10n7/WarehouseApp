using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations;

/// <summary>
/// Конфигурация <see cref="Counterparty"/>: длины строк для обязательных и опциональных полей.
/// </summary>
public class CounterpartyConfiguration : IEntityTypeConfiguration<Counterparty>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Counterparty> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(100);
    }
}
