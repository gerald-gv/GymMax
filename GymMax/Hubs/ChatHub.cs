using GymMax.Data;
using GymMax.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace GymMax.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        public ChatHub(AppDbContext context)
        {
            _context = context;
        }
        public override async Task OnConnectedAsync()
        {
            var usuarioId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(usuarioId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"usuario-{usuarioId}"
                );
            }
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var usuarioId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(usuarioId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"usuario-{usuarioId}"
                );
            }
            await base.OnDisconnectedAsync(exception);
        }
        public async Task EnviarMensaje(int conversacionId,string contenido)
        {
            var usuarioIdStr = Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                throw new HubException("Usuario no autenticado.");
            if (string.IsNullOrWhiteSpace(contenido))
                throw new HubException("El mensaje no puede estar vacío.");
            contenido = contenido.Trim();
            // Verificar que el usuario pertenece
            // activamente a la conversación.
            var esMiembro = await _context.ConversacionMiembros
                .AnyAsync(cm =>
                    cm.ConversacionId == conversacionId &&
                    cm.UsuarioId == usuarioId &&
                    cm.Activo);
            if (!esMiembro)
                throw new HubException(
                    "No perteneces a esta conversación."
                );
            // Verificar que la conversación está activa.
            var conversacionExiste = await _context.Conversaciones
                .AnyAsync(c =>
                    c.ConversacionId == conversacionId &&
                    c.Activa);
            if (!conversacionExiste)
                throw new HubException(
                    "La conversación no está disponible."
                );
            var mensaje = new Mensaje
            {
                ConversacionId = conversacionId,
                UsuarioId = usuarioId,
                Contenido = contenido,
                FechaEnvio = DateTime.Now,
                Editado = false,
                Eliminado = false
            };
            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();
            var usuario = await _context.Usuarios
                .FindAsync(usuarioId);
            var nombreUsuario = usuario != null
                ? $"{usuario.Nombres} {usuario.Apellidos}"
                : "Usuario";
            await Clients.Group($"conversacion-{conversacionId}")
                .SendAsync(
                    "RecibirMensaje",
                    new
                    {
                        mensaje.MensajeId,
                        mensaje.ConversacionId,
                        mensaje.UsuarioId,
                        NombreUsuario = nombreUsuario,
                        mensaje.Contenido,
                        mensaje.FechaEnvio,
                        mensaje.Editado,
                        mensaje.Eliminado
                    });
        }
        public async Task UnirseAConversacion(int conversacionId)
        {
            var usuarioIdStr = Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                throw new HubException("Usuario no autenticado.");
            var esMiembro = await _context.ConversacionMiembros
                .AnyAsync(cm =>
                    cm.ConversacionId == conversacionId &&
                    cm.UsuarioId == usuarioId &&
                    cm.Activo);
            if (!esMiembro)
                throw new HubException(
                    "No perteneces a esta conversación."
                );
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"conversacion-{conversacionId}"
            );
        }
        public async Task EditarMensaje(int mensajeId, string nuevoContenido)
        {
            var usuarioIdStr = Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );
            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                throw new HubException("Usuario no autenticado.");
            if (string.IsNullOrWhiteSpace(nuevoContenido))
                throw new HubException("El mensaje no puede estar vacío.");
            nuevoContenido = nuevoContenido.Trim();
            var mensaje = await _context.Mensajes
                .FirstOrDefaultAsync(m => m.MensajeId == mensajeId);
            if (mensaje == null)
                throw new HubException("El mensaje no existe.");
            // Solo el autor puede editarlo
            if (mensaje.UsuarioId != usuarioId)
                throw new HubException(
                    "No puedes editar un mensaje de otro usuario."
                );
            // Un mensaje eliminado no puede editarse
            if (mensaje.Eliminado)
                throw new HubException(
                    "No puedes editar un mensaje eliminado."
                );
            // Verificar que la conversación siga activa
            var conversacionExiste = await _context.Conversaciones
                .AnyAsync(c =>
                    c.ConversacionId == mensaje.ConversacionId &&
                    c.Activa);
            if (!conversacionExiste)
                throw new HubException(
                    "La conversación no está disponible."
                );
            mensaje.Contenido = nuevoContenido;
            mensaje.Editado = true;
            await _context.SaveChangesAsync();
            await Clients.Group($"conversacion-{mensaje.ConversacionId}")
                .SendAsync(
                    "MensajeEditado",
                    new
                    {
                        mensaje.MensajeId,
                        mensaje.ConversacionId,
                        mensaje.UsuarioId,
                        mensaje.Contenido,
                        mensaje.Editado,
                        mensaje.Eliminado
                    }
                );
        }
        public async Task EliminarMensaje(int mensajeId)
        {
            var usuarioIdStr = Context.User?.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(usuarioIdStr, out int usuarioId))
                throw new HubException("Usuario no autenticado.");

            var mensaje = await _context.Mensajes
                .FirstOrDefaultAsync(m => m.MensajeId == mensajeId);

            if (mensaje == null)
                throw new HubException("El mensaje no existe.");

            // Solo el autor puede eliminarlo
            if (mensaje.UsuarioId != usuarioId)
                throw new HubException(
                    "No puedes eliminar un mensaje de otro usuario."
                );

            if (mensaje.Eliminado)
                throw new HubException(
                    "El mensaje ya fue eliminado."
                );

            // Verificar que la conversación siga activa
            var conversacionExiste = await _context.Conversaciones
                .AnyAsync(c =>
                    c.ConversacionId == mensaje.ConversacionId &&
                    c.Activa);

            if (!conversacionExiste)
                throw new HubException(
                    "La conversación no está disponible."
                );

            mensaje.Eliminado = true;

            await _context.SaveChangesAsync();

            await Clients.Group($"conversacion-{mensaje.ConversacionId}")
                .SendAsync(
                    "MensajeEliminado",
                    new
                    {
                        mensaje.MensajeId,
                        mensaje.ConversacionId,
                        mensaje.UsuarioId,
                        mensaje.Editado,
                        mensaje.Eliminado
                    }
                );
        }
    }
}
