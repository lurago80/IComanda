import React, { useCallback, useEffect, useState } from 'react'
import {
  ArrowLeft, Loader2, Plus, Search, ShoppingBag, Trash2, ChevronDown, ChevronUp
} from 'lucide-react'
import { forcaVendasService, clientesService, produtosService } from '../services/api'
import { BuscarProdutoRequest } from '../types/api'
import { Vendedor, Cliente, Produto } from '../types/api'
import { Button } from '../components/ui/button'
import { useToast } from '../hooks/useToast'

interface ItemTemp {
  idProduto: number
  descricao: string
  unidade: string
  precoUnitario: number
  quantidade: number
  desconto: number
}

interface NovoPedidoFVPageProps {
  onClose: () => void
  onSucesso: () => void
}

const NovoPedidoFVPage: React.FC<NovoPedidoFVPageProps> = ({ onClose, onSucesso }) => {
  const [vendedores, setVendedores]       = useState<Vendedor[]>([])
  const [idVendedor, setIdVendedor]       = useState<number | ''>('')

  const [buscaCliente, setBuscaCliente]   = useState('')
  const [clientes, setClientes]           = useState<Cliente[]>([])
  const [clienteSel, setClienteSel]       = useState<Cliente | null>(null)
  const [buscandoCliente, setBuscandoCliente] = useState(false)

  const [buscaProduto, setBuscaProduto]   = useState('')
  const [produtos, setProdutos]           = useState<Produto[]>([])
  const [buscandoProduto, setBuscandoProduto] = useState(false)

  const [itens, setItens]                 = useState<ItemTemp[]>([])
  const [condicaoPgto, setCondicaoPgto]   = useState('')
  const [obs, setObs]                     = useState('')
  const [descontoGeral, setDescontoGeral] = useState(0)
  const [acrescimo, setAcrescimo]         = useState(0)

  const [salvando, setSalvando]           = useState(false)
  const { showSuccess, showError, showWarning } = useToast()

  const [produtoParaAdicionar, setProdutoParaAdicionar] = useState<Produto | null>(null)
  const [qtdInputStr, setQtdInputStr]     = useState('1')

  useEffect(() => {
    forcaVendasService.getVendedoresAtivos().then(setVendedores).catch(() => {})
  }, [])

  // Busca clientes com debounce
  useEffect(() => {
    if (buscaCliente.length < 2) { setClientes([]); return }
    const timer = setTimeout(async () => {
      setBuscandoCliente(true)
      try {
        const r = await clientesService.buscar({ q: buscaCliente, ativo: true, naoBloqueado: true })
        setClientes(r)
      } catch { setClientes([]) }
      finally { setBuscandoCliente(false) }
    }, 400)
    return () => clearTimeout(timer)
  }, [buscaCliente])

  // Busca produtos com debounce
  const buscarProdutos = useCallback(async (q: string) => {
    if (q.length < 2) { setProdutos([]); return }
    setBuscandoProduto(true)
    const req: BuscarProdutoRequest = { q, ativo: true }
    try {
      const r = await produtosService.buscar(req)
      setProdutos(r)
    } catch { setProdutos([]) }
    finally { setBuscandoProduto(false) }
  }, [])

  useEffect(() => {
    const t = setTimeout(() => buscarProdutos(buscaProduto), 400)
    return () => clearTimeout(t)
  }, [buscaProduto, buscarProdutos])

  const selecionarCliente = (c: Cliente) => {
    setClienteSel(c)
    setBuscaCliente('')
    setClientes([])
  }

  const iniciarAdicaoProduto = (p: Produto) => {
    setProdutoParaAdicionar(p)
    setQtdInputStr('1')
    setBuscaProduto('')
    setProdutos([])
  }

  const handleQtdChange = (v: string) => {
    // Aceita dígitos e no máximo um separador decimal (ponto ou vírgula)
    const normalized = v.replace(',', '.')
    if (/^(\d*\.?\d*)$/.test(normalized)) {
      setQtdInputStr(v)
    }
  }

  const adicionarProduto = (p: Produto, qtd: number) => {
    const existe = itens.find(i => i.idProduto === p.id)
    if (existe) {
      setItens(prev => prev.map(i => i.idProduto === p.id ? { ...i, quantidade: i.quantidade + qtd } : i))
    } else {
      setItens(prev => [...prev, {
        idProduto: p.id,
        descricao: p.descricao,
        unidade: p.unMedida ?? 'UN',
        precoUnitario: p.precoVenda ?? 0,
        quantidade: qtd,
        desconto: 0,
      }])
    }
    setProdutoParaAdicionar(null)
  }

  const confirmarAdicaoProduto = () => {
    if (!produtoParaAdicionar) return
    const qtd = parseFloat(qtdInputStr.replace(',', '.'))
    if (isNaN(qtd) || qtd <= 0) {
      showWarning('Informe uma quantidade válida (maior que zero)')
      return
    }
    adicionarProduto(produtoParaAdicionar, qtd)
  }

  const removerItem = (idProduto: number) => setItens(prev => prev.filter(i => i.idProduto !== idProduto))

  const atualizarItem = (idProduto: number, campo: keyof ItemTemp, valor: number) => {
    setItens(prev => prev.map(i => i.idProduto === idProduto ? { ...i, [campo]: valor } : i))
  }

  const calcSubtotal = (item: ItemTemp) =>
    item.quantidade * item.precoUnitario * (1 - item.desconto / 100)

  const subtotal = itens.reduce((s, i) => s + calcSubtotal(i), 0)
  const total    = subtotal * (1 - descontoGeral / 100) + acrescimo

  const fmtMoeda = (v: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)

  const handleSalvar = async () => {
    if (!idVendedor) { showWarning('Selecione o vendedor'); return }
    if (!clienteSel) { showWarning('Selecione o cliente'); return }
    if (itens.length === 0) { showWarning('Adicione pelo menos um produto'); return }

    setSalvando(true)
    try {
      await forcaVendasService.criarPedido({
        idVendedor: Number(idVendedor),
        idCliente: clienteSel.id,
        condicaoPgto: condicaoPgto,
        obs: obs,
        desconto: descontoGeral,
        acrescimo,
        itens: itens.map(i => ({
          idProduto: i.idProduto,
          quantidade: i.quantidade,
          precoUnitario: i.precoUnitario,
          desconto: i.desconto,
        })),
      })
      showSuccess('Pedido criado com sucesso!')
      onSucesso()
    } catch {
      showError('Erro ao criar pedido')
    } finally {
      setSalvando(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white border-b px-4 py-3 flex items-center gap-3 shadow-sm">
        <button onClick={onClose} className="p-1.5 hover:bg-gray-100 rounded-lg" disabled={salvando}>
          <ArrowLeft className="w-5 h-5 text-gray-600" />
        </button>
        <div className="flex-1">
          <h1 className="font-bold text-gray-900 text-lg">Novo Pedido FV</h1>
          <p className="text-xs text-gray-500">{itens.length} item(s) · {fmtMoeda(total)}</p>
        </div>
        <Button onClick={handleSalvar} disabled={salvando} className="flex items-center gap-1.5 text-sm">
          {salvando ? <Loader2 className="w-4 h-4 animate-spin" /> : <ShoppingBag className="w-4 h-4" />}
          Salvar
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto p-4 space-y-4">

        {/* Vendedor */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
          <p className="text-sm font-semibold text-gray-700 mb-2">Vendedor *</p>
          <select
            value={idVendedor}
            onChange={e => setIdVendedor(e.target.value === '' ? '' : Number(e.target.value))}
            className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2 bg-white"
          >
            <option value="">Selecione o vendedor...</option>
            {vendedores.map(v => <option key={v.id} value={v.id}>{v.nome}</option>)}
          </select>
        </div>

        {/* Cliente */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
          <p className="text-sm font-semibold text-gray-700 mb-2">Cliente *</p>
          {clienteSel ? (
            <div className="flex items-center justify-between bg-green-50 border border-green-200 rounded-lg px-3 py-2">
              <div>
                <p className="text-sm font-medium text-green-800">{clienteSel.nome}</p>
                <p className="text-xs text-green-600">{(clienteSel as any).cpfCnpj ?? ''}</p>
              </div>
              <button onClick={() => setClienteSel(null)} className="text-green-700 hover:text-red-600">
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          ) : (
            <div className="relative">
              <input
                type="text"
                value={buscaCliente}
                onChange={e => setBuscaCliente(e.target.value)}
                placeholder="Buscar por nome ou CPF/CNPJ..."
                className="w-full text-sm border border-gray-300 rounded-lg pl-9 pr-3 py-2"
              />
              <Search className="absolute left-3 top-2.5 w-4 h-4 text-gray-400" />
              {buscandoCliente && <Loader2 className="absolute right-3 top-2.5 w-4 h-4 text-gray-400 animate-spin" />}
              {clientes.length > 0 && (
                <div className="absolute left-0 right-0 top-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-10 max-h-48 overflow-y-auto">
                  {clientes.map(c => (
                    <button
                      key={c.id}
                      onClick={() => selecionarCliente(c)}
                      className="w-full text-left px-3 py-2.5 hover:bg-gray-50 border-b border-gray-50 last:border-0"
                    >
                      <p className="text-sm font-medium text-gray-800">{c.nome}</p>
                      <p className="text-xs text-gray-400">{(c as any).cpfCnpj ?? ''}</p>
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Produtos */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4">
          <p className="text-sm font-semibold text-gray-700 mb-2">Produtos *</p>
          {/* Busca produto */}
          <div className="relative mb-3">
            <input
              type="text"
              value={buscaProduto}
              onChange={e => setBuscaProduto(e.target.value)}
              placeholder="Buscar produto..."
              className="w-full text-sm border border-gray-300 rounded-lg pl-9 pr-3 py-2"
            />
            <Search className="absolute left-3 top-2.5 w-4 h-4 text-gray-400" />
            {buscandoProduto && <Loader2 className="absolute right-3 top-2.5 w-4 h-4 text-gray-400 animate-spin" />}
            {produtos.length > 0 && (
              <div className="absolute left-0 right-0 top-full mt-1 bg-white border border-gray-200 rounded-lg shadow-lg z-10 max-h-48 overflow-y-auto">
                {produtos.map(p => (
                  <button
                    key={p.id}
                    onClick={() => iniciarAdicaoProduto(p)}
                    className="w-full text-left px-3 py-2 hover:bg-gray-50 border-b border-gray-50 last:border-0 flex justify-between items-center"
                  >
                    <div>
                      <p className="text-sm font-medium text-gray-800">{p.descricao}</p>
                      <p className="text-xs text-gray-400">{p.codigoInterno || p.codigoBarra || ''}</p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm font-semibold text-gray-900">{fmtMoeda(p.precoVenda ?? 0)}</p>
                      <Plus className="w-3 h-3 text-green-600 ml-auto mt-0.5" />
                    </div>
                  </button>
                ))}
              </div>
            )}
          </div>

          {/* Itens adicionados */}
          {itens.length === 0 ? (
            <p className="text-xs text-gray-400 text-center py-4">Nenhum produto adicionado ainda</p>
          ) : (
            <div className="space-y-2">
              {itens.map(item => (
                <div key={item.idProduto} className="border border-gray-100 rounded-lg p-3 space-y-2">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <p className="text-sm font-medium text-gray-800 truncate">{item.descricao}</p>
                      <p className="text-xs text-gray-400">{fmtMoeda(item.precoUnitario)}/{item.unidade}</p>
                    </div>
                    <button onClick={() => removerItem(item.idProduto)} className="text-red-400 hover:text-red-600 ml-2">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                  <div className="flex gap-2 items-center">
                    <div className="flex items-center gap-1">
                      <button
                        onClick={() => item.quantidade > 1 && atualizarItem(item.idProduto, 'quantidade', item.quantidade - 1)}
                        className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded text-gray-600 hover:bg-gray-50"
                      >
                        <ChevronDown className="w-3 h-3" />
                      </button>
                      <input
                        type="number"
                        min={0.01}
                        step={0.01}
                        value={item.quantidade}
                        onChange={e => atualizarItem(item.idProduto, 'quantidade', parseFloat(e.target.value) || 0)}
                        className="w-14 text-center text-sm border border-gray-300 rounded px-1 py-1"
                      />
                      <button
                        onClick={() => atualizarItem(item.idProduto, 'quantidade', item.quantidade + 1)}
                        className="w-7 h-7 flex items-center justify-center border border-gray-300 rounded text-gray-600 hover:bg-gray-50"
                      >
                        <ChevronUp className="w-3 h-3" />
                      </button>
                    </div>
                    <div className="flex-1 flex gap-1 items-center">
                      <span className="text-xs text-gray-400">Desc.%</span>
                      <input
                        type="number"
                        min={0}
                        max={100}
                        value={item.desconto}
                        onChange={e => atualizarItem(item.idProduto, 'desconto', parseFloat(e.target.value) || 0)}
                        className="w-16 text-sm border border-gray-300 rounded px-1 py-1"
                      />
                    </div>
                    <p className="text-sm font-semibold text-gray-900 text-right">{fmtMoeda(calcSubtotal(item))}</p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Condição de pagamento */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4 space-y-3">
          <p className="text-sm font-semibold text-gray-700">Condições e Observações</p>
          <input
            type="text"
            value={condicaoPgto}
            onChange={e => setCondicaoPgto(e.target.value)}
            placeholder="Condição de pagamento (ex: 30/60/90)"
            className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
          />
          <div className="flex gap-3">
            <div className="flex-1">
              <label className="text-xs text-gray-500 block mb-1">Desconto geral (%)</label>
              <input
                type="number"
                min={0}
                max={100}
                value={descontoGeral}
                onChange={e => setDescontoGeral(parseFloat(e.target.value) || 0)}
                className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
              />
            </div>
            <div className="flex-1">
              <label className="text-xs text-gray-500 block mb-1">Acréscimo (R$)</label>
              <input
                type="number"
                min={0}
                step={0.01}
                value={acrescimo}
                onChange={e => setAcrescimo(parseFloat(e.target.value) || 0)}
                className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
              />
            </div>
          </div>
          <textarea
            value={obs}
            onChange={e => setObs(e.target.value)}
            placeholder="Observações..."
            rows={3}
            className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2 resize-none"
          />
        </div>

        {/* Totais */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-4 space-y-1.5">
          <div className="flex justify-between text-sm text-gray-600">
            <span>Subtotal</span>
            <span>{fmtMoeda(subtotal)}</span>
          </div>
          {descontoGeral > 0 && (
            <div className="flex justify-between text-sm text-red-600">
              <span>Desconto geral ({descontoGeral}%)</span>
              <span>- {fmtMoeda(subtotal * descontoGeral / 100)}</span>
            </div>
          )}
          {acrescimo > 0 && (
            <div className="flex justify-between text-sm text-blue-600">
              <span>Acréscimo</span>
              <span>+ {fmtMoeda(acrescimo)}</span>
            </div>
          )}
          <div className="flex justify-between font-bold text-gray-900 text-base pt-1 border-t border-gray-100">
            <span>Total</span>
            <span>{fmtMoeda(total)}</span>
          </div>
        </div>

        {/* Botão final */}
        <Button onClick={handleSalvar} disabled={salvando} className="w-full py-3 text-base font-semibold">
          {salvando ? <><Loader2 className="w-5 h-5 animate-spin mr-2" /> Salvando...</> : 'Confirmar Pedido'}
        </Button>

      </div>

      {/* Modal de quantidade ao adicionar produto */}
      {produtoParaAdicionar && (
        <div className="fixed inset-0 bg-black/50 flex items-end sm:items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl w-full max-w-sm shadow-xl p-5 space-y-4">

            {/* Cabeçalho */}
            <div>
              <p className="text-xs text-gray-400 uppercase tracking-wide mb-1">Adicionar produto</p>
              <p className="text-base font-bold text-gray-900 leading-tight">{produtoParaAdicionar.descricao}</p>
              <p className="text-sm text-gray-500 mt-0.5">
                {fmtMoeda(produtoParaAdicionar.precoVenda ?? 0)} / {produtoParaAdicionar.unMedida ?? 'UN'}
              </p>
            </div>

            {/* Campo de quantidade */}
            <div>
              <label className="text-sm font-semibold text-gray-700 block mb-1.5">Quantidade</label>
              <input
                type="text"
                inputMode="decimal"
                value={qtdInputStr}
                onChange={e => handleQtdChange(e.target.value)}
                onFocus={e => e.target.select()}
                autoFocus
                className="w-full text-center text-3xl font-bold border-2 border-blue-400 rounded-xl px-4 py-3 focus:outline-none focus:border-blue-600 tracking-widest"
                placeholder="1"
              />
              <p className="text-xs text-gray-400 mt-1.5 text-center">
                Use vírgula ou ponto para fracionados &nbsp;·&nbsp; ex: 0,250
              </p>
            </div>

            {/* Preview do total */}
            {(() => {
              const qtd = parseFloat(qtdInputStr.replace(',', '.'))
              return !isNaN(qtd) && qtd > 0 ? (
                <div className="bg-blue-50 border border-blue-100 rounded-xl px-4 py-2.5 flex justify-between items-center">
                  <span className="text-sm text-blue-700 font-medium">Total do item</span>
                  <span className="text-lg font-bold text-blue-900">
                    {fmtMoeda(qtd * (produtoParaAdicionar.precoVenda ?? 0))}
                  </span>
                </div>
              ) : null
            })()}

            {/* Botões */}
            <div className="flex gap-3 pt-1">
              <button
                onClick={() => setProdutoParaAdicionar(null)}
                className="flex-1 py-2.5 border border-gray-300 rounded-xl text-sm font-semibold text-gray-700 hover:bg-gray-50 active:bg-gray-100"
              >
                Cancelar
              </button>
              <button
                onClick={confirmarAdicaoProduto}
                className="flex-1 py-2.5 bg-blue-600 rounded-xl text-sm font-semibold text-white hover:bg-blue-700 active:bg-blue-800"
              >
                Adicionar
              </button>
            </div>

          </div>
        </div>
      )}

    </div>
  )
}

export default NovoPedidoFVPage
