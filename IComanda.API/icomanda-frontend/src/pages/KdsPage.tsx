import { useCallback, useEffect, useRef, useState } from 'react'
import { ArrowLeft, ChefHat, Clock, RefreshCw, Utensils } from 'lucide-react'
import { kdsService } from '../services/api'
import { KdsPedido } from '../types/api'

type StatusCozinha = 'PENDENTE' | 'EM_PREPARO' | 'PRONTO' | 'ENTREGUE'

const STATUS_CONFIG: Record<StatusCozinha, { label: string; cor: string; corFundo: string; corBorda: string; proximoStatus: StatusCozinha | null; labelBotao: string }> = {
  PENDENTE:   { label: 'Pendente',   cor: 'text-yellow-700',  corFundo: 'bg-yellow-50',  corBorda: 'border-yellow-400', proximoStatus: 'EM_PREPARO', labelBotao: 'Iniciar Preparo' },
  EM_PREPARO: { label: 'Preparando', cor: 'text-blue-700',    corFundo: 'bg-blue-50',    corBorda: 'border-blue-400',   proximoStatus: 'PRONTO',     labelBotao: 'Marcar Pronto'   },
  PRONTO:     { label: 'Pronto',     cor: 'text-green-700',   corFundo: 'bg-green-50',   corBorda: 'border-green-400',  proximoStatus: 'ENTREGUE',   labelBotao: 'Confirmar Entrega' },
  ENTREGUE:   { label: 'Entregue',   cor: 'text-gray-500',    corFundo: 'bg-gray-50',    corBorda: 'border-gray-300',   proximoStatus: null,         labelBotao: '' },
}

const COR_TEMPO: (minutos: number) => string = (m) => {
  if (m <= 10) return 'text-green-600'
  if (m <= 20) return 'text-yellow-600'
  return 'text-red-600'
}

interface CardPedidoProps {
  pedido: KdsPedido
  atualizando: boolean
  onAvancar: (nota: string, novoStatus: StatusCozinha) => void
}

function CardPedido({ pedido, atualizando, onAvancar }: CardPedidoProps) {
  const cfg = STATUS_CONFIG[pedido.statusCozinha as StatusCozinha] ?? STATUS_CONFIG.PENDENTE
  const identificador = pedido.comanda
    ? `Comanda #${pedido.comanda}`
    : pedido.mesa
    ? `Mesa ${pedido.mesa}`
    : `Nota ${pedido.nota}`
  const origem = pedido.origem === 'DL' ? 'Delivery' : 'Salão'

  return (
    <div className={`rounded-xl border-2 ${cfg.corBorda} ${cfg.corFundo} flex flex-col shadow-md overflow-hidden`}>
      {/* Header do card */}
      <div className="flex items-center justify-between px-4 py-3 bg-white border-b border-gray-100">
        <div className="flex flex-col">
          <span className="font-bold text-gray-900 text-lg leading-tight">{identificador}</span>
          {pedido.nomeCliente && (
            <span className="text-sm text-gray-500 truncate max-w-[180px]">{pedido.nomeCliente}</span>
          )}
          <span className="text-xs text-gray-400">{origem}</span>
        </div>
        <div className="flex flex-col items-end gap-1">
          <span className={`text-sm font-semibold px-2 py-0.5 rounded-full ${cfg.corFundo} ${cfg.cor} border ${cfg.corBorda}`}>
            {cfg.label}
          </span>
          <span className={`flex items-center gap-1 text-sm font-bold ${COR_TEMPO(pedido.minutosEspera)}`}>
            <Clock className="w-3.5 h-3.5" />
            {pedido.minutosEspera < 1 ? 'agora' : `${pedido.minutosEspera} min`}
          </span>
        </div>
      </div>

      {/* Itens */}
      <ul className="flex-1 px-4 py-3 space-y-2">
        {pedido.itens.map((item) => (
          <li key={item.item} className="flex items-start gap-2">
            <span className="font-bold text-gray-800 min-w-[2rem] text-right">{item.qtd}x</span>
            <div className="flex flex-col">
              <span className="text-gray-800 font-medium leading-snug">{item.descricao}</span>
              {item.observacao && (
                <span className="text-xs text-orange-600 italic">{item.observacao}</span>
              )}
            </div>
          </li>
        ))}
      </ul>

      {/* Botão de ação */}
      {cfg.proximoStatus && (
        <div className="px-4 pb-4 pt-2">
          <button
            disabled={atualizando}
            onClick={() => onAvancar(pedido.nota, cfg.proximoStatus!)}
            className={`w-full py-2.5 rounded-lg font-semibold text-white text-sm transition-opacity
              ${pedido.statusCozinha === 'PENDENTE'   ? 'bg-yellow-500 hover:bg-yellow-600' : ''}
              ${pedido.statusCozinha === 'EM_PREPARO' ? 'bg-blue-600 hover:bg-blue-700'     : ''}
              ${pedido.statusCozinha === 'PRONTO'     ? 'bg-green-600 hover:bg-green-700'   : ''}
              ${atualizando ? 'opacity-50 cursor-not-allowed' : 'active:scale-95'}
            `}
          >
            {cfg.labelBotao}
          </button>
        </div>
      )}
    </div>
  )
}

interface KdsPageProps {
  onClose: () => void
}

