using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QalatAldhaman.Store.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRafidainProductPriceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "rafidain_payment_amount",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rafidain_total_price",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rafidain_payment_amount",
                table: "products");

            migrationBuilder.DropColumn(
                name: "rafidain_total_price",
                table: "products");
        }
    }
}
