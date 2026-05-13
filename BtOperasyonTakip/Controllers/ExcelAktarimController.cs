using System.Globalization;
using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Controllers;

[Authorize(Roles = "Operasyon,Admin")]
public sealed class ExcelAktarimController : Controller
{
    private readonly AppDbContext _context;

    public ExcelAktarimController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 25)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 25 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.ExcelAktarimKayitlari
            .AsNoTracking()
            .Include(x => x.EslesenMusteri)
            .AsQueryable();

        q = (q ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(x =>
                (x.DosyaAdi ?? string.Empty).Contains(q) ||
                (x.MusteriAdi ?? string.Empty).Contains(q) ||
                (x.Gorusulen ?? string.Empty).Contains(q) ||
                (x.Aciklama ?? string.Empty).Contains(q) ||
                (x.Ekleyen ?? string.Empty).Contains(q) ||
                (x.YukleyenKullaniciAdi ?? string.Empty).Contains(q));
        }

        var totalCount = await query.CountAsync();
        var eslesenCount = await query.CountAsync(x => x.EslesenMusteriID != null);
        var eslesmeyenCount = await query.CountAsync(x => x.EslesenMusteriID == null);

        var kayitlar = await query
            .OrderByDescending(x => x.YuklemeTarihi)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new ExcelAktarimIndexViewModel
        {
            Kayitlar = kayitlar,
            Q = q,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            EslesenCount = eslesenCount,
            EslesmeyenCount = eslesmeyenCount
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile? file)
    {
        if (file == null || file.Length <= 0)
        {
            TempData["ExcelAktarimError"] = "Lütfen bir Excel dosyası seçiniz.";
            return RedirectToAction(nameof(Index));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ExcelAktarimError"] = "Sadece .xlsx dosyaları yüklenebilir.";
            return RedirectToAction(nameof(Index));
        }

        List<ParsedExcelRow> parsedRows;

        try
        {
            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;
            parsedRows = ReadImportedRows(stream);
        }
        catch (Exception ex)
        {
            TempData["ExcelAktarimError"] = $"Excel okunamadı: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }

        if (!parsedRows.Any())
        {
            TempData["ExcelAktarimError"] = "Excel içinde aktarılacak kayıt bulunamadı.";
            return RedirectToAction(nameof(Index));
        }

        var musteriler = await _context.Musteriler
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.Firma))
            .Select(x => new { x.MusteriID, x.Firma })
            .ToListAsync();

        var musteriLookup = musteriler
            .GroupBy(x => NormalizeMatchText(x.Firma))
            .ToDictionary(g => g.Key, g => g.First().MusteriID);

        var matchedIds = parsedRows
            .Select(x => x.MusteriAdi)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => NormalizeMatchText(x))
            .Where(x => musteriLookup.ContainsKey(x))
            .Select(x => musteriLookup[x])
            .Distinct()
            .ToList();

        var existingDetailKeys = await _context.Detaylar
            .AsNoTracking()
            .Where(x => matchedIds.Contains(x.MusteriID))
            .Select(x => BuildDetailKey(x.MusteriID, x.Tarih, x.Gorusulen, x.Aciklama, x.Kekleyen))
            .ToListAsync();

        var detailKeySet = new HashSet<string>(existingDetailKeys, StringComparer.OrdinalIgnoreCase);
        var uploader = User?.Identity?.Name;
        var now = DateTime.Now;
        var importedCount = 0;
        var detailAddedCount = 0;

        foreach (var row in parsedRows)
        {
            int? eslesenMusteriId = null;
            if (!string.IsNullOrWhiteSpace(row.MusteriAdi))
            {
                var normalizedMusteri = NormalizeMatchText(row.MusteriAdi);
                if (musteriLookup.TryGetValue(normalizedMusteri, out var matchedId))
                {
                    eslesenMusteriId = matchedId;
                }
            }

            _context.ExcelAktarimKayitlari.Add(new ExcelAktarimKaydi
            {
                DosyaAdi = Path.GetFileName(file.FileName),
                SayfaAdi = row.SayfaAdi,
                SatirNo = row.SatirNo,
                MusteriAdi = row.MusteriAdi,
                Tarih = row.Tarih,
                Gorusulen = row.Gorusulen,
                Aciklama = row.Aciklama,
                Ekleyen = row.Ekleyen,
                EslesenMusteriID = eslesenMusteriId,
                YukleyenKullaniciAdi = uploader,
                YuklemeTarihi = now
            });

            importedCount++;

            if (eslesenMusteriId.HasValue)
            {
                var detayTarihi = row.Tarih ?? now;
                var gorusulen = string.IsNullOrWhiteSpace(row.Gorusulen) ? "Excel Aktarımı" : row.Gorusulen.Trim();
                var aciklama = string.IsNullOrWhiteSpace(row.Aciklama) ? row.MusteriAdi : row.Aciklama.Trim();
                var ekleyen = string.IsNullOrWhiteSpace(row.Ekleyen) ? uploader : row.Ekleyen.Trim();
                var detailKey = BuildDetailKey(eslesenMusteriId.Value, detayTarihi, gorusulen, aciklama, ekleyen);

                if (!detailKeySet.Contains(detailKey))
                {
                    _context.Detaylar.Add(new Detay
                    {
                        MusteriID = eslesenMusteriId.Value,
                        Tarih = detayTarihi,
                        Gorusulen = gorusulen,
                        Aciklama = aciklama,
                        Kekleyen = ekleyen
                    });

                    detailKeySet.Add(detailKey);
                    detailAddedCount++;
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData["ExcelAktarimOk"] = $"{importedCount} kayıt içe aktarıldı. {detailAddedCount} kayıt detaylara eklendi.";
        return RedirectToAction(nameof(Index));
    }

    private static List<ParsedExcelRow> ReadImportedRows(Stream stream)
    {
        using var document = SpreadsheetDocument.Open(stream, false);
        var workbookPart = document.WorkbookPart;
        if (workbookPart?.Workbook?.Sheets == null)
            return new List<ParsedExcelRow>();

        var firstSheet = workbookPart.Workbook.Sheets.Elements<Sheet>().FirstOrDefault();
        if (firstSheet?.Id == null)
            return new List<ParsedExcelRow>();

        var worksheetPart = (WorksheetPart)workbookPart.GetPartById(firstSheet.Id!);
        var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        if (sheetData == null)
            return new List<ParsedExcelRow>();

        var rows = sheetData.Elements<Row>().ToList();
        if (rows.Count < 2)
            return new List<ParsedExcelRow>();

        var headerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in rows[0].Elements<Cell>())
        {
            var columnName = GetColumnName(cell.CellReference?.Value);
            if (string.IsNullOrWhiteSpace(columnName)) continue;
            headerMap[columnName] = NormalizeHeaderName(GetCellText(workbookPart, cell));
        }

        var result = new List<ParsedExcelRow>();
        foreach (var row in rows.Skip(1))
        {
            var valueMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var cell in row.Elements<Cell>())
            {
                var columnName = GetColumnName(cell.CellReference?.Value);
                if (string.IsNullOrWhiteSpace(columnName)) continue;
                if (!headerMap.TryGetValue(columnName, out var normalizedHeader)) continue;
                if (string.IsNullOrWhiteSpace(normalizedHeader)) continue;

                valueMap[normalizedHeader] = (GetCellText(workbookPart, cell) ?? string.Empty).Trim();
            }

            if (!valueMap.Values.Any(x => !string.IsNullOrWhiteSpace(x)))
                continue;

            var tarihText = GetValue(valueMap, "tarih", "tarihsaat", "kayittarihi", "veritarihi", "olusturmatarihi");
            TryParseImportedDate(tarihText, out var parsedDate);

            result.Add(new ParsedExcelRow
            {
                SayfaAdi = firstSheet.Name?.Value,
                SatirNo = (int)(row.RowIndex?.Value ?? 0),
                MusteriAdi = GetValue(valueMap, "musteri", "musteriadi", "firma"),
                Tarih = parsedDate,
                Gorusulen = GetValue(valueMap, "isgorusulen", "gorusulen", "talepkonusu", "is"),
                Aciklama = GetValue(valueMap, "aciklama", "yorum", "detay", "yorumdetay"),
                Ekleyen = GetValue(valueMap, "ekleyen", "kekleyen", "yorumekleyen")
            });
        }

        return result;
    }

    private static string GetCellText(WorkbookPart workbookPart, Cell? cell)
    {
        if (cell == null)
            return string.Empty;

        var value = cell.CellValue?.InnerText ?? cell.InnerText ?? string.Empty;

        if (cell.DataType == null)
            return value;

        var dataType = cell.DataType.InnerText;

        if (string.Equals(dataType, "s", StringComparison.OrdinalIgnoreCase))
        {
            return int.TryParse(value, out var index)
                ? workbookPart.SharedStringTablePart?.SharedStringTable?.Elements<SharedStringItem>().ElementAtOrDefault(index)?.InnerText ?? string.Empty
                : string.Empty;
        }

        if (string.Equals(dataType, "inlineStr", StringComparison.OrdinalIgnoreCase))
            return cell.InlineString?.InnerText ?? value;

        if (string.Equals(dataType, "b", StringComparison.OrdinalIgnoreCase))
            return value == "1" ? "Evet" : "Hayır";

        return value;
    }

    private static string GetColumnName(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
            return string.Empty;

        return new string(cellReference.Where(char.IsLetter).ToArray());
    }

    private static string NormalizeHeaderName(string? value)
    {
        var text = NormalizeMatchText(value);
        return text
            .Replace("/", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);
    }

    private static string NormalizeMatchText(string? value)
    {
        return (value ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Replace("ç", "c")
            .Replace("ğ", "g")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ş", "s")
            .Replace("ü", "u");
    }

    private static string? GetValue(IReadOnlyDictionary<string, string> values, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    private static bool TryParseImportedDate(string? value, out DateTime date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
        {
            try
            {
                date = DateTime.FromOADate(oaDate);
                return true;
            }
            catch
            {
            }
        }

        var formats = new[]
        {
            "dd.MM.yyyy HH:mm",
            "d.M.yyyy HH:mm",
            "dd.MM.yyyy H:mm",
            "d.M.yyyy H:mm",
            "dd.MM.yyyy",
            "d.M.yyyy",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd"
        };

        return DateTime.TryParseExact(value.Trim(), formats, new CultureInfo("tr-TR"), DateTimeStyles.None, out date)
            || DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
            || DateTime.TryParse(value, new CultureInfo("tr-TR"), DateTimeStyles.None, out date);
    }

    private static string BuildDetailKey(int musteriId, DateTime tarih, string? gorusulen, string? aciklama, string? ekleyen)
    {
        return string.Join("|",
            musteriId.ToString(CultureInfo.InvariantCulture),
            tarih.ToString("O", CultureInfo.InvariantCulture),
            NormalizeMatchText(gorusulen),
            NormalizeMatchText(aciklama),
            NormalizeMatchText(ekleyen));
    }

    private sealed class ParsedExcelRow
    {
        public string? SayfaAdi { get; set; }
        public int SatirNo { get; set; }
        public string? MusteriAdi { get; set; }
        public DateTime? Tarih { get; set; }
        public string? Gorusulen { get; set; }
        public string? Aciklama { get; set; }
        public string? Ekleyen { get; set; }
    }
}
