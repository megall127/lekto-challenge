import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {
  usuarioLogado: any = null;
  isAuthenticated = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.verificarAutenticacao();
  }

  verificarAutenticacao() {
    this.isAuthenticated = this.authService.isAuthenticated();
    if (this.isAuthenticated) {
      this.usuarioLogado = this.authService.getUsuario();
    }
  }

  logout() {
    this.authService.logout();
    this.isAuthenticated = false;
    this.usuarioLogado = null;
    this.router.navigate(['/login']);
  }
}
