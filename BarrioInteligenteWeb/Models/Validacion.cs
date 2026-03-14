using System;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class Validacion
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime FechaVoto { get; set; } = DateTime.Now;

        // ¿Qué reporte recibió el upvote?
        public int ReporteId { get; set; }
        public Reporte Reporte { get; set; } = default!;

        // ¿Qué usuario dio el upvote?
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = default!;
    }
}