using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLySan.Migrations
{
    /// <inheritdoc />
    public partial class AddGiaoLuuTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matchmakings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDangId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NguoiDangName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiSan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayChoi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDang = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NguoiGiaoLuuId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NguoiGiaoLuuName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchmakings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matchmakings");
        }
    }
}
