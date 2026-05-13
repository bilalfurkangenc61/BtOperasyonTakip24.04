using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace BtOperasyonTakip.Controllers
{
    [Authorize(Roles = "Saha,KurumsalSaha,Operasyon,Uyum,Admin")]
    public class TicketController : Controller
    {
        private readonly AppDbContext _context;

        private const string TicketDurumOperasyon1 = "Operasyon 1 Onay Bekleniyor";
        private const string TicketDurumUyum = "Uyum Onayı Bekleniyor";
        private const string TicketDurumOperasyon2 = "Operasyon 2 Onay Bekleniyor";
        private const string TicketDurumEntegrasyonBekliyor = "Entegrasyon Bekleniyor";
        private const string TicketDurumSahaCanliBekleniyor = "Saha Canli Bekleniyor";
        private const string TicketDurumEksikEvrak = "Eksik Evrak Bekleniyor";
        private const string TicketDurumMusteriKaydedildi = "Musteri Kaydedildi";
        private const string TicketDurumReddedildi = "Reddedildi";

        private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();

        private int GetCurrentUserId() => int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

        private bool IsSahaFamilyUser() => User.IsInRole("Saha") || User.IsInRole("KurumsalSaha");

        private List<string> GetTicketDurumlari()
        {
            return _context.Parametreler
                .Where(p =>
                    (p.Tur == "TicketDurum" || p.Tur == "Durum") &&
                    p.ParAdi != null &&
                    p.ParAdi != "")
                .Select(p => p.ParAdi!)
                .Concat(
                    _context.Tickets
                        .Where(t => t.Durum != null && t.Durum != "")
                        .Select(t => t.Durum!))
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        private List<string> GetTicketTalepEdenleri()
        {
            return _context.Parametreler
                .Where(p =>
                    (p.Tur == "TalepEden" || p.Tur == "TalepSahibi") &&
                    p.ParAdi != null &&
                    p.ParAdi != "")
                .Select(p => p.ParAdi!)
                .Concat(
                    _context.Musteriler
                        .Where(m => m.TalepSahibi != null && m.TalepSahibi != "")
                        .Select(m => m.TalepSahibi!))
                .Concat(
                    _context.Tickets
                        .Where(t => t.OlusturanKullaniciAdi != null && t.OlusturanKullaniciAdi != "")
                        .Select(t => t.OlusturanKullaniciAdi!))
                .Concat(
                    _context.Users
                        .Where(u => u.Role == "Saha" || u.Role == "KurumsalSaha")
                        .Select(u => string.IsNullOrWhiteSpace(u.FullName) ? u.UserName : u.FullName!))
                .Where(x => x != null && x != "")
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        public TicketController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? searchFirma, string? durum, string? talepEden)
        {
            IQueryable<Ticket> baseQuery = _context.Tickets;

            if (IsSahaFamilyUser())
            {
                var userId = GetCurrentUserId();
                baseQuery = baseQuery.Where(t => t.OlusturanUserId == userId);
            }
            else if (User.IsInRole("Uyum"))
            {
                baseQuery = baseQuery.Where(t => t.Durum == TicketDurumUyum);
            }
            else if (User.IsInRole("Operasyon"))
            {
                var userId = GetCurrentUserId();
                baseQuery = baseQuery.Where(t =>
                    t.AtananOperasyonUserId == userId &&
                    (t.Durum == TicketDurumOperasyon1 ||
                     t.Durum == TicketDurumOperasyon2 ||
                     t.Durum == TicketDurumEntegrasyonBekliyor ||
                     t.Durum == TicketDurumSahaCanliBekleniyor ||
                     t.Durum == TicketDurumMusteriKaydedildi ||
                     t.Durum == TicketDurumReddedildi));
            }

            var durumlar = GetTicketDurumlari();
            var talepEdenler = GetTicketTalepEdenleri();

            IQueryable<Ticket> query = baseQuery;

            if (!string.IsNullOrWhiteSpace(searchFirma))
            {
                query = query.Where(t =>
                    t.FirmaAdi.Contains(searchFirma) ||
                    t.MusteriWebSitesi.Contains(searchFirma) ||
                    t.YazilimciAdi.Contains(searchFirma) ||
                    t.YazilimciSoyadi.Contains(searchFirma));
            }

            if (!string.IsNullOrWhiteSpace(durum))
            {
                query = query.Where(t => t.Durum == durum);
            }

            if (!string.IsNullOrWhiteSpace(talepEden))
            {
                var selectedTalepEden = talepEden.Trim();
                var matchingUserIds = _context.Users
                    .Where(u =>
                        (u.Role == "Saha" || u.Role == "KurumsalSaha") &&
                        (string.IsNullOrWhiteSpace(u.FullName) ? u.UserName : u.FullName!) == selectedTalepEden)
                    .Select(u => u.Id)
                    .ToList();

                query = query.Where(t =>
                    t.OlusturanKullaniciAdi == selectedTalepEden ||
                    matchingUserIds.Contains(t.OlusturanUserId));
            }

            if (User.IsInRole("Admin"))
            {
                var operasyonUsers = _context.Users
                    .Where(u => u.Role == "Operasyon")
                    .OrderBy(u => u.Id)
                    .Select(u => new
                    {
                        u.Id,
                        Name = (u.FullName ?? u.UserName)
                    })
                    .ToList();

                ViewBag.OperasyonUsers = operasyonUsers;
            }

            var tickets = query.OrderByDescending(t => t.OlusturmaTarihi).ToList();
            ViewBag.SearchFirma = searchFirma;
            ViewBag.SelectedDurum = durum;
            ViewBag.SelectedTalepEden = talepEden;
            ViewBag.Durumlar = durumlar;
            ViewBag.TalepEdenler = talepEdenler;

            return View(tickets);
        }


        [HttpGet]
        public IActionResult Create()
        {
            if (!IsSahaFamilyUser())
                return Unauthorized();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ticket ticket)
        {
            if (!IsSahaFamilyUser())
                return Unauthorized();

            var odemeTurleriValues = Request.Form["OdemeTurleri"].Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
            var odemeYontemleriValues = Request.Form["OdemeYontemleri"].Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();

            ticket.OdemeTurleri = odemeTurleriValues.Length > 0
                ? string.Join(", ", odemeTurleriValues)
                : null;

            ticket.OdemeYontemleri = odemeYontemleriValues.Length > 0
                ? string.Join(", ", odemeYontemleriValues)
                : null;

            if (!ModelState.IsValid)
            {
                return View(ticket);
            }

            var firmaAdi = (ticket.FirmaAdi ?? string.Empty).Trim();
            var siteAdi = (ticket.MusteriWebSitesi ?? string.Empty).Trim();
            var mail = (ticket.Mail ?? string.Empty).Trim();

            var firmaKey = Normalize(firmaAdi);
            var siteKey = Normalize(siteAdi);
            var mailKey = Normalize(mail);

            var acikTickets = _context.Tickets.Where(t =>
                t.Durum != TicketDurumReddedildi &&
                t.Durum != TicketDurumMusteriKaydedildi);

            if (!string.IsNullOrWhiteSpace(firmaAdi))
            {
                var firmaVar = acikTickets.Any(t =>
                    t.FirmaAdi != null &&
                    t.FirmaAdi.Trim().ToLower() == firmaKey);
                if (firmaVar)
                    ModelState.AddModelError(nameof(ticket.FirmaAdi), "Bu firma adı ile açık bir ticket zaten var.");
            }

            if (!string.IsNullOrWhiteSpace(siteAdi))
            {
                var siteVar = acikTickets.Any(t =>
                    t.MusteriWebSitesi != null &&
                    t.MusteriWebSitesi.Trim().ToLower() == siteKey);
                if (siteVar)
                    ModelState.AddModelError(nameof(ticket.MusteriWebSitesi), "Bu web sitesi ile açık bir ticket zaten var.");
            }

            if (!string.IsNullOrWhiteSpace(mail))
            {
                var mailVar = acikTickets.Any(t =>
                    t.Mail != null &&
                    t.Mail.Trim().ToLower() == mailKey);
                if (mailVar)
                    ModelState.AddModelError(nameof(ticket.Mail), "Bu mail adresi ile açık bir ticket zaten var.");
            }

            if (!ModelState.IsValid)
            {
                return View(ticket);
            }

            try
            {
                var userId = GetCurrentUserId();

                if (!string.Equals(ticket.MusteriTipi, "Kurumsal", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(ticket.MusteriTipi, "Bireysel", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(ticket.MusteriTipi), "Müşteri tipi 'Kurumsal' veya 'Bireysel' olmalıdır.");
                    return View(ticket);
                }

                ticket.OlusturanUserId = userId;
                ticket.OlusturanKullaniciAdi = User.Identity?.Name ?? "Bilinmiyor";
                ticket.OlusturmaTarihi = DateTime.UtcNow;

                ticket.Durum = TicketDurumOperasyon1;

    
                if (User.IsInRole("KurumsalSaha"))
                {
                    var test = _context.Users.FirstOrDefault(u =>
                        u.Role == "Operasyon" &&
                        Normalize(u.UserName) == "test");

                    if (test == null)
                    {
                        ModelState.AddModelError("", "Kurumsal ticket ataması için 'test' kullanıcısı bulunamadı. Kullanıcı Operasyon rolünde olmalı ve UserName değeri 'test' olmalıdır.");
                        return View(ticket);
                    }

                    ticket.AtananOperasyonUserId = test.Id;
                    ticket.AtananOperasyonKullaniciAdi = test.FullName ?? test.UserName;
                    ticket.AtanmaTarihi = DateTime.UtcNow;
                }
                else
                {
                    var operasyonAdaylari = _context.Users
                        .Where(u => u.Role == "Operasyon")
                        .OrderBy(u => u.Id)
                        .Take(5)
                        .ToList();

                    if (operasyonAdaylari.Count > 0)
                    {
                        var index = Random.Shared.Next(operasyonAdaylari.Count);
                        var secilen = operasyonAdaylari[index];

                        ticket.AtananOperasyonUserId = secilen.Id;
                        ticket.AtananOperasyonKullaniciAdi = secilen.FullName ?? secilen.UserName;
                        ticket.AtanmaTarihi = DateTime.UtcNow;
                    }
                }

                _context.Tickets.Add(ticket);
                _context.SaveChanges();

                TempData["Success"] = "Ticket oluşturuldu. Operasyon 1 onayı bekleniyor.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
                return View(ticket);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Operasyon")]
        public IActionResult Operasyon1Decide([FromBody] Operasyon1DecideRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz ticket ID." });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı." });

            if (ticket.Durum != TicketDurumOperasyon1)
                return Json(new { success = false, message = $"Ticket bu aşamada Operasyon 1 kararına uygun değil. Durum: {ticket.Durum}" });

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            if (ticket.AtananOperasyonUserId != userId)
                return Json(new { success = false, message = "Bu ticket size atanmadığı için işlem yapamazsınız." });

            ticket.EntegreOlabilirMi = request.EntegreOlabilirMi;
            ticket.EntegrasyonNotu = string.IsNullOrWhiteSpace(request.Not) ? null : request.Not.Trim();
            ticket.OnaylayanUserId = userId;
            ticket.OnaylayanKullaniciAdi = User.Identity?.Name ?? "Bilinmiyor";
            ticket.Operasyon1OnayTarihi = DateTime.UtcNow;

            ticket.Durum = TicketDurumUyum;

            _context.SaveChanges();
            return Json(new { success = true, message = "Operasyon 1 kararı kaydedildi. Uyum onayı bekleniyor." });
        }

        [HttpPost]
        [Authorize(Roles = "Operasyon")]
        public IActionResult Operasyon2Decide([FromBody] Operasyon2DecideRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz ticket ID." });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı." });

            if (ticket.Durum != TicketDurumOperasyon2)
                return Json(new { success = false, message = $"Ticket bu aşamada Operasyon 2 kararına uygun değil. Durum: {ticket.Durum}" });

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            if (ticket.AtananOperasyonUserId != userId)
                return Json(new { success = false, message = "Bu ticket size atanmadığı için işlem yapamazsınız." });

            ticket.MailGonderildiMi = request.MailGonderildiMi;
            ticket.MailNotu = string.IsNullOrWhiteSpace(request.Not) ? null : request.Not.Trim();
            ticket.Operasyon2OnayTarihi = DateTime.UtcNow;

            if (request.MailGonderildiMi)
            {
                ticket.Durum = TicketDurumEntegrasyonBekliyor;

                var jiraId = $"TICKET-{ticket.Id}";
                var talepKonusu = $"Entegrasyon: {ticket.FirmaAdi}".Trim();
                var talepAcan = (ticket.AtananOperasyonKullaniciAdi ?? User.Identity?.Name ?? "Sistem").Trim();

                var existing = _context.JiraTasks.FirstOrDefault(x => x.JiraId == jiraId);
                if (existing == null)
                {
                    _context.JiraTasks.Add(new JiraTask
                    {
                        JiraId = jiraId,
                        TalepKonusu = talepKonusu,
                        TalepTuru = "Entegrasyon",
                        TalepAcan = talepAcan,
                        Durum = "Beklemede",
                        TakipEden = ticket.AtananOperasyonKullaniciAdi
                    });
                }
                else
                {
                    existing.TalepTuru = "Entegrasyon";
                    existing.Durum = "Beklemede";
                    if (string.IsNullOrWhiteSpace(existing.TalepKonusu))
                        existing.TalepKonusu = talepKonusu;
                    if (string.IsNullOrWhiteSpace(existing.TalepAcan))
                        existing.TalepAcan = talepAcan;
                }
            }

            _context.SaveChanges();
            return Json(new { success = true, message = "Operasyon 2 kararı kaydedildi." });
        }

        [HttpPost]
        [Authorize(Roles = "Operasyon,Admin")]
        public IActionResult EntegrasyonTamamlandi([FromBody] EntegrasyonTamamlandiRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz ticket ID." });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı." });

            if (ticket.Durum != TicketDurumEntegrasyonBekliyor)
                return Json(new { success = false, message = $"Ticket bu aşamada entegrasyon tamamlama işlemine uygun değil. Durum: {ticket.Durum}" });

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            if (!User.IsInRole("Admin") && ticket.AtananOperasyonUserId != userId)
                return Json(new { success = false, message = "Bu ticket size atanmadığı için işlem yapamazsınız." });

            ticket.Durum = TicketDurumSahaCanliBekleniyor;

            var jiraId = $"TICKET-{ticket.Id}";
            var task = _context.JiraTasks.FirstOrDefault(x => x.JiraId == jiraId);
            if (task != null)
                task.Durum = "Aktif";

            _context.SaveChanges();
            return Json(new { success = true, message = "Entegrasyon tamamlandı. Ticket tekrar Saha Canli Bekleniyor durumuna alındı." });
        }

        [HttpPost]
        [Authorize(Roles = "Saha,KurumsalSaha,Admin")]
        public IActionResult SahaCanliAcildi([FromBody] SahaCanliRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz ticket ID." });

            var canliOrtamId = (request.CanliOrtamId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(canliOrtamId))
                return Json(new { success = false, message = "Canlı ortam ID girmek zorunludur." });

            if (!canliOrtamId.All(char.IsDigit))
                return Json(new { success = false, message = "Canlı ortam ID sadece rakamlardan oluşmalıdır." });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı." });

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            if (!User.IsInRole("Admin") && ticket.OlusturanUserId != userId)
                return Json(new { success = false, message = "Sadece ticket'ı açan saha kullanıcısı bu işlemi yapabilir." });

            if (ticket.Durum != TicketDurumSahaCanliBekleniyor)
                return Json(new { success = false, message = $"Ticket bu aşamada canlı açılışa uygun değil. Durum: {ticket.Durum}" });

            ticket.CanliAcildiTarihi = DateTime.UtcNow;
            var not = (request.Not ?? string.Empty).Trim();
            ticket.CanliNotu = string.IsNullOrWhiteSpace(not)
                ? $"CanlıOrtamId: {canliOrtamId}"
                : $"CanlıOrtamId: {canliOrtamId} | {not}";

            var firmaAdi = (ticket.FirmaAdi ?? string.Empty).Trim();
            var siteUrl = (ticket.MusteriWebSitesi ?? string.Empty).Trim();

            var firmaKey = firmaAdi.ToLowerInvariant();
            var siteKey = siteUrl.ToLowerInvariant();

            var mevcutMusteri = _context.Musteriler.FirstOrDefault(m =>
                ((m.Firma ?? string.Empty).Trim().ToLower()) == firmaKey &&
                ((m.SiteUrl ?? string.Empty).Trim().ToLower()) == siteKey);

            var kaynak = string.Equals((ticket.MusteriTipi ?? string.Empty).Trim(), "Kurumsal", StringComparison.OrdinalIgnoreCase)
                ? "Kurumsal"
                : "Bireysel";

            Musteri musteri;
            if (mevcutMusteri != null)
            {
                mevcutMusteri.Firma = string.IsNullOrWhiteSpace(firmaAdi) ? mevcutMusteri.Firma : firmaAdi;
                mevcutMusteri.FirmaYetkilisi = $"{ticket.YazilimciAdi} {ticket.YazilimciSoyadi}";
                mevcutMusteri.Telefon = ticket.IrtibatNumarasi;
                mevcutMusteri.SiteUrl = siteUrl;
                mevcutMusteri.Teknoloji = ticket.TeknolojiBilgisi;
                mevcutMusteri.Durum = "Aktif";
                mevcutMusteri.TalepSahibi = ticket.OlusturanKullaniciAdi;
                mevcutMusteri.Aciklama = ticket.Aciklama;
                mevcutMusteri.KayitTarihi = DateTime.Now;

                mevcutMusteri.Kaynak = kaynak;

                musteri = mevcutMusteri;
                _context.SaveChanges();
            }
            else
            {
                musteri = new Musteri
                {
                    Firma = firmaAdi,
                    FirmaYetkilisi = $"{ticket.YazilimciAdi} {ticket.YazilimciSoyadi}",
                    Telefon = ticket.IrtibatNumarasi,
                    SiteUrl = siteUrl,
                    Teknoloji = ticket.TeknolojiBilgisi,
                    Durum = "Aktif",
                    TalepSahibi = ticket.OlusturanKullaniciAdi,
                    Aciklama = ticket.Aciklama,
                    KayitTarihi = DateTime.Now,
                    Kaynak = kaynak
                };

                _context.Musteriler.Add(musteri);
                _context.SaveChanges();
            }

            ticket.MusteriID = musteri.MusteriID;
            ticket.Durum = TicketDurumMusteriKaydedildi;

            _context.SaveChanges();
            return Json(new { success = true, message = "Canlı açılış kaydedildi. Müşteri kaydı oluşturuldu veya güncellendi." });
        }

        [HttpPost]
        [Authorize(Roles = "Saha,KurumsalSaha,Admin")]
        public IActionResult SahaReddet([FromBody] SahaRedRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz ticket ID." });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı." });

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            if (!User.IsInRole("Admin") && ticket.OlusturanUserId != userId)
                return Json(new { success = false, message = "Sadece ticket'ı açan saha kullanıcısı bu işlemi yapabilir." });

            var not = (request.Not ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(not))
                return Json(new { success = false, message = "Açıklama zorunlu." });

            ticket.Durum = TicketDurumReddedildi;
            ticket.KararAciklamasi = not;

            _context.SaveChanges();
            return Json(new { success = true, message = "Ticket reddedildi." });
        }

        [HttpPost]
        [Authorize(Roles = "Saha,KurumsalSaha,Admin")]
        public IActionResult SahaEksikEvrakTamamlandi([FromBody] SahaEksikEvrakTamamlandiRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz ticket ID." });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı." });

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
            if (!User.IsInRole("Admin") && ticket.OlusturanUserId != userId)
                return Json(new { success = false, message = "Sadece ticket'ı açan saha kullanıcısı bu işlemi yapabilir." });

            if (ticket.Durum != TicketDurumEksikEvrak)
                return Json(new { success = false, message = $"Ticket bu aşamada eksik evrak tamamlama işlemine uygun değil. Durum: {ticket.Durum}" });

            ticket.Durum = TicketDurumUyum;
            ticket.UyumOnaylayanUserId = null;
            ticket.UyumOnaylayanKullaniciAdi = null;
            ticket.UyumOnayTarihi = null;

            _context.SaveChanges();
            return Json(new { success = true, message = "Eksik evrak tamamlandı. Ticket yeniden uyum onayına gönderildi." });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangeAssignment([FromBody] ChangeAssignmentRequest request)
        {
            if (request == null)
                return BadRequest(new { success = false, message = "Geçersiz istek." });

            if (request.TicketId <= 0)
                return BadRequest(new { success = false, message = "Geçersiz Ticket ID." });

            var neden = (request.Neden ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(neden))
                return BadRequest(new { success = false, message = "Değişiklik nedeni zorunlu." });

            if (neden.Length > 500)
                return BadRequest(new { success = false, message = "Değişiklik nedeni maksimum 500 karakter olmalıdır." });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.TicketId);
            if (ticket == null)
                return NotFound(new { success = false, message = "Ticket bulunamadı." });

            var oldAssigneeName = ticket.AtananOperasyonKullaniciAdi;
            var oldAssigneeId = ticket.AtananOperasyonUserId;
            var oldStatus = ticket.Durum;

            var assignmentChanged = false;
            var statusChanged = false;

            if (request.YeniOperasyonUserId > 0 && request.YeniOperasyonUserId != ticket.AtananOperasyonUserId)
            {
                var newUser = _context.Users.FirstOrDefault(u => u.Id == request.YeniOperasyonUserId && u.Role == "Operasyon");
                if (newUser == null)
                    return BadRequest(new { success = false, message = "Seçilen kullanıcı operasyon rolünde değil veya bulunamadı." });

                ticket.AtananOperasyonUserId = newUser.Id;
                ticket.AtananOperasyonKullaniciAdi = newUser.FullName ?? newUser.UserName;
                ticket.AtanmaTarihi = DateTime.UtcNow;
                assignmentChanged = true;
            }

            var yeniDurum = (request.YeniDurum ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(yeniDurum) && !string.Equals(ticket.Durum, yeniDurum, StringComparison.OrdinalIgnoreCase))
            {
                var gecerliDurumlar = GetTicketDurumlari();
                if (!gecerliDurumlar.Contains(yeniDurum, StringComparer.OrdinalIgnoreCase))
                    return BadRequest(new { success = false, message = "Seçilen durum geçerli değil." });

                ticket.Durum = yeniDurum;
                statusChanged = true;
            }

            if (!assignmentChanged && !statusChanged)
                return BadRequest(new { success = false, message = "Kişi veya durum değişikliği yapmadınız." });

            var adminId = int.TryParse(User.FindFirst("UserId")?.Value, out var adminUid) ? adminUid : 0;
            var adminName = User.Identity?.Name ?? "Bilinmiyor";

            if (assignmentChanged)
            {
                _context.TicketAtamaLoglari.Add(new TicketAtamaLog
                {
                    TicketId = ticket.Id,
                    EskiOperasyonUserId = oldAssigneeId,
                    EskiOperasyonKullaniciAdi = oldAssigneeName,
                    YeniOperasyonUserId = ticket.AtananOperasyonUserId ?? 0,
                    YeniOperasyonKullaniciAdi = ticket.AtananOperasyonKullaniciAdi,
                    DegisiklikNedeni = neden,
                    DegistirenUserId = adminId,
                    DegistirenKullaniciAdi = adminName,
                    DegisiklikTarihi = DateTime.UtcNow
                });
            }

            _context.SaveChanges();
            var mesaj = assignmentChanged && statusChanged
                ? $"Kişi ve durum güncellendi. ({oldStatus} → {ticket.Durum})"
                : assignmentChanged
                    ? "Kişi ataması güncellendi."
                    : $"Durum güncellendi. ({oldStatus} → {ticket.Durum})";

            return Json(new { success = true, message = mesaj });
        }

        public sealed class Operasyon1DecideRequest
        {
            public int Id { get; set; }
            public bool EntegreOlabilirMi { get; set; }
            public string? Not { get; set; }
        }

        public sealed class Operasyon2DecideRequest
        {
            public int Id { get; set; }
            public bool MailGonderildiMi { get; set; }
            public string? Not { get; set; }
        }

        public sealed class EntegrasyonTamamlandiRequest
        {
            public int Id { get; set; }
        }

        public sealed class SahaCanliRequest
        {
            public int Id { get; set; }
            public string? Not { get; set; }
            public string? CanliOrtamId { get; set; }
        }

        public sealed class SahaRedRequest
        {
            public int Id { get; set; }
            public string? Not { get; set; }
        }

        public sealed class SahaEksikEvrakTamamlandiRequest
        {
            public int Id { get; set; }
        }

        public sealed class ChangeAssignmentRequest
        {
            public int TicketId { get; set; }
            public int YeniOperasyonUserId { get; set; }
            public string? YeniDurum { get; set; }
            public string? Neden { get; set; }
        }
    }
}