using GymMax.Enums;
using GymMax.ViewModels;
using System.Security.Claims;

namespace GymMax.Services.Auth {
    public interface IAuthService {
        Task<ClaimsPrincipal?> AutenticarAsync(LoginViewModel model);
        Task<ResultadoRegistro> RegistrarAsync(RegistroViewModel model);
    }
}
