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
            migrationBuilder.CreateTable(
                name: "Shifts",
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
                columns: table => new
                {
                    ShiftAssignment_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeRole_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Assigned_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftAssignments", x => x.ShiftAssignment_Id);
                    table.ForeignKey(
                        name: "FK_ShiftAssignments_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Shift_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShiftRequirements",
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
                        principalTable: "Shifts",
                        principalColumn: "Shift_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftAssignments_ShiftId",
                table: "ShiftAssignments",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftRequirements_ShiftId",
                table: "ShiftRequirements",
                column: "ShiftId");

            migrationBuilder.Sql(@"
                ALTER TABLE ShiftAssignments
                ADD CONSTRAINT FK_ShiftAssignments_EmployeeRoles
                FOREIGN KEY (EmployeeRole_Id)
                REFERENCES EmployeeRoles(EmployeeRole_Id)
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ShiftRequirements
                ADD CONSTRAINT FK_ShiftRequirements_Roles
                FOREIGN KEY (Role_Id)
                REFERENCES Roles(Role_Id)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE ShiftAssignments
                DROP CONSTRAINT FK_ShiftAssignments_EmployeeRoles
            ");
            migrationBuilder.DropTable(
                name: "ShiftAssignments");

            migrationBuilder.DropTable(
                name: "ShiftRequirements");

            migrationBuilder.DropTable(
                name: "Shifts");
        }
    }
}
