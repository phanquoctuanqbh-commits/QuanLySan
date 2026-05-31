using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLySan.Migrations
{
    /// <inheritdoc />
    public partial class AddWarningFieldsToWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "WarnedUntil",
                table: "Wallets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarningReason",
                table: "Wallets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WarnedUntil",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "WarningReason",
                table: "Wallets");
        }
    }
}
