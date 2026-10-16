using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARCServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProduct360Videos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Product360Videos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdaterId = table.Column<int>(type: "int", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletorId = table.Column<int>(type: "int", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product360Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product360Videos_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Product360Videos_Id_Deleted",
                table: "Product360Videos",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product360Videos_ProductId_Order",
                table: "Product360Videos",
                columns: new[] { "ProductId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Product360Videos");
        }
    }
}
