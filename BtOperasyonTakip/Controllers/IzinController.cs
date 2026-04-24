using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using BtOperasyonTakip.Security;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace BtOperasyonTakip.Controllers;

[Authorize(Roles = AppRoles.Operasyon + "," + AppRoles.Admin)]
public class IzinController : Controller
{
    private readonly AppDbContext _context;

    public IzinController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? editId = null)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return RedirectToAction("Login", "Auth");

        var model = await BuildViewModelAsync(currentUserId, IsAdmin(), editId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return RedirectToAction("Login", "Auth");

        var isAdmin = IsAdmin();

        var taleplerQuery = _context.IzinTalepleri.AsNoTracking();
        if (!isAdmin)
            taleplerQuery = taleplerQuery.Where(x => x.UserId == currentUserId);

        var talepler = await taleplerQuery
            .OrderByDescending(x => x.TalepTarihi)
            .ToListAsync();

        using var stream = new MemoryStream();
        using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            worksheetPart.Worksheet = new Worksheet(sheetData);

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Izin Talepleri"
            });

            sheetData.Append(CreateRow(
                "Talep Sahibi",
                "Başlangıç Tarihi",
                "Bitiş Tarihi",
                "Gün Sayısı",
                "Açıklama",
                "Durum",
                "Admin Notu",
                "Karar Veren",
                "Talep Tarihi",
                "Karar Tarihi"));

            foreach (var talep in talepler)
            {
                sheetData.Append(CreateRow(
                    talep.TalepSahibiAdi,
                    talep.BaslangicTarihi.ToString("dd.MM.yyyy"),
                    talep.BitisTarihi.ToString("dd.MM.yyyy"),
                    talep.GunSayisi.ToString(),
                    talep.Aciklama,
                    talep.Durum,
                    talep.AdminAciklama,
                    talep.KararVerenAdi,
                    talep.TalepTarihi.ToString("dd.MM.yyyy HH:mm"),
                    talep.KararTarihi?.ToString("dd.MM.yyyy HH:mm") ?? "-"));
            }

            workbookPart.Workbook.Save();
        }

        var fileName = $"IzinTalepleri_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRequest(IzinTalepFormViewModel form)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId <= 0)
            return RedirectToAction("Login", "Auth");

        if (form.BaslangicTarihi is null || form.BitisTarihi is null)
        {
            TempData["Error"] = "Başlangıç ve bitiş tarihi zorunludur.";
            return RedirectToAction(nameof(Index), new { editId = form.Id });
        }

        var baslangic = form.BaslangicTarihi.Value.Date;
        var bitis = form.BitisTarihi.Value.Date;

        if (bitis < baslangic)
        {
            TempData["Error"] = "Bitiş tarihi başlangıç tarihinden küçük olamaz.";
            return RedirectToAction(nameof(Index), new { editId = form.Id });
        }

        var gunSayisi = CalculateGunSayisi(baslangic, bitis);
        var isAdmin = IsAdmin();
        var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId);
        if (currentUser == null)
        {
            TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        IzinTalebi entity;

        if (form.Id.HasValue && form.Id.Value > 0)
        {
            entity = await _context.IzinTalepleri.FirstOrDefaultAsync(x => x.Id == form.Id.Value);
            if (entity == null)
            {
                TempData["Error"] = "İzin talebi bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            if (!isAdmin && entity.UserId != currentUserId)
                return Forbid();
        }
        else
        {
            entity = new IzinTalebi
            {
                UserId = currentUserId,
                TalepSahibiAdi = GetUserDisplayName(currentUser),
                TalepTarihi = DateTime.Now
            };

            _context.IzinTalepleri.Add(entity);
        }

        entity.BaslangicTarihi = baslangic;
        entity.BitisTarihi = bitis;
        entity.GunSayisi = gunSayisi;
        entity.Aciklama = (form.Aciklama ?? string.Empty).Trim();
        entity.Durum = IzinTalepDurumlari.Beklemede;
        entity.AdminAciklama = null;
        entity.KararTarihi = null;
        entity.KararVerenUserId = null;
        entity.KararVerenAdi = null;
        entity.GuncellemeTarihi = DateTime.Now;

        if (string.IsNullOrWhiteSpace(entity.TalepSahibiAdi))
            entity.TalepSahibiAdi = GetUserDisplayName(currentUser);

        await _context.SaveChangesAsync();

        TempData["Success"] = form.Id.HasValue && form.Id.Value > 0
            ? "İzin talebi güncellendi ve yeniden onaya gönderildi."
            : "İzin talebi oluşturuldu.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decide(int id, string karar, string? adminAciklama)
    {
        var talep = await _context.IzinTalepleri.FirstOrDefaultAsync(x => x.Id == id);
        if (talep == null)
        {
            TempData["Error"] = "İzin talebi bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        karar = (karar ?? string.Empty).Trim();
        adminAciklama = (adminAciklama ?? string.Empty).Trim();
        var kararVerenAdi = User.Identity?.Name ?? "Admin";

        if (karar == "approve")
        {
            var izinHakki = await _context.IzinHaklari.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == talep.UserId);
            if (izinHakki == null)
            {
                TempData["Error"] = "Bu kullanıcı için önce izin hakkı tanımlanmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            var kullanilanGun = await GetApprovedDaysAsync(talep.UserId, talep.Id);
            var kalanGun = izinHakki.ToplamGun - kullanilanGun;
            if (talep.GunSayisi > kalanGun)
            {
                TempData["Error"] = $"Yetersiz izin hakkı. Kalan gün: {kalanGun}.";
                return RedirectToAction(nameof(Index));
            }

            talep.Durum = IzinTalepDurumlari.Onaylandi;
            talep.AdminAciklama = string.IsNullOrWhiteSpace(adminAciklama) ? "Onaylandı." : adminAciklama;
        }
        else if (karar == "reject")
        {
            if (string.IsNullOrWhiteSpace(adminAciklama))
            {
                TempData["Error"] = "Red işlemi için mazeret girilmelidir.";
                return RedirectToAction(nameof(Index));
            }

            talep.Durum = IzinTalepDurumlari.Reddedildi;
            talep.AdminAciklama = adminAciklama;
        }
        else
        {
            TempData["Error"] = "Geçersiz karar tipi.";
            return RedirectToAction(nameof(Index));
        }

        talep.KararTarihi = DateTime.Now;
        talep.KararVerenUserId = GetCurrentUserId();
        talep.KararVerenAdi = kararVerenAdi;
        talep.GuncellemeTarihi = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = talep.Durum == IzinTalepDurumlari.Onaylandi
            ? "İzin talebi onaylandı."
            : "İzin talebi reddedildi.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetAllowance(int userId, int toplamGun)
    {
        if (toplamGun < 0)
        {
            TempData["Error"] = "İzin hakkı negatif olamaz.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId && x.Role == AppRoles.Operasyon);
        if (user == null)
        {
            TempData["Error"] = "Operasyon kullanıcısı bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var izinHakki = await _context.IzinHaklari.FirstOrDefaultAsync(x => x.UserId == userId);
        if (izinHakki == null)
        {
            izinHakki = new IzinHakki
            {
                UserId = userId
            };
            _context.IzinHaklari.Add(izinHakki);
        }

        izinHakki.ToplamGun = toplamGun;
        izinHakki.GuncellemeTarihi = DateTime.Now;
        izinHakki.GuncelleyenAdi = User.Identity?.Name ?? "Admin";

        await _context.SaveChangesAsync();

        TempData["Success"] = $"{GetUserDisplayName(user)} için izin hakkı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IzinTakipViewModel> BuildViewModelAsync(int currentUserId, bool isAdmin, int? editId)
    {
        var currentUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == currentUserId);
        var currentUserName = currentUser is null ? User.Identity?.Name ?? string.Empty : GetUserDisplayName(currentUser);

        var taleplerQuery = _context.IzinTalepleri.AsNoTracking();
        if (!isAdmin)
            taleplerQuery = taleplerQuery.Where(x => x.UserId == currentUserId);

        var talepler = await taleplerQuery
            .OrderBy(x => x.Durum == IzinTalepDurumlari.Beklemede ? 0 : 1)
            .ThenByDescending(x => x.TalepTarihi)
            .ToListAsync();

        var izinHakki = await _context.IzinHaklari.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == currentUserId);
        var kullanilanGun = await GetApprovedDaysAsync(currentUserId);

        var model = new IzinTakipViewModel
        {
            IsAdmin = isAdmin,
            CurrentUserId = currentUserId,
            CurrentUserName = currentUserName,
            ToplamHak = izinHakki?.ToplamGun ?? 0,
            KullanilanGun = kullanilanGun,
            KalanGun = Math.Max(0, (izinHakki?.ToplamGun ?? 0) - kullanilanGun),
            BekleyenTalepSayisi = talepler.Count(x => x.Durum == IzinTalepDurumlari.Beklemede),
            OnaylananTalepSayisi = talepler.Count(x => x.Durum == IzinTalepDurumlari.Onaylandi),
            ReddedilenTalepSayisi = talepler.Count(x => x.Durum == IzinTalepDurumlari.Reddedildi),
            Talepler = talepler.Select(x => new IzinTalebiSatirViewModel
            {
                Id = x.Id,
                UserId = x.UserId,
                TalepSahibiAdi = x.TalepSahibiAdi,
                BaslangicTarihi = x.BaslangicTarihi,
                BitisTarihi = x.BitisTarihi,
                GunSayisi = x.GunSayisi,
                Aciklama = x.Aciklama,
                Durum = x.Durum,
                AdminAciklama = x.AdminAciklama,
                KararVerenAdi = x.KararVerenAdi,
                TalepTarihi = x.TalepTarihi,
                KararTarihi = x.KararTarihi,
                CanEdit = isAdmin || x.UserId == currentUserId,
                CanDecide = isAdmin
            }).ToList()
        };

        if (editId.HasValue && editId.Value > 0)
        {
            var editTalep = talepler.FirstOrDefault(x => x.Id == editId.Value);
            if (editTalep != null && (isAdmin || editTalep.UserId == currentUserId))
            {
                model.Form = new IzinTalepFormViewModel
                {
                    Id = editTalep.Id,
                    BaslangicTarihi = editTalep.BaslangicTarihi,
                    BitisTarihi = editTalep.BitisTarihi,
                    Aciklama = editTalep.Aciklama
                };
            }
        }

        if (isAdmin)
        {
            var operationUsers = await _context.Users
                .AsNoTracking()
                .Where(x => x.Role == AppRoles.Operasyon)
                .OrderBy(x => x.FullName ?? x.UserName)
                .ToListAsync();

            var haklar = await _context.IzinHaklari.AsNoTracking().ToDictionaryAsync(x => x.UserId, x => x.ToplamGun);
            var kullanilanMap = await _context.IzinTalepleri.AsNoTracking()
                .Where(x => x.Durum == IzinTalepDurumlari.Onaylandi)
                .GroupBy(x => x.UserId)
                .Select(x => new { UserId = x.Key, GunSayisi = x.Sum(y => y.GunSayisi) })
                .ToDictionaryAsync(x => x.UserId, x => x.GunSayisi);

            model.KullaniciHaklari = operationUsers.Select(x =>
            {
                var toplamHak = haklar.TryGetValue(x.Id, out var tanimliHak) ? tanimliHak : 0;
                var kullanilan = kullanilanMap.TryGetValue(x.Id, out var toplamKullanilan) ? toplamKullanilan : 0;

                return new IzinHakkiSatirViewModel
                {
                    UserId = x.Id,
                    KullaniciAdi = GetUserDisplayName(x),
                    ToplamGun = toplamHak,
                    KullanilanGun = kullanilan,
                    KalanGun = Math.Max(0, toplamHak - kullanilan)
                };
            }).ToList();
        }

        return model;
    }

    private async Task<int> GetApprovedDaysAsync(int userId, int? excludeTalepId = null)
    {
        var query = _context.IzinTalepleri.AsNoTracking()
            .Where(x => x.UserId == userId && x.Durum == IzinTalepDurumlari.Onaylandi);

        if (excludeTalepId.HasValue)
            query = query.Where(x => x.Id != excludeTalepId.Value);

        return await query.SumAsync(x => (int?)x.GunSayisi) ?? 0;
    }

    private static int CalculateGunSayisi(DateTime baslangic, DateTime bitis)
    {
        return (bitis.Date - baslangic.Date).Days + 1;
    }

    private static Row CreateRow(params string?[] values)
    {
        var row = new Row();

        foreach (var value in values)
        {
            row.Append(new Cell
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(value ?? string.Empty))
            });
        }

        return row;
    }

    private int GetCurrentUserId()
    {
        return int.TryParse(User.FindFirst("UserId")?.Value, out var userId) ? userId : 0;
    }

    private bool IsAdmin()
    {
        return User.IsInRole(AppRoles.Admin);
    }

    private static string GetUserDisplayName(User user)
    {
        return string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName.Trim();
    }
}
