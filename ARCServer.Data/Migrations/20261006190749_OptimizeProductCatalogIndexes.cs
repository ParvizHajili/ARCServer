using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARCServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeProductCatalogIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_Id",
                table: "Products",
                columns: new[] { "CategoryId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubCategoryId_Id",
                table: "Products",
                columns: new[] { "SubCategoryId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_HasWarranty_Id",
                table: "Products",
                columns: new[] { "HasWarranty", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsMadeToOrder_Id",
                table: "Products",
                columns: new[] { "IsMadeToOrder", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_ProductColorId_Order",
                table: "ProductImages",
                columns: new[] { "ProductId", "ProductColorId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubCategoryId_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_HasWarranty_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsMadeToOrder_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_ProductColorId_Order",
                table: "ProductImages");
        }
    }
}
