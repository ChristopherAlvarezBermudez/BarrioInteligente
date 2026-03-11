using System;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string NombreCompleto { get; set; }
        
        [Required]
        [EmailAddress]
        public string Correo { get; set; }
        
        [Required]
        public string Password { get; set; } // En un entorno real esto se guarda encriptado (Hash)
        
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}