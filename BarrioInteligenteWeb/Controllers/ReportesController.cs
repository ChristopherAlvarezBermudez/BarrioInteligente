using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Models;

namespace BarrioInteligenteWeb.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            ILogger<ReportesController> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public IActionResult Index()
        {
            try
            {
                var lista = _context.Reportes
                    .OrderByDescending(r => r.Fecha)
                    .ToList();
                ViewBag.UsuarioId = UsuarioActualId;
                return View(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el índice de reportes");
                return View(new List<Reporte>());
            }
        }

        public IActionResult MisReportes()
        {
            try
            {
                var lista = _context.Reportes
                    .Where(r => r.UsuarioId == UsuarioActualId)
                    .OrderByDescending(r => r.Fecha)
                    .ToList();
                return View("MisReportes", lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar Mis Reportes para usuario {UsuarioId}", UsuarioActualId);
                return View("MisReportes", new List<Reporte>());
            }
        }

        public IActionResult Crear() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Reporte reporte, IFormFile? imagen)
        {
            try
            {
                reporte.UsuarioId = UsuarioActualId;
                reporte.Fecha = DateTime.Now;

                if (imagen != null && imagen.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(imagen.FileName).ToLowerInvariant();
                    if (!new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" }.Contains(ext))
                    {
                        _logger.LogWarning("Tipo de archivo no permitido: {Extension} por usuario {UsuarioId}", ext, UsuarioActualId);
                        ModelState.AddModelError("imagen", "Solo se permiten imágenes (.jpg, .png, .webp).");
                        return View(reporte);
                    }

                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await imagen.CopyToAsync(stream);

                    reporte.ImagenUrl = $"/uploads/{fileName}";
                    _logger.LogInformation("Imagen guardada: {Path}", reporte.ImagenUrl);
                }

                ModelState.Remove("Usuario");
                if (ModelState.IsValid)
                {
                    _context.Reportes.Add(reporte);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Reporte creado ID={Id} por usuario {UsuarioId}", reporte.Id, UsuarioActualId);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al crear reporte. UsuarioId={UsuarioId}, Titulo={Titulo}", UsuarioActualId, reporte.Titulo);
                ViewBag.ErrorGuardado = "Ocurrió un error al guardar el reporte. Intenta de nuevo.";
            }

            return View(reporte);
        }

        public IActionResult Detalles(int id)
        {
            try
            {
                var reporte = _context.Reportes
                    .Include(r => r.Usuario)
                    .FirstOrDefault(r => r.Id == id);

                if (reporte == null)
                {
                    _logger.LogWarning("Reporte no encontrado: ID={Id}", id);
                    return NotFound();
                }

                var comentarios = _context.Comentarios
                    .Include(c => c.Usuario)
                    .Where(c => c.ReporteId == id)
                    .OrderBy(c => c.Fecha)
                    .ToList();

                var yaVoto = _context.Validaciones
                    .Any(v => v.ReporteId == id && v.UsuarioId == UsuarioActualId);

                return View(new ReporteDetalleViewModel
                {
                    Reporte = reporte,
                    Comentarios = comentarios,
                    YaVoto = yaVoto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar detalles del reporte ID={Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upvote(int id)
        {
            try
            {
                var reporte = _context.Reportes.FirstOrDefault(r => r.Id == id);
                if (reporte == null) return NotFound();

                var yaVoto = _context.Validaciones
                    .Any(v => v.ReporteId == id && v.UsuarioId == UsuarioActualId);

                if (!yaVoto)
                {
                    _context.Validaciones.Add(new Validacion
                    {
                        ReporteId = id,
                        UsuarioId = UsuarioActualId,
                        FechaVoto = DateTime.Now
                    });
                    reporte.Upvotes++;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar upvote. ReporteId={Id}, UsuarioId={UsuarioId}", id, UsuarioActualId);
            }

            return RedirectToAction(nameof(Detalles), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(int reporteId, string texto)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(texto))
                {
                    _context.Comentarios.Add(new Comentario
                    {
                        ReporteId = reporteId,
                        UsuarioId = UsuarioActualId,
                        Texto = texto.Trim(),
                        Fecha = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar comentario. ReporteId={Id}, UsuarioId={UsuarioId}", reporteId, UsuarioActualId);
            }

            return RedirectToAction(nameof(Detalles), new { id = reporteId });
        }
    }
}