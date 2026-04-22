import { AnimatePresence, motion } from 'framer-motion'
import { Loader2, Package, TrendingUp, X } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { useToast } from '../hooks/useToast'
import { vendasService, receberService } from '../services/api'
import { Venda, ContasAberto } from '../types/api'
import { Button } from './ui/button'

interface TotalComandasAbertasModalProps {
  isOpen: boolean
  onClose: () => void
  onEditarComanda?: (nota: string) => void
}

const TotalComandasAbertasModal: React.FC<TotalComandasAbertasModalProps> = ({
  isOpen,
  onClose,
  onEditarComanda
}) => {
  const [vendas, setVendas] = useState<Venda[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [contasAbertoPorNota, setContasAbertoPorNota] = useState<Map<string, ContasAberto>>(new Map())
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
      // Ordenar por nome do cliente (alfabético; sem nome vai para o final)
      vendasAbertas.sort((a: Venda, b: Venda) => {
        const nomeA = (a.nomeCliente || '').trim()
        const nomeB = (b.nomeCliente || '').trim()
        if (!nomeA && !nomeB) return 0
        if (!nomeA) return 1
        if (!nomeB) return -1
        return nomeA.localeCompare(nomeB, 'pt-BR')
      })
      // Exibir vendas imediatamente — sem esperar contas em aberto
      setVendas(vendasAbertas)
      setIsLoading(false)

      // Carregar contas em aberto sequencialmente em background para não exceder rate limit
      const contasMap = new Map<string, ContasAberto>()
      const vendasComCliente = vendasAbertas.filter((v: Venda) => v.cliente && v.cliente > 0)
      for (const v of vendasComCliente) {
        try {
          const contas = await receberService.verificarContasAberto(v.cliente!)
          if (contas.temContasAberto) {
            contasMap.set(v.nota, contas)
            setContasAbertoPorNota(new Map(contasMap))
          }
        } catch { /* ignorar */ }
      }
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

  const totalValor = vendas.reduce((acc, venda) => acc + (venda.total || 0), 0)

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
          <div className="bg-gradient-to-r from-primary to-primary-700 text-primary-foreground p-6 flex justify-between items-center flex-shrink-0">
            <div className="flex items-center gap-3">
              <TrendingUp className="w-6 h-6" />
              <h2 className="text-2xl font-bold">Resumo de Comandas Abertas</h2>
            </div>
            <button
              onClick={onClose}
              className="p-2 hover:bg-white/20 rounded-xl transition-colors"
            >
              <X size={24} />
            </button>
          </div>

          {/* Resumo */}
          <div className="p-6 bg-gradient-to-r from-primary/10 via-primary/5 to-accent/10 border-b border-border">
            <div className="grid grid-cols-2 gap-4">
              <div className="bg-card rounded-2xl p-4 border border-border">
                <p className="text-sm text-text-secondary mb-1">Total de Comandas</p>
                <p className="text-3xl font-bold text-primary">{vendas.length}</p>
              </div>
              <div className="bg-card rounded-2xl p-4 border border-border">
                <p className="text-sm text-text-secondary mb-1">Valor Total</p>
                <p className="text-3xl font-bold text-primary">
                  R$ {formatarPreco(totalValor)}
                </p>
              </div>
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
                    key={`venda-${String(venda.nota || '').trim() || 'n'}-${index}`}
                    className="bg-card-secondary rounded-2xl p-5 border border-border hover:border-primary/30 transition-all"
                  >
                    <div className="flex items-start justify-between mb-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2 flex-wrap">
                          <span className="text-sm font-semibold text-text-secondary">Nota:</span>
                          <span className="text-lg font-bold text-primary">
                            {formatarNota(venda.nota)}
                          </span>
                          {contasAbertoPorNota.get(venda.nota)?.temContasAberto && contasAbertoPorNota.get(venda.nota)!.valorTotalPendente > 0 && (
                            <span className="text-red-600 font-bold text-sm px-2 py-0.5 rounded-md bg-red-50 border border-red-200" title={contasAbertoPorNota.get(venda.nota)?.mensagem}>
                              ⚠️ Cliente deve
                            </span>
                          )}
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
                    {onEditarComanda && (
                      <Button
                        onClick={() => {
                          onEditarComanda(venda.nota)
                          onClose()
                        }}
                        className="w-full mt-3"
                        variant="outline"
                      >
                        Editar Comanda
                      </Button>
                    )}
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

export default TotalComandasAbertasModal

