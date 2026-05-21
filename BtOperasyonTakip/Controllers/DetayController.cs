using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace BtOperasyonTakip.Controllers
{
    [Authorize(Roles = "Operasyon,Admin")]
    public class DetayController : Controller
    {
        private readonly AppDbContext _context;
        public DetayController(AppDbContext context)
        {
            _context = context;
        }

        private List<string> GetDurumSecenekleri()
        {
            return _context.Parametreler
                .AsNoTracking()
                .Where(x => x.Tur == "Durum" && x.ParAdi != null && x.ParAdi != "")
                .Select(x => x.ParAdi!)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private List<string> GetKullaniciSecenekleri()
        {
            return _context.Users
                .AsNoTracking()
                .Select(x => string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : x.FullName!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        // Helper to find archived JiraTasks related to a Musteri (or Detay) so we can link from Detay page to Arşiv
        private List<Models.JiraTask> GetRelatedArchivedTasksForMusteri(int musteriId)
        {
            var now = DateTime.Now;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);

            var tasks = _context.JiraTasks
                .AsNoTracking()
                .Include(t => t.Yorumlar)
                .Where(t => (t.MusteriID == musteriId && ((t.Durum ?? "").Trim() == "Arşiv" || (t.Durum ?? "").Trim() == "Tamamlandı"))
                            || (t.MusteriID == musteriId && t.OlusturmaTarihi < currentMonthStart))
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToList();

            // also include legacy Detay-derived tasks (negative Ids) related to this musteri
            var detayDerived = _context.Detaylar
                .AsNoTracking()
                .Where(d => d.MusteriID == musteriId)
                .OrderByDescending(d => d.Tarih)
                .ToList()
                .Where(d => !string.IsNullOrWhiteSpace(d.Kekleyen) && !string.Equals(d.Kekleyen?.Trim(), "Sistem", StringComparison.OrdinalIgnoreCase))
                .Where(d => !string.Equals((d.Gorusulen ?? string.Empty).Trim(), "İş Takip Yorumu", StringComparison.OrdinalIgnoreCase))
                .Select(d => new Models.JiraTask
                {
                    Id = -d.DetayID,
                    MusteriID = d.MusteriID,
                    MusteriAdi = string.Empty,
                    JiraId = string.Empty,
                    TalepKonusu = string.IsNullOrWhiteSpace(d.Gorusulen) ? "Detay" : d.Gorusulen,
                    TalepTuru = "Detay",
                    TalepAcan = d.Kekleyen,
                    Durum = "Aktif",
                    TakipEden = d.Kekleyen,
                    OlusturmaTarihi = d.Tarih,
                    Yorumlar = new List<Models.JiraYorum>()
                })
                .ToList();

            tasks = tasks.Concat(detayDerived)
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToList();

            return tasks;
        }

        private IActionResult RedirectToLocalOr(string? returnUrl, string action, object? routeValues = null)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(action, routeValues);
        }

        public IActionResult Index(
            int? musteriId = null,
            string? q = null,
            string? durum = null,
            string? teknoloji = null,
            string? talepSahibi = null,
            string? minDate = null,
            string? maxDate = null,
            int page = 1,
            int pageSize = 10)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? 10 : pageSize;
            pageSize = pageSize > 50 ? 50 : pageSize;

            IQueryable<Musteri> query = _context.Musteriler.AsNoTracking();

            q = (q ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(x =>
                    (x.Firma ?? string.Empty).Contains(q) ||
                    (x.FirmaYetkilisi ?? string.Empty).Contains(q) ||
                    (x.Telefon ?? string.Empty).Contains(q) ||
                    (x.Teknoloji ?? string.Empty).Contains(q) ||
                    (x.TalepSahibi ?? string.Empty).Contains(q) ||
                    (x.Aciklama ?? string.Empty).Contains(q));
            }

            durum = (durum ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(durum))
            {
                query = query.Where(x => x.Durum == durum);
            }

            teknoloji = (teknoloji ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(teknoloji))
            {
                query = query.Where(x => x.Teknoloji == teknoloji);
            }

            talepSahibi = (talepSahibi ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(talepSahibi))
            {
                query = query.Where(x => x.TalepSahibi == talepSahibi);
            }

            if (TryParseTrDate(minDate, out var min))
            {
                query = query.Where(x => x.KayitTarihi != null && x.KayitTarihi.Value.Date >= min.Date);
            }

            if (TryParseTrDate(maxDate, out var max))
            {
                query = query.Where(x => x.KayitTarihi != null && x.KayitTarihi.Value.Date <= max.Date);
            }

            var durumSecenekleri = GetDurumSecenekleri();

            var aktifCount = query.Count(x => (x.Durum ?? string.Empty).Trim() == "Aktif");
            var beklemedeCount = query.Count(x => (x.Durum ?? string.Empty).Trim() == "Beklemede");
            ViewBag.DokumanIletildiCount = query.Count(x => (x.Durum ?? string.Empty).Trim() == "Döküman İletildi");
            ViewBag.Durumlar = durumSecenekleri;

            var orderedQuery = query
                .OrderByDescending(x => x.KayitTarihi)
                .ThenByDescending(x => x.MusteriID);

            var totalCount = orderedQuery.Count();

            if (musteriId.HasValue && musteriId.Value > 0)
            {
                var orderedIds = orderedQuery
                    .Select(x => x.MusteriID)
                    .ToList();

                var selectedIndex = orderedIds.FindIndex(x => x == musteriId.Value);
                if (selectedIndex >= 0)
                {
                    page = (selectedIndex / pageSize) + 1;
                }
            }

            var totalPages = totalCount <= 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);
            page = page > totalPages ? totalPages : page;

            var musteriler = orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var musteriIds = musteriler.Select(x => x.MusteriID).ToList();

            var sonDetaylar = _context.Detaylar
                .AsNoTracking()
                .Where(x => musteriIds.Contains(x.MusteriID))
                .OrderByDescending(x => x.Tarih)
                .ThenByDescending(x => x.DetayID)
                .ToList()
                .GroupBy(x => x.MusteriID)
                .ToDictionary(g => g.Key, g => g.First());

            List<Detay> selectedDetaylar = new();
            string? selectedMusteriAdi = null;

            if (musteriId.HasValue && musteriId.Value > 0)
            {
                selectedDetaylar = _context.Detaylar
                    .AsNoTracking()
                    .Where(x => x.MusteriID == musteriId.Value)
                    .OrderByDescending(x => x.Tarih)
                    .ThenByDescending(x => x.DetayID)
                    .ToList();

                selectedMusteriAdi = _context.Musteriler
                    .AsNoTracking()
                    .Where(x => x.MusteriID == musteriId.Value)
                    .Select(x => x.Firma)
                    .FirstOrDefault();
            }

            var model = new DetayBoardViewModel
            {
                Musteriler = musteriler,
                SonDetaylar = sonDetaylar,
                KullaniciSecenekleri = GetKullaniciSecenekleri(),
                TeknolojiSecenekleri = _context.Musteriler
                    .AsNoTracking()
                    .Where(x => !string.IsNullOrWhiteSpace(x.Teknoloji))
                    .Select(x => x.Teknoloji!)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList(),
                TalepSahibiSecenekleri = _context.Musteriler
                    .AsNoTracking()
                    .Where(x => !string.IsNullOrWhiteSpace(x.TalepSahibi))
                    .Select(x => x.TalepSahibi!)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList(),
                Q = q,
                Durum = durum,
                Teknoloji = teknoloji,
                TalepSahibi = talepSahibi,
                MinDate = minDate,
                MaxDate = maxDate,
                SelectedMusteriId = musteriId,
                SelectedMusteriAdi = selectedMusteriAdi,
                SelectedMusteriDetaylar = selectedDetaylar,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                AktifCount = aktifCount,
                PasifCount = 0,
                BeklemedeCount = beklemedeCount
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult ExportExcelByMonth(string? month)
        {
            bool tumAylar = string.IsNullOrWhiteSpace(month) || string.Equals(month.Trim(), "all", StringComparison.OrdinalIgnoreCase);
            DateTime monthStart = default;

            if (!tumAylar && !DateTime.TryParseExact(month!.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out monthStart))
                return BadRequest("Geçersiz ay formatı. Örn: 2026-02");

            IQueryable<Detay> sorgu = _context.Detaylar
                .AsNoTracking()
                .Include(x => x.Musteri);

            if (!tumAylar)
            {
                var start = new DateTime(monthStart.Year, monthStart.Month, 1, 0, 0, 0);
                var end = start.AddMonths(1);
                sorgu = sorgu.Where(x => x.Tarih >= start && x.Tarih < end);
            }

            var detaylar = sorgu
                .OrderBy(x => x.Musteri!.Firma)
                .ThenByDescending(x => x.Tarih)
                .ThenByDescending(x => x.DetayID)
                .ToList();

            using var ms = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateDetayStylesheet();
                stylesPart.Stylesheet.Save();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                var worksheet = new Worksheet();

                worksheet.Append(new Columns(
                    CreateColumn(1, 1, 12),
                    CreateColumn(2, 2, 28),
                    CreateColumn(3, 3, 20),
                    CreateColumn(4, 4, 34),
                    CreateColumn(5, 5, 60),
                    CreateColumn(6, 6, 22),
                    CreateColumn(7, 7, 24),
                    CreateColumn(8, 8, 18)
                ));

                worksheet.Append(sheetData);

                uint currentRow = 1;
                var header = new Row { RowIndex = currentRow };
                header.Append(
                    CreateTextCell("A", currentRow, "Müşteri ID", 1),
                    CreateTextCell("B", currentRow, "Müşteri", 1),
                    CreateTextCell("C", currentRow, "Tarih Saat", 1),
                    CreateTextCell("D", currentRow, "İş / Görüşülen", 1),
                    CreateTextCell("E", currentRow, "Açıklama", 1),
                    CreateTextCell("F", currentRow, "Ekleyen", 1),
                    CreateTextCell("G", currentRow, "Saha Sorumlusu", 1),
                    CreateTextCell("H", currentRow, "Durum", 1)
                );
                sheetData.Append(header);

                foreach (var detay in detaylar)
                {
                    currentRow++;

                    var row = new Row { RowIndex = currentRow };
                    row.Append(
                        CreateNumberCell("A", currentRow, detay.MusteriID.ToString(CultureInfo.InvariantCulture), 0),
                        CreateTextCell("B", currentRow, detay.Musteri?.Firma, 0),
                        CreateTextCell("C", currentRow, detay.Tarih.ToString("dd.MM.yyyy HH:mm"), 0),
                        CreateTextCell("D", currentRow, detay.Gorusulen, 0),
                        CreateTextCell("E", currentRow, detay.Aciklama, 0),
                        CreateTextCell("F", currentRow, detay.Kekleyen, 0),
                        CreateTextCell("G", currentRow, detay.Musteri?.TalepSahibi, 0),
                        CreateTextCell("H", currentRow, detay.Musteri?.Durum, 0)
                    );

                    sheetData.Append(row);
                }

                var lastRowIndex = currentRow == 1 ? 1U : currentRow;
                var filterRange = $"A1:H{lastRowIndex}";
                worksheet.Append(new AutoFilter { Reference = filterRange });

                if (detaylar.Any())
                {
                    var tableDefinitionPart = worksheetPart.AddNewPart<TableDefinitionPart>();
                    var tableId = 1U;

                    tableDefinitionPart.Table = new Table
                    {
                        Id = tableId,
                        Name = "DetaylarTablo",
                        DisplayName = "DetaylarTablo",
                        Reference = filterRange,
                        TotalsRowShown = false
                    };

                    tableDefinitionPart.Table.AppendChild(new AutoFilter { Reference = filterRange });
                    tableDefinitionPart.Table.AppendChild(new TableColumns(
                        new TableColumn { Id = 1U, Name = "Müşteri ID" },
                        new TableColumn { Id = 2U, Name = "Müşteri" },
                        new TableColumn { Id = 3U, Name = "Tarih Saat" },
                        new TableColumn { Id = 4U, Name = "İş / Görüşülen" },
                        new TableColumn { Id = 5U, Name = "Açıklama" },
                        new TableColumn { Id = 6U, Name = "Ekleyen" },
                        new TableColumn { Id = 7U, Name = "Saha Sorumlusu" },
                        new TableColumn { Id = 8U, Name = "Durum" }
                    ) { Count = 8U });

                    tableDefinitionPart.Table.AppendChild(new TableStyleInfo
                    {
                        Name = "TableStyleMedium2",
                        ShowFirstColumn = false,
                        ShowLastColumn = false,
                        ShowRowStripes = true,
                        ShowColumnStripes = false
                    });

                    tableDefinitionPart.Table.Save();

                    var tableParts = worksheet.GetFirstChild<TableParts>();
                    if (tableParts == null)
                    {
                        tableParts = new TableParts { Count = 0U };
                        worksheet.Append(tableParts);
                    }

                    tableParts.Append(new TablePart { Id = worksheetPart.GetIdOfPart(tableDefinitionPart) });
                    tableParts.Count = 1U;
                }

                worksheetPart.Worksheet = worksheet;
                worksheetPart.Worksheet.Save();

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Detaylar"
                });

                workbookPart.Workbook.Save();
            }

            ms.Position = 0;
            var dosyaAdi = tumAylar ? "Detaylar_TumAylar.xlsx" : $"Detaylar_{monthStart:yyyy-MM}.xlsx";
            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                dosyaAdi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateMusteri(Musteri musteri, string? returnUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(musteri.Firma))
                {
                    TempData["MusteriError"] = "Firma adı zorunludur.";
                    return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
                }

                var now = DateTime.Now;

                musteri.Firma = musteri.Firma.Trim();
                musteri.FirmaYetkilisi = musteri.FirmaYetkilisi?.Trim();
                musteri.Telefon = musteri.Telefon?.Trim();
                musteri.SiteUrl = musteri.SiteUrl?.Trim();
                musteri.Teknoloji = musteri.Teknoloji?.Trim();
                musteri.Durum = musteri.Durum?.Trim();
                musteri.TalepSahibi = musteri.TalepSahibi?.Trim();
                musteri.Aciklama = musteri.Aciklama?.Trim();

                var durumSecenekleri = GetDurumSecenekleri();

                if (string.IsNullOrWhiteSpace(musteri.Durum) || !durumSecenekleri.Contains(musteri.Durum, StringComparer.OrdinalIgnoreCase))
                {
                    TempData["MusteriError"] = "Geçerli bir durum seçiniz.";
                    return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
                }

                musteri.KayitTarihi = now;
                musteri.DurumDegisiklikTarihi = now;
                musteri.DokumanGonderildiMi = null;
                musteri.DokumanGonderimSayisi = 0;
                musteri.DokumanKontrolBaslangicTarihi = null;

                _context.Musteriler.Add(musteri);
                _context.SaveChanges();

                TempData["MusteriOk"] = "Müşteri başarıyla eklendi!";
                return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
            }
            catch (Exception ex)
            {
                TempData["MusteriError"] = $"Kayıt sırasında hata: {ex.Message}";
                return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
            }
        }

        public IActionResult GetMusteri(int id)
        {
            var musteri = _context.Musteriler.Find(id);
            if (musteri == null)
                return NotFound();

            return Json(new
            {
                musteriID = musteri.MusteriID,
                firma = musteri.Firma,
                firmaYetkilisi = musteri.FirmaYetkilisi,
                telefon = musteri.Telefon,
                siteUrl = musteri.SiteUrl,
                teknoloji = musteri.Teknoloji,
                durum = musteri.Durum,
                talepSahibi = musteri.TalepSahibi,
                aciklama = musteri.Aciklama,
                kayitTarihi = musteri.KayitTarihi?.ToString("yyyy-MM-dd"),
                durumDegisiklikTarihi = musteri.DurumDegisiklikTarihi?.ToString("yyyy-MM-dd")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateMusteri(Musteri musteri, string? returnUrl)
        {
            try
            {
                var existingMusteri = _context.Musteriler.Find(musteri.MusteriID);
                if (existingMusteri == null)
                {
                    TempData["MusteriError"] = $"Müşteri ID {musteri.MusteriID} bulunamadı";
                    return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
                }

                var eskiDurum = (existingMusteri.Durum ?? string.Empty).Trim();
                var yeniDurum = (musteri.Durum ?? string.Empty).Trim();

                var durumSecenekleri = GetDurumSecenekleri();

                if (string.IsNullOrWhiteSpace(yeniDurum) || !durumSecenekleri.Contains(yeniDurum, StringComparer.OrdinalIgnoreCase))
                {
                    TempData["MusteriError"] = "Geçerli bir durum seçiniz.";
                    return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
                }

                var durumDegisti = !string.Equals(eskiDurum, yeniDurum, StringComparison.OrdinalIgnoreCase);

                if (durumDegisti && string.IsNullOrWhiteSpace(musteri.Aciklama))
                {
                    TempData["MusteriError"] = "Durum değiştirirken açıklama zorunludur.";
                    return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
                }

                existingMusteri.Firma = musteri.Firma;
                existingMusteri.FirmaYetkilisi = musteri.FirmaYetkilisi;
                existingMusteri.Telefon = musteri.Telefon;
                existingMusteri.SiteUrl = musteri.SiteUrl;
                existingMusteri.Teknoloji = musteri.Teknoloji;
                existingMusteri.Durum = yeniDurum;
                existingMusteri.TalepSahibi = musteri.TalepSahibi;
                existingMusteri.Aciklama = musteri.Aciklama;

                if (durumDegisti)
                {
                    existingMusteri.DurumDegisiklikTarihi = DateTime.Now;

                    existingMusteri.DokumanGonderildiMi = null;
                    existingMusteri.DokumanGonderimSayisi = 0;
                    existingMusteri.DokumanKontrolBaslangicTarihi = existingMusteri.DurumDegisiklikTarihi;

                    _context.MusteriDurumGecmisleri.Add(new MusteriDurumGecmisi
                    {
                        MusteriID = existingMusteri.MusteriID,
                        EskiDurum = eskiDurum,
                        YeniDurum = yeniDurum,
                        Aciklama = musteri.Aciklama!.Trim(),
                        Tarih = existingMusteri.DurumDegisiklikTarihi.Value,
                        DegistirenKullanici = User?.Identity?.Name
                    });
                }

                _context.SaveChanges();
                TempData["MusteriOk"] = "Müşteri başarıyla güncellendi!";
                return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
            }
            catch (Exception ex)
            {
                TempData["MusteriError"] = $"Güncelleme sırasında hata: {ex.Message}";
                return RedirectToLocalOr(returnUrl, "Index", new { controller = "Musteri" });
            }
        }

        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteMusteri(int id)
        {
            try
            {
                var musteri = _context.Musteriler
                    .Include(m => m.Detaylar)
                    .Include(m => m.DurumGecmisi)
                    .FirstOrDefault(m => m.MusteriID == id);

                if (musteri == null)
                    return Json(new { success = false, message = "Müşteri bulunamadı." });

                // Ticket.MusteriID → null
                var ilgiliTicketlar = _context.Tickets.Where(t => t.MusteriID == id).ToList();
                foreach (var t in ilgiliTicketlar)
                    t.MusteriID = null;

                // JiraTask.MusteriID → null
                var ilgiliJiralar = _context.JiraTasks.Where(j => j.MusteriID == id).ToList();
                foreach (var j in ilgiliJiralar)
                    j.MusteriID = null;

                if (musteri.DurumGecmisi != null && musteri.DurumGecmisi.Any())
                    _context.MusteriDurumGecmisleri.RemoveRange(musteri.DurumGecmisi);

                if (musteri.Detaylar != null && musteri.Detaylar.Any())
                    _context.Detaylar.RemoveRange(musteri.Detaylar);

                _context.Musteriler.Remove(musteri);
                _context.SaveChanges();

                return Json(new { success = true, message = "Müşteri başarıyla silindi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Silme sırasında hata: {ex.Message}" });
            }
        }

        public IActionResult GetDetaylar(int musteriId)
        {
            var detaylar = _context.Detaylar
                .Where(d => d.MusteriID == musteriId)
                .OrderByDescending(d => d.Tarih)
                .ToList();

            ViewBag.MusteriID = musteriId;
            ViewBag.MusteriAdi = _context.Musteriler
                .AsNoTracking()
                .Where(x => x.MusteriID == musteriId)
                .Select(x => x.Firma)
                .FirstOrDefault();
            ViewBag.KullaniciSecenekleri = GetKullaniciSecenekleri();
            return PartialView("_DetayListesi", detaylar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddDetay(Detay detay, string? returnUrl)
        {
            try
            {
                if (!_context.Musteriler.Any(m => m.MusteriID == detay.MusteriID))
                {
                    TempData["DetayError"] = $"Müşteri ID {detay.MusteriID} bulunamadı";
                    return RedirectToLocalOr(returnUrl, "Index", new { musteriId = detay.MusteriID });
                }

                if (detay.Tarih == default)
                {
                    detay.Tarih = DateTime.Now;
                }

                detay.Gorusulen = detay.Gorusulen?.Trim();
                detay.Aciklama = detay.Aciklama?.Trim();
                detay.Kekleyen = detay.Kekleyen?.Trim();

                _context.Detaylar.Add(detay);
                _context.SaveChanges();
                TempData["DetayOk"] = "Detay kaydı eklendi.";
            }
            catch (Exception ex)
            {
                TempData["DetayError"] = "Detay eklenemedi: " + ex.Message;
            }

            return RedirectToLocalOr(returnUrl, "Index", new { musteriId = detay.MusteriID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDetay(Detay detay, string? returnUrl)
        {
            var existing = _context.Detaylar.FirstOrDefault(d => d.DetayID == detay.DetayID);
            if (existing == null)
            {
                TempData["DetayError"] = "Detay kaydı bulunamadı.";
                return RedirectToLocalOr(returnUrl, "Index", new { musteriId = detay.MusteriID });
            }

            detay.Gorusulen = detay.Gorusulen?.Trim();
            detay.Aciklama = detay.Aciklama?.Trim();
            detay.Kekleyen = detay.Kekleyen?.Trim();

            existing.Tarih = detay.Tarih;
            existing.Gorusulen = detay.Gorusulen;
            existing.Aciklama = detay.Aciklama;
            existing.Kekleyen = detay.Kekleyen;
            _context.SaveChanges();

            TempData["DetayOk"] = "Detay kaydı güncellendi.";
            return RedirectToLocalOr(returnUrl, "Index", new { musteriId = detay.MusteriID });
        }

        [HttpGet]
        public IActionResult GetDetay(int id)
        {
            var detay = _context.Detaylar.FirstOrDefault(d => d.DetayID == id);
            if (detay == null)
                return NotFound();

            return Json(new
            {
                detayID = detay.DetayID,
                musteriID = detay.MusteriID,
                tarih = detay.Tarih.ToString("yyyy-MM-ddTHH:mm"),
                gorusulen = detay.Gorusulen,
                aciklama = detay.Aciklama,
                kekleyen = detay.Kekleyen
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteDetay(int id, string? returnUrl)
        {
            var detay = _context.Detaylar.Find(id);
            if (detay == null)
            {
                TempData["DetayError"] = "Detay kaydı bulunamadı.";
                return RedirectToLocalOr(returnUrl, "Index");
            }

            var musteriId = detay.MusteriID;
            _context.Detaylar.Remove(detay);
            _context.SaveChanges();
            TempData["DetayOk"] = "Detay kaydı silindi.";
            return RedirectToLocalOr(returnUrl, "Index", new { musteriId });
        }

        private static bool TryParseTrDate(string? s, out DateTime dt)
        {
            dt = default;
            if (string.IsNullOrWhiteSpace(s)) return false;

            return DateTime.TryParseExact(
                s.Trim(),
                new[] { "dd.MM.yyyy", "d.M.yyyy", "yyyy-MM-dd" },
                new CultureInfo("tr-TR"),
                DateTimeStyles.None,
                out dt);
        }

        private static Stylesheet CreateDetayStylesheet()
        {
            return new Stylesheet(
                new Fonts(
                    new Font(
                        new FontSize { Val = 11 }
                    ),
                    new Font(
                        new Bold(),
                        new FontSize { Val = 11 },
                        new Color { Rgb = HexBinaryValue.FromString("FFFFFFFF") }
                    )
                ),
                new Fills(
                    new Fill(new PatternFill { PatternType = PatternValues.None }),
                    new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
                    new Fill(new PatternFill(
                        new ForegroundColor { Rgb = HexBinaryValue.FromString("1D4ED8") },
                        new BackgroundColor { Indexed = 64U }
                    ) { PatternType = PatternValues.Solid })
                ),
                new Borders(
                    new Border(
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder()
                    ),
                    new Border(
                        new LeftBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                        new RightBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                        new TopBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                        new BottomBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                        new DiagonalBorder()
                    )
                ),
                new CellFormats(
                    new CellFormat(),
                    new CellFormat
                    {
                        FontId = 1U,
                        FillId = 2U,
                        BorderId = 1U,
                        ApplyFont = true,
                        ApplyFill = true,
                        ApplyBorder = true,
                        Alignment = new Alignment
                        {
                            Horizontal = HorizontalAlignmentValues.Center,
                            Vertical = VerticalAlignmentValues.Center,
                            WrapText = true
                        },
                        ApplyAlignment = true
                    }
                )
            );
        }

        private static Column CreateColumn(uint min, uint max, double width)
        {
            return new Column
            {
                Min = min,
                Max = max,
                Width = width,
                CustomWidth = true
            };
        }

        private static Cell CreateTextCell(string columnName, uint rowIndex, string? text, uint styleIndex)
        {
            return new Cell
            {
                CellReference = columnName + rowIndex,
                DataType = CellValues.InlineString,
                StyleIndex = styleIndex,
                InlineString = new InlineString(new Text(text ?? string.Empty))
            };
        }

        private static Cell CreateNumberCell(string columnName, uint rowIndex, string value, uint styleIndex)
        {
            return new Cell
            {
                CellReference = columnName + rowIndex,
                CellValue = new CellValue(value),
                DataType = CellValues.Number,
                StyleIndex = styleIndex
            };
        }
    }
}