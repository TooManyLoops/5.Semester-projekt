using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timegrip.Employees.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialEmployeeDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Employee");

            migrationBuilder.CreateTable(
                name: "Employees",
                schema: "Employee",
                columns: table => new
                {
                    employee_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    firstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    lastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    employeeStatus = table.Column<int>(type: "int", nullable: false),
                    hiredAt = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.employee_Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Employee",
                columns: table => new
                {
                    role_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.role_Id);
                });

            migrationBuilder.CreateTable(
                name: "Employments",
                schema: "Employee",
                columns: table => new
                {
                    EmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentType = table.Column<int>(type: "int", nullable: false),
                    WeeklyHours = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employments", x => x.EmploymentId);
                    table.ForeignKey(
                        name: "FK_Employments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "employee_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRoles",
                schema: "Employee",
                columns: table => new
                {
                    employeeRole_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    isPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRoles", x => x.employeeRole_Id);
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Employees_employee_Id",
                        column: x => x.employee_Id,
                        principalSchema: "Employee",
                        principalTable: "Employees",
                        principalColumn: "employee_Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Roles_role_Id",
                        column: x => x.role_Id,
                        principalSchema: "Employee",
                        principalTable: "Roles",
                        principalColumn: "role_Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRoles_employee_Id",
                schema: "Employee",
                table: "EmployeeRoles",
                column: "employee_Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRoles_role_Id",
                schema: "Employee",
                table: "EmployeeRoles",
                column: "role_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Employments_EmployeeId",
                schema: "Employee",
                table: "Employments",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeRoles",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "Employments",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Employee");

            migrationBuilder.DropTable(
                name: "Employees",
                schema: "Employee");
        }
    }
}
