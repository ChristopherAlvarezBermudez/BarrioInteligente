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

            usuario.PuntosReputacion = Math.Clamp(usuario.PuntosReputacion + puntos, 0, 200);
            usuario.Reputacion = ProfanityService.CalcularNivel(usuario.PuntosReputacion);
            usuario.MotivoReputacion = motivo;

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
