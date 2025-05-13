using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class OneOnOneRelationEmailMessageAndEmailMessageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailMessageContents_EmailMessageId",
                table: "EmailMessageContents");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessageContents_EmailMessageId",
                table: "EmailMessageContents",
                column: "EmailMessageId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmailMessageContents_EmailMessageId",
                table: "EmailMessageContents");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessageContents_EmailMessageId",
                table: "EmailMessageContents",
                column: "EmailMessageId");
        }
    }
}
