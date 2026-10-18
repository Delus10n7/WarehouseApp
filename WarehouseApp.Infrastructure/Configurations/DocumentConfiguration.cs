using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Infrastructure.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<WareDocument>
    {
        public void Configure(EntityTypeBuilder<WareDocument> builder)
        {
            builder.Property(d => d.Number).HasMaxLength(50).IsRequired();
            builder.Property(d => d.TotalAmount).HasPrecision(18, 2);

            builder.HasOne(d => d.Counterparty)
                   .WithMany(c => c.Documents)
                   .HasForeignKey(d => d.CounterpartyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
