namespace BarrioInteligenteWeb.Services
{
    public interface IReputacionService
    {
        Task AgregarPuntosAsync(int usuarioId, int puntos, string motivo);
        Task<int> GetPuntosAsync(int usuarioId);
    }
}
