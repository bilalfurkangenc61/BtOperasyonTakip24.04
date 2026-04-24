using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BtOperasyonTakip.Controllers
{
    [AllowAnonymous]
    public class WelcomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Karşılama Sayfası";
            return View();
        }
    }
}
