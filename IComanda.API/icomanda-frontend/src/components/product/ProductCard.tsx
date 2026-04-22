import { Package, Plus } from 'lucide-react'
import React, { useState } from 'react'
import { useCartStore } from '../../store/cartStore'
import { Produto } from '../../types/api'
import { normalizarTexto } from '../../lib/utils'

const isTaxaEntregaProduct = (p: Produto) =>
  (p.descricao || '').toUpperCase().trim().includes('TAXA DE ENTREGA')

interface ProductCardProps {
  produto: Produto
  /** Quando o produto for "TAXA DE ENTREGA", chama este callback em vez de adicionar direto (abre modal de seleção) */
  onRequestTaxaEntrega?: (produto: Produto) => void
}

const ProductCard: React.FC<ProductCardProps> = ({ produto, onRequestTaxaEntrega }) => {
  const { addItem } = useCartStore()
  const [modalAberto, setModalAberto] = useState(false)
  const [qtdInputStr, setQtdInputStr] = useState('1')

  const formatarPreco = (preco: number) => preco.toFixed(2).replace('.', ',')

  const descricaoFormatada = normalizarTexto(
    produto.descricao?.replace(/\s+/g, ' ').trim() ||
    produto.caracteristica?.replace(/\s+/g, ' ').trim() ||
    `Produto #${produto.id}`
  )

  const abrirModal = () => {
    if (isTaxaEntregaProduct(produto) && onRequestTaxaEntrega) {
      onRequestTaxaEntrega(produto)
      return
    }
    setQtdInputStr('1')
    setModalAberto(true)
  }

  const handleQtdChange = (v: string) => {
    const normalized = v.replace(',', '.')
    if (/^(\d*\.?\d*)$/.test(normalized)) {
      setQtdInputStr(v)
    }
  }

  const confirmar = () => {
    const qtd = parseFloat(qtdInputStr.replace(',', '.'))
    if (isNaN(qtd) || qtd <= 0) return
    addItem(produto, qtd)
    setModalAberto(false)
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') confirmar()
    if (e.key === 'Escape') setModalAberto(false)
  }

  const qtdPreview = parseFloat(qtdInputStr.replace(',', '.'))
  const totalPreview = !isNaN(qtdPreview) && qtdPreview > 0 ? qtdPreview * (produto.precoVenda ?? 0) : null

  return (
    <>
    <div className="card-modern group animate-fade-in-up overflow-hidden">
      <div className="flex items-start gap-3 sm:gap-4 md:gap-6 flex-wrap sm:flex-nowrap max-w-full">
        {/* Imagem do Produto com design moderno - Responsiva */}
        <div className="w-16 h-16 sm:w-20 sm:h-20 md:w-24 md:h-24 bg-gradient-to-br from-primary/20 via-primary/10 to-accent/10 rounded-2xl sm:rounded-3xl flex items-center justify-center border-2 border-primary/20 shadow-large group-hover:shadow-glow transition-all duration-500 relative overflow-hidden flex-shrink-0 max-w-full">
          <div className="absolute inset-0 bg-gradient-to-br from-primary/5 to-transparent"></div>
          <div className="group-hover:scale-110 group-hover:rotate-6 transition-all duration-500 relative z-10 w-full h-full flex items-center justify-center">
            <Package className="w-8 h-8 sm:w-10 sm:h-10 md:w-12 md:h-12 text-primary max-w-full max-h-full" />
          </div>
        </div>

        {/* Informações do Produto - Responsivo */}
        <div className="flex-1 min-w-0 w-full sm:w-auto max-w-full overflow-hidden">
          <h3 className="font-bold text-text-primary text-base sm:text-lg mb-2 group-hover:text-gradient transition-all duration-300 leading-tight break-words whitespace-normal">
            {descricaoFormatada}
          </h3>
          
          {produto.caracteristica && (
            <p className="text-sm text-text-secondary mb-4 leading-relaxed break-words whitespace-normal">
              {normalizarTexto(produto.caracteristica)}
            </p>
          )}

          <div className="flex items-center justify-between gap-2 sm:gap-3 flex-wrap sm:flex-nowrap">
            <div className="flex flex-col space-y-2 min-w-0 flex-1">
              <div className="flex items-center space-x-2 sm:space-x-3 flex-wrap">
                <span className="text-xl sm:text-2xl font-bold text-text-primary group-hover:text-gradient transition-all duration-300">
                  R$ {formatarPreco(produto.precoVenda)}
                </span>
                {produto.unMedida && (
                  <div className="px-2 sm:px-3 py-1 bg-card-secondary border border-border rounded-full flex-shrink-0">
                    <span className="text-xs text-text-secondary font-medium">{produto.unMedida}</span>
                  </div>
                )}
              </div>
              
              {/* Indicador de disponibilidade */}
              <div className="flex items-center space-x-2">
                <div className="w-2 h-2 bg-success rounded-full flex-shrink-0"></div>
                <span className="text-xs text-text-secondary">Disponível</span>
              </div>
            </div>

            {/* Botão Adicionar com design moderno - Responsivo */}
            <button
              onClick={abrirModal}
              className="w-12 h-12 sm:w-14 sm:h-14 bg-primary hover:bg-primary/90 rounded-2xl sm:rounded-3xl flex items-center justify-center text-white shadow-large hover:shadow-glow transition-all duration-300 group-hover:scale-110 active:scale-95 focus:outline-none focus:ring-2 focus:ring-primary/50 flex-shrink-0"
            >
              <Plus className="w-5 h-5 sm:w-6 sm:h-6 md:w-7 md:h-7" />
            </button>
          </div>
        </div>
      </div>
    </div>

    {/* Modal de quantidade */}
    {modalAberto && (
      <div
        className="fixed inset-0 bg-black/50 flex items-end sm:items-center justify-center z-[90] p-4"
        onClick={e => { if (e.target === e.currentTarget) setModalAberto(false) }}
      >
        <div className="bg-white rounded-2xl w-full max-w-sm shadow-xl p-5 space-y-4 overflow-y-auto max-h-[90dvh]">

          {/* Cabeçalho */}
          <div>
            <p className="text-xs text-gray-400 uppercase tracking-wide mb-1">Adicionar produto</p>
            <p className="text-base font-bold text-gray-900 leading-tight">{descricaoFormatada}</p>
            <p className="text-sm text-gray-500 mt-0.5">
              R$ {formatarPreco(produto.precoVenda ?? 0)} / {produto.unMedida ?? 'UN'}
            </p>
          </div>

          {/* Campo de quantidade */}
          <div>
            <label className="text-sm font-semibold text-gray-700 block mb-1.5">Quantidade</label>
            <input
              type="text"
              inputMode="decimal"
              value={qtdInputStr}
              onChange={e => handleQtdChange(e.target.value)}
              onFocus={e => e.target.select()}
              onKeyDown={handleKeyDown}
              autoFocus
              className="w-full text-center text-3xl font-bold border-2 border-blue-400 rounded-xl px-4 py-3 focus:outline-none focus:border-blue-600 tracking-widest"
              placeholder="1"
            />
            <p className="text-xs text-gray-400 mt-1.5 text-center">
              Use vírgula ou ponto para fracionados &nbsp;·&nbsp; ex: 0,250
            </p>
          </div>

          {/* Preview do total */}
          {totalPreview !== null && (
            <div className="bg-blue-50 border border-blue-100 rounded-xl px-4 py-2.5 flex justify-between items-center">
              <span className="text-sm text-blue-700 font-medium">Total do item</span>
              <span className="text-lg font-bold text-blue-900">
                R$ {totalPreview.toFixed(2).replace('.', ',')}
              </span>
            </div>
          )}

          {/* Botões */}
          <div className="flex gap-3 pt-1">
            <button
              onClick={() => setModalAberto(false)}
              className="flex-1 py-2.5 border border-gray-300 rounded-xl text-sm font-semibold text-gray-700 hover:bg-gray-50 active:bg-gray-100"
            >
              Cancelar
            </button>
            <button
              onClick={confirmar}
              disabled={isNaN(qtdPreview) || qtdPreview <= 0}
              className="flex-1 py-2.5 bg-blue-600 rounded-xl text-sm font-semibold text-white hover:bg-blue-700 active:bg-blue-800 disabled:opacity-40 disabled:cursor-not-allowed"
            >
              Adicionar
            </button>
          </div>

        </div>
      </div>
    )}
    </>
  )
}

export default ProductCard
