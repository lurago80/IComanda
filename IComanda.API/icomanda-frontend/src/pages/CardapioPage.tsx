import { useCallback, useEffect, useRef, useState } from 'react'
import { ChevronDown, ChevronUp, Search, UtensilsCrossed, ShoppingCart, Plus, Minus, Trash2, X, CheckCircle, Loader2 } from 'lucide-react'
import { getApiBaseUrl } from '../services/api'

const apiUrl = getApiBaseUrl()
const fetchPublic = async (path: string) => {
  const res = await fetch(`${apiUrl}${path}`)
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  return res.json()
}
const postPublic = async (path: string, body: unknown) => {
  const res = await fetch(`${apiUrl}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  })
  const data = await res.json()
  if (!res.ok) throw new Error(data.mensagem ?? `HTTP ${res.status}`)
  return data
}

const R$ = (v: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)

// ── Tipos ─────────────────────────────────────────────────────────────────────

interface Grupo { id: number; descricao: string; tipo: string }
interface Produto { id: number; descricao: string; caracteristica?: string; preco: number }
interface Estabelecimento { nome: string; telefone?: string; cidade?: string; uf?: string }

interface Sabor { id: number; descricao: string; ingredientes?: string; preco: number }
interface Tamanho { id: number; descricao: string; ordem: number; sabores: Sabor[] }
interface Borda { id: number; descricao: string; preco: number }
interface PizzaData { tamanhos: Tamanho[]; bordas: Borda[] }

interface GrupoComProdutos extends Grupo {
  produtos: Produto[]
  pizza?: PizzaData
  aberto: boolean
  carregando: boolean
}

interface ItemCarrinho {
  id: string // uuid local
  produtoId: number
  descricao: string
  preco: number
  quantidade: number
  observacao?: string
}

// ── Carrinho lateral ──────────────────────────────────────────────────────────

interface CarrinhoModalProps {
  itens: ItemCarrinho[]
  mesa?: number
  onClose: () => void
  onAlterar: (id: string, delta: number) => void
  onRemover: (id: string) => void
  onPedidoEnviado: () => void
}

function CarrinhoModal({ itens, mesa, onClose, onAlterar, onRemover, onPedidoEnviado }: CarrinhoModalProps) {
  const [nome, setNome] = useState('')
  const [obs, setObs] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [sucesso, setSucesso] = useState(false)
  const [erro, setErro] = useState('')

  const total = itens.reduce((s, i) => s + i.preco * i.quantidade, 0)

  const enviar = async () => {
    if (itens.length === 0) return
    setEnviando(true)
    setErro('')
    try {
      await postPublic('/cardapio/pedido', {
        mesa: mesa ?? 0,
        nomeCliente: nome.trim() || undefined,
        observacao: obs.trim() || undefined,
        itens: itens.map(i => ({
          produtoId: i.produtoId,
          descricao: i.descricao,
          preco: i.preco,
          quantidade: i.quantidade,
          observacao: i.observacao,
        })),
      })
      setSucesso(true)
      setTimeout(() => {
        onPedidoEnviado()
      }, 2500)
    } catch (e: any) {
      setErro(e.message ?? 'Erro ao enviar pedido. Tente novamente.')
    } finally {
      setEnviando(false)
    }
  }

  if (sucesso) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 px-4">
        <div className="bg-white rounded-3xl p-8 max-w-sm w-full text-center shadow-2xl">
          <CheckCircle className="w-16 h-16 text-green-500 mx-auto mb-4" />
          <h2 className="text-xl font-bold text-gray-800 mb-2">Pedido enviado!</h2>
          <p className="text-gray-500 text-sm">Em breve será preparado. Aguarde o garçom.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="fixed inset-0 z-50 flex flex-col bg-white">
      {/* Header */}
      <div className="bg-amber-700 text-white px-4 py-4 flex items-center gap-3">
        <button onClick={onClose} className="p-1.5 rounded-full hover:bg-amber-600 transition-colors">
          <X className="w-5 h-5" />
        </button>
        <div className="flex-1">
          <h2 className="font-bold text-lg leading-tight">Meu Pedido</h2>
          {mesa && <p className="text-amber-200 text-xs">Mesa {mesa}</p>}
        </div>
        <span className="font-bold text-lg">{R$(total)}</span>
      </div>

      {/* Lista de itens */}
      <div className="flex-1 overflow-y-auto px-4 py-3 space-y-3">
        {itens.map(item => (
          <div key={item.id} className="bg-amber-50 rounded-2xl p-3 flex items-start gap-3">
            <div className="flex-1 min-w-0">
              <p className="font-medium text-gray-800 text-sm leading-snug">{item.descricao}</p>
              {item.observacao && <p className="text-xs text-gray-500 mt-0.5">{item.observacao}</p>}
              <p className="text-amber-700 font-bold text-sm mt-1">{R$(item.preco)} un.</p>
            </div>
            <div className="flex items-center gap-1 shrink-0">
              <button
                onClick={() => onAlterar(item.id, -1)}
                className="w-7 h-7 rounded-full bg-amber-200 text-amber-800 flex items-center justify-center hover:bg-amber-300 transition-colors"
              >
                {item.quantidade === 1 ? <Trash2 className="w-3.5 h-3.5" /> : <Minus className="w-3.5 h-3.5" />}
              </button>
              <span className="w-6 text-center font-bold text-sm">{item.quantidade}</span>
              <button
                onClick={() => onAlterar(item.id, 1)}
                className="w-7 h-7 rounded-full bg-amber-500 text-white flex items-center justify-center hover:bg-amber-600 transition-colors"
              >
                <Plus className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        ))}
      </div>

      {/* Rodapé com formulário e botão */}
      <div className="border-t border-amber-100 px-4 py-4 space-y-3 bg-white">
        <input
          type="text"
          placeholder="Seu nome (opcional)"
          value={nome}
          onChange={e => setNome(e.target.value)}
          className="w-full border border-gray-200 rounded-xl px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-amber-400"
        />
        <textarea
          placeholder="Observação geral (opcional)"
          value={obs}
          onChange={e => setObs(e.target.value)}
          rows={2}
          className="w-full border border-gray-200 rounded-xl px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-amber-400"
        />
        {erro && <p className="text-red-500 text-sm text-center">{erro}</p>}
        <div className="flex items-center justify-between pt-1">
          <div>
            <p className="text-xs text-gray-400">{itens.reduce((s, i) => s + i.quantidade, 0)} item(ns)</p>
            <p className="text-xl font-bold text-amber-700">{R$(total)}</p>
          </div>
          <button
            onClick={enviar}
            disabled={enviando || itens.length === 0}
            className="flex items-center gap-2 bg-amber-600 hover:bg-amber-700 disabled:opacity-50 text-white font-bold px-6 py-3 rounded-2xl transition-colors shadow-lg"
          >
            {enviando ? <Loader2 className="w-5 h-5 animate-spin" /> : <CheckCircle className="w-5 h-5" />}
            {enviando ? 'Enviando...' : 'Enviar Pedido'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ── Thumbnail do produto ──────────────────────────────────────────────────────

function ProdutoThumb({ id }: { id: number }) {
  const [visivel, setVisivel] = useState(true)
  if (!visivel) return null
  return (
    <img
      src={`${apiUrl}/produtos/${id}/imagem`}
      alt=""
      onError={() => setVisivel(false)}
      className="w-14 h-14 rounded-xl object-cover shrink-0 bg-amber-50"
    />
  )
}

// ── Pizza Builder ─────────────────────────────────────────────────────────────

interface PizzaBuilderProps {
  pizza: PizzaData
  nomeGrupo: string
  onAdicionarAoCarrinho: (item: Omit<ItemCarrinho, 'id' | 'quantidade'>) => void
}

function PizzaBuilder({ pizza, nomeGrupo, onAdicionarAoCarrinho }: PizzaBuilderProps) {
  const [modo, setModo] = useState<'inteira' | 'meia'>('inteira')
  const [tamanhoId, setTamanhoId] = useState<number>(pizza.tamanhos[0]?.id ?? 0)
  const [sabor1Id, setSabor1Id] = useState<number>(0)
  const [sabor2Id, setSabor2Id] = useState<number>(0)
  const [bordaId, setBordaId] = useState<number>(pizza.bordas[0]?.id ?? 0)
  const [adicionado, setAdicionado] = useState(false)

  const tamanho = pizza.tamanhos.find(t => t.id === tamanhoId)
  const sabores = tamanho?.sabores ?? []
  const sabor1 = sabores.find(s => s.id === sabor1Id)
  const sabor2 = sabores.find(s => s.id === sabor2Id)
  const borda = pizza.bordas.find(b => b.id === bordaId)

  let precoPizza = 0
  if (modo === 'inteira' && sabor1) precoPizza = sabor1.preco
  if (modo === 'meia' && sabor1 && sabor2) precoPizza = (sabor1.preco + sabor2.preco) / 2
  const precoTotal = precoPizza + (borda?.preco ?? 0)

  const pronto = modo === 'inteira' ? sabor1Id > 0 : (sabor1Id > 0 && sabor2Id > 0)

  const handleTamanho = (id: number) => { setTamanhoId(id); setSabor1Id(0); setSabor2Id(0) }
  const handleModo = (m: 'inteira' | 'meia') => { setModo(m); setSabor2Id(0) }

  const montarDescricao = () => {
    const tam = tamanho ? `${tamanho.descricao} ` : ''
    const base = modo === 'inteira'
      ? sabor1?.descricao ?? ''
      : sabor2 ? `${sabor1?.descricao} / ${sabor2.descricao}` : `${sabor1?.descricao ?? ''} / ?`
    const bordaDesc = borda && borda.preco > 0 ? ` • Borda ${borda.descricao}` : ''
    return `${nomeGrupo} ${tam}${base}${bordaDesc}`.trim()
  }

  const adicionar = () => {
    if (!pronto) return
    onAdicionarAoCarrinho({
      produtoId: 0,
      descricao: montarDescricao(),
      preco: precoTotal,
    })
    setAdicionado(true)
    setSabor1Id(0); setSabor2Id(0)
    setTimeout(() => setAdicionado(false), 1500)
  }

  return (
    <div className="px-4 pb-4 space-y-4">

      {/* Tamanho */}
      {pizza.tamanhos.length > 1 && (
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Tamanho</p>
          <div className="flex flex-wrap gap-2">
            {pizza.tamanhos.map(t => (
              <button key={t.id} onClick={() => handleTamanho(t.id)}
                className={`px-4 py-2 rounded-full text-sm font-medium border-2 transition-colors ${tamanhoId === t.id ? 'border-amber-500 bg-amber-500 text-white' : 'border-gray-200 bg-white text-gray-700 hover:border-amber-300'}`}>
                {t.descricao}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Modo */}
      <div>
        <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Quantos sabores?</p>
        <div className="flex gap-2">
          {(['inteira', 'meia'] as const).map(m => (
            <button key={m} onClick={() => handleModo(m)}
              className={`flex-1 py-2.5 rounded-xl text-sm font-semibold border-2 transition-colors ${modo === m ? 'border-amber-500 bg-amber-500 text-white' : 'border-gray-200 bg-white text-gray-700 hover:border-amber-300'}`}>
              {m === 'inteira' ? '🍕 Inteira' : '🍕 Meio a Meio'}
            </button>
          ))}
        </div>
      </div>

      {/* Sabor 1 */}
      {sabores.length > 0 && (
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">
            {modo === 'meia' ? '1º Sabor' : 'Sabor'}
          </p>
          <div className="space-y-1.5 max-h-52 overflow-y-auto pr-1">
            {sabores.map(s => (
              <label key={s.id} className={`flex items-start gap-3 p-2.5 rounded-xl border-2 cursor-pointer transition-colors ${sabor1Id === s.id ? 'border-amber-400 bg-amber-50' : 'border-gray-100 bg-white hover:border-amber-200'}`}>
                <input type="radio" name="sabor1" checked={sabor1Id === s.id} onChange={() => setSabor1Id(s.id)} className="mt-0.5 accent-amber-500" />
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-gray-800 text-sm">{s.descricao}</p>
                  {s.ingredientes && <p className="text-xs text-gray-500 mt-0.5 leading-snug">{s.ingredientes}</p>}
                </div>
                <span className="text-sm font-bold text-amber-700 shrink-0">{R$(s.preco)}</span>
              </label>
            ))}
          </div>
        </div>
      )}

      {/* Sabor 2 */}
      {modo === 'meia' && sabores.length > 0 && (
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">2º Sabor</p>
          <div className="space-y-1.5 max-h-52 overflow-y-auto pr-1">
            {sabores.map(s => (
              <label key={s.id} className={`flex items-start gap-3 p-2.5 rounded-xl border-2 cursor-pointer transition-colors ${sabor2Id === s.id ? 'border-orange-400 bg-orange-50' : 'border-gray-100 bg-white hover:border-orange-200'}`}>
                <input type="radio" name="sabor2" checked={sabor2Id === s.id} onChange={() => setSabor2Id(s.id)} className="mt-0.5 accent-orange-500" />
                <div className="flex-1 min-w-0">
                  <p className="font-medium text-gray-800 text-sm">{s.descricao}</p>
                  {s.ingredientes && <p className="text-xs text-gray-500 mt-0.5 leading-snug">{s.ingredientes}</p>}
                </div>
                <span className="text-sm font-bold text-amber-700 shrink-0">{R$(s.preco)}</span>
              </label>
            ))}
          </div>
          {sabor1Id > 0 && sabor2Id > 0 && sabor1 && sabor2 && (
            <p className="text-xs text-gray-500 mt-2 text-center">
              Preço médio: ({R$(sabor1.preco)} + {R$(sabor2.preco)}) ÷ 2 = <strong>{R$((sabor1.preco + sabor2.preco) / 2)}</strong>
            </p>
          )}
        </div>
      )}

      {/* Bordas */}
      {pizza.bordas.length > 0 && (
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">Borda</p>
          <div className="space-y-1.5">
            {pizza.bordas.map(b => (
              <label key={b.id} className={`flex items-center gap-3 p-2.5 rounded-xl border-2 cursor-pointer transition-colors ${bordaId === b.id ? 'border-amber-400 bg-amber-50' : 'border-gray-100 bg-white hover:border-amber-200'}`}>
                <input type="radio" name="borda" checked={bordaId === b.id} onChange={() => setBordaId(b.id)} className="accent-amber-500" />
                <span className="flex-1 font-medium text-gray-800 text-sm">{b.descricao}</span>
                <span className="text-sm font-bold text-amber-700">{b.preco === 0 ? 'Grátis' : `+ ${R$(b.preco)}`}</span>
              </label>
            ))}
          </div>
        </div>
      )}

      {/* Botão adicionar */}
      {pronto && (
        <div className="bg-amber-700 text-white rounded-xl p-3">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs text-amber-200">{montarDescricao()}</p>
              <p className="text-xl font-bold">{R$(precoTotal)}</p>
            </div>
            <button
              onClick={adicionar}
              className={`flex items-center gap-2 px-4 py-2.5 rounded-xl font-bold text-sm transition-all ${adicionado ? 'bg-green-500' : 'bg-white text-amber-700 hover:bg-amber-50'}`}
            >
              {adicionado ? <><CheckCircle className="w-4 h-4" /> Adicionado!</> : <><Plus className="w-4 h-4" /> Adicionar</>}
            </button>
          </div>
        </div>
      )}
      {!pronto && (
        <p className="text-center text-xs text-amber-600 py-2">
          {modo === 'meia' ? 'Selecione os 2 sabores para continuar' : 'Selecione um sabor para continuar'}
        </p>
      )}
    </div>
  )
}

// ── Componente principal ──────────────────────────────────────────────────────

interface CardapioPageProps { mesa?: number }

export default function CardapioPage({ mesa }: CardapioPageProps) {
  const [estabelecimento, setEstabelecimento] = useState<Estabelecimento | null>(null)
  const [grupos, setGrupos] = useState<GrupoComProdutos[]>([])
  const [busca, setBusca] = useState('')
  const [carregandoInicial, setCarregandoInicial] = useState(true)
  const [carrinho, setCarrinho] = useState<ItemCarrinho[]>([])
  const [carrinhoAberto, setCarrinhoAberto] = useState(false)
  const nextId = useRef(0)

  useEffect(() => {
    const init = async () => {
      try {
        const [est, grps] = await Promise.all([
          fetchPublic('/cardapio/estabelecimento'),
          fetchPublic('/cardapio/grupos'),
        ])
        setEstabelecimento(est)
        setGrupos((grps as Grupo[]).map(g => ({ ...g, produtos: [], aberto: false, carregando: false })))
      } catch { /* silencioso */ }
      finally { setCarregandoInicial(false) }
    }
    init()
  }, [])

  const carregarConteudo = useCallback(async (grupoId: number, tipo: string) => {
    setGrupos(prev => prev.map(g => g.id === grupoId ? { ...g, carregando: true } : g))
    try {
      if (tipo === 'PIZZA') {
        const data: PizzaData = await fetchPublic(`/cardapio/grupos/${grupoId}/pizza`)
        setGrupos(prev => prev.map(g => g.id === grupoId ? { ...g, pizza: data, carregando: false, aberto: true } : g))
      } else {
        const produtos: Produto[] = await fetchPublic(`/cardapio/grupos/${grupoId}/produtos`)
        setGrupos(prev => prev.map(g => g.id === grupoId ? { ...g, produtos, carregando: false, aberto: true } : g))
      }
    } catch {
      setGrupos(prev => prev.map(g => g.id === grupoId ? { ...g, carregando: false } : g))
    }
  }, [])

  const toggleGrupo = (g: GrupoComProdutos) => {
    if (g.aberto) {
      setGrupos(prev => prev.map(x => x.id === g.id ? { ...x, aberto: false } : x))
    } else if ((g.tipo === 'PIZZA' && g.pizza) || (g.tipo !== 'PIZZA' && g.produtos.length > 0)) {
      setGrupos(prev => prev.map(x => x.id === g.id ? { ...x, aberto: true } : x))
    } else {
      carregarConteudo(g.id, g.tipo)
    }
  }

  const adicionarAoCarrinho = (item: Omit<ItemCarrinho, 'id' | 'quantidade'>) => {
    const id = String(nextId.current++)
    setCarrinho(prev => {
      // Agrupa itens idênticos (mesmo produto e descrição)
      const idx = prev.findIndex(i => i.produtoId === item.produtoId && i.descricao === item.descricao)
      if (idx >= 0) {
        const atualizado = [...prev]
        atualizado[idx] = { ...atualizado[idx], quantidade: atualizado[idx].quantidade + 1 }
        return atualizado
      }
      return [...prev, { ...item, id, quantidade: 1 }]
    })
  }

  const alterarQuantidade = (id: string, delta: number) => {
    setCarrinho(prev => {
      const idx = prev.findIndex(i => i.id === id)
      if (idx < 0) return prev
      const novaQtd = prev[idx].quantidade + delta
      if (novaQtd <= 0) return prev.filter(i => i.id !== id)
      const atualizado = [...prev]
      atualizado[idx] = { ...atualizado[idx], quantidade: novaQtd }
      return atualizado
    })
  }

  const removerItem = (id: string) => setCarrinho(prev => prev.filter(i => i.id !== id))

  const pedidoEnviado = () => {
    setCarrinho([])
    setCarrinhoAberto(false)
  }

  // Busca cross-grupo (apenas grupos normais)
  const buscaNorm = busca.trim().toLowerCase()
  const resultadosBusca: Array<{ grupo: string; produto: Produto }> = []
  if (buscaNorm) {
    for (const g of grupos) {
      for (const p of g.produtos) {
        if ((p.descricao?.toLowerCase().includes(buscaNorm)) || (p.caracteristica?.toLowerCase().includes(buscaNorm)))
          resultadosBusca.push({ grupo: g.descricao, produto: p })
      }
    }
  }

  const totalCarrinho = carrinho.reduce((s, i) => s + i.quantidade, 0)

  if (carregandoInicial) {
    return (
      <div className="min-h-screen bg-amber-50 flex items-center justify-center">
        <div className="text-center space-y-3">
          <UtensilsCrossed className="w-12 h-12 text-amber-600 mx-auto animate-pulse" />
          <p className="text-amber-800 font-medium">Carregando cardápio...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-amber-50 pb-24">
      {/* Header */}
      <header className="bg-amber-700 text-white px-4 py-5 shadow-lg">
        <div className="max-w-lg mx-auto">
          <div className="flex items-center gap-3 mb-1">
            <UtensilsCrossed className="w-7 h-7 text-amber-200 shrink-0" />
            <h1 className="text-xl font-bold leading-tight">{estabelecimento?.nome ?? 'Cardápio Digital'}</h1>
          </div>
          {estabelecimento?.cidade && (
            <p className="text-amber-200 text-sm pl-10">
              {estabelecimento.cidade}{estabelecimento.uf ? ` — ${estabelecimento.uf}` : ''}
            </p>
          )}
          {mesa && (
            <div className="mt-3 inline-flex items-center gap-2 bg-amber-600 rounded-full px-4 py-1.5">
              <span className="text-amber-100 text-sm font-medium">Mesa {mesa}</span>
            </div>
          )}
        </div>
      </header>

      <div className="max-w-lg mx-auto px-4 pt-4 space-y-4">
        {/* Busca */}
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar no cardápio..."
            value={busca}
            onChange={e => setBusca(e.target.value)}
            className="w-full pl-9 pr-4 py-2.5 rounded-xl border border-amber-200 bg-white shadow-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-amber-400 text-sm"
          />
        </div>

        {/* Resultados de busca */}
        {buscaNorm && (
          <div className="space-y-2">
            {resultadosBusca.length === 0
              ? <p className="text-center text-gray-500 py-6 text-sm">Nenhum item encontrado para "{busca}"</p>
              : <>
                  <p className="text-xs text-gray-500 font-medium uppercase tracking-wide px-1">{resultadosBusca.length} resultado{resultadosBusca.length !== 1 ? 's' : ''}</p>
                  {resultadosBusca.map(({ grupo, produto }) => (
                    <div key={produto.id} className="bg-white rounded-xl shadow-sm border border-amber-100 px-4 py-3 flex items-center gap-3">
                      <ProdutoThumb id={produto.id} />
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-amber-600 font-medium uppercase tracking-wide mb-0.5">{grupo}</p>
                        <p className="font-medium text-gray-800 text-sm">{produto.descricao}</p>
                        {produto.caracteristica && <p className="text-xs text-gray-500 mt-0.5">{produto.caracteristica}</p>}
                      </div>
                      <div className="flex items-center gap-2 shrink-0">
                        <span className="font-bold text-amber-700 text-sm">{R$(produto.preco)}</span>
                        <button
                          onClick={() => adicionarAoCarrinho({ produtoId: produto.id, descricao: produto.descricao, preco: produto.preco })}
                          className="w-8 h-8 rounded-full bg-amber-500 text-white flex items-center justify-center hover:bg-amber-600 transition-colors"
                        >
                          <Plus className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  ))}
                </>
            }
          </div>
        )}

        {/* Grupos */}
        {!buscaNorm && (
          <div className="space-y-3">
            {grupos.length === 0 && (
              <div className="text-center py-12 text-gray-400">
                <UtensilsCrossed className="w-10 h-10 mx-auto mb-2 opacity-40" />
                <p>Cardápio não disponível no momento.</p>
              </div>
            )}
            {grupos.map(grupo => (
              <div key={grupo.id} className="bg-white rounded-2xl shadow-sm overflow-hidden border border-amber-100">
                <button onClick={() => toggleGrupo(grupo)} className="w-full flex items-center justify-between px-4 py-4 text-left">
                  <div className="flex items-center gap-2">
                    <span className="font-semibold text-gray-800 text-base">{grupo.descricao}</span>
                    {grupo.tipo === 'PIZZA' && <span className="text-xs bg-orange-100 text-orange-600 px-2 py-0.5 rounded-full">🍕 Monte sua pizza</span>}
                  </div>
                  <span className="text-amber-600 ml-2 shrink-0">
                    {grupo.carregando
                      ? <span className="block w-4 h-4 border-2 border-amber-400 border-t-transparent rounded-full animate-spin" />
                      : grupo.aberto ? <ChevronUp className="w-5 h-5" /> : <ChevronDown className="w-5 h-5" />
                    }
                  </span>
                </button>

                {grupo.aberto && grupo.tipo === 'PIZZA' && grupo.pizza && (
                  <div className="border-t border-amber-50">
                    <PizzaBuilder
                      pizza={grupo.pizza}
                      nomeGrupo={grupo.descricao}
                      onAdicionarAoCarrinho={adicionarAoCarrinho}
                    />
                  </div>
                )}

                {grupo.aberto && grupo.tipo !== 'PIZZA' && grupo.produtos.length > 0 && (
                  <ul className="border-t border-amber-50 divide-y divide-amber-50">
                    {grupo.produtos.map(produto => (
                      <li key={produto.id} className="px-4 py-3.5 flex items-center gap-3">
                        {/* Thumbnail do produto */}
                        <ProdutoThumb id={produto.id} />
                        <div className="flex-1 min-w-0">
                          <p className="font-medium text-gray-800 text-sm leading-snug">{produto.descricao}</p>
                          {produto.caracteristica && <p className="text-xs text-gray-500 mt-0.5 leading-snug">{produto.caracteristica}</p>}
                        </div>
                        <div className="flex items-center gap-2 shrink-0">
                          <span className="font-bold text-amber-700 text-sm">{R$(produto.preco)}</span>
                          <button
                            onClick={() => adicionarAoCarrinho({ produtoId: produto.id, descricao: produto.descricao, preco: produto.preco })}
                            className="w-8 h-8 rounded-full bg-amber-500 text-white flex items-center justify-center hover:bg-amber-600 transition-colors shadow-sm"
                          >
                            <Plus className="w-4 h-4" />
                          </button>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}

                {grupo.aberto && grupo.tipo !== 'PIZZA' && grupo.produtos.length === 0 && !grupo.carregando && (
                  <p className="px-4 pb-4 text-sm text-gray-400 text-center">Nenhum item nesta categoria</p>
                )}
              </div>
            ))}
          </div>
        )}

        <p className="text-center text-xs text-gray-400 pt-4">Cardápio informativo — preços sujeitos a alteração</p>
      </div>

      {/* FAB do carrinho */}
      {totalCarrinho > 0 && !carrinhoAberto && (
        <button
          onClick={() => setCarrinhoAberto(true)}
          className="fixed bottom-6 left-1/2 -translate-x-1/2 flex items-center gap-3 bg-amber-600 hover:bg-amber-700 text-white font-bold px-6 py-3.5 rounded-2xl shadow-2xl transition-all animate-bounce-once z-40"
        >
          <ShoppingCart className="w-5 h-5" />
          <span>{totalCarrinho} item{totalCarrinho !== 1 ? 'ns' : ''} no carrinho</span>
          <span className="bg-white text-amber-700 rounded-full px-2 py-0.5 text-sm font-bold">
            {R$(carrinho.reduce((s, i) => s + i.preco * i.quantidade, 0))}
          </span>
        </button>
      )}

      {/* Modal do carrinho */}
      {carrinhoAberto && (
        <CarrinhoModal
          itens={carrinho}
          mesa={mesa}
          onClose={() => setCarrinhoAberto(false)}
          onAlterar={alterarQuantidade}
          onRemover={removerItem}
          onPedidoEnviado={pedidoEnviado}
        />
      )}
    </div>
  )
}
