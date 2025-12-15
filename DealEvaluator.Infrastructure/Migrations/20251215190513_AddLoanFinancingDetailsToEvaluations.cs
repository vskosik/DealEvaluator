using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanFinancingDetailsToEvaluations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownPayment",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoanAmount",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyPayment",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalFinancingCosts",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalInterest",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DefaultLoanRate",
                table: "DealSettings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DownPaymentPercentage",
                table: "DealSettings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownPayment",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "LoanAmount",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "MonthlyPayment",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "TotalFinancingCosts",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "TotalInterest",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "DefaultLoanRate",
                table: "DealSettings");

            migrationBuilder.DropColumn(
                name: "DownPaymentPercentage",
                table: "DealSettings");
        }
    }
}
