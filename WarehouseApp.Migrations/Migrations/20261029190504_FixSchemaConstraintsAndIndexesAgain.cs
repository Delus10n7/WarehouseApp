using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseApp.Migrations.Migrations;

/// <inheritdoc />
public partial class FixSchemaConstraintsAndIndexesAgain : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "Quantity",
            table: "DocumentItems",
            type: "decimal(18,3)",
            precision: 18,
            scale: 3,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Quantity",
            table: "DocumentItems",
            type: "int",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,3)",
            oldPrecision: 18,
            oldScale: 3);
    }
}
