using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QalatAldhaman.Store.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDownPaymentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "monthly_down_payment",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rafidain_down_payment",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "monthly_down_payment",
                table: "products");

            migrationBuilder.DropColumn(
                name: "rafidain_down_payment",
                table: "products");
        }
    }
}
