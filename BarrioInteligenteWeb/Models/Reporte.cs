using System;

namespace BarrioInteligenteWeb.Models
{
    public class Reporte
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public string Ubicacion { get; set; }
        public DateTime Fecha { get; set; }
    }
}