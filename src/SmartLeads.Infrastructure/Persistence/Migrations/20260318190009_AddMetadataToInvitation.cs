using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLeads.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMetadataToInvitation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Invitations",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "Invitations");
        }
    }
}
