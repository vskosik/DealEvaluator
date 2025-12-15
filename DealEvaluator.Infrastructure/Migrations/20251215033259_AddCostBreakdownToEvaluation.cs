using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCostBreakdownToEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentCommission",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyingClosingCosts",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContingencyBuffer",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InsuranceCost",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PropertyTaxesCost",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SellingClosingCosts",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UtilitiesCost",
                table: "Evaluations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentCommission",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "BuyingClosingCosts",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "ContingencyBuffer",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "InsuranceCost",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "PropertyTaxesCost",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "SellingClosingCosts",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "UtilitiesCost",
                table: "Evaluations");
        }
    }
}
