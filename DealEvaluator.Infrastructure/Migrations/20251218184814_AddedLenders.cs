using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedLenders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LenderId",
                table: "Evaluations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultLenderId",
                table: "DealSettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Lenders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AnnualRate = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: false),
                    OriginationFee = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: false),
                    LoanServiceFee = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lenders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lenders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_LenderId",
                table: "Evaluations",
                column: "LenderId");

            migrationBuilder.CreateIndex(
                name: "IX_DealSettings_DefaultLenderId",
                table: "DealSettings",
                column: "DefaultLenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Lenders_UserId",
                table: "Lenders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DealSettings_Lenders_DefaultLenderId",
                table: "DealSettings",
                column: "DefaultLenderId",
                principalTable: "Lenders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Lenders_LenderId",
                table: "Evaluations",
                column: "LenderId",
                principalTable: "Lenders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DealSettings_Lenders_DefaultLenderId",
                table: "DealSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Lenders_LenderId",
                table: "Evaluations");

            migrationBuilder.DropTable(
                name: "Lenders");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_LenderId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_DealSettings_DefaultLenderId",
                table: "DealSettings");

            migrationBuilder.DropColumn(
                name: "LenderId",
                table: "Evaluations");

            migrationBuilder.DropColumn(
                name: "DefaultLenderId",
                table: "DealSettings");
        }
    }
}
