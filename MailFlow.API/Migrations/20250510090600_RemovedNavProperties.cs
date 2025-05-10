using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MailFlow.API.Migrations
{
    /// <inheritdoc />
    public partial class RemovedNavProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailMessages_Users_UserId",
                table: "EmailMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GoogleTokens_Users_UserId",
                table: "GoogleTokens");

            migrationBuilder.DropIndex(
                name: "IX_GoogleTokens_UserId",
                table: "GoogleTokens");

            migrationBuilder.DropIndex(
                name: "IX_EmailMessages_UserId",
                table: "EmailMessages");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "GoogleTokens");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmailMessages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "GoogleTokens",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "EmailMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_GoogleTokens_UserId",
                table: "GoogleTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailMessages_UserId",
                table: "EmailMessages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmailMessages_Users_UserId",
                table: "EmailMessages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoogleTokens_Users_UserId",
                table: "GoogleTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
