import React, { useState } from 'react';

const Login: React.FC = () => {
  const [nome, setNome] = useState('');
  const [senha, setSenha] = useState('');

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    if (nome.trim() && senha.trim()) {
      // Salva informações do usuário para usar nas vendas
      const usuarioInfo = {
        nome: nome.trim(),
        id: 1, // Por enquanto ID fixo, mas poderia buscar do backend
        dataLogin: new Date().toISOString()
      };
      localStorage.setItem('usuario_logado', JSON.stringify(usuarioInfo));
      window.location.reload();
    }
  };

  return (
    <div className="min-h-screen bg-background flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="bg-card rounded-3xl shadow-2xl p-8 border border-border">
          {/* Logo e Título */}
          <div className="text-center mb-8">
            <div className="w-20 h-20 bg-primary rounded-full flex items-center justify-center mx-auto mb-4 shadow-lg">
              <span className="text-4xl">📋</span>
            </div>
            <h1 className="text-4xl font-bold text-primary mb-2">
              IComanda
            </h1>
            <p className="text-text-secondary font-medium">Sistema de Pedidos - Padaria</p>
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
              <input
                id="senha"
                type="password"
                value={senha}
                onChange={(e) => setSenha(e.target.value)}
                placeholder="Digite sua senha"
                className="w-full px-4 py-3 bg-background-secondary border-2 border-border rounded-xl focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary/10 transition-all text-text-primary placeholder-text-muted"
                required
              />
            </div>

            <button
              type="submit"
              className="w-full bg-primary text-primary-foreground font-bold py-4 rounded-xl hover:bg-primary/90 transform hover:scale-[1.02] active:scale-[0.98] transition-all shadow-lg hover:shadow-xl"
            >
              Entrar no Sistema
            </button>
          </form>

          {/* Informação adicional */}
          <div className="mt-6 text-center">
            <p className="text-xs text-text-muted">
              Digite qualquer nome e senha para acessar
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
