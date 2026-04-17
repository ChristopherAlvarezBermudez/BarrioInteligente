using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class Insignia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = default!;

        [Required]
        public string IconoEmoji { get; set; } = default!;

        [Required]
        [MaxLength(20)]
        public string ColorCss { get; set; } = "#3b82f6"; // Default Blue

        // Relación Muchos-a-Muchos con Usuarios
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
