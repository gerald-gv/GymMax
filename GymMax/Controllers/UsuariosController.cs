using GymMax.Data;
using GymMax.Domain.Entities;
using GymMax.Enums;
using GymMax.Models;
using GymMax.Services.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymMax.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        public UsuariosController(IUsuarioService usuarioService) {
            _usuarioService = usuarioService;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index(
            string? nombre,
            int? rolId,
            EstadoUsuario? estado,
            DateOnly? fechaDesde,
            DateOnly? fechaHasta
            ) {
            var usuarios = await _usuarioService.GetAllAsync(
                nombre,
                rolId,
                estado,
                fechaDesde,
                fechaHasta);

            ViewBag.FiltroNombre = nombre;

            ViewBag.FiltroRolId = await _usuarioService.GetRolesSelectListAsync(rolId);

            ViewBag.FiltroEstado = new SelectList(
                Enum.GetValues<EstadoUsuario>()
                    .Select(e => new {
                        Value = (int)e,
                        Text = e.ToString()
                    }),
                "Value",
                "Text",
                (int?)estado);

            ViewBag.FiltroDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = fechaHasta?.ToString("yyyy-MM-dd");

            return View(usuarios);
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id) {
            if (id == null) {
                return NotFound();
            }

            var usuario = await _usuarioService.GetByIdAsync(id.Value);

            if (usuario == null) {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public async Task<IActionResult> Create() {
            ViewData["RolId"] = await _usuarioService.GetRolesSelectListAsync();
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Usuario usuario,
            string Password
            ) {
            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid) {
                await _usuarioService.CreateAsync(usuario, Password);
                return RedirectToAction(nameof(Index));
            }

            ViewData["RolId"] = await _usuarioService.GetRolesSelectListAsync(usuario.RolId);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id) {
            if (id == null) {
                return NotFound();
            }

            var usuario = await _usuarioService.GetForEditAsync(id.Value);

            if (usuario == null) {
                return NotFound();
            }

            ViewData["RolId"] = await _usuarioService.GetRolesSelectListAsync(usuario.RolId);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("UsuarioId,RolId,Nombres,Apellidos,Dni,Email,Telefono,FechaNacimiento,CodigoMembresia,Estado")]
            Usuario usuarioInput,
            string? NuevaPassword
            ) {
            if (id != usuarioInput.UsuarioId) {
                return NotFound();
            }

            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid) {
                try {
                    var actualizado = await _usuarioService.UpdateAsync( usuarioInput, NuevaPassword);

                    if (!actualizado) {
                        return NotFound();
                    }
                } catch {
                    if (!await _usuarioService.ExistsAsync(usuarioInput.UsuarioId)) {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["RolId"] = await _usuarioService.GetRolesSelectListAsync(usuarioInput.RolId);
            return View(usuarioInput);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id) {
            if (id == null) {
                return NotFound();
            }

            var usuario = await _usuarioService.GetByIdAsync(id.Value);

            if (usuario == null) {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            await _usuarioService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
