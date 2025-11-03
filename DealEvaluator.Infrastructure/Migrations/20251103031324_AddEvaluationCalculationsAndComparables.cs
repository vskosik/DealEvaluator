using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEvaluationCalculationsAndComparables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxOffer",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Profit",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Roi",
                table: "Evaluations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EvaluationComparable",
                columns: table => new
                {
                    ComparableId = table.Column<int>(type: "int", nullable: false),
                    EvaluationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationComparable", x => new { x.ComparableId, x.EvaluationId });
                    table.ForeignKey(
                        name: "FK_EvaluationComparable_Comparables_ComparableId",
                        column: x => x.ComparableId,
                        principalTable: "Comparables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EvaluationComparable_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationComparable_EvaluationId",
                table: "EvaluationComparable",
                column: "EvaluationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationComparable");

            migrationBuilder.DropColumn(
                name: "MaxOffer",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "Profit",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "Roi",
                table: "Evaluations");
        }
    }
}
