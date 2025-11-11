using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DealEvaluator.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedRehabEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepairCost",
                table: "Evaluations");

            migrationBuilder.CreateTable(
                name: "RehabCostTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LineItemType = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    DefaultCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabCostTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehabCostTemplates_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RehabEstimates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehabEstimates_Evaluations_EvaluationId",
                        column: x => x.EvaluationId,
                        principalTable: "Evaluations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RehabLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RehabEstimateId = table.Column<int>(type: "int", nullable: false),
                    LineItemType = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RehabLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RehabLineItems_RehabEstimates_RehabEstimateId",
                        column: x => x.RehabEstimateId,
                        principalTable: "RehabEstimates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RehabCostTemplates_UserId_LineItemType_Condition",
                table: "RehabCostTemplates",
                columns: new[] { "UserId", "LineItemType", "Condition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RehabEstimates_EvaluationId",
                table: "RehabEstimates",
                column: "EvaluationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RehabLineItems_RehabEstimateId",
                table: "RehabLineItems",
                column: "RehabEstimateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RehabCostTemplates");

            migrationBuilder.DropTable(
                name: "RehabLineItems");

            migrationBuilder.DropTable(
                name: "RehabEstimates");

            migrationBuilder.AddColumn<int>(
                name: "RepairCost",
                table: "Evaluations",
                type: "int",
                nullable: true);
        }
    }
}
