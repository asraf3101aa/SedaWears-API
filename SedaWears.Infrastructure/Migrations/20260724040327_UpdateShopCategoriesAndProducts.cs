using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SedaWears.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateShopCategoriesAndProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shops_Name",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_SubdomainSlug",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ShopId_Name",
                table: "Categories");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Shops",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_Name",
                table: "Shops",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_SubdomainSlug",
                table: "Shops",
                column: "SubdomainSlug",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ShopId_Name",
                table: "Categories",
                columns: new[] { "ShopId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shops_Name",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_SubdomainSlug",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ShopId_Name",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_Name",
                table: "Shops",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_SubdomainSlug",
                table: "Shops",
                column: "SubdomainSlug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ShopId_Name",
                table: "Categories",
                columns: new[] { "ShopId", "Name" },
                unique: true);
        }
    }
}
