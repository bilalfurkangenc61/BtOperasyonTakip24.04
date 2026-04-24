using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Text;
using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Controllers;

[Authorize(Roles = "Operasyon,Admin")]
public sealed class MusteriController : Controller
{
    private readonly AppDbContext _context;

    private List<string> GetParametreDegerleri(string tur) =>
        _context.Parametreler
            .AsNoTracking()
            .Where(p => p.Tur == tur && p.ParAdi != null && p.ParAdi != "")
            .OrderBy(p => p.ParAdi)
            .Select(p => p.ParAdi!)
            .ToList();

    public MusteriController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("/Musteri/SetDokumanDurumu")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDokumanDurumu([FromForm] int musteriId, [FromForm] bool gonderildi, [FromForm] string? returnUrl)
    {
        var musteri = await _context.Musteriler.FirstOrDefaultAsync(x => x.MusteriID == musteriId);
        if (musteri == null)
        {
            TempData["MusteriError"] = "Müşteri bulunamadı.";
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        if (!string.Equals((musteri.Durum ?? string.Empty).Trim(), "Döküman Gönderildi", StringComparison.OrdinalIgnoreCase))
        {
            TempData["MusteriError"] = "Bu müşteri için döküman durumu güncellenemez.";
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        musteri.DokumanGonderildiMi = gonderildi;

        if (gonderildi)
        {
            musteri.DokumanGonderimSayisi = (musteri.DokumanGonderimSayisi ?? 0) + 1;
            musteri.DurumDegisiklikTarihi = DateTime.Now;
            musteri.DokumanKontrolBaslangicTarihi = musteri.DurumDegisiklikTarihi;
        }
        else
        {
            musteri.DokumanKontrolBaslangicTarihi ??= musteri.DurumDegisiklikTarihi ?? musteri.KayitTarihi;
        }

        await _context.SaveChangesAsync();

        TempData["MusteriOk"] = "Döküman durumu güncellendi.";
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 25,
        string? search = null,
        string? durum = null,
        string? teknoloji = null,
        string? talepSahibi = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 25;
        if (pageSize > 100) pageSize = 100;

        IQueryable<Musteri> query = _context.Musteriler.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Firma != null && x.Firma.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(durum))
        {
            var d = durum.Trim();
            query = query.Where(x => x.Durum == d);
        }

        if (!string.IsNullOrWhiteSpace(teknoloji))
        {
            var t = teknoloji.Trim();
            query = query.Where(x => x.Teknoloji == t);
        }

        if (!string.IsNullOrWhiteSpace(talepSahibi))
        {
            var ts = talepSahibi.Trim();
            query = query.Where(x => x.TalepSahibi == ts);
        }

        var totalCount = await query.CountAsync();

        var model = await query
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.MusteriID)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalCount"] = totalCount;
        ViewData["Search"] = search;
        ViewData["SelectedDurum"] = durum;
        ViewData["SelectedTeknoloji"] = teknoloji;
        ViewData["SelectedTalepSahibi"] = talepSahibi;

        ViewData["Durumlar"] = GetParametreDegerleri("Durum");
        ViewData["Teknolojiler"] = GetParametreDegerleri("Teknoloji");
        ViewData["TalepSahipleri"] = GetParametreDegerleri("TalepEden");

        return View(model);
    }

    [HttpGet("/Musteri/Filters")]
    public async Task<IActionResult> Filters()
    {
        var teknolojiler = await _context.Parametreler
            .AsNoTracking()
            .Where(p => p.Tur == "Teknoloji" && p.ParAdi != null && p.ParAdi != "")
            .OrderBy(p => p.ParAdi)
            .Select(p => p.ParAdi!)
            .ToListAsync();

        var talepSahipleri = await _context.Parametreler
            .AsNoTracking()
            .Where(p => p.Tur == "TalepEden" && p.ParAdi != null && p.ParAdi != "")
            .OrderBy(p => p.ParAdi)
            .Select(p => p.ParAdi!)
            .ToListAsync();

        var durumlar = await _context.Parametreler
            .AsNoTracking()
            .Where(p => p.Tur == "Durum" && p.ParAdi != null && p.ParAdi != "")
            .OrderBy(p => p.ParAdi)
            .Select(p => p.ParAdi!)
            .ToListAsync();

        return Json(new
        {
            teknolojiler,
            talepSahipleri,
            durumlar
        });
    }

