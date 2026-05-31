using Microsoft.EntityFrameworkCore;
using QuanLySan.Data;
using QuanLySan.Models;

namespace QuanLySan.Services
{
    public class BookingReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BookingReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CreateDueRemindersAsync(stoppingToken);
                }
                catch
                {
                    // Reminder failures must not stop the web host.
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task CreateDueRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var now = DateTime.Now;
            var from = now.AddMinutes(55);
            var to = now.AddMinutes(65);

            var candidateDates = new[] { from.Date, to.Date }.Distinct().ToList();
            var candidates = await context.DatSans
                .Include(x => x.San)
                .Where(x => x.TrangThai != "Cancelled")
                .Where(x => candidateDates.Contains(x.Ngay.Date))
                .ToListAsync(stoppingToken);

            var bookings = candidates
                .Where(x => x.Ngay.Date.Add(x.GioBatDau) >= from && x.Ngay.Date.Add(x.GioBatDau) <= to)
                .ToList();

            foreach (var booking in bookings)
            {
                var marker = $"booking-reminder:{booking.Id}";
                var exists = await context.ThongBaos.AnyAsync(x => x.UserId == booking.UserId && x.LienKet == marker, stoppingToken);
                if (exists)
                {
                    continue;
                }

                context.ThongBaos.Add(new ThongBao
                {
                    UserId = booking.UserId,
                    TieuDe = "Sắp đến giờ đặt sân",
                    NoiDung = $"Còn khoảng 1 tiếng nữa đến lịch {booking.San?.TenSan} lúc {booking.GioBatDau:hh\\:mm}.",
                    LienKet = marker
                });
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
