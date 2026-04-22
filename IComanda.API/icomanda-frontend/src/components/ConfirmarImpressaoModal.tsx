import { AnimatePresence, motion } from 'framer-motion'
import { Printer, X, List } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { Button } from './ui/button'

interface ConfirmarImpressaoModalProps {
  isOpen: boolean
  onConfirm: (modo: 'novos' | 'completo', vias: 1 | 2) => void
  onCancel: () => void
  isEdicao: boolean
  quantidadeNovosItens?: number
  habilitarImprimirDuasVias?: boolean
}

const ConfirmarImpressaoModal: React.FC<ConfirmarImpressaoModalProps> = ({
  isOpen,
  onConfirm,
  onCancel,
  isEdicao,
  quantidadeNovosItens = 0,
  habilitarImprimirDuasVias = false
}) => {
  const [modo, setModo] = useState<'novos' | 'completo'>('novos')
  const [vias, setVias] = useState<1 | 2>(1)

  useEffect(() => {
    if (isOpen) {
      setModo('novos')
      setVias(1)
    }
  }, [isOpen])

  if (!isOpen) return null

  const handleConfirm = () => {
    onConfirm(modo, vias)
  }

  return (
    <AnimatePresence>
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        className="fixed inset-0 z-[150] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        onClick={onCancel}
      >
        <motion.div
          initial={{ scale: 0.9, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0.9, opacity: 0 }}
          onClick={(e) => e.stopPropagation()}
          className="bg-card rounded-3xl shadow-xl border border-border p-6 max-w-md w-full"
        >
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center space-x-3">
              <div className="w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center">
                <Printer className="w-6 h-6 text-primary" />
              </div>
              <h3 className="text-xl font-bold text-text-primary">
                Imprimir Pedido?
              </h3>
            </div>
            <button
              onClick={onCancel}
              className="w-8 h-8 flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-background-secondary rounded-lg transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          <div className="mb-6 space-y-3">
            {isEdicao ? (
              <>
                <p className="text-text-secondary">
                  Escolha o que deseja imprimir:
                </p>
                <div className="space-y-2">
                  <label className={`flex items-start gap-3 p-3 border rounded-2xl cursor-pointer transition-colors ${modo === 'novos' ? 'border-primary/60 bg-primary/5' : 'border-border'}`}>
                    <input
                      type="radio"
                      name="modo-impressao"
                      value="novos"
                      checked={modo === 'novos'}
                      onChange={() => setModo('novos')}
                      className="mt-1"
                    />
                    <div>
                      <div className="font-semibold text-text-primary">Apenas novos itens</div>
                      <div className="text-sm text-text-secondary">
                        Imprime somente o que foi adicionado agora.
                        {quantidadeNovosItens > 0 && (
                          <span className="block text-xs text-text-muted mt-1">
                            {quantidadeNovosItens} {quantidadeNovosItens === 1 ? 'item novo' : 'itens novos'} serão impressos.
                          </span>
                        )}
                      </div>
                    </div>
                  </label>

                  <label className={`flex items-start gap-3 p-3 border rounded-2xl cursor-pointer transition-colors ${modo === 'completo' ? 'border-primary/60 bg-primary/5' : 'border-border'}`}>
                    <input
                      type="radio"
                      name="modo-impressao"
                      value="completo"
                      checked={modo === 'completo'}
                      onChange={() => setModo('completo')}
                      className="mt-1"
                    />
                    <div>
                      <div className="font-semibold text-text-primary">Comanda completa</div>
                      <div className="text-sm text-text-secondary">
                        Imprime todos os itens da comanda.
                      </div>
                    </div>
                  </label>
                </div>
              </>
            ) : (
              <p className="text-text-secondary">
                Deseja imprimir o <strong>pedido completo</strong>?
              </p>
            )}
          </div>

          {!habilitarImprimirDuasVias && (
            <div className="flex space-x-3">
              <label className={`flex-1 flex flex-col items-center gap-1 p-3 border-2 rounded-2xl cursor-pointer transition-colors ${vias === 1 ? 'border-primary bg-primary/5' : 'border-border'}`}>
                <input
                  type="radio"
                  name="vias-impressao"
                  value={1}
                  checked={vias === 1}
                  onChange={() => setVias(1)}
                  className="sr-only"
                />
                <Printer className="w-5 h-5 text-primary" />
                <span className="text-sm font-semibold text-text-primary">1 Via</span>
                <span className="text-xs text-text-muted text-center">Balcão</span>
              </label>
              <label className={`flex-1 flex flex-col items-center gap-1 p-3 border-2 rounded-2xl cursor-pointer transition-colors ${vias === 2 ? 'border-primary bg-primary/5' : 'border-border'}`}>
                <input
                  type="radio"
                  name="vias-impressao"
                  value={2}
                  checked={vias === 2}
                  onChange={() => setVias(2)}
                  className="sr-only"
                />
                <div className="flex">
                  <Printer className="w-5 h-5 text-primary" />
                  <Printer className="w-5 h-5 text-primary -ml-1" />
                </div>
                <span className="text-sm font-semibold text-text-primary">2 Vias</span>
                <span className="text-xs text-text-muted text-center">Balcão + Bar/Cozinha</span>
              </label>
            </div>
          )}

          <div className="flex space-x-3 mt-4">
            <Button
              onClick={onCancel}
              variant="outline"
              className="flex-1"
            >
              Não Imprimir
            </Button>
            <Button
              onClick={handleConfirm}
              className="flex-1 bg-primary text-primary-foreground"
            >
              {isEdicao && modo === 'completo' ? <List className="w-4 h-4 mr-2" /> : <Printer className="w-4 h-4 mr-2" />}
              {isEdicao && modo === 'completo' ? 'Imprimir comanda completa' : 'Sim, Imprimir'}
            </Button>
          </div>
        </motion.div>
      </motion.div>
    </AnimatePresence>
  )
}

export default ConfirmarImpressaoModal

