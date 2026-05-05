using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using BtOperasyonTakip.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BtOperasyonTakip.Controllers
{
    [Authorize]
    public class TakipController : Controller
    {
        private readonly AppDbContext _context;

        private static readonly string[] AllowedTalepTurleri = ["Geliştirme", "Eğitim", "Entegrasyon", "Hata Çözüm"];
        private static readonly string[] ExcelDisiTalepTurleri = ["Geliştirme", "Eğitim"];
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        public TakipController(AppDbContext context) => _context = context;

        private IActionResult RedirectToLocalOrIndex(string? returnUrl, object? routeValues = null)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index), routeValues);
        }

        private string? RemoveQueryParametersFromLocalUrl(string? returnUrl, params string[] parameterNames)
        {
            if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl) || parameterNames == null || parameterNames.Length == 0)
                return returnUrl;

            var hashIndex = returnUrl.IndexOf('#');
            var hashPart = hashIndex >= 0 ? returnUrl[hashIndex..] : string.Empty;
            var pathAndQuery = hashIndex >= 0 ? returnUrl[..hashIndex] : returnUrl;

            var queryIndex = pathAndQuery.IndexOf('?');
            if (queryIndex < 0)
                return returnUrl;

            var path = pathAndQuery[..queryIndex];
            var query = QueryHelpers.ParseQuery(pathAndQuery[queryIndex..]);
            var filteredQuery = query
                .Where(x => !parameterNames.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
                .SelectMany(x => x.Value, (x, value) => new KeyValuePair<string, string?>(x.Key, value));

            var newQueryString = QueryString.Create(filteredQuery).ToString();
            return $"{path}{newQueryString}{hashPart}";
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

        private List<string> GetOperasyonKullaniciSecenekleri()
        {
            return _context.Users
                .AsNoTracking()
                .Where(x => x.Role == AppRoles.Operasyon)
                .Select(x => string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : x.FullName!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private List<string> GetTalepAcanSecenekleri()
        {
            return _context.Users
                .AsNoTracking()
                .Select(x => string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : x.FullName!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Concat(
                    _context.Parametreler
                        .AsNoTracking()
                        .Where(x =>
                            (x.Tur == "TalepSahibi" || x.Tur == "TalepEden") &&
                            x.ParAdi != null &&
                            x.ParAdi != "")
                        .Select(x => x.ParAdi!.Trim()))
                .Concat(
                    _context.Musteriler
                        .AsNoTracking()
                        .Where(x => x.TalepSahibi != null && x.TalepSahibi != "")
                        .Select(x => x.TalepSahibi!.Trim()))
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private void AddYorumVeMusteriDetay(JiraTask task, string yorum, string ekleyen)
        {
            var now = DateTime.Now;

            _context.JiraYorumlar.Add(new JiraYorum
            {
                JiraTaskId = task.Id,
                YorumMetni = yorum,
                Ekleyen = ekleyen,
                Tarih = now
            });

            if (task.MusteriID.HasValue && task.MusteriID.Value > 0)
            {
                _context.Detaylar.Add(new Detay
                {
                    MusteriID = task.MusteriID.Value,
                    Tarih = now,
                    Gorusulen = string.IsNullOrWhiteSpace(task.TalepKonusu) ? "İş Takip Yorumu" : task.TalepKonusu.Trim(),
                    Aciklama = yorum,
                    Kekleyen = ekleyen
                });
            }

            _context.SaveChanges();
        }

        [HttpGet]
        public IActionResult Index(string? q, string? kisi = null, int? selectedTaskId = null, int page = 1, int pageSize = DefaultPageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? DefaultPageSize : pageSize;
            pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

            var anaDurumlar = new[] { "Beklemede", "Aktif", "Tamamlandı" };
            var ekstraDurumlar = Request.Query["ekstraDurumlar"].ToArray();
            var seciliEkstraDurumlar = (ekstraDurumlar ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Where(x => !anaDurumlar.Contains(x, StringComparer.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var talepAcanList = GetTalepAcanSecenekleri();
            var operasyonKullaniciList = GetOperasyonKullaniciSecenekleri();
            kisi = (kisi ?? string.Empty).Trim();

            var musteriList = _context.Musteriler
                .AsNoTracking()
                .Where(m => !string.IsNullOrWhiteSpace(m.Firma))
                .OrderBy(m => m.Firma)
                .ToList();

            var query = _context.JiraTasks
                .AsNoTracking()
                .Include(x => x.Yorumlar)
                .AsQueryable();

            q = (q ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.ToLowerInvariant();
                query = query.Where(t =>
                    (t.MusteriAdi ?? "").ToLower().Contains(qq) ||
                    (t.TalepKonusu ?? "").ToLower().Contains(qq) ||
                    (t.TalepAcan ?? "").ToLower().Contains(qq) ||
                    (t.TakipEden ?? "").ToLower().Contains(qq) ||
                    (t.Durum ?? "").ToLower().Contains(qq));
            }

            if (!string.IsNullOrWhiteSpace(kisi))
            {
                var selectedKisi = kisi.ToLowerInvariant();
                query = query.Where(t => (t.TakipEden ?? "").ToLower() == selectedKisi);
            }

            query = query.OrderByDescending(x => x.OlusturmaTarihi);

            var totalCount = query.Count();

            var paged = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var durumListesi = anaDurumlar
                .Concat(_context.Parametreler
                .AsNoTracking()
                .Where(x => x.Tur == "Durum" && x.ParAdi != null && x.ParAdi != "")
                .OrderBy(x => x.Id)
                .Select(x => x.ParAdi!.Trim())
                .Where(x => x != "")
                .ToList())
                .Concat(
                    paged
                        .Select(x => (x.Durum ?? string.Empty).Trim())
                        .Where(x => x != ""))
                .Concat(seciliEkstraDurumlar)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (durumListesi.Count == 0)
            {
                durumListesi = ["Beklemede", "Aktif", "Tamamlandı"];
            }

           
            var model = new JiraBoardViewModel
            {
                Q = q,
                SelectedTaskId = selectedTaskId,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Beklemede = paged.Where(t => (t.Durum ?? "").Trim().Equals("Beklemede", StringComparison.OrdinalIgnoreCase)).ToList(),
                Aktif = paged.Where(t => (t.Durum ?? "").Trim().Equals("Aktif", StringComparison.OrdinalIgnoreCase)).ToList(),
                Tamamlandi = paged.Where(t => (t.Durum ?? "").Trim().Equals("Tamamlandı", StringComparison.OrdinalIgnoreCase)).ToList(),
                MusteriSecenekleri = musteriList,
                TalepAcanSecenekleri = talepAcanList,
                TakipEdenSecenekleri = operasyonKullaniciList
            };

            if (selectedTaskId.HasValue && selectedTaskId.Value > 0)
            {
                model.SelectedTask = _context.JiraTasks
                    .AsNoTracking()
                    .Include(x => x.Yorumlar)
                    .FirstOrDefault(x => x.Id == selectedTaskId.Value);
            }

            ViewBag.KullaniciSecenekleri = operasyonKullaniciList;
            ViewBag.KisiSecenekleri = operasyonKullaniciList;
            ViewBag.SelectedKisi = kisi;
            ViewBag.DurumListesi = durumListesi;
            ViewBag.AnaDurumlar = anaDurumlar;
            ViewBag.SelectedEkstraDurumlar = seciliEkstraDurumlar;
            ViewBag.PagedTasks = paged;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(JiraTask model)
        {
            if (model == null) return RedirectToAction(nameof(Index));

            model.JiraId = (model.JiraId ?? string.Empty).Trim();
            model.TalepKonusu = (model.TalepKonusu ?? "").Trim();
            model.TalepTuru = (model.TalepTuru ?? "").Trim();
            model.TalepAcan = (model.TalepAcan ?? "").Trim();
            model.Durum = string.IsNullOrWhiteSpace(model.Durum) ? "Beklemede" : model.Durum.Trim();
            model.TakipEden = (model.TakipEden ?? "").Trim();
            model.MusteriAdi = (model.MusteriAdi ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(model.TalepTuru) && !AllowedTalepTurleri.Contains(model.TalepTuru, StringComparer.Ordinal))
            {
                TempData["JiraError"] = "Geçersiz iş tipi seçildi.";
                return RedirectToAction(nameof(Index));
            }

            if (model.MusteriID.HasValue && model.MusteriID > 0)
            {
                var musteri = _context.Musteriler
                    .AsNoTracking()
                    .FirstOrDefault(x => x.MusteriID == model.MusteriID.Value);

                if (musteri == null)
                {
                    TempData["JiraError"] = "Seçilen müşteri bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                model.MusteriAdi = (musteri.Firma ?? "").Trim();
            }
            else
            {
                model.MusteriID = null;
                model.MusteriAdi = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(model.TalepKonusu))
            {
                TempData["JiraError"] = "Talep Konusu zorunludur.";
                return RedirectToAction(nameof(Index));
            }

            model.OlusturmaTarihi = DateTime.Now;

            _context.JiraTasks.Add(model);
            _context.SaveChanges();

            TempData["JiraOk"] = "Görev eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddYorum(int jiraTaskId, string yorum, string ekleyen)
        {
            yorum = (yorum ?? "").Trim();
            ekleyen = string.IsNullOrWhiteSpace(ekleyen) ? "Sistem" : ekleyen.Trim();

            if (string.IsNullOrWhiteSpace(yorum))
            {
                TempData["JiraError"] = "Yorum boş olamaz.";
                return RedirectToAction(nameof(Index));
            }

            var task = _context.JiraTasks.FirstOrDefault(t => t.Id == jiraTaskId);
            if (task == null)
            {
                TempData["JiraError"] = "Görev bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            AddYorumVeMusteriDetay(task, yorum, ekleyen);
            TempData["JiraOk"] = "Yorum eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddYorumForm(int jiraTaskId, string yorum, string ekleyen, string? returnUrl)
        {
            yorum = (yorum ?? "").Trim();
            ekleyen = string.IsNullOrWhiteSpace(ekleyen) ? "Sistem" : ekleyen.Trim();

            if (string.IsNullOrWhiteSpace(yorum))
            {
                TempData["JiraError"] = "Yorum boş olamaz.";
                return RedirectToLocalOrIndex(returnUrl);
            }

            var task = _context.JiraTasks.FirstOrDefault(t => t.Id == jiraTaskId);
            if (task == null)
            {
                TempData["JiraError"] = "Görev bulunamadı.";
                return RedirectToLocalOrIndex(returnUrl);
            }

            AddYorumVeMusteriDetay(task, yorum, ekleyen);
            TempData["JiraOk"] = "Yorum eklendi.";
            var redirectUrl = RemoveQueryParametersFromLocalUrl(returnUrl, "selectedTaskId", "focusDetail");
            return RedirectToLocalOrIndex(redirectUrl);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operasyon")]
        [ValidateAntiForgeryToken]
        public IActionResult AssignForm(int id, string? takipEden, string? returnUrl)
        {
            if (id <= 0)
            {
                TempData["JiraError"] = "Geçersiz kayıt.";
                return RedirectToLocalOrIndex(returnUrl);
            }

            var task = _context.JiraTasks.Find(id);
            if (task == null)
            {
                TempData["JiraError"] = "Kayıt bulunamadı.";
                return RedirectToLocalOrIndex(returnUrl);
            }

            task.TakipEden = (takipEden ?? string.Empty).Trim();
            _context.SaveChanges();
            TempData["JiraOk"] = "Takip eden güncellendi.";
            return RedirectToLocalOrIndex(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDurumForm(int id, string? yeniDurum, string? returnUrl)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(yeniDurum))
            {
                TempData["JiraError"] = "Geçersiz durum güncellemesi.";
                return RedirectToLocalOrIndex(returnUrl);
            }

            var task = _context.JiraTasks.Find(id);
            if (task == null)
            {
                TempData["JiraError"] = "Kayıt bulunamadı.";
                return RedirectToLocalOrIndex(returnUrl);
            }

            task.Durum = yeniDurum.Trim();
            _context.SaveChanges();
            TempData["JiraOk"] = "Durum güncellendi.";
            return RedirectToLocalOrIndex(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteForm(int id, string? returnUrl)
        {
            var task = _context.JiraTasks
                .Include(t => t.Yorumlar)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
            {
                TempData["JiraError"] = "Kayıt bulunamadı.";
                return RedirectToLocalOrIndex(returnUrl);
            }

            if (task.Yorumlar != null && task.Yorumlar.Any())
                _context.JiraYorumlar.RemoveRange(task.Yorumlar);

            _context.JiraTasks.Remove(task);
            _context.SaveChanges();
            TempData["JiraOk"] = "Kayıt silindi.";
            return RedirectToLocalOrIndex(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddYorumJson([FromBody] AddYorumJsonModel model)
        {
            if (model == null || model.JiraTaskId <= 0)
                return Json(new { success = false, message = "Geçersiz model" });

            var yorum = (model.Yorum ?? "").Trim();
            var ekleyen = string.IsNullOrWhiteSpace(model.Ekleyen) ? "Sistem" : model.Ekleyen.Trim();

            if (string.IsNullOrWhiteSpace(yorum))
                return Json(new { success = false, message = "Yorum boş olamaz." });

            var task = _context.JiraTasks.FirstOrDefault(t => t.Id == model.JiraTaskId);
            if (task == null)
                return Json(new { success = false, message = "Görev bulunamadı." });

            AddYorumVeMusteriDetay(task, yorum, ekleyen);
            return Json(new { success = true });
        }

        public class AddYorumJsonModel
        {
            public int JiraTaskId { get; set; }
            public string? Yorum { get; set; }
            public string? Ekleyen { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UpdateDurum([FromBody] UpdateDurumModel model)
        {
            try
            {
                if (model == null)
                    return Json(new { success = false, message = "Model is null" });

                if (model.Id <= 0)
                    return Json(new { success = false, message = "Invalid ID" });

                if (string.IsNullOrWhiteSpace(model.YeniDurum))
                    return Json(new { success = false, message = "Empty status" });

                var task = _context.JiraTasks.Find(model.Id);
                if (task == null)
                    return Json(new { success = false, message = "Task not found" });

                task.Durum = model.YeniDurum.Trim();
                _context.SaveChanges();

                return Json(new { success = true, message = "Status updated" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Delete([FromBody] DeleteModel model)
        {
            var task = _context.JiraTasks
                .Include(t => t.Yorumlar)
                .FirstOrDefault(t => t.Id == model.Id);

            if (task == null)
                return Json(new { success = false, message = "Kayıt bulunamadı." });

            if (task.Yorumlar != null && task.Yorumlar.Any())
                _context.JiraYorumlar.RemoveRange(task.Yorumlar);

            _context.JiraTasks.Remove(task);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operasyon")]
        [ValidateAntiForgeryToken]
        public JsonResult Assign([FromBody] AssignModel model)
        {
            if (model == null || model.Id <= 0)
                return Json(new { success = false, message = "Geçersiz model" });

            var task = _context.JiraTasks.Find(model.Id);
            if (task == null)
                return Json(new { success = false, message = "Kayıt bulunamadı" });

            task.TakipEden = (model.TakipEden ?? "").Trim();
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public class AssignModel
        {
            public int Id { get; set; }
            public string? TakipEden { get; set; }
        }

        [HttpGet]
        public IActionResult DetailCard(int id)
        {
            var task = _context.JiraTasks
                .Include(t => t.Yorumlar)
                .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return Content("<div class='text-danger small'>Kayıt bulunamadı.</div>", "text/html");

            ViewBag.KullaniciSecenekleri = GetOperasyonKullaniciSecenekleri();
            return PartialView("~/Views/Jira/_JiraDetailCard.cshtml", task);
        }

        [HttpGet]
        public IActionResult ExportExcelGrouped()
        {
            var query = _context.JiraTasks
                .AsNoTracking()
                .Where(x => !ExcelDisiTalepTurleri.Contains((x.TalepTuru ?? string.Empty).Trim()))
                .AsQueryable();

            var rows = BuildExportTaskRows(query, true);
            return CreateTaskExcelFile(rows, "IsTakip", $"IsTakip_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        [HttpGet]
        public IActionResult ExportExcelTumAylar()
        {
            var query = _context.JiraTasks
                .AsNoTracking()
                .Where(x => !ExcelDisiTalepTurleri.Contains((x.TalepTuru ?? string.Empty).Trim()))
                .AsQueryable();

            var rows = BuildExportTaskRows(query, false);
            return CreateTaskExcelFile(rows, "IsTakipTumAylar", $"IsTakip_TumAylar_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
        }

        private List<ExportTaskRow> BuildExportTaskRows(IQueryable<JiraTask> query, bool orderByJiraId)
        {
            var yorumLookup = _context.JiraYorumlar
                .AsNoTracking()
                .OrderBy(y => y.Tarih)
                .Select(y => new
                {
                    y.JiraTaskId,
                    y.Ekleyen,
                    y.YorumMetni,
                    y.Tarih
                })
                .ToList()
                .GroupBy(y => y.JiraTaskId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(Environment.NewLine + Environment.NewLine,
                        g.Select(y =>
                            $"{y.Tarih:dd.MM.yyyy HH:mm} - {(y.Ekleyen ?? "").Trim()}" +
                            (string.IsNullOrWhiteSpace(y.YorumMetni) ? string.Empty : Environment.NewLine + y.YorumMetni.Trim()))));

            var rows = query
                .Select(x => new ExportTaskRow
                {
                    Id = x.Id,
                    MusteriAdi = x.MusteriAdi,
                    JiraId = x.JiraId,
                    TalepKonusu = x.TalepKonusu,
                    TalepTuru = x.TalepTuru,
                    TalepAcan = x.TalepAcan,
                    TakipEden = x.TakipEden,
                    Durum = x.Durum,
                    OlusturmaTarihi = x.OlusturmaTarihi
                })
                .ToList();

            var orderedRows = orderByJiraId
                ? rows.OrderBy(x => x.JiraId ?? string.Empty, new JiraIdNaturalComparer()).ThenByDescending(x => x.OlusturmaTarihi)
                : rows.OrderByDescending(x => x.OlusturmaTarihi).ThenBy(x => x.JiraId ?? string.Empty, new JiraIdNaturalComparer());

            return orderedRows
                .Select(x =>
                {
                    x.Yorumlar = yorumLookup.TryGetValue(x.Id, out var yorumlar) ? yorumlar : string.Empty;
                    return x;
                })
                .ToList();
        }

        private IActionResult CreateTaskExcelFile(List<ExportTaskRow> rows, string sheetName, string fileName)
        {
            using var ms = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateStylesheet();
                stylesPart.Stylesheet.Save();

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                uint sheetId = 1;

                var isTakipPart = workbookPart.AddNewPart<WorksheetPart>();
                var isTakipSheetData = new SheetData();
                var isTakipWorksheet = new Worksheet();

                var isTakipColumns = new Columns(
                    CreateColumn(1, 1, 24),  // Müşteri
                    CreateColumn(2, 2, 48),  // Talep Konusu
                    CreateColumn(3, 3, 16),  // Talep Türü
                    CreateColumn(4, 4, 20),  // Talep Açan
                    CreateColumn(5, 5, 20),  // Takip Eden
                    CreateColumn(6, 6, 16),  // Durum
                    CreateColumn(7, 7, 20),  // Oluşturma Tarihi
                    CreateColumn(8, 8, 60)   // Yorumlar
                );

                isTakipWorksheet.Append(isTakipColumns);
                isTakipWorksheet.Append(isTakipSheetData);

                BuildTaskSheet(isTakipSheetData, isTakipWorksheet, rows);

                isTakipPart.Worksheet = isTakipWorksheet;
                isTakipPart.Worksheet.Save();

                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(isTakipPart),
                    SheetId = sheetId++,
                    Name = sheetName
                });

                workbookPart.Workbook.Save();
            }

            ms.Position = 0;

            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        [HttpGet]
        public IActionResult ExportExcelDisiIsler(string? q)
        {
            q = (q ?? string.Empty).Trim();

            var query = _context.JiraTasks
                .AsNoTracking()
                .Where(x => ExcelDisiTalepTurleri.Contains((x.TalepTuru ?? string.Empty).Trim()))
                .AsQueryable();

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

            var yorumLookup = _context.JiraYorumlar
                .AsNoTracking()
                .OrderBy(y => y.Tarih)
                .Select(y => new
                {
                    y.JiraTaskId,
                    y.Ekleyen,
                    y.YorumMetni,
                    y.Tarih
                })
                .ToList()
                .GroupBy(y => y.JiraTaskId)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(Environment.NewLine + Environment.NewLine,
                        g.Select(y =>
                            $"{y.Tarih:dd.MM.yyyy HH:mm} - {(y.Ekleyen ?? string.Empty).Trim()}" +
                            (string.IsNullOrWhiteSpace(y.YorumMetni) ? string.Empty : Environment.NewLine + y.YorumMetni.Trim()))));

            var rows = query
                .Select(x => new ExportTaskRow
                {
                    Id = x.Id,
                    MusteriAdi = x.MusteriAdi,
                    JiraId = x.JiraId,
                    TalepKonusu = x.TalepKonusu,
                    TalepTuru = x.TalepTuru,
                    TalepAcan = x.TalepAcan,
                    TakipEden = x.TakipEden,
                    Durum = x.Durum,
                    OlusturmaTarihi = x.OlusturmaTarihi
                })
                .ToList()
                .OrderBy(x => x.JiraId ?? string.Empty, new JiraIdNaturalComparer())
                .ThenByDescending(x => x.OlusturmaTarihi)
                .Select(x =>
                {
                    x.Yorumlar = yorumLookup.TryGetValue(x.Id, out var yorumlar) ? yorumlar : string.Empty;
                    return x;
                })
                .ToList();

            using var ms = new MemoryStream();

            using (var document = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                var workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                stylesPart.Stylesheet = CreateStylesheet();
                stylesPart.Stylesheet.Save();

                var sheets = workbookPart.Workbook.AppendChild(new Sheets());

                uint sheetId = 1;

                var isTakipPart = workbookPart.AddNewPart<WorksheetPart>();
                var isTakipSheetData = new SheetData();
                var isTakipWorksheet = new Worksheet();

                var isTakipColumns = new Columns(
                    CreateColumn(1, 1, 24),
                    CreateColumn(2, 2, 48),
                    CreateColumn(3, 3, 16),
                    CreateColumn(4, 4, 20),
                    CreateColumn(5, 5, 20),
                    CreateColumn(6, 6, 16),
                    CreateColumn(7, 7, 20),
                    CreateColumn(8, 8, 60)
                );

                isTakipWorksheet.Append(isTakipColumns);
                isTakipWorksheet.Append(isTakipSheetData);

                BuildTaskSheet(isTakipSheetData, isTakipWorksheet, rows);

                isTakipPart.Worksheet = isTakipWorksheet;
                isTakipPart.Worksheet.Save();

                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(isTakipPart),
                    SheetId = sheetId++,
                    Name = "ExcelDisiIsler"
                });

                workbookPart.Workbook.Save();
            }

            ms.Position = 0;

            var fileName = $"ExcelDisiIsler_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        [HttpGet]
        public IActionResult ExcelDisiIsler(string? q)
        {
            q = (q ?? string.Empty).Trim();

            var query = _context.JiraTasks
                .AsNoTracking()
                .Where(x => ExcelDisiTalepTurleri.Contains((x.TalepTuru ?? string.Empty).Trim()))
                .AsQueryable();

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

            var tasks = query
                .OrderByDescending(x => x.OlusturmaTarihi)
                .ToList();

            ViewBag.Title = "Geliştirme ve Eğitim İşleri";
            ViewBag.Q = q;
            return View(tasks);
        }

        private static void BuildTaskSheet(SheetData sheetData, Worksheet worksheet, List<ExportTaskRow> rows)
        {
            uint currentRow = 1;

            var header = new Row { RowIndex = currentRow };
            header.Append(
                CreateTextCell("A", currentRow, "Müşteri", 1),
                CreateTextCell("B", currentRow, "Talep Konusu", 1),
                CreateTextCell("C", currentRow, "Talep Türü", 1),
                CreateTextCell("D", currentRow, "Talep Açan", 1),
                CreateTextCell("E", currentRow, "Takip Eden", 1),
                CreateTextCell("F", currentRow, "Durum", 1),
                CreateTextCell("G", currentRow, "Oluşturma Tarihi", 1),
                CreateTextCell("H", currentRow, "Yorumlar", 1)
            );
            sheetData.Append(header);

            bool alternate = false;

            foreach (var item in rows)
            {
                currentRow++;
                alternate = !alternate;

                uint style = alternate ? 2u : 3u;

                var row = new Row { RowIndex = currentRow };
                row.Append(
                    CreateTextCell("A", currentRow, item.MusteriAdi ?? "", style),
                    CreateTextCell("B", currentRow, item.TalepKonusu ?? "", style),
                    CreateTextCell("C", currentRow, item.TalepTuru ?? "", style),
                    CreateTextCell("D", currentRow, item.TalepAcan ?? "", style),
                    CreateTextCell("E", currentRow, item.TakipEden ?? "", style),
                    CreateTextCell("F", currentRow, item.Durum ?? "", style),
                    CreateDateCell("G", currentRow, item.OlusturmaTarihi, alternate ? 4u : 5u),
                    CreateTextCell("H", currentRow, item.Yorumlar ?? "", style)
                );
                sheetData.Append(row);
            }

            if (currentRow >= 1)
            {
                var autoFilter = new AutoFilter { Reference = $"A1:H{Math.Max(currentRow, 1)}" };
                var sheetViews = new SheetViews(
                    new SheetView
                    {
                        WorkbookViewId = 0U,
                        Pane = new Pane
                        {
                            VerticalSplit = 1D,
                            TopLeftCell = "A2",
                            ActivePane = PaneValues.BottomLeft,
                            State = PaneStateValues.Frozen
                        }
                    });

                worksheet.InsertAt(sheetViews, 0);
                worksheet.Append(autoFilter);
            }
        }

        private static Stylesheet CreateStylesheet()
        {
            var fonts = new Fonts(
                new Font(
                    new FontSize { Val = 11 }
                ),
                new Font(
                    new Bold(),
                    new FontSize { Val = 11 },
                    new Color { Rgb = HexBinaryValue.FromString("FFFFFFFF") }
                )
            );

            var fills = new Fills(
                new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
                new Fill(new PatternFill(
                    new ForegroundColor { Rgb = HexBinaryValue.FromString("1F4E78") },
                    new BackgroundColor { Indexed = 64U }
                ) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(
                    new ForegroundColor { Rgb = HexBinaryValue.FromString("F8FAFC") },
                    new BackgroundColor { Indexed = 64U }
                ) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(
                    new ForegroundColor { Rgb = HexBinaryValue.FromString("FFFFFF") },
                    new BackgroundColor { Indexed = 64U }
                ) { PatternType = PatternValues.Solid })
            );

            var borders = new Borders(
                new Border(),
                new Border(
                    new LeftBorder { Style = BorderStyleValues.Thin },
                    new RightBorder { Style = BorderStyleValues.Thin },
                    new TopBorder { Style = BorderStyleValues.Thin },
                    new BottomBorder { Style = BorderStyleValues.Thin },
                    new DiagonalBorder()
                )
            );

            var cellFormats = new CellFormats(
                new CellFormat(), // 0 default
                new CellFormat // 1 header
                {
                    FontId = 1,
                    FillId = 2,
                    BorderId = 1,
                    ApplyFill = true,
                    ApplyFont = true,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Horizontal = HorizontalAlignmentValues.Center,
                        Vertical = VerticalAlignmentValues.Center,
                        WrapText = true
                    },
                    ApplyAlignment = true
                },
                new CellFormat // 2 alt row
                {
                    FontId = 0,
                    FillId = 3,
                    BorderId = 1,
                    ApplyFill = true,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Vertical = VerticalAlignmentValues.Center,
                        WrapText = true
                    },
                    ApplyAlignment = true
                },
                new CellFormat // 3 normal row
                {
                    FontId = 0,
                    FillId = 4,
                    BorderId = 1,
                    ApplyFill = true,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Vertical = VerticalAlignmentValues.Center,
                        WrapText = true
                    },
                    ApplyAlignment = true
                },
                new CellFormat // 4 alt row date
                {
                    FontId = 0,
                    FillId = 3,
                    BorderId = 1,
                    ApplyFill = true,
                    ApplyBorder = true,
                    NumberFormatId = 22,
                    ApplyNumberFormat = true,
                    Alignment = new Alignment
                    {
                        Vertical = VerticalAlignmentValues.Center
                    },
                    ApplyAlignment = true
                },
                new CellFormat // 5 normal row date
                {
                    FontId = 0,
                    FillId = 4,
                    BorderId = 1,
                    ApplyFill = true,
                    ApplyBorder = true,
                    NumberFormatId = 22,
                    ApplyNumberFormat = true,
                    Alignment = new Alignment
                    {
                        Vertical = VerticalAlignmentValues.Center
                    },
                    ApplyAlignment = true
                }
            );

            return new Stylesheet(fonts, fills, borders, cellFormats);
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

        private static Cell CreateTextCell(string columnName, uint rowIndex, string text, uint styleIndex)
        {
            return new Cell
            {
                CellReference = columnName + rowIndex,
                DataType = CellValues.InlineString,
                StyleIndex = styleIndex,
                InlineString = new InlineString(new Text(text ?? "") { Space = SpaceProcessingModeValues.Preserve })
            };
        }

        private static Cell CreateNumberCell(string columnName, uint rowIndex, string numberText, uint styleIndex)
        {
            return new Cell
            {
                CellReference = columnName + rowIndex,
                CellValue = new CellValue(numberText),
                DataType = CellValues.Number,
                StyleIndex = styleIndex
            };
        }

        private static Cell CreateDateCell(string columnName, uint rowIndex, DateTime dateValue, uint styleIndex)
        {
            return new Cell
            {
                CellReference = columnName + rowIndex,
                CellValue = new CellValue(dateValue.ToOADate().ToString(CultureInfo.InvariantCulture)),
                DataType = CellValues.Number,
                StyleIndex = styleIndex
            };
        }

        public class UpdateDurumModel
        {
            public int Id { get; set; }
            public string YeniDurum { get; set; } = "";
        }

        public class DeleteModel
        {
            public int Id { get; set; }
        }

        private sealed class JiraIdNaturalComparer : IComparer<string>
        {
            public int Compare(string? x, string? y)
            {
                x ??= "";
                y ??= "";

                if (string.Equals(x, y, StringComparison.OrdinalIgnoreCase))
                    return 0;

                var px = ParseJira(x);
                var py = ParseJira(y);

                var prefixCompare = string.Compare(px.Prefix, py.Prefix, StringComparison.OrdinalIgnoreCase);
                if (prefixCompare != 0)
                    return prefixCompare;

                var numberCompare = px.Number.CompareTo(py.Number);
                if (numberCompare != 0)
                    return numberCompare;

                return string.Compare(px.Raw, py.Raw, StringComparison.OrdinalIgnoreCase);
            }

            private static (string Prefix, long Number, string Raw) ParseJira(string value)
            {
                var v = value.Trim();

                if (string.IsNullOrWhiteSpace(v))
                    return ("", long.MinValue, value);

                var match = Regex.Match(v, @"^(.*?)-?(\d+)$");
                if (match.Success)
                {
                    var prefix = (match.Groups[1].Value ?? "").Trim();
                    var numText = match.Groups[2].Value;

                    if (long.TryParse(numText, out var number))
                        return (prefix, number, value);
                }

                return (v, long.MaxValue, value);
            }
        }

        private sealed class ExportTaskRow
        {
            public int Id { get; set; }
            public string? MusteriAdi { get; set; }
            public string? JiraId { get; set; }
            public string? TalepKonusu { get; set; }
            public string? TalepTuru { get; set; }
            public string? TalepAcan { get; set; }
            public string? TakipEden { get; set; }
            public string? Durum { get; set; }
            public DateTime OlusturmaTarihi { get; set; }
            public string? Yorumlar { get; set; }
        }

        private sealed class ExportCommentRow
        {
            public int Id { get; set; }
            public int JiraTaskId { get; set; }
            public string? MusteriAdi { get; set; }
            public string? JiraId { get; set; }
            public string? TalepKonusu { get; set; }
            public string? TalepTuru { get; set; }
            public string? TalepAcan { get; set; }
            public string? TakipEden { get; set; }
            public string? Durum { get; set; }
            public string? Ekleyen { get; set; }
            public string? YorumMetni { get; set; }
            public DateTime Tarih { get; set; }
        }

        private sealed class ExportStatusRow
        {
            public string? Durum { get; set; }
            public int KayitSayisi { get; set; }
        }
    }
}
