using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddYearBuiltLotSizeHomeStatusToCachedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HomeStatus",
                table: "CachedProperties",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LotSizeSqft",
                table: "CachedProperties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearBuilt",
                table: "CachedProperties",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeStatus",
                table: "CachedProperties");

            migrationBuilder.DropColumn(
                name: "LotSizeSqft",
                table: "CachedProperties");

            migrationBuilder.DropColumn(
                name: "YearBuilt",
                table: "CachedProperties");
        }
    }
}
