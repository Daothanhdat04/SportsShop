using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SportsShop.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "ProductVariants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPrice",
                table: "ProductVariants",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercent",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountStartDate",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "DiscountPercent", "DiscountPrice" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "DiscountEndDate", "DiscountPercent", "DiscountPrice", "DiscountStartDate" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "DiscountEndDate", "DiscountPercent", "DiscountPrice", "DiscountStartDate" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "DiscountEndDate", "DiscountPercent", "DiscountPrice", "DiscountStartDate" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "DiscountEndDate",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountStartDate",
                table: "Products");
        }
    }
}
