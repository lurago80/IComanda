import { Package, Plus } from 'lucide-react'
import React from 'react'
import { useCartStore } from '../../store/cartStore'
import { Produto } from '../../types/api'

interface ProductCardProps {
  produto: Produto
}

const ProductCard: React.FC<ProductCardProps> = ({ produto }) => {
  const { addItem } = useCartStore()

  const formatarPreco = (preco: number) => {
    return preco.toFixed(2).replace('.', ',')
  }

  const handleAddToCart = () => {
    addItem(produto, 1)
  }

  return (
    <div className="card-modern group animate-fade-in-up">
      <div className="flex items-start space-x-6">
        {/* Imagem do Produto com design moderno */}
        <div className="w-24 h-24 bg-gradient-to-br from-primary/20 via-primary/10 to-accent/10 rounded-3xl flex items-center justify-center border-2 border-primary/20 shadow-large group-hover:shadow-glow transition-all duration-500 relative overflow-hidden">
          <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-transparent"></div>
          <div className="group-hover:scale-110 group-hover:rotate-6 transition-all duration-500 relative z-10">
            <Package className="w-12 h-12 text-primary" />
          </div>
        </div>

        {/* Informações do Produto */}
        <div className="flex-1 min-w-0">
          <h3 className="font-bold text-text-primary text-lg truncate mb-2 group-hover:text-gradient transition-all duration-300 leading-tight">
            {produto.descricao}
          </h3>
          
          {produto.caracteristica && (
            <p className="text-sm text-text-secondary truncate mb-4 leading-relaxed">
              {produto.caracteristica}
            </p>
          )}

          <div className="flex items-center justify-between">
            <div className="flex flex-col space-y-2">
              <div className="flex items-center space-x-3">
                <span className="text-2xl font-bold text-text-primary group-hover:text-gradient transition-all duration-300">
                  R$ {formatarPreco(produto.precoVenda)}
                </span>
                {produto.unMedida && (
                  <div className="px-3 py-1 bg-card-secondary border border-border rounded-full">
                    <span className="text-xs text-text-secondary font-medium">{produto.unMedida}</span>
                  </div>
                )}
              </div>
              
              {/* Indicador de disponibilidade */}
              <div className="flex items-center space-x-2">
                <div className="w-2 h-2 bg-success rounded-full"></div>
                <span className="text-xs text-text-secondary">Disponível</span>
              </div>
            </div>

            {/* Botão Adicionar com design moderno */}
            <button
              onClick={handleAddToCart}
              className="w-14 h-14 bg-primary hover:bg-primary/90 rounded-3xl flex items-center justify-center text-white shadow-large hover:shadow-glow transition-all duration-300 group-hover:scale-110 active:scale-95 focus:outline-none focus:ring-2 focus:ring-primary/50"
            >
              <Plus className="w-7 h-7" />
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default ProductCard
