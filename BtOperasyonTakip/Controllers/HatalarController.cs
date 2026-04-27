using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using BtOperasyonTakip.Models.ViewModels;
using BtOperasyonTakip.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace BtOperasyonTakip.Controllers
{
    [Authorize(Roles = AppRoles.Operasyon + "," + AppRoles.Admin + "," + AppRoles.Saha)]
    public class HatalarController : Controller
    {
        private readonly AppDbContext _context;

        public HatalarController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string q, string durum, string kategori, string? period)
        {
            var query =
                from h in _context.Hatalar.AsNoTracking()
                join m in _context.Musteriler.AsNoTracking() on h.MusteriID equals m.MusteriID into mj
                from m in mj.DefaultIfEmpty()
                select new { h, m };

            query = query.Where(x => x.h.Durum != "Satışa Gönderildi");

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    x.h.HataAdi.Contains(term) ||
                    x.h.HataAciklama.Contains(term) ||
                    x.h.OlusturanKullaniciAdi.Contains(term) ||
                    (x.m != null && x.m.Firma != null && x.m.Firma.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(durum))
                query = query.Where(x => x.h.Durum == durum);

            if (!string.IsNullOrWhiteSpace(kategori))
                query = query.Where(x => x.h.KategoriBilgisi == kategori);

            if (TryParsePeriod(period, out var start, out var end))
                query = query.Where(x => x.h.OlusturmaTarihi >= start && x.h.OlusturmaTarihi < end);

            var hatalar = await query
                .OrderByDescending(x => x.h.OlusturmaTarihi)
                .Select(x => new HataListItemVm
                {
                    Id = x.h.Id,
                    HataAdi = x.h.HataAdi,
                    KategoriBilgisi = x.h.KategoriBilgisi,
                    OlusturanKullaniciAdi = x.h.OlusturanKullaniciAdi,
                    Durum = x.h.Durum,
                    OlusturmaTarihi = x.h.OlusturmaTarihi,
                    MusteriFirma = x.m != null ? x.m.Firma : null
                })
                .ToListAsync();

            var mevcutHatalar = await _context.Hatalar
                .AsNoTracking()
                .Where(h => h.Durum == "Açık")
                .OrderBy(h => h.HataAdi)
                .ToListAsync();

            ViewBag.MevcutHatalar = mevcutHatalar;
            ViewBag.Q = q;
            ViewBag.Durum = durum;
            ViewBag.Kategori = kategori;
            ViewBag.Period = period;

            return View(hatalar);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel(string q, string durum, string kategori, string? period)
        {
            var query =
                from h in _context.Hatalar.AsNoTracking()
                join m in _context.Musteriler.AsNoTracking() on h.MusteriID equals m.MusteriID into mj
                from m in mj.DefaultIfEmpty()
                select new { h, m };

            query = query.Where(x => x.h.Durum != "Satışa Gönderildi");

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(x =>
                    x.h.HataAdi.Contains(term) ||
                    x.h.HataAciklama.Contains(term) ||
                    x.h.OlusturanKullaniciAdi.Contains(term) ||
                    (x.m != null && x.m.Firma != null && x.m.Firma.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(durum))
                query = query.Where(x => x.h.Durum == durum);

            if (!string.IsNullOrWhiteSpace(kategori))
                query = query.Where(x => x.h.KategoriBilgisi == kategori);

            if (TryParsePeriod(period, out var start, out var end))
                query = query.Where(x => x.h.OlusturmaTarihi >= start && x.h.OlusturmaTarihi < end);

            var data = await query
                .OrderByDescending(x => x.h.OlusturmaTarihi)
                .Select(x => new
                {
                    x.h.HataAdi,
                    x.h.HataAciklama,
                    x.h.KategoriBilgisi,
                    x.h.OlusturanKullaniciAdi,
                    MusteriFirma = x.m != null ? x.m.Firma : null,
                    x.h.Durum,
                    x.h.OlusturmaTarihi
                })
                .ToListAsync();

            var sb = new StringBuilder();

            sb.Append('\uFEFF');

            sb.AppendLine(string.Join(';', new[]
            {
                "Hata",
                "Açıklama",
                "Müşteri",
                "Kategori",
                "Atanan Kullanıcı",
                "Durum",
                "Tarih"
            }));

            foreach (var x in data)
            {
                sb.AppendLine(string.Join(';', new[]
                {
                    CsvEscape(x.HataAdi),
                    CsvEscape(x.HataAciklama),
                    CsvEscape(x.MusteriFirma ?? "-"),
                    CsvEscape(x.KategoriBilgisi),
                    CsvEscape(x.OlusturanKullaniciAdi),
                    CsvEscape(x.Durum),
                    CsvEscape(x.OlusturmaTarihi.ToString("dd.MM.yyyy HH:mm", CultureInfo.GetCultureInfo("tr-TR")))
                }));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var safePeriod = string.IsNullOrWhiteSpace(period) ? "tum-aylar" : period;
            var fileName = $"hatalar_{safePeriod}_{DateTime.Now:yyyyMMdd_HHmm}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> Yeni()
        {
            await LoadYeniViewDataAsync();

            return View(new Hata());
        }

        [HttpPost]
        public async Task<IActionResult> Yeni(Hata h, int? atanacakUserId)
        {
            ModelState.Remove("OperasyonCevabi");
            ModelState.Remove("CevaplayanKullaniciAdi");
            ModelState.Remove("CevaplaamaTarihi");
            ModelState.Remove("OlusturanUserId");
            ModelState.Remove("OlusturanKullaniciAdi");
            ModelState.Remove("OlusturmaTarihi");
            ModelState.Remove("SecilenHataId");

            if (h.MusteriID is null || !await _context.Musteriler.AnyAsync(x => x.MusteriID == h.MusteriID.Value))
                ModelState.AddModelError(nameof(Hata.MusteriID), "Lütfen bir müşteri seçiniz.");

            User? atanacakKullanici = null;
            var currentUserId = int.Parse(User.FindFirst("UserId")!.Value);

            if (User.IsInRole(AppRoles.Admin))
            {
                if (atanacakUserId is null)
                {
                    ModelState.AddModelError("atanacakUserId", "Lütfen atanacak kullanıcı seçiniz.");
                }
                else
                {
                    atanacakKullanici = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == atanacakUserId.Value);
                    if (atanacakKullanici == null)
                        ModelState.AddModelError("atanacakUserId", "Seçilen kullanıcı bulunamadı.");
                }
            }
            else
            {
                atanacakKullanici = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId);
            }

            if (!ModelState.IsValid)
            {
                await LoadYeniViewDataAsync();

                return View(h);
            }

            h.OlusturanUserId = atanacakKullanici?.Id ?? currentUserId;
            h.OlusturanKullaniciAdi = !string.IsNullOrWhiteSpace(atanacakKullanici?.FullName)
                ? atanacakKullanici!.FullName!
                : atanacakKullanici?.UserName ?? User.Identity!.Name!;
            h.OlusturmaTarihi = DateTime.Now;
            h.OperasyonCevabi = "";
            h.CevaplayanKullaniciAdi = "";

            _context.Hatalar.Add(h);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(string hataAdi, string hataAciklama, string kategori, int? mevcutHataId, int? musteriId, int? atanacakUserId)
        {
            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var userName = User.Identity!.Name!;

            if (musteriId is null || !await _context.Musteriler.AnyAsync(x => x.MusteriID == musteriId.Value))
                return BadRequest("Lütfen geçerli bir müşteri seçiniz.");

            User? atanacakKullanici;
            if (User.IsInRole(AppRoles.Admin))
            {
                if (atanacakUserId is null)
                    return BadRequest("Lütfen atanacak kullanıcı seçiniz.");

                atanacakKullanici = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == atanacakUserId.Value);
                if (atanacakKullanici == null)
                    return BadRequest("Seçilen kullanıcı bulunamadı.");
            }
            else
            {
                atanacakKullanici = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            }

            var hata = new Hata
            {
                HataAdi = hataAdi,
                HataAciklama = hataAciklama,
                KategoriBilgisi = kategori,
                Durum = "Açık",
                SecilenHataId = mevcutHataId,
                MusteriID = musteriId,
                OlusturanUserId = atanacakKullanici?.Id ?? userId,
                OlusturanKullaniciAdi = !string.IsNullOrWhiteSpace(atanacakKullanici?.FullName)
                    ? atanacakKullanici!.FullName!
                    : atanacakKullanici?.UserName ?? userName,
                OlusturmaTarihi = DateTime.Now,
                OperasyonCevabi = "",
                CevaplayanKullaniciAdi = ""
            };

            _context.Hatalar.Add(hata);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Detay(int id)
        {
            var hata = await _context.Hatalar.FindAsync(id);
            if (hata == null) return NotFound();

            if (User.IsInRole(AppRoles.Admin))
            {
                ViewBag.Kullanicilar = await _context.Users
                    .AsNoTracking()
                    .OrderBy(x => x.FullName ?? x.UserName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = !string.IsNullOrWhiteSpace(x.FullName) ? x.FullName! : x.UserName
                    })
                    .ToListAsync();
            }

            return View(hata);
        }

        [HttpPost]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> AtaDegistir(int id, int atanacakUserId)
        {
            var hata = await _context.Hatalar.FindAsync(id);
            if (hata == null) return NotFound();

            var atanacakKullanici = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == atanacakUserId);

            if (atanacakKullanici == null)
                return NotFound();

            hata.OlusturanUserId = atanacakKullanici.Id;
            hata.OlusturanKullaniciAdi = !string.IsNullOrWhiteSpace(atanacakKullanici.FullName)
                ? atanacakKullanici.FullName!
                : atanacakKullanici.UserName;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Detay), new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Cevapla(int id, string cevap, string durum)
        {
            var hata = await _context.Hatalar.FindAsync(id);
            if (hata == null) return NotFound();

            var userId = int.Parse(User.FindFirst("UserId")!.Value);
            var fullName = User.Identity!.Name;

            if (hata.OlusturanUserId != userId)
                return Forbid();

            hata.OperasyonCevabi = cevap;
            hata.Durum = durum;
            hata.CevaplayanUserId = userId;
            hata.CevaplayanKullaniciAdi = fullName;
            hata.CevaplaamaTarihi = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["HataYanitiVar"] = "ok";

            return RedirectToAction("Detay", new { id });
        }

        private static string CsvEscape(string? value)
        {
            value ??= "";
            var mustQuote = value.Contains(';') || value.Contains('\"') || value.Contains('\n') || value.Contains('\r');
            value = value.Replace("\"", "\"\"");
            return mustQuote ? $"\"{value}\"" : value;
        }

        private static bool TryParsePeriod(string? period, out DateTime start, out DateTime end)
        {
            start = default;
            end = default;

            if (string.IsNullOrWhiteSpace(period))
                return false;

            if (!DateTime.TryParseExact(
                    period.Trim(),
                    "yyyy-MM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var dt))
                return false;

            start = new DateTime(dt.Year, dt.Month, 1);
            end = start.AddMonths(1);
            return true;
        }

        private async Task LoadYeniViewDataAsync()
        {
            ViewBag.Musteriler = await _context.Musteriler
                .AsNoTracking()
                .OrderBy(x => x.Firma)
                .Select(x => new SelectListItem
                {
                    Value = x.MusteriID.ToString(),
                    Text = x.Firma ?? $"Müşteri #{x.MusteriID}"
                })
                .ToListAsync();

            if (User.IsInRole(AppRoles.Admin))
            {
                ViewBag.Kullanicilar = await _context.Users
                    .AsNoTracking()
                    .OrderBy(x => x.FullName ?? x.UserName)
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = !string.IsNullOrWhiteSpace(x.FullName) ? x.FullName! : x.UserName
                    })
                    .ToListAsync();
            }
        }
    }
}
