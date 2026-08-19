using GymMax.Enums;
using GymMax.DTOs;

namespace GymMax.Services.Perfil
{
    public interface IPerfilService
    {
        Task<PerfilViewDTO?> ObtenerPerfilAsync(int usuarioId);
        Task<EditarPerfilDTO?> ObtenerParaEditarAsync(int usuarioId);
        Task<ResultadoActualizacion> ActualizarAsync(int usuarioId, EditarPerfilDTO model);
        Task<ResultadoCambioPassword> CambiarPasswordAsync(int usuarioId, CambiarPasswordDTO model);
    }
}
