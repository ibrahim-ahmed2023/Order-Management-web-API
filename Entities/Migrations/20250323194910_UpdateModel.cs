using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: new Guid("24d71ac2-0a9c-4914-9fd3-13bc25d42694"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: new Guid("2e27b6a4-469d-4d7f-8b8b-54af129675fd"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: new Guid("ac90b8bc-349d-43fd-87a6-6a7ed8057697"));

            migrationBuilder.DeleteData(
                table: "OrderItems",
                keyColumn: "OrderItemId",
                keyValue: new Guid("d20882df-7fca-4ee8-88bb-37d2fc75e63f"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "OrderItems",
                columns: new[] { "OrderItemId", "OrderId", "ProductName", "Quantity", "TotalPrice", "UnitPrice" },
                values: new object[,]
                {
                    { new Guid("24d71ac2-0a9c-4914-9fd3-13bc25d42694"), new Guid("735886c0-faf3-49ca-9776-8a20b756f1cb"), "Product C", 7, 25.00m, 25.40m },
                    { new Guid("2e27b6a4-469d-4d7f-8b8b-54af129675fd"), new Guid("f4816224-70d6-4491-ac52-34f298ace16f"), "Product B", 3, 46.50m, 15.50m },
                    { new Guid("ac90b8bc-349d-43fd-87a6-6a7ed8057697"), new Guid("735886c0-faf3-49ca-9776-8a20b756f1cb"), "Product D", 4, 25.00m, 12.00m },
                    { new Guid("d20882df-7fca-4ee8-88bb-37d2fc75e63f"), new Guid("f4816224-70d6-4491-ac52-34f298ace16f"), "Product A", 2, 20.00m, 10.00m }
                });
        }
    }
}
