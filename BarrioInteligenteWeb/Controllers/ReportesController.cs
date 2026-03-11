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

        public ReportesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // GET: /Reportes
        public IActionResult Index()
        {
            var lista = _context.Reportes
                .OrderByDescending(r => r.Fecha)
                .ToList();

            ViewBag.UsuarioId = UsuarioActualId;
            return View(lista);
        }

        // GET: /Reportes/MisReportes
        public IActionResult MisReportes()
        {
            var lista = _context.Reportes
                .Where(r => r.UsuarioId == UsuarioActualId)
                .OrderByDescending(r => r.Fecha)
                .ToList();

            ViewBag.SoloMios = true;
            ViewBag.UsuarioId = UsuarioActualId;
            return View("Index", lista);
        }

        // GET: /Reportes/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Reportes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Reporte reporte, IFormFile? imagen)
        {
            reporte.UsuarioId = UsuarioActualId;
            reporte.Fecha = DateTime.Now;

            if (imagen != null && imagen.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);

                var ext = Path.GetExtension(imagen.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                    await imagen.CopyToAsync(stream);

                reporte.ImagenUrl = $"/uploads/{fileName}";
            }

            ModelState.Remove("Usuario");
            if (ModelState.IsValid)
            {
                _context.Reportes.Add(reporte);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(reporte);
        }

        // GET: /Reportes/Detalles/5
        public IActionResult Detalles(int id)
        {
            var reporte = _context.Reportes
                .Include(r => r.Usuario)
                .FirstOrDefault(r => r.Id == id);

            if (reporte == null) return NotFound();

            var comentarios = _context.Comentarios
                .Include(c => c.Usuario)
                .Where(c => c.ReporteId == id)
                .OrderBy(c => c.Fecha)
                .ToList();

            var yaVoto = _context.Validaciones
                .Any(v => v.ReporteId == id && v.UsuarioId == UsuarioActualId);

            var vm = new ReporteDetalleViewModel
            {
                Reporte = reporte,
                Comentarios = comentarios,
                YaVoto = yaVoto
            };

            return View(vm);
        }

        // POST: /Reportes/Upvote/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upvote(int id)
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

            return RedirectToAction(nameof(Detalles), new { id });
        }

        // POST: /Reportes/AgregarComentario
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarComentario(int reporteId, string texto)
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

            return RedirectToAction(nameof(Detalles), new { id = reporteId });
        }
    }
}