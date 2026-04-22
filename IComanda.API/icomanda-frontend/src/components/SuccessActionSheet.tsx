import { AnimatePresence, motion } from 'framer-motion'
import { ClipboardList, History, Home } from 'lucide-react'
import React from 'react'

interface SuccessActionSheetProps {
  mode: 'nova' | 'edicao'
  isOpen: boolean
  onNovaComanda: () => void
  onBuscarComanda: () => void
  onVoltarMenu: () => void
  onClose: () => void
}

const SuccessActionSheet: React.FC<SuccessActionSheetProps> = ({
  mode,
  isOpen,
  onNovaComanda,
  onBuscarComanda,
  onVoltarMenu,
  onClose,
}) => {
  return (
    <AnimatePresence>
      {isOpen && (
        <>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 0.5 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/70 z-[130]"
            onClick={onClose}
          />

          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: 'spring', stiffness: 260, damping: 25 }}
            className="fixed bottom-0 left-0 right-0 z-[131] bg-card border-t border-border rounded-t-3xl p-4 sm:p-6 shadow-2xl max-w-md mx-auto sm:max-w-none"
          >
            <div className="text-center mb-4 sm:mb-5">
              <p className="text-xs sm:text-sm font-semibold text-primary mb-1">
                {mode === 'edicao' ? 'Edição atualizada' : 'Pedido finalizado'}
              </p>
              <h3 className="text-lg sm:text-xl font-bold text-text-primary">
                O que deseja fazer agora?
              </h3>
            </div>

            <div className="space-y-2 sm:space-y-3">
              <button
                onClick={onNovaComanda}
                className="w-full flex items-center justify-between bg-primary text-primary-foreground rounded-2xl px-4 sm:px-5 py-3 sm:py-4 shadow-lg hover:bg-primary/90 active:bg-primary/80 transition-colors"
              >
                <div className="flex items-center space-x-2 sm:space-x-3">
                  <div className="w-9 h-9 sm:w-10 sm:h-10 bg-white/20 rounded-xl flex items-center justify-center flex-shrink-0">
                    <ClipboardList className="w-4 h-4 sm:w-5 sm:h-5" />
                  </div>
                  <div className="text-left min-w-0 flex-1">
                    <p className="font-semibold text-sm sm:text-base">Abrir nova comanda</p>
                    <p className="text-xs opacity-80">Começar outro atendimento</p>
                  </div>
                </div>
              </button>

              <button
                onClick={onBuscarComanda}
                className="w-full flex items-center justify-between border border-border rounded-2xl px-4 sm:px-5 py-3 sm:py-4 hover:bg-card-hover active:bg-card-hover transition-colors"
              >
                <div className="flex items-center space-x-2 sm:space-x-3">
                  <div className="w-9 h-9 sm:w-10 sm:h-10 bg-background-secondary rounded-xl flex items-center justify-center flex-shrink-0">
                    <History className="w-4 h-4 sm:w-5 sm:h-5 text-text-secondary" />
                  </div>
                  <div className="text-left min-w-0 flex-1">
                    <p className="font-semibold text-sm sm:text-base">Buscar/editar comanda</p>
                    <p className="text-xs text-text-secondary">
                      Localizar mesa ou comanda existente
                    </p>
                  </div>
                </div>
              </button>

              <button
                onClick={onVoltarMenu}
                className="w-full flex items-center justify-between border border-border rounded-2xl px-4 sm:px-5 py-3 sm:py-4 hover:bg-card-hover active:bg-card-hover transition-colors"
              >
                <div className="flex items-center space-x-2 sm:space-x-3">
                  <div className="w-9 h-9 sm:w-10 sm:h-10 bg-background-secondary rounded-xl flex items-center justify-center flex-shrink-0">
                    <Home className="w-4 h-4 sm:w-5 sm:h-5 text-text-secondary" />
                  </div>
                  <div className="text-left min-w-0 flex-1">
                    <p className="font-semibold text-sm sm:text-base">Voltar para tela principal</p>
                    <p className="text-xs text-text-secondary">
                      Gerar conferência ou extrato
                    </p>
                  </div>
                </div>
              </button>
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  )
}

export default SuccessActionSheet

