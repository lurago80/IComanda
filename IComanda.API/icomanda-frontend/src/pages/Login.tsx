import React, { useState } from 'react';

const Login: React.FC = () => {
  const [nome, setNome] = useState('');

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault();
    if (nome.trim()) {
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
    <div className="min-h-screen bg-gradient-to-br from-amber-50 via-orange-50 to-yellow-50 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        <div className="bg-white rounded-3xl shadow-2xl p-8 border border-amber-100">
          {/* Logo e Título */}
          <div className="text-center mb-8">
            <div className="w-20 h-20 bg-gradient-to-br from-amber-500 to-orange-500 rounded-full flex items-center justify-center mx-auto mb-4 shadow-lg">
              <span className="text-4xl">🥐</span>
            </div>
            <h1 className="text-4xl font-bold bg-gradient-to-r from-amber-600 to-orange-600 bg-clip-text text-transparent mb-2">
              IComanda
            </h1>
            <p className="text-gray-600 font-medium">Sistema de Pedidos - Padaria</p>
          </div>

          {/* Formulário */}
          <form onSubmit={handleLogin} className="space-y-6">
            <div>
              <label 
                htmlFor="nome" 
                className="block text-sm font-semibold text-gray-700 mb-2"
              >
                Nome do Usuário
              </label>
              <input
                id="nome"
                type="text"
                value={nome}
                onChange={(e) => setNome(e.target.value)}
                placeholder="Digite seu nome"
                className="w-full px-4 py-3 bg-gray-50 border-2 border-gray-200 rounded-xl focus:outline-none focus:border-amber-500 focus:ring-4 focus:ring-amber-100 transition-all text-gray-800 placeholder-gray-400"
                required
                autoFocus
              />
            </div>

            <button
              type="submit"
              className="w-full bg-gradient-to-r from-amber-500 to-orange-500 text-white font-bold py-4 rounded-xl hover:from-amber-600 hover:to-orange-600 transform hover:scale-[1.02] active:scale-[0.98] transition-all shadow-lg hover:shadow-xl"
            >
              Entrar no Sistema
            </button>
          </form>

          {/* Informação adicional */}
          <div className="mt-6 text-center">
            <p className="text-xs text-gray-500">
              Digite qualquer nome para acessar
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
