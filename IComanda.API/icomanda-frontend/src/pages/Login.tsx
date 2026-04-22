import React, { useState } from 'react';
import { Eye, EyeOff, AlertCircle } from 'lucide-react';
import api from '../services/api';

const Login: React.FC = () => {
  const [nome, setNome] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [erroLogin, setErroLogin] = useState<string | null>(null);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setErroLogin(null);
    if (nome.trim() && senha.trim()) {
      try {
        const response = await api.post('/auth/login', {
          username: nome.trim(),
          password: senha.trim()
        });

        const data = response.data;

        // Salva apenas informações de UI do usuário (token fica em cookie httpOnly)
        const usuarioInfo = {
          nome: data.username,
          id: data.userId,
          role: data.role,
          podeVisualizar: data.podeVisualizar,
          podeVerTotal: data.podeVerTotal,
          podeCancelar: data.podeCancelar,
          dataLogin: new Date().toISOString()
        };
        localStorage.setItem('usuario_logado', JSON.stringify(usuarioInfo));

        // Salva o refresh token para renovação automática do acesso
        if (data.refreshToken) {
          localStorage.setItem('refresh_token', data.refreshToken);
        }

        // Salva o tempo de expiração do JWT para checagem proativa
        if (data.expiresIn) {
          localStorage.setItem('jwt_expires_at', String(Date.now() + data.expiresIn * 3600000));
        }

        window.location.reload();
      } catch (err: any) {
        const errorMsg = err.response?.data?.error || err.message || 'Verifique se o backend está rodando';
        setErroLogin(errorMsg);
      }
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5 flex items-center justify-center p-4">
      {/* Pontos decorativos de fundo */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute top-1/4 left-1/4 w-64 h-64 bg-primary/5 rounded-full blur-3xl" />
        <div className="absolute bottom-1/4 right-1/4 w-48 h-48 bg-primary/8 rounded-full blur-2xl" />
      </div>

      <div className="w-full max-w-md relative">
        <div className="bg-card rounded-3xl shadow-2xl p-8 border border-border">
          {/* Logo e Título */}
          <div className="text-center mb-8">
            <div className="h-36 rounded-2xl flex items-center justify-center mx-auto mb-4 overflow-hidden px-8 py-4 inline-flex">
              <img src="/iComanda.jpg" alt="iComanda Logo" className="h-full w-auto object-contain" />
            </div>
            <p className="text-text-secondary font-medium text-lg">Sistema de Pedidos - Comandas</p>
          </div>

          {/* Formulário */}
          <form onSubmit={handleLogin} className="space-y-6">
            <div>
              <label 
                htmlFor="nome" 
                className="block text-sm font-semibold text-text-primary mb-2"
              >
                Nome do Usuário
              </label>
              <input
                id="nome"
                type="text"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                placeholder="Digite seu nome"
                className="w-full px-4 py-3 bg-background-secondary border-2 border-border rounded-xl focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary/10 transition-all text-text-primary placeholder-text-muted"
                required
                autoFocus
              />
            </div>

            <div>
              <label 
                htmlFor="senha" 
                className="block text-sm font-semibold text-text-primary mb-2"
              >
                Senha
              </label>
              <div className="relative">
                <input
                  id="senha"
                  type={mostrarSenha ? 'text' : 'password'}
                  value={senha}
                  onChange={(e) => setSenha(e.target.value)}
                  placeholder="Digite sua senha"
                  className="w-full px-4 py-3 pr-12 bg-background-secondary border-2 border-border rounded-xl focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary/10 transition-all text-text-primary placeholder-text-muted"
                  required
                />
                <button
                  type="button"
                  onClick={() => setMostrarSenha(v => !v)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 p-1 text-text-muted hover:text-text-primary transition-colors"
                  tabIndex={-1}
                  aria-label={mostrarSenha ? 'Ocultar senha' : 'Mostrar senha'}
                >
                  {mostrarSenha ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                </button>
              </div>
            </div>

            <button
              type="submit"
              className="w-full bg-primary text-primary-foreground font-bold py-4 rounded-xl hover:bg-primary/90 transform hover:scale-[1.02] active:scale-[0.98] transition-all shadow-lg hover:shadow-xl"
            >
              Entrar no Sistema
            </button>

            {erroLogin && (
              <div className="flex items-start gap-2 p-3 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
                <AlertCircle className="w-4 h-4 mt-0.5 shrink-0" />
                <span>{erroLogin}</span>
              </div>
            )}
          </form>
        </div>
      </div>
    </div>
  );
};

export default Login;
