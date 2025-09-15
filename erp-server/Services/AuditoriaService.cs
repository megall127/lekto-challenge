using erp_server.Models;

namespace erp_server.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(ILogger<AuditoriaService> logger)
        {
            _logger = logger;
        }

        public void LogarCadastroUsuario(Usuario usuario)
        {
            _logger.LogInformation("NOVO USUÁRIO CADASTRADO - ID: {Id} | Nome: {Nome} | CPF: {CPF} | Email: {Email} | Telefone: {Telefone} | Endereços: {EnderecosCount}",
                usuario.Id,
                usuario.Nome,
                usuario.CPF,
                usuario.Email,
                usuario.Telefone,
                usuario.Enderecos.Count);
        }

        public void LogarAlteracaoUsuario(Usuario usuarioAnterior, Usuario usuarioAtual)
        {
            var alteracoes = new List<string>();

            // Verificar alterações nos dados básicos
            if (usuarioAnterior.Nome != usuarioAtual.Nome)
            {
                alteracoes.Add($"Nome: '{usuarioAnterior.Nome}' -> '{usuarioAtual.Nome}'");
            }

            if (usuarioAnterior.CPF != usuarioAtual.CPF)
            {
                alteracoes.Add($"CPF: '{usuarioAnterior.CPF}' -> '{usuarioAtual.CPF}'");
            }

            if (usuarioAnterior.Email != usuarioAtual.Email)
            {
                alteracoes.Add($"Email: '{usuarioAnterior.Email}' -> '{usuarioAtual.Email}'");
            }

            if (usuarioAnterior.Telefone != usuarioAtual.Telefone)
            {
                alteracoes.Add($"Telefone: '{usuarioAnterior.Telefone}' -> '{usuarioAtual.Telefone}'");
            }

            // Verificar alterações nos endereços
            if (usuarioAnterior.Enderecos.Count != usuarioAtual.Enderecos.Count)
            {
                alteracoes.Add($"Quantidade de Endereços: {usuarioAnterior.Enderecos.Count} -> {usuarioAtual.Enderecos.Count}");
            }

            // Verificar alterações detalhadas nos endereços
            var enderecosAnteriores = usuarioAnterior.Enderecos.OrderBy(e => e.Id).ToList();
            var enderecosAtuais = usuarioAtual.Enderecos.OrderBy(e => e.Id).ToList();

            for (int i = 0; i < Math.Max(enderecosAnteriores.Count, enderecosAtuais.Count); i++)
            {
                var enderecoAnterior = i < enderecosAnteriores.Count ? enderecosAnteriores[i] : null;
                var enderecoAtual = i < enderecosAtuais.Count ? enderecosAtuais[i] : null;

                if (enderecoAnterior == null)
                {
                    alteracoes.Add($"Endereço {i + 1} ADICIONADO: {FormatarEndereco(enderecoAtual)}");
                }
                else if (enderecoAtual == null)
                {
                    alteracoes.Add($"Endereço {i + 1} REMOVIDO: {FormatarEndereco(enderecoAnterior)}");
                }
                else
                {
                    var alteracoesEndereco = VerificarAlteracoesEndereco(enderecoAnterior, enderecoAtual, i + 1);
                    alteracoes.AddRange(alteracoesEndereco);
                }
            }

            if (alteracoes.Any())
            {
                var logMessage = $"USUÁRIO ALTERADO - ID: {usuarioAtual.Id} | Alterações: {string.Join(" | ", alteracoes)}";
                _logger.LogInformation(logMessage);
            }
            else
            {
                _logger.LogInformation("USUÁRIO ALTERADO - ID: {Id} | Nenhuma alteração detectada", usuarioAtual.Id);
            }
        }

        private List<string> VerificarAlteracoesEndereco(Endereco enderecoAnterior, Endereco enderecoAtual, int numeroEndereco)
        {
            var alteracoes = new List<string>();

            if (enderecoAnterior.Logradouro != enderecoAtual.Logradouro)
            {
                alteracoes.Add($"Endereço {numeroEndereco} - Logradouro: '{enderecoAnterior.Logradouro}' -> '{enderecoAtual.Logradouro}'");
            }

            if (enderecoAnterior.Numero != enderecoAtual.Numero)
            {
                alteracoes.Add($"Endereço {numeroEndereco} - Número: '{enderecoAnterior.Numero}' -> '{enderecoAtual.Numero}'");
            }

            if (enderecoAnterior.Complemento != enderecoAtual.Complemento)
            {
                alteracoes.Add($"Endereço {numeroEndereco} - Complemento: '{enderecoAnterior.Complemento}' -> '{enderecoAtual.Complemento}'");
            }

            if (enderecoAnterior.Bairro != enderecoAtual.Bairro)
            {
                alteracoes.Add($"Endereço {numeroEndereco} - Bairro: '{enderecoAnterior.Bairro}' -> '{enderecoAtual.Bairro}'");
            }

            if (enderecoAnterior.Cidade != enderecoAtual.Cidade)
            {
                alteracoes.Add($"Endereço {numeroEndereco} - Cidade: '{enderecoAnterior.Cidade}' -> '{enderecoAtual.Cidade}'");
            }

            if (enderecoAnterior.Estado != enderecoAtual.Estado)
            {
                alteracoes.Add($"Endereço {numeroEndereco} - Estado: '{enderecoAnterior.Estado}' -> '{enderecoAtual.Estado}'");
            }

            if (enderecoAnterior.CEP != enderecoAtual.CEP)
            {
                alteracoes.Add($"Endereço {numeroEndereco} - CEP: '{enderecoAnterior.CEP}' -> '{enderecoAtual.CEP}'");
            }

            return alteracoes;
        }

        private string FormatarEndereco(Endereco endereco)
        {
            return $"{endereco.Logradouro}, {endereco.Numero}, {endereco.Complemento}, {endereco.Bairro}, {endereco.Cidade}/{endereco.Estado}, CEP: {endereco.CEP}";
        }
    }
}
