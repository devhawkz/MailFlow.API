using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class NewChangeInEmailMessageEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LabelIds",
                table: "EmailMessages",
                newName: "LabelIdsJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LabelIdsJson",
                table: "EmailMessages",
                newName: "LabelIds");
        }
    }
}
