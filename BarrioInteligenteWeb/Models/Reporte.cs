using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarrioInteligenteWeb.Models
{
    public class Reporte
    {
        public int Id { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [Required]
        public string Categoria { get; set; }

        public string Ubicacion { get; set; }

        public string? DireccionFisica { get; set; }

        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string? ImagenUrl { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Estado { get; set; } = "Pendiente";
        public int Upvotes { get; set; } = 0;

        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}