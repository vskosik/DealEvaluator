using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class DealSettingsVariableTypeUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "SellingClosingCosts",
                table: "DealSettings",
                type: "float(5)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "SellingAgentCommission",
                table: "DealSettings",
                type: "float(5)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "ProfitTargetValue",
                table: "DealSettings",
                type: "float(10)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,4)",
                oldPrecision: 10,
                oldScale: 4);

            migrationBuilder.AlterColumn<int>(
                name: "MonthlyUtilities",
                table: "DealSettings",
                type: "int",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "MonthlyInsurance",
                table: "DealSettings",
                type: "int",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<double>(
                name: "ContingencyPercentage",
                table: "DealSettings",
                type: "float(5)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "BuyingClosingCosts",
                table: "DealSettings",
                type: "float(5)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<double>(
                name: "AnnualPropertyTaxRate",
                table: "DealSettings",
                type: "float(5)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,4)",
                oldPrecision: 5,
                oldScale: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "SellingClosingCosts",
                table: "DealSettings",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(5)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "SellingAgentCommission",
                table: "DealSettings",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(5)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "ProfitTargetValue",
                table: "DealSettings",
                type: "decimal(10,4)",
                precision: 10,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyUtilities",
                table: "DealSettings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "MonthlyInsurance",
                table: "DealSettings",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "ContingencyPercentage",
                table: "DealSettings",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(5)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "BuyingClosingCosts",
                table: "DealSettings",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(5)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "AnnualPropertyTaxRate",
                table: "DealSettings",
                type: "decimal(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(5)",
                oldPrecision: 5,
                oldScale: 4);
        }
    }
}
