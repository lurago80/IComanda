export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  id: number;
  nome: string;
  token: string;
  refreshToken: string;
  tipo: string;
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
  expiresIn: number;
  userId: number;
  username: string;
  role: string;
  tokenType: string;
}

export interface Usuario {
  id: number;
  nome: string;
  role: string;
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
}