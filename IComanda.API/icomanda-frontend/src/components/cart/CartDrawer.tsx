import { AnimatePresence, motion } from 'framer-motion'
import { Minus, Package, Plus, Trash2, X } from 'lucide-react'
import React from 'react'
import { useCartStore } from '../../store/cartStore'
import { Button } from '../ui/button'

const CartDrawer: React.FC = () => {
  const { 
    isOpen, 
    setCartOpen, 
    items, 
    updateQuantity, 
    removeItem, 
    getTotalPrice, 
    clearCart 
  } = useCartStore()

  const totalPrice = getTotalPrice()

  const formatarPreco = (preco: number) => {
    return preco.toFixed(2).replace('.', ',')
  }

  const handleFinalizarPedido = () => {
    // TODO: Implementar finalização do pedido
    console.log('Finalizando pedido:', items)
    clearCart()
    setCartOpen(false)
  }

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/50 z-50"
            onClick={() => setCartOpen(false)}
          />

          {/* Drawer */}
          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: "spring", stiffness: 300, damping: 30 }}
            className="fixed bottom-0 left-0 right-0 bg-bg border-t border-border rounded-t-3xl z-50 max-h-[80vh] flex flex-col"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-6 border-b border-border">
              <h2 className="text-xl font-bold text-white">Carrinho</h2>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setCartOpen(false)}
                className="text-textMuted hover:text-white"
              >
                <X className="w-5 h-5" />
              </Button>
            </div>

            {/* Lista de Itens */}
            <div className="flex-1 overflow-y-auto px-6 py-4">
              {items.length === 0 ? (
                <div className="text-center py-12">
                  <div className="w-16 h-16 bg-bgMuted rounded-full flex items-center justify-center mx-auto mb-4">
                    <Package className="w-8 h-8 text-textMuted" />
                  </div>
                  <h3 className="text-lg font-medium text-white mb-2">Carrinho vazio</h3>
                  <p className="text-textMuted">Adicione produtos para começar seu pedido</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {items.map((item) => (
                    <div
                      key={item.produto.id}
                      className="bg-bgMuted rounded-2xl p-4"
                    >
                      <div className="flex items-center space-x-4">
                        {/* Imagem do Produto */}
                        <div className="w-12 h-12 bg-gradient-to-br from-primary/20 to-primary/10 rounded-xl flex items-center justify-center border border-primary/20">
                          <Package className="w-6 h-6 text-primary" />
                        </div>

                        {/* Informações do Produto */}
                        <div className="flex-1 min-w-0">
                          <h3 className="font-medium text-white truncate">
                            {item.produto.descricao}
                          </h3>
                          <p className="text-sm text-textMuted">
                            R$ {formatarPreco(item.produto.precoVenda)} cada
                          </p>
                        </div>

                        {/* Controles de Quantidade */}
                        <div className="flex items-center space-x-2">
                          <Button
                            variant="outline"
                            size="icon"
                            onClick={() => updateQuantity(item.produto.id, item.quantidade - 1)}
                            className="w-8 h-8 rounded-lg"
                          >
                            <Minus className="w-4 h-4" />
                          </Button>
                          
                          <span className="w-8 text-center font-medium text-white">
                            {item.quantidade}
                          </span>
                          
                          <Button
                            size="icon"
                            onClick={() => updateQuantity(item.produto.id, item.quantidade + 1)}
                            className="w-8 h-8 rounded-lg"
                          >
                            <Plus className="w-4 h-4" />
                          </Button>
                        </div>

                        {/* Preço Total do Item */}
                        <div className="text-right">
                          <p className="font-semibold text-white">
                            R$ {formatarPreco(item.produto.precoVenda * item.quantidade)}
                          </p>
                        </div>

                        {/* Botão Remover */}
                        <Button
                          variant="outline"
                          size="icon"
                          onClick={() => removeItem(item.produto.id)}
                          className="w-8 h-8 rounded-lg text-red-400 border-red-400/20 hover:bg-red-400/10"
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Footer com Total e Botão Finalizar */}
            {items.length > 0 && (
              <div className="border-t border-border p-6">
                <div className="flex justify-between items-center mb-4">
                  <span className="text-lg font-medium text-white">Total:</span>
                  <span className="text-2xl font-bold text-primary">
                    R$ {formatarPreco(totalPrice)}
                  </span>
                </div>
                
                <Button 
                  onClick={handleFinalizarPedido}
                  className="w-full bg-primary text-white py-4 rounded-2xl font-semibold text-lg hover:bg-primary/90 transition-colors"
                >
                  Finalizar Pedido
                </Button>
                
                <p className="text-center text-sm text-textMuted mt-3">
                  {items.length} {items.length === 1 ? 'item' : 'itens'} no carrinho
                </p>
              </div>
            )}
          </motion.div>
        </>
      )}
    </AnimatePresence>
  )
}

export default CartDrawer

