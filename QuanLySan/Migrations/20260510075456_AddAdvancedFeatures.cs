using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLySan.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GiamGia",
                table: "DatSans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "MaVoucher",
                table: "DatSans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MaXacNhan",
                table: "DatSans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VoucherId",
                table: "DatSans",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BaoTriSans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanId = table.Column<int>(type: "int", nullable: false),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioBatDau = table.Column<TimeSpan>(type: "time", nullable: false),
                    GioKetThuc = table.Column<TimeSpan>(type: "time", nullable: false),
                    LyDo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoTriSans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BaoTriSans_Sans_SanId",
                        column: x => x.SanId,
                        principalTable: "Sans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGiaSans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatSanId = table.Column<int>(type: "int", nullable: false),
                    SanId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    BinhLuan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGiaSans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGiaSans_DatSans_DatSanId",
                        column: x => x.DatSanId,
                        principalTable: "DatSans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DanhGiaSans_Sans_SanId",
                        column: x => x.SanId,
                        principalTable: "Sans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TieuDe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LienKet = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DaDoc = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vouchers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ten = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoaiGiamGia = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DonToiThieu = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuotToiDa = table.Column<int>(type: "int", nullable: false),
                    SoLuotDaDung = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<bool>(type: "bit", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vouchers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DatSans_VoucherId",
                table: "DatSans",
                column: "VoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_BaoTriSans_SanId",
                table: "BaoTriSans",
                column: "SanId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaSans_DatSanId",
                table: "DanhGiaSans",
                column: "DatSanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGiaSans_SanId",
                table: "DanhGiaSans",
                column: "SanId");

            migrationBuilder.CreateIndex(
                name: "IX_Vouchers_Ma",
                table: "Vouchers",
                column: "Ma",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_Vouchers_VoucherId",
                table: "DatSans",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_Vouchers_VoucherId",
                table: "DatSans");

            migrationBuilder.DropTable(
                name: "BaoTriSans");

            migrationBuilder.DropTable(
                name: "DanhGiaSans");

            migrationBuilder.DropTable(
                name: "ThongBaos");

            migrationBuilder.DropTable(
                name: "Vouchers");

            migrationBuilder.DropIndex(
                name: "IX_DatSans_VoucherId",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "GiamGia",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "MaVoucher",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "MaXacNhan",
                table: "DatSans");

            migrationBuilder.DropColumn(
                name: "VoucherId",
                table: "DatSans");
        }
    }
}
