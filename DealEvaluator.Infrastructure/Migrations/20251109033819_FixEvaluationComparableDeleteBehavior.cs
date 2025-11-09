using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class FixEvaluationComparableDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationComparable_Comparables_ComparableId",
                table: "EvaluationComparable");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationComparable_Comparables_ComparableId",
                table: "EvaluationComparable",
                column: "ComparableId",
                principalTable: "Comparables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvaluationComparable_Comparables_ComparableId",
                table: "EvaluationComparable");

            migrationBuilder.AddForeignKey(
                name: "FK_EvaluationComparable_Comparables_ComparableId",
                table: "EvaluationComparable",
                column: "ComparableId",
                principalTable: "Comparables",
                principalColumn: "Id");
        }
    }
}
