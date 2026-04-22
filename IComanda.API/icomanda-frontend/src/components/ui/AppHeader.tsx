import { ClipboardList, History, LogOut, Receipt, Search, ShoppingCart, User } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { useCartStore } from '../../store/cartStore'

interface AppHeaderProps {
  onSearch?: (query: string) => void
  onOpenHistory?: () => void
  onOpenConferencia?: () => void
  onOpenAbrirComanda?: () => void
}

const AppHeader: React.FC<AppHeaderProps> = ({ onSearch, onOpenHistory, onOpenConferencia, onOpenAbrirComanda }) => {
  const { toggleCart, getTotalItems, getTotalPrice, comandaAtiva, vendaEmEdicao, flowState, setCartOpen, isOpen } = useCartStore()
  const totalItems = getTotalItems()
  const totalPrice = getTotalPrice()
  const [nomeUsuario, setNomeUsuario] = useState<string>('')

  useEffect(() => {
    const usuarioLogadoStr = localStorage.getItem('usuario_logado');
    if (usuarioLogadoStr) {
      try {
        const usuarioInfo = JSON.parse(usuarioLogadoStr);
        setNomeUsuario(usuarioInfo.nome || 'Usuário');
      } catch {
        setNomeUsuario('Usuário');
      }
    }
  }, []);

  const handleLogout = () => {
    if (window.confirm('Deseja realmente sair do sistema?')) {
      console.log('🚪 [Logout] Limpando dados de autenticação...');
      localStorage.removeItem('jwt_token');
      localStorage.removeItem('usuario_logado');
      console.log('🔄 [Logout] Redirecionando para login...');
      window.location.reload();
    }
  };

  // Diminuir z-index do header quando drawer ou modais estiverem abertos
  const headerZIndex = isOpen ? 'z-40' : 'z-50'
  
  return (
    <header className={`sticky top-0 ${headerZIndex} glass-effect border-b border-border/30`}>
      <div className="max-w-md mx-auto px-6 py-6">
        {/* Logo e Título */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-4">
            <div className="h-20 rounded-3xl flex items-center justify-center overflow-hidden px-6 py-2">
              <img src="/iComanda.jpg" alt="iComanda Logo" className="h-full w-auto object-contain" />
            </div>
            <div>
              <p className="text-sm text-text-muted font-medium">In9ve Informática</p>
            </div>
          </div>

          {/* Usuário e Ações */}
          <div className="flex items-center space-x-2">
            {nomeUsuario && (
              <div className="flex items-center space-x-2 px-3 py-2 bg-background-secondary border border-border rounded-2xl">
                <User className="w-4 h-4 text-primary" />
                <span className="text-sm font-semibold text-text-primary">{nomeUsuario}</span>
              </div>
            )}
            <button
              onClick={handleLogout}
              className="w-10 h-10 bg-red-50 border border-red-200 rounded-2xl flex items-center justify-center text-red-600 hover:bg-red-100 hover:border-red-300 transition-all duration-300 hover:scale-105 active:scale-95"
              aria-label="Sair do sistema"
              title="Sair do sistema"
            >
              <LogOut className="w-4 h-4" />
            </button>
          </div>
        </div>

        {/* Indicador de Estado */}
        {flowState === 'edicao' && vendaEmEdicao ? (
          <div className="mb-4 p-4 bg-warning/10 border-2 border-warning/50 rounded-2xl">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 bg-warning rounded-full animate-pulse"></div>
                <div>
                  <span className="text-sm font-bold text-warning block">MODO DE EDIÇÃO</span>
                  <span className="text-xs text-text-secondary">
                    {vendaEmEdicao.mesa && `Mesa ${vendaEmEdicao.mesa}`}
                    {vendaEmEdicao.comanda && `Comanda ${String(vendaEmEdicao.comanda).padStart(6, '0')}`}
                  </span>
                </div>
              </div>
              <div className="text-xs text-text-muted font-medium">
                Nota: {vendaEmEdicao.nota}
              </div>
            </div>
            <button
              onClick={() => setCartOpen(true)}
              className="w-full px-4 py-2 bg-warning text-warning-foreground rounded-xl text-xs font-semibold hover:bg-warning/90 transition-colors"
            >
              Revisar / Atualizar
            </button>
          </div>
        ) : flowState === 'nova' && comandaAtiva ? (
          <div className="mb-4 p-4 bg-success/10 border-2 border-success/50 rounded-2xl">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 bg-success rounded-full animate-pulse"></div>
                <div>
                  <span className="text-sm font-bold text-success block">COMANDA ATIVA</span>
                  <span className="text-xs text-text-secondary">
                    Comanda #{comandaAtiva.numeroComanda}
                    {comandaAtiva.numeroMesa && ` | Mesa ${comandaAtiva.numeroMesa}`}
                  </span>
                </div>
              </div>
              <button
                onClick={() => setCartOpen(true)}
                className="px-4 py-2 bg-success text-white rounded-xl text-xs font-semibold"
              >
                Revisar Pedido
              </button>
            </div>
          </div>
        ) : (
          <div className="mb-4 p-4 bg-background-secondary border-2 border-border rounded-2xl">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 bg-text-muted rounded-full"></div>
                <span className="text-sm font-semibold text-text-secondary">Nenhuma comanda aberta</span>
              </div>
            </div>
          </div>
        )}

        <div className="flex items-center justify-between mb-6">
          {/* Botões de Ação */}
          {flowState === 'idle' && (
            <div className="flex items-center space-x-2">
              {onOpenAbrirComanda && (
                <button
                  onClick={onOpenAbrirComanda}
                  className="px-6 py-3 bg-primary text-primary-foreground rounded-2xl font-semibold
                             hover:bg-primary/90 transition-all duration-300 hover:scale-105 active:scale-95
                             flex items-center space-x-2 shadow-medium"
                >
                  <ClipboardList className="w-5 h-5" />
                  <span>Nova Comanda</span>
                </button>
              )}
              {onOpenConferencia && (
                <button
                  onClick={onOpenConferencia}
                  className="px-4 py-3 bg-background-secondary border border-border text-text-primary rounded-2xl font-semibold
                             hover:bg-card-hover transition-all duration-300 hover:scale-105 active:scale-95
                             flex items-center space-x-2"
                  title="Buscar e editar comanda/mesa"
                >
                  <Receipt className="w-5 h-5" />
                  <span>Buscar/Editar</span>
                </button>
              )}
            </div>
          )}
          
          <div className="flex-1"></div>
          
          {/* Ações */}
          <div className="flex items-center space-x-3">
            {onOpenConferencia && (
            <button
              onClick={onOpenConferencia}
              className="w-12 h-12 bg-background-secondary border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
              aria-label="Conferência de Mesa/Comanda"
              title="Conferência"
            >
              <Receipt className="w-5 h-5" />
            </button>
            )}
            
            {onOpenHistory && (
            <button
              onClick={onOpenHistory}
              className="w-12 h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
              aria-label="Abrir histórico de pedidos"
            >
              <History className="w-5 h-5" />
            </button>
            )}
            
            <button
              onClick={toggleCart}
              className="relative w-12 h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
              aria-label={`Abrir carrinho de compras. ${totalItems} itens no carrinho`}
            >
              <ShoppingCart className="w-5 h-5" />
              {totalItems > 0 && (
                <div className="absolute -top-2 -right-2 w-6 h-6 bg-primary rounded-full flex items-center justify-center text-xs font-bold text-white shadow-lg animate-bounce-soft">
                  {totalItems}
                </div>
              )}
            </button>
          </div>
        </div>

        {/* Barra de Busca */}
        {onSearch && (
          <div className="relative mb-6">
            <Search className="absolute left-5 top-1/2 transform -translate-y-1/2 w-5 h-5 text-text-muted" />
            <input
              type="text"
              placeholder="Buscar produtos..."
              className="input-modern w-full pl-14 pr-5 py-4 text-lg"
              onChange={(e) => onSearch(e.target.value)}
              aria-label="Buscar produtos"
            />
          </div>
        )}

        {/* Total do Carrinho */}
        {totalItems > 0 && (
          <div className="p-5 bg-gradient-to-r from-primary/10 via-primary/5 to-accent/10 border border-primary/20 rounded-3xl backdrop-blur-sm animate-fade-in-up">
            <div className="flex justify-between items-center">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 bg-primary rounded-full animate-pulse-soft"></div>
                <span className="text-sm text-primary font-semibold">Total do carrinho:</span>
              </div>
              <span className="text-2xl font-bold text-text-primary">
                R$ {totalPrice.toFixed(2).replace('.', ',')}
              </span>
            </div>
          </div>
        )}
      </div>
    </header>
  )
}

export default AppHeader
