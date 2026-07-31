using GymMax.Domain.Entities;
using GymMax.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymMax.Services.Usuarios {
    public interface IUsuarioService {
        Task<List<Usuario>> GetAllAsync(
            string? nombre,
            int? rolId,
            EstadoUsuario? estado,
            DateOnly? fechaDesde,
            DateOnly? fechaHasta
            );

        Task<Usuario?> GetByIdAsync(int id);

        Task<Usuario?> GetForEditAsync(int id);

        Task<bool> CreateAsync(Usuario usuario, string password);

        Task<bool> UpdateAsync( Usuario usuarioInput, string? nuevaPassword);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);

        Task<SelectList> GetRolesSelectListAsync(int? selected = null);
    }
}
