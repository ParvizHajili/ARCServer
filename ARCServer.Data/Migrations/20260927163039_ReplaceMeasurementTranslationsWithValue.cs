using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARCServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMeasurementTranslationsWithValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Sizes",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Diameters",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Powers",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.Sql(@"
UPDATE s
SET s.[Value] = TRY_CONVERT(decimal(18,3), num.Token)
FROM Sizes s
INNER JOIN SizeTranslations t ON t.SizeId = s.Id AND t.LanguageCode = N'az' AND t.Deleted = 0
CROSS APPLY (
    SELECT REPLACE(REPLACE(t.Name, ',', '.'), ' ', '') AS Cleaned
) c
CROSS APPLY (
    SELECT PATINDEX('%[0-9]%', c.Cleaned) AS Pos
) p
CROSS APPLY (
    SELECT CASE WHEN p.Pos = 0 THEN NULL
        ELSE SUBSTRING(c.Cleaned, p.Pos, PATINDEX('%[^0-9.]%', SUBSTRING(c.Cleaned + 'x', p.Pos, 100)) - 1)
    END AS Token
) num;

UPDATE s SET s.[Value] = 1000000 + s.Id FROM Sizes s WHERE s.[Value] IS NULL;

;WITH ranked AS (
    SELECT Id, [Value], ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Sizes WHERE Deleted = 0
)
UPDATE ps
SET ps.Deleted = ps.Id, ps.DeletedDate = SYSUTCDATETIME()
FROM ProductSizes ps
INNER JOIN ranked dup ON dup.Id = ps.SizeId AND dup.rn > 1
INNER JOIN ranked keeper ON keeper.[Value] = dup.[Value] AND keeper.rn = 1
WHERE ps.Deleted = 0
  AND EXISTS (
      SELECT 1 FROM ProductSizes keep
      WHERE keep.ProductId = ps.ProductId AND keep.SizeId = keeper.Id AND keep.Deleted = 0
  );

;WITH ranked AS (
    SELECT Id, [Value], ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Sizes WHERE Deleted = 0
)
UPDATE ps
SET ps.SizeId = keeper.Id
FROM ProductSizes ps
INNER JOIN ranked dup ON dup.Id = ps.SizeId AND dup.rn > 1
INNER JOIN ranked keeper ON keeper.[Value] = dup.[Value] AND keeper.rn = 1
WHERE ps.Deleted = 0;

;WITH ranked AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Sizes WHERE Deleted = 0
)
UPDATE s
SET s.Deleted = s.Id, s.DeletedDate = SYSUTCDATETIME()
FROM Sizes s
INNER JOIN ranked d ON d.Id = s.Id AND d.rn > 1;

UPDATE d
SET d.[Value] = TRY_CONVERT(decimal(18,3), num.Token)
FROM Diameters d
INNER JOIN DiameterTranslations t ON t.DiameterId = d.Id AND t.LanguageCode = N'az' AND t.Deleted = 0
CROSS APPLY (SELECT REPLACE(REPLACE(t.Name, ',', '.'), ' ', '') AS Cleaned) c
CROSS APPLY (SELECT PATINDEX('%[0-9]%', c.Cleaned) AS Pos) p
CROSS APPLY (
    SELECT CASE WHEN p.Pos = 0 THEN NULL
        ELSE SUBSTRING(c.Cleaned, p.Pos, PATINDEX('%[^0-9.]%', SUBSTRING(c.Cleaned + 'x', p.Pos, 100)) - 1)
    END AS Token
) num;

UPDATE d SET d.[Value] = 1000000 + d.Id FROM Diameters d WHERE d.[Value] IS NULL;

;WITH ranked AS (
    SELECT Id, [Value], ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Diameters WHERE Deleted = 0
)
UPDATE ps
SET ps.Deleted = ps.Id, ps.DeletedDate = SYSUTCDATETIME()
FROM ProductDiameters ps
INNER JOIN ranked dup ON dup.Id = ps.DiameterId AND dup.rn > 1
INNER JOIN ranked keeper ON keeper.[Value] = dup.[Value] AND keeper.rn = 1
WHERE ps.Deleted = 0
  AND EXISTS (
      SELECT 1 FROM ProductDiameters keep
      WHERE keep.ProductId = ps.ProductId AND keep.DiameterId = keeper.Id AND keep.Deleted = 0
  );

