using Microsoft.AspNetCore.Mvc;
using erp_server.DTOs;
using erp_server.Services;

namespace erp_server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> CadastrarUsuario([FromBody] CadastrarUsuarioRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _usuarioService.CadastrarUsuarioAsync(request);

            if (resultado == null)
            {
                return BadRequest(new { message = "CPF, Email já cadastrado ou usuário com dados idênticos já existe no sistema" });
            }

            return CreatedAtAction(nameof(CadastrarUsuario), new { id = resultado.Id }, resultado);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListarUsuarios()
        {
            try
            {
                var usuarios = await _usuarioService.ListarUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObterUsuarioPorId(int id)
        {
            try
            {
                var usuario = await _usuarioService.ObterUsuarioPorIdAsync(id);
                
                if (usuario == null)
                {
                    return NotFound(new { message = "Usuário não encontrado" });
                }

                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarUsuario(int id, [FromBody] AtualizarUsuarioRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var usuarioAtualizado = await _usuarioService.AtualizarUsuarioAsync(id, request);

                if (usuarioAtualizado == null)
                {
                    // Verificar se o usuário existe
                    var usuarioExiste = await _usuarioService.ObterUsuarioPorIdAsync(id);
                    if (usuarioExiste == null)
                    {
                        return NotFound(new { message = "Usuário não encontrado" });
                    }
                    else
                    {
                        return BadRequest(new { message = "CPF ou Email já estão em uso por outro usuário" });
                    }
                }

                return Ok(usuarioAtualizado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", details = ex.Message });
            }
        }
    }
}
