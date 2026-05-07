using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timegrip.Roles.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Role");

            migrationBuilder.CreateTable(
                name: "EmployeeRoles",
                schema: "Role",
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
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Role",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeRoles",
                schema: "Role");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Role");
        }
    }
}
