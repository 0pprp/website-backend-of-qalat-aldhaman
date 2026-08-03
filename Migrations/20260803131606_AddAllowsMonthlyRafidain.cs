using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QalatAldhaman.Store.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowsMonthlyRafidain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "allows_monthly_rafidain",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // الفئات الموجودة كانت تعتمد ضمنياً على ظهور قسط الرافدين تلقائياً مع القسط الشهري العادي
            // (حقل واحد يتحكم بالاثنين سابقاً) — ننسخ القيمة الحالية مرة واحدة وقت الترحيل فقط، حتى لا
            // يختفي خيار الرافدين فجأة من أي فئة كانت تعرضه، والأدمن يقدر يفصلهما يدوياً بعدين لو أراد.
            migrationBuilder.Sql(
                "UPDATE categories SET allows_monthly_rafidain = allows_monthly_installment;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "allows_monthly_rafidain",
                table: "categories");
        }
    }
}
