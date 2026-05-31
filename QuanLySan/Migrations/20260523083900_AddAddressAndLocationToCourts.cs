using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLySan.Migrations
{
    /// <inheritdoc />
    public partial class AddAddressAndLocationToCourts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Courts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Courts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Courts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Courts");
        }
    }
}
