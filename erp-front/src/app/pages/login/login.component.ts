import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      senha: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      
      const { email, senha } = this.loginForm.value;
      
      this.authService.login(email, senha).subscribe({
        next: (response) => {
          this.isLoading = false;
          console.log('Login realizado com sucesso:', response);
          this.router.navigate(['/home']);
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Erro no login:', error);
          
          if (error.status === 401) {
            this.errorMessage = 'Email ou senha incorretos';
          } else if (error.status === 400) {
            this.errorMessage = 'Dados inválidos';
          } else {
            this.errorMessage = 'Erro interno do servidor. Tente novamente.';
          }
        }
      });
    }
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }
}
