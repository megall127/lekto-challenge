using erp_server.DTOs;
using erp_server.Models;
using erp_server.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace erp_server.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ErpDbContext _context;
        private readonly IAuditoriaService _auditoriaService;

        public UsuarioService(ErpDbContext context, IAuditoriaService auditoriaService)
        {
            _context = context;
            _auditoriaService = auditoriaService;
        }

        public async Task<CadastrarUsuarioResponse?> CadastrarUsuarioAsync(CadastrarUsuarioRequest request)
        {
            // Verificar se já existe um usuário com dados idênticos
            if (await VerificarUsuarioDuplicadoAsync(request))
            {
                return null;
            }

            if (await VerificarCPFExistenteAsync(request.CPF))
            {
                return null; 
            }
            if (await VerificarEmailExistenteAsync(request.Email))
            {
                return null;
            }

            var usuario = new Usuario
            {
                Nome = request.Nome,
                CPF = request.CPF,
                Email = request.Email,
                Telefone = request.Telefone,
                Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha), // Hash da senha
                Enderecos = new List<Endereco>()
            };

            foreach (var enderecoRequest in request.Enderecos)
            {
                var endereco = new Endereco
                {
                    Logradouro = enderecoRequest.Logradouro,
                    Numero = enderecoRequest.Numero,
                    Complemento = enderecoRequest.Complemento,
                    Bairro = enderecoRequest.Bairro,
                    Cidade = enderecoRequest.Cidade,
                    Estado = enderecoRequest.Estado,
                    CEP = enderecoRequest.CEP
                };

                usuario.Enderecos.Add(endereco);
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Log de auditoria
            _auditoriaService.LogarCadastroUsuario(usuario);

            return new CadastrarUsuarioResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                CPF = usuario.CPF,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                Enderecos = usuario.Enderecos.Select(e => new EnderecoResponse
                {
                    Id = e.Id,
                    Logradouro = e.Logradouro,
                    Numero = e.Numero,
                    Complemento = e.Complemento,
                    Bairro = e.Bairro,
                    Cidade = e.Cidade,
                    Estado = e.Estado,
                    CEP = e.CEP
                }).ToList()
            };
        }

        public async Task<List<ListarUsuariosResponse>> ListarUsuariosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Enderecos)
                .ToListAsync();

            return usuarios.Select(u => new ListarUsuariosResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                CPF = u.CPF,
                Email = u.Email,
                Telefone = u.Telefone,
                Enderecos = u.Enderecos.Select(e => new EnderecoListagemDto
                {
                    Id = e.Id,
                    Logradouro = e.Logradouro,
                    Numero = e.Numero,
                    Complemento = e.Complemento,
                    Bairro = e.Bairro,
                    Cidade = e.Cidade,
                    Estado = e.Estado,
                    CEP = e.CEP
                }).ToList()
            }).ToList();
        }

        public async Task<bool> VerificarCPFExistenteAsync(string cpf, int? usuarioIdExcluir = null)
        {
            var query = _context.Usuarios.Where(u => u.CPF == cpf);
            if (usuarioIdExcluir.HasValue)
            {
                query = query.Where(u => u.Id != usuarioIdExcluir.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<bool> VerificarEmailExistenteAsync(string email, int? usuarioIdExcluir = null)
        {
            var query = _context.Usuarios.Where(u => u.Email == email);
            if (usuarioIdExcluir.HasValue)
            {
                query = query.Where(u => u.Id != usuarioIdExcluir.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<bool> VerificarEnderecoDuplicadoAsync(int usuarioId, EnderecoRequest endereco)
        {
            return await _context.Enderecos.AnyAsync(e => 
                e.UsuarioId == usuarioId &&
                e.Logradouro == endereco.Logradouro &&
                e.Numero == endereco.Numero &&
                e.Bairro == endereco.Bairro &&
                e.Cidade == endereco.Cidade &&
                e.Estado == endereco.Estado &&
                e.CEP == endereco.CEP);
        }

        public async Task<bool> VerificarUsuarioDuplicadoAsync(CadastrarUsuarioRequest request)
        {
            // Verificar se existe um usuário com exatamente os mesmos dados
            var usuarioExistente = await _context.Usuarios
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => 
                    u.Nome == request.Nome &&
                    u.CPF == request.CPF &&
                    u.Email == request.Email &&
                    u.Telefone == request.Telefone);

            if (usuarioExistente == null)
            {
                return false;
            }

            // Verificar se os endereços também são idênticos
            if (usuarioExistente.Enderecos.Count != request.Enderecos.Count)
            {
                return false;
            }

            foreach (var enderecoRequest in request.Enderecos)
            {
                var enderecoCorrespondente = usuarioExistente.Enderecos.FirstOrDefault(e =>
                    e.Logradouro == enderecoRequest.Logradouro &&
                    e.Numero == enderecoRequest.Numero &&
                    e.Complemento == enderecoRequest.Complemento &&
                    e.Bairro == enderecoRequest.Bairro &&
                    e.Cidade == enderecoRequest.Cidade &&
                    e.Estado == enderecoRequest.Estado &&
                    e.CEP == enderecoRequest.CEP);

                if (enderecoCorrespondente == null)
                {
                    return false;
                }
            }

            return true; // Usuário com dados idênticos encontrado
        }

        public async Task<ListarUsuariosResponse?> ObterUsuarioPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return null;
            }

            return new ListarUsuariosResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                CPF = usuario.CPF,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                Enderecos = usuario.Enderecos.Select(e => new EnderecoListagemDto
                {
                    Id = e.Id,
                    Logradouro = e.Logradouro,
                    Numero = e.Numero,
                    Complemento = e.Complemento,
                    Bairro = e.Bairro,
                    Cidade = e.Cidade,
                    Estado = e.Estado,
                    CEP = e.CEP
                }).ToList()
            };
        }

        public async Task<ListarUsuariosResponse?> AtualizarUsuarioAsync(int id, AtualizarUsuarioRequest request)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Enderecos)
                .FirstOrDefaultAsync(u => u.Id == id);
            
            if (usuario == null)
            {
                return null;
            }

            // Capturar estado anterior para auditoria
            var usuarioAnterior = new Usuario
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                CPF = usuario.CPF,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                Enderecos = usuario.Enderecos.Select(e => new Endereco
                {
                    Id = e.Id,
                    Logradouro = e.Logradouro,
                    Numero = e.Numero,
                    Complemento = e.Complemento,
                    Bairro = e.Bairro,
                    Cidade = e.Cidade,
                    Estado = e.Estado,
                    CEP = e.CEP,
                    UsuarioId = e.UsuarioId
                }).ToList()
            };

            // Verificar se CPF já existe em outro usuário
            if (await VerificarCPFExistenteAsync(request.CPF, id))
            {
                return null;
            }

            // Verificar se Email já existe em outro usuário
            if (await VerificarEmailExistenteAsync(request.Email, id))
            {
                return null;
            }

            // Atualizar dados do usuário
            usuario.Nome = request.Nome;
            usuario.CPF = request.CPF;
            usuario.Email = request.Email;
            usuario.Telefone = request.Telefone;

            // Atualizar senha apenas se fornecida
            if (!string.IsNullOrEmpty(request.Senha))
            {
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha);
            }

            // Remover endereços existentes
            _context.Enderecos.RemoveRange(usuario.Enderecos);

            // Adicionar novos endereços
            usuario.Enderecos.Clear();
            foreach (var enderecoRequest in request.Enderecos)
            {
                var endereco = new Endereco
                {
                    Logradouro = enderecoRequest.Logradouro,
                    Numero = enderecoRequest.Numero,
                    Complemento = enderecoRequest.Complemento,
                    Bairro = enderecoRequest.Bairro,
                    Cidade = enderecoRequest.Cidade,
                    Estado = enderecoRequest.Estado,
                    CEP = enderecoRequest.CEP,
                    UsuarioId = id
                };

                usuario.Enderecos.Add(endereco);
            }

            await _context.SaveChangesAsync();

            _auditoriaService.LogarAlteracaoUsuario(usuarioAnterior, usuario);

            return new ListarUsuariosResponse
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                CPF = usuario.CPF,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                Enderecos = usuario.Enderecos.Select(e => new EnderecoListagemDto
                {
                    Id = e.Id,
                    Logradouro = e.Logradouro,
                    Numero = e.Numero,
                    Complemento = e.Complemento,
                    Bairro = e.Bairro,
                    Cidade = e.Cidade,
                    Estado = e.Estado,
                    CEP = e.CEP
                }).ToList()
            };
        }
    }
}
