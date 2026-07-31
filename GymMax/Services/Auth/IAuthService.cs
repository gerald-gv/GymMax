using GymMax.Models;
using System.Security.Claims;

namespace GymMax.Services.Auth {
    public interface IAuthService {
        Task<ClaimsPrincipal?> AutenticarAsync(LoginViewModel model);
    }
}
