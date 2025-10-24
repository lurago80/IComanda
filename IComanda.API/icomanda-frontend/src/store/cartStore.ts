import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { Produto } from '../types/api'

export interface CartItem {
  produto: Produto
  quantidade: number
  observacoes?: string
}

interface CartStore {
  items: CartItem[]
  isOpen: boolean
  addItem: (produto: Produto, quantidade?: number) => void
  removeItem: (produtoId: number) => void
  updateQuantity: (produtoId: number, quantidade: number) => void
  clearCart: () => void
  toggleCart: () => void
  setCartOpen: (isOpen: boolean) => void
  getTotalItems: () => number
  getTotalPrice: () => number
}

export const useCartStore = create<CartStore>()(
  persist(
    (set, get) => ({
      items: [],
      isOpen: false,

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
    }),
    {
      name: 'icomanda-cart',
      partialize: (state) => ({ items: state.items }),
    }
  )
)

