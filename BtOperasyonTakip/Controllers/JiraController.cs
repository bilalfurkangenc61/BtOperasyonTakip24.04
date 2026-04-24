using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BtOperasyonTakip.Controllers
{
    public class JiraController : Controller
    {
        private readonly AppDbContext _context;
        public JiraController(AppDbContext context) => _context = context;

        [HttpGet]
        public IActionResult Index(int? selectedTaskId = null)
        {
            var tasks = _context.JiraTasks
                                .Include(x => x.Yorumlar)
                                .OrderByDescending(x => x.OlusturmaTarihi)
                                .ToList();

            var kullaniciList = _context.Users
                .AsNoTracking()
                .Select(x => string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : x.FullName!)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var model = new JiraBoardViewModel
            {
                SelectedTaskId = selectedTaskId,
                Beklemede = tasks.Where(t => (t.Durum ?? "").Trim().Equals("Beklemede", StringComparison.OrdinalIgnoreCase)).ToList(),
                Aktif = tasks.Where(t => (t.Durum ?? "").Trim().Equals("Aktif", StringComparison.OrdinalIgnoreCase)).ToList(),
                Tamamlandi = tasks.Where(t => (t.Durum ?? "").Trim().Equals("Tamamlandı", StringComparison.OrdinalIgnoreCase)).ToList(),
                TakipEdenSecenekleri = kullaniciList,
                TalepAcanSecenekleri = kullaniciList
            };

            if (selectedTaskId.HasValue && selectedTaskId.Value > 0)
            {
                model.SelectedTask = tasks.FirstOrDefault(t => t.Id == selectedTaskId.Value);
            }

            ViewBag.KullaniciSecenekleri = kullaniciList;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(JiraTask model)
        {
            if (model == null) return RedirectToAction(nameof(Index));

            model.JiraId = (model.JiraId ?? "").Trim();
            model.TalepKonusu = (model.TalepKonusu ?? "").Trim();
            model.TalepAcan = (model.TalepAcan ?? "").Trim();
            model.Durum = string.IsNullOrWhiteSpace(model.Durum) ? "Beklemede" : model.Durum.Trim();

            if (string.IsNullOrWhiteSpace(model.JiraId) || string.IsNullOrWhiteSpace(model.TalepKonusu))
            {
                TempData["JiraError"] = "Jira ID ve Talep Konusu zorunludur.";
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

            var taskExists = _context.JiraTasks.Any(t => t.Id == jiraTaskId);
            if (!taskExists)
            {
                TempData["JiraError"] = "Görev bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            _context.JiraYorumlar.Add(new JiraYorum
            {
                JiraTaskId = jiraTaskId,
                YorumMetni = yorum,
                Ekleyen = ekleyen,
                Tarih = DateTime.Now
            });

            _context.SaveChanges();
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
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Index));
            }

            var taskExists = _context.JiraTasks.Any(t => t.Id == jiraTaskId);
            if (!taskExists)
            {
                TempData["JiraError"] = "Görev bulunamadı.";
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Index));
            }

            _context.JiraYorumlar.Add(new JiraYorum
            {
                JiraTaskId = jiraTaskId,
                YorumMetni = yorum,
                Ekleyen = ekleyen,
                Tarih = DateTime.Now
            });

            _context.SaveChanges();
            TempData["JiraOk"] = "Yorum eklendi.";
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignForm(int id, string? takipEden, string? returnUrl)
        {
            var task = _context.JiraTasks.Find(id);
            if (task == null)
            {
                TempData["JiraError"] = "Kayıt bulunamadı.";
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Index));
            }

            task.TakipEden = (takipEden ?? string.Empty).Trim();
            _context.SaveChanges();
            TempData["JiraOk"] = "Takip eden güncellendi.";
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateDurumForm(int id, string? yeniDurum, string? returnUrl)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(yeniDurum))
            {
                TempData["JiraError"] = "Geçersiz durum güncellemesi.";
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Index));
            }

            var task = _context.JiraTasks.Find(id);
            if (task == null)
            {
                TempData["JiraError"] = "Task not found";
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Index));
            }

            task.Durum = yeniDurum.Trim();
            _context.SaveChanges();
            TempData["JiraOk"] = "Durum güncellendi.";
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
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
                return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? LocalRedirect(returnUrl)
                    : RedirectToAction(nameof(Index));
            }

            if (task.Yorumlar != null && task.Yorumlar.Any())
                _context.JiraYorumlar.RemoveRange(task.Yorumlar);

            _context.JiraTasks.Remove(task);
            _context.SaveChanges();

            TempData["JiraOk"] = "Kayıt silindi.";
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public JsonResult UpdateDurum([FromBody] UpdateDurumModel model)
        {
            try
            {
                Console.WriteLine($"🟢 UPDATE DURUM BAŞLADI: ID={model?.Id}, Durum='{model?.YeniDurum}'");

                if (model == null)
                {
                    Console.WriteLine("🔴 Model is null");
                    return Json(new { success = false, message = "Model is null" });
                }

                if (model.Id <= 0)
                {
                    Console.WriteLine($"🔴 Invalid ID: {model.Id}");
                    return Json(new { success = false, message = "Invalid ID" });
                }

                if (string.IsNullOrWhiteSpace(model.YeniDurum))
                {
                    Console.WriteLine("🔴 Empty status");
                    return Json(new { success = false, message = "Empty status" });
                }

                var task = _context.JiraTasks.Find(model.Id);
                if (task == null)
                {
                    Console.WriteLine($"🔴 Task not found: {model.Id}");
                    return Json(new { success = false, message = "Task not found" });
                }

                var oldStatus = task.Durum;
                task.Durum = model.YeniDurum.Trim();
                _context.SaveChanges();

                Console.WriteLine($"🟢 SUCCESS: {task.JiraId} {oldStatus} -> {task.Durum}");
                return Json(new { success = true, message = "Status updated" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔴 EXCEPTION: {ex.Message}");
                Console.WriteLine($"🔴 STACK TRACE: {ex.StackTrace}");
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpPost]
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

        [HttpGet]
        public IActionResult DetailCard(int id)
        {
            var task = _context.JiraTasks
                               .Include(t => t.Yorumlar)
                               .FirstOrDefault(t => t.Id == id);

            if (task == null)
                return Content("<div class='text-danger small'>Kayıt bulunamadı.</div>", "text/html");

            return PartialView("_JiraDetailCard", task);
        }

        public class UpdateDurumModel
        {
            public int Id { get; set; }
            public string YeniDurum { get; set; }
        }

        public class DeleteModel
        {
            public int Id { get; set; }
        }
    }
}