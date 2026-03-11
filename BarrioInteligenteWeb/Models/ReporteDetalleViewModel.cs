using System.Collections.Generic;
using BarrioInteligenteWeb.Models;

namespace BarrioInteligenteWeb.Models
{
    public class ReporteDetalleViewModel
    {
        public Reporte Reporte { get; set; }
        public List<Comentario> Comentarios { get; set; } = new List<Comentario>();
        public bool YaVoto { get; set; }
    }
}

