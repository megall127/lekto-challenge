using System.ComponentModel.DataAnnotations;
using erp_server.Attributes;

namespace erp_server.DTOs
{
    public class AtualizarUsuarioRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CPF é obrigatório")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter exatamente 11 dígitos")]
        public string CPF { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Telefone é obrigatório")]
        [TelefoneValido]
        public string Telefone { get; set; } = string.Empty;
        
        // Senha é opcional na edição
        public string? Senha { get; set; }
        
        [Required(ErrorMessage = "Pelo menos um endereço é obrigatório")]
        public List<EnderecoRequest> Enderecos { get; set; } = new List<EnderecoRequest>();
    }
}
