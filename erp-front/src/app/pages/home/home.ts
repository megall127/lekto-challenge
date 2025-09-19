import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { UsuarioService, Usuario } from '../../services/usuario.service';
import { AuthService } from '../../services/auth.service';
import { ModalCadastroUsuarioComponent } from '../../components/modal-cadastro-usuario/modal-cadastro-usuario';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterModule, ModalCadastroUsuarioComponent],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit {
  usuarios: Usuario[] = [];
  isLoading = false;
  errorMessage = '';
  usuarioLogado: any = null;
  isAuthenticated = false;
  
  isModalCadastroOpen = false;
  usuarioParaEditar: Usuario | null = null;

  constructor(
    private usuarioService: UsuarioService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.verificarAutenticacao();
    this.carregarUsuarios();
  }

  verificarAutenticacao() {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.usuarioLogado = this.authService.getUsuario();
    }
  }

  carregarUsuarios() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.usuarioService.listarUsuarios().subscribe({
      next: (usuarios) => {
        this.usuarios = usuarios;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Erro ao carregar usuários:', error);
        this.errorMessage = 'Erro ao carregar usuários';
        this.isLoading = false;
      }
    });
  }

  formatarTelefone(telefone: string): string {
    if (!telefone) return '';
    const tel = telefone.replace(/\D/g, '');
    if (tel.length === 11) {
      return `(${tel.slice(0, 2)}) ${tel.slice(2, 7)}-${tel.slice(7)}`;
    } else if (tel.length === 10) {
      return `(${tel.slice(0, 2)}) ${tel.slice(2, 6)}-${tel.slice(6)}`;
    }
    return telefone;
  }

  formatarCPF(cpf: string): string {
    if (!cpf) return '';
    const c = cpf.replace(/\D/g, '');
    return c.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
  }

  abrirModalCadastro() {
    this.usuarioParaEditar = null;
    this.isModalCadastroOpen = true;
  }

  abrirModalEdicao(usuario: Usuario) {
    this.usuarioParaEditar = usuario;
    this.isModalCadastroOpen = true;
  }

  fecharModalCadastro() {
    this.isModalCadastroOpen = false;
    this.usuarioParaEditar = null;
  }

  onUsuarioCadastrado(novoUsuario: any) {
    this.carregarUsuarios();
  }

  onUsuarioAtualizado(usuarioAtualizado: any) {
    this.carregarUsuarios();
  }

  irParaCadastro() {
    this.router.navigate(['/register']);
  }

  irParaLogin() {
    this.router.navigate(['/login']);
  }

  logout() {
    this.authService.logout();
    this.isAuthenticated = false;
    this.usuarioLogado = null;
    this.router.navigate(['/login']);
  }

  getTotalEnderecos(): number {
    return this.usuarios.reduce((total, usuario) => total + usuario.enderecos.length, 0);
  }
}
