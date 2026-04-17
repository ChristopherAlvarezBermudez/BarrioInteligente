namespace BarrioInteligenteWeb.Services
{
    /// <summary>
    /// Resultado del análisis de profanidades.
    /// </summary>
    public class ProfanityResult
    {
        public string TextoOriginal { get; set; } = default!;
        public string TextoCensurado { get; set; } = default!;
        public bool FueCensurado { get; set; }
        public List<string> PalabrasDetectadas { get; set; } = new();
    }

    public interface IProfanityService
    {
        /// <summary>
        /// Valida el texto contra el diccionario local y la API externa.
        /// Si detecta ofensas, censura el texto y penaliza al usuario con -5 puntos.
        /// </summary>
        Task<ProfanityResult> ValidarYCensurarAsync(string texto, int usuarioId);
    }
}
