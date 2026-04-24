using BtOperasyonTakip.Models;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Musteri> Musteriler => Set<Musteri>();
        public DbSet<MusteriDurumGecmisi> MusteriDurumGecmisleri => Set<MusteriDurumGecmisi>();
        public DbSet<IletisimBilgileri> IletisimBilgileri => Set<IletisimBilgileri>();
        public DbSet<Detay> Detaylar => Set<Detay>();
        public DbSet<Parametre> Parametreler => Set<Parametre>();
        public DbSet<ToplantiNotu> ToplantiNotlari => Set<ToplantiNotu>();
        public DbSet<JiraTask> JiraTasks => Set<JiraTask>();
        public DbSet<JiraYorum> JiraYorumlar { get; set; }
        public DbSet<ExcelAktarimKaydi> ExcelAktarimKayitlari => Set<ExcelAktarimKaydi>();
        public DbSet<User> Users { get; set; }
        public DbSet<IzinTalebi> IzinTalepleri => Set<IzinTalebi>();
        public DbSet<IzinHakki> IzinHaklari => Set<IzinHakki>();
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Hata> Hatalar { get; set; } // YENİ
        public DbSet<Issue> Issues { get; set; }

        public DbSet<TicketAtamaLog> TicketAtamaLoglari { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MusteriDurumGecmisi>()
                .HasOne(x => x.Musteri)
                .WithMany(x => x.DurumGecmisi)
                .HasForeignKey(x => x.MusteriID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<IzinTalebi>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IzinHakki>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<IzinHakki>()
                .HasIndex(x => x.UserId)
                .IsUnique();

            modelBuilder.Entity<ExcelAktarimKaydi>()
                .HasOne(x => x.EslesenMusteri)
                .WithMany()
                .HasForeignKey(x => x.EslesenMusteriID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}