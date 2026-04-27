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
using System.Text.RegularExpressions;

namespace BtOperasyonTakip.Controllers
{
    [Authorize]
    public class TakipController : Controller
    {
        private readonly AppDbContext _context;

        private static readonly string[] AllowedTalepTurleri = ["Geliştirme", "Eğitim", "Entegrasyon", "Hata Çözüm"];
        private const int DefaultPageSize = 10;
        private const int MaxPageSize = 100;

        public TakipController(AppDbContext context) => _context = context;

        private IActionResult RedirectToLocalOrIndex(string? returnUrl, object? routeValues = null)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Index), routeValues);
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
        public IActionResult Index(string? q, int? selectedTaskId = null, int page = 1, int pageSize = DefaultPageSize)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize < 1 ? DefaultPageSize : pageSize;
            pageSize = pageSize > MaxPageSize ? MaxPageSize : pageSize;

            var talepAcanList = GetTalepAcanSecenekleri();
            var operasyonKullaniciList = GetOperasyonKullaniciSecenekleri();

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

            query = query.OrderByDescending(x => x.OlusturmaTarihi);

            var totalCount = query.Count();

            var paged = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

           
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
            return RedirectToLocalOrIndex(returnUrl);
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
        public IActionResult ExportExcelGrouped(string? q)
        {
            var query = _context.JiraTasks
                .AsNoTracking()
                .AsQueryable();

            q = (q ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.ToLowerInvariant();
                query = query.Where(t =>
                    (t.MusteriAdi ?? "").ToLower().Contains(qq) ||
                    (t.TalepKonusu ?? "").ToLower().Contains(qq) ||
                    (t.TalepTuru ?? "").ToLower().Contains(qq) ||
                    (t.TalepAcan ?? "").ToLower().Contains(qq) ||
                    (t.TakipEden ?? "").ToLower().Contains(qq) ||
                    (t.Durum ?? "").ToLower().Contains(qq));
            }

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
                .OrderBy(x => x.JiraId ?? "", new JiraIdNaturalComparer())
                .ThenByDescending(x => x.OlusturmaTarihi)
                .ThenBy(x => x.Id)
                .ToList();

            var yorumQuery = _context.JiraYorumlar
                .AsNoTracking()
                .Include(y => y.JiraTask)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qq = q.ToLowerInvariant();
                yorumQuery = yorumQuery.Where(y =>
                    (y.JiraTask.MusteriAdi ?? "").ToLower().Contains(qq) ||
                    (y.JiraTask.JiraId ?? "").ToLower().Contains(qq) ||
                    (y.JiraTask.TalepKonusu ?? "").ToLower().Contains(qq) ||
                    (y.JiraTask.TalepTuru ?? "").ToLower().Contains(qq) ||
                    (y.JiraTask.TalepAcan ?? "").ToLower().Contains(qq) ||
                    (y.JiraTask.TakipEden ?? "").ToLower().Contains(qq) ||
                    (y.JiraTask.Durum ?? "").ToLower().Contains(qq) ||
                    (y.YorumMetni ?? "").ToLower().Contains(qq) ||
                    (y.Ekleyen ?? "").ToLower().Contains(qq));
            }

            var yorumRows = yorumQuery
                .Select(y => new ExportCommentRow
                {
                    Id = y.Id,
                    JiraTaskId = y.JiraTaskId,
                    MusteriAdi = y.JiraTask.MusteriAdi,
                    JiraId = y.JiraTask.JiraId,
                    TalepKonusu = y.JiraTask.TalepKonusu,
                    TalepTuru = y.JiraTask.TalepTuru,
                    TalepAcan = y.JiraTask.TalepAcan,
                    TakipEden = y.JiraTask.TakipEden,
                    Ekleyen = y.Ekleyen,
                    YorumMetni = y.YorumMetni,
                    Tarih = y.Tarih
                })
                .ToList()
                .OrderBy(x => x.JiraId ?? "", new JiraIdNaturalComparer())
                .ThenByDescending(x => x.Tarih)
                .ThenBy(x => x.Id)
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
                    CreateColumn(1, 1, 12),  // Jira Grup
                    CreateColumn(2, 2, 10),  // Id
                    CreateColumn(3, 3, 24),  // Müşteri
                    CreateColumn(4, 4, 16),  // JiraId
                    CreateColumn(5, 5, 48),  // Talep Konusu
                    CreateColumn(6, 6, 16),  // Talep Türü
                    CreateColumn(7, 7, 20),  // Talep Açan
                    CreateColumn(8, 8, 20),  // Takip Eden
                    CreateColumn(9, 9, 16),  // Durum
                    CreateColumn(10, 10, 20) // Oluşturma Tarihi
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
                    Name = "IsTakip"
                });

                var yorumPart = workbookPart.AddNewPart<WorksheetPart>();
                var yorumSheetData = new SheetData();
                var yorumWorksheet = new Worksheet();

                var yorumColumns = new Columns(
                    CreateColumn(1, 1, 12),   // Jira Grup
                    CreateColumn(2, 2, 10),   // Yorum Id
                    CreateColumn(3, 3, 12),   // JiraTaskId
                    CreateColumn(4, 4, 24),   // Müşteri
                    CreateColumn(5, 5, 16),   // JiraId
                    CreateColumn(6, 6, 40),   // Talep Konusu
                    CreateColumn(7, 7, 16),   // Talep Türü
                    CreateColumn(8, 8, 20),   // Talep Açan
                    CreateColumn(9, 9, 20),   // Takip Eden
                    CreateColumn(10, 10, 18), // Ekleyen
                    CreateColumn(11, 11, 55), // Yorum
                    CreateColumn(12, 12, 20)  // Yorum Tarihi
                );

                yorumWorksheet.Append(yorumColumns);
                yorumWorksheet.Append(yorumSheetData);

                BuildCommentSheet(yorumSheetData, yorumWorksheet, yorumRows);

                yorumPart.Worksheet = yorumWorksheet;
                yorumPart.Worksheet.Save();

                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(yorumPart),
                    SheetId = sheetId++,
                    Name = "Yorumlar"
                });

                workbookPart.Workbook.Save();
            }

            ms.Position = 0;

            var fileName = $"IsTakip_JiraId_Grup_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        private static void BuildTaskSheet(SheetData sheetData, Worksheet worksheet, List<ExportTaskRow> rows)
        {
            uint currentRow = 1;

            var header = new Row { RowIndex = currentRow };
            header.Append(
                CreateTextCell("A", currentRow, "Jira Grup", 1),
                CreateTextCell("B", currentRow, "Id", 1),
                CreateTextCell("C", currentRow, "Müşteri", 1),
                CreateTextCell("D", currentRow, "JiraId", 1),
                CreateTextCell("E", currentRow, "Talep Konusu", 1),
                CreateTextCell("F", currentRow, "Talep Türü", 1),
                CreateTextCell("G", currentRow, "Talep Açan", 1),
                CreateTextCell("H", currentRow, "Takip Eden", 1),
                CreateTextCell("I", currentRow, "Durum", 1),
                CreateTextCell("J", currentRow, "Oluşturma Tarihi", 1)
            );
            sheetData.Append(header);

            int groupNo = 0;
            string lastJiraId = "";
            bool alternate = false;

            foreach (var item in rows)
            {
                var jira = (item.JiraId ?? "").Trim();

                if (!string.Equals(lastJiraId, jira, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(lastJiraId))
                    {
                        currentRow++;
                        var spacer = new Row { RowIndex = currentRow };
                        spacer.Append(
                            CreateTextCell("A", currentRow, "", 0),
                            CreateTextCell("B", currentRow, "", 0),
                            CreateTextCell("C", currentRow, "", 0),
                            CreateTextCell("D", currentRow, "", 0),
                            CreateTextCell("E", currentRow, "", 0),
                            CreateTextCell("F", currentRow, "", 0),
                            CreateTextCell("G", currentRow, "", 0),
                            CreateTextCell("H", currentRow, "", 0),
                            CreateTextCell("I", currentRow, "", 0),
                            CreateTextCell("J", currentRow, "", 0)
                        );
                        sheetData.Append(spacer);
                    }

                    groupNo++;
                    alternate = !alternate;
                    lastJiraId = jira;
                }

                currentRow++;

                uint style = alternate ? 2u : 3u;

                var row = new Row { RowIndex = currentRow };
                row.Append(
                    CreateTextCell("A", currentRow, string.IsNullOrWhiteSpace(jira) ? "" : groupNo.ToString(), style),
                    CreateNumberCell("B", currentRow, item.Id.ToString(CultureInfo.InvariantCulture), style),
                    CreateTextCell("C", currentRow, item.MusteriAdi ?? "", style),
                    CreateTextCell("D", currentRow, item.JiraId ?? "", style),
                    CreateTextCell("E", currentRow, item.TalepKonusu ?? "", style),
                    CreateTextCell("F", currentRow, item.TalepTuru ?? "", style),
                    CreateTextCell("G", currentRow, item.TalepAcan ?? "", style),
                    CreateTextCell("H", currentRow, item.TakipEden ?? "", style),
                    CreateTextCell("I", currentRow, item.Durum ?? "", style),
                    CreateDateCell("J", currentRow, item.OlusturmaTarihi, alternate ? 4u : 5u)
                );
                sheetData.Append(row);
            }

            if (currentRow >= 1)
            {
                var autoFilter = new AutoFilter { Reference = $"A1:J{Math.Max(currentRow, 1)}" };
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

        private static void BuildCommentSheet(SheetData sheetData, Worksheet worksheet, List<ExportCommentRow> rows)
        {
            uint currentRow = 1;

            var header = new Row { RowIndex = currentRow };
            header.Append(
                CreateTextCell("A", currentRow, "Jira Grup", 1),
                CreateTextCell("B", currentRow, "Yorum Id", 1),
                CreateTextCell("C", currentRow, "JiraTaskId", 1),
                CreateTextCell("D", currentRow, "Müşteri", 1),
                CreateTextCell("E", currentRow, "JiraId", 1),
                CreateTextCell("F", currentRow, "Talep Konusu", 1),
                CreateTextCell("G", currentRow, "Talep Türü", 1),
                CreateTextCell("H", currentRow, "Talep Açan", 1),
                CreateTextCell("I", currentRow, "Takip Eden", 1),
                CreateTextCell("J", currentRow, "Ekleyen", 1),
                CreateTextCell("K", currentRow, "Yorum", 1),
                CreateTextCell("L", currentRow, "Yorum Tarihi", 1)
            );
            sheetData.Append(header);

            int groupNo = 0;
            string lastJiraId = "";
            bool alternate = false;

            foreach (var item in rows)
            {
                var jira = (item.JiraId ?? "").Trim();

                if (!string.Equals(lastJiraId, jira, StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(lastJiraId))
                    {
                        currentRow++;
                        var spacer = new Row { RowIndex = currentRow };
                        spacer.Append(
                            CreateTextCell("A", currentRow, "", 0),
                            CreateTextCell("B", currentRow, "", 0),
                            CreateTextCell("C", currentRow, "", 0),
                            CreateTextCell("D", currentRow, "", 0),
                            CreateTextCell("E", currentRow, "", 0),
                            CreateTextCell("F", currentRow, "", 0),
                            CreateTextCell("G", currentRow, "", 0),
                            CreateTextCell("H", currentRow, "", 0),
                            CreateTextCell("I", currentRow, "", 0),
                            CreateTextCell("J", currentRow, "", 0),
                            CreateTextCell("K", currentRow, "", 0),
                            CreateTextCell("L", currentRow, "", 0)
                        );
                        sheetData.Append(spacer);
                    }

                    groupNo++;
                    alternate = !alternate;
                    lastJiraId = jira;
                }

                currentRow++;

                uint style = alternate ? 2u : 3u;

                var row = new Row { RowIndex = currentRow };
                row.Append(
                    CreateTextCell("A", currentRow, string.IsNullOrWhiteSpace(jira) ? "" : groupNo.ToString(), style),
                    CreateNumberCell("B", currentRow, item.Id.ToString(CultureInfo.InvariantCulture), style),
                    CreateNumberCell("C", currentRow, item.JiraTaskId.ToString(CultureInfo.InvariantCulture), style),
                    CreateTextCell("D", currentRow, item.MusteriAdi ?? "", style),
                    CreateTextCell("E", currentRow, item.JiraId ?? "", style),
                    CreateTextCell("F", currentRow, item.TalepKonusu ?? "", style),
                    CreateTextCell("G", currentRow, item.TalepTuru ?? "", style),
                    CreateTextCell("H", currentRow, item.TalepAcan ?? "", style),
                    CreateTextCell("I", currentRow, item.TakipEden ?? "", style),
                    CreateTextCell("J", currentRow, item.Ekleyen ?? "", style),
                    CreateTextCell("K", currentRow, item.YorumMetni ?? "", style),
                    CreateDateCell("L", currentRow, item.Tarih, alternate ? 4u : 5u)
                );
                sheetData.Append(row);
            }

            if (currentRow >= 1)
            {
                var autoFilter = new AutoFilter { Reference = $"A1:L{Math.Max(currentRow, 1)}" };
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
            public string? Ekleyen { get; set; }
            public string? YorumMetni { get; set; }
            public DateTime Tarih { get; set; }
        }
    }
}
