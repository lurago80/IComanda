import React, { useCallback, useEffect, useState } from 'react'
import { ArrowLeft, RefreshCw, TrendingUp, ShoppingBag, MapPin, CheckCircle2, Clock } from 'lucide-react'
import { forcaVendasService } from '../services/api'
import { DashboardVendedor } from '../types/api'
import { useToast } from '../hooks/useToast'

interface DashboardVendedorPageProps {
  idVendedor: number
  onClose: () => void
}

const DashboardVendedorPage: React.FC<DashboardVendedorPageProps> = ({ idVendedor, onClose }) => {
  const [dash, setDash]         = useState<DashboardVendedor | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const { showError }           = useToast()

  const carregar = useCallback(async () => {
    setIsLoading(true)
    try {
      const d = await forcaVendasService.getDashboard(idVendedor)
      setDash(d)
    } catch {
      showError('Erro ao carregar dashboard do vendedor')
    } finally {
      setIsLoading(false)
    }
  }, [idVendedor, showError])

  useEffect(() => { carregar() }, [carregar])

  const fmtMoeda = (v: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)

  const fmtHora = (d: string) => new Date(d).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })

  const statusVisita = (statusCodigo: number) => {
    const map: Record<number, { label: string; color: string }> = {
      0: { label: 'Agendada',      color: 'text-yellow-700 bg-yellow-100' },
      1: { label: 'Em andamento',  color: 'text-blue-700 bg-blue-100' },
      2: { label: 'Concluída',     color: 'text-green-700 bg-green-100' },
      3: { label: 'Não realizada', color: 'text-red-700 bg-red-100' },
    }
    return map[statusCodigo] ?? map[0]
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <RefreshCw className="w-8 h-8 text-gray-400 animate-spin" />
      </div>
    )
  }

  if (!dash) return null

  const pct = Math.min(dash.percentualMeta ?? 0, 100)
  const comissaoEstimada = dash.comissaoEstimada ?? 0

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white border-b px-4 py-3 flex items-center gap-3 shadow-sm">
        <button onClick={onClose} className="p-1.5 hover:bg-gray-100 rounded-lg">
          <ArrowLeft className="w-5 h-5 text-gray-600" />
        </button>
        <div className="flex-1">
          <h1 className="font-bold text-gray-900 text-lg">{dash.nomeVendedor}</h1>
          <p className="text-xs text-gray-500">Dashboard do vendedor</p>
        </div>
        <button onClick={carregar} className="p-1.5 hover:bg-gray-100 rounded-lg">
          <RefreshCw className="w-4 h-4 text-gray-500" />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">

        {/* Meta do mês */}
        {dash.metaMes > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center gap-2">
                <TrendingUp className="w-5 h-5 text-indigo-600" />
                <p className="font-semibold text-gray-800">Meta do Mês</p>
              </div>
              <span className={`text-sm font-bold ${pct >= 100 ? 'text-green-600' : 'text-indigo-600'}`}>
                {pct.toFixed(1)}%
              </span>
            </div>
            {/* Barra de progresso */}
            <div className="w-full bg-gray-100 rounded-full h-3 mb-3">
              <div
                className={`h-3 rounded-full transition-all duration-700 ${pct >= 100 ? 'bg-green-500' : 'bg-indigo-500'}`}
                style={{ width: `${pct}%` }}
              />
            </div>
            <div className="flex justify-between text-xs text-gray-500">
              <span>Vendido: {fmtMoeda(dash.vendasMes)}</span>
              <span>Meta: {fmtMoeda(dash.metaMes)}</span>
            </div>
            {comissaoEstimada > 0 && (
              <div className="mt-2 pt-2 border-t border-gray-100">
                <p className="text-xs text-gray-500">
                  Comissão estimada: <span className="font-semibold text-green-600">{fmtMoeda(comissaoEstimada)}</span>
                </p>
              </div>
            )}
          </div>
        )}

        {/* Resumo pedidos */}
        <div className="grid grid-cols-3 gap-2">
          <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-3 text-center">
            <ShoppingBag className="w-5 h-5 text-yellow-600 mx-auto mb-1" />
            <p className="text-xl font-bold text-yellow-900">{dash.totalPedidosPendentes}</p>
            <p className="text-xs text-yellow-600">Pendentes</p>
          </div>
          <div className="bg-blue-50 border border-blue-200 rounded-xl p-3 text-center">
            <CheckCircle2 className="w-5 h-5 text-blue-600 mx-auto mb-1" />
            <p className="text-xl font-bold text-blue-900">{dash.totalPedidosAprovados}</p>
            <p className="text-xs text-blue-600">Aprovados</p>
          </div>
          <div className="bg-green-50 border border-green-200 rounded-xl p-3 text-center">
            <TrendingUp className="w-5 h-5 text-green-600 mx-auto mb-1" />
            <p className="text-lg font-bold text-green-900">{dash.totalPedidosFaturados}</p>
            <p className="text-xs text-green-600">Total mês</p>
          </div>
        </div>

        {/* Visitas de hoje */}
        {dash.proximasVisitas && dash.proximasVisitas.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
            <div className="flex items-center gap-2 mb-3">
              <MapPin className="w-5 h-5 text-orange-500" />
              <p className="font-semibold text-gray-800">Próximas Visitas ({dash.proximasVisitas.length})</p>
            </div>
            <div className="space-y-2">
              {dash.proximasVisitas.map(v => {
                const st = statusVisita(v.statusCodigo)
                return (
                  <div key={v.id} className="flex items-center justify-between py-2 border-b border-gray-50 last:border-0">
                    <div className="flex items-center gap-2">
                      <Clock className="w-3.5 h-3.5 text-gray-400 flex-shrink-0" />
                      <div>
                        <p className="text-sm font-medium text-gray-800">{v.nomeCliente}</p>
                        <p className="text-xs text-gray-400">{new Date(v.dataAgendada).toLocaleDateString('pt-BR')}</p>
                        {v.obs && (
                          <p className="text-xs text-gray-500 truncate max-w-[200px]">{v.obs}</p>
                        )}
                      </div>
                    </div>
                    <span className={`text-xs font-medium px-2 py-0.5 rounded-full flex-shrink-0 ${st.color}`}>
                      {st.label}
                    </span>
                  </div>
                )
              })}
            </div>
          </div>
        )}

        {/* Últimos pedidos */}
        {dash.ultimosPedidos && dash.ultimosPedidos.length > 0 && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
            <p className="font-semibold text-gray-800 mb-3">Últimos Pedidos</p>
            <div className="space-y-2">
              {dash.ultimosPedidos.map(p => (
                <div key={p.id} className="flex items-center justify-between py-1.5 border-b border-gray-50 last:border-0">
                  <div>
                    <p className="text-sm font-medium text-gray-800">{p.nomeCliente}</p>
                    <p className="text-xs text-gray-400">
                      {new Date(p.dataPedido).toLocaleDateString('pt-BR')} · {p.itens.length} item(s)
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-semibold text-gray-900">{fmtMoeda(p.total)}</p>
                    <p className="text-xs text-gray-400">{p.status}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}

      </div>
    </div>
  )
}

export default DashboardVendedorPage
