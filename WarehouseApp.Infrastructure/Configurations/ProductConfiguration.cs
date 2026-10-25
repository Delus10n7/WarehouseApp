using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations;

/// <summary>
/// Конфигурация сущности <see cref="Product"/> для Entity Framework Core.
/// Настраивает ограничения полей, индексы и связи.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Настраивает правила отображения сущности на таблицу базы данных.
    /// </summary>
    /// <param name="builder">Построитель для настройки сущности.</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Article).HasMaxLength(50).IsRequired();

        // Настройка точности для денежных цен
        builder.Property(p => p.PurchasePrice).HasPrecision(18, 2);
        builder.Property(p => p.SalePrice).HasPrecision(18, 2);

        // Артикул должен быть уникальным (не может быть двух товаров с одинаковым артикулом)
        builder.HasIndex(p => p.Article).IsUnique();

        // Связь с категорией. Запрещаем удалять категорию, если в ней есть товары.
        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
