using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QalatAldhaman.Store.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagePriceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "cash_price",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_payment_amount",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "daily_total_price",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_down_payment",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_payment_amount",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "monthly_total_price",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rafidain_down_payment",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rafidain_payment_amount",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rafidain_total_price",
                table: "packages",
                type: "numeric(12,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cash_price",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "daily_payment_amount",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "daily_total_price",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "monthly_down_payment",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "monthly_payment_amount",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "monthly_total_price",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "rafidain_down_payment",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "rafidain_payment_amount",
                table: "packages");

            migrationBuilder.DropColumn(
                name: "rafidain_total_price",
                table: "packages");
        }
    }
}
