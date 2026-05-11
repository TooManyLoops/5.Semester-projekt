using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timegrip.Shifts.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Shift");

            migrationBuilder.CreateTable(
                name: "Shifts",
                schema: "Shift",
                columns: table => new
                {
                    Shift_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    startTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Shift_Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAssignments",
                schema: "Shift",
                columns: table => new
                {
                    ShiftAssignment_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    shift_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employeeRole_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Assigned_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAssignments", x => x.ShiftAssignment_Id);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_Shifts_shift_Id",
                        column: x => x.shift_Id,
                        principalSchema: "Shift",
                        principalTable: "Shifts",
                        principalColumn: "Shift_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftRequirements",
                schema: "Shift",
                columns: table => new
                {
                    ShiftRequirement_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftRequirements", x => x.ShiftRequirement_Id);
                    table.ForeignKey(
                        name: "FK_ShiftRequirements_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalSchema: "Shift",
                        principalTable: "Shifts",
                        principalColumn: "Shift_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_shift_Id",
                schema: "Shift",
                table: "ShiftAssignments",
                column: "shift_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequirements_ShiftId",
                schema: "Shift",
                table: "ShiftRequirements",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftAssignments",
                schema: "Shift");

            migrationBuilder.DropTable(
                name: "ShiftRequirements",
                schema: "Shift");

            migrationBuilder.DropTable(
                name: "Shifts",
                schema: "Shift");
        }
    }
}
