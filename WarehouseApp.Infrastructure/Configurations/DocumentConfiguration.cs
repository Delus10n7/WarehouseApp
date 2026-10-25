using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="WareDocument"/> для Entity Framework Core.
/// Настраивает ограничения полей и связи с другими таблицами.
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<WareDocument>
{
    /// <summary>
    /// Настраивает правила отображения сущности на таблицу базы данных.
    /// </summary>
    /// <param name="builder">Построитель для настройки сущности.</param>
    public void Configure(EntityTypeBuilder<WareDocument> builder)
    {
        // Ограничение длины номера документа и обязательность заполнения
        builder.Property(d => d.Number).HasMaxLength(50).IsRequired();
        // Настройка точности для денежной суммы (всего 18 цифр, из них 2 после запятой)
        builder.Property(d => d.TotalAmount).HasPrecision(18, 2);

        // Настройка связи один-ко-многим: Один контрагент -> Много документов.
        // Restrict запрещает удаление контрагента, если у него есть документы.
        builder.HasOne(d => d.Counterparty)
               .WithMany(c => c.Documents)
               .HasForeignKey(d => d.CounterpartyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
