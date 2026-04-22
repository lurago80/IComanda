import { AnimatePresence, motion } from 'framer-motion'
import { FileText, Loader2, Package, Printer, RefreshCw, X } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { useToast } from '../hooks/useToast'
import { vendasService } from '../services/api'
import { Venda } from '../types/api'
import { Button } from './ui/button'

interface ExtratoModalProps {
  isOpen: boolean
  onClose: () => void
  onGerarExtrato: (nota: string) => void
}

const ExtratoModal: React.FC<ExtratoModalProps> = ({
  isOpen,
  onClose,
  onGerarExtrato
}) => {
  const [vendas, setVendas] = useState<Venda[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const { showError } = useToast()

  useEffect(() => {
    if (isOpen) {
      carregarVendasAbertas()
    }
  }, [isOpen])

  const carregarVendasAbertas = async () => {
    setIsLoading(true)
    try {
      const vendasAbertas = await vendasService.getAbertas()
      vendasAbertas.sort((a: Venda, b: Venda) => {
        const nomeA = (a.nomeCliente || '').trim()
        const nomeB = (b.nomeCliente || '').trim()
        if (!nomeA && !nomeB) return 0
        if (!nomeA) return 1
        if (!nomeB) return -1
        return nomeA.localeCompare(nomeB, 'pt-BR')
      })
      setVendas(vendasAbertas)
    } catch (error: any) {
      console.error('Erro ao carregar vendas abertas:', error)
      showError('Erro', 'Não foi possível carregar as comandas abertas')
    } finally {
      setIsLoading(false)
    }
  }

  const formatarPreco = (valor: number) => {
    return valor.toFixed(2).replace('.', ',')
  }

  const formatarNota = (nota: string) => {
    return nota.padStart(6, '0')
  }

  if (!isOpen) return null

  return (
    <AnimatePresence>
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        className="fixed inset-0 z-[110] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        onClick={onClose}
      >
        <motion.div
          initial={{ scale: 0.95, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0.95, opacity: 0 }}
          className="bg-card w-full max-w-2xl max-h-[90vh] rounded-3xl shadow-large border border-border overflow-hidden flex flex-col"
          onClick={(e) => e.stopPropagation()}
        >
          {/* Header */}
          <div className="bg-primary text-primary-foreground p-6 flex justify-between items-center flex-shrink-0">
            <div className="flex items-center gap-3">
              <FileText className="w-6 h-6" />
              <h2 className="text-2xl font-bold">Gerar Extrato</h2>
            </div>
            <div className="flex items-center gap-2">
              <button
                onClick={carregarVendasAbertas}
                disabled={isLoading}
                className="p-2 hover:bg-white/20 rounded-xl transition-colors disabled:opacity-50"
                title="Atualizar lista"
              >
                <RefreshCw size={20} className={isLoading ? 'animate-spin' : ''} />
              </button>
              <button
                onClick={onClose}
                className="p-2 hover:bg-white/20 rounded-xl transition-colors"
              >
                <X size={24} />
              </button>
            </div>
          </div>

          {/* Content */}
          <div className="flex-1 overflow-y-auto p-6">
            {isLoading ? (
              <div className="flex flex-col items-center justify-center py-12 space-y-4">
                <Loader2 size={48} className="text-primary animate-spin" />
                <p className="text-lg text-text-secondary">Carregando comandas...</p>
              </div>
            ) : vendas.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 space-y-4">
                <Package className="w-16 h-16 text-text-muted" />
                <p className="text-lg font-semibold text-text-primary">Nenhuma comanda aberta</p>
                <p className="text-sm text-text-secondary">Todas as comandas foram finalizadas</p>
              </div>
            ) : (
              <div className="space-y-4">
                {vendas.map((venda, index) => (
                  <div
                    key={venda.nota && String(venda.nota).trim() ? venda.nota : `venda-${index}`}
                    className="bg-card-secondary rounded-2xl p-5 border border-border hover:border-primary/30 transition-all"
                  >
                    <div className="flex items-start justify-between mb-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2">
                          <span className="text-sm font-semibold text-text-secondary">Nota:</span>
                          <span className="text-lg font-bold text-primary">
                            {formatarNota(venda.nota)}
                          </span>
                        </div>
                        <div className="space-y-1 text-sm text-text-secondary">
                          {venda.comanda && (
                            <p>Comanda: <span className="font-semibold text-text-primary">{venda.comanda}</span></p>
                          )}
                          {venda.mesa && (
                            <p>Mesa: <span className="font-semibold text-text-primary">{venda.mesa}</span></p>
                          )}
                          {venda.nomeCliente && (
                            <p>Cliente: <span className="font-semibold text-primary">{venda.nomeCliente}</span></p>
                          )}
                          {venda.numeroPessoas && (
                            <p>Pessoas: <span className="font-semibold text-text-primary">{venda.numeroPessoas}</span></p>
                          )}
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="text-sm text-text-secondary mb-1">Total</p>
                        <p className="text-2xl font-bold text-primary">
                          R$ {formatarPreco(venda.total)}
                        </p>
                      </div>
                    </div>
                    <Button
                      onClick={() => {
                        onGerarExtrato(venda.nota)
                        onClose()
                      }}
                      className="w-full mt-3 bg-primary hover:bg-primary/90"
                    >
                      <Printer className="w-4 h-4 mr-2" />
                      Gerar Extrato
                    </Button>
                  </div>
                ))}
              </div>
            )}
          </div>
        </motion.div>
      </motion.div>
    </AnimatePresence>
  )
}

export default ExtratoModal

