using Microsoft.AspNetCore.Mvc;
using erp_server.DTOs;
using erp_server.Services;

namespace erp_server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    sucesso = false, 
                    mensagem = "Dados inválidos",
                    erros = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var resultado = await _authService.LoginAsync(request);

            if (!resultado.Sucesso)
            {
                return Unauthorized(new { 
                    sucesso = false, 
                    mensagem = resultado.Mensagem 
                });
            }

            return Ok(new { 
                sucesso = true,
                token = resultado.Token,
                refreshToken = resultado.RefreshToken,
                expiresAt = resultado.ExpiresAt,
                usuario = resultado.Usuario,
                mensagem = resultado.Mensagem
            });
        }

        [HttpPost("validator-token")]
        public async Task<IActionResult> ValidarToken([FromBody] ValidarTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new { 
                    sucesso = false, 
                    mensagem = "Token é obrigatório" 
                });
            }

            var isValid = _authService.ValidarToken(request.Token);

            return Ok(new { 
                sucesso = true,
                valido = isValid,
                mensagem = isValid ? "Token válido" : "Token inválido"
            });
        }
    }

    public class ValidarTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
