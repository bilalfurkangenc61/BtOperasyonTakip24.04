using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using BtOperasyonTakip.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Controllers
{
    [Authorize(Roles =  AppRoles.Admin)]
    public class ParametreController : Controller
    {
        private readonly AppDbContext _context;

        public ParametreController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Operasyon}")]
        public async Task<IActionResult> Index()
        {
            var parametreler = await _context.Parametreler
                .AsNoTracking()
                .OrderBy(p => p.Tur)
                .ThenBy(p => p.ParAdi)
                .ToListAsync();

            var turler = await _context.Parametreler
                .AsNoTracking()
                .Where(p => p.Tur != null && p.Tur != "")
                .Select(p => p.Tur!)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();

            var durumParametreleri = await _context.Parametreler
                .AsNoTracking()
                .Where(p => p.Tur == "Durum" && p.ParAdi != null && p.ParAdi != "")
                .OrderBy(p => p.Id)
                .ToListAsync();

            ViewBag.Turler = turler;
            ViewBag.DurumParametreleri = durumParametreleri;

            return View(parametreler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public IActionResult Index(Parametre model)
        {
            if (ModelState.IsValid)
            {
                model.ParAdi = (model.ParAdi ?? "").Trim();
                model.Tur = (model.Tur ?? "").Trim();

                _context.Parametreler.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(_context.Parametreler.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public IActionResult Delete(int id)
        {
            var param = _context.Parametreler.FirstOrDefault(p => p.Id == id);
            if (param != null)
            {
                _context.Parametreler.Remove(param);
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public IActionResult MoveDurum(int id, string direction)
        {
            direction = (direction ?? string.Empty).Trim().ToLowerInvariant();
            if (id <= 0 || (direction != "up" && direction != "down"))
                return RedirectToAction(nameof(Index));

            var durumlar = _context.Parametreler
                .Where(p => p.Tur == "Durum" && p.ParAdi != null && p.ParAdi != "")
                .OrderBy(p => p.Id)
                .ToList();

            var currentIndex = durumlar.FindIndex(x => x.Id == id);
            if (currentIndex < 0)
                return RedirectToAction(nameof(Index));

            var targetIndex = direction == "up" ? currentIndex - 1 : currentIndex + 1;
            if (targetIndex < 0 || targetIndex >= durumlar.Count)
                return RedirectToAction(nameof(Index));

            var current = durumlar[currentIndex];
            var target = durumlar[targetIndex];

            (current.ParAdi, target.ParAdi) = (target.ParAdi, current.ParAdi);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<IActionResult> TurEkle(string tur)
        {
            tur = (tur ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tur))
                return RedirectToAction(nameof(Index));

            var exists = await _context.Parametreler.AnyAsync(p => p.Tur == tur);
            if (!exists)
            {
                _context.Parametreler.Add(new Parametre { Tur = tur, ParAdi = null });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/Parametre/Durumlar")]
        public async Task<IActionResult> Durumlar()
        {
            var durumlar = await _context.Parametreler
                .AsNoTracking()
                .Where(p => p.Tur == "Durum" && p.ParAdi != null && p.ParAdi != "")
                .OrderBy(p => p.Id)
                .Select(p => p.ParAdi!)
                .ToListAsync();

            return Json(durumlar);
        }
    }
}
