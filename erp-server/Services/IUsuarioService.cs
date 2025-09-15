using erp_server.DTOs;
using erp_server.Models;

namespace erp_server.Services
{
    public interface IUsuarioService
    {
        Task<CadastrarUsuarioResponse?> CadastrarUsuarioAsync(CadastrarUsuarioRequest request);
        Task<ListarUsuariosResponse?> AtualizarUsuarioAsync(int id, AtualizarUsuarioRequest request);
        Task<List<ListarUsuariosResponse>> ListarUsuariosAsync();
        Task<ListarUsuariosResponse?> ObterUsuarioPorIdAsync(int id);
        Task<bool> VerificarCPFExistenteAsync(string cpf, int? usuarioIdExcluir = null);
        Task<bool> VerificarEmailExistenteAsync(string email, int? usuarioIdExcluir = null);
        Task<bool> VerificarEnderecoDuplicadoAsync(int usuarioId, EnderecoRequest endereco);
        Task<bool> VerificarUsuarioDuplicadoAsync(CadastrarUsuarioRequest request);
    }
}
