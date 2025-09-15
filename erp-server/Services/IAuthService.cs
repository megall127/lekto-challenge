using erp_server.DTOs;

namespace erp_server.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        bool ValidarToken(string token);
        string GerarToken(int usuarioId, string email);
        string GerarRefreshToken();
    }
}
