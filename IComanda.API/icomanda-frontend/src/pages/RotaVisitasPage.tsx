import React, { useCallback, useEffect, useState } from 'react'
import { ArrowLeft, CalendarCheck, CheckCircle2, MapPin, MessageSquare, RefreshCw, XCircle } from 'lucide-react'
import { forcaVendasService } from '../services/api'
import { Vendedor, VisitaFV } from '../types/api'
import { useToast } from '../hooks/useToast'

interface RotaVisitasPageProps {
  idVendedor?: number
  onClose: () => void
}

const STATUS_MAP: Record<number, { label: string; color: string; bg: string }> = {
  0: { label: 'Agendada',      color: 'text-yellow-700', bg: 'bg-yellow-100' },
  1: { label: 'Em andamento',  color: 'text-blue-700',   bg: 'bg-blue-100' },
  2: { label: 'Concluída',     color: 'text-green-700',  bg: 'bg-green-100' },
  3: { label: 'Não realizada', color: 'text-red-700',    bg: 'bg-red-100' },
}

const RotaVisitasPage: React.FC<RotaVisitasPageProps> = ({ idVendedor, onClose }) => {
  const [visitas, setVisitas]           = useState<VisitaFV[]>([])
  const [vendedores, setVendedores]     = useState<Vendedor[]>([])
  const [filtroVendedor, setFiltroVendedor] = useState<number | ''>(idVendedor ?? '')
  const [abaAtiva, setAbaAtiva]         = useState<'hoje' | 'todas'>('hoje')
  const [isLoading, setIsLoading]       = useState(true)
  const [fazendoCheckin, setFazendoCheckin] = useState<number | null>(null)
  const { showSuccess, showError }      = useToast()

  const carregar = useCallback(async () => {
    setIsLoading(true)
    try {
      const [v, vend] = await Promise.all([
        abaAtiva === 'hoje' && filtroVendedor !== ''
          ? forcaVendasService.getAgendaHoje(filtroVendedor)
          : abaAtiva === 'todas' && filtroVendedor !== ''
            ? forcaVendasService.getVisitas(filtroVendedor, {
                dataInicio: new Date(new Date().setDate(1)).toISOString().split('T')[0],
              })
            : Promise.resolve([]),
        forcaVendasService.getVendedoresAtivos(),
      ])
      setVisitas(v)
      setVendedores(vend)
    } catch {
      showError('Erro ao carregar agenda')
    } finally {
      setIsLoading(false)
    }
  }, [abaAtiva, filtroVendedor, showError])

  useEffect(() => { carregar() }, [carregar])

  const handleCheckin = async (id: number) => {
    setFazendoCheckin(id)
    try {
      await forcaVendasService.checkin(id, {})
      showSuccess('Check-in realizado!')
      carregar()
    } catch {
      showError('Erro ao fazer check-in')
    } finally {
      setFazendoCheckin(null)
    }
  }

  const handleConcluir = async (id: number) => {
    const resumo = window.prompt('Resumo da visita (opcional):') ?? ''
    setFazendoCheckin(id)
    try {
      await forcaVendasService.concluirVisita(id, { resumo })
      showSuccess('Visita concluída!')
      carregar()
    } catch {
      showError('Erro ao concluir visita')
    } finally {
      setFazendoCheckin(null)
    }
  }

  const handleNaoRealizada = async (id: number) => {
    const motivo = window.prompt('Motivo de não realização:')
    if (!motivo) return
    setFazendoCheckin(id)
    try {
      await forcaVendasService.marcarNaoRealizada(id, motivo)
      showSuccess('Visita marcada como não realizada.')
      carregar()
    } catch {
      showError('Erro ao atualizar visita')
    } finally {
      setFazendoCheckin(null)
    }
  }

  const fmtData = (d: string) => new Date(d).toLocaleDateString('pt-BR')
  const fmtHora = (d: string) => new Date(d).toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white border-b px-4 py-3 flex items-center gap-3 shadow-sm">
        <button onClick={onClose} className="p-1.5 hover:bg-gray-100 rounded-lg">
          <ArrowLeft className="w-5 h-5 text-gray-600" />
        </button>
        <div className="flex-1">
          <h1 className="font-bold text-gray-900 text-lg">Rota de Visitas</h1>
          <p className="text-xs text-gray-500">{visitas.length} visita(s)</p>
        </div>
        <button onClick={carregar} className="p-1.5 hover:bg-gray-100 rounded-lg">
          <RefreshCw className="w-4 h-4 text-gray-500" />
        </button>
      </div>

      {/* Abas */}
      <div className="flex bg-white border-b px-4">
        {(['hoje', 'todas'] as const).map(aba => (
          <button
            key={aba}
            onClick={() => setAbaAtiva(aba)}
            className={`py-3 px-4 text-sm font-medium border-b-2 transition-colors ${
              abaAtiva === aba
                ? 'border-indigo-500 text-indigo-600'
                : 'border-transparent text-gray-500 hover:text-gray-700'
            }`}
          >
            {aba === 'hoje' ? 'Agenda de Hoje' : 'Todas do Mês'}
          </button>
        ))}
      </div>

      {/* Filtro vendedor */}
      {vendedores.length > 0 && (
        <div className="px-4 pt-3 pb-1">
          <select
            value={filtroVendedor}
            onChange={e => setFiltroVendedor(e.target.value === '' ? '' : Number(e.target.value))}
            className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2 bg-white"
          >
            <option value="">Todos os vendedores</option>
            {vendedores.map(v => (
              <option key={v.id} value={v.id}>{v.nome}</option>
            ))}
          </select>
        </div>
      )}

      {/* Lista */}
      <div className="flex-1 overflow-y-auto p-4 space-y-3">
        {isLoading ? (
          <div className="flex justify-center py-12">
            <RefreshCw className="w-8 h-8 text-gray-400 animate-spin" />
          </div>
        ) : visitas.length === 0 ? (
          <div className="text-center py-12 text-gray-400">
            <CalendarCheck className="w-10 h-10 mx-auto mb-2 opacity-40" />
            <p className="text-sm">
              {abaAtiva === 'hoje' ? 'Nenhuma visita agendada para hoje' : 'Nenhuma visita no mês'}
            </p>
          </div>
        ) : (
          visitas.map(visita => {
            const st = STATUS_MAP[visita.statusCodigo] ?? STATUS_MAP[0]
            const isAgendada = visita.statusCodigo === 0
            const isEmAndamento = visita.statusCodigo === 1
            const isPending = fazendoCheckin === visita.id

            return (
              <div key={visita.id} className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
                {/* Cabeçalho */}
                <div className="px-4 pt-3 pb-2 flex items-start justify-between">
                  <div className="flex items-start gap-2">
                    <MapPin className="w-4 h-4 text-gray-400 mt-0.5 flex-shrink-0" />
                    <div>
                      <p className="font-semibold text-gray-900 text-sm">{visita.nomeCliente}</p>
                      <p className="text-xs text-gray-500">{visita.nomeVendedor}</p>
                    </div>
                  </div>
                  <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${st.color} ${st.bg}`}>
                    {st.label}
                  </span>
                </div>

                {/* Detalhes */}
                <div className="px-4 py-1 border-t border-gray-50 space-y-1">
                  {visita.dataAgendada && (
                    <p className="text-xs text-gray-500">
                      🕐 {fmtData(visita.dataAgendada)}
                    </p>
                  )}
                  {visita.obs && (
                    <p className="text-xs text-gray-600 flex items-start gap-1">
                      <MessageSquare className="w-3 h-3 text-gray-400 mt-0.5 flex-shrink-0" />
                      {visita.obs}
                    </p>
                  )}
                  {visita.resultado && (
                    <p className="text-xs text-green-700 bg-green-50 rounded px-2 py-1">{visita.resultado}</p>
                  )}
                </div>

                {/* Ações */}
                {(isAgendada || isEmAndamento) && (
                  <div className="px-4 pb-3 pt-2 flex gap-2 border-t border-gray-50">
                    {isAgendada && (
                      <>
                        <button
                          onClick={() => handleNaoRealizada(visita.id)}
                          disabled={isPending}
                          className="flex-1 text-xs text-red-600 border border-red-200 py-1.5 rounded-lg hover:bg-red-50 flex items-center justify-center gap-1"
                        >
                          <XCircle className="w-3.5 h-3.5" /> Não realizada
                        </button>
                        <button
                          onClick={() => handleCheckin(visita.id)}
                          disabled={isPending}
                          className="flex-1 text-xs text-white bg-blue-600 py-1.5 rounded-lg hover:bg-blue-700 flex items-center justify-center gap-1"
                        >
                          {isPending ? (
                            <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                          ) : (
                            <MapPin className="w-3.5 h-3.5" />
                          )}
                          Check-in
                        </button>
                      </>
                    )}
                    {isEmAndamento && (
                      <button
                        onClick={() => handleConcluir(visita.id)}
                        disabled={isPending}
                        className="flex-1 text-xs text-white bg-green-600 py-1.5 rounded-lg hover:bg-green-700 flex items-center justify-center gap-1"
                      >
                        {isPending ? (
                          <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                        ) : (
                          <CheckCircle2 className="w-3.5 h-3.5" />
                        )}
                        Concluir visita
                      </button>
                    )}
                  </div>
                )}
              </div>
            )
          })
        )}
      </div>
    </div>
  )
}

export default RotaVisitasPage