;WITH ranked AS (
    SELECT Id, [Value], ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Diameters WHERE Deleted = 0
)
UPDATE ps
SET ps.DiameterId = keeper.Id
FROM ProductDiameters ps
INNER JOIN ranked dup ON dup.Id = ps.DiameterId AND dup.rn > 1
INNER JOIN ranked keeper ON keeper.[Value] = dup.[Value] AND keeper.rn = 1
WHERE ps.Deleted = 0;

;WITH ranked AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Diameters WHERE Deleted = 0
)
UPDATE d
SET d.Deleted = d.Id, d.DeletedDate = SYSUTCDATETIME()
FROM Diameters d
INNER JOIN ranked r ON r.Id = d.Id AND r.rn > 1;

UPDATE p
SET p.[Value] = TRY_CONVERT(decimal(18,3), num.Token)
FROM Powers p
INNER JOIN PowerTranslations t ON t.PowerId = p.Id AND t.LanguageCode = N'az' AND t.Deleted = 0
CROSS APPLY (SELECT REPLACE(REPLACE(t.Name, ',', '.'), ' ', '') AS Cleaned) c
CROSS APPLY (SELECT PATINDEX('%[0-9]%', c.Cleaned) AS Pos) ppos
CROSS APPLY (
    SELECT CASE WHEN ppos.Pos = 0 THEN NULL
        ELSE SUBSTRING(c.Cleaned, ppos.Pos, PATINDEX('%[^0-9.]%', SUBSTRING(c.Cleaned + 'x', ppos.Pos, 100)) - 1)
    END AS Token
) num;

UPDATE p SET p.[Value] = 1000000 + p.Id FROM Powers p WHERE p.[Value] IS NULL;

;WITH ranked AS (
    SELECT Id, [Value], ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Powers WHERE Deleted = 0
)
UPDATE ps
SET ps.Deleted = ps.Id, ps.DeletedDate = SYSUTCDATETIME()
FROM ProductPowers ps
INNER JOIN ranked dup ON dup.Id = ps.PowerId AND dup.rn > 1
INNER JOIN ranked keeper ON keeper.[Value] = dup.[Value] AND keeper.rn = 1
WHERE ps.Deleted = 0
  AND EXISTS (
      SELECT 1 FROM ProductPowers keep
      WHERE keep.ProductId = ps.ProductId AND keep.PowerId = keeper.Id AND keep.Deleted = 0
  );

;WITH ranked AS (
    SELECT Id, [Value], ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Powers WHERE Deleted = 0
)
UPDATE ps
SET ps.PowerId = keeper.Id
FROM ProductPowers ps
INNER JOIN ranked dup ON dup.Id = ps.PowerId AND dup.rn > 1
INNER JOIN ranked keeper ON keeper.[Value] = dup.[Value] AND keeper.rn = 1
WHERE ps.Deleted = 0;

;WITH ranked AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY [Value] ORDER BY Id) AS rn
    FROM Powers WHERE Deleted = 0
)
UPDATE p
SET p.Deleted = p.Id, p.DeletedDate = SYSUTCDATETIME()
FROM Powers p
INNER JOIN ranked r ON r.Id = p.Id AND r.rn > 1;
");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Sizes",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Diameters",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "Powers",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_Value",
                table: "Sizes",
                column: "Value",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Powers_Value",
                table: "Powers",
                column: "Value",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Diameters_Value",
                table: "Diameters",
                column: "Value",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.DropTable(
                name: "DiameterTranslations");

            migrationBuilder.DropTable(
                name: "PowerTranslations");

            migrationBuilder.DropTable(
                name: "SizeTranslations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sizes_Value",
                table: "Sizes");

            migrationBuilder.DropIndex(
                name: "IX_Powers_Value",
                table: "Powers");

            migrationBuilder.DropIndex(
                name: "IX_Diameters_Value",
                table: "Diameters");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Powers");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Diameters");

            migrationBuilder.CreateTable(
                name: "DiameterTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiameterId = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletorId = table.Column<int>(type: "int", nullable: true),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdaterId = table.Column<int>(type: "int", nullable: true)
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
                name: "PowerTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PowerId = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletorId = table.Column<int>(type: "int", nullable: true),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdaterId = table.Column<int>(type: "int", nullable: true)
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
                name: "SizeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SizeId = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<int>(type: "int", nullable: true),
                    Deleted = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletorId = table.Column<int>(type: "int", nullable: true),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdaterId = table.Column<int>(type: "int", nullable: true)
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
        }
    }
}