    [HttpGet("/Musteri/Data")]
    public async Task<IActionResult> Data(
        int draw,
        int start,
        int length,
        string? firma,
        string? durum,
        string? teknoloji,
        string? talepSahibi,
        string? minDate,
        string? maxDate)
    {
        IQueryable<Musteri> query = _context.Musteriler.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(firma))
        {
            var term = firma.Trim();
            query = query.Where(x => x.Firma != null && x.Firma.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(durum))
        {
            query = query.Where(x => x.Durum == durum);
        }

        if (!string.IsNullOrWhiteSpace(teknoloji))
        {
            query = query.Where(x => x.Teknoloji == teknoloji);
        }

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

        var recordsTotal = await _context.Musteriler.AsNoTracking().CountAsync();
        var recordsFiltered = await query.CountAsync();

        query = query
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.MusteriID);

        var page = await query
            .Skip(start)
            .Take(length <= 0 ? 25 : length)
            .Select(x => new
            {
                musteriID = x.MusteriID,
                firma = x.Firma,
                firmaYetkilisi = x.FirmaYetkilisi,
                telefon = x.Telefon,
                siteUrl = x.SiteUrl,
                teknoloji = x.Teknoloji,
                durum = x.Durum,
                talepSahibi = x.TalepSahibi,
                kaynak = x.Kaynak,
                kayitTarihiText = x.KayitTarihi.HasValue ? x.KayitTarihi.Value.ToString("dd.MM.yyyy") : "-",
                aciklama = x.Aciklama
            })
            .ToListAsync();

