using erp_server.Models;

namespace erp_server.Services
{
    public interface IAuditoriaService
    {
        void LogarCadastroUsuario(Usuario usuario);
        void LogarAlteracaoUsuario(Usuario usuarioAnterior, Usuario usuarioAtual);
    }
}
