import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface EnderecoRequest {
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  estado: string;
  cep: string;
}

export interface CadastrarUsuarioRequest {
  nome: string;
  cpf: string;
  email: string;
  telefone: string;
  senha: string;
  enderecos: EnderecoRequest[];
}

export interface AtualizarUsuarioRequest {
  nome: string;
  cpf: string;
  email: string;
  telefone: string;
  senha?: string;
  enderecos: EnderecoRequest[];
}

export interface EnderecoResponse {
  id: number;
  logradouro: string;
  numero: string;
  complemento: string;
  bairro: string;
  cidade: string;
  estado: string;
  cep: string;
}

export interface CadastrarUsuarioResponse {
  id: number;
  nome: string;
  cpf: string;
  email: string;
  telefone: string;
  enderecos: EnderecoResponse[];
}

export interface Usuario {
  id: number;
  nome: string;
  cpf: string;
  email: string;
  telefone: string;
  enderecos: EnderecoResponse[];
}

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  cadastrarUsuario(usuario: CadastrarUsuarioRequest): Observable<CadastrarUsuarioResponse> {
    return this.http.post<CadastrarUsuarioResponse>(`${this.apiUrl}/api/user/register`, usuario);
  }

  listarUsuarios(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(`${this.apiUrl}/api/user/list`);
  }

  obterUsuarioPorId(id: number): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.apiUrl}/api/user/${id}`);
  }

  atualizarUsuario(id: number, usuario: AtualizarUsuarioRequest): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.apiUrl}/api/user/${id}`, usuario);
  }
}
