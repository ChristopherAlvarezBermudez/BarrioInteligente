using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class DenunciaUsuario
    {
        [Key]
        public int Id { get; set; }

        /// <summary>ID del usuario que realiza la denuncia.</summary>
        public int DenuncianteId { get; set; }
        public Usuario Denunciante { get; set; } = default!;

        /// <summary>ID del usuario reportado.</summary>
        public int ReportadoId { get; set; }
        public Usuario Reportado { get; set; } = default!;

        [Required]
        [StringLength(50)]
        public string Motivo { get; set; } = default!; // Spam, Comportamiento indebido, Contenido ofensivo, Otro

        [StringLength(500)]
        public string? Descripcion { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public bool Procesada { get; set; } = false;
    }
}
