using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseApp.Migrations.Migrations;

/// <inheritdoc />
public partial class FixSchemaConstraintsAndIndexes : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DocumentItems_Products_ProductId",
            table: "DocumentItems");

        migrationBuilder.DropForeignKey(
            name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
            table: "Products");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "UnitsOfMeasure",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<decimal>(
            name: "Quantity",
            table: "Products",
            type: "decimal(18,3)",
            precision: 18,
            scale: 3,
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "Phone",
            table: "Counterparties",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Counterparties",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Counterparties",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Categories",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_UnitsOfMeasure_Name",
            table: "UnitsOfMeasure",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Documents_Date",
            table: "Documents",
            column: "Date");

        migrationBuilder.CreateIndex(
            name: "IX_Categories_Name",
            table: "Categories",
            column: "Name",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_DocumentItems_Products_ProductId",
            table: "DocumentItems",
            column: "ProductId",
            principalTable: "Products",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
            table: "Products",
            column: "UnitOfMeasureId",
            principalTable: "UnitsOfMeasure",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_DocumentItems_Products_ProductId",
            table: "DocumentItems");

        migrationBuilder.DropForeignKey(
            name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
            table: "Products");

        migrationBuilder.DropIndex(
            name: "IX_UnitsOfMeasure_Name",
            table: "UnitsOfMeasure");

        migrationBuilder.DropIndex(
            name: "IX_Documents_Date",
            table: "Documents");

        migrationBuilder.DropIndex(
            name: "IX_Categories_Name",
            table: "Categories");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "UnitsOfMeasure",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<int>(
            name: "Quantity",
            table: "Products",
            type: "int",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(18,3)",
            oldPrecision: 18,
            oldScale: 3);

        migrationBuilder.AlterColumn<string>(
            name: "Phone",
            table: "Counterparties",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Counterparties",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "Email",
            table: "Counterparties",
            type: "nvarchar(max)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Categories",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AddForeignKey(
            name: "FK_DocumentItems_Products_ProductId",
            table: "DocumentItems",
            column: "ProductId",
            principalTable: "Products",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
            table: "Products",
            column: "UnitOfMeasureId",
            principalTable: "UnitsOfMeasure",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
