using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Models;

namespace BarrioInteligenteWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Reportes");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // GET: /Account/Perfil
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult Perfil()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return RedirectToAction("Login");
            return View(usuario);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string password, string? returnUrl = null)
        {
            var hash = HashPassword(password);
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == correo && u.Password == hash);

            if (usuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Correo)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Reportes");
        }

        // GET: /Account/Registro
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Reportes");

            return View();
        }

        // POST: /Account/Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(string nombreCompleto, string correo, string password)
        {
            if (_context.Usuarios.Any(u => u.Correo == correo))
            {
                ViewBag.Error = "Ya existe una cuenta con ese correo.";
                return View();
            }

            var usuario = new Usuario
            {
                NombreCompleto = nombreCompleto,
                Correo = correo,
                Password = HashPassword(password),
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Correo)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Reportes");
        }

        // POST: /Account/SubirFoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> SubirFoto(IFormFile foto)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return RedirectToAction("Login");

            if (foto != null && foto.Length > 0)
            {
                var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
                if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext))
                {
                    ViewBag.Error = "Solo se permiten imágenes (.jpg, .png, .webp).";
                    return View("Perfil", usuario);
                }

                var perfilesDir = Path.Combine(_env.WebRootPath, "uploads", "perfiles");
                Directory.CreateDirectory(perfilesDir);

                var fileName = $"perfil_{id}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(perfilesDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await foto.CopyToAsync(stream);

                usuario.FotoPerfil = $"/uploads/perfiles/{fileName}";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Perfil");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "BarrioInteligenteSalt2026");
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }
    }
}