export default function KdsPage({ onClose }: KdsPageProps) {
  const [pedidos, setPedidos] = useState<KdsPedido[]>([])
  const [carregando, setCarregando] = useState(true)
  const [atualizandoNota, setAtualizandoNota] = useState<string | null>(null)
  const [ultimaAtualizacao, setUltimaAtualizacao] = useState<Date>(new Date())
  const [filtroStatus, setFiltroStatus] = useState<StatusCozinha | 'TODOS'>('TODOS')
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const carregar = useCallback(async (silencioso = false) => {
    if (!silencioso) setCarregando(true)
    try {
      const data = await kdsService.getPedidos()
      setPedidos(data)
      setUltimaAtualizacao(new Date())
    } catch {
      // erro silencioso no polling
    } finally {
      if (!silencioso) setCarregando(false)
    }
  }, [])

  useEffect(() => {
    carregar()
    intervalRef.current = setInterval(() => carregar(true), 10000)
    return () => { if (intervalRef.current) clearInterval(intervalRef.current) }
  }, [carregar])

  const handleAvancar = async (nota: string, novoStatus: StatusCozinha) => {
    setAtualizandoNota(nota)
    try {
      await kdsService.atualizarStatus(nota, novoStatus)
      if (novoStatus === 'ENTREGUE') {
        setPedidos(prev => prev.filter(p => p.nota !== nota))
      } else {
        setPedidos(prev => prev.map(p => p.nota === nota ? { ...p, statusCozinha: novoStatus } : p))
      }
    } catch {
      // silencioso — polling vai corrigir
    } finally {
      setAtualizandoNota(null)
    }
  }

  const pedidosFiltrados = filtroStatus === 'TODOS'
    ? pedidos
    : pedidos.filter(p => p.statusCozinha === filtroStatus)

  const contadores = {
    PENDENTE:   pedidos.filter(p => p.statusCozinha === 'PENDENTE').length,
    EM_PREPARO: pedidos.filter(p => p.statusCozinha === 'EM_PREPARO').length,
    PRONTO:     pedidos.filter(p => p.statusCozinha === 'PRONTO').length,
  }

  return (
    <div className="min-h-screen bg-gray-950 text-white flex flex-col">
      {/* Cabeçalho */}
      <header className="bg-gray-900 border-b border-gray-800 px-4 py-3 flex items-center gap-3 sticky top-0 z-10">
        <button
          onClick={onClose}
          className="p-2 rounded-lg hover:bg-gray-800 transition-colors text-gray-400 hover:text-white"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>

        <div className="flex items-center gap-2">
          <ChefHat className="w-6 h-6 text-orange-400" />
          <h1 className="text-lg font-bold text-white">Cozinha — KDS</h1>
        </div>

        <div className="ml-auto flex items-center gap-3">
          <span className="text-xs text-gray-500 hidden sm:block">
            Atualizado: {ultimaAtualizacao.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}
          </span>
          <button
            onClick={() => carregar()}
            disabled={carregando}
            className="p-2 rounded-lg hover:bg-gray-800 text-gray-400 hover:text-white transition-colors"
          >
            <RefreshCw className={`w-4 h-4 ${carregando ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </header>

      {/* Filtros por status */}
      <div className="bg-gray-900 border-b border-gray-800 px-4 py-2 flex gap-2 overflow-x-auto scrollbar-none">
        {(['TODOS', 'PENDENTE', 'EM_PREPARO', 'PRONTO'] as const).map((s) => {
          const count = s === 'TODOS' ? pedidos.length : contadores[s as keyof typeof contadores]
          const ativo = filtroStatus === s
          const labels: Record<string, string> = { TODOS: 'Todos', PENDENTE: 'Pendentes', EM_PREPARO: 'Preparando', PRONTO: 'Prontos' }
          const cores: Record<string, string> = {
            TODOS:      ativo ? 'bg-white text-gray-900' : 'bg-gray-800 text-gray-300',
            PENDENTE:   ativo ? 'bg-yellow-500 text-white' : 'bg-gray-800 text-yellow-400',
            EM_PREPARO: ativo ? 'bg-blue-600 text-white' : 'bg-gray-800 text-blue-400',
            PRONTO:     ativo ? 'bg-green-600 text-white' : 'bg-gray-800 text-green-400',
          }
          return (
            <button
              key={s}
              onClick={() => setFiltroStatus(s)}
              className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium whitespace-nowrap transition-colors ${cores[s]}`}
            >
              {labels[s]}
              <span className={`text-xs font-bold px-1.5 py-0.5 rounded-full ${ativo ? 'bg-black/20' : 'bg-gray-700'}`}>
                {count}
              </span>
            </button>
          )
        })}
      </div>

      {/* Conteúdo */}
      <main className="flex-1 p-4">
        {carregando && pedidos.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-64 gap-3 text-gray-500">
            <RefreshCw className="w-8 h-8 animate-spin" />
            <span>Carregando pedidos...</span>
          </div>
        ) : pedidosFiltrados.length === 0 ? (
          <div className="flex flex-col items-center justify-center h-64 gap-3 text-gray-600">
            <Utensils className="w-12 h-12 opacity-30" />
            <p className="text-lg font-medium">
              {filtroStatus === 'TODOS' ? 'Nenhum pedido na fila' : `Nenhum pedido ${STATUS_CONFIG[filtroStatus as StatusCozinha]?.label.toLowerCase()}`}
            </p>
            <p className="text-sm opacity-60">Atualizando automaticamente a cada 10 segundos</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {pedidosFiltrados.map((pedido) => (
              <CardPedido
                key={pedido.nota}
                pedido={pedido}
                atualizando={atualizandoNota === pedido.nota}
                onAvancar={handleAvancar}
              />
            ))}
          </div>
        )}
      </main>
    </div>
  )
}
