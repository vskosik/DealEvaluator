using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeCachedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RawJson",
                table: "MarketData");

            migrationBuilder.CreateTable(
                name: "CachedProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MarketDataId = table.Column<int>(type: "int", nullable: false),
                    Zpid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PropertyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<float>(type: "real", nullable: true),
                    LivingArea = table.Column<int>(type: "int", nullable: true),
                    DetailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ListingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Zestimate = table.Column<int>(type: "int", nullable: true),
                    DaysOnZillow = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<int>(type: "int", nullable: true),
                    DateSoldTimestamp = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CachedProperties_MarketData_MarketDataId",
                        column: x => x.MarketDataId,
                        principalTable: "MarketData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CachedProperties_MarketDataId",
                table: "CachedProperties",
                column: "MarketDataId");

            migrationBuilder.CreateIndex(
                name: "IX_CachedProperties_MarketDataId_Zpid",
                table: "CachedProperties",
                columns: new[] { "MarketDataId", "Zpid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedProperties");

            migrationBuilder.AddColumn<string>(
                name: "RawJson",
                table: "MarketData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
