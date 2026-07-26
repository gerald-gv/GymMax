using GymMax.Data;
using GymMax.Domain.Entities;
using GymMax.Enums;
using GymMax.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(string? nombre, int? rolId, EstadoUsuario? estado, DateOnly? fechaDesde, DateOnly? fechaHasta)
        {
            var query = _context.Usuarios.Include(u => u.Rol).AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(u => u.Nombres.Contains(nombre) || u.Apellidos.Contains(nombre));

            if (rolId.HasValue)
                query = query.Where(u => u.RolId == rolId);

            if (estado.HasValue)
                query = query.Where(u => u.Estado == estado);

            if (fechaDesde.HasValue)
                query = query.Where(u => DateOnly.FromDateTime(u.FechaRegistro) >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(u => DateOnly.FromDateTime(u.FechaRegistro) <= fechaHasta.Value);

            ViewBag.FiltroNombre = nombre;
            ViewBag.FiltroRolId = new SelectList(_context.Roles, "RolId", "Nombre", rolId);
            ViewBag.FiltroEstado = new SelectList(
                Enum.GetValues<EstadoUsuario>().Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value", "Text", (int?)estado);
            ViewBag.FiltroDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = fechaHasta?.ToString("yyyy-MM-dd");

            return View(await query.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            ViewData["RolId"] = new SelectList(_context.Roles, "RolId", "Nombre");
            return View();
        }
        private string GenerarCodigoMembresia()
        {
            return $"GM-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario, string Password)
        {
            ModelState.Remove("PasswordHash");
            if (ModelState.IsValid)
            {
                usuario.FechaRegistro = DateTime.Now;
                var passwordHasher = new PasswordHasher<Usuario>();
                usuario.PasswordHash = passwordHasher.HashPassword(usuario, Password);
                if (usuario.RolId == (int)RolUsuario.Cliente)
                {
                    usuario.CodigoMembresia = GenerarCodigoMembresia();
                }
                _context.Usuarios.Add(usuario);
                if (usuario.RolId == (int)RolUsuario.Coach)
                {
                    var coach = new Coach
                    {
                        Usuario = usuario,
                        FechaIngreso = DateOnly.FromDateTime(DateTime.Now),
                        Activo = true
                    };
                    _context.Coaches.Add(coach);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            ViewData["RolId"] = new SelectList(
                _context.Roles,
                "RolId",
                "Nombre",
                usuario.RolId
            );
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["RolId"] = new SelectList(_context.Roles, "RolId", "Nombre", usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UsuarioId,RolId,Nombres,Apellidos,Dni,Email,Telefono,FechaNacimiento,CodigoMembresia,Estado")] Usuario usuarioInput, string? NuevaPassword)
        {
            if (id != usuarioInput.UsuarioId)
            {
                return NotFound();
            }
            // Ignoramos la validación del modelo para la contraseña, ya que la gestionamos manualmente
            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                try
                {
                    // Traer el usuario original guardado en la base de datos
                    var usuarioDb = await _context.Usuarios.FindAsync(id);
                    if (usuarioDb == null)
                    {
                        return NotFound();
                    }

                    // Actualizar los campos normles
                    usuarioDb.RolId = usuarioInput.RolId;
                    usuarioDb.Nombres = usuarioInput.Nombres;
                    usuarioDb.Apellidos = usuarioInput.Apellidos;
                    usuarioDb.Dni = usuarioInput.Dni;
                    usuarioDb.Email = usuarioInput.Email;
                    usuarioDb.Telefono = usuarioInput.Telefono;
                    usuarioDb.FechaNacimiento = usuarioInput.FechaNacimiento;
                    usuarioDb.Estado = usuarioInput.Estado;

                    // Solo si ingresó una nueva contraseña, la hasheamos y actualizamos
                    if (!string.IsNullOrWhiteSpace(NuevaPassword))
                    {
                        var passwordHasher = new PasswordHasher<Usuario>();
                        usuarioDb.PasswordHash = passwordHasher.HashPassword(usuarioDb, NuevaPassword);
                    }

                    // Guardar los cambios
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuarioInput.UsuarioId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RolId"] = new SelectList(_context.Roles, "RolId", "Nombre", usuarioInput.RolId);
            return View(usuarioInput);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(m => m.UsuarioId == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }
    }
}
