import { useEffect, useState } from 'react'
import { ArrowLeft, ChevronDown, ChevronRight, Edit2, Pizza, Plus, Save, Trash2, X } from 'lucide-react'
import api from '../services/api'

// ── Tipos ────────────────────────────────────────────────────────────────────

interface Grupo { id: number; descricao: string; tipo: string }
interface Tamanho { id: number; grupoId: number; descricao: string; ordem: number; sabores?: Sabor[] }
interface Sabor { id: number; tamanhoId: number; descricao: string; ingredientes?: string; preco: number; ativo: boolean }
interface Borda { id: number; descricao: string; preco: number; ativo: boolean }

const R$ = (v: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)
const inputCls = 'border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-orange-400'
const inputFullCls = `${inputCls} w-full`
const btnPrimary = 'flex items-center gap-1.5 px-3 py-2 bg-orange-600 text-white rounded-lg text-sm font-medium hover:bg-orange-700 transition-colors'
const btnSecondary = 'flex items-center gap-1.5 px-3 py-2 border border-gray-300 text-gray-700 rounded-lg text-sm font-medium hover:bg-gray-50 transition-colors'
const btnDanger = 'flex items-center gap-1.5 px-2 py-1.5 text-red-500 hover:bg-red-50 rounded-lg transition-colors'

// ── Componente principal ──────────────────────────────────────────────────────

interface Props { onClose: () => void }

