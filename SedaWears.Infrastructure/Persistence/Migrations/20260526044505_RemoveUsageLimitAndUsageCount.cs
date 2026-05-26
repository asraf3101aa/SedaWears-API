using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SedaWears.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUsageLimitAndUsageCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsageCount",
                table: "PromoCodes");

            migrationBuilder.DropColumn(
                name: "UsageLimit",
                table: "PromoCodes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "PromoCodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsageLimit",
                table: "PromoCodes",
                type: "integer",
                nullable: true);
        }
    }
}
