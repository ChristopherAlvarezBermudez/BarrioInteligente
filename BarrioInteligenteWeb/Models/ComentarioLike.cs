using System;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class ComentarioLike
    {
        [Key]
        public int Id { get; set; }

        public int ComentarioId { get; set; }
        public Comentario Comentario { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
