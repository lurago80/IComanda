import React, { useCallback, useEffect, useState } from 'react'
import {
  ArrowLeft, RefreshCw, Trophy, TrendingUp, TrendingDown, Minus
} from 'lucide-react'
import { forcaVendasService } from '../services/api'
import { MetaFV } from '../types/api'

interface RankingVendedoresPageProps {
  onClose: () => void
}

const MESES = ['Janeiro','Fevereiro','Março','Abril','Maio','Junho',
               'Julho','Agosto','Setembro','Outubro','Novembro','Dezembro']

const fmtMoeda = (v: number) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)

const medalha = (pos: number) => {
  if (pos === 1) return { emoji: '🥇', bg: 'bg-yellow-50 border-yellow-300' }
  if (pos === 2) return { emoji: '🥈', bg: 'bg-gray-50 border-gray-300' }
  if (pos === 3) return { emoji: '🥉', bg: 'bg-orange-50 border-orange-300' }
  return { emoji: `${pos}º`, bg: 'bg-white border-gray-200' }
}

const RankingVendedoresPage: React.FC<RankingVendedoresPageProps> = ({ onClose }) => {
  const agora = new Date()
  const [mes, setMes]       = useState(agora.getMonth() + 1)
  const [ano, setAno]       = useState(agora.getFullYear())
  const [ranking, setRanking] = useState<MetaFV[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [erro, setErro]     = useState('')

  const anos = Array.from({ length: 3 }, (_, i) => agora.getFullYear() - i)

  const carregar = useCallback(async () => {
    setIsLoading(true)
    setErro('')
    try {
      const data = await forcaVendasService.getRanking(mes, ano)
      // ordenar por percentual atingido desc
      const sorted = [...data].sort(
        (a: MetaFV, b: MetaFV) => b.percentualAtingido - a.percentualAtingido
      )
      setRanking(sorted)
    } catch {
      setErro('Erro ao carregar ranking')
    } finally {
      setIsLoading(false)
    }
  }, [mes, ano])

  useEffect(() => { carregar() }, [carregar])

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white border-b px-4 py-3 flex items-center gap-3 shadow-sm">
        <button onClick={onClose} className="p-1.5 hover:bg-gray-100 rounded-lg">
          <ArrowLeft className="w-5 h-5 text-gray-600" />
        </button>
        <Trophy className="w-5 h-5 text-yellow-500" />
        <div className="flex-1">
          <h1 className="font-bold text-gray-900 text-lg">Ranking de Vendedores</h1>
          <p className="text-xs text-gray-500">{ranking.length} vendedor(es) com meta</p>
        </div>
        <button onClick={carregar} className="p-1.5 hover:bg-gray-100 rounded-lg" title="Recarregar">
          <RefreshCw className="w-4 h-4 text-gray-500" />
        </button>
      </div>

      {/* Filtro de período */}
      <div className="px-4 pt-4 pb-2 flex gap-2">
        <select
          value={mes}
          onChange={e => setMes(Number(e.target.value))}
          className="flex-1 text-sm border border-gray-300 rounded-lg px-3 py-2 bg-white"
        >
          {MESES.map((m, i) => (
            <option key={i + 1} value={i + 1}>{m}</option>
          ))}
        </select>
        <select
          value={ano}
          onChange={e => setAno(Number(e.target.value))}
          className="w-28 text-sm border border-gray-300 rounded-lg px-3 py-2 bg-white"
        >
          {anos.map(a => <option key={a} value={a}>{a}</option>)}
        </select>
      </div>

      {/* Conteúdo */}
      <div className="flex-1 px-4 pb-6 space-y-3 overflow-y-auto">
        {isLoading ? (
          <div className="flex justify-center py-12">
            <RefreshCw className="w-8 h-8 text-gray-400 animate-spin" />
          </div>
        ) : erro ? (
          <div className="text-center py-12 text-red-400">
            <p className="text-sm">{erro}</p>
            <button onClick={carregar} className="mt-2 text-sm text-indigo-600 underline">
              Tentar novamente
            </button>
          </div>
        ) : ranking.length === 0 ? (
          <div className="text-center py-12 text-gray-400">
            <Trophy className="w-10 h-10 mx-auto mb-2 opacity-30" />
            <p className="text-sm">Nenhuma meta definida para {MESES[mes - 1]}/{ano}</p>
            <p className="text-xs mt-1">Defina metas nos perfis dos vendedores</p>
          </div>
        ) : (
          ranking.map((item, idx) => {
            const pos    = idx + 1
            const medal  = medalha(pos)
            const pct    = item.percentualAtingido
            const barW   = Math.min(pct, 100)
            const barColor = pct >= 100 ? 'bg-green-500'
                           : pct >= 75  ? 'bg-blue-500'
                           : pct >= 50  ? 'bg-yellow-500'
                           : 'bg-red-400'

            return (
              <div
                key={item.id}
                className={`rounded-xl border shadow-sm overflow-hidden ${medal.bg}`}
              >
                <div className="px-4 pt-3 pb-2 flex items-center gap-3">
                  {/* Posição */}
                  <div className="text-xl w-8 text-center shrink-0 font-bold">
                    {medal.emoji}
                  </div>

                  {/* Dados do vendedor */}
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center justify-between gap-2">
                      <p className="font-semibold text-gray-900 truncate">{item.nomeVendedor}</p>
                      <div className="flex items-center gap-1 shrink-0">
                        {item.metaAtingida
                          ? <TrendingUp className="w-4 h-4 text-green-600" />
                          : pct >= 75
                            ? <Minus className="w-4 h-4 text-yellow-600" />
                            : <TrendingDown className="w-4 h-4 text-red-500" />
                        }
                        <span className={`text-sm font-bold ${
                          item.metaAtingida ? 'text-green-600'
                          : pct >= 75 ? 'text-yellow-600'
                          : 'text-red-500'
                        }`}>
                          {pct.toFixed(1)}%
                        </span>
                      </div>
                    </div>

                    {/* Barra de progresso */}
                    <div className="mt-1.5 mb-1 h-2 bg-gray-200 rounded-full overflow-hidden">
                      <div
                        className={`h-full rounded-full transition-all ${barColor}`}
                        style={{ width: `${barW}%` }}
                      />
                    </div>

                    <div className="flex justify-between text-xs text-gray-500">
                      <span>Realizado: <span className="font-medium text-gray-700">{fmtMoeda(item.valorRealizado)}</span></span>
                      <span>Meta: <span className="font-medium text-gray-700">{fmtMoeda(item.valorMeta)}</span></span>
                    </div>
                  </div>
                </div>

                {item.metaAtingida && (
                  <div className="px-4 pb-2">
                    <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded-full font-medium">
                      ✅ Meta atingida!
                    </span>
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

export default RankingVendedoresPage
