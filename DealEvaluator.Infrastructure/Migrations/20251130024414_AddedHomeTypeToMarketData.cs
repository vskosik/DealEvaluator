using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedHomeTypeToMarketData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarketData_ZipCode_Keywords",
                table: "MarketData");

            migrationBuilder.AddColumn<string>(
                name: "HomeType",
                table: "MarketData",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MarketData_ZipCode_HomeType_Keywords",
                table: "MarketData",
                columns: new[] { "ZipCode", "HomeType", "Keywords" },
                unique: true,
                filter: "[Keywords] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarketData_ZipCode_HomeType_Keywords",
                table: "MarketData");

            migrationBuilder.DropColumn(
                name: "HomeType",
                table: "MarketData");

            migrationBuilder.CreateIndex(
                name: "IX_MarketData_ZipCode_Keywords",
                table: "MarketData",
                columns: new[] { "ZipCode", "Keywords" },
                unique: true,
                filter: "[Keywords] IS NOT NULL");
        }
    }
}
