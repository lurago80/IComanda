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
    clearCart,
    comandaAtiva,
    fecharComanda,
    vendaEmEdicao 
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

    // Validar comanda obrigatória
    if (!comandaAtiva) {
      showError('Comanda obrigatória', 'Por favor, abra uma comanda antes de finalizar o pedido.')
      return
    }

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
        cliente: comandaAtiva.cliente.id,
        nomeCliente: comandaAtiva.cliente.nomeCompleto,
        cpfCnpjCliente: comandaAtiva.cliente.documento,
        operador: operadorId,
        vendedor: operadorId,
        comanda: comandaAtiva.numeroComanda,
        mesa: comandaAtiva.numeroMesa,
        numeroPessoas: comandaAtiva.numeroPessoas,
        itens: items.map(item => ({
          codigo: item.produto.id,
          qtd: item.quantidade,
          preco: item.produto.precoVenda
        }))
      }

      const venda = await vendasService.criar(vendaRequest)
      
      // Mostrar sucesso
      setShowSuccess(true)
      const msgComanda = comandaAtiva.numeroComanda ? ` - Comanda ${comandaAtiva.numeroComanda}` : ''
      const msgMesa = comandaAtiva.numeroMesa ? ` - Mesa ${comandaAtiva.numeroMesa}` : ''
      showSuccessToast('Pedido finalizado!', `Pedido #${venda.nota}${msgComanda}${msgMesa}`)
      
      // Limpar e fechar comanda
      clearCart()
      fecharComanda()
      setClienteSelecionado(null)
      
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
            className="fixed inset-0 bg-black/50 z-[90]"
            onClick={() => setCartOpen(false)}
          />

          {/* Drawer */}
          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: "spring", stiffness: 300, damping: 30 }}
            className="fixed bottom-0 left-0 right-0 bg-card border-t border-border rounded-t-3xl z-[100] max-h-[80vh] flex flex-col shadow-2xl"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-6 border-b border-border">
              <h2 className="text-xl font-bold text-text-primary">
                {vendaEmEdicao ? 'Editar Pedido' : 'Carrinho'}
              </h2>
              <Button
                variant="ghost"
                size="icon"
                onClick={() => setCartOpen(false)}
                className="text-text-secondary hover:text-text-primary"
              >
                <X className="w-5 h-5" />
              </Button>
            </div>

            {/* Banner de Edição */}
            {vendaEmEdicao && (
              <div className="mx-6 mt-4 p-4 bg-primary/10 border border-primary/30 rounded-xl">
                <div className="flex items-center gap-2 text-primary">
                  <svg xmlns="http://www.w3.org/2000/svg" className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                  </svg>
                  <div className="flex-1">
                    <p className="font-semibold text-sm">Modo de Edição</p>
                    <p className="text-xs text-text-secondary">
                      {vendaEmEdicao.mesa && `Mesa ${vendaEmEdicao.mesa}`}
                      {vendaEmEdicao.comanda && `Comanda ${String(vendaEmEdicao.comanda).padStart(6, '0')}`}
                    </p>
                  </div>
                </div>
                <p className="text-xs text-text-muted mt-2">
                  Você pode adicionar, alterar ou remover itens. Clique em "Atualizar Pedido" para salvar as alterações.
                </p>
              </div>
            )}

            {/* Lista de Itens */}
            <div className="flex-1 overflow-y-auto px-6 py-4">
              {items.length === 0 ? (
                <div className="text-center py-12">
                  <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Package className="w-8 h-8 text-primary" />
                  </div>
                  <h3 className="text-lg font-medium text-text-primary mb-2">Carrinho vazio</h3>
                  <p className="text-text-secondary">Adicione produtos para começar seu pedido</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {items.map((item, index) => (
                    <div
                      key={`${item.produto.id}-${index}`}
                      className="bg-card-secondary rounded-2xl p-4 border border-border"
                    >
                      <div className="flex items-center space-x-4">
                        {/* Imagem do Produto */}
                        <div className="w-12 h-12 bg-primary/10 rounded-xl flex items-center justify-center border border-primary/20">
                          <Package className="w-6 h-6 text-primary" />
                        </div>

                        {/* Informações do Produto */}
                        <div className="flex-1 min-w-0">
                          <h3 className="font-medium text-text-primary truncate">
                            {item.produto.descricao}
                          </h3>
                          <p className="text-sm text-text-secondary">
                            R$ {formatarPreco(item.produto.precoVenda)} cada
                          </p>
                        </div>

                        {/* Controles de Quantidade */}
                        <div className="flex items-center space-x-2">
                          <Button
                            variant="outline"
                            size="icon"
                            onClick={() => updateQuantity(item.produto.id, item.quantidade - 1)}
                            className="w-8 h-8 rounded-lg border-border hover:bg-background-tertiary"
                          >
                            <Minus className="w-4 h-4" />
                          </Button>
                          
                          <span className="w-8 text-center font-medium text-text-primary">
                            {item.quantidade}
                          </span>
                          
                          <Button
                            size="icon"
                            onClick={() => updateQuantity(item.produto.id, item.quantidade + 1)}
                            className="w-8 h-8 rounded-lg bg-primary hover:bg-primary/90 text-primary-foreground"
                          >
                            <Plus className="w-4 h-4" />
                          </Button>
                        </div>

                        {/* Preço Total do Item */}
                        <div className="text-right">
                          <p className="font-semibold text-text-primary">
                            R$ {formatarPreco(item.produto.precoVenda * item.quantidade)}
                          </p>
                        </div>

                        {/* Botão Remover */}
                        <Button
                          variant="outline"
                          size="icon"
                          onClick={() => removeItem(item.produto.id)}
                          className="w-8 h-8 rounded-lg text-error border-error/30 hover:bg-error/10"
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* Informações da Comanda Ativa */}
            {items.length > 0 && comandaAtiva && (
              <div className="border-t border-border p-6 bg-success/5">
                <h3 className="text-lg font-semibold text-text-primary mb-4">Comanda Ativa</h3>
                
                <div className="space-y-3 p-4 bg-success/10 border border-success/30 rounded-2xl">
                  <div className="flex justify-between items-center">
                    <span className="text-sm text-text-secondary">Comanda:</span>
                    <span className="text-sm font-bold text-text-primary">#{comandaAtiva.numeroComanda}</span>
                  </div>

                  {comandaAtiva.numeroMesa && (
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-text-secondary">Mesa:</span>
                      <span className="text-sm font-bold text-text-primary">{comandaAtiva.numeroMesa}</span>
                    </div>
                  )}

                  {comandaAtiva.numeroPessoas && (
                    <div className="flex justify-between items-center">
                      <span className="text-sm text-text-secondary">Pessoas:</span>
                      <span className="text-sm font-bold text-text-primary">{comandaAtiva.numeroPessoas}</span>
                    </div>
                  )}

                  <div className="pt-2 border-t border-success/20">
                    <div className="text-sm text-text-secondary mb-1">Cliente:</div>
                    <div className="text-sm font-semibold text-text-primary">{comandaAtiva.cliente.nome}</div>
                    {comandaAtiva.cliente.cpfCnpj && (
                      <div className="text-xs text-text-muted">{comandaAtiva.cliente.cpfCnpj}</div>
                    )}
                  </div>
                </div>
              </div>
            )}

            {/* Aviso se não houver comanda ativa */}
            {items.length > 0 && !comandaAtiva && (
              <div className="border-t border-warning p-6 bg-warning/5">
                <div className="p-4 bg-warning/10 border border-warning/30 rounded-2xl text-center">
                  <p className="text-sm text-warning font-semibold">⚠️ Abra uma comanda antes de finalizar o pedido</p>
                </div>
              </div>
            )}

            {/* Seleção de Cliente */}
            {items.length > 0 && (
              <div className="border-t border-border p-6">
                <div className="mb-4">
                  <h3 className="text-lg font-semibold text-text-primary mb-3">Cliente (Opcional)</h3>
                  
                  {clienteSelecionado ? (
                    <div className="bg-card-secondary border border-border rounded-2xl p-4">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center space-x-3">
                          <div className="w-10 h-10 bg-primary/20 rounded-xl flex items-center justify-center">
                            <User className="w-5 h-5 text-primary" />
                          </div>
                          <div>
                            <h4 className="font-semibold text-text-primary">{clienteSelecionado.nomeCompleto}</h4>
                            {clienteSelecionado.documento && (
                              <p className="text-sm text-text-secondary">{clienteSelecionado.documento}</p>
                            )}
                          </div>
                        </div>
                        <button
                          onClick={() => setClienteSelecionado(null)}
                          className="w-8 h-8 bg-error/10 text-error rounded-xl flex items-center justify-center hover:bg-error/20 transition-colors"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  ) : (
                    <button
                      onClick={() => setShowClienteSearch(true)}
                      className="w-full bg-card-secondary border border-border rounded-2xl p-4 hover:bg-card-hover transition-colors text-left"
                    >
                      <div className="flex items-center space-x-3">
                        <div className="w-10 h-10 bg-primary/20 rounded-xl flex items-center justify-center">
                          <UserPlus className="w-5 h-5 text-primary" />
                        </div>
                        <div>
                          <h4 className="font-semibold text-text-primary">Selecionar Cliente</h4>
                          <p className="text-sm text-text-secondary">Buscar por nome, CPF/CNPJ ou telefone</p>
                        </div>
                      </div>
                    </button>
                  )}
                </div>
              </div>
            )}

            {/* Footer com Total e Botão Finalizar */}
            {items.length > 0 && (
              <div className="border-t border-border p-6 bg-background-secondary">
                <div className="flex justify-between items-center mb-4">
                  <span className="text-lg font-medium text-text-secondary">Total:</span>
                  <span className="text-2xl font-bold text-primary">
                    R$ {formatarPreco(totalPrice)}
                  </span>
                </div>
                
                <Button 
                  onClick={handleFinalizarPedido}
                  disabled={isProcessing || showSuccess}
                  className="w-full bg-primary text-primary-foreground py-4 rounded-2xl font-semibold text-lg hover:bg-primary/90 transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isProcessing ? (
                    <div className="flex items-center space-x-2">
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                      <span>{vendaEmEdicao ? 'Atualizando...' : 'Processando...'}</span>
                    </div>
                  ) : showSuccess ? (
                    <div className="flex items-center space-x-2">
                      <CheckCircle className="w-5 h-5" />
                      <span>{vendaEmEdicao ? 'Pedido Atualizado!' : 'Pedido Finalizado!'}</span>
                    </div>
                  ) : (
                    vendaEmEdicao ? 'Atualizar Pedido' : 'Finalizar Pedido'
                  )}
                </Button>
                
                <p className="text-center text-sm text-text-muted mt-3">
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

