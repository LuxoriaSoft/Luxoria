using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuxAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ActivityLogs",
                newName: "Timestamp");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ActivityLogs",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ActivityLogs");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "ActivityLogs",
                newName: "CreatedAt");
        }
    }
}