        return Json(new
        {
            draw,
            recordsTotal,
            recordsFiltered,
            data = page
        });
    }

    [HttpGet("/Musteri/ExportExcelByMonth")]
    public async Task<IActionResult> ExportExcelByMonth(string? month)
    {
        if (string.IsNullOrWhiteSpace(month))
            return BadRequest("Ay bilgisi zorunlu. Örn: 2026-02");

        if (!DateTime.TryParseExact(month.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var monthStart))
            return BadRequest("Geçersiz ay formatı. Örn: 2026-02");

        var start = new DateTime(monthStart.Year, monthStart.Month, 1, 0, 0, 0);
        var end = start.AddMonths(1);

        var rows = await _context.Musteriler
            .AsNoTracking()
            .Where(x =>
                (x.KayitTarihi != null && x.KayitTarihi.Value >= start && x.KayitTarihi.Value < end)
                || (x.DurumDegisiklikTarihi != null && x.DurumDegisiklikTarihi.Value >= start && x.DurumDegisiklikTarihi.Value < end))
            .OrderByDescending(x => x.KayitTarihi)
            .ThenByDescending(x => x.MusteriID)
            .Select(x => new
            {
                x.MusteriID,
                x.Firma,
                x.FirmaYetkilisi,
                x.Telefon,
                x.SiteUrl,
                x.Teknoloji,
                x.Durum,
                x.TalepSahibi,
                x.Kaynak,
                x.KayitTarihi,
                x.DurumDegisiklikTarihi,
                x.Aciklama
            })
            .ToListAsync();

        using var ms = new MemoryStream();

        using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
        {
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            stylesPart.Stylesheet = CreateMusteriStylesheet();
            stylesPart.Stylesheet.Save();

            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            var worksheet = new Worksheet();
            var sheetData = new SheetData();

            worksheet.Append(new Columns(
                CreateColumn(1, 1, 10),
                CreateColumn(2, 2, 26),
                CreateColumn(3, 3, 22),
                CreateColumn(4, 4, 18),
                CreateColumn(5, 5, 28),
                CreateColumn(6, 6, 18),
                CreateColumn(7, 7, 18),
                CreateColumn(8, 8, 22),
                CreateColumn(9, 9, 20),
                CreateColumn(10, 10, 18),
                CreateColumn(11, 11, 18),
                CreateColumn(12, 12, 40)
            ));

            worksheet.Append(sheetData);

            uint currentRow = 1;
            var header = new Row { RowIndex = currentRow };
            header.Append(
                CreateTextCell("A", currentRow, "ID", 1),
                CreateTextCell("B", currentRow, "Firma", 1),
                CreateTextCell("C", currentRow, "Yetkili", 1),
                CreateTextCell("D", currentRow, "Telefon", 1),
                CreateTextCell("E", currentRow, "SiteUrl", 1),
                CreateTextCell("F", currentRow, "Teknoloji", 1),
                CreateTextCell("G", currentRow, "Durum", 1),
                CreateTextCell("H", currentRow, "DurumDegisiklikTarihi", 1),
                CreateTextCell("I", currentRow, "TalepSahibi", 1),
                CreateTextCell("J", currentRow, "Kaynak", 1),
                CreateTextCell("K", currentRow, "KayitTarihi", 1),
                CreateTextCell("L", currentRow, "Aciklama", 1)
            );
            sheetData.Append(header);

            foreach (var r in rows)
            {
                currentRow++;

                var row = new Row { RowIndex = currentRow };
                row.Append(
                    CreateNumberCell("A", currentRow, r.MusteriID.ToString(CultureInfo.InvariantCulture), 0),
                    CreateTextCell("B", currentRow, r.Firma, 0),
                    CreateTextCell("C", currentRow, r.FirmaYetkilisi, 0),
                    CreateTextCell("D", currentRow, r.Telefon, 0),
                    CreateTextCell("E", currentRow, r.SiteUrl, 0),
                    CreateTextCell("F", currentRow, r.Teknoloji, 0),
                    CreateTextCell("G", currentRow, r.Durum, 0),
                    CreateTextCell("H", currentRow, r.DurumDegisiklikTarihi?.ToString("dd.MM.yyyy") ?? "-", 0),
                    CreateTextCell("I", currentRow, r.TalepSahibi, 0),
                    CreateTextCell("J", currentRow, r.Kaynak, 0),
                    CreateTextCell("K", currentRow, r.KayitTarihi?.ToString("dd.MM.yyyy") ?? "-", 0),
                    CreateTextCell("L", currentRow, r.Aciklama, 0)
                );

                sheetData.Append(row);
            }

            var lastRowIndex = currentRow == 1 ? 1U : currentRow;
            var filterRange = $"A1:L{lastRowIndex}";
            worksheet.Append(new AutoFilter { Reference = filterRange });

            if (rows.Any())
            {
                var tableDefinitionPart = worksheetPart.AddNewPart<TableDefinitionPart>();
                tableDefinitionPart.Table = new Table
                {
                    Id = 1U,
                    Name = "MusterilerTablo",
                    DisplayName = "MusterilerTablo",
                    Reference = filterRange,
                    TotalsRowShown = false
                };

                tableDefinitionPart.Table.AppendChild(new AutoFilter { Reference = filterRange });
                tableDefinitionPart.Table.AppendChild(new TableColumns(
                    new TableColumn { Id = 1U, Name = "ID" },
                    new TableColumn { Id = 2U, Name = "Firma" },
                    new TableColumn { Id = 3U, Name = "Yetkili" },
                    new TableColumn { Id = 4U, Name = "Telefon" },
                    new TableColumn { Id = 5U, Name = "SiteUrl" },
                    new TableColumn { Id = 6U, Name = "Teknoloji" },
                    new TableColumn { Id = 7U, Name = "Durum" },
                    new TableColumn { Id = 8U, Name = "DurumDegisiklikTarihi" },
                    new TableColumn { Id = 9U, Name = "TalepSahibi" },
                    new TableColumn { Id = 10U, Name = "Kaynak" },
                    new TableColumn { Id = 11U, Name = "KayitTarihi" },
                    new TableColumn { Id = 12U, Name = "Aciklama" }
                ) { Count = 12U });

                tableDefinitionPart.Table.AppendChild(new TableStyleInfo
                {
                    Name = "TableStyleMedium2",
                    ShowFirstColumn = false,
                    ShowLastColumn = false,
                    ShowRowStripes = true,
                    ShowColumnStripes = false
                });

                tableDefinitionPart.Table.Save();

                var tableParts = new TableParts { Count = 1U };
                tableParts.Append(new TablePart { Id = worksheetPart.GetIdOfPart(tableDefinitionPart) });
                worksheet.Append(tableParts);
            }

            worksheetPart.Worksheet = worksheet;
            worksheetPart.Worksheet.Save();

            var sheets = workbookPart.Workbook.AppendChild(new Sheets());
            sheets.Append(new Sheet
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "Musteriler"
            });

            workbookPart.Workbook.Save();
        }

        ms.Position = 0;
        return File(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Musteriler_{month}.xlsx");
    }
    
    
    [HttpGet("/Musteri/ExportNewCustomersExcelByMonth")]
