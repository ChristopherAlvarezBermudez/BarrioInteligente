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
            return user != null && (user.EsAdmin || user.Rol == RolesUsuario.Administrador || user.Correo == "stevenrodriguez77777@gmail.com" || user.Correo == "christopherxd2005@gmail.com");
        }

        public async Task<IActionResult> Panel()
        {
            if (!await IsValidAdminAsync()) return RedirectToAction("Index", "Reportes");

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
            else
            {
                // Asegurar que insignias estén sincronizadas incluso si no hubo cambio neto de puntos
                await _reputacionService.SincronizarInsigniasAsync(usuarioId);
            }

            var usuarioActualizado = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Insignias)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            return Json(new { 
                success = true, 
                newReputation = usuarioActualizado!.Reputacion.ToString(),
                puntos = usuarioActualizado.PuntosReputacion,
                estaSuspendido = usuarioActualizado.EstaSuspendido,
                motivoSuspension = usuarioActualizado.MotivoSuspension,
                fechaSuspension = usuarioActualizado.FechaSuspensionHasta?.ToString("dd/MM/yyyy HH:mm"),
                insignias = usuarioActualizado.Insignias.Select(i => new {
                    id = i.Id,
                    nombre = i.Nombre,
                    iconoEmoji = i.IconoEmoji,
                    colorCss = i.ColorCss
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LevantarSuspension(int usuarioId)
        {
            if (!await IsValidAdminAsync()) return Json(new { success = false, message = "Acceso denegado." });

            var usuario = await _context.Usuarios.Include(u => u.Insignias).FirstOrDefaultAsync(u => u.Id == usuarioId);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            usuario.FechaSuspensionHasta = null;
            usuario.MotivoSuspension = null;
            if (usuario.PuntosReputacion <= -50)
            {
                usuario.PuntosReputacion = 0;
                usuario.Reputacion = NivelReputacion.Critica;
                usuario.MotivoReputacion = "Suspensión levantada manualmente por Administrador.";
            }
            await _reputacionService.SincronizarInsigniasAsync(usuarioId);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Suspensión levantada para {usuario.NombreCompleto}." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SincronizarTodasLasInsignias()
        {
            if (!await IsValidAdminAsync()) return Json(new { success = false, message = "Acceso denegado." });

            var usuarios = await _context.Usuarios.Select(u => u.Id).ToListAsync();
            foreach (var id in usuarios)
            {
                await _reputacionService.SincronizarInsigniasAsync(id);
            }

            return Json(new { success = true, message = $"Se sincronizaron automáticamente las insignias de {usuarios.Count} ciudadanos según sus puntos." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearInsignia(string nombre, string iconoEmoji, string colorCss, int puntosRequeridos = 50)
        {
            if (!await IsValidAdminAsync()) return Json(new { success = false, message = "Acceso denegado." });

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(iconoEmoji))
            {
                return Json(new { success = false, message = "Nombre e ícono son requeridos." });
            }

            var insignia = new Insignia
            {
                Nombre = nombre.Trim(),
                IconoEmoji = iconoEmoji.Trim(),
                ColorCss = string.IsNullOrWhiteSpace(colorCss) ? "#3b82f6" : colorCss.Trim(),
                PuntosRequeridos = puntosRequeridos > 0 ? puntosRequeridos : 50
            };

            _context.Insignias.Add(insignia);
            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Insignia '{insignia.Nombre}' creada correctamente.",
                insignia = new { id = insignia.Id, nombre = insignia.Nombre, iconoEmoji = insignia.IconoEmoji, colorCss = insignia.ColorCss, puntosRequeridos = insignia.PuntosRequeridos }
            });
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

            return Json(new { success = true, agregada = agregada, insignia = new { id = insignia.Id, nombre = insignia.Nombre, colorCss = insignia.ColorCss, iconoEmoji = insignia.IconoEmoji } });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(int usuarioId, string nuevoRol)
        {
            if (!await IsValidAdminAsync()) return Json(new { success = false, message = "Acceso denegado." });

            if (!RolesUsuario.Todos.Contains(nuevoRol))
            {
                return Json(new { success = false, message = "Rol no reconocido por el sistema." });
            }

            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado." });

            if (usuario.Correo == "stevenrodriguez77777@gmail.com" || usuario.Correo == "christopherxd2005@gmail.com")
            {
                if (nuevoRol != RolesUsuario.Administrador)
                {
                    return Json(new { success = false, message = "El rol de Propietario no puede ser degradado." });
                }
            }

            usuario.Rol = nuevoRol;
            usuario.EsAdmin = (nuevoRol == RolesUsuario.Administrador);

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = $"Rol de {usuario.NombreCompleto} actualizado a {nuevoRol}.",
                nuevoRol = usuario.Rol,
                esAdmin = usuario.EsAdmin
            });
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