export default function GerenciarPizzaPage({ onClose }: Props) {
  const [aba, setAba] = useState<'grupos' | 'bordas'>('grupos')
  const [grupos, setGrupos] = useState<Grupo[]>([])
  const [bordas, setBordas] = useState<Borda[]>([])
  const [tamanhosPorGrupo, setTamanhosPorGrupo] = useState<Record<number, Tamanho[]>>({})
  const [grupoExpandido, setGrupoExpandido] = useState<number | null>(null)
  const [tamanhoExpandido, setTamanhoExpandido] = useState<number | null>(null)

  const [erro, setErro] = useState<string | null>(null)

  // forms
  const [novoBorda, setNovoBorda] = useState({ descricao: '', preco: '' })
  const [editandoBorda, setEditandoBorda] = useState<Borda | null>(null)
  const [novoTamanho, setNovoTamanho] = useState<Record<number, { descricao: string; ordem: string }>>({})
  const [novoSabor, setNovoSabor] = useState<Record<number, { descricao: string; ingredientes: string; preco: string }>>({})
  const [editandoSabor, setEditandoSabor] = useState<Sabor | null>(null)

  useEffect(() => { carregarGrupos(); carregarBordas() }, [])

  const carregarGrupos = async () => {
    try {
      const res = await api.get('/grupos')
      setGrupos(res.data)
    } catch { /* silencioso */ }
  }

  const carregarBordas = async () => {
    try {
      const res = await api.get('/pizza/bordas')
      setBordas(res.data)
    } catch { /* silencioso */ }
  }

  const carregarTamanhos = async (grupoId: number) => {
    try {
      const res = await api.get(`/pizza/grupos/${grupoId}/tamanhos`, { params: { comSabores: true } })
      setTamanhosPorGrupo(prev => ({ ...prev, [grupoId]: res.data }))
    } catch { /* silencioso */ }
  }

  const toggleGrupo = async (g: Grupo) => {
    if (grupoExpandido === g.id) { setGrupoExpandido(null); return }
    setGrupoExpandido(g.id)
    setTamanhoExpandido(null)
    if (!tamanhosPorGrupo[g.id]) await carregarTamanhos(g.id)
  }

  const marcarComoPizza = async (g: Grupo) => {
    const novoTipo = g.tipo === 'PIZZA' ? 'NORMAL' : 'PIZZA'
    try {
      await api.patch(`/pizza/grupos/${g.id}/tipo`, { tipo: novoTipo })
      setGrupos(prev => prev.map(x => x.id === g.id ? { ...x, tipo: novoTipo } : x))
    } catch { /* silencioso */ }
  }

  // ── Tamanhos ────────────────────────────────────────────────────────────────

  const criarTamanho = async (grupoId: number) => {
    const form = novoTamanho[grupoId]
    if (!form?.descricao?.trim()) return
    try {
      await api.post(`/pizza/grupos/${grupoId}/tamanhos`, { descricao: form.descricao.trim(), ordem: parseInt(form.ordem || '0', 10) })
      setNovoTamanho(prev => ({ ...prev, [grupoId]: { descricao: '', ordem: '' } }))
      await carregarTamanhos(grupoId)
    } catch { /* silencioso */ }
  }

  const excluirTamanho = async (id: number, grupoId: number) => {
    if (!window.confirm('Excluir este tamanho e todos os seus sabores?')) return
    try {
      await api.delete(`/pizza/tamanhos/${id}`)
      await carregarTamanhos(grupoId)
    } catch { /* silencioso */ }
  }

  // ── Sabores ─────────────────────────────────────────────────────────────────

  const criarSabor = async (tamanhoId: number, grupoId: number) => {
    const form = novoSabor[tamanhoId]
    if (!form?.descricao?.trim()) return
    try {
      await api.post(`/pizza/tamanhos/${tamanhoId}/sabores`, {
        descricao: form.descricao.trim(),
        ingredientes: form.ingredientes?.trim() || null,
        preco: parseFloat((form.preco ?? '').replace(',', '.') || '0'),
      })
      setNovoSabor(prev => ({ ...prev, [tamanhoId]: { descricao: '', ingredientes: '', preco: '' } }))
      await carregarTamanhos(grupoId)
    } catch (e: any) { setErro(e?.response?.data?.mensagem ?? e?.message ?? 'Erro ao salvar sabor') }
  }

  const salvarSabor = async (grupoId: number) => {
    if (!editandoSabor) return
    try {
      await api.put(`/pizza/sabores/${editandoSabor.id}`, {
        descricao: editandoSabor.descricao,
        ingredientes: editandoSabor.ingredientes || null,
        preco: editandoSabor.preco,
        ativo: editandoSabor.ativo,
      })
      setEditandoSabor(null)
      await carregarTamanhos(grupoId)
    } catch (e: any) { setErro(e?.response?.data?.mensagem ?? e?.message ?? 'Erro ao salvar sabor') }
  }

  const excluirSabor = async (id: number, grupoId: number) => {
    if (!window.confirm('Excluir este sabor?')) return
    try {
      await api.delete(`/pizza/sabores/${id}`)
      await carregarTamanhos(grupoId)
    } catch (e: any) { setErro(e?.response?.data?.mensagem ?? e?.message ?? 'Erro ao excluir sabor') }
  }

  // ── Bordas ──────────────────────────────────────────────────────────────────

  const criarBorda = async () => {
    if (!novoBorda.descricao.trim()) return
    try {
      await api.post('/pizza/bordas', { descricao: novoBorda.descricao.trim(), preco: parseFloat((novoBorda.preco ?? '').replace(',', '.') || '0') })
      setNovoBorda({ descricao: '', preco: '' })
      await carregarBordas()
    } catch (e: any) { setErro(e?.response?.data?.mensagem ?? e?.message ?? 'Erro ao salvar borda') }
  }

  const salvarBorda = async () => {
    if (!editandoBorda) return
    try {
      await api.put(`/pizza/bordas/${editandoBorda.id}`, { descricao: editandoBorda.descricao, preco: editandoBorda.preco, ativo: editandoBorda.ativo })
      setEditandoBorda(null)
      await carregarBordas()
    } catch { /* silencioso */ }
  }

  const excluirBorda = async (id: number) => {
    if (!window.confirm('Excluir esta borda?')) return
    try {
      await api.delete(`/pizza/bordas/${id}`)
      await carregarBordas()
    } catch { /* silencioso */ }
  }

  const gruposPizza = grupos.filter(g => g.tipo === 'PIZZA')

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-3 flex items-center gap-3 sticky top-0 z-10 shadow-sm">
        <button onClick={onClose} className="p-2 rounded-lg hover:bg-gray-100 text-gray-500">
          <ArrowLeft className="w-5 h-5" />
        </button>
        <Pizza className="w-5 h-5 text-orange-600" />
        <h1 className="font-bold text-gray-900 text-lg">Gerenciar Pizzas</h1>
      </header>

      {/* Abas */}
      <div className="bg-white border-b border-gray-200 px-4 flex gap-0">
        {(['grupos', 'bordas'] as const).map(a => (
          <button
            key={a}
            onClick={() => setAba(a)}
            className={`px-5 py-3 text-sm font-medium border-b-2 transition-colors capitalize ${aba === a ? 'border-orange-500 text-orange-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`}
          >
            {a === 'grupos' ? 'Tamanhos & Sabores' : 'Bordas'}
          </button>
        ))}
      </div>

      {erro && (
        <div className="mx-4 mt-3 bg-red-50 border border-red-200 text-red-700 text-sm rounded-xl px-4 py-3 flex items-center justify-between">
          <span>{erro}</span>
          <button onClick={() => setErro(null)} className="ml-3 text-red-400 hover:text-red-600 font-bold">✕</button>
        </div>
      )}

      <div className="max-w-2xl mx-auto px-4 py-5 space-y-4">

        {/* ── ABA: GRUPOS / SABORES ── */}
        {aba === 'grupos' && (
          <>
            <p className="text-sm text-gray-500">Marque um grupo como <strong>Pizza</strong> para habilitar a montagem com tamanhos e sabores no cardápio digital.</p>

            {grupos.map(g => (
              <div key={g.id} className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
                {/* Linha do grupo */}
                <div className="flex items-center gap-3 px-4 py-3">
                  <button onClick={() => toggleGrupo(g)} className="flex-1 flex items-center gap-2 text-left min-w-0">
                    {grupoExpandido === g.id ? <ChevronDown className="w-4 h-4 text-gray-400 shrink-0" /> : <ChevronRight className="w-4 h-4 text-gray-400 shrink-0" />}
                    <span className="font-medium text-gray-800 truncate">{g.descricao}</span>
                    {g.tipo === 'PIZZA' && <span className="ml-1 px-2 py-0.5 text-xs bg-orange-100 text-orange-700 rounded-full shrink-0">Pizza</span>}
                  </button>
                  <label className="flex items-center gap-2 cursor-pointer shrink-0">
                    <span className="text-xs text-gray-500">Pizza</span>
                    <div
                      onClick={() => marcarComoPizza(g)}
                      className={`w-10 h-5 rounded-full transition-colors cursor-pointer ${g.tipo === 'PIZZA' ? 'bg-orange-500' : 'bg-gray-300'}`}
                    >
                      <div className={`w-5 h-5 bg-white rounded-full shadow transition-transform ${g.tipo === 'PIZZA' ? 'translate-x-5' : 'translate-x-0'}`} />
                    </div>
                  </label>
                </div>

                {/* Tamanhos (expandido + tipo pizza) */}
                {grupoExpandido === g.id && g.tipo === 'PIZZA' && (
                  <div className="border-t border-gray-100 px-4 py-3 space-y-3 bg-orange-50/30">
                    <p className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Tamanhos</p>

                    {(tamanhosPorGrupo[g.id] ?? []).map(tam => (
                      <div key={tam.id} className="bg-white rounded-lg border border-gray-200 overflow-hidden">
                        {/* Header do tamanho */}
                        <div className="flex items-center gap-2 px-3 py-2.5">
                          <button onClick={() => setTamanhoExpandido(tamanhoExpandido === tam.id ? null : tam.id)} className="flex-1 flex items-center gap-1.5 text-left">
                            {tamanhoExpandido === tam.id ? <ChevronDown className="w-3.5 h-3.5 text-gray-400" /> : <ChevronRight className="w-3.5 h-3.5 text-gray-400" />}
                            <span className="font-semibold text-gray-700 text-sm">{tam.descricao}</span>
                            <span className="text-xs text-gray-400">({(tam.sabores ?? []).length} sabores)</span>
                          </button>
                          <button onClick={() => excluirTamanho(tam.id, g.id)} className={btnDanger}>
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>

                        {/* Sabores */}
                        {tamanhoExpandido === tam.id && (
                          <div className="border-t border-gray-100 px-3 py-2 space-y-1.5">
                            {(tam.sabores ?? []).map(s => (
                              <div key={s.id}>
                                {editandoSabor?.id === s.id ? (
                                  <div className="space-y-1.5 bg-orange-50 rounded-lg p-2">
                                    <input className={inputFullCls} value={editandoSabor.descricao} onChange={e => setEditandoSabor({ ...editandoSabor, descricao: e.target.value })} placeholder="Nome do sabor" />
                                    <input className={inputFullCls} value={editandoSabor.ingredientes ?? ''} onChange={e => setEditandoSabor({ ...editandoSabor, ingredientes: e.target.value })} placeholder="Ingredientes (opcional)" />
                                    <input className={inputFullCls} value={String(editandoSabor.preco)} onChange={e => setEditandoSabor({ ...editandoSabor, preco: parseFloat(e.target.value.replace(',', '.')) || 0 })} placeholder="Preço" type="number" step="0.01" />
                                    <div className="flex gap-2">
                                      <button onClick={() => salvarSabor(g.id)} className={btnPrimary}><Save className="w-3.5 h-3.5" />Salvar</button>
                                      <button onClick={() => setEditandoSabor(null)} className={btnSecondary}><X className="w-3.5 h-3.5" />Cancelar</button>
                                    </div>
                                  </div>
                                ) : (
                                  <div className="flex items-center gap-2 py-1">
                                    <div className="flex-1 min-w-0">
                                      <span className="text-sm text-gray-800 font-medium">{s.descricao}</span>
                                      {s.ingredientes && <span className="text-xs text-gray-400 block truncate">{s.ingredientes}</span>}
                                    </div>
                                    <span className="text-sm font-semibold text-orange-700 shrink-0">{R$(s.preco)}</span>
                                    <button onClick={() => setEditandoSabor(s)} className="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"><Edit2 className="w-3.5 h-3.5" /></button>
                                    <button onClick={() => excluirSabor(s.id, g.id)} className={btnDanger}><Trash2 className="w-3.5 h-3.5" /></button>
                                  </div>
                                )}
                              </div>
                            ))}

                            {/* Novo sabor */}
                            <div className="pt-2 border-t border-gray-100 space-y-1.5">
                              <p className="text-xs text-gray-400 font-medium">Novo sabor</p>
                              <input className={inputFullCls} value={novoSabor[tam.id]?.descricao ?? ''} onChange={e => setNovoSabor(p => ({ ...p, [tam.id]: { ...p[tam.id], descricao: e.target.value } }))} placeholder="Ex: Calabresa" />
                              <input className={inputFullCls} value={novoSabor[tam.id]?.ingredientes ?? ''} onChange={e => setNovoSabor(p => ({ ...p, [tam.id]: { ...p[tam.id], ingredientes: e.target.value } }))} placeholder="Ingredientes (opcional)" />
                              <div className="flex gap-2">
                                <input className={`${inputCls} w-32`} value={novoSabor[tam.id]?.preco ?? ''} onChange={e => setNovoSabor(p => ({ ...p, [tam.id]: { ...p[tam.id], preco: e.target.value } }))} placeholder="R$ 0,00" type="number" step="0.01" />
                                <button onClick={() => criarSabor(tam.id, g.id)} className={btnPrimary}><Plus className="w-3.5 h-3.5" />Adicionar</button>
                              </div>
                            </div>
                          </div>
                        )}
                      </div>
                    ))}

                    {/* Novo tamanho */}
                    <div className="bg-white rounded-lg border border-dashed border-orange-300 p-3 space-y-2">
                      <p className="text-xs text-orange-600 font-medium">Novo tamanho (ex: P, M, G, Família)</p>
                      <div className="flex gap-2">
                        <input className={`${inputCls} flex-1`} value={novoTamanho[g.id]?.descricao ?? ''} onChange={e => setNovoTamanho(p => ({ ...p, [g.id]: { ...p[g.id], descricao: e.target.value } }))} placeholder="Ex: Grande" onKeyDown={e => e.key === 'Enter' && criarTamanho(g.id)} />
                        <input className={`${inputCls} w-20`} value={novoTamanho[g.id]?.ordem ?? ''} onChange={e => setNovoTamanho(p => ({ ...p, [g.id]: { ...p[g.id], ordem: e.target.value } }))} placeholder="Ordem" type="number" />
                        <button onClick={() => criarTamanho(g.id)} className={btnPrimary}><Plus className="w-4 h-4" />Criar</button>
                      </div>
                    </div>
                  </div>
                )}

                {/* Expandido mas não é pizza */}
                {grupoExpandido === g.id && g.tipo !== 'PIZZA' && (
                  <div className="border-t border-gray-100 px-4 py-3 text-sm text-gray-400">
                    Ative o toggle <strong>Pizza</strong> para configurar tamanhos e sabores neste grupo.
                  </div>
                )}
              </div>
            ))}
          </>
        )}

        {/* ── ABA: BORDAS ── */}
        {aba === 'bordas' && (
          <>
            <p className="text-sm text-gray-500">Bordas são exibidas no cardápio digital para todos os grupos de pizza. O cliente visualiza as opções e o preço adicional de cada uma.</p>

            {/* Lista de bordas */}
            <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden divide-y divide-gray-100">
              {bordas.length === 0 && (
                <div className="px-4 py-6 text-center text-gray-400 text-sm">Nenhuma borda cadastrada ainda.</div>
              )}
              {bordas.map(b => (
                <div key={b.id} className="px-4 py-3">
                  {editandoBorda?.id === b.id ? (
                    <div className="space-y-2">
                      <div className="flex gap-2">
                        <input className={`${inputCls} flex-1`} value={editandoBorda.descricao} placeholder="Descrição" onChange={e => setEditandoBorda({ ...editandoBorda, descricao: e.target.value })} />
                        <input className={`${inputCls} w-32`} value={String(editandoBorda.preco)} onChange={e => setEditandoBorda({ ...editandoBorda, preco: parseFloat(e.target.value.replace(',', '.')) || 0 })} type="number" step="0.01" placeholder="Preço (R$)" />
                      </div>
                      <label className="flex items-center gap-2 text-sm text-gray-600 cursor-pointer">
                        <input type="checkbox" checked={editandoBorda.ativo} onChange={e => setEditandoBorda({ ...editandoBorda, ativo: e.target.checked })} className="accent-orange-500" />
                        Ativa (visível no cardápio)
                      </label>
                      <div className="flex gap-2">
                        <button onClick={salvarBorda} className={btnPrimary}><Save className="w-3.5 h-3.5" />Salvar</button>
                        <button onClick={() => setEditandoBorda(null)} className={btnSecondary}><X className="w-3.5 h-3.5" />Cancelar</button>
                      </div>
                    </div>
                  ) : (
                    <div className="flex items-center gap-3">
                      <div className="flex-1">
                        <p className="font-medium text-gray-800 text-sm">{b.descricao}</p>
                        {!b.ativo && <span className="text-xs text-red-400">Inativa</span>}
                      </div>
                      <span className="font-semibold text-orange-700 text-sm">{b.preco > 0 ? `+ ${R$(b.preco)}` : 'Grátis'}</span>
                      <button onClick={() => setEditandoBorda(b)} className="p-1.5 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"><Edit2 className="w-4 h-4" /></button>
                      <button onClick={() => excluirBorda(b.id)} className={btnDanger}><Trash2 className="w-4 h-4" /></button>
                    </div>
                  )}
                </div>
              ))}
            </div>

            {/* Nova borda */}
            <div className="bg-white rounded-xl border border-dashed border-orange-300 p-4 space-y-3">
              <p className="text-sm font-semibold text-orange-700">Nova borda</p>
              <div className="flex gap-2 items-center">
                <input className={`${inputCls} flex-1 min-w-0`} value={novoBorda.descricao} onChange={e => setNovoBorda(p => ({ ...p, descricao: e.target.value }))} placeholder="Ex: Catupiry, Cheddar, Doce..." onKeyDown={e => e.key === 'Enter' && criarBorda()} />
                <input className={`${inputCls} w-32 shrink-0`} value={novoBorda.preco} onChange={e => setNovoBorda(p => ({ ...p, preco: e.target.value }))} placeholder="R$ 0,00" type="number" step="0.01" />
                <button onClick={criarBorda} className={`${btnPrimary} shrink-0`}><Plus className="w-4 h-4" />Adicionar</button>
              </div>
              <p className="text-xs text-gray-400">Preço 0 = exibido como "Grátis" ou "Nenhuma borda"</p>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
