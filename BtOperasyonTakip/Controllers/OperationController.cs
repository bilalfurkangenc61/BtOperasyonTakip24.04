using BtOperasyonTakip.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Controllers
{
    public class OperationController : Controller
    {
        private readonly AppDbContext _context;

        public OperationController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Issues()
        {
            var issues = await _context.Issues
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
            return View(issues);
        }
    }
}