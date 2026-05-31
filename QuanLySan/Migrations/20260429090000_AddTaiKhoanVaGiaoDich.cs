using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuanLySan.Data;

#nullable disable

namespace QuanLySan.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260429090000_AddTaiKhoanVaGiaoDich")]
    public partial class AddTaiKhoanVaGiaoDich : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaiKhoanNguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SoDu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanNguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GiaoDichs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaiKhoanNguoiDungId = table.Column<int>(type: "int", nullable: false),
                    DatSanId = table.Column<int>(type: "int", nullable: true),
                    LoaiGiaoDich = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoDuSauGiaoDich = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayGiaoDich = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDichs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiaoDichs_DatSans_DatSanId",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GiaoDichs_TaiKhoanNguoiDungs_TaiKhoanNguoiDungId",
                        column: x => x.TaiKhoanNguoiDungId,
                        principalTable: "TaiKhoanNguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichs_DatSanId",
                table: "GiaoDichs",
                column: "DatSanId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDichs_TaiKhoanNguoiDungId",
                table: "GiaoDichs",
                column: "TaiKhoanNguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanNguoiDungs_UserId",
                table: "TaiKhoanNguoiDungs",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiaoDichs");

            migrationBuilder.DropTable(
                name: "TaiKhoanNguoiDungs");
        }
    }
}
