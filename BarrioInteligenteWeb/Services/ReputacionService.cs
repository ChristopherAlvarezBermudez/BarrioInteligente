using Microsoft.EntityFrameworkCore;
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
            var usuario = await _context.Usuarios
                .Include(u => u.Insignias)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

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

            // Sincronización automática de insignias por puntaje
            await SincronizarInsigniasInternoAsync(usuario);

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "[Reputación] Usuario {Id}: {Signo}{Puntos}pts → {Total} ({Nivel}). Insignias: {InsigniasCount}. Motivo: {Motivo}",
                usuarioId, puntos >= 0 ? "+" : "", puntos, usuario.PuntosReputacion, usuario.Reputacion, usuario.Insignias.Count, motivo);
        }

        public async Task SincronizarInsigniasAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Insignias)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null) return;

            await SincronizarInsigniasInternoAsync(usuario);
            await _context.SaveChangesAsync();
        }

        private async Task SincronizarInsigniasInternoAsync(Usuario usuario)
        {
            // Regla estricta:
            // "y si tengo -xx puntos se me quitan las insignias y no se me pone nada sino entro a critico"
            if (usuario.PuntosReputacion <= 0)
            {
                if (usuario.Insignias.Any())
                {
                    _logger.LogWarning(
                        "[Insignias-Auto] Usuario {Id} tiene {Puntos}pts (<=0). Se revocan automáticamente todas sus insignias.",
                        usuario.Id, usuario.PuntosReputacion);
                    usuario.Insignias.Clear();
                }
                usuario.Reputacion = NivelReputacion.Critica;
                return;
            }

            // Regla automática progresiva:
            // "si tengo 50 puntos me toque x insignia y el maximo de puntos me da la insignia mejor de todas y ya y se van agregando mientras mas pts consiga"
            var todasInsignias = await _context.Insignias
                .OrderBy(i => i.PuntosRequeridos)
                .ToListAsync();

            if (!todasInsignias.Any()) return;

            // Insignias para las cuales el usuario cumple los puntos requeridos
            var insigniasElegiblesIds = todasInsignias
                .Where(i => i.PuntosRequeridos <= usuario.PuntosReputacion)
                .Select(i => i.Id)
                .ToHashSet();

            // 1. Remover las insignias que el usuario ya no califica por haber perdido puntos
            var insigniasARemover = usuario.Insignias
                .Where(i => !insigniasElegiblesIds.Contains(i.Id))
                .ToList();

            foreach (var rem in insigniasARemover)
            {
                usuario.Insignias.Remove(rem);
            }

            // 2. Agregar las insignias ganadas que aún no tenga asignadas
            var insigniasActualesIds = usuario.Insignias.Select(i => i.Id).ToHashSet();
            var insigniasParaAgregar = todasInsignias
                .Where(i => insigniasElegiblesIds.Contains(i.Id) && !insigniasActualesIds.Contains(i.Id))
                .ToList();

            foreach (var nueva in insigniasParaAgregar)
            {
                usuario.Insignias.Add(nueva);
            }
        }

        public async Task<int> GetPuntosAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            return usuario?.PuntosReputacion ?? 0;
        }
    }
}
