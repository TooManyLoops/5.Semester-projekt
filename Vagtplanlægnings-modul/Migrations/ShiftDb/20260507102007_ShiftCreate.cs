using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vagtplanlægnings_modul.Migrations.ShiftDb
{
    /// <inheritdoc />
    public partial class ShiftCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shift");

            migrationBuilder.CreateTable(
                name: "Shifts",
                schema: "shift",
                columns: table => new
                {
                    Shift_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Shift_Id);
                });

            migrationBuilder.CreateTable(
                name: "ShiftAssignments",
                schema: "shift",
                columns: table => new
                {
                    ShiftAssignment_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_Shift_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FK_EmployeeRole_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Assigned_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAssignments", x => x.ShiftAssignment_Id);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_EmployeeRoles_FK_EmployeeRole_Id",
                        column: x => x.FK_EmployeeRole_Id,
                        principalSchema: "dbo",
                        principalTable: "EmployeeRoles",
                        principalColumn: "EmployeeRole_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_Shifts_FK_Shift_Id",
                        column: x => x.FK_Shift_Id,
                        principalSchema: "shift",
                        principalTable: "Shifts",
                        principalColumn: "Shift_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_FK_EmployeeRole_Id",
                schema: "shift",
                table: "ShiftAssignments",
                column: "FK_EmployeeRole_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_FK_Shift_Id",
                schema: "shift",
                table: "ShiftAssignments",
                column: "FK_Shift_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftAssignments",
                schema: "shift");

            migrationBuilder.DropTable(
                name: "Shifts",
                schema: "shift");
        }
    }
}
