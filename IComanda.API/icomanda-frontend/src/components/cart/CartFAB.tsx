import { AnimatePresence, motion } from 'framer-motion'
import { ShoppingCart } from 'lucide-react'
import React from 'react'
import { useCartStore } from '../../store/cartStore'

const CartFAB: React.FC = () => {
  const { toggleCart, getTotalItems, getTotalPrice } = useCartStore()
  const totalItems = getTotalItems()
  const totalPrice = getTotalPrice()

  if (totalItems === 0) return null

  return (
    <AnimatePresence>
      <motion.div
        initial={{ y: 100, opacity: 0, scale: 0.8 }}
        animate={{ y: 0, opacity: 1, scale: 1 }}
        exit={{ y: 100, opacity: 0, scale: 0.8 }}
        transition={{ type: "spring", stiffness: 300, damping: 30 }}
        className="fixed bottom-6 left-4 right-4 z-50"
      >
        <button
          onClick={toggleCart}
          className="w-full bg-gradient-to-r from-primary via-primary/95 to-primary/90 text-white rounded-3xl py-6 px-6 shadow-glow-lg flex items-center justify-between hover:shadow-glow transition-all duration-300 backdrop-blur-md border border-primary/30 hover:scale-105 active:scale-95"
        >
          <div className="flex items-center space-x-5">
            <div className="w-14 h-14 bg-white/20 rounded-3xl flex items-center justify-center backdrop-blur-sm border border-white/10">
              <ShoppingCart className="w-7 h-7" />
            </div>
            <div className="text-left">
              <p className="font-bold text-xl">Ver comanda</p>
              <p className="text-sm opacity-90 font-medium">
                {totalItems} {totalItems === 1 ? 'item' : 'itens'} no carrinho
              </p>
            </div>
          </div>
          
          <div className="text-right">
            <p className="text-2xl font-bold">
              R$ {totalPrice.toFixed(2).replace('.', ',')}
            </p>
            <p className="text-xs opacity-75">Total</p>
          </div>
        </button>
      </motion.div>
    </AnimatePresence>
  )
}

export default CartFAB
