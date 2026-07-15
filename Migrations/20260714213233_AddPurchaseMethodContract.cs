using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QalatAldhaman.Store.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseMethodContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_pdf_url",
                table: "products");

            migrationBuilder.CreateTable(
                name: "purchase_method_contracts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    purchase_method = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    contract_pdf_url = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_purchase_method_contracts", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "purchase_method_contracts",
                columns: new[] { "id", "contract_pdf_url", "purchase_method", "updated_at" },
                values: new object[,]
                {
                    { 1, null, "Cash", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, null, "MonthlyInstallment", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, null, "MonthlyRafidain", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, null, "DailyInstallment", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "ix_purchase_method_contracts_purchase_method",
                table: "purchase_method_contracts",
                column: "purchase_method",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "purchase_method_contracts");

            migrationBuilder.AddColumn<string>(
                name: "contract_pdf_url",
                table: "products",
                type: "text",
                nullable: true);
        }
    }
}
