using GymMax.Data;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GymMax.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        public ChatController(AppDbContext context)
        {
            _context = context;
        }
        // GET: /Chat
        // Muestra las conversaciones del usuario actual
        public async Task<IActionResult> Index()
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");
            var conversaciones = await _context.Conversaciones
                .Include(c => c.Miembros)
                    .ThenInclude(m => m.Usuario)
                .Where(c => c.Activa &&
                            c.Miembros.Any(m =>
                                m.UsuarioId == usuarioId.Value &&
                                m.Activo))
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
            return View(conversaciones);
        }
        // GET: /Chat/Abrir/5
        // Abre una conversación específica
        public async Task<IActionResult> Abrir(int id)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");
            var conversacion = await _context.Conversaciones
                .Include(c => c.Miembros)
                    .ThenInclude(m => m.Usuario)
                .Include(c => c.Mensajes)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.ConversacionId == id &&
                    c.Activa);
            if (conversacion == null)
                return NotFound();
            // Verificar que el usuario pertenezca a la conversación
            var esMiembro = conversacion.Miembros.Any(m =>
                m.UsuarioId == usuarioId.Value &&
                m.Activo);
            if (!esMiembro)
                return Forbid();
            // Ordenar los mensajes
            conversacion.Mensajes = conversacion.Mensajes
                .OrderBy(m => m.FechaEnvio)
                .ToList();
            return View(conversacion);
        }
        // GET: /Chat/Usuarios
        // Lista de usuarios disponibles para iniciar una conversación privada
        public async Task<IActionResult> Usuarios()
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");
            var usuarios = await _context.Usuarios
                .Where(u => u.UsuarioId != usuarioId.Value)
                .OrderBy(u => u.Nombres)
                .ThenBy(u => u.Apellidos)
                .ToListAsync();
            return View(usuarios);
        }
        // POST: /Chat/CrearPrivada
        // Crea una conversación privada entre dos usuarios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearPrivada(int usuarioDestinoId)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");
            if (usuarioDestinoId == usuarioId.Value)
            {
                TempData["Error"] = "No puedes iniciar una conversación contigo mismo.";
                return RedirectToAction(nameof(Usuarios));
            }
            var usuarioDestino = await _context.Usuarios
                .FindAsync(usuarioDestinoId);
            if (usuarioDestino == null)
            {
                TempData["Error"] = "El usuario seleccionado no existe.";
                return RedirectToAction(nameof(Usuarios));
            }
            // Buscar si ya existe una conversación privada entre ambos
            var conversacionExistente = await _context.Conversaciones
                .Where(c => c.Tipo == TipoConversacion.Privada && c.Activa)
                .Where(c => c.Miembros.Count(m => m.Activo) == 2)
                .Where(c => c.Miembros.Any(m =>
                    m.UsuarioId == usuarioId.Value && m.Activo))
                .Where(c => c.Miembros.Any(m =>
                    m.UsuarioId == usuarioDestinoId && m.Activo))
                .FirstOrDefaultAsync();
            if (conversacionExistente != null)
            {
                return RedirectToAction(
                    nameof(Abrir),
                    new { id = conversacionExistente.ConversacionId });
            }
            // Crear conversación
            var conversacion = new Conversacion
            {
                Tipo = TipoConversacion.Privada,
                Nombre = null,
                FechaCreacion = DateTime.Now,
                CreadaPorUsuarioId = usuarioId.Value,
                Activa = true
            };
            _context.Conversaciones.Add(conversacion);
            await _context.SaveChangesAsync();
            // Agregar creador
            _context.ConversacionMiembros.Add(new ConversacionMiembro
            {
                ConversacionId = conversacion.ConversacionId,
                UsuarioId = usuarioId.Value,
                FechaIngreso = DateTime.Now,
                Activo = true
            });
            // Agregar destinatario
            _context.ConversacionMiembros.Add(new ConversacionMiembro
            {
                ConversacionId = conversacion.ConversacionId,
                UsuarioId = usuarioDestinoId,
                FechaIngreso = DateTime.Now,
                Activo = true
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(
                nameof(Abrir),
                new { id = conversacion.ConversacionId });
        }
        private int? ObtenerUsuarioId()
        {
            var usuarioIdStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(usuarioIdStr, out int usuarioId))
                return usuarioId;
            return null;
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> CrearGrupo()
        {
            var usuarios = await _context.Usuarios
            .Include(u => u.Suscripciones
                .Where(s => s.Estado == EstadoSuscripcion.Activa))
            .ThenInclude(s => s.Plan)
            .Where(u => u.Estado == EstadoUsuario.Activo)
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .ToListAsync();

            return View(usuarios);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGrupo(string nombre,List<int> usuariosIds)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError("nombre", "El nombre del grupo es obligatorio.");
            }

            if (usuariosIds == null || !usuariosIds.Any())
            {
                ModelState.AddModelError("usuariosIds","Debes seleccionar al menos un miembro.");
            }
            if (!ModelState.IsValid)
            {
                var usuarios = await _context.Usuarios
                    .Include(u => u.Suscripciones
                        .Where(s => s.Estado == EstadoSuscripcion.Activa))
                    .ThenInclude(s => s.Plan)
                    .Where(u => u.Estado == EstadoUsuario.Activo)
                    .OrderBy(u => u.Nombres)
                    .ThenBy(u => u.Apellidos)
                    .ToListAsync();

                return View(usuarios);
            }
            var usuarioIdStr = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );
            if (!int.TryParse(usuarioIdStr, out int administradorId))
                return RedirectToAction("Login", "Auth");
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
            // Agregar al administrador como miembro
            var miembros = new List<ConversacionMiembro>{
            new ConversacionMiembro
            {
                ConversacionId = grupo.ConversacionId,
                UsuarioId = administradorId,
                FechaIngreso = DateTime.Now,
                Activo = true
            }
            };
            // Agregar los usuarios seleccionados
            foreach (var usuarioId in usuariosIds.Distinct())
            {
                if (usuarioId == administradorId)
                    continue;
                miembros.Add(new ConversacionMiembro
                {
                    ConversacionId = grupo.ConversacionId,
                    UsuarioId = usuarioId,
                    FechaIngreso = DateTime.Now,
                    Activo = true
                });
            }
            _context.ConversacionMiembros.AddRange(miembros);
            await _context.SaveChangesAsync();
            return RedirectToAction(
                "Abrir",
                new { id = grupo.ConversacionId }
            );
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> AdministrarGrupo(int id)
        {
            var grupo = await _context.Conversaciones
                .Include(c => c.Miembros)
                    .ThenInclude(cm => cm.Usuario)
                .FirstOrDefaultAsync(c =>
                    c.ConversacionId == id &&
                    c.Tipo == TipoConversacion.Grupo &&
                    c.Activa);
            if (grupo == null)
                return NotFound();
            var miembrosIds = grupo.Miembros
                .Where(m => m.Activo)
                .Select(m => m.UsuarioId)
                .ToList();
            var usuariosDisponibles = await _context.Usuarios
                .Include(u => u.Suscripciones
        .Where(s => s.Estado == EstadoSuscripcion.Activa))
        .ThenInclude(s => s.Plan)
    .Where(u =>
        u.Estado == EstadoUsuario.Activo &&
        !miembrosIds.Contains(u.UsuarioId))
    .OrderBy(u => u.Nombres)
    .ThenBy(u => u.Apellidos)
    .ToListAsync();
            var suscripcionesActivas = await _context.Suscripciones
                .Include(s => s.Plan)
                .Where(s =>
                    usuariosDisponibles
                        .Select(u => u.UsuarioId)
                        .Contains(s.UsuarioId) &&
                    s.Estado == EstadoSuscripcion.Activa &&
                    s.FechaFin >= DateOnly.FromDateTime(DateTime.Today))
                .ToListAsync();
            ViewBag.UsuariosDisponibles = usuariosDisponibles;
            ViewBag.SuscripcionesActivas = suscripcionesActivas;
            return View(grupo);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarMiembro(int conversacionId, int usuarioId)
        {
            var grupo = await _context.Conversaciones
                .FirstOrDefaultAsync(c =>
                    c.ConversacionId == conversacionId &&
                    c.Tipo == TipoConversacion.Grupo &&
                    c.Activa);
            if (grupo == null)
                return NotFound();
            var miembro = await _context.ConversacionMiembros
                .FirstOrDefaultAsync(cm =>
                    cm.ConversacionId == conversacionId &&
                    cm.UsuarioId == usuarioId);
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
            TempData["Exito"] = "Usuario agregado al grupo.";
            return RedirectToAction(
                "AdministrarGrupo",
                new { id = conversacionId });
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMiembro(int conversacionId, int usuarioId)
        {
            var idActual = ObtenerUsuarioId();
            if (idActual != null && idActual.Value == usuarioId)
            {
                TempData["Error"] = "No puedes eliminarte a ti mismo del grupo. Usa la opción de salir del grupo.";
                return RedirectToAction("AdministrarGrupo", new { id = conversacionId });
            }
            var grupo = await _context.Conversaciones
                .FirstOrDefaultAsync(c =>
                    c.ConversacionId == conversacionId &&
                    c.Tipo == TipoConversacion.Grupo);
            if (grupo == null)
                return NotFound();
            var miembro = await _context.ConversacionMiembros
                .FirstOrDefaultAsync(cm =>
                    cm.ConversacionId == conversacionId &&
                    cm.UsuarioId == usuarioId &&
                    cm.Activo);
            if (miembro == null)
                return NotFound();
            miembro.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Usuario eliminado del grupo.";
            return RedirectToAction("AdministrarGrupo", new { id = conversacionId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalirDeGrupo(int conversacionId)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");
            var miembro = await _context.ConversacionMiembros
                .FirstOrDefaultAsync(cm =>
                    cm.ConversacionId == conversacionId &&
                    cm.UsuarioId == usuarioId.Value &&
                    cm.Activo);
            if (miembro == null)
                return NotFound();
            miembro.Activo = false;
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Has salido del grupo.";
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Grupos()
        {
            var grupos = await _context.Conversaciones
                .Where(c => c.Tipo == TipoConversacion.Grupo && c.Activa)
                .Include(c => c.Miembros.Where(m => m.Activo))
                    .ThenInclude(m => m.Usuario)
                        .ThenInclude(u => u.Rol)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
            return View(grupos);
        }
        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnirmeAGrupo(int conversacionId)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == null)
                return RedirectToAction("Login", "Auth");
            var grupo = await _context.Conversaciones
                .FirstOrDefaultAsync(c =>
                    c.ConversacionId == conversacionId &&
                    c.Tipo == TipoConversacion.Grupo &&
                    c.Activa);
            if (grupo == null)
                return NotFound();
            var miembro = await _context.ConversacionMiembros
                .FirstOrDefaultAsync(cm =>
                    cm.ConversacionId == conversacionId &&
                    cm.UsuarioId == usuarioId.Value);
            if (miembro != null)
            {
                if (miembro.Activo)
                {
                    TempData["Error"] = "Ya eres miembro de este grupo.";
                    return RedirectToAction("Grupos");
                }
                miembro.Activo = true;
                miembro.FechaIngreso = DateTime.Now;
            }
            else
            {
                _context.ConversacionMiembros.Add(new ConversacionMiembro
                {
                    ConversacionId = conversacionId,
                    UsuarioId = usuarioId.Value,
                    FechaIngreso = DateTime.Now,
                    Activo = true
                });
            }
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Te uniste al grupo correctamente.";
            return RedirectToAction("Grupos");
        }
    }
}