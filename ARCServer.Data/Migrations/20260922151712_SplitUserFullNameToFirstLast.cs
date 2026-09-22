using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ARCServer.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitUserFullNameToFirstLast : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "Membership",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "Membership",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Membership].[Users]
                SET
                    [FirstName] = CASE
                        WHEN CHARINDEX(' ', LTRIM(RTRIM([FullName]))) > 0
                            THEN LEFT(LTRIM(RTRIM([FullName])), CHARINDEX(' ', LTRIM(RTRIM([FullName]))) - 1)
                        ELSE LTRIM(RTRIM([FullName]))
                    END,
                    [LastName] = CASE
                        WHEN CHARINDEX(' ', LTRIM(RTRIM([FullName]))) > 0
                            THEN LTRIM(SUBSTRING(LTRIM(RTRIM([FullName])), CHARINDEX(' ', LTRIM(RTRIM([FullName]))) + 1, 4000))
                        ELSE N''
                    END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                schema: "Membership",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                schema: "Membership",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "Membership",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "Membership",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Membership].[Users]
                SET [FullName] = LTRIM(RTRIM(CONCAT([FirstName], N' ', [LastName])));
                """);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "Membership",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "Membership",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "Membership",
                table: "Users");
        }
    }
}
