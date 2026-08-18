using GymMax.Models;

namespace GymMax.Services.ChatHub
{
    public interface IChatService
    {
        Task<Conversacion> CrearConversacionPrivadaAsync(
            int usuarioId,
            int otroUsuarioId);
        Task<Conversacion> CrearGrupoAsync(
            int administradorId,
            string nombre);
        Task<bool> AgregarMiembroAsync(
            int conversacionId,
            int usuarioId,
            int administradorId);

        Task<bool> EliminarMiembroAsync(
            int conversacionId,
            int usuarioId,
            int administradorId);
        Task<List<Conversacion>> ObtenerConversacionesUsuarioAsync(
            int usuarioId);
        Task<List<Mensaje>> ObtenerMensajesAsync(
            int conversacionId,
            int usuarioId);
        Task<Mensaje?> GuardarMensajeAsync(
            int conversacionId,
            int usuarioId,
            string contenido);
    }
}
