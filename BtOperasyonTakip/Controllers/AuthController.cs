using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using BtOperasyonTakip.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BtOperasyonTakip.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static string ResolveRole(User user)
        {
            if (string.Equals(user.UserName, "admin", StringComparison.OrdinalIgnoreCase))
                return AppRoles.Admin;

            return string.IsNullOrWhiteSpace(user.Role) ? AppRoles.Saha : user.Role;
        }

        private static ClaimsPrincipal CreatePrincipal(User user)
        {
            var role = ResolveRole(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName ?? user.UserName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(AppRoles.Operasyon))
                    return RedirectToAction("Index", "Dashboard");
                if (User.IsInRole(AppRoles.Saha))
                    return RedirectToAction("Index", "Ticket");
                if (User.IsInRole(AppRoles.Uyum))
                    return RedirectToAction("Index", "Uyum");

                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SeedAdmin()
        {
            const string username = "admin";
            const string password = "123456";
            const string email = "admin@local";

            var hash = HashPassword(password);

            var user = _context.Users.FirstOrDefault(x => x.UserName == username);
            if (user == null)
            {
                user = new User
                {
                    UserName = username,
                    FullName = "Admin",
                    Email = email,
                    PasswordHash = hash,
                    CreatedAt = DateTime.Now,
                    Role = AppRoles.Saha
                };

                _context.Users.Add(user);
            }
            else
            {
                user.PasswordHash = hash;
                if (string.IsNullOrWhiteSpace(user.Email))
                    user.Email = email;
            }

            _context.SaveChanges();

            return Content($"OK. Kullanıcı: {username} / Şifre: {password}");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe)
        {
            var hash = HashPassword(password);

            var user = _context.Users.FirstOrDefault(u => u.UserName == username && u.PasswordHash == hash);
            if (user == null)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }

            var role = ResolveRole(user);
            var principal = CreatePrincipal(user);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties
                {
                    IsPersistent = rememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            if (role == AppRoles.Operasyon || role == AppRoles.Admin)
                return RedirectToAction("Index", "Dashboard");
            if (role == AppRoles.Uyum)
                return RedirectToAction("Index", "Uyum");

            return RedirectToAction("Index", "Ticket");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(string fullName, string username, string email, string password, string role)
        {
            if (_context.Users.Any(u => u.UserName == username))
            {
                ViewBag.Error = "Bu kullanıcı adı zaten mevcut.";
                return View();
            }
            else if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Bu e-mail adı zaten mevcut.";
                return View();
            }

            var user = new User
            {
                FullName = fullName,
                UserName = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.Now,
                Role = string.IsNullOrWhiteSpace(role) ? AppRoles.Saha : role
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            await SignInUser(user);

            var resolvedRole = ResolveRole(user);

            if (resolvedRole == AppRoles.Operasyon || resolvedRole == AppRoles.Admin)
                return RedirectToAction("Index", "Home");
            if (resolvedRole == AppRoles.Uyum)
                return RedirectToAction("Index", "Uyum");

            return RedirectToAction("Index", "Ticket");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private async Task SignInUser(User user)
        {
            var principal = CreatePrincipal(user);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });
        }
    }
}