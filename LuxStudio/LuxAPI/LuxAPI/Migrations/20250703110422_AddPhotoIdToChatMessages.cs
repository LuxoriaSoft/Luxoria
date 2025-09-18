using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoIdToChatMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PhotoId",
                table: "ChatMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_PhotoId",
                table: "ChatMessages",
                column: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Photos_PhotoId",
                table: "ChatMessages",
                column: "PhotoId",
                principalTable: "Photos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Photos_PhotoId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_PhotoId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "PhotoId",
                table: "ChatMessages");
        }
    }
}
