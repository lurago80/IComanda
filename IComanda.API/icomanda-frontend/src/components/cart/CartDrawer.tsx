import { AnimatePresence, motion } from 'framer-motion'
import { CheckCircle, Minus, Package, Plus, Trash2, User, UserPlus, X } from 'lucide-react'
import React, { useState } from 'react'
import { useToast } from '../../hooks/useToast'
import { vendasService } from '../../services/api'
import { useCartStore } from '../../store/cartStore'
import { Cliente } from '../../types/api'
import ClienteSearch from '../ClienteSearch'
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

  const [isProcessing, setIsProcessing] = useState(false)
  const [showSuccess, setShowSuccess] = useState(false)
  const [clienteSelecionado, setClienteSelecionado] = useState<Cliente | null>(null)
  const [showClienteSearch, setShowClienteSearch] = useState(false)
  const { showSuccess: showSuccessToast, showError } = useToast()

  const totalPrice = getTotalPrice()

  const formatarPreco = (preco: number) => {
    return preco.toFixed(2).replace('.', ',')
  }

  const handleFinalizarPedido = async () => {
    if (items.length === 0) return

    setIsProcessing(true)
    
    try {
      // Recupera o usuário logado
      const usuarioLogadoStr = localStorage.getItem('usuario_logado');
      let operadorId = 1; // Padrão
      
      if (usuarioLogadoStr) {
        try {
          const usuarioInfo = JSON.parse(usuarioLogadoStr);
          operadorId = usuarioInfo.id || 1;
        } catch {
          // Se falhar o parse, mantém o padrão
          operadorId = 1;
        }
      }

      const vendaRequest = {
        cliente: clienteSelecionado?.id || 1, // Cliente selecionado ou padrão
        nomeCliente: clienteSelecionado?.nomeCompleto,
        cpfCnpjCliente: clienteSelecionado?.documento,
        operador: operadorId, // ID do garçom/operador logado
        vendedor: operadorId, // Mesmo ID para vendedor
        itens: items.map(item => ({
          codigo: item.produto.id,
          qtd: item.quantidade,
          preco: item.produto.precoVenda
        }))
      }

      const venda = await vendasService.criar(vendaRequest)
      
      // Mostrar sucesso
      setShowSuccess(true)
      showSuccessToast('Pedido finalizado!', `Pedido #${venda.nota} criado com sucesso.`)
      clearCart()
      
      // Fechar modal após 2 segundos
      setTimeout(() => {
        setCartOpen(false)
        setShowSuccess(false)
      }, 2000)
      
    } catch (error) {
      console.error('Erro ao finalizar pedido:', error)
      showError('Erro ao finalizar pedido', 'Não foi possível processar o pedido. Tente novamente.')
    } finally {
      setIsProcessing(false)
    }
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
            className="fixed bottom-0 left-0 right-0 bg-white border-t border-amber-200 rounded-t-3xl z-50 max-h-[80vh] flex flex-col shadow-2xl"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-6 border-b border-amber-100">
              <h2 className="text-xl font-bold text-gray-800">Carrinho</h2>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setCartOpen(false)}
                className="text-gray-500 hover:text-gray-800"
              >
                <X className="w-5 h-5" />
              </Button>
            </div>

            {/* Lista de Itens */}
            <div className="flex-1 overflow-y-auto px-6 py-4">
              {items.length === 0 ? (
                <div className="text-center py-12">
                  <div className="w-16 h-16 bg-amber-100 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Package className="w-8 h-8 text-amber-600" />
                  </div>
                  <h3 className="text-lg font-medium text-gray-800 mb-2">Carrinho vazio</h3>
                  <p className="text-gray-600">Adicione produtos para começar seu pedido</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {items.map((item, index) => (
                    <div
                      key={`${item.produto.id}-${index}`}
                      className="bg-amber-50 rounded-2xl p-4 border border-amber-100"
                    >
                      <div className="flex items-center space-x-4">
                        {/* Imagem do Produto */}
                        <div className="w-12 h-12 bg-gradient-to-br from-amber-200 to-orange-200 rounded-xl flex items-center justify-center border border-amber-300">
                          <Package className="w-6 h-6 text-amber-700" />
                        </div>

                        {/* Informações do Produto */}
                        <div className="flex-1 min-w-0">
                          <h3 className="font-medium text-gray-800 truncate">
                            {item.produto.descricao}
                          </h3>
                          <p className="text-sm text-gray-600">
                            R$ {formatarPreco(item.produto.precoVenda)} cada
                          </p>
                        </div>

                        {/* Controles de Quantidade */}
                        <div className="flex items-center space-x-2">
                          <Button
                            variant="outline"
                            size="icon"
                            onClick={() => updateQuantity(item.produto.id, item.quantidade - 1)}
                            className="w-8 h-8 rounded-lg border-amber-300 hover:bg-amber-100"
                          >
                            <Minus className="w-4 h-4" />
                          </Button>
                          
                          <span className="w-8 text-center font-medium text-gray-800">
                            {item.quantidade}
                          </span>
                          
                          <Button
                            size="icon"
                            onClick={() => updateQuantity(item.produto.id, item.quantidade + 1)}
                            className="w-8 h-8 rounded-lg bg-amber-500 hover:bg-amber-600"
                          >
                            <Plus className="w-4 h-4" />
                          </Button>
                        </div>

                        {/* Preço Total do Item */}
                        <div className="text-right">
                          <p className="font-semibold text-gray-800">
                            R$ {formatarPreco(item.produto.precoVenda * item.quantidade)}
                          </p>
                        </div>

                        {/* Botão Remover */}
                        <Button
                          variant="outline"
                          size="icon"
                          onClick={() => removeItem(item.produto.id)}
                          className="w-8 h-8 rounded-lg text-red-500 border-red-300 hover:bg-red-50"
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Seleção de Cliente */}
            {items.length > 0 && (
              <div className="border-t border-amber-100 p-6">
                <div className="mb-4">
                  <h3 className="text-lg font-semibold text-gray-800 mb-3">Cliente</h3>
                  
                  {clienteSelecionado ? (
                    <div className="bg-amber-50 border border-amber-200 rounded-2xl p-4">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-3">
                          <div className="w-10 h-10 bg-amber-200 rounded-xl flex items-center justify-center">
                            <User className="w-5 h-5 text-amber-700" />
                          </div>
                          <div>
                            <h4 className="font-semibold text-gray-800">{clienteSelecionado.nomeCompleto}</h4>
                            {clienteSelecionado.documento && (
                              <p className="text-sm text-gray-600">{clienteSelecionado.documento}</p>
                            )}
                          </div>
                        </div>
                        <button
                          onClick={() => setClienteSelecionado(null)}
                          className="w-8 h-8 bg-red-100 text-red-600 rounded-xl flex items-center justify-center hover:bg-red-200 transition-colors"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  ) : (
                    <button
                      onClick={() => setShowClienteSearch(true)}
                      className="w-full bg-amber-50 border border-amber-200 rounded-2xl p-4 hover:bg-amber-100 transition-colors text-left"
                    >
                      <div className="flex items-center space-x-3">
                        <div className="w-10 h-10 bg-amber-200 rounded-xl flex items-center justify-center">
                          <UserPlus className="w-5 h-5 text-amber-700" />
                        </div>
                        <div>
                          <h4 className="font-semibold text-gray-800">Selecionar Cliente</h4>
                          <p className="text-sm text-gray-600">Buscar por nome, CPF/CNPJ ou telefone</p>
                        </div>
                      </div>
                    </button>
                  )}
                </div>
              </div>
            )}

            {/* Footer com Total e Botão Finalizar */}
            {items.length > 0 && (
              <div className="border-t border-amber-100 p-6 bg-amber-50/50">
                <div className="flex justify-between items-center mb-4">
                  <span className="text-lg font-medium text-gray-700">Total:</span>
                  <span className="text-2xl font-bold text-amber-600">
                    R$ {formatarPreco(totalPrice)}
                  </span>
                </div>
                
                <Button 
                  onClick={handleFinalizarPedido}
                  disabled={isProcessing || showSuccess}
                  className="w-full bg-gradient-to-r from-amber-500 to-orange-500 text-white py-4 rounded-2xl font-semibold text-lg hover:from-amber-600 hover:to-orange-600 transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isProcessing ? (
                    <div className="flex items-center space-x-2">
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                      <span>Processando...</span>
                    </div>
                  ) : showSuccess ? (
                    <div className="flex items-center space-x-2">
                      <CheckCircle className="w-5 h-5" />
                      <span>Pedido Finalizado!</span>
                    </div>
                  ) : (
                    'Finalizar Pedido'
                  )}
                </Button>
                
                <p className="text-center text-sm text-gray-600 mt-3">
                  {items.length} {items.length === 1 ? 'item' : 'itens'} no carrinho
                </p>
              </div>
            )}
          </motion.div>
        </>
      )}
      
      {/* Modal de Busca de Cliente */}
      <ClienteSearch
        isOpen={showClienteSearch}
        onClose={() => setShowClienteSearch(false)}
        onSelectCliente={setClienteSelecionado}
      />
    </AnimatePresence>
  )
}

export default CartDrawer

