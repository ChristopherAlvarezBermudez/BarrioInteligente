using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Net.Mail;
using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Models;
using BarrioInteligenteWeb.Services;

namespace BarrioInteligenteWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment env, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _env = env;
            _emailService = emailService;
            _config = config;
        }

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Reportes");

            if (TempData["LoginMessage"] != null)
            {
                ViewBag.LoginMessage = TempData["LoginMessage"].ToString();
            }

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

        // GET: /Account/EditarPerfil
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult EditarPerfil()
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

            if (usuario.FechaEliminacionProgramada.HasValue)
            {
                if (usuario.FechaEliminacionProgramada.Value <= DateTime.Now)
                {
                    if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                    {
                        var oldFilePath = Path.Combine(_env.WebRootPath, usuario.FotoPerfil.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }
                    _context.Usuarios.Remove(usuario);
                    await _context.SaveChangesAsync();
                    
                    ViewBag.Error = "Esta cuenta ya no existe.";
                    return View();
                }
                else
                {
                    TempData["UsuarioIdReactivar"] = usuario.Id;
                    return RedirectToAction("ReactivarCuenta");
                }
            }

            if (!usuario.EmailConfirmado)
            {
                TempData["CorreoAConfirmar"] = usuario.Correo;
                return RedirectToAction("VerificarEmail");
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
        public async Task<IActionResult> Registro(string nombreCompleto, string correo, string password, string confirmPassword)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            if (_context.Usuarios.Any(u => u.Correo == correo))
            {
                ViewBag.Error = "Ya existe una cuenta con ese correo.";
                return View();
            }

            var codigo = new Random().Next(100000, 999999).ToString();
            var usuario = new Usuario
            {
                NombreCompleto = nombreCompleto,
                Correo = correo,
                Password = HashPassword(password),
                FechaRegistro = DateTime.Now,
                EmailConfirmado = false,
                CodigoVerificacion = codigo
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var asunto = "Verifica tu cuenta - Barrio Inteligente";
            var cuerpo = $"<h3>¡Hola {nombreCompleto}!</h3><p>Tu código de verificación es: <b>{codigo}</b></p>";
            await _emailService.EnviarAsync(correo, asunto, cuerpo);

            TempData["CorreoAConfirmar"] = correo;
            return RedirectToAction("VerificarEmail");
        }

        // GET: /Account/VerificarEmail
        public IActionResult VerificarEmail(string? correo = null)
        {
            var correoTemp = TempData["CorreoAConfirmar"]?.ToString() ?? correo;
            if (string.IsNullOrEmpty(correoTemp))
                return RedirectToAction("Login");

            ViewBag.Correo = correoTemp;
            return View();
        }

        // POST: /Account/VerificarEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarEmail(string correo, string codigo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null) return RedirectToAction("Login");

            if (usuario.CodigoVerificacion == codigo)
            {
                usuario.EmailConfirmado = true;
                usuario.CodigoVerificacion = null;
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

            ViewBag.Error = "Código incorrecto. Intenta de nuevo.";
            ViewBag.Correo = correo;
            return View();
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

                if (!string.IsNullOrEmpty(usuario.FotoPerfil))
                {
                    var oldFilePath = Path.Combine(_env.WebRootPath, usuario.FotoPerfil.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
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

        // POST: /Account/ProgramarEliminacion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ProgramarEliminacion(string passwordActual)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return RedirectToAction("Login");

            if (HashPassword(passwordActual) != usuario.Password)
            {
                ViewBag.Error = "La contraseña actual es incorrecta.";
                return View("Perfil", usuario);
            }

            usuario.FechaEliminacionProgramada = DateTime.Now.AddHours(24);
            await _context.SaveChangesAsync();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["LoginMessage"] = "Tu cuenta ha sido programada para eliminación y dejará de existir en 24 horas.";
            return RedirectToAction("Login", "Account");
        }

        // GET: /Account/ReactivarCuenta
        public IActionResult ReactivarCuenta()
        {
            if (TempData["UsuarioIdReactivar"] == null)
            {
                return RedirectToAction("Login");
            }
            
            var id = (int)TempData["UsuarioIdReactivar"];
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);

            if (usuario == null || !usuario.FechaEliminacionProgramada.HasValue)
            {
                return RedirectToAction("Login");
            }

            ViewBag.FechaEliminacion = usuario.FechaEliminacionProgramada.Value.ToString("o");
            ViewBag.UsuarioId = id;
            TempData.Keep("UsuarioIdReactivar");
            
            return View();
        }

        // POST: /Account/ReactivarCuenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivarCuenta(int usuarioId, string password)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
            if (usuario == null) return RedirectToAction("Login");

            if (HashPassword(password) != usuario.Password)
            {
                ViewBag.Error = "Contraseña incorrecta. Intenta de nuevo.";
                ViewBag.FechaEliminacion = usuario.FechaEliminacionProgramada?.ToString("o");
                ViewBag.UsuarioId = usuarioId;
                TempData.Keep("UsuarioIdReactivar");
                return View();
            }

            usuario.FechaEliminacionProgramada = null;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Correo)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return RedirectToAction("Perfil", "Account");
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // POST: /Account/SolicitarEdicionPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> SolicitarEdicionPerfil(string nuevoNombre, string nuevoCorreo, string? nuevaPassword, string? confirmarNuevaPassword, string passwordActual)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return RedirectToAction("Login");

            if (HashPassword(passwordActual) != usuario.Password)
            {
                ViewBag.Error = "La contraseña actual es incorrecta.";
                return View("Perfil", usuario);
            }

            if (!string.IsNullOrEmpty(nuevaPassword) && nuevaPassword != confirmarNuevaPassword)
            {
                ViewBag.Error = "Las nuevas contraseñas no coinciden.";
                return View("Perfil", usuario);
            }

            var codigo = new Random().Next(100000, 999999).ToString();
            usuario.CodigoVerificacion = codigo;
            await _context.SaveChangesAsync();

            var asunto = "Confirma los cambios en tu perfil - Barrio Inteligente";
            var cuerpo = $"<h3>¡Hola {usuario.NombreCompleto}!</h3><p>Alguien intentó modificar tu perfil. Tu código de autorización es: <b>{codigo}</b></p>";
            await _emailService.EnviarAsync(usuario.Correo, asunto, cuerpo);

            TempData["EditNombre"] = nuevoNombre;
            TempData["EditCorreo"] = nuevoCorreo;
            if (!string.IsNullOrEmpty(nuevaPassword)) TempData["EditPassword"] = nuevaPassword;

            return RedirectToAction("ConfirmarEdicion");
        }

        // GET: /Account/ConfirmarEdicion
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult ConfirmarEdicion()
        {
            var nombre = TempData["EditNombre"]?.ToString();
            var correo = TempData["EditCorreo"]?.ToString();

            if (nombre == null || correo == null)
            {
                return RedirectToAction("Perfil"); 
            }

            ViewBag.EditNombre = nombre;
            ViewBag.EditCorreo = correo;
            ViewBag.EditPassword = TempData["EditPassword"]?.ToString();

            // Keep TempData valid for the POST request
            TempData.Keep("EditNombre");
            TempData.Keep("EditCorreo");
            TempData.Keep("EditPassword");

            return View();
        }

        // POST: /Account/ConfirmarEdicion
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> ConfirmarEdicion(string codigo, string editNombre, string editCorreo, string? editPassword)
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return RedirectToAction("Login");

            if (usuario.CodigoVerificacion != codigo)
            {
                ViewBag.Error = "El código ingresado es incorrecto.";
                ViewBag.EditNombre = editNombre;
                ViewBag.EditCorreo = editCorreo;
                ViewBag.EditPassword = editPassword;
                return View();
            }

            bool requireRelogin = false;

            if (usuario.Correo != editCorreo)
            {
                usuario.Correo = editCorreo;
                requireRelogin = true;
            }

            if (!string.IsNullOrEmpty(editPassword))
            {
                usuario.Password = HashPassword(editPassword);
                requireRelogin = true;
            }

            usuario.NombreCompleto = editNombre;
            usuario.CodigoVerificacion = null;
            await _context.SaveChangesAsync();

            if (requireRelogin)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["LoginMessage"] = "Tus credenciales cambiaron. Por favor, inicia sesión de nuevo.";
                return RedirectToAction("Login");
            }
            else
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                    new Claim(ClaimTypes.Email, usuario.Correo)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Perfil");
            }
        }

        // ─── RECUPERACIÓN DE CONTRASEÑA ────────────────────────────────────────

        // GET: /Account/OlvidePassword
        public IActionResult OlvidePassword()
        {
            return View();
        }

        // POST: /Account/OlvidePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OlvidePassword(string correo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null)
            {
                ViewBag.Error = "No existe una cuenta con ese correo.";
                return View();
            }

            var codigo = new Random().Next(100000, 999999).ToString();
            usuario.CodigoRecuperacion = codigo;
            usuario.ExpiracionCodigo = DateTime.Now.AddMinutes(15);
            await _context.SaveChangesAsync();

            EnviarCorreoRecuperacion(correo, codigo);

            TempData["CorreoRecuperacion"] = correo;
            return RedirectToAction("VerificarCodigo");
        }

        // GET: /Account/VerificarCodigo
        public IActionResult VerificarCodigo()
        {
            var correo = TempData["CorreoRecuperacion"]?.ToString();
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("OlvidePassword");

            ViewBag.Correo = correo;
            TempData.Keep("CorreoRecuperacion");
            return View();
        }

        // POST: /Account/VerificarCodigo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerificarCodigo(string correo, string codigo)
        {
            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null ||
                usuario.CodigoRecuperacion != codigo ||
                usuario.ExpiracionCodigo == null ||
                usuario.ExpiracionCodigo <= DateTime.Now)
            {
                ViewBag.Error = "El código es incorrecto o ha expirado.";
                ViewBag.Correo = correo;
                TempData["CorreoRecuperacion"] = correo;
                TempData.Keep("CorreoRecuperacion");
                return View();
            }

            TempData["CorreoRestablecimiento"] = correo;
            return RedirectToAction("RestablecerPassword");
        }

        // GET: /Account/RestablecerPassword
        public IActionResult RestablecerPassword()
        {
            var correo = TempData["CorreoRestablecimiento"]?.ToString();
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("OlvidePassword");

            ViewBag.Correo = correo;
            TempData.Keep("CorreoRestablecimiento");
            return View();
        }

        // POST: /Account/RestablecerPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestablecerPassword(string correo, string nuevaPassword, string confirmarPassword)
        {
            if (nuevaPassword != confirmarPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                ViewBag.Correo = correo;
                TempData["CorreoRestablecimiento"] = correo;
                TempData.Keep("CorreoRestablecimiento");
                return View();
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Correo == correo);
            if (usuario == null)
                return RedirectToAction("OlvidePassword");

            usuario.Password = HashPassword(nuevaPassword);
            usuario.CodigoRecuperacion = null;
            usuario.ExpiracionCodigo = null;
            await _context.SaveChangesAsync();

            TempData["LoginMessage"] = "¡Contraseña restablecida con éxito! Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }

        private void EnviarCorreoRecuperacion(string destino, string codigo)
        {
            var from = _config["EmailSettings:From"];
            var password = _config["EmailSettings:Password"];

            var mensaje = new MailMessage();
            mensaje.From = new MailAddress(from!);
            mensaje.To.Add(destino);
            mensaje.Subject = "Recuperación de contraseña - Barrio Inteligente";
            mensaje.IsBodyHtml = true;
            mensaje.Body = $"<h3>Recupera tu contraseña</h3><p>Tu código de verificación es: <b>{codigo}</b></p><p>Este código expira en 15 minutos.</p>";

            using var smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential(from, password);
            smtp.EnableSsl = true;
            smtp.Send(mensaje);
        }

        // ──────────────────────────────────────────────────────────────────────

        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "BarrioInteligenteSalt2026");
            return Convert.ToHexString(sha.ComputeHash(bytes));
        }
    }
}
