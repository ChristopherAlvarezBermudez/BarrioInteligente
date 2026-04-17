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
    public class DenunciasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IReputacionService _reputacionService;
        private readonly ILogger<DenunciasController> _logger;

        // Correo del administrador que recibe las denuncias
        private const string ADMIN_EMAIL = "christopherxd2005@gmail.com";

        public DenunciasController(
            ApplicationDbContext context,
            IEmailService emailService,
            IReputacionService reputacionService,
            ILogger<DenunciasController> logger)
        {
            _context = context;
            _emailService = emailService;
            _reputacionService = reputacionService;
            _logger = logger;
        }

        private int UsuarioActualId =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        /// <summary>
        /// POST: /Denuncias/ReportarPerfil
        /// Recibe el ID del usuario reportado y el motivo de la denuncia.
        /// Envía un correo automático al administrador con los detalles.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportarPerfil(int usuarioReportadoId, string motivo, string? descripcion)
        {
            try
            {
                var denuncianteId = UsuarioActualId;

                // Evitar autoreportes
                if (denuncianteId == usuarioReportadoId)
                {
                    return Json(new { success = false, message = "No puedes reportarte a ti mismo." });
                }

                // Validar que ambos usuarios existan
                var denunciante = await _context.Usuarios.FindAsync(denuncianteId);
                var reportado = await _context.Usuarios.FindAsync(usuarioReportadoId);

                if (denunciante == null || reportado == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado." });
                }

                // Evitar denuncias duplicadas recientes (misma combinación en las últimas 24h)
                var denunciaReciente = await _context.DenunciasUsuarios
                    .AnyAsync(d => d.DenuncianteId == denuncianteId
                               && d.ReportadoId == usuarioReportadoId
                               && d.Fecha > DateTime.Now.AddHours(-24));

                if (denunciaReciente)
                {
                    return Json(new { success = false, message = "Ya reportaste a este usuario recientemente. Espera 24 horas." });
                }

                // Guardar la denuncia en la base de datos
                var denuncia = new DenunciaUsuario
                {
                    DenuncianteId = denuncianteId,
                    ReportadoId = usuarioReportadoId,
                    Motivo = motivo,
                    Descripcion = descripcion?.Trim(),
                    Fecha = DateTime.Now
                };

                _context.DenunciasUsuarios.Add(denuncia);
                await _context.SaveChangesAsync();

                // Penalizar al usuario reportado con -3 puntos por denuncia recibida
                await _reputacionService.AgregarPuntosAsync(
                    usuarioReportadoId, -3,
                    $"Denuncia recibida por: {motivo}");

                // Enviar correo al administrador
                var asunto = $"🚨 Denuncia de Usuario — {motivo}";
                var cuerpoHtml = $@"
                    <div style='font-family:Inter,sans-serif;max-width:600px;margin:0 auto;'>
                        <h2 style='color:#c0392b;'>🚨 Nueva Denuncia de Usuario</h2>
                        <hr style='border:1px solid #eee;' />
                        
                        <h3 style='color:#333;'>Denunciante (quien reporta)</h3>
                        <ul>
                            <li><strong>Nombre:</strong> {denunciante.NombreCompleto}</li>
                            <li><strong>Correo:</strong> {denunciante.Correo}</li>
                            <li><strong>ID:</strong> #{denunciante.Id}</li>
                        </ul>

                        <h3 style='color:#e74c3c;'>Usuario Reportado</h3>
                        <ul>
                            <li><strong>Nombre:</strong> {reportado.NombreCompleto}</li>
                            <li><strong>Correo:</strong> {reportado.Correo}</li>
                            <li><strong>ID:</strong> #{reportado.Id}</li>
                            <li><strong>Reputación:</strong> {reportado.PuntosReputacion} pts ({reportado.Reputacion})</li>
                        </ul>

                        <h3 style='color:#333;'>Detalles de la Denuncia</h3>
                        <ul>
                            <li><strong>Motivo:</strong> {motivo}</li>
                            <li><strong>Descripción:</strong> {descripcion ?? "(sin descripción adicional)"}</li>
                            <li><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm:ss}</li>
                        </ul>

                        <hr style='border:1px solid #eee;' />
                        <p style='font-size:12px;color:#999;'>
                            Este correo fue generado automáticamente por el sistema de moderación de Barrio Inteligente.
                        </p>
                    </div>";

                try
                {
                    await _emailService.EnviarAsync(ADMIN_EMAIL, asunto, cuerpoHtml);
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx,
                        "[Denuncia] Correo de notificación falló, pero la denuncia #{Id} fue guardada.",
                        denuncia.Id);
                }

                _logger.LogInformation(
                    "[Denuncia] Usuario #{DenuncianteId} reportó a #{ReportadoId}. Motivo: {Motivo}",
                    denuncianteId, usuarioReportadoId, motivo);

                return Json(new { success = true, message = "Denuncia enviada correctamente. Gracias por ayudar a mantener la comunidad segura." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Denuncia] Error al procesar denuncia");
                return Json(new { success = false, message = "Error interno al procesar la denuncia." });
            }
        }

        /// <summary>
        /// GET: /Denuncias/ObtenerPerfilPublico?id=5
        /// Retorna datos públicos de un usuario para el modal de perfil.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerPerfilPublico(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Where(u => u.Id == id)
                    .Select(u => new
                    {
                        id = u.Id,
                        nombre = u.NombreCompleto,
                        foto = u.FotoPerfil ?? "",
                        puntos = u.PuntosReputacion,
                        nivel = u.Reputacion.ToString(),
                        miembro = u.FechaRegistro.ToString("MMMM yyyy")
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado." });

                return Json(new { success = true, data = usuario });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Denuncia] Error al obtener perfil público. UsuarioId={Id}", id);
                return Json(new { success = false, message = "Error al obtener datos del usuario." });
            }
        }
    }
}
