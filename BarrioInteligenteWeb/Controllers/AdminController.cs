using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Models;
using BarrioInteligenteWeb.Services;

namespace BarrioInteligenteWeb.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IReputacionService _reputacionService;

        public AdminController(ApplicationDbContext context, IReputacionService reputacionService)
        {
            _context = context;
            _reputacionService = reputacionService;
        }

        private async Task<bool> IsValidAdminAsync()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return false;

            var user = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            return user != null && user.EsAdmin;
        }

        public async Task<IActionResult> Panel()
        {
            if (!await IsValidAdminAsync()) return RedirectToAction("Comunidad", "Reportes");

            var usuarios = await _context.Usuarios
                .Include(u => u.Insignias)
                .OrderByDescending(u => u.FechaRegistro)
                .ToListAsync();

            var insigniasDisponibles = await _context.Insignias.ToListAsync();

            ViewBag.InsigniasDisponibles = insigniasDisponibles;

            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjustarReputacion(int usuarioId, int puntosForm)
        {
            if (!await IsValidAdminAsync()) return Json(new { success = false, message = "Acceso denegado." });

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            var diff = puntosForm - usuario.PuntosReputacion;
            if (diff != 0)
            {
                await _reputacionService.AgregarPuntosAsync(usuarioId, diff, "Ajuste manual por Administrador");
            }

            return Json(new { success = true, newReputation = usuario.Reputacion.ToString() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleInsignia(int usuarioId, int insigniaId)
        {
            if (!await IsValidAdminAsync()) return Json(new { success = false, message = "Acceso denegado." });

            var usuario = await _context.Usuarios.Include(u => u.Insignias).FirstOrDefaultAsync(u => u.Id == usuarioId);
            var insignia = await _context.Insignias.FindAsync(insigniaId);

            if (usuario == null || insignia == null) return Json(new { success = false, message = "Usuario o Insignia no encontrados." });

            bool agregada = false;
            if (usuario.Insignias.Any(i => i.Id == insigniaId))
            {
                usuario.Insignias.Remove(insignia);
            }
            else
            {
                usuario.Insignias.Add(insignia);
                agregada = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, agregada = agregada, insignia = new { nombre = insignia.Nombre, colorCss = insignia.ColorCss, iconoEmoji = insignia.IconoEmoji } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarContenido(int id, string tipo)
        {
            if (!await IsValidAdminAsync()) return Json(new { success = false, message = "Acceso denegado." });

            try
            {
                if (tipo == "Reporte")
                {
                    var reporte = await _context.Reportes.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == id);
                    if (reporte != null)
                    {
                        reporte.EsEliminado = true;
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Reporte eliminado permanentemente (lógica)." });
                    }
                }
                else if (tipo == "Comentario")
                {
                    var comentario = await _context.Comentarios.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);
                    if (comentario != null)
                    {
                        comentario.EsEliminado = true;
                        await _context.SaveChangesAsync();
                        return Json(new { success = true, message = "Comentario eliminado." });
                    }
                }
                return Json(new { success = false, message = "Contenido no encontrado." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }
    }
}
