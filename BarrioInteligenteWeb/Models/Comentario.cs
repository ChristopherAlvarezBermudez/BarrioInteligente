using System;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class Comentario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Texto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        // Llaves Foráneas (Relaciones)
        public int ReporteId { get; set; }
        public Reporte Reporte { get; set; } // Propiedad de navegación

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } // Propiedad de navegación

        public ICollection<ComentarioLike> Likes { get; set; }
    }
}