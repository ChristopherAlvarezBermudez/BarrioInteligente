using System;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class HistorialEstado
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string EstadoAnterior { get; set; }
        
        [Required]
        public string EstadoNuevo { get; set; }
        
        public DateTime FechaCambio { get; set; } = DateTime.Now;

        // Relación con el Reporte modificado
        public int ReporteId { get; set; }
        public Reporte Reporte { get; set; }

        // Relación con el Usuario que hizo el cambio (ej. un administrador del ayuntamiento)
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
    }
}