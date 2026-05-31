using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLySan.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameToWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Wallets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Wallets");
        }
    }
}
