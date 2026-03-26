using System;
using System.ComponentModel.DataAnnotations;

namespace BarrioInteligenteWeb.Models
{
    public enum NivelReputacion
    {
        Excelente = 0,
        Buena = 1,
        Regular = 2,
        Mala = 3,
        Critica = 4
    }

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

        public string? CodigoRecuperacion { get; set; }
        public DateTime? ExpiracionCodigo { get; set; }

        public int PuntosReputacion { get; set; } = 0;

        public NivelReputacion Reputacion { get; set; } = NivelReputacion.Excelente;

        public string? MotivoReputacion { get; set; }
    }
}