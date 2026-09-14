using BarrioInteligenteWeb.Data;
using BarrioInteligenteWeb.Models;

namespace BarrioInteligenteWeb.Services
{
    public class ReputacionService : IReputacionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReputacionService> _logger;

        public ReputacionService(ApplicationDbContext context, ILogger<ReputacionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AgregarPuntosAsync(int usuarioId, int puntos, string motivo)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return;

            usuario.PuntosReputacion = Math.Clamp(usuario.PuntosReputacion + puntos, -500, 500);
            usuario.Reputacion = ProfanityService.CalcularNivel(usuario.PuntosReputacion);
            usuario.MotivoReputacion = motivo;

            // Evaluamos suspensión por acumulación de puntos negativos
            if (usuario.PuntosReputacion <= -100)
            {
                var nuevaSuspension = DateTime.UtcNow.AddDays(7);
                if (!usuario.FechaSuspensionHasta.HasValue || usuario.FechaSuspensionHasta.Value < nuevaSuspension)
                {
                    usuario.FechaSuspensionHasta = nuevaSuspension;
                }
                usuario.MotivoSuspension = "Suspensión de 1 semana por acumular -100 pts o más en sanciones.";
            }
            else if (usuario.PuntosReputacion <= -50)
            {
                var nuevaSuspension = DateTime.UtcNow.AddDays(2);
                if (!usuario.FechaSuspensionHasta.HasValue || usuario.FechaSuspensionHasta.Value < nuevaSuspension)
                {
                    usuario.FechaSuspensionHasta = nuevaSuspension;
                }
                usuario.MotivoSuspension = "Suspensión temporal de 2 días por acumular -50 pts en sanciones.";
            }
            else if (usuario.PuntosReputacion > -50 && usuario.FechaSuspensionHasta.HasValue)
            {
                // Si la reputación se restablece sobre -50, se remueve la suspensión por puntos
                usuario.FechaSuspensionHasta = null;
                usuario.MotivoSuspension = null;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "[Reputación] Usuario {Id}: {Signo}{Puntos}pts → {Total} ({Nivel}). Motivo: {Motivo}",
                usuarioId, puntos >= 0 ? "+" : "", puntos, usuario.PuntosReputacion, usuario.Reputacion, motivo);
        }

        public async Task<int> GetPuntosAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            return usuario?.PuntosReputacion ?? 0;
        }
    }
}
