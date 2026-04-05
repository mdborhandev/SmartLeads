using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLeads.Infrastructure.Persistence.Migrations.Default
{
    /// <inheritdoc />
    public partial class AddCompanyNavigationToBaseEntityAndDepartmentToDesignation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Designations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Designations_Departments_DepartmentId",
                table: "Designations",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Designations_Departments_DepartmentId",
                table: "Designations");

            migrationBuilder.DropIndex(
                name: "IX_Designations_DepartmentId",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Designations");
        }
    }
}
