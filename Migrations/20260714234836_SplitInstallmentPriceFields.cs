using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QalatAldhaman.Store.Api.Migrations
{
    /// <inheritdoc />
    public partial class SplitInstallmentPriceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "monthly_installment_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "daily_installment_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "price_snapshot",
                table: "orders");

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_total_price",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_payment_amount",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_total_price",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_payment_amount",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_price_snapshot",
                table: "orders",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "installment_payment_amount_snapshot",
                table: "orders",
                type: "numeric(12,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "monthly_total_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "monthly_payment_amount",
                table: "products");

            migrationBuilder.DropColumn(
                name: "daily_total_price",
                table: "products");

            migrationBuilder.DropColumn(
                name: "daily_payment_amount",
                table: "products");

            migrationBuilder.DropColumn(
                name: "total_price_snapshot",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "installment_payment_amount_snapshot",
                table: "orders");

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_installment_price",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_installment_price",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price_snapshot",
                table: "orders",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
