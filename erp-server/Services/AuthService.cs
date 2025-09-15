using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using erp_server.Data;
using erp_server.DTOs;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace erp_server.Services
{
    public class AuthService : IAuthService
    {
        private readonly ErpDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ErpDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Buscar usuário por email
                var usuario = await _context.Usuarios
                    .Include(u => u.Enderecos)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (usuario == null)
                {
                    return new LoginResponse
                    {
                        Sucesso = false,
                        Mensagem = "Email ou senha inválidos"
                    };
                }

                // Verificar senha
                if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
                {
                    return new LoginResponse
                    {
                        Sucesso = false,
                        Mensagem = "Email ou senha inválidos"
                    };
                }

                // Gerar tokens
                var token = GerarToken(usuario.Id, usuario.Email);
                var refreshToken = GerarRefreshToken();

                // Converter endereços para DTO
                var enderecosResponse = usuario.Enderecos.Select(e => new EnderecoResponse
                {
                    Id = e.Id,
                    Logradouro = e.Logradouro,
                    Numero = e.Numero,
                    Complemento = e.Complemento,
                    Bairro = e.Bairro,
                    Cidade = e.Cidade,
                    Estado = e.Estado,
                    CEP = e.CEP
                }).ToList();

                return new LoginResponse
                {
                    Sucesso = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24), // Token válido por 24 horas
                    Usuario = new UsuarioResponse
                    {
                        Id = usuario.Id,
                        Nome = usuario.Nome,
                        CPF = usuario.CPF,
                        Email = usuario.Email,
                        Enderecos = enderecosResponse
                    },
                    Mensagem = "Login realizado com sucesso"
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro interno: {ex.Message}"
                };
            }
        }

        public bool ValidarToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada"));

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GerarToken(int usuarioId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey não configurada"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GerarRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