public async Task<IActionResult> ExportNewCustomersExcelByMonth(string? month)
{
    DateTime monthStart;
    if (string.IsNullOrWhiteSpace(month))
    {
        // Ay seçilmezse her zaman bu (güncel) ayı kullan
        monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    }
    else if (!DateTime.TryParseExact(month.Trim(), "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out monthStart))
    {
        return BadRequest("Geçersiz ay formatı. Örn: 2026-02");
    }

    var start = new DateTime(monthStart.Year, monthStart.Month, 1, 0, 0, 0);
    var end = start.AddMonths(1);

    var rows = await _context.Musteriler
        .AsNoTracking()
        .Where(x => x.KayitTarihi != null
                    && x.KayitTarihi.Value >= start
                    && x.KayitTarihi.Value < end)
        .OrderByDescending(x => x.KayitTarihi)
        .ThenByDescending(x => x.MusteriID)
        .Select(x => new
        {
            x.MusteriID,
            x.Firma,
            x.FirmaYetkilisi,
            x.Telefon,
            x.SiteUrl,
            x.Teknoloji,
            x.Durum,
            x.TalepSahibi,
            x.Kaynak,
            x.KayitTarihi,
            x.DurumDegisiklikTarihi,
            x.Aciklama
        })
        .ToListAsync();

    // İş takip yorumlarını ilgili müşteri ID'lerine göre çek
    var musteriIds = rows.Select(r => r.MusteriID).ToList();

    var yorumlarByMusteri = await _context.JiraYorumlar
        .AsNoTracking()
        .Include(y => y.JiraTask)
        .Where(y => y.JiraTask.MusteriID != null && musteriIds.Contains(y.JiraTask.MusteriID!.Value))
        .Select(y => new
        {
            MusteriID = y.JiraTask.MusteriID!.Value,
            y.JiraTask.JiraId,
            y.JiraTask.TalepKonusu,
            y.Ekleyen,
            y.YorumMetni,
            y.Tarih
        })
        .OrderBy(y => y.MusteriID)
        .ThenBy(y => y.Tarih)
        .ToListAsync();

    // MusteriID -> yorumlar metni sözlüğü
    var yorumDict = yorumlarByMusteri
        .GroupBy(y => y.MusteriID)
        .ToDictionary(
            g => g.Key,
            g => string.Join(" || ", g.Select(y =>
                $"[{y.Tarih:dd.MM.yyyy}] {y.JiraId} - {y.TalepKonusu} | {y.Ekleyen}: {y.YorumMetni}"))
        );

    using var ms = new MemoryStream();

    using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
    {
        var workbookPart = document.AddWorkbookPart();
        workbookPart.Workbook = new Workbook();

        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        stylesPart.Stylesheet = CreateMusteriStylesheet();
        stylesPart.Stylesheet.Save();

        var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
        var worksheet = new Worksheet();
        var sheetData = new SheetData();

        worksheet.Append(new Columns(
            CreateColumn(1, 1, 10),
            CreateColumn(2, 2, 26),
            CreateColumn(3, 3, 22),
            CreateColumn(4, 4, 18),
            CreateColumn(5, 5, 28),
            CreateColumn(6, 6, 18),
            CreateColumn(7, 7, 18),
            CreateColumn(8, 8, 20),
            CreateColumn(9, 9, 18),
            CreateColumn(10, 10, 18),
            CreateColumn(11, 11, 22),
            CreateColumn(12, 12, 40),
            CreateColumn(13, 13, 70)
        ));

        worksheet.Append(sheetData);

        uint currentRow = 1;
        var header = new Row { RowIndex = currentRow };
        header.Append(
            CreateTextCell("A", currentRow, "ID", 1),
            CreateTextCell("B", currentRow, "Firma", 1),
            CreateTextCell("C", currentRow, "Yetkili", 1),
            CreateTextCell("D", currentRow, "Telefon", 1),
            CreateTextCell("E", currentRow, "SiteUrl", 1),
            CreateTextCell("F", currentRow, "Teknoloji", 1),
            CreateTextCell("G", currentRow, "Durum", 1),
            CreateTextCell("H", currentRow, "TalepSahibi", 1),
            CreateTextCell("I", currentRow, "Kaynak", 1),
            CreateTextCell("J", currentRow, "KayitTarihi", 1),
            CreateTextCell("K", currentRow, "DurumDegisiklikTarihi", 1),
            CreateTextCell("L", currentRow, "Aciklama", 1),
            CreateTextCell("M", currentRow, "IsTakipYorumlari", 1)
        );
        sheetData.Append(header);

        foreach (var r in rows)
        {
            yorumDict.TryGetValue(r.MusteriID, out var yorumlar);
            currentRow++;

            var row = new Row { RowIndex = currentRow };
            row.Append(
                CreateNumberCell("A", currentRow, r.MusteriID.ToString(CultureInfo.InvariantCulture), 0),
                CreateTextCell("B", currentRow, r.Firma, 0),
                CreateTextCell("C", currentRow, r.FirmaYetkilisi, 0),
                CreateTextCell("D", currentRow, r.Telefon, 0),
                CreateTextCell("E", currentRow, r.SiteUrl, 0),
                CreateTextCell("F", currentRow, r.Teknoloji, 0),
                CreateTextCell("G", currentRow, r.Durum, 0),
                CreateTextCell("H", currentRow, r.TalepSahibi, 0),
                CreateTextCell("I", currentRow, r.Kaynak, 0),
                CreateTextCell("J", currentRow, r.KayitTarihi?.ToString("dd.MM.yyyy") ?? "-", 0),
                CreateTextCell("K", currentRow, r.DurumDegisiklikTarihi?.ToString("dd.MM.yyyy") ?? "-", 0),
                CreateTextCell("L", currentRow, r.Aciklama, 0),
                CreateTextCell("M", currentRow, yorumlar, 0)
            );

            sheetData.Append(row);
        }

        var lastRowIndex = currentRow == 1 ? 1U : currentRow;
        var filterRange = $"A1:M{lastRowIndex}";
        worksheet.Append(new AutoFilter { Reference = filterRange });

        if (rows.Any())
        {
            var tableDefinitionPart = worksheetPart.AddNewPart<TableDefinitionPart>();
            tableDefinitionPart.Table = new Table
            {
                Id = 1U,
                Name = "YeniMusterilerTablo",
                DisplayName = "YeniMusterilerTablo",
                Reference = filterRange,
                TotalsRowShown = false
            };

            tableDefinitionPart.Table.AppendChild(new AutoFilter { Reference = filterRange });
            tableDefinitionPart.Table.AppendChild(new TableColumns(
                new TableColumn { Id = 1U, Name = "ID" },
                new TableColumn { Id = 2U, Name = "Firma" },
                new TableColumn { Id = 3U, Name = "Yetkili" },
                new TableColumn { Id = 4U, Name = "Telefon" },
                new TableColumn { Id = 5U, Name = "SiteUrl" },
                new TableColumn { Id = 6U, Name = "Teknoloji" },
                new TableColumn { Id = 7U, Name = "Durum" },
                new TableColumn { Id = 8U, Name = "TalepSahibi" },
                new TableColumn { Id = 9U, Name = "Kaynak" },
                new TableColumn { Id = 10U, Name = "KayitTarihi" },
                new TableColumn { Id = 11U, Name = "DurumDegisiklikTarihi" },
                new TableColumn { Id = 12U, Name = "Aciklama" },
                new TableColumn { Id = 13U, Name = "IsTakipYorumlari" }
            ) { Count = 13U });

            tableDefinitionPart.Table.AppendChild(new TableStyleInfo
            {
                Name = "TableStyleMedium2",
                ShowFirstColumn = false,
                ShowLastColumn = false,
                ShowRowStripes = true,
                ShowColumnStripes = false
            });

            tableDefinitionPart.Table.Save();

            var tableParts = new TableParts { Count = 1U };
            tableParts.Append(new TablePart { Id = worksheetPart.GetIdOfPart(tableDefinitionPart) });
            worksheet.Append(tableParts);
        }

        worksheetPart.Worksheet = worksheet;
        worksheetPart.Worksheet.Save();

        var sheets = workbookPart.Workbook.AppendChild(new Sheets());
        sheets.Append(new Sheet
        {
            Id = workbookPart.GetIdOfPart(worksheetPart),
            SheetId = 1,
            Name = "YeniMusteriler"
        });

        workbookPart.Workbook.Save();
    }

    ms.Position = 0;
    var ayLabel = $"{monthStart:yyyy-MM}";
    return File(
        ms.ToArray(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"YeniMusteriler_{ayLabel}.xlsx");
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

    private static Stylesheet CreateMusteriStylesheet()
    {
        return new Stylesheet(
            new Fonts(
                new Font(new FontSize { Val = 11 }),
                new Font(new Bold(), new FontSize { Val = 11 }, new Color { Rgb = HexBinaryValue.FromString("FFFFFFFF") })
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
                new Border(new LeftBorder(), new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder()),
                new Border(
                    new LeftBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                    new RightBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                    new TopBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                    new BottomBorder { Style = BorderStyleValues.Thin, Color = new Color { Auto = true } },
                    new DiagonalBorder())
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
