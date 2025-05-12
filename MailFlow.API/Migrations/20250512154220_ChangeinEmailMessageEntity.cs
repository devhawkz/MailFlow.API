using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeinEmailMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LabelIdJson",
                table: "EmailMessages",
                newName: "LabelIds");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LabelIds",
                table: "EmailMessages",
                newName: "LabelIdJson");
        }
    }
}
