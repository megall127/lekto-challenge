import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ModalComponent } from '../modal/modal.component';
import { AddressFormComponent } from '../adress-form/adress-form.component';
import { UsuarioService, Usuario, AtualizarUsuarioRequest } from '../../services/usuario.service';

@Component({
  selector: 'app-modal-cadastro-usuario',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ModalComponent, AddressFormComponent],
  templateUrl: './modal-cadastro-usuario.html',
  styleUrls: ['./modal-cadastro-usuario.css']
})
export class ModalCadastroUsuarioComponent implements OnInit {
  @Input() isOpen = false;
  @Input() usuarioParaEditar: Usuario | null = null; // Novo input para edição
  @Output() close = new EventEmitter<void>();
  @Output() usuarioCadastrado = new EventEmitter<any>();
  @Output() usuarioAtualizado = new EventEmitter<any>();

  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  addresses: any[] = [];
  
  get isEdicao(): boolean {
    return this.usuarioParaEditar !== null;
  }
  
  get tituloModal(): string {
    return this.isEdicao ? 'Editar Usuário' : 'Cadastrar Novo Usuário';
  }

  constructor(
    private fb: FormBuilder,
    private usuarioService: UsuarioService
  ) {
    this.registerForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(4), this.noNumbersValidator]],
      cpf: ['', [Validators.required, this.cpfValidator]],
      email: ['', [Validators.required, this.emailValidator]],
      phone: ['', [Validators.required, this.phoneValidator]],
      password: ['', [Validators.minLength(6), this.passwordValidator]], // Required removido para permitir edição sem senha
      confirmPassword: ['']
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit() {
    // Reset form when modal opens
    if (this.isOpen) {
      this.resetForm();
    }
  }

  ngOnChanges() {
    if (this.isOpen) {
      this.resetForm();
      
      // Se for edição, carregar dados do usuário
      if (this.isEdicao && this.usuarioParaEditar) {
        this.carregarDadosUsuario();
      }
    }
  }

  resetForm() {
    this.registerForm.reset();
    this.addresses = [];
    this.errorMessage = '';
    this.successMessage = '';
    this.isLoading = false;
    
    // Ajustar validadores baseado no modo
    this.ajustarValidadores();
  }

  ajustarValidadores() {
    const passwordControl = this.registerForm.get('password');
    const confirmPasswordControl = this.registerForm.get('confirmPassword');
    
    if (this.isEdicao) {
      // Na edição, senha é opcional
      passwordControl?.clearValidators();
      passwordControl?.setValidators([Validators.minLength(6), this.passwordValidator]);
      confirmPasswordControl?.clearValidators();
    } else {
      // No cadastro, senha é obrigatória
      passwordControl?.setValidators([Validators.required, Validators.minLength(6), this.passwordValidator]);
      confirmPasswordControl?.setValidators([Validators.required]);
    }
    
    passwordControl?.updateValueAndValidity();
    confirmPasswordControl?.updateValueAndValidity();
  }

  carregarDadosUsuario() {
    if (!this.usuarioParaEditar) return;
    
    this.registerForm.patchValue({
      name: this.usuarioParaEditar.nome,
      cpf: this.formatarCPF(this.usuarioParaEditar.cpf),
      email: this.usuarioParaEditar.email,
      phone: this.formatarTelefone(this.usuarioParaEditar.telefone)
    });
    
    // Carregar endereços
    this.addresses = this.usuarioParaEditar.enderecos.map(endereco => ({
      street: endereco.logradouro,
      number: endereco.numero,
      complement: endereco.complemento,
      neighborhood: endereco.bairro,
      city: endereco.cidade,
      state: endereco.estado,
      zipcode: endereco.cep
    }));
  }

  formatarCPF(cpf: string): string {
    if (!cpf) return '';
    const c = cpf.replace(/\D/g, '');
    return c.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
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

  onAddressesChange(addresses: any) {
    this.addresses = addresses;
  }

  closeModal() {
    this.resetForm();
    this.close.emit();
  }

  // Validador personalizado para não permitir números no nome
  noNumbersValidator(control: any) {
    if (!control.value) {
      return null;
    }
    const hasNumbers = /\d/.test(control.value);
    return hasNumbers ? { hasNumbers: true } : null;
  }

  // Validador personalizado para CPF
  cpfValidator(control: any) {
    if (!control.value) {
      return null;
    }

    // Remove formatação
    const cpf = control.value.replace(/\D/g, '');

    // Verifica se tem 11 dígitos e se não são todos iguais
    if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
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

    return null;
  }

  // Validador personalizado para email
  emailValidator(control: any) {
    if (!control.value) {
      return null;
    }

    const email = control.value.toLowerCase().trim();
    
    // Regex mais rigoroso para email
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    
    if (!emailRegex.test(email)) {
      return { invalidEmail: true };
    }

    // Verificações adicionais
    if (email.includes('..')) {
      return { invalidEmail: true };
    }

    const [localPart, domainPart] = email.split('@');
    
    if (localPart.startsWith('.') || localPart.endsWith('.')) {
      return { invalidEmail: true };
    }

    if (domainPart.startsWith('-') || domainPart.endsWith('-')) {
      return { invalidEmail: true };
    }

    if (domainPart.length < 3) {
      return { invalidEmail: true };
    }

    return null;
  }

  // Validador personalizado para telefone
  phoneValidator(control: any) {
    if (!control.value) {
      return null;
    }

    const phone = control.value.replace(/\D/g, '');

    // Verifica se tem 10 ou 11 dígitos
    if (phone.length !== 10 && phone.length !== 11) {
      return { invalidPhone: true };
    }

    // Verifica DDD válido
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

    // Para celular (11 dígitos), o primeiro dígito após DDD deve ser 9
    if (phone.length === 11 && parseInt(phone.charAt(2)) !== 9) {
      return { invalidPhone: true };
    }

    // Para fixo (10 dígitos), o primeiro dígito após DDD deve ser 2, 3, 4 ou 5
    if (phone.length === 10) {
      const firstDigit = parseInt(phone.charAt(2));
      if (firstDigit < 2 || firstDigit > 5) {
        return { invalidPhone: true };
      }
    }

    return null;
  }

  // Validador personalizado para senha
  passwordValidator(control: any) {
    if (!control.value) {
      return null;
    }

    const password = control.value;
    const errors: any = {};

    // Verificar se tem pelo menos uma letra minúscula
    if (!/[a-z]/.test(password)) {
      errors.noLowerCase = true;
    }

    // Verificar se tem pelo menos uma letra maiúscula
    if (!/[A-Z]/.test(password)) {
      errors.noUpperCase = true;
    }

    // Verificar se tem pelo menos um caractere especial
    if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
      errors.noSpecialChar = true;
    }

    return Object.keys(errors).length > 0 ? errors : null;
  }

  // Validador para confirmar senha
  passwordMatchValidator(group: FormGroup) {
    const password = group.get('password');
    const confirmPassword = group.get('confirmPassword');

    if (!password || !confirmPassword) {
      return null;
    }

    return password.value === confirmPassword.value ? null : { passwordMismatch: true };
  }

  onSubmit() {
    // Validação especial para edição (senha opcional)
    const isFormValid = this.isEdicao ? 
      this.registerForm.valid || (this.registerForm.invalid && this.registerForm.get('password')?.invalid && !this.registerForm.value.password) :
      this.registerForm.valid;

    if (isFormValid && this.addresses.length > 0) {
      this.isLoading = true;
      this.errorMessage = '';

      const userData = {
        nome: this.registerForm.value.name,
        cpf: this.registerForm.value.cpf.replace(/\D/g, ''), // Remove formatação do CPF
        email: this.registerForm.value.email,
        telefone: this.registerForm.value.phone.replace(/\D/g, ''), // Remove formatação do telefone
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

      if (this.isEdicao) {
        // Edição de usuário
        const updateData: AtualizarUsuarioRequest = {
          ...userData,
          ...(this.registerForm.value.password && { senha: this.registerForm.value.password })
        };

        this.usuarioService.atualizarUsuario(this.usuarioParaEditar!.id, updateData).subscribe({
          next: (response) => {
            this.isLoading = false;
            this.successMessage = 'Usuário atualizado com sucesso!';
            
            // Emitir evento para o componente pai
            this.usuarioAtualizado.emit(response);
            
            // Fechar modal após 2 segundos
            setTimeout(() => {
              this.closeModal();
            }, 2000);
          },
          error: (error) => {
            this.isLoading = false;
            this.handleError(error, 'atualização');
          }
        });
      } else {
        // Cadastro de novo usuário
        const registerData = {
          ...userData,
          senha: this.registerForm.value.password
        };

        this.usuarioService.cadastrarUsuario(registerData).subscribe({
          next: (response) => {
            this.isLoading = false;
            this.successMessage = 'Usuário cadastrado com sucesso!';
            
            // Emitir evento para o componente pai
            this.usuarioCadastrado.emit(response);
            
            // Fechar modal após 2 segundos
            setTimeout(() => {
              this.closeModal();
            }, 2000);
          },
          error: (error) => {
            this.isLoading = false;
            this.handleError(error, 'cadastro');
          }
        });
      }
    } else {
      if (this.isEdicao) {
        this.errorMessage = 'Preencha todos os campos obrigatórios e adicione pelo menos um endereço.';
      } else {
        this.errorMessage = 'Preencha todos os campos obrigatórios, incluindo a senha, e adicione pelo menos um endereço.';
      }
    }
  }

  private handleError(error: any, operacao: string) {
    console.error(`Erro na ${operacao}:`, error);
    
    if (error.status === 400) {
      if (error.error && error.error.message) {
        this.errorMessage = error.error.message;
      } else if (error.error && typeof error.error === 'object') {
        // Processar erros de validação
        const validationErrors = Object.values(error.error).flat();
        this.errorMessage = validationErrors.join(', ');
      } else {
        this.errorMessage = `Dados inválidos. Verifique os campos e tente novamente.`;
      }
    } else if (error.status === 404) {
      this.errorMessage = 'Usuário não encontrado.';
    } else if (error.status === 500) {
      this.errorMessage = 'Erro interno do servidor. Tente novamente mais tarde.';
    } else {
      this.errorMessage = 'Erro inesperado. Tente novamente.';
    }
  }
}