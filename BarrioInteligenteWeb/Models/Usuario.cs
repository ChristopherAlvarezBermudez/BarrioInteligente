using System;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string NombreCompleto { get; set; } = default!;
        
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = default!;
        
        [Required]
        public string Password { get; set; } = default!; // En un entorno real esto se guarda encriptado (Hash)
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public string? FotoPerfil { get; set; }

        public bool EmailConfirmado { get; set; } = true;
        
        public string? CodigoVerificacion { get; set; }

        public DateTime? FechaEliminacionProgramada { get; set; }
    }
}