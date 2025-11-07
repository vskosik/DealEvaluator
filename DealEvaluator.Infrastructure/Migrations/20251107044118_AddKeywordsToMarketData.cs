using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddKeywordsToMarketData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Evaluations");

            migrationBuilder.AddColumn<string>(
                name: "Keywords",
                table: "MarketData",
                type: "nvarchar(450)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MarketData_ZipCode_Keywords",
                table: "MarketData",
                columns: new[] { "ZipCode", "Keywords" },
                unique: true,
                filter: "[Keywords] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MarketData_ZipCode_Keywords",
                table: "MarketData");

            migrationBuilder.DropColumn(
                name: "Keywords",
                table: "MarketData");

            migrationBuilder.AddColumn<int>(
                name: "PurchasePrice",
                table: "Evaluations",
                type: "int",
                nullable: true);
        }
    }
}
