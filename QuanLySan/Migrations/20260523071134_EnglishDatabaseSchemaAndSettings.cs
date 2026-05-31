using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLySan.Migrations
{
    /// <inheritdoc />
    public partial class EnglishDatabaseSchemaAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaoTriSans_Sans_SanId",
                table: "BaoTriSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGiaSans_DatSans_DatSanId",
                table: "DanhGiaSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DanhGiaSans_Sans_SanId",
                table: "DanhGiaSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_Sans_SanId",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_DatSans_Vouchers_VoucherId",
                table: "DatSans");

            migrationBuilder.DropForeignKey(
                name: "FK_GiaoDichs_DatSans_DatSanId",
                table: "GiaoDichs");

            migrationBuilder.DropForeignKey(
                name: "FK_GiaoDichs_TaiKhoanNguoiDungs_TaiKhoanNguoiDungId",
                table: "GiaoDichs");

            migrationBuilder.DropForeignKey(
                name: "FK_Sans_LoaiSans_LoaiSanId",
                table: "Sans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_YeuCauNapTiens",
                table: "YeuCauNapTiens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ThongBaos",
                table: "ThongBaos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TaiKhoanNguoiDungs",
                table: "TaiKhoanNguoiDungs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sans",
                table: "Sans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoaiSans",
                table: "LoaiSans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiaoDichs",
                table: "GiaoDichs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DatSans",
                table: "DatSans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DanhGiaSans",
                table: "DanhGiaSans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BaoTriSans",
                table: "BaoTriSans");

            migrationBuilder.RenameTable(
                name: "YeuCauNapTiens",
                newName: "TopUpRequests");

            migrationBuilder.RenameTable(
                name: "ThongBaos",
                newName: "Notifications");

            migrationBuilder.RenameTable(
                name: "TaiKhoanNguoiDungs",
                newName: "Wallets");

            migrationBuilder.RenameTable(
                name: "Sans",
                newName: "Courts");

            migrationBuilder.RenameTable(
                name: "LoaiSans",
                newName: "CourtTypes");

            migrationBuilder.RenameTable(
                name: "GiaoDichs",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "DatSans",
                newName: "Bookings");

            migrationBuilder.RenameTable(
                name: "DanhGiaSans",
                newName: "CourtReviews");

            migrationBuilder.RenameTable(
                name: "BaoTriSans",
                newName: "CourtMaintenances");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "Vouchers",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "Ten",
                table: "Vouchers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "SoLuotToiDa",
                table: "Vouchers",
                newName: "UsageLimit");

            migrationBuilder.RenameColumn(
                name: "SoLuotDaDung",
                table: "Vouchers",
                newName: "UsedCount");

            migrationBuilder.RenameColumn(
                name: "NgayKetThuc",
                table: "Vouchers",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "NgayBatDau",
                table: "Vouchers",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "MoTa",
                table: "Vouchers",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "Ma",
                table: "Vouchers",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "LoaiGiamGia",
                table: "Vouchers",
                newName: "DiscountType");

            migrationBuilder.RenameColumn(
                name: "GiaTri",
                table: "Vouchers",
                newName: "DiscountValue");

            migrationBuilder.RenameColumn(
                name: "DonToiThieu",
                table: "Vouchers",
                newName: "MinimumOrderAmount");

            migrationBuilder.RenameIndex(
                name: "IX_Vouchers_Ma",
                table: "Vouchers",
                newName: "IX_Vouchers_Code");

            migrationBuilder.RenameColumn(
                name: "NoiDung",
                table: "ChatMessages",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "NgayGui",
                table: "ChatMessages",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "GuiBoiAdmin",
                table: "ChatMessages",
                newName: "SentByAdmin");

            migrationBuilder.RenameColumn(
                name: "DaDoc",
                table: "ChatMessages",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "TopUpRequests",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "SoTien",
                table: "TopUpRequests",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "PhuongThuc",
                table: "TopUpRequests",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "NgayXuLy",
                table: "TopUpRequests",
                newName: "ProcessedAt");

            migrationBuilder.RenameColumn(
                name: "NgayGui",
                table: "TopUpRequests",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "MaThamChieu",
                table: "TopUpRequests",
                newName: "ReferenceCode");

            migrationBuilder.RenameColumn(
                name: "GhiChu",
                table: "TopUpRequests",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "TieuDe",
                table: "Notifications",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "NoiDung",
                table: "Notifications",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "NgayTao",
                table: "Notifications",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "LienKet",
                table: "Notifications",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "DaDoc",
                table: "Notifications",
                newName: "IsRead");

            migrationBuilder.RenameColumn(
                name: "SoDu",
                table: "Wallets",
                newName: "Balance");

            migrationBuilder.RenameColumn(
                name: "NgayTao",
                table: "Wallets",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_TaiKhoanNguoiDungs_UserId",
                table: "Wallets",
                newName: "IX_Wallets_UserId");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "Courts",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "TenSan",
                table: "Courts",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "MoTa",
                table: "Courts",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "LoaiSanId",
                table: "Courts",
                newName: "CourtTypeId");

            migrationBuilder.RenameColumn(
                name: "HinhAnh",
                table: "Courts",
                newName: "ImageUrl");

            migrationBuilder.RenameColumn(
                name: "Gia",
                table: "Courts",
                newName: "HourlyPrice");

            migrationBuilder.RenameIndex(
                name: "IX_Sans_LoaiSanId",
                table: "Courts",
                newName: "IX_Courts_CourtTypeId");

            migrationBuilder.RenameColumn(
                name: "TenLoai",
                table: "CourtTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TaiKhoanNguoiDungId",
                table: "Transactions",
                newName: "WalletId");

            migrationBuilder.RenameColumn(
                name: "SoTien",
                table: "Transactions",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "SoDuSauGiaoDich",
                table: "Transactions",
                newName: "BalanceAfter");

            migrationBuilder.RenameColumn(
                name: "NoiDung",
                table: "Transactions",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "NgayGiaoDich",
                table: "Transactions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "LoaiGiaoDich",
                table: "Transactions",
                newName: "TransactionType");

            migrationBuilder.RenameColumn(
                name: "DatSanId",
                table: "Transactions",
                newName: "BookingId");

            migrationBuilder.RenameIndex(
                name: "IX_GiaoDichs_TaiKhoanNguoiDungId",
                table: "Transactions",
                newName: "IX_Transactions_WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_GiaoDichs_DatSanId",
                table: "Transactions",
                newName: "IX_Transactions_BookingId");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "Bookings",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "TongTien",
                table: "Bookings",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "SanId",
                table: "Bookings",
                newName: "CourtId");

            migrationBuilder.RenameColumn(
                name: "Ngay",
                table: "Bookings",
                newName: "BookingDate");

            migrationBuilder.RenameColumn(
                name: "MaXacNhan",
                table: "Bookings",
                newName: "ConfirmationCode");

            migrationBuilder.RenameColumn(
                name: "MaVoucher",
                table: "Bookings",
                newName: "VoucherCode");

            migrationBuilder.RenameColumn(
                name: "GioKetThuc",
                table: "Bookings",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "GioBatDau",
                table: "Bookings",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "GiamGia",
                table: "Bookings",
                newName: "DiscountAmount");

            migrationBuilder.RenameIndex(
                name: "IX_DatSans_VoucherId",
                table: "Bookings",
                newName: "IX_Bookings_VoucherId");

            migrationBuilder.RenameIndex(
                name: "IX_DatSans_SanId",
                table: "Bookings",
                newName: "IX_Bookings_CourtId");

            migrationBuilder.RenameColumn(
                name: "SoSao",
                table: "CourtReviews",
                newName: "Rating");

            migrationBuilder.RenameColumn(
                name: "SanId",
                table: "CourtReviews",
                newName: "CourtId");

            migrationBuilder.RenameColumn(
                name: "NgayDanhGia",
                table: "CourtReviews",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "DatSanId",
                table: "CourtReviews",
                newName: "BookingId");

            migrationBuilder.RenameColumn(
                name: "BinhLuan",
                table: "CourtReviews",
                newName: "Comment");

            migrationBuilder.RenameIndex(
                name: "IX_DanhGiaSans_SanId",
                table: "CourtReviews",
                newName: "IX_CourtReviews_CourtId");

            migrationBuilder.RenameIndex(
                name: "IX_DanhGiaSans_DatSanId",
                table: "CourtReviews",
                newName: "IX_CourtReviews_BookingId");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "CourtMaintenances",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "SanId",
                table: "CourtMaintenances",
                newName: "CourtId");

            migrationBuilder.RenameColumn(
                name: "Ngay",
                table: "CourtMaintenances",
                newName: "MaintenanceDate");

            migrationBuilder.RenameColumn(
                name: "LyDo",
                table: "CourtMaintenances",
                newName: "Reason");

            migrationBuilder.RenameColumn(
                name: "GioKetThuc",
                table: "CourtMaintenances",
                newName: "EndTime");

            migrationBuilder.RenameColumn(
                name: "GioBatDau",
                table: "CourtMaintenances",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX_BaoTriSans_SanId",
                table: "CourtMaintenances",
                newName: "IX_CourtMaintenances_CourtId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TopUpRequests",
                table: "TopUpRequests",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Courts",
                table: "Courts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourtTypes",
                table: "CourtTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourtReviews",
                table: "CourtReviews",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourtMaintenances",
                table: "CourtMaintenances",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpeningHours = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MapEmbedUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "BusinessName", "Address", "PhoneNumber", "Email", "OpeningHours", "MapEmbedUrl", "Description" },
                values: new object[]
                {
                    "SportHub",
                    "12 Nguyen Van Bao, Ward 4, Go Vap, Ho Chi Minh City",
                    "0909 123 456",
                    "support@sporthub.vn",
                    "06:00 - 23:00 daily",
                    "https://www.google.com/maps?q=12%20Nguyen%20Van%20Bao%20Go%20Vap%20Ho%20Chi%20Minh&output=embed",
                    "Online sport court booking and management system."
                });

            migrationBuilder.Sql("""
                UPDATE Bookings
                SET Status = CASE Status
                    WHEN N'Đã thanh toán' THEN 'Paid'
                    WHEN N'Đã xác nhận' THEN 'Confirmed'
                    WHEN N'Đã hủy' THEN 'Cancelled'
                    WHEN N'Chờ xác nhận' THEN 'Pending'
                    ELSE Status
                END;

                UPDATE Transactions
                SET TransactionType = CASE TransactionType
                    WHEN N'Thanh toán' THEN 'Payment'
                    WHEN N'Hoàn tiền' THEN 'Refund'
                    WHEN N'Nạp tiền' THEN 'TopUp'
                    ELSE TransactionType
                END;

                UPDATE TopUpRequests
                SET Status = CASE Status
                    WHEN N'Chờ kiểm tra' THEN 'Pending'
                    WHEN N'Đã duyệt' THEN 'Approved'
                    WHEN N'Từ chối' THEN 'Rejected'
                    ELSE Status
                END,
                PaymentMethod = CASE PaymentMethod
                    WHEN N'Chuyển khoản QR' THEN 'BankTransferQR'
                    WHEN N'Thẻ cào điện thoại' THEN 'PhoneCard'
                    ELSE PaymentMethod
                END;

                UPDATE Vouchers
                SET DiscountType = CASE DiscountType
                    WHEN N'SoTien' THEN 'Fixed'
                    WHEN N'PhanTram' THEN 'Percent'
                    ELSE DiscountType
                END,
                Name = CASE Code
                    WHEN 'SPORT50' THEN 'Discount 50,000 VND'
                    WHEN 'GIAM20' THEN 'Discount 20%'
                    ELSE Name
                END,
                Description = CASE Code
                    WHEN 'SPORT50' THEN 'Apply for bookings from 150,000 VND.'
                    WHEN 'GIAM20' THEN 'Featured offer for off-peak hours.'
                    ELSE Description
                END;

                UPDATE CourtTypes
                SET Name = CASE Name
                    WHEN N'Sân bóng đá' THEN 'Football'
                    WHEN N'Sân cầu lông' THEN 'Badminton'
                    WHEN N'Sân tennis' THEN 'Tennis'
                    ELSE Name
                END;

                UPDATE Courts
                SET Description = CASE Name
                    WHEN N'Sân A1 - Cỏ nhân tạo' THEN 'New turf, LED lighting, suitable for 5-7 players.'
                    WHEN N'Sân B2 - Cầu lông' THEN 'Anti-slip floor, clean changing area.'
                    WHEN N'Sân C3 - Tennis' THEN 'Standard court with rest area and drinking water.'
                    ELSE Description
                END,
                Name = CASE Name
                    WHEN N'Sân A1 - Cỏ nhân tạo' THEN 'Court A1 - Artificial Turf'
                    WHEN N'Sân B2 - Cầu lông' THEN 'Court B2 - Badminton'
                    WHEN N'Sân C3 - Tennis' THEN 'Court C3 - Tennis'
                    ELSE Name
                END;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Courts_CourtId",
                table: "Bookings",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Vouchers_VoucherId",
                table: "Bookings",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CourtMaintenances_Courts_CourtId",
                table: "CourtMaintenances",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourtReviews_Bookings_BookingId",
                table: "CourtReviews",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourtReviews_Courts_CourtId",
                table: "CourtReviews",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_CourtTypes_CourtTypeId",
                table: "Courts",
                column: "CourtTypeId",
                principalTable: "CourtTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Bookings_BookingId",
                table: "Transactions",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions",
                column: "WalletId",
                principalTable: "Wallets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Courts_CourtId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Vouchers_VoucherId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_CourtMaintenances_Courts_CourtId",
                table: "CourtMaintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_CourtReviews_Bookings_BookingId",
                table: "CourtReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_CourtReviews_Courts_CourtId",
                table: "CourtReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Courts_CourtTypes_CourtTypeId",
                table: "Courts");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Bookings_BookingId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Wallets_WalletId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wallets",
                table: "Wallets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TopUpRequests",
                table: "TopUpRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourtTypes",
                table: "CourtTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courts",
                table: "Courts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourtReviews",
                table: "CourtReviews");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourtMaintenances",
                table: "CourtMaintenances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "Wallets",
                newName: "TaiKhoanNguoiDungs");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "GiaoDichs");

            migrationBuilder.RenameTable(
                name: "TopUpRequests",
                newName: "YeuCauNapTiens");

            migrationBuilder.RenameTable(
                name: "Notifications",
                newName: "ThongBaos");

            migrationBuilder.RenameTable(
                name: "CourtTypes",
                newName: "LoaiSans");

            migrationBuilder.RenameTable(
                name: "Courts",
                newName: "Sans");

            migrationBuilder.RenameTable(
                name: "CourtReviews",
                newName: "DanhGiaSans");

            migrationBuilder.RenameTable(
                name: "CourtMaintenances",
                newName: "BaoTriSans");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "DatSans");

            migrationBuilder.RenameColumn(
                name: "UsedCount",
                table: "Vouchers",
                newName: "SoLuotDaDung");

            migrationBuilder.RenameColumn(
                name: "UsageLimit",
                table: "Vouchers",
                newName: "SoLuotToiDa");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Vouchers",
                newName: "NgayBatDau");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Vouchers",
                newName: "Ten");

            migrationBuilder.RenameColumn(
                name: "MinimumOrderAmount",
                table: "Vouchers",
                newName: "DonToiThieu");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Vouchers",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Vouchers",
                newName: "NgayKetThuc");

            migrationBuilder.RenameColumn(
                name: "DiscountValue",
                table: "Vouchers",
                newName: "GiaTri");

            migrationBuilder.RenameColumn(
                name: "DiscountType",
                table: "Vouchers",
                newName: "LoaiGiamGia");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Vouchers",
                newName: "MoTa");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Vouchers",
                newName: "Ma");

            migrationBuilder.RenameIndex(
                name: "IX_Vouchers_Code",
                table: "Vouchers",
                newName: "IX_Vouchers_Ma");

            migrationBuilder.RenameColumn(
                name: "SentByAdmin",
                table: "ChatMessages",
                newName: "GuiBoiAdmin");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "ChatMessages",
                newName: "DaDoc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ChatMessages",
                newName: "NgayGui");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "ChatMessages",
                newName: "NoiDung");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TaiKhoanNguoiDungs",
                newName: "NgayTao");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "TaiKhoanNguoiDungs",
                newName: "SoDu");

            migrationBuilder.RenameIndex(
                name: "IX_Wallets_UserId",
                table: "TaiKhoanNguoiDungs",
                newName: "IX_TaiKhoanNguoiDungs_UserId");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                table: "GiaoDichs",
                newName: "TaiKhoanNguoiDungId");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "GiaoDichs",
                newName: "LoaiGiaoDich");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "GiaoDichs",
                newName: "NoiDung");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "GiaoDichs",
                newName: "NgayGiaoDich");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "GiaoDichs",
                newName: "DatSanId");

            migrationBuilder.RenameColumn(
                name: "BalanceAfter",
                table: "GiaoDichs",
                newName: "SoDuSauGiaoDich");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "GiaoDichs",
                newName: "SoTien");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_WalletId",
                table: "GiaoDichs",
                newName: "IX_GiaoDichs_TaiKhoanNguoiDungId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_BookingId",
                table: "GiaoDichs",
                newName: "IX_GiaoDichs_DatSanId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "YeuCauNapTiens",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "ReferenceCode",
                table: "YeuCauNapTiens",
                newName: "MaThamChieu");

            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "YeuCauNapTiens",
                newName: "NgayXuLy");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "YeuCauNapTiens",
                newName: "PhuongThuc");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "YeuCauNapTiens",
                newName: "GhiChu");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "YeuCauNapTiens",
                newName: "NgayGui");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "YeuCauNapTiens",
                newName: "SoTien");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "ThongBaos",
                newName: "LienKet");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "ThongBaos",
                newName: "TieuDe");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "ThongBaos",
                newName: "DaDoc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ThongBaos",
                newName: "NgayTao");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "ThongBaos",
                newName: "NoiDung");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "LoaiSans",
                newName: "TenLoai");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Sans",
                newName: "TenSan");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Sans",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Sans",
                newName: "HinhAnh");

            migrationBuilder.RenameColumn(
                name: "HourlyPrice",
                table: "Sans",
                newName: "Gia");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Sans",
                newName: "MoTa");

            migrationBuilder.RenameColumn(
                name: "CourtTypeId",
                table: "Sans",
                newName: "LoaiSanId");

            migrationBuilder.RenameIndex(
                name: "IX_Courts_CourtTypeId",
                table: "Sans",
                newName: "IX_Sans_LoaiSanId");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "DanhGiaSans",
                newName: "SoSao");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "DanhGiaSans",
                newName: "NgayDanhGia");

            migrationBuilder.RenameColumn(
                name: "CourtId",
                table: "DanhGiaSans",
                newName: "SanId");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "DanhGiaSans",
                newName: "BinhLuan");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "DanhGiaSans",
                newName: "DatSanId");

            migrationBuilder.RenameIndex(
                name: "IX_CourtReviews_CourtId",
                table: "DanhGiaSans",
                newName: "IX_DanhGiaSans_SanId");

            migrationBuilder.RenameIndex(
                name: "IX_CourtReviews_BookingId",
                table: "DanhGiaSans",
                newName: "IX_DanhGiaSans_DatSanId");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "BaoTriSans",
                newName: "GioBatDau");

            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "BaoTriSans",
                newName: "LyDo");

            migrationBuilder.RenameColumn(
                name: "MaintenanceDate",
                table: "BaoTriSans",
                newName: "Ngay");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "BaoTriSans",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "BaoTriSans",
                newName: "GioKetThuc");

            migrationBuilder.RenameColumn(
                name: "CourtId",
                table: "BaoTriSans",
                newName: "SanId");

            migrationBuilder.RenameIndex(
                name: "IX_CourtMaintenances_CourtId",
                table: "BaoTriSans",
                newName: "IX_BaoTriSans_SanId");

            migrationBuilder.RenameColumn(
                name: "VoucherCode",
                table: "DatSans",
                newName: "MaVoucher");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "DatSans",
                newName: "TongTien");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "DatSans",
                newName: "TrangThai");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "DatSans",
                newName: "GioBatDau");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "DatSans",
                newName: "GioKetThuc");

            migrationBuilder.RenameColumn(
                name: "DiscountAmount",
                table: "DatSans",
                newName: "GiamGia");

            migrationBuilder.RenameColumn(
                name: "CourtId",
                table: "DatSans",
                newName: "SanId");

            migrationBuilder.RenameColumn(
                name: "ConfirmationCode",
                table: "DatSans",
                newName: "MaXacNhan");

            migrationBuilder.RenameColumn(
                name: "BookingDate",
                table: "DatSans",
                newName: "Ngay");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_VoucherId",
                table: "DatSans",
                newName: "IX_DatSans_VoucherId");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_CourtId",
                table: "DatSans",
                newName: "IX_DatSans_SanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TaiKhoanNguoiDungs",
                table: "TaiKhoanNguoiDungs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiaoDichs",
                table: "GiaoDichs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_YeuCauNapTiens",
                table: "YeuCauNapTiens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ThongBaos",
                table: "ThongBaos",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoaiSans",
                table: "LoaiSans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sans",
                table: "Sans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DanhGiaSans",
                table: "DanhGiaSans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BaoTriSans",
                table: "BaoTriSans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DatSans",
                table: "DatSans",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BaoTriSans_Sans_SanId",
                table: "BaoTriSans",
                column: "SanId",
                principalTable: "Sans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGiaSans_DatSans_DatSanId",
                table: "DanhGiaSans",
                column: "DatSanId",
                principalTable: "DatSans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DanhGiaSans_Sans_SanId",
                table: "DanhGiaSans",
                column: "SanId",
                principalTable: "Sans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_Sans_SanId",
                table: "DatSans",
                column: "SanId",
                principalTable: "Sans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DatSans_Vouchers_VoucherId",
                table: "DatSans",
                column: "VoucherId",
                principalTable: "Vouchers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoDichs_DatSans_DatSanId",
                table: "GiaoDichs",
                column: "DatSanId",
                principalTable: "DatSans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_GiaoDichs_TaiKhoanNguoiDungs_TaiKhoanNguoiDungId",
                table: "GiaoDichs",
                column: "TaiKhoanNguoiDungId",
                principalTable: "TaiKhoanNguoiDungs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sans_LoaiSans_LoaiSanId",
                table: "Sans",
                column: "LoaiSanId",
                principalTable: "LoaiSans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
