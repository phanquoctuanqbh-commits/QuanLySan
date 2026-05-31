using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuanLySan.Models;

namespace QuanLySan.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<San> Sans { get; set; }
        public DbSet<LoaiSan> LoaiSans { get; set; }
        public DbSet<DatSan> DatSans { get; set; }
        public DbSet<TaiKhoanNguoiDung> TaiKhoanNguoiDungs { get; set; }
        public DbSet<GiaoDich> GiaoDichs { get; set; }
        public DbSet<YeuCauNapTien> YeuCauNapTiens { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<BaoTriSan> BaoTriSans { get; set; }
        public DbSet<DanhGiaSan> DanhGiaSans { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<CourtReport> CourtReports { get; set; }
        public DbSet<MatchPost> MatchPosts { get; set; }
        public DbSet<MatchApplication> MatchApplications { get; set; }
        public DbSet<PlayingGroup> PlayingGroups { get; set; }
        public DbSet<PlayingGroupMember> PlayingGroupMembers { get; set; }
        public DbSet<GiaoLuu> GiaoLuus { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<San>().ToTable("Courts");
            builder.Entity<LoaiSan>().ToTable("CourtTypes");
            builder.Entity<DatSan>().ToTable("Bookings");
            builder.Entity<TaiKhoanNguoiDung>().ToTable("Wallets");
            builder.Entity<GiaoDich>().ToTable("Transactions");
            builder.Entity<YeuCauNapTien>().ToTable("TopUpRequests");
            builder.Entity<Voucher>().ToTable("Vouchers");
            builder.Entity<BaoTriSan>().ToTable("CourtMaintenances");
            builder.Entity<DanhGiaSan>().ToTable("CourtReviews");
            builder.Entity<ThongBao>().ToTable("Notifications");
            builder.Entity<ChatMessage>().ToTable("ChatMessages");
            builder.Entity<SiteSetting>().ToTable("SiteSettings");
            builder.Entity<CourtReport>().ToTable("Reports");
            builder.Entity<MatchPost>().ToTable("MatchPosts");
            builder.Entity<MatchApplication>().ToTable("MatchApplications");
            builder.Entity<PlayingGroup>().ToTable("PlayingGroups");
            builder.Entity<PlayingGroupMember>().ToTable("PlayingGroupMembers");
            builder.Entity<GiaoLuu>().ToTable("Matchmakings");

            builder.Entity<San>(entity =>
            {
                entity.Property(x => x.TenSan).HasColumnName("Name");
                entity.Property(x => x.LoaiSanId).HasColumnName("CourtTypeId");
                entity.Property(x => x.Gia).HasColumnName("HourlyPrice");
                entity.Property(x => x.MoTa).HasColumnName("Description");
                entity.Property(x => x.HinhAnh).HasColumnName("ImageUrl");
                entity.Property(x => x.TrangThai).HasColumnName("IsActive");
                entity.Property(x => x.DiaChi).HasColumnName("Address").HasDefaultValue(string.Empty);
                entity.Property(x => x.Latitude).HasColumnName("Latitude").HasDefaultValue(0.0);
                entity.Property(x => x.Longitude).HasColumnName("Longitude").HasDefaultValue(0.0);
                entity.Property(x => x.OwnerId).HasColumnName("OwnerId").HasDefaultValue(string.Empty);
                entity.Property(x => x.OwnerName).HasColumnName("OwnerName").HasDefaultValue(string.Empty);
            });

            builder.Entity<LoaiSan>()
                .Property(x => x.TenLoai)
                .HasColumnName("Name");

            builder.Entity<DatSan>(entity =>
            {
                entity.Property(x => x.UserId).HasColumnName("UserId");
                entity.Property(x => x.SanId).HasColumnName("CourtId");
                entity.Property(x => x.Ngay).HasColumnName("BookingDate");
                entity.Property(x => x.GioBatDau).HasColumnName("StartTime");
                entity.Property(x => x.GioKetThuc).HasColumnName("EndTime");
                entity.Property(x => x.TrangThai).HasColumnName("Status");
                entity.Property(x => x.TongTien).HasColumnName("TotalAmount");
                entity.Property(x => x.GiamGia).HasColumnName("DiscountAmount");
                entity.Property(x => x.MaVoucher).HasColumnName("VoucherCode");
                entity.Property(x => x.MaXacNhan).HasColumnName("ConfirmationCode");
            });

            builder.Entity<TaiKhoanNguoiDung>(entity =>
            {
                entity.Property(x => x.SoDu).HasColumnName("Balance");
                entity.Property(x => x.NgayTao).HasColumnName("CreatedAt");
                entity.Property(x => x.HoTen).HasColumnName("FullName").HasDefaultValue(string.Empty);
                entity.Property(x => x.CanhCaoDen).HasColumnName("WarnedUntil").IsRequired(false);
                entity.Property(x => x.LyDoCanhCao).HasColumnName("WarningReason").IsRequired(false);
            });

            builder.Entity<GiaoDich>(entity =>
            {
                entity.Property(x => x.TaiKhoanNguoiDungId).HasColumnName("WalletId");
                entity.Property(x => x.DatSanId).HasColumnName("BookingId");
                entity.Property(x => x.LoaiGiaoDich).HasColumnName("TransactionType");
                entity.Property(x => x.SoTien).HasColumnName("Amount");
                entity.Property(x => x.SoDuSauGiaoDich).HasColumnName("BalanceAfter");
                entity.Property(x => x.NoiDung).HasColumnName("Description");
                entity.Property(x => x.NgayGiaoDich).HasColumnName("CreatedAt");
            });

            builder.Entity<YeuCauNapTien>(entity =>
            {
                entity.Property(x => x.SoTien).HasColumnName("Amount");
                entity.Property(x => x.PhuongThuc).HasColumnName("PaymentMethod");
                entity.Property(x => x.MaThamChieu).HasColumnName("ReferenceCode");
                entity.Property(x => x.GhiChu).HasColumnName("Note");
                entity.Property(x => x.TrangThai).HasColumnName("Status");
                entity.Property(x => x.NgayGui).HasColumnName("CreatedAt");
                entity.Property(x => x.NgayXuLy).HasColumnName("ProcessedAt");
            });

            builder.Entity<Voucher>(entity =>
            {
                entity.Property(x => x.Ma).HasColumnName("Code");
                entity.Property(x => x.Ten).HasColumnName("Name");
                entity.Property(x => x.LoaiGiamGia).HasColumnName("DiscountType");
                entity.Property(x => x.GiaTri).HasColumnName("DiscountValue");
                entity.Property(x => x.DonToiThieu).HasColumnName("MinimumOrderAmount");
                entity.Property(x => x.NgayBatDau).HasColumnName("StartDate");
                entity.Property(x => x.NgayKetThuc).HasColumnName("EndDate");
                entity.Property(x => x.SoLuotToiDa).HasColumnName("UsageLimit");
                entity.Property(x => x.SoLuotDaDung).HasColumnName("UsedCount");
                entity.Property(x => x.TrangThai).HasColumnName("IsActive");
                entity.Property(x => x.MoTa).HasColumnName("Description");
            });

            builder.Entity<BaoTriSan>(entity =>
            {
                entity.Property(x => x.SanId).HasColumnName("CourtId");
                entity.Property(x => x.Ngay).HasColumnName("MaintenanceDate");
                entity.Property(x => x.GioBatDau).HasColumnName("StartTime");
                entity.Property(x => x.GioKetThuc).HasColumnName("EndTime");
                entity.Property(x => x.LyDo).HasColumnName("Reason");
                entity.Property(x => x.TrangThai).HasColumnName("IsActive");
            });

            builder.Entity<DanhGiaSan>(entity =>
            {
                entity.Property(x => x.DatSanId).HasColumnName("BookingId");
                entity.Property(x => x.SanId).HasColumnName("CourtId");
                entity.Property(x => x.SoSao).HasColumnName("Rating");
                entity.Property(x => x.BinhLuan).HasColumnName("Comment");
                entity.Property(x => x.NgayDanhGia).HasColumnName("CreatedAt");
            });

            builder.Entity<ThongBao>(entity =>
            {
                entity.Property(x => x.TieuDe).HasColumnName("Title");
                entity.Property(x => x.NoiDung).HasColumnName("Content");
                entity.Property(x => x.LienKet).HasColumnName("Url");
                entity.Property(x => x.DaDoc).HasColumnName("IsRead");
                entity.Property(x => x.NgayTao).HasColumnName("CreatedAt");
            });

            builder.Entity<ChatMessage>(entity =>
            {
                entity.Property(x => x.NoiDung).HasColumnName("Content");
                entity.Property(x => x.GuiBoiAdmin).HasColumnName("SentByAdmin");
                entity.Property(x => x.DaDoc).HasColumnName("IsRead");
                entity.Property(x => x.NgayGui).HasColumnName("CreatedAt");
            });

            builder.Entity<CourtReport>(entity =>
            {
                entity.Property(x => x.SanId).HasColumnName("CourtId");
                entity.Property(x => x.UserId).HasColumnName("UserId");
                entity.Property(x => x.OwnerId).HasColumnName("OwnerId");
                entity.Property(x => x.Reason).HasMaxLength(120);
                entity.Property(x => x.Status).HasMaxLength(40);
                entity.Property(x => x.CreatedAt).HasColumnName("CreatedAt");
                entity.Property(x => x.ResolvedAt).HasColumnName("ResolvedAt");
            });

            builder.Entity<MatchPost>(entity =>
            {
                entity.Property(x => x.SportTypeId).HasColumnName("SportTypeId");
                entity.Property(x => x.CourtId).HasColumnName("CourtId");
                entity.Property(x => x.Date).HasColumnName("PlayDate");
                entity.Property(x => x.StartTime).HasColumnName("StartTime");
                entity.Property(x => x.EndTime).HasColumnName("EndTime");
                entity.Property(x => x.Status).HasMaxLength(40);
                entity.Property(x => x.SkillLevel).HasMaxLength(40);
                entity.Property(x => x.PreferredGender).HasMaxLength(40);
            });

            builder.Entity<MatchApplication>(entity =>
            {
                entity.Property(x => x.MatchPostId).HasColumnName("PostId");
                entity.Property(x => x.Status).HasMaxLength(40);
                entity.Property(x => x.SkillLevel).HasMaxLength(40);
            });

            builder.Entity<PlayingGroup>(entity =>
            {
                entity.Property(x => x.MatchPostId).HasColumnName("PostId");
                entity.Property(x => x.CreatedDate).HasColumnName("CreatedDate");
                entity.Property(x => x.BookingId).HasColumnName("BookingId");
            });

            builder.Entity<PlayingGroupMember>(entity =>
            {
                entity.Property(x => x.PlayingGroupId).HasColumnName("GroupId");
                entity.Property(x => x.Role).HasMaxLength(40);
            });

            builder.Entity<San>()
                .Property(x => x.Gia)
                .HasPrecision(18, 2);

            builder.Entity<DatSan>()
                .Property(x => x.TongTien)
                .HasPrecision(18, 2);

            builder.Entity<DatSan>()
                .Property(x => x.GiamGia)
                .HasPrecision(18, 2);

            builder.Entity<Voucher>()
                .Property(x => x.GiaTri)
                .HasPrecision(18, 2);

            builder.Entity<Voucher>()
                .Property(x => x.DonToiThieu)
                .HasPrecision(18, 2);

            builder.Entity<TaiKhoanNguoiDung>()
                .Property(x => x.SoDu)
                .HasPrecision(18, 2);

            builder.Entity<GiaoDich>()
                .Property(x => x.SoTien)
                .HasPrecision(18, 2);

            builder.Entity<GiaoDich>()
                .Property(x => x.SoDuSauGiaoDich)
                .HasPrecision(18, 2);

            builder.Entity<YeuCauNapTien>()
                .Property(x => x.SoTien)
                .HasPrecision(18, 2);

            builder.Entity<TaiKhoanNguoiDung>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            builder.Entity<San>()
                .HasOne(x => x.LoaiSan)
                .WithMany(x => x.Sans)
                .HasForeignKey(x => x.LoaiSanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DatSan>()
                .HasOne(x => x.San)
                .WithMany(x => x.DatSans)
                .HasForeignKey(x => x.SanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DatSan>()
                .HasOne(x => x.Voucher)
                .WithMany(x => x.DatSans)
                .HasForeignKey(x => x.VoucherId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<BaoTriSan>()
                .HasOne(x => x.San)
                .WithMany()
                .HasForeignKey(x => x.SanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DanhGiaSan>()
                .HasOne(x => x.DatSan)
                .WithOne(x => x.DanhGiaSan)
                .HasForeignKey<DanhGiaSan>(x => x.DatSanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DanhGiaSan>()
                .HasOne(x => x.San)
                .WithMany()
                .HasForeignKey(x => x.SanId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Voucher>()
                .HasIndex(x => x.Ma)
                .IsUnique();

            builder.Entity<GiaoDich>()
                .HasOne(x => x.TaiKhoanNguoiDung)
                .WithMany(x => x.GiaoDichs)
                .HasForeignKey(x => x.TaiKhoanNguoiDungId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GiaoDich>()
                .HasOne(x => x.DatSan)
                .WithMany(x => x.GiaoDichs)
                .HasForeignKey(x => x.DatSanId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<CourtReport>()
                .HasOne(x => x.San)
                .WithMany(x => x.Reports)
                .HasForeignKey(x => x.SanId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MatchPost>()
                .HasOne(x => x.SportType)
                .WithMany()
                .HasForeignKey(x => x.SportTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MatchPost>()
                .HasOne(x => x.Court)
                .WithMany()
                .HasForeignKey(x => x.CourtId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<MatchApplication>()
                .HasOne(x => x.MatchPost)
                .WithMany(x => x.Applications)
                .HasForeignKey(x => x.MatchPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PlayingGroup>()
                .HasOne(x => x.MatchPost)
                .WithOne(x => x.PlayingGroup)
                .HasForeignKey<PlayingGroup>(x => x.MatchPostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PlayingGroup>()
                .HasOne(x => x.Booking)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PlayingGroupMember>()
                .HasOne(x => x.PlayingGroup)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.PlayingGroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
