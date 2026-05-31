using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuanLySan.Data;

#nullable disable

namespace QuanLySan.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260429083000_RestrictLoaiSanDelete")]
    public partial class RestrictLoaiSanDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sans_LoaiSans_LoaiSanId",
                table: "Sans");

            migrationBuilder.AddForeignKey(
                name: "FK_Sans_LoaiSans_LoaiSanId",
                table: "Sans",
                column: "LoaiSanId",
                principalTable: "LoaiSans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sans_LoaiSans_LoaiSanId",
                table: "Sans");

            migrationBuilder.AddForeignKey(
                name: "FK_Sans_LoaiSans_LoaiSanId",
                table: "Sans",
                column: "LoaiSanId",
                principalTable: "LoaiSans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
