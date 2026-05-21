using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using BtOperasyonTakip.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;

namespace BtOperasyonTakip.Controllers
{
    [Authorize]
    public class TakipArsivController : Controller
    {
        private readonly AppDbContext _context;

        public TakipArsivController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(string? q, string? ay, string? durum)
        {
            q = (q ?? string.Empty).Trim();

            var months = _context.JiraTasks
                .AsNoTracking()
                .Select(x => x.OlusturmaTarihi)
                .ToList()
                .Select(dt => new DateTime(dt.Year, dt.Month, 1))
                .Distinct()
                .OrderByDescending(x => x)
                .ToList();

            ViewBag.AvailableAylar = months;

            DateTime? filterMonthStart = null;
            DateTime? filterNextMonthStart = null;

            if (!string.IsNullOrWhiteSpace(ay) &&
                DateTime.TryParseExact(
                    ay.Trim(),
                    "yyyy-MM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                filterMonthStart = new DateTime(parsed.Year, parsed.Month, 1);
                filterNextMonthStart = filterMonthStart.Value.AddMonths(1);
                ViewBag.SelectedAy = filterMonthStart.Value.ToString("yyyy-MM");
            }
            else
            {
                ViewBag.SelectedAy = string.Empty;
            }

            var query = _context.JiraTasks
                .AsNoTracking()
                .Include(x => x.Yorumlar)
                .AsQueryable();

            var selectedDurum = (durum ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(selectedDurum))
            {
                var selectedDurumLower = selectedDurum.ToLower();

                query = query.Where(x =>
                    ((x.Durum ?? string.Empty).Trim().ToLower()) == selectedDurumLower);
            }

            var now = DateTime.Now;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);

            if (filterMonthStart.HasValue && filterNextMonthStart.HasValue)
            {
                var start = filterMonthStart.Value;
                var end = filterNextMonthStart.Value;

                if (start < currentMonthStart)
                {
                    query = query.Where(x =>
                        x.OlusturmaTarihi >= start &&
                        x.OlusturmaTarihi < end);
                }
                else
                {
                    query = query.Where(x =>
                        x.OlusturmaTarihi >= start &&
                        x.OlusturmaTarihi < end &&
                        (
                            ((x.Durum ?? string.Empty).Trim()) == "Tamamlandı" ||
                            ((x.Durum ?? string.Empty).Trim()) == "Arşiv"
                        ));
                }
            }
            else
            {
                query = query.Where(x =>
                    x.OlusturmaTarihi < currentMonthStart ||
                    ((x.Durum ?? string.Empty).Trim()) == "Tamamlandı" ||
                    ((x.Durum ?? string.Empty).Trim()) == "Arşiv");
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.ToLowerInvariant();

                query = query.Where(t =>
                    (t.MusteriAdi ?? string.Empty).ToLower().Contains(qq) ||
                    (t.JiraId ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TalepKonusu ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TalepTuru ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TalepAcan ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TakipEden ?? string.Empty).ToLower().Contains(qq) ||
                    (t.Durum ?? string.Empty).ToLower().Contains(qq));
            }

            var all = query.ToList();

            var yorumLookup = _context.JiraYorumlar
                .AsNoTracking()
                .Where(y => y.YorumMetni == "Arşivlendi")
                .GroupBy(y => y.JiraTaskId)
                .ToDictionary(g => g.Key, g => g.Max(y => y.Tarih));

            var tasks = all
                .OrderByDescending(t => yorumLookup.TryGetValue(t.Id, out var dt)
                    ? dt
                    : t.OlusturmaTarihi)
                .ToList();

            IQueryable<Detay> detayQuery = _context.Detaylar.AsNoTracking();

            if (filterMonthStart.HasValue && filterNextMonthStart.HasValue)
            {
                var s = filterMonthStart.Value;
                var e = filterNextMonthStart.Value;

                detayQuery = detayQuery.Where(d => d.Tarih >= s && d.Tarih < e);
            }
            else
            {
                detayQuery = detayQuery.Where(d =>
                    d.Tarih < currentMonthStart ||
                    d.Tarih >= currentMonthStart);
            }

            var detaylar = detayQuery
                .OrderByDescending(d => d.Tarih)
                .ToList()
                .Where(d =>
                    !string.IsNullOrWhiteSpace(d.Kekleyen) &&
                    !string.Equals(d.Kekleyen.Trim(), "Sistem", StringComparison.OrdinalIgnoreCase))
                .Where(d =>
                    !string.Equals(
                        (d.Gorusulen ?? string.Empty).Trim(),
                        "İş Takip Yorumu",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            var musteriLookup = _context.Musteriler
                .AsNoTracking()
                .ToDictionary(m => m.MusteriID, m => m.Firma);

            var musteriDurumById = _context.Musteriler
                .AsNoTracking()
                .ToDictionary(x => x.MusteriID, x => (x.Durum ?? string.Empty).Trim());

            var legacyTasks = detaylar.Select(d => new JiraTask
            {
                Id = -d.DetayID,
                MusteriID = d.MusteriID,
                MusteriAdi = musteriLookup.TryGetValue(d.MusteriID, out var ma)
                    ? ma
                    : string.Empty,
                JiraId = string.Empty,
                TalepKonusu = string.IsNullOrWhiteSpace(d.Gorusulen)
                    ? "Detay"
                    : d.Gorusulen.Trim(),
                TalepTuru = "Detay",
                TalepAcan = string.IsNullOrWhiteSpace(d.Kekleyen)
                    ? "-"
                    : d.Kekleyen.Trim(),
                Durum = musteriDurumById.TryGetValue(d.MusteriID, out var md)
                    ? md
                    : "Aktif",
                TakipEden = d.Kekleyen,
                OlusturmaTarihi = d.Tarih,
                Yorumlar = new List<JiraYorum>()
            }).ToList();

            tasks = tasks
                .Concat(legacyTasks)
                .OrderByDescending(t => yorumLookup.TryGetValue(t.Id, out var dt)
                    ? dt
                    : t.OlusturmaTarihi)
                .ToList();

            var archiveDateByTask = tasks.ToDictionary(
                t => t.Id,
                t => yorumLookup.TryGetValue(t.Id, out var dt) ? (DateTime?)dt : null);

            ViewBag.ArchiveDateByTask = archiveDateByTask;

            var durumList = _context.Parametreler
                .AsNoTracking()
                .Where(p => p.Tur == "Durum" && p.ParAdi != null && p.ParAdi != string.Empty)
                .Select(p => p.ParAdi!)
                .ToList()
                .Select(s => s.Trim())
                .Where(s => s != string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();

            ViewBag.DurumList = durumList;
            ViewBag.SelectedDurum = selectedDurum;

            var operasyonKullaniciList = _context.Users
                .AsNoTracking()
                .Where(u => u.Role == AppRoles.Operasyon)
                .Select(x => string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : x.FullName!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var latestDetailByMusteri = _context.Detaylar
                .AsNoTracking()
                .OrderByDescending(d => d.Tarih)
                .ThenByDescending(d => d.DetayID)
                .ToList()
                .GroupBy(d => d.MusteriID)
                .ToDictionary(g => g.Key, g => g.First());

            var taskLatestDetaylar = tasks.ToDictionary(
                t => t.Id,
                t => t.MusteriID.HasValue &&
                     t.MusteriID.Value > 0 &&
                     latestDetailByMusteri.TryGetValue(t.MusteriID.Value, out var det)
                    ? det
                    : null);

            var taskMusteriDurumlari = tasks.ToDictionary(
                t => t.Id,
                t => t.MusteriID.HasValue &&
                     t.MusteriID.Value > 0 &&
                     musteriDurumById.TryGetValue(t.MusteriID.Value, out var d)
                    ? d
                    : string.Empty);

            ViewBag.KullaniciSecenekleri = operasyonKullaniciList;
            ViewBag.TaskLatestDetaylar = taskLatestDetaylar;
            ViewBag.TaskMusteriDurumlari = taskMusteriDurumlari;

            ViewBag.Title = "Arşiv - Diğer İşler";
            ViewBag.Q = q;

            return View("~/Views/Takip/Arsiv.cshtml", tasks);
        }

        [HttpGet]
        public async Task<IActionResult> DetailCard(int id)
        {
            int? musteriId = null;
            string musteriAdi = string.Empty;

            // Arşivde Detaylar tablosundan gelen kayıtlar negatif ID ile geliyor.
            // Örnek: Id = -DetayID
            if (id < 0)
            {
                var detayId = Math.Abs(id);

                var detay = await _context.Detaylar
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.DetayID == detayId);

                if (detay == null)
                    return NotFound("Detay kaydı bulunamadı.");

                musteriId = detay.MusteriID;
            }
            else
            {
                var task = await _context.JiraTasks
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (task == null)
                    return NotFound("İş takip kaydı bulunamadı.");

                musteriId = task.MusteriID;
            }

            if (!musteriId.HasValue || musteriId.Value <= 0)
                return NotFound("Müşteri bilgisi bulunamadı.");

            var musteri = await _context.Musteriler
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MusteriID == musteriId.Value);

            if (musteri != null)
                musteriAdi = musteri.Firma ?? string.Empty;

            var detaylar = await _context.Detaylar
                .AsNoTracking()
                .Where(x => x.MusteriID == musteriId.Value)
                .OrderByDescending(x => x.Tarih)
                .ThenByDescending(x => x.DetayID)
                .ToListAsync();

            var operasyonKullaniciList = await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == BtOperasyonTakip.Security.AppRoles.Operasyon)
                .Select(x => string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : x.FullName!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.MusteriID = musteriId.Value;
            ViewBag.MusteriAdi = musteriAdi;
            ViewBag.KullaniciSecenekleri = operasyonKullaniciList;

            return PartialView("~/Views/Takip/_DetailCard.cshtml", detaylar);
        }
        [HttpGet]
        public IActionResult ExportExcel(string? q, string? ay, string? durum)
        {
            q = (q ?? string.Empty).Trim();

            var query = _context.JiraTasks
                .AsNoTracking()
                .Include(x => x.Yorumlar)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(durum))
            {
                var durumLower = durum.Trim().ToLower();

                query = query.Where(x =>
                    ((x.Durum ?? string.Empty).Trim().ToLower()) == durumLower);
            }

            var now = DateTime.Now;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);

            DateTime? filterMonthStart = null;
            DateTime? filterNextMonthStart = null;

            if (!string.IsNullOrWhiteSpace(ay) &&
                DateTime.TryParseExact(
                    ay.Trim(),
                    "yyyy-MM",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                filterMonthStart = new DateTime(parsed.Year, parsed.Month, 1);
                filterNextMonthStart = filterMonthStart.Value.AddMonths(1);

                if (filterMonthStart.Value < currentMonthStart)
                {
                    query = query.Where(x =>
                        x.OlusturmaTarihi >= filterMonthStart.Value &&
                        x.OlusturmaTarihi < filterNextMonthStart.Value);
                }
                else
                {
                    query = query.Where(x =>
                        x.OlusturmaTarihi >= filterMonthStart.Value &&
                        x.OlusturmaTarihi < filterNextMonthStart.Value &&
                        (
                            ((x.Durum ?? string.Empty).Trim()) == "Tamamlandı" ||
                            ((x.Durum ?? string.Empty).Trim()) == "Arşiv"
                        ));
                }
            }
            else
            {
                query = query.Where(x =>
                    x.OlusturmaTarihi < currentMonthStart ||
                    ((x.Durum ?? string.Empty).Trim()) == "Tamamlandı" ||
                    ((x.Durum ?? string.Empty).Trim()) == "Arşiv");
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.ToLowerInvariant();

                query = query.Where(t =>
                    (t.MusteriAdi ?? string.Empty).ToLower().Contains(qq) ||
                    (t.JiraId ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TalepKonusu ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TalepTuru ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TalepAcan ?? string.Empty).ToLower().Contains(qq) ||
                    (t.TakipEden ?? string.Empty).ToLower().Contains(qq) ||
                    (t.Durum ?? string.Empty).ToLower().Contains(qq));
            }

            var list = query
                .OrderByDescending(x => x.OlusturmaTarihi)
                .ToList();

            var yorumLookup = _context.JiraYorumlar
                .AsNoTracking()
                .GroupBy(y => y.JiraTaskId)
                .ToDictionary(g => g.Key, g => g.OrderBy(y => y.Tarih).ToList());

            var archiveDateLookup = _context.JiraYorumlar
                .AsNoTracking()
                .Where(y => y.YorumMetni == "Arşivlendi")
                .GroupBy(y => y.JiraTaskId)
                .ToDictionary(g => g.Key, g => g.Max(y => y.Tarih));

            var rows = list.Select(x => new
            {
                Kaynak = string.Equals(x.TalepTuru, "Detay", StringComparison.OrdinalIgnoreCase)
                    ? "Detay"
                    : "Takip",
                x.MusteriAdi,
                x.JiraId,
                x.TalepKonusu,
                x.TalepTuru,
                x.TalepAcan,
                x.TakipEden,
                x.Durum,
                x.OlusturmaTarihi,
                ArchiveDate = archiveDateLookup.TryGetValue(x.Id, out var ad) ? (DateTime?)ad : null,
                Yorumlar = yorumLookup.TryGetValue(x.Id, out var y)
                    ? string.Join("\n\n", y.Select(c =>
                        $"{c.Tarih:dd.MM.yyyy HH:mm} - {((c.Ekleyen ?? string.Empty).Trim())}\n{(c.YorumMetni ?? string.Empty)}"))
                    : string.Empty
            }).ToList();

            using var ms = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

                var numberingFormats = new NumberingFormats();
                uint dateFormatId = 164;

                numberingFormats.Append(new NumberingFormat
                {
                    NumberFormatId = dateFormatId,
                    FormatCode = StringValue.FromString("dd.MM.yyyy HH:mm")
                });

                var fonts = new Fonts(
                    new Font(),
                    new Font(new Bold())
                );

                var fills = new Fills(
                    new Fill(new PatternFill { PatternType = PatternValues.None }),
                    new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
                    new Fill(new PatternFill(
                        new ForegroundColor { Rgb = HexBinaryValue.FromString("FFDCE6F1") })
                    {
                        PatternType = PatternValues.Solid
                    })
                );

                var borders = new Borders(new Border());
                var cellStyleFormats = new CellStyleFormats(new CellFormat());

                var cellFormats = new CellFormats(
                    new CellFormat(),
                    new CellFormat
                    {
                        FontId = 1U,
                        FillId = 2U,
                        ApplyFont = true,
                        ApplyFill = true
                    },
                    new CellFormat
                    {
                        NumberFormatId = dateFormatId,
                        ApplyNumberFormat = true
                    },
                    new CellFormat
                    {
                        Alignment = new Alignment { WrapText = true },
                        ApplyAlignment = true
                    }
                );

                var stylesheet = new Stylesheet();
                stylesheet.Append(numberingFormats);
                stylesheet.Append(fonts);
                stylesheet.Append(fills);
                stylesheet.Append(borders);
                stylesheet.Append(cellStyleFormats);
                stylesheet.Append(cellFormats);

                stylesPart.Stylesheet = stylesheet;
                stylesPart.Stylesheet.Save();

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());
                var sheetPart = workbookPart.AddNewPart<WorksheetPart>();

                var columns = new Columns(
                    new Column { Min = 1, Max = 1, Width = 10, CustomWidth = true },
                    new Column { Min = 2, Max = 2, Width = 28, CustomWidth = true },
                    new Column { Min = 3, Max = 3, Width = 12, CustomWidth = true },
                    new Column { Min = 4, Max = 4, Width = 36, CustomWidth = true },
                    new Column { Min = 5, Max = 5, Width = 16, CustomWidth = true },
                    new Column { Min = 6, Max = 6, Width = 16, CustomWidth = true },
                    new Column { Min = 7, Max = 7, Width = 20, CustomWidth = true },
                    new Column { Min = 8, Max = 8, Width = 14, CustomWidth = true },
                    new Column { Min = 9, Max = 9, Width = 20, CustomWidth = true },
                    new Column { Min = 10, Max = 10, Width = 20, CustomWidth = true },
                    new Column { Min = 11, Max = 11, Width = 60, CustomWidth = true }
                );

                var sheetData = new SheetData();

                var worksheet = new Worksheet();
                worksheet.Append(columns);
                worksheet.Append(sheetData);

                var headerRow = new Row();

                var headers = new[]
                {
                    "Kaynak",
                    "Müşteri",
                    "Jira No",
                    "Talep Konusu",
                    "Talep Türü",
                    "Talep Açan",
                    "Takip Eden",
                    "Durum",
                    "Oluşturma Tarihi",
                    "Tamamlanma/Arşiv Tarihi",
                    "Yorumlar"
                };

                foreach (var h in headers)
                {
                    headerRow.Append(new Cell
                    {
                        DataType = CellValues.InlineString,
                        InlineString = new InlineString(new Text(h)),
                        StyleIndex = 1U
                    });
                }

                sheetData.Append(headerRow);

                foreach (var r in rows)
                {
                    var row = new Row();

                    void AddText(string? value, uint style = 0U)
                    {
                        row.Append(new Cell
                        {
                            DataType = CellValues.InlineString,
                            InlineString = new InlineString(new Text(value ?? string.Empty)),
                            StyleIndex = style
                        });
                    }

                    void AddDate(DateTime? date)
                    {
                        if (date.HasValue)
                        {
                            row.Append(new Cell
                            {
                                CellValue = new CellValue(date.Value.ToOADate().ToString(CultureInfo.InvariantCulture)),
                                StyleIndex = 2U
                            });
                        }
                        else
                        {
                            row.Append(new Cell
                            {
                                DataType = CellValues.InlineString,
                                InlineString = new InlineString(new Text(string.Empty))
                            });
                        }
                    }

                    AddText(r.Kaynak);
                    AddText(r.MusteriAdi);
                    AddText(r.JiraId);
                    AddText(r.TalepKonusu);
                    AddText(r.TalepTuru);
                    AddText(r.TalepAcan);
                    AddText(r.TakipEden);
                    AddText(r.Durum);
                    AddDate(r.OlusturmaTarihi);
                    AddDate(r.ArchiveDate);
                    AddText(r.Yorumlar, 3U);

                    sheetData.Append(row);
                }

                sheetPart.Worksheet = worksheet;
                sheetPart.Worksheet.Save();

                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(sheetPart),
                    SheetId = 1,
                    Name = "Arsiv"
                });

                workbookPart.Workbook.Save();
            }

            ms.Position = 0;

            var fileName = $"Arsiv_TumAylar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}