import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { Cliente, Produto } from '../types/api'

export interface CartItem {
  produto: Produto
  quantidade: number
  observacoes?: string
}

export interface ComandaAtiva {
  numeroComanda: number
  numeroMesa?: number
  numeroPessoas?: number
  cliente: Cliente
  dataAbertura: Date
}

export interface VendaEmEdicao {
  nota: string
  mesa?: number
  comanda?: number
}

interface CartStore {
  items: CartItem[]
  isOpen: boolean
  comandaAtiva: ComandaAtiva | null
  vendaEmEdicao: VendaEmEdicao | null
  addItem: (produto: Produto, quantidade?: number) => void
  removeItem: (produtoId: number) => void
  updateQuantity: (produtoId: number, quantidade: number) => void
  clearCart: () => void
  toggleCart: () => void
  setCartOpen: (isOpen: boolean) => void
  getTotalItems: () => number
  getTotalPrice: () => number
  setComandaAtiva: (comanda: ComandaAtiva | null) => void
  fecharComanda: () => void
  carregarPedidoParaEdicao: (venda: VendaEmEdicao, itens: Array<{descricao: string, qtd: number, precoUnitario: number}>) => void
  finalizarEdicao: () => void
}

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      items: [],
      isOpen: false,
      comandaAtiva: null,
      vendaEmEdicao: null,

      addItem: (produto, quantidade = 1) => {
        set((state) => {
          const existingItem = state.items.find(
            (item) => item.produto.id === produto.id
          )

          if (existingItem) {
            return {
              items: state.items.map((item) =>
                item.produto.id === produto.id
                  ? { ...item, quantidade: item.quantidade + quantidade }
                  : item
              ),
            }
          }

          return {
            items: [...state.items, { produto, quantidade }],
          }
        })
      },

      removeItem: (produtoId) => {
        set((state) => ({
          items: state.items.filter((item) => item.produto.id !== produtoId),
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

      clearCart: () => {
        set({ items: [] })
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
          (total, item) => total + item.produto.precoVenda * item.quantidade,
          0
        )
      },

      setComandaAtiva: (comanda) => {
        set({ comandaAtiva: comanda })
      },

      fecharComanda: () => {
        set({ 
          comandaAtiva: null,
          items: []
        })
      },

      carregarPedidoParaEdicao: (venda, itens) => {
        // Converte os itens da conferência para produtos do carrinho
        const cartItems: CartItem[] = itens.map((item, index) => ({
          produto: {
            id: index + 1000, // ID temporário para produtos da edição
            codigoBarra: '',
            codigoInterno: '',
            descricao: item.descricao,
            caracteristica: '',
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
          quantidade: item.qtd
        }))

        set({ 
          items: cartItems,
          vendaEmEdicao: venda,
          isOpen: true // Abre o carrinho automaticamente
        })
      },

      finalizarEdicao: () => {
        set({ 
          vendaEmEdicao: null,
          items: []
        })
      },
    }),
    {
      name: 'icomanda-cart',
      partialize: (state) => ({ 
        items: state.items,
        comandaAtiva: state.comandaAtiva,
        vendaEmEdicao: state.vendaEmEdicao
      }),
    }
  )
)

