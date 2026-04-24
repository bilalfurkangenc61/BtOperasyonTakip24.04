using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BtOperasyonTakip.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? period)
        {
            var tr = CultureInfo.GetCultureInfo("tr-TR");
            var now = DateTime.Now;

            var seciliYil = now.Year;
            var seciliAy = now.Month;

            if (!string.IsNullOrWhiteSpace(period))
            {
                var parts = period.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out var y) &&
                    int.TryParse(parts[1], out var m) &&
                    m is >= 1 and <= 12)
                {
                    seciliYil = y;
                    seciliAy = m;
                }
            }

            var seciliAyBaslangic = new DateTime(seciliYil, seciliAy, 1);
            var seciliAyBitis = seciliAyBaslangic.AddMonths(1);

            static bool DurumEsitMi(string? value, string expected) =>
                string.Equals((value ?? string.Empty).Trim(), expected, StringComparison.OrdinalIgnoreCase);

            static bool DurumBekleniyorMu(string? value) =>
                (value ?? string.Empty).Contains("Bekleniyor", StringComparison.OrdinalIgnoreCase);

            var aySecenekleri = Enumerable.Range(0, 12)
                .Select(i =>
                {
                    var d = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
                    return (Year: d.Year, Month: d.Month, Label: d.ToString("MMMM yyyy", tr));
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            var musteriler = _context.Musteriler?.ToList() ?? new List<Musteri>();
            var jiraTasksAll = _context.JiraTasks?.ToList() ?? new List<JiraTask>();
            var toplantiNotlari = _context.ToplantiNotlari?.ToList() ?? new List<ToplantiNotu>();
            var ticketsAll = _context.Tickets?.ToList() ?? new List<Ticket>();
            var hatalar = _context.Hatalar?.ToList() ?? new List<Hata>();

            var jiraTasks = jiraTasksAll
                .Where(t => t.OlusturmaTarihi >= seciliAyBaslangic && t.OlusturmaTarihi < seciliAyBitis)
                .ToList();

            var jiraBeklemede = jiraTasks.Count(t => DurumEsitMi(t.Durum, "Beklemede"));
            var jiraAktif = jiraTasks.Count(t => DurumEsitMi(t.Durum, "Aktif"));
            var jiraTamam = jiraTasks.Count(t => DurumEsitMi(t.Durum, "Tamamlandı"));

            var tickets = ticketsAll
                .Where(t => t.OlusturmaTarihi >= seciliAyBaslangic && t.OlusturmaTarihi < seciliAyBitis)
                .ToList();

            var ticketOnaybekleniyor = tickets.Count(t => DurumBekleniyorMu(t.Durum));
            var ticketOnaylandi = tickets.Count(t => DurumEsitMi(t.Durum, "Musteri Kaydedildi"));
            var ticketReddedildi = tickets.Count(t => DurumEsitMi(t.Durum, "Reddedildi"));

            var aktifMusteri = musteriler.Count(m => DurumEsitMi(m.Durum, "Aktif"));
            var bekleyenIs = jiraBeklemede;

            var buAyEklenen = musteriler.Count(m =>
                m.KayitTarihi.HasValue &&
                m.KayitTarihi.Value >= seciliAyBaslangic &&
                m.KayitTarihi.Value < seciliAyBitis);

            var aylar = Enumerable.Range(0, 6)
                .Select(i => seciliAyBaslangic.AddMonths(-i))
                .OrderBy(x => x)
                .ToList();

            var aylikSayilar = new List<int>();
            var ayEtiketleri = new List<string>();

            foreach (var ay in aylar)
            {
                var ayStart = new DateTime(ay.Year, ay.Month, 1);
                var ayEnd = ayStart.AddMonths(1);

                int sayi = _context.Musteriler.Count(m =>
                    m.KayitTarihi.HasValue &&
                    m.KayitTarihi.Value >= ayStart &&
                    m.KayitTarihi.Value < ayEnd);

                aylikSayilar.Add(sayi);
                ayEtiketleri.Add(ay.ToString("MMMM yyyy", tr));
            }

            var paramDurumlar = _context.Parametreler
                .Where(p => p.Tur == "Durum" && p.ParAdi != null && p.ParAdi != "")
                .OrderBy(p => p.ParAdi)
                .Select(p => p.ParAdi!)
                .ToList();

            var musterilerSeciliAy = musteriler
                .Where(m =>
                    (m.KayitTarihi.HasValue && m.KayitTarihi.Value >= seciliAyBaslangic && m.KayitTarihi.Value < seciliAyBitis)
                    || (m.DurumDegisiklikTarihi.HasValue && m.DurumDegisiklikTarihi.Value >= seciliAyBaslangic && m.DurumDegisiklikTarihi.Value < seciliAyBitis))
                .ToList();

            var pasifMusteriSeciliAy = musterilerSeciliAy.Count(m => DurumEsitMi(m.Durum, "Pasif"));

            var durumSayac = musterilerSeciliAy
                .GroupBy(m => string.IsNullOrWhiteSpace(m.Durum) ? "" : m.Durum.Trim())
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var musteriDurumEtiketleri = new List<string>();
            var musteriDurumSayilari = new List<int>();

            foreach (var d in paramDurumlar)
            {
                musteriDurumEtiketleri.Add(d);
                musteriDurumSayilari.Add(durumSayac.TryGetValue(d, out var c) ? c : 0);
            }

            var otherCount = durumSayac
                .Where(kvp =>
                    !string.IsNullOrWhiteSpace(kvp.Key) &&
                    !paramDurumlar.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                .Sum(kvp => kvp.Value);

            if (otherCount > 0)
            {
                musteriDurumEtiketleri.Add("Diğer");
                musteriDurumSayilari.Add(otherCount);
            }

            var hatalarSeciliAy = hatalar
                .Where(h => h.OlusturmaTarihi.Year == seciliYil && h.OlusturmaTarihi.Month == seciliAy)
                .ToList();

            int HataDurumSay(string durum) =>
                hatalarSeciliAy.Count(h => string.Equals((h.Durum ?? "").Trim(), durum, StringComparison.OrdinalIgnoreCase));

            var model = new DashboardViewModel
            {
                ToplamMusteri = musteriler.Count,
                AktifMusteri = aktifMusteri,
                PasifMusteri = pasifMusteriSeciliAy,
                Bekleyen = bekleyenIs,
                BuAyEklenen = buAyEklenen,
                AylikMusteriSayilari = aylikSayilar,
                AyEtiketleri = ayEtiketleri,

                JiraBeklemede = jiraBeklemede,
                JiraAktif = jiraAktif,
                JiraTamamlandi = jiraTamam,

                TicketOnaybekleniyor = ticketOnaybekleniyor,
                TicketOnaylandi = ticketOnaylandi,
                TicketReddedildi = ticketReddedildi,
                ToplamTicket = tickets.Count,

                MusteriDurumEtiketleri = musteriDurumEtiketleri,
                MusteriDurumSayilari = musteriDurumSayilari,

                Musteriler = musteriler
                    .OrderByDescending(m => m.KayitTarihi ?? DateTime.MinValue)
                    .Take(10)
                    .ToList(),

                JiraTasks = jiraTasksAll
                    .OrderByDescending(t => t.OlusturmaTarihi)
                    .Take(10)
                    .ToList(),

                Tickets = ticketsAll
                    .OrderByDescending(t => t.OlusturmaTarihi)
                    .ToList(),

                ToplantiNotlari = toplantiNotlari
                    .OrderByDescending(n => n.Tarih)
                    .Take(10)
                    .ToList(),

                SeciliAy = seciliAy,
                SeciliYil = seciliYil,
                AySecenekleri = aySecenekleri,

                HataToplamSeciliAy = hatalarSeciliAy.Count,
                HataAcikSeciliAy = HataDurumSay("Açık"),
                HataBeklemedeSeciliAy = HataDurumSay("Beklemede"),
                HataKapaliSeciliAy = HataDurumSay("Kapalı")
            };

            return View(model);
        }
    }
}