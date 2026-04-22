import React, { useCallback, useEffect, useState } from 'react'
import {
  ArrowLeft, Plus, Search, RefreshCw, CheckCircle2, Clock,
  XCircle, Truck, BarChart2, Calendar, User, Users, Trophy
} from 'lucide-react'
import { forcaVendasService } from '../services/api'
import { PedidoFV, Vendedor } from '../types/api'
import { Button } from '../components/ui/button'
import { useToast } from '../hooks/useToast'
import NavModulos from '../components/NavModulos'

interface ForcaVendasPageProps {
  onClose: () => void
  onNovoPedido: () => void
  onDashboard: (idVendedor: number) => void
  onCadastroVendedores?: () => void
  onRanking?: () => void
  onIrParaComanda?: () => void
  onIrParaDelivery?: () => void
}

const STATUS_MAP: Record<number, { label: string; color: string; icon: React.ReactNode }> = {
  0: { label: 'Pendente',  color: 'bg-yellow-100 text-yellow-800',  icon: <Clock className="w-3 h-3" /> },
  1: { label: 'Aprovado',  color: 'bg-blue-100 text-blue-800',      icon: <CheckCircle2 className="w-3 h-3" /> },
  2: { label: 'Faturado',  color: 'bg-green-100 text-green-800',    icon: <Truck className="w-3 h-3" /> },
  3: { label: 'Cancelado', color: 'bg-red-100 text-red-800',        icon: <XCircle className="w-3 h-3" /> },
}

const MESES = ['Jan','Fev','Mar','Abr','Mai','Jun','Jul','Ago','Set','Out','Nov','Dez']

const ForcaVendasPage: React.FC<ForcaVendasPageProps> = ({ onClose, onNovoPedido, onDashboard, onCadastroVendedores, onRanking, onIrParaComanda, onIrParaDelivery }) => {
  const [pedidos, setPedidos]           = useState<PedidoFV[]>([])
  const [vendedores, setVendedores]     = useState<Vendedor[]>([])
  const [filtroVendedor, setFiltroVendedor] = useState<number | ''>('')
  const [filtroStatus, setFiltroStatus] = useState<number | ''>('')
  const [isLoading, setIsLoading]       = useState(true)
  const [isAprovando, setIsAprovando]   = useState<number | null>(null)
  const { showSuccess, showError }      = useToast()

  const carregar = useCallback(async () => {
    setIsLoading(true)
    try {
      const params: any = {}
      if (filtroVendedor !== '') params.idVendedor = filtroVendedor
      if (filtroStatus   !== '') params.status     = filtroStatus
      const [p, v] = await Promise.all([
        forcaVendasService.getPedidos(params),
        forcaVendasService.getVendedoresAtivos(),
      ])
      setPedidos(p)
      setVendedores(v)
    } catch {
      showError('Erro ao carregar pedidos FV')
    } finally {
      setIsLoading(false)
    }
  }, [filtroVendedor, filtroStatus, showError])

  useEffect(() => { carregar() }, [carregar])

  const handleAprovar = async (id: number) => {
    setIsAprovando(id)
    try {
      await forcaVendasService.atualizarStatusPedido(id, { status: 1 })
      showSuccess('Pedido aprovado com sucesso!')
      carregar()
    } catch {
      showError('Erro ao aprovar pedido')
    } finally {
      setIsAprovando(null)
    }
  }

  const handleCancelar = async (id: number) => {
    const motivo = window.prompt('Informe o motivo do cancelamento:')
    if (!motivo) return
    setIsAprovando(id)
    try {
      await forcaVendasService.atualizarStatusPedido(id, { status: 3, motivo })
      showSuccess('Pedido cancelado.')
      carregar()
    } catch {
      showError('Erro ao cancelar pedido')
    } finally {
      setIsAprovando(null)
    }
  }

  const formatarMoeda = (v: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)

  const formatarData = (d: string) =>
    new Date(d).toLocaleDateString('pt-BR')

  const totalPendente = pedidos.filter(p => p.statusCodigo === 0).reduce((s, p) => s + p.total, 0)
  const totalAprovado = pedidos.filter(p => p.statusCodigo === 1).reduce((s, p) => s + p.total, 0)

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white border-b shadow-sm">
        <div className="px-4 py-3 flex items-center gap-3">
          <button onClick={onClose} className="p-1.5 hover:bg-gray-100 rounded-lg">
            <ArrowLeft className="w-5 h-5 text-gray-600" />
          </button>
          <div className="flex-1">
            <h1 className="font-bold text-gray-900 text-lg">Força de Vendas</h1>
            <p className="text-xs text-gray-500">{pedidos.length} pedido(s)</p>
          </div>
          <button
            onClick={carregar}
            className="p-1.5 hover:bg-gray-100 rounded-lg"
            title="Recarregar"
          >
            <RefreshCw className="w-4 h-4 text-gray-500" />
          </button>
          {onCadastroVendedores && (
            <button
              onClick={onCadastroVendedores}
              className="flex items-center gap-1.5 text-xs border border-indigo-200 text-indigo-700 px-3 py-1.5 rounded-lg hover:bg-indigo-50"
              title="Cadastro de Vendedores"
            >
              <Users className="w-4 h-4" />
              Vendedores
            </button>
          )}
          {onRanking && (
            <button
              onClick={onRanking}
              className="flex items-center gap-1.5 text-xs border border-yellow-300 text-yellow-700 px-3 py-1.5 rounded-lg hover:bg-yellow-50"
              title="Ranking de Vendedores"
            >
              <Trophy className="w-4 h-4" />
              Ranking
            </button>
          )}
          <Button onClick={onNovoPedido} className="flex items-center gap-1.5 text-sm">
            <Plus className="w-4 h-4" />
            Novo Pedido
          </Button>
        </div>
        {(onIrParaComanda || onIrParaDelivery) && (
          <NavModulos
            moduloAtivo="forca-vendas"
            onComanda={onIrParaComanda}
            onDelivery={onIrParaDelivery}
            onForcaVendas={undefined}
          />
        )}
      </div>

      {/* Cards resumo */}
      <div className="grid grid-cols-2 gap-3 p-4">
        <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-3">
          <p className="text-xs text-yellow-700 font-medium">Pendentes</p>
          <p className="text-lg font-bold text-yellow-900">{formatarMoeda(totalPendente)}</p>
          <p className="text-xs text-yellow-600">{pedidos.filter(p => p.statusCodigo === 0).length} pedido(s)</p>
        </div>
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-3">
          <p className="text-xs text-blue-700 font-medium">Aprovados</p>
          <p className="text-lg font-bold text-blue-900">{formatarMoeda(totalAprovado)}</p>
          <p className="text-xs text-blue-600">{pedidos.filter(p => p.statusCodigo === 1).length} pedido(s)</p>
        </div>
      </div>

      {/* Acesso rápido por vendedor */}
      {vendedores.length > 0 && (
        <div className="px-4 mb-3">
          <p className="text-xs text-gray-500 font-medium mb-2 uppercase tracking-wide">Dashboard por vendedor</p>
          <div className="flex gap-2 overflow-x-auto pb-1">
            {vendedores.map(v => (
              <button
                key={v.id}
                onClick={() => onDashboard(v.id)}
                className="flex-shrink-0 flex items-center gap-2 bg-white border border-gray-200 rounded-lg px-3 py-2 hover:bg-gray-50 shadow-sm"
              >
                <div className="w-7 h-7 rounded-full bg-indigo-100 flex items-center justify-center">
                  <User className="w-3.5 h-3.5 text-indigo-600" />
                </div>
                <div className="text-left">
                  <p className="text-xs font-medium text-gray-800">{v.nome.split(' ')[0]}</p>
                  <p className="text-xs text-gray-400">{v.percentualMeta.toFixed(0)}% meta</p>
                </div>
                <BarChart2 className="w-3.5 h-3.5 text-indigo-400" />
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Filtros */}
      <div className="px-4 flex gap-2 mb-3">
        <select
          value={filtroVendedor}
          onChange={e => setFiltroVendedor(e.target.value === '' ? '' : Number(e.target.value))}
          className="flex-1 text-sm border border-gray-300 rounded-lg px-3 py-2 bg-white"
        >
          <option value="">Todos os vendedores</option>
          {vendedores.map(v => (
            <option key={v.id} value={v.id}>{v.nome}</option>
          ))}
        </select>
        <select
          value={filtroStatus}
          onChange={e => setFiltroStatus(e.target.value === '' ? '' : Number(e.target.value))}
          className="flex-1 text-sm border border-gray-300 rounded-lg px-3 py-2 bg-white"
        >
          <option value="">Todos os status</option>
          <option value={0}>Pendente</option>
          <option value={1}>Aprovado</option>
          <option value={2}>Faturado</option>
          <option value={3}>Cancelado</option>
        </select>
      </div>

      {/* Lista de pedidos */}
      <div className="flex-1 px-4 pb-6 space-y-3 overflow-y-auto">
        {isLoading ? (
          <div className="flex justify-center py-12">
            <RefreshCw className="w-8 h-8 text-gray-400 animate-spin" />
          </div>
        ) : pedidos.length === 0 ? (
          <div className="text-center py-12 text-gray-400">
            <Search className="w-10 h-10 mx-auto mb-2 opacity-40" />
            <p className="text-sm">Nenhum pedido encontrado</p>
          </div>
        ) : (
          pedidos.map(pedido => {
            const st = STATUS_MAP[pedido.statusCodigo] ?? STATUS_MAP[0]
            const isPending = pedido.statusCodigo === 0
            return (
              <div key={pedido.id} className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
                {/* Cabeçalho do card */}
                <div className="px-4 pt-3 pb-2 flex items-start justify-between">
                  <div>
                    <p className="font-semibold text-gray-900 text-sm">{pedido.nomeCliente}</p>
                    <p className="text-xs text-gray-500">{pedido.nomeVendedor} · {formatarData(pedido.dataPedido)}</p>
                  </div>
                  <span className={`flex items-center gap-1 text-xs font-medium px-2 py-0.5 rounded-full ${st.color}`}>
                    {st.icon} {st.label}
                  </span>
                </div>

                {/* Itens resumo */}
                <div className="px-4 py-1 border-t border-gray-50">
                  {pedido.itens.slice(0, 2).map(item => (
                    <p key={item.id} className="text-xs text-gray-600 truncate">
                      {item.quantidade}x {item.descricaoProduto}
                    </p>
                  ))}
                  {pedido.itens.length > 2 && (
                    <p className="text-xs text-gray-400">+{pedido.itens.length - 2} item(s)...</p>
                  )}
                </div>

                {/* Rodapé */}
                <div className="px-4 pb-3 pt-2 flex items-center justify-between border-t border-gray-50">
                  <p className="font-bold text-gray-900">{formatarMoeda(pedido.total)}</p>
                  <div className="flex gap-2">
                    {isPending && (
                      <>
                        <button
                          onClick={() => handleCancelar(pedido.id)}
                          disabled={isAprovando === pedido.id}
                          className="text-xs text-red-600 border border-red-200 px-2.5 py-1 rounded-lg hover:bg-red-50"
                        >
                          Cancelar
                        </button>
                        <button
                          onClick={() => handleAprovar(pedido.id)}
                          disabled={isAprovando === pedido.id}
                          className="text-xs text-white bg-green-600 px-2.5 py-1 rounded-lg hover:bg-green-700 flex items-center gap-1"
                        >
                          {isAprovando === pedido.id ? (
                            <RefreshCw className="w-3 h-3 animate-spin" />
                          ) : (
                            <CheckCircle2 className="w-3 h-3" />
                          )}
                          Aprovar
                        </button>
                      </>
                    )}
                    {pedido.statusCodigo === 1 && (
                      <button
                        onClick={() => forcaVendasService.atualizarStatusPedido(pedido.id, { status: 2, notaFiscal: window.prompt('Número da NF:') || '' }).then(carregar)}
                        className="text-xs text-white bg-blue-600 px-2.5 py-1 rounded-lg hover:bg-blue-700"
                      >
                        Faturar
                      </button>
                    )}
                  </div>
                </div>
              </div>
            )
          })
        )}
      </div>
    </div>
  )
}

export default ForcaVendasPage
