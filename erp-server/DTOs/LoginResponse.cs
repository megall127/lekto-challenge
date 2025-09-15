namespace erp_server.DTOs
{
    public class LoginResponse
    {
        public bool Sucesso { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public UsuarioResponse? Usuario { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    public class UsuarioResponse
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<EnderecoResponse> Enderecos { get; set; } = new();
    }
}
