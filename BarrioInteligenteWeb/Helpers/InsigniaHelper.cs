namespace BarrioInteligenteWeb.Helpers
{
    public static class InsigniaHelper
    {
        public record Insignia(string Emoji, string Titulo);

        public static Insignia GetInsignia(int puntos) => puntos switch
        {
            >= 500 => new Insignia("🦸", "Héroe Comunitario"),
            >= 200 => new Insignia("🏅", "Guardián del Barrio"),
            >= 50  => new Insignia("⭐", "Ciudadano Activo"),
            _      => new Insignia("🌱", "Vecino Nuevo"),
        };
    }
}
