import { History, Search, ShoppingCart } from 'lucide-react'
import React from 'react'
import { useCartStore } from '../../store/cartStore'

interface AppHeaderProps {
  onSearch?: (query: string) => void
  onOpenHistory?: () => void
}

const AppHeader: React.FC<AppHeaderProps> = ({ onSearch, onOpenHistory }) => {
  const { toggleCart, getTotalItems, getTotalPrice } = useCartStore()
  const totalItems = getTotalItems()
  const totalPrice = getTotalPrice()

  return (
    <header className="sticky top-0 z-50 glass-effect border-b border-border/30">
      <div className="max-w-md mx-auto px-6 py-6">
        {/* Logo e Título */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-4">
            <div className="w-14 h-14 bg-gradient-to-br from-primary via-primary/90 to-primary/80 rounded-3xl flex items-center justify-center shadow-glow">
              <span className="text-white font-bold text-2xl">🍔</span>
            </div>
            <div>
              <h1 className="text-3xl font-bold text-gradient">IComanda</h1>
              <p className="text-sm text-text-secondary font-medium">Sistema de Pedidos</p>
            </div>
          </div>

          {/* Ações */}
          <div className="flex items-center space-x-3">
            {onOpenHistory && (
              <button
                onClick={onOpenHistory}
                className="w-12 h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
              >
                <History className="w-5 h-5" />
              </button>
            )}
            
            <button
              onClick={toggleCart}
              className="relative w-12 h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
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
