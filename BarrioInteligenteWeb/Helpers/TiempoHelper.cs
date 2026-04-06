namespace BarrioInteligenteWeb.Helpers
{
    /// <summary>
    /// Genera texto legible de tiempo transcurrido desde una fecha hasta ahora.
    /// </summary>
    public static class TiempoHelper
    {
        public static string TiempoRelativo(DateTime fecha)
        {
            var diff = DateTime.Now - fecha;

            // Menos de 1 minuto
            if (diff.TotalMinutes < 1)
                return "hace un momento";

            // Menos de 60 minutos → minutos
            if (diff.TotalMinutes < 60)
            {
                int mins = (int)diff.TotalMinutes;
                return $"hace {mins} {(mins == 1 ? "minuto" : "minutos")}";
            }

            // Menos de 24 horas → horas
            if (diff.TotalHours < 24)
            {
                int horas = (int)diff.TotalHours;
                return $"hace {horas} {(horas == 1 ? "hora" : "horas")}";
            }

            // Menos de 7 días → días
            if (diff.TotalDays < 7)
            {
                int dias = (int)diff.TotalDays;
                return $"hace {dias} {(dias == 1 ? "día" : "días")}";
            }

            // Menos de 30 días → semanas
            if (diff.TotalDays < 30)
            {
                int semanas = (int)(diff.TotalDays / 7);
                return $"hace {semanas} {(semanas == 1 ? "semana" : "semanas")}";
            }

            // Menos de 365 días → meses
            if (diff.TotalDays < 365)
            {
                int meses = (int)(diff.TotalDays / 30);
                return $"hace {meses} {(meses == 1 ? "mes" : "meses")}";
            }

            // 1 año o más → años, meses y días exactos
            int totalAnos = (int)(diff.TotalDays / 365);
            int diasRestantes = (int)(diff.TotalDays % 365);
            int totalMeses = diasRestantes / 30;
            int totalDias = diasRestantes % 30;

            var partes = new System.Collections.Generic.List<string>();
            partes.Add($"{totalAnos} {(totalAnos == 1 ? "año" : "años")}");
            if (totalMeses > 0)
                partes.Add($"{totalMeses} {(totalMeses == 1 ? "mes" : "meses")}");
            if (totalDias > 0)
                partes.Add($"{totalDias} {(totalDias == 1 ? "día" : "días")}");

            return "hace " + string.Join(", ", partes);
        }
    }
}
