using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Services.ChatHub
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        public ChatService(AppDbContext context)
        {
            _context = context;
        }
        // CREAR CONVERSACIÓN PRIVADA
        public async Task<Conversacion> CrearConversacionPrivadaAsync(
            int usuarioId,
            int otroUsuarioId)
        {
            if (usuarioId == otroUsuarioId)
                throw new InvalidOperationException(
                    "No puedes iniciar una conversación contigo mismo.");
            var conversacionExistente = await _context.Conversaciones
                .Where(c => c.Tipo == TipoConversacion.Privada)
                .Where(c => c.Activa)
                .Where(c => c.Miembros.Any(m =>
                    m.UsuarioId == usuarioId && m.Activo))
                .Where(c => c.Miembros.Any(m =>
                    m.UsuarioId == otroUsuarioId && m.Activo))
                .FirstOrDefaultAsync();
            if (conversacionExistente != null)
                return conversacionExistente;
            var conversacion = new Conversacion
            {
                Tipo = TipoConversacion.Privada,
                FechaCreacion = DateTime.Now,
                CreadaPorUsuarioId = usuarioId,
                Activa = true
            };
            _context.Conversaciones.Add(conversacion);
            await _context.SaveChangesAsync();
            _context.ConversacionMiembros.AddRange(
                new ConversacionMiembro
                {
                    ConversacionId = conversacion.ConversacionId,
                    UsuarioId = usuarioId,
                    FechaIngreso = DateTime.Now,
                    Activo = true
                },
                new ConversacionMiembro
                {
                    ConversacionId = conversacion.ConversacionId,
                    UsuarioId = otroUsuarioId,
                    FechaIngreso = DateTime.Now,
                    Activo = true
                });
            await _context.SaveChangesAsync();
            return conversacion;
        }
        // CREAR GRUPO
        public async Task<Conversacion> CrearGrupoAsync(
            int administradorId,
            string nombre)
        {
            var administrador = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.UsuarioId == administradorId);

            if (administrador == null ||
                administrador.Rol?.Nombre != "Administrador")
            {
                throw new UnauthorizedAccessException(
                    "Solo los administradores pueden crear grupos.");
            }

            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException(
                    "El nombre del grupo es obligatorio.");

            var grupo = new Conversacion
            {
                Tipo = TipoConversacion.Grupo,
                Nombre = nombre.Trim(),
                FechaCreacion = DateTime.Now,
                CreadaPorUsuarioId = administradorId,
                Activa = true
            };

            _context.Conversaciones.Add(grupo);

            await _context.SaveChangesAsync();
            // El administrador que creó el grupo
            // también se convierte en miembro
            var miembro = new ConversacionMiembro
            {
                ConversacionId = grupo.ConversacionId,
                UsuarioId = administradorId,
                FechaIngreso = DateTime.Now,
                Activo = true
            };

            _context.ConversacionMiembros.Add(miembro);

            await _context.SaveChangesAsync();

            return grupo;
        }
        // AGREGAR MIEMBRO
        public async Task<bool> AgregarMiembroAsync(
            int conversacionId,
            int usuarioId,
            int administradorId)
        {
            var administrador = await EsAdministradorAsync(
                administradorId);

            if (!administrador)
                return false;

            var grupo = await _context.Conversaciones
                .FirstOrDefaultAsync(c =>
                    c.ConversacionId == conversacionId &&
                    c.Tipo == TipoConversacion.Grupo &&
                    c.Activa);

            if (grupo == null)
                return false;

            var miembro = await _context.ConversacionMiembros
                .FirstOrDefaultAsync(m =>
                    m.ConversacionId == conversacionId &&
                    m.UsuarioId == usuarioId);

            if (miembro != null)
            {
                miembro.Activo = true;
                miembro.FechaIngreso = DateTime.Now;
            }
            else
            {
                miembro = new ConversacionMiembro
                {
                    ConversacionId = conversacionId,
                    UsuarioId = usuarioId,
                    FechaIngreso = DateTime.Now,
                    Activo = true
                };

                _context.ConversacionMiembros.Add(miembro);
            }

            await _context.SaveChangesAsync();

            return true;
        }
        // ELIMINAR MIEMBRO
        public async Task<bool> EliminarMiembroAsync(
            int conversacionId,
            int usuarioId,
            int administradorId)
        {
            var administrador = await EsAdministradorAsync(
                administradorId);
            if (!administrador)
                return false;
            var miembro = await _context.ConversacionMiembros
                .FirstOrDefaultAsync(m =>
                    m.ConversacionId == conversacionId &&
                    m.UsuarioId == usuarioId &&
                    m.Activo);
            if (miembro == null)
                return false;
            miembro.Activo = false;

            await _context.SaveChangesAsync();

            return true;
        }
        // CONVERSACIONES DEL USUARIO
        public async Task<List<Conversacion>>
            ObtenerConversacionesUsuarioAsync(int usuarioId)
        {
            return await _context.ConversacionMiembros
                .Where(m =>
                    m.UsuarioId == usuarioId &&
                    m.Activo &&
                    m.Conversacion.Activa)
                .Include(m => m.Conversacion)
                    .ThenInclude(c => c.Miembros)
                    .ThenInclude(m => m.Usuario)
                .Select(m => m.Conversacion)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
        }
        // OBTENER MENSAJES
        public async Task<List<Mensaje>> ObtenerMensajesAsync(
            int conversacionId,
            int usuarioId)
        {
            var pertenece = await _context.ConversacionMiembros
                .AnyAsync(m =>
                    m.ConversacionId == conversacionId &&
                    m.UsuarioId == usuarioId &&
                    m.Activo);
            if (!pertenece)
                return new List<Mensaje>();
            return await _context.Mensajes
                .Where(m =>
                    m.ConversacionId == conversacionId &&
                    !m.Eliminado)
                .Include(m => m.Usuario)
                .OrderBy(m => m.FechaEnvio)
                .ToListAsync();
        }
        // GUARDAR MENSAJE
        public async Task<Mensaje?> GuardarMensajeAsync(
            int conversacionId,
            int usuarioId,
            string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                return null;
            var pertenece = await _context.ConversacionMiembros
                .AnyAsync(m =>
                    m.ConversacionId == conversacionId &&
                    m.UsuarioId == usuarioId &&
                    m.Activo);
            if (!pertenece)
                return null;
            var mensaje = new Mensaje
            {
                ConversacionId = conversacionId,
                UsuarioId = usuarioId,
                Contenido = contenido.Trim(),
                FechaEnvio = DateTime.Now,
                Editado = false,
                Eliminado = false
            };
            _context.Mensajes.Add(mensaje);
            await _context.SaveChangesAsync();
            await _context.Entry(mensaje)
                .Reference(m => m.Usuario)
                .LoadAsync();

            return mensaje;
        }
        // COMPROBAR ADMINISTRADOR
        private async Task<bool> EsAdministradorAsync(
            int usuarioId)
        {
            return await _context.Usuarios
                .Include(u => u.Rol)
                .AnyAsync(u =>
                    u.UsuarioId == usuarioId &&
                    u.Rol != null &&
                    u.Rol.Nombre == "Administrador");
        }
    }
}
