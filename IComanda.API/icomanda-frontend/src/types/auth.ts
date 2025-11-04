export interface LoginRequest {
  nome: string;
  senha: string;
}

export interface LoginResponse {
  id: number;
  nome: string;
  token: string;
  tipo: string;
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
}

export interface Usuario {
  id: number;
  nome: string;
  tipo: string;
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
}