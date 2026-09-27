using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARCServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtractProductMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_Diameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Powers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_Powers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
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
                    table.PrimaryKey("PK_Sizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiameterTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiameterId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_DiameterTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiameterTranslations_Diameters_DiameterId",
                        column: x => x.DiameterId,
                        principalTable: "Diameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductDiameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    DiameterId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ProductDiameters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDiameters_Diameters_DiameterId",
                        column: x => x.DiameterId,
                        principalTable: "Diameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductDiameters_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PowerTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PowerId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_PowerTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerTranslations_Powers_PowerId",
                        column: x => x.PowerId,
                        principalTable: "Powers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductPowers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PowerId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ProductPowers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPowers_Powers_PowerId",
                        column: x => x.PowerId,
                        principalTable: "Powers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductPowers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductSizes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SizeId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ProductSizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSizes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSizes_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SizeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SizeId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_SizeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SizeTranslations_Sizes_SizeId",
                        column: x => x.SizeId,
                        principalTable: "Sizes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diameters_Id_Deleted",
                table: "Diameters",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiameterTranslations_DiameterId_LanguageCode",
                table: "DiameterTranslations",
                columns: new[] { "DiameterId", "LanguageCode" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DiameterTranslations_Id_Deleted",
                table: "DiameterTranslations",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiameterTranslations_LanguageCode_Name",
                table: "DiameterTranslations",
                columns: new[] { "LanguageCode", "Name" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Powers_Id_Deleted",
                table: "Powers",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerTranslations_Id_Deleted",
                table: "PowerTranslations",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerTranslations_LanguageCode_Name",
                table: "PowerTranslations",
                columns: new[] { "LanguageCode", "Name" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PowerTranslations_PowerId_LanguageCode",
                table: "PowerTranslations",
                columns: new[] { "PowerId", "LanguageCode" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDiameters_DiameterId",
                table: "ProductDiameters",
                column: "DiameterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDiameters_Id_Deleted",
                table: "ProductDiameters",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductDiameters_ProductId_DiameterId",
                table: "ProductDiameters",
                columns: new[] { "ProductId", "DiameterId" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPowers_Id_Deleted",
                table: "ProductPowers",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductPowers_PowerId",
                table: "ProductPowers",
                column: "PowerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPowers_ProductId_PowerId",
                table: "ProductPowers",
                columns: new[] { "ProductId", "PowerId" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSizes_Id_Deleted",
                table: "ProductSizes",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductSizes_ProductId_SizeId",
                table: "ProductSizes",
                columns: new[] { "ProductId", "SizeId" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSizes_SizeId",
                table: "ProductSizes",
                column: "SizeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_Id_Deleted",
                table: "Sizes",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SizeTranslations_Id_Deleted",
                table: "SizeTranslations",
                columns: new[] { "Id", "Deleted" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SizeTranslations_LanguageCode_Name",
                table: "SizeTranslations",
                columns: new[] { "LanguageCode", "Name" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SizeTranslations_SizeId_LanguageCode",
                table: "SizeTranslations",
                columns: new[] { "SizeId", "LanguageCode" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.Sql(@"
CREATE TABLE #SizeMap (Name nvarchar(200) NOT NULL PRIMARY KEY, CatalogId int NOT NULL);
MERGE Sizes AS target
USING (
    SELECT DISTINCT LTRIM(RTRIM([Size])) AS Name
    FROM Products
    WHERE LTRIM(RTRIM([Size])) <> ''
) AS source
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (CreateDate, Deleted) VALUES (SYSUTCDATETIME(), 0)
OUTPUT source.Name, inserted.Id INTO #SizeMap (Name, CatalogId);
INSERT INTO SizeTranslations (SizeId, LanguageCode, Name, CreateDate, Deleted)
SELECT CatalogId, N'az', Name, SYSUTCDATETIME(), 0 FROM #SizeMap;
INSERT INTO ProductSizes (ProductId, SizeId, CreateDate, Deleted)
SELECT p.Id, m.CatalogId, SYSUTCDATETIME(), 0
FROM Products p
INNER JOIN #SizeMap m ON m.Name = LTRIM(RTRIM(p.[Size]))
WHERE LTRIM(RTRIM(p.[Size])) <> '';
DROP TABLE #SizeMap;

CREATE TABLE #DiameterMap (Name nvarchar(200) NOT NULL PRIMARY KEY, CatalogId int NOT NULL);
MERGE Diameters AS target
USING (
    SELECT DISTINCT LTRIM(RTRIM([Diameter])) AS Name
    FROM Products
    WHERE LTRIM(RTRIM([Diameter])) <> ''
) AS source
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (CreateDate, Deleted) VALUES (SYSUTCDATETIME(), 0)
OUTPUT source.Name, inserted.Id INTO #DiameterMap (Name, CatalogId);
INSERT INTO DiameterTranslations (DiameterId, LanguageCode, Name, CreateDate, Deleted)
SELECT CatalogId, N'az', Name, SYSUTCDATETIME(), 0 FROM #DiameterMap;
INSERT INTO ProductDiameters (ProductId, DiameterId, CreateDate, Deleted)
SELECT p.Id, m.CatalogId, SYSUTCDATETIME(), 0
FROM Products p
INNER JOIN #DiameterMap m ON m.Name = LTRIM(RTRIM(p.[Diameter]))
WHERE LTRIM(RTRIM(p.[Diameter])) <> '';
DROP TABLE #DiameterMap;

CREATE TABLE #PowerMap (Name nvarchar(200) NOT NULL PRIMARY KEY, CatalogId int NOT NULL);
MERGE Powers AS target
USING (
    SELECT DISTINCT FORMAT(PowerAmperes, '0.###') AS Name
    FROM Products
) AS source
ON 1 = 0
WHEN NOT MATCHED THEN
    INSERT (CreateDate, Deleted) VALUES (SYSUTCDATETIME(), 0)
OUTPUT source.Name, inserted.Id INTO #PowerMap (Name, CatalogId);
INSERT INTO PowerTranslations (PowerId, LanguageCode, Name, CreateDate, Deleted)
SELECT CatalogId, N'az', Name, SYSUTCDATETIME(), 0 FROM #PowerMap;
INSERT INTO ProductPowers (ProductId, PowerId, CreateDate, Deleted)
SELECT p.Id, m.CatalogId, SYSUTCDATETIME(), 0
FROM Products p
INNER JOIN #PowerMap m ON m.Name = FORMAT(p.PowerAmperes, '0.###');
DROP TABLE #PowerMap;
");

            migrationBuilder.DropColumn(
                name: "Diameter",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PowerAmperes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiameterTranslations");

            migrationBuilder.DropTable(
                name: "PowerTranslations");

            migrationBuilder.DropTable(
                name: "ProductDiameters");

            migrationBuilder.DropTable(
                name: "ProductPowers");

            migrationBuilder.DropTable(
                name: "ProductSizes");

            migrationBuilder.DropTable(
                name: "SizeTranslations");

            migrationBuilder.DropTable(
                name: "Diameters");

            migrationBuilder.DropTable(
                name: "Powers");

            migrationBuilder.DropTable(
                name: "Sizes");

            migrationBuilder.AddColumn<string>(
                name: "Diameter",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PowerAmperes",
                table: "Products",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
