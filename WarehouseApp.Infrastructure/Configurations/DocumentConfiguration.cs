using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="WareDocument"/> для Entity Framework Core.
/// Настраивает ограничения полей, индекс по дате и связь с контрагентом.
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<WareDocument>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<WareDocument> builder)
    {
        builder.Property(d => d.Number).HasMaxLength(50).IsRequired();

        // Деньги: 18 цифр всего, 2 после запятой.
        builder.Property(d => d.TotalAmount).HasPrecision(18, 2);

        // Один контрагент → много документов. Restrict запрещает удалять контрагента,
        // у которого есть документы (защита финансовой истории).
        builder.HasOne(d => d.Counterparty)
               .WithMany(c => c.Documents)
               .HasForeignKey(d => d.CounterpartyId)
               .OnDelete(DeleteBehavior.Restrict);

        // Индекс по дате — ускоряет отчёты за период.
        builder.HasIndex(d => d.Date);
    }
}
