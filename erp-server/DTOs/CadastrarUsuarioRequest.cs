using System.ComponentModel.DataAnnotations;
using erp_server.Attributes;

namespace erp_server.DTOs
{
    public class CadastrarUsuarioRequest
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
        
        [Required(ErrorMessage = "Senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
        public string Senha { get; set; } = string.Empty;
        
        public List<EnderecoRequest> Enderecos { get; set; } = new List<EnderecoRequest>();
    }
    
    public class EnderecoRequest
    {
        [Required(ErrorMessage = "Logradouro é obrigatório")]
        public string Logradouro { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Número é obrigatório")]
        public string Numero { get; set; } = string.Empty;
        
        public string Complemento { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Bairro é obrigatório")]
        public string Bairro { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Cidade é obrigatória")]
        public string Cidade { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Estado é obrigatório")]
        public string Estado { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "CEP é obrigatório")]
        public string CEP { get; set; } = string.Empty;
    }
}
