import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AddressFormComponent } from '../../components/adress-form/adress-form.component';
import { UsuarioService } from '../../services/usuario.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, AddressFormComponent],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  addresses: any[] = [];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private usuarioService: UsuarioService
  ) {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(4), this.noNumbersValidator]],
      cpf: ['', [Validators.required, this.cpfValidator]],
      email: ['', [Validators.required, this.emailValidator]],
      phone: ['', [Validators.required, this.phoneValidator]],
      password: ['', [Validators.required, Validators.minLength(6), this.passwordValidator]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  onAddressesChange(addresses: any[]) {
    this.addresses = addresses;
  }

  // Validador personalizado para não permitir números no nome
  noNumbersValidator(control: any) {
    if (!control.value) {
      return null; // Se não há valor, deixa outros validadores lidarem
    }
    
    const hasNumbers = /\d/.test(control.value);
    return hasNumbers ? { hasNumbers: true } : null;
  }

  // Validador personalizado para CPF
  cpfValidator(control: any) {
    if (!control.value) {
      return null; // Se não há valor, deixa outros validadores lidarem
    }

    const cpf = control.value.replace(/\D/g, ''); // Remove caracteres não numéricos
    
    // Verifica se tem 11 dígitos
    if (cpf.length !== 11) {
      return { invalidCpf: true };
    }

    // Verifica se todos os dígitos são iguais (ex: 111.111.111-11)
    if (/^(\d)\1{10}$/.test(cpf)) {
      return { invalidCpf: true };
    }

    // Validação do primeiro dígito verificador
    let sum = 0;
    for (let i = 0; i < 9; i++) {
      sum += parseInt(cpf.charAt(i)) * (10 - i);
    }
    let firstDigit = 11 - (sum % 11);
    if (firstDigit >= 10) firstDigit = 0;

    if (parseInt(cpf.charAt(9)) !== firstDigit) {
      return { invalidCpf: true };
    }

    // Validação do segundo dígito verificador
    sum = 0;
    for (let i = 0; i < 10; i++) {
      sum += parseInt(cpf.charAt(i)) * (11 - i);
    }
    let secondDigit = 11 - (sum % 11);
    if (secondDigit >= 10) secondDigit = 0;

    if (parseInt(cpf.charAt(10)) !== secondDigit) {
      return { invalidCpf: true };
    }

    return null; // CPF válido
  }

  // Validador personalizado para email
  emailValidator(control: any) {
    if (!control.value) {
      return null; // Se não há valor, deixa outros validadores lidarem
    }

    const email = control.value.toLowerCase().trim();
    
    // Regex mais rigorosa para email
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    
    // Verifica formato básico
    if (!emailRegex.test(email)) {
      return { invalidEmail: true };
    }

    // Verifica se não tem pontos consecutivos
    if (email.includes('..')) {
      return { invalidEmail: true };
    }

    // Verifica se não começa ou termina com ponto antes do @
    const [localPart, domainPart] = email.split('@');
    if (localPart.startsWith('.') || localPart.endsWith('.')) {
      return { invalidEmail: true };
    }

    // Verifica se o domínio não começa ou termina com hífen
    if (domainPart.startsWith('-') || domainPart.endsWith('-')) {
      return { invalidEmail: true };
    }

    // Verifica se tem pelo menos 3 caracteres no domínio
    if (domainPart.length < 3) {
      return { invalidEmail: true };
    }

    return null; // Email válido
  }

  // Validador personalizado para telefone com DDD
  phoneValidator(control: any) {
    if (!control.value) {
      return null; // Se não há valor, deixa outros validadores lidarem
    }

    const phone = control.value.replace(/\D/g, ''); // Remove caracteres não numéricos
    
    // Verifica se tem 10 ou 11 dígitos (DDD + número)
    if (phone.length !== 10 && phone.length !== 11) {
      return { invalidPhone: true };
    }

    // Verifica se o DDD é válido (11 a 99, exceto alguns códigos não utilizados)
    const ddd = parseInt(phone.substring(0, 2));
    const validDDDs = [
      11, 12, 13, 14, 15, 16, 17, 18, 19, // SP
      21, 22, 24, // RJ/ES
      27, 28, // ES
      31, 32, 33, 34, 35, 37, 38, // MG
      41, 42, 43, 44, 45, 46, // PR
      47, 48, 49, // SC
      51, 53, 54, 55, // RS
      61, // DF/GO
      62, 64, // GO
      63, // TO
      65, 66, // MT
      67, // MS
      68, // AC
      69, // RO
      71, 73, 74, 75, 77, // BA
      79, // SE
      81, 87, // PE
      82, // AL
      83, // PB
      84, // RN
      85, 88, // CE
      86, 89, // PI
      91, 93, 94, // PA
      92, 97, // AM
      95, // RR
      96, // AP
      98, 99 // MA
    ];

    if (!validDDDs.includes(ddd)) {
      return { invalidDDD: true };
    }

    // Para números de 11 dígitos, o primeiro dígito após o DDD deve ser 9 (celular)
    if (phone.length === 11) {
      const firstDigitAfterDDD = parseInt(phone.charAt(2));
      if (firstDigitAfterDDD !== 9) {
        return { invalidPhone: true };
      }
    }

    // Para números de 10 dígitos, o primeiro dígito após o DDD deve ser 2-5 (fixo)
    if (phone.length === 10) {
      const firstDigitAfterDDD = parseInt(phone.charAt(2));
      if (firstDigitAfterDDD < 2 || firstDigitAfterDDD > 5) {
        return { invalidPhone: true };
      }
    }

    return null; // Telefone válido
  }

  // Validador personalizado para senha forte
  passwordValidator(control: any) {
    if (!control.value) {
      return null; // Se não há valor, deixa outros validadores lidarem
    }

    const password = control.value;
    const errors: any = {};

    // Verifica se tem pelo menos uma letra minúscula
    if (!/[a-z]/.test(password)) {
      errors.noLowerCase = true;
    }

    // Verifica se tem pelo menos uma letra maiúscula
    if (!/[A-Z]/.test(password)) {
      errors.noUpperCase = true;
    }

    // Verifica se tem pelo menos um caractere especial
    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
      errors.noSpecialChar = true;
    }

    // Se algum erro foi encontrado, retorna os erros
    return Object.keys(errors).length > 0 ? errors : null;
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  onSubmit() {
    if (this.registerForm.valid && this.addresses.length > 0) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';
      
      const registerData = {
        nome: this.registerForm.value.name,
        cpf: this.registerForm.value.cpf.replace(/\D/g, ''),
        email: this.registerForm.value.email,
        telefone: this.registerForm.value.phone,
        senha: this.registerForm.value.password,
        enderecos: this.addresses.map(address => ({
          logradouro: address.street,
          numero: address.number,
          complemento: address.complement || '',
          bairro: address.neighborhood,
          cidade: address.city,
          estado: address.state,
          cep: address.zipcode
        }))
      };
      
      console.log('Dados para cadastro:', registerData);
      

      this.usuarioService.cadastrarUsuario(registerData).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.successMessage = 'Cadastro realizado com sucesso!';
          console.log('Usuário cadastrado:', response);
          
    
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        },
        error: (error) => {
          this.isLoading = false;
          console.error('Erro no cadastro:', error);
          
          if (error.status === 400) {
            if (error.error?.message?.includes('CPF')) {
              this.errorMessage = 'CPF já cadastrado no sistema';
            } else if (error.error?.message?.includes('Email')) {
              this.errorMessage = 'Email já cadastrado no sistema';
            } else {
              this.errorMessage = 'Dados inválidos. Verifique os campos.';
            }
          } else {
            this.errorMessage = 'Erro interno do servidor. Tente novamente.';
          }
        }
      });
    } else if (this.addresses.length === 0) {
      this.errorMessage = 'Pelo menos um endereço é obrigatório';
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
