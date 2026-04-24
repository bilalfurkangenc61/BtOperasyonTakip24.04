using BtOperasyonTakip.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace BtOperasyonTakip.Controllers
{
    [Authorize(Roles = "Uyum")]
    public class UyumController : Controller
    {
        private readonly AppDbContext _context;
        private const string TicketDurumUyum = "Uyum Onayı Bekleniyor";
        private const string TicketDurumOperasyon2 = "Operasyon 2 Onay Bekleniyor";
        private const string TicketDurumSahaCanliBekleniyor = "Saha Canli Bekleniyor";
        private const string TicketDurumMusteriKaydedildi = "Musteri Kaydedildi";
        private const string TicketDurumReddedildi = "Reddedildi";
        private const string TicketDurumEksikEvrak = "Eksik Evrak Bekleniyor";

        public UyumController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string? durumFilter, string? searchWebsite)
        {
            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;

            var filter = string.IsNullOrWhiteSpace(durumFilter) ? "Bekleyen" : durumFilter.Trim();
            var query = _context.Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchWebsite))
                query = query.Where(t => t.MusteriWebSitesi.Contains(searchWebsite));

            query = filter switch
            {
                "Tümü" => query.Where(t =>
                    t.Durum == TicketDurumUyum ||
                    t.Durum == TicketDurumOperasyon2 ||
                    t.Durum == TicketDurumSahaCanliBekleniyor ||
                    t.Durum == TicketDurumMusteriKaydedildi ||
                    t.Durum == TicketDurumReddedildi ||
                    t.Durum == TicketDurumEksikEvrak),

                "Bekleyen" => query.Where(t => t.Durum == TicketDurumUyum),

                "Onaylanan" => query.Where(t => t.UyumOnaylayanUserId == userId && t.Durum != TicketDurumUyum),

                "Reddedilen" => query.Where(t => t.UyumOnaylayanUserId == userId && t.Durum == TicketDurumReddedildi),

                "Eksik Evrak" => query.Where(t => t.UyumOnaylayanUserId == userId && t.Durum == TicketDurumEksikEvrak),

                _ => query.Where(t => t.Durum == TicketDurumUyum)
            };

            var tickets = query
                .OrderByDescending(t => t.UyumOnayTarihi ?? t.OlusturmaTarihi)
                .ToList();

            ViewBag.DurumFilter = filter;
            ViewBag.SearchWebsite = searchWebsite;

            return View(tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Decide(int id, string karar, string? aciklama)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);
            if (ticket == null)
                return NotFound();

            if (ticket.Durum != TicketDurumUyum)
                return BadRequest($"Ticket bu aşamada uyum kararına uygun değil. Durum: {ticket.Durum}");

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;

            ticket.UyumOnaylayanUserId = userId;
            ticket.UyumOnaylayanKullaniciAdi = User.Identity?.Name ?? "Bilinmiyor";
            ticket.UyumOnayTarihi = DateTime.UtcNow;
            ticket.UyumKararAciklamasi = string.IsNullOrWhiteSpace(aciklama) ? null : aciklama.Trim();

            if (string.Equals(karar, "Onay", StringComparison.OrdinalIgnoreCase))
            {
                ticket.Durum = TicketDurumOperasyon2;
                _context.SaveChanges();
                TempData["Success"] = "✅ Uyum onayladı. Ticket Operasyon 2 onayına gönderildi.";
                return RedirectToAction(nameof(Index), new { durumFilter = "Bekleyen" });
            }

            if (string.Equals(karar, "EksikEvrak", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(aciklama))
                    return BadRequest("Eksik evrak seçildiğinde açıklama girmek zorunludur.");

                ticket.Durum = TicketDurumEksikEvrak;
                _context.SaveChanges();
                TempData["Success"] = "⚠️ Ticket eksik evrak nedeniyle sahaya geri gönderildi.";
                return RedirectToAction(nameof(Index), new { durumFilter = "Bekleyen" });
            }

            if (string.Equals(karar, "Red", StringComparison.OrdinalIgnoreCase))
            {
                ticket.Durum = TicketDurumReddedildi;
                _context.SaveChanges();
                TempData["Error"] = "❌ Ticket uyum tarafından reddedildi.";
                return RedirectToAction(nameof(Index), new { durumFilter = "Bekleyen" });
            }

            return BadRequest("Geçersiz karar. (Onay/EksikEvrak/Red)");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, string? aciklama) => Decide(id, "Onay", aciklama);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, string? aciklama) => Decide(id, "Red", aciklama);
    }
}