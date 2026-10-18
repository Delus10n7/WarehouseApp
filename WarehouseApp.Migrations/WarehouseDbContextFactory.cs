using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Infrastructure;

namespace WarehouseApp.Migrations
{
    public class WarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
    {
        public WarehouseDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost\\SQLEXPRESS; Database=WarehouseDb; Trusted_Connection=True; TrustServerCertificate=True;",
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(WarehouseDbContextFactory).Assembly.GetName().Name));

            return new WarehouseDbContext(optionsBuilder.Options);
        }
    }
}
