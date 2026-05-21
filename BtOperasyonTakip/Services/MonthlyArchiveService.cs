using BtOperasyonTakip.Data;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Services
{
    public class MonthlyArchiveService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private Timer? _timer;

        public MonthlyArchiveService(IServiceProvider services)
        {
            _services = services;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // run once a day
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var now = DateTime.Now;
                // perform archival only on the last day of month
                if (now.Day != DateTime.DaysInMonth(now.Year, now.Month))
                    return;

                var monthStart = new DateTime(now.Year, now.Month, 1);

                // Archive tasks that are completed and created before this month
                var candidates = db.JiraTasks
                    .Where(t => (t.Durum ?? "").Trim().Equals("Tamamlandı", StringComparison.OrdinalIgnoreCase)
                                && t.OlusturmaTarihi < monthStart)
                    .ToList();

                if (!candidates.Any())
                    return;

                foreach (var t in candidates)
                {
                    t.Durum = "Arşiv";
                }

                db.SaveChanges();
            }
            catch
            {
                // swallow exceptions; consider logging in future
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
