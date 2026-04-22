import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { Cliente, Produto } from '../types/api'

export type FlowState = 'idle' | 'nova' | 'edicao'

export interface CartItem {
  produto: Produto
  quantidade: number
  observacoes?: string
  /** Preço unitário override (ex.: taxa de entrega selecionada). Se definido, usa em vez de produto.precoVenda */
  precoOverride?: number
  /** ISO timestamp registrado no momento em que o item foi adicionado ao carrinho */
  adicionadoEm: string
}

export interface ComandaAtiva {
  numeroComanda: number
  numeroMesa?: number
  numeroPessoas?: number
  cliente?: Cliente
  /** Nome para exibição na comanda quando cliente não foi cadastrado (id = 0) */
  nomeClienteExibicao?: string
  dataAbertura: Date
}

export interface VendaEmEdicao {
  nota: string
  mesa?: number
  comanda?: number
  /** Nome do cliente quando não cadastrado (nome_cliente da venda) - exibir na tela Editar Pedido */
  nomeCliente?: string
}

interface CartStore {
  items: CartItem[]
  itensOriginais: CartItem[] // Itens originais quando carregar para edição
  isOpen: boolean
  comandaAtiva: ComandaAtiva | null
  vendaEmEdicao: VendaEmEdicao | null
  clienteEdicao: Cliente | null
  flowState: FlowState
  /** Caixa Rápido / PDV: venda sem comanda, cliente = 0; ao finalizar vai para recebimento e imprime cupom */
  caixaRapidoMode: boolean
  /** Delivery: venda com origem DL; cliente obrigatório; sem comanda/mesa */
  deliveryMode: boolean
  addItem: (produto: Produto, quantidade?: number, precoOverride?: number) => void
  removeItem: (produtoId: number) => void
  removeItemAt: (index: number) => void
  updateQuantity: (produtoId: number, quantidade: number) => void
  updateQuantityAt: (index: number, quantidade: number) => void
  updateObservacao: (index: number, observacao: string) => void
  clearCart: () => void
  toggleCart: () => void
  setCartOpen: (isOpen: boolean) => void
  getTotalItems: () => number
  getTotalPrice: () => number
  setComandaAtiva: (comanda: ComandaAtiva | null) => void
  setCaixaRapidoMode: (value: boolean) => void
  setDeliveryMode: (value: boolean) => void
  /** Cliente pré-selecionado ao iniciar novo pedido delivery (busca/cadastro na tela Delivery) */
  deliveryClientePreSelecionado: Cliente | null
  setDeliveryClientePreSelecionado: (cliente: Cliente | null) => void
  fecharComanda: () => void
  carregarPedidoParaEdicao: (venda: VendaEmEdicao, itens: Array<{codigo: number, descricao: string, qtd: number, precoUnitario: number, observacao?: string}>, cliente?: Cliente) => void
  finalizarEdicao: () => void
  setFlowState: (state: FlowState) => void
  getNovosItens: () => CartItem[] // Retorna apenas os itens novos (não estavam nos originais)
}

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      items: [],
      itensOriginais: [], // Itens originais quando carregar para edição
      isOpen: false,
      comandaAtiva: null,
      vendaEmEdicao: null,
      clienteEdicao: null,
      flowState: 'idle',
      caixaRapidoMode: false,
      deliveryMode: false,
      deliveryClientePreSelecionado: null,

      setCaixaRapidoMode: (value) => set({ caixaRapidoMode: value }),
      setDeliveryMode: (value) => set({ deliveryMode: value }),
      setDeliveryClientePreSelecionado: (cliente) => set({ deliveryClientePreSelecionado: cliente }),

      addItem: (produto, quantidade = 1, precoOverride) => {
        // Sempre cria nova entrada com timestamp próprio, mesmo que o produto já exista no carrinho.
        // Isso preserva a hora individual de cada lançamento na verificação de itens.
        set((state) => ({
          items: [...state.items, { produto, quantidade, precoOverride, adicionadoEm: new Date().toISOString() }],
        }))
      },

      removeItem: (produtoId) => {
        set((state) => ({
          items: state.items.filter((item) => item.produto.id !== produtoId),
        }))
      },

      removeItemAt: (index) => {
        set((state) => ({
          items: state.items.filter((_, i) => i !== index),
        }))
      },

      updateQuantity: (produtoId, quantidade) => {
        if (quantidade <= 0) {
          get().removeItem(produtoId)
          return
        }

        set((state) => ({
          items: state.items.map((item) =>
            item.produto.id === produtoId
              ? { ...item, quantidade }
              : item
          ),
        }))
      },

      updateQuantityAt: (index, quantidade) => {
        if (quantidade <= 0) {
          get().removeItemAt(index)
          return
        }
        set((state) => ({
          items: state.items.map((item, i) =>
            i === index ? { ...item, quantidade } : item
          ),
        }))
      },

      updateObservacao: (index, observacao) => {
        set((state) => ({
          items: state.items.map((item, i) =>
            i === index ? { ...item, observacoes: observacao || undefined } : item
          ),
        }))
      },

      clearCart: () => {
        set({ items: [], itensOriginais: [] })
      },

      toggleCart: () => {
        set((state) => ({ isOpen: !state.isOpen }))
      },

      setCartOpen: (isOpen) => {
        set({ isOpen })
      },

      getTotalItems: () => {
        return get().items.reduce((total, item) => total + item.quantidade, 0)
      },

      getTotalPrice: () => {
        return get().items.reduce(
          (total, item) => total + (item.precoOverride ?? item.produto.precoVenda) * item.quantidade,
          0
        )
      },

      setComandaAtiva: (comanda) => {
        set({ 
          comandaAtiva: comanda,
          flowState: comanda ? 'nova' : 'idle',
          // Ao abrir nova comanda, limpar carrinho para não puxar produtos da comanda anterior
          ...(comanda ? { items: [], itensOriginais: [], vendaEmEdicao: null, clienteEdicao: null } : {})
        })
      },

      fecharComanda: () => {
        set({ 
          comandaAtiva: null,
          items: [],
          flowState: 'idle',
          caixaRapidoMode: false
        })
      },

      carregarPedidoParaEdicao: (venda, itens, cliente) => {
        const sanitize = (value?: string) => {
          if (!value) return ''
          // Remove quebras de linha, tabs e outros caracteres de controle que causam texto quebrado
          return value
            .replace(/[\r\n\t\f\v]+/g, ' ') // Remove todos os caracteres de controle de linha
            .replace(/\s+/g, ' ') // Normaliza espaços múltiplos
            .trim()
        }

        // Converte os itens para o formato do carrinho (permite lista vazia para incluir itens depois)
        console.log('🔄 Carregando pedido para edição:', { venda, itensCount: itens?.length ?? 0, itens })
        
        const list = itens ?? []
        const cartItems: CartItem[] = list.map((item, index) => {
          // Sanitizar descrição - se vier vazia do backend, manter vazio (será tratado no componente)
          const descricao = sanitize(item.descricao)
          
          console.log(`📦 Item ${index}:`, { 
            codigo: item.codigo, 
            descricao: descricao || '(vazia)', 
            qtd: item.qtd, 
            preco: item.precoUnitario 
          })
          
          return {
            produto: {
              id: item.codigo, // Usar código real do produto
              codigoBarra: '',
              codigoInterno: '',
              descricao: descricao, // Backend já deve preencher, mas pode estar vazio
              caracteristica: '', // Não temos característica na conferência
              quantidade: item.qtd,
              precoCusto: 0,
              precoVenda: item.precoUnitario,
              atacado: 0,
              preco3: 0,
              unMedida: 'UN',
              ativo: true,
              grupo: 0,
              pesavel: false
            },
            quantidade: item.qtd,
            observacoes: item.observacao || undefined,
            adicionadoEm: new Date().toISOString()
          }
        })
        
        console.log('✅ CartItems criados:', cartItems.length, cartItems)
        console.log('💾 Salvando no store...')

        set({ 
          items: cartItems,
          itensOriginais: [...cartItems], // Salvar cópia dos itens originais
          vendaEmEdicao: {
            ...venda,
            nota: String(venda.nota).padStart(6, '0'), // Garantir formato correto da nota
            nomeCliente: venda.nomeCliente?.trim() || undefined
          },
          clienteEdicao: cliente || null, // Armazenar cliente da venda
          flowState: 'edicao',
          isOpen: false // NÃO abrir drawer automaticamente - deixar usuário adicionar itens primeiro
        })
        
        // Verificar se foi salvo corretamente
        setTimeout(() => {
          const state = get()
          console.log('🔍 Estado após salvar:', { 
            itemsCount: state.items.length, 
            vendaEmEdicao: state.vendaEmEdicao,
            flowState: state.flowState
          })
        }, 100)
      },

      finalizarEdicao: () => {
        set({ 
          vendaEmEdicao: null,
          clienteEdicao: null,
          items: [],
          itensOriginais: [], // Limpar itens originais também
          flowState: 'idle'
        })
      },

      getNovosItens: () => {
        const state = get()
        if (!state.vendaEmEdicao || state.itensOriginais.length === 0) {
          // Se não está em edição, todos os itens são "novos" (será todo o pedido)
          return state.items
        }
        
        // Comparar itens atuais com originais para encontrar apenas os novos.
        // addItem sempre cria uma NOVA entrada separada mesmo para o mesmo produto,
        // portanto precisamos "consumir" originais um a um para não confundir
        // entradas duplicadas do mesmo produto adicionadas pelo usuário.
        const usedOriginalIndices = new Set<number>()
        const novosItens: CartItem[] = []
        
        state.items.forEach(itemAtual => {
          // Buscar um original ainda não utilizado para este produto
          const originalIndex = state.itensOriginais.findIndex(
            (orig, idx) =>
              orig.produto.id === itemAtual.produto.id && !usedOriginalIndices.has(idx)
          )

          if (originalIndex === -1) {
            // Sem original disponível → item completamente novo
            novosItens.push(itemAtual)
          } else {
            // Marcar este original como consumido para não reutilizá-lo
            usedOriginalIndices.add(originalIndex)
            const itemOriginal = state.itensOriginais[originalIndex]

            if (itemAtual.quantidade > itemOriginal.quantidade) {
              // Mesma entrada original, mas quantidade aumentou → imprimir a diferença
              novosItens.push({
                ...itemAtual,
                quantidade: itemAtual.quantidade - itemOriginal.quantidade
              })
            }
            // Se quantidade igual ou menor → não é novo, não imprimir
          }
        })
        
        return novosItens
      },

      setFlowState: (state) => {
        set({ flowState: state })
      },
    }),
    {
      name: 'icomanda-cart',
      partialize: (state) => ({ 
        items: state.items,
        comandaAtiva: state.comandaAtiva
        // vendaEmEdicao e flowState NÃO são persistidos
        // Eles devem ser resetados quando a aplicação é fechada
        // Uma venda em edição é uma sessão ativa, não deve ser mantida entre sessões
      }),
      onRehydrateStorage: () => (state) => {
        // Garantir que vendaEmEdicao e flowState sempre sejam resetados na inicialização
        if (state) {
          state.vendaEmEdicao = null
          state.flowState = 'idle'
        }
      },
    }
  )
)

