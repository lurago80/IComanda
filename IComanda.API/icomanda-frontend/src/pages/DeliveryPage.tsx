import { ArrowLeft, Clock, Loader2, MapPin, MessageCircle, Package, Pencil, Phone, Plus, Printer, UserPlus, Search, Trash2, Truck } from 'lucide-react'
import React, { useCallback, useEffect, useState } from 'react'
import { vendasService } from '../services/api'
import { Cliente, Venda } from '../types/api'
import { Button } from '../components/ui/button'
import ClienteSearch from '../components/ClienteSearch'
import CadastroRapidoClienteDeliveryModal from '../components/CadastroRapidoClienteDeliveryModal'
import { useToast } from '../hooks/useToast'
import { enviarWhatsApp, formatarMensagemComanda, abrirLinkWhatsApp } from '../utils/whatsapp'
import NavModulos from '../components/NavModulos'

interface DeliveryPageProps {
  /** 'abertos' = só lista de pedidos em aberto; 'novo' = só cadastro (buscar/cadastrar cliente) */
  mode: 'abertos' | 'novo'
  onClose: () => void
  onNovoPedido: (cliente?: Cliente) => void
  onEditarPedido: (nota: string) => void
  onSwitchToNovo?: () => void
  onSwitchToAbertos?: () => void
  onIrParaComanda?: () => void
  onIrParaForcaVendas?: () => void
}

const DeliveryPage: React.FC<DeliveryPageProps> = ({
  mode,
  onClose,
  onNovoPedido,
  onEditarPedido,
  onSwitchToNovo,
  onSwitchToAbertos,
  onIrParaComanda,
  onIrParaForcaVendas
}) => {
  const [vendas, setVendas] = useState<Venda[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [nomeEstabelecimento, setNomeEstabelecimento] = useState<string>('')
  const [showClienteSearch, setShowClienteSearch] = useState(false)
  const [showCadastroRapido, setShowCadastroRapido] = useState(false)
  const [enviandoNota, setEnviandoNota] = useState<string | null>(null)
  const [imprimindoNota, setImprimindoNota] = useState<string | null>(null)
  const [enviandoWhatsApp, setEnviandoWhatsApp] = useState<string | null>(null)
  const [showCancelarModal, setShowCancelarModal] = useState(false)
  const [vendaParaCancelar, setVendaParaCancelar] = useState<Venda | null>(null)
  const [senhaCancelar, setSenhaCancelar] = useState('')
  const [justificativaCancelar, setJustificativaCancelar] = useState('')
  const [cancelando, setCancelando] = useState(false)
  const [erroCancel, setErroCancel] = useState('')
  const { showSuccess, showError, showWarning } = useToast()

  const carregar = useCallback(async () => {
    setIsLoading(true)
    try {
      const lista = await vendasService.getAbertas('DL')
      setVendas(lista)
    } catch (error) {
      console.error('Erro ao carregar pedidos delivery:', error)
      setVendas([])
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    carregar()
    vendasService.getEmitente().then((emitente) => {
      const nome = emitente?.nomeFantasia?.trim() || emitente?.nome?.trim() || ''
      if (nome) setNomeEstabelecimento(nome)
    }).catch(() => {})
  }, [carregar])

  const formatarMoeda = (valor: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor)

  const formatarTempoDecorrido = (dataEmissao?: string): string => {
    if (!dataEmissao) return ''
    const diff = Date.now() - new Date(dataEmissao).getTime()
    const minutos = Math.floor(diff / 60000)
    if (minutos < 1) return 'Agora'
    if (minutos < 60) return `${minutos}min`
    const horas = Math.floor(minutos / 60)
    const min = minutos % 60
    return min > 0 ? `${horas}h${min}min` : `${horas}h`
  }

  const handleClienteSelecionado = (cliente: Cliente) => {
    setShowClienteSearch(false)
    onNovoPedido(cliente)
  }

  const handleClienteCriado = (cliente: Cliente) => {
    setShowCadastroRapido(false)
    onNovoPedido(cliente)
  }

  const handleSaiuParaEntrega = async (e: React.MouseEvent, nota: string) => {
    e.stopPropagation()
    if (enviandoNota) return
    setEnviandoNota(nota)
    try {
      const { enviado, mensagem } = await vendasService.notificarSaiuParaEntrega(nota)
      
      // Sempre recarrega a lista (status é atualizado no backend mesmo que WhatsApp não envie)
      await carregar()
      
      if (enviado) {
        showSuccess('Pedido saiu para entrega', 'Status atualizado e cliente foi notificado por WhatsApp.')
      } else {
        showSuccess('Status atualizado', 'O pedido foi marcado como "saiu para entrega".')
        if (mensagem) {
          showWarning('WhatsApp', mensagem)
        }
      }
    } catch (err: any) {
      showError('Erro', err?.response?.data?.mensagem || err?.message || 'Não foi possível atualizar o status.')
    } finally {
      setEnviandoNota(null)
    }
  }

  const handleReimprimir = async (e: React.MouseEvent, nota: string) => {
    e.stopPropagation()
    if (imprimindoNota) return
    setImprimindoNota(nota)
    try {
      const vendaCompleta = await vendasService.getByNota(nota)
      const statusUpper = String(vendaCompleta.lancado || '').toUpperCase()
      const isAberta = statusUpper === 'ABERTO' || statusUpper === 'SAINDO'

      let itens = vendaCompleta.itens && vendaCompleta.itens.length > 0
        ? vendaCompleta.itens
        : []

      if (itens.length === 0) {
        try {
          // Para vendas abertas/saindo: buscar da tabela temporária
          // Para fechadas: buscar da tabela definitiva
          if (isAberta) {
            itens = await vendasService.getItensTemporariosByCupom(nota)
          } else {
            itens = await vendasService.getItensByNota(nota)
          }
        } catch (_) {
          itens = []
        }
      }

      const itensParaImprimir = itens.map((item) => ({
        codigo: item.codigo,
        descricao: item.descricao || `Item ${item.item}`,
        quantidade: item.qtd,
        preco: item.preco,
        observacao: undefined as string | undefined
      }))
      await vendasService.imprimir(nota, {
        itens: itensParaImprimir,
        apenasNovosItens: false,
        clienteNome: vendaCompleta.nomeCliente,
        subtotal: vendaCompleta.subtotal,
        desconto: vendaCompleta.desconto,
        acrescimo: vendaCompleta.acrescimo,
        isCupomDelivery: true,
        // Campos obrigatórios para a reimpressão sair igual à impressão original:
        enderecoEntrega: vendaCompleta.enderecoEntrega || undefined,
        pontoReferencia: vendaCompleta.pontoReferencia || undefined,
        telefoneEntrega: vendaCompleta.telefoneCliente || undefined,
        formaPgtoDelivery: vendaCompleta.formasPgto || undefined,
        // Detectar "já pago": se a forma de pagamento contém "PAGAMENTO" (ex: "PAGAMENTO ON LINE")
        // ou se o campo não for entrega em dinheiro/cobrar na entrega
        jaPagoDelivery: vendaCompleta.formasPgto
          ? !vendaCompleta.formasPgto.toUpperCase().includes('COBRAR')
          : false
      })
      showSuccess('Impressão', 'Pedido enviado para a impressora.')
    } catch (err: any) {
      showError('Impressão', err?.response?.data?.mensagem || 'Não foi possível imprimir. Verifique a impressora.')
    } finally {
      setImprimindoNota(null)
    }
  }

  const handleAbrirCancelar = (venda: Venda, e: React.MouseEvent) => {
    e.stopPropagation()
    setVendaParaCancelar(venda)
    setSenhaCancelar('')
    setJustificativaCancelar('')
    setErroCancel('')
    setShowCancelarModal(true)
  }

  const handleConfirmarCancelar = async () => {
    if (!vendaParaCancelar) return
    if (!justificativaCancelar.trim()) {
      showError('Atenção', 'Informe a justificativa para o cancelamento.')
      return
    }
    if (!senhaCancelar.trim()) {
      showError('Atenção', 'Digite a senha de cancelamento.')
      return
    }
    setCancelando(true)
    setErroCancel('')
    try {
      await vendasService.excluirComanda(vendaParaCancelar.nota, {
        justificativa: justificativaCancelar.trim(),
        senha: senhaCancelar.trim()
      })
      showSuccess('Pedido cancelado', 'O pedido delivery foi cancelado e registrado no histórico.')
      setShowCancelarModal(false)
      setVendaParaCancelar(null)
      setSenhaCancelar('')
      setJustificativaCancelar('')
      setErroCancel('')
      await carregar()
    } catch (err: any) {
      const msg = err?.response?.data?.mensagem || err?.message || 'Não foi possível cancelar. Verifique a senha.'
      setErroCancel(msg)
      setSenhaCancelar('')
    } finally {
      setCancelando(false)
    }
  }

  const handleEnviarWhatsApp = async (venda: Venda, e: React.MouseEvent) => {
    e.stopPropagation()
    if (!venda.telefoneCliente) {
      showError('Aviso', 'Cliente não possui telefone cadastrado.')
      return
    }
    if (enviandoWhatsApp) return
    setEnviandoWhatsApp(venda.nota)
    try {
      // Garantir que temos o nome fantasia antes de montar a mensagem
      let nomeParaMensagem = nomeEstabelecimento
      if (!nomeParaMensagem) {
        try {
          const emitente = await vendasService.getEmitente()
          nomeParaMensagem = emitente?.nomeFantasia?.trim() || emitente?.nome?.trim() || ''
          if (nomeParaMensagem) setNomeEstabelecimento(nomeParaMensagem)
        } catch {}
      }
      const vendaCompleta = await vendasService.getByNota(venda.nota)
      const vendaComItens = vendaCompleta.itens?.length
        ? vendaCompleta
        : { ...vendaCompleta, itens: await vendasService.getItensByNota(venda.nota) }
      // Usar nome retornado pelo backend como fallback (evita exibir "Estabelecimento")
      if (!nomeParaMensagem && vendaComItens.nomeEstabelecimento) {
        nomeParaMensagem = vendaComItens.nomeEstabelecimento
        setNomeEstabelecimento(nomeParaMensagem)
      }
      const mensagem = formatarMensagemComanda({ ...vendaComItens, nomeEstabelecimento: nomeParaMensagem })
      const resultado = await enviarWhatsApp(venda.telefoneCliente, mensagem)
      if (resultado.method === 'sent') {
        showSuccess('Enviado', 'Mensagem enviada com sucesso!')
      } else if (resultado.method === 'link' && resultado.link) {
        abrirLinkWhatsApp(resultado.link)
        showSuccess('WhatsApp aberto', 'Envie a mensagem na janela que abriu.')
      } else {
        showWarning('Não enviou', 'WhatsApp não está conectado. Use o link que abriu para enviar.')
      }
    } catch (err: any) {
      showWarning('Não enviou', err?.message || 'Não foi possível enviar. Use o link do WhatsApp se abriu.')
    } finally {
      setEnviandoWhatsApp(null)
    }
  }

  const titulo = mode === 'abertos' ? 'Delivery em aberto' : 'Novo pedido delivery'

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-50 border-b bg-card shadow-sm">
        <div className="flex items-center justify-between px-4 py-3">
          <button
            type="button"
            onClick={onClose}
            className="flex items-center gap-2 rounded-lg p-2 text-muted-foreground hover:bg-muted hover:text-foreground"
            aria-label="Voltar"
          >
            <ArrowLeft className="h-5 w-5" />
            <span className="hidden sm:inline">Voltar</span>
          </button>
          <h1 className="text-lg font-semibold text-foreground">{titulo}</h1>
          <div className="w-20 sm:w-24" />
        </div>
        {(onIrParaComanda || onIrParaForcaVendas) && (
          <NavModulos
            moduloAtivo="delivery"
            onComanda={onIrParaComanda}
            onDelivery={undefined}
            onForcaVendas={onIrParaForcaVendas}
          />
        )}
      </header>

      <ClienteSearch
        isOpen={showClienteSearch}
        onClose={() => setShowClienteSearch(false)}
        onSelectCliente={handleClienteSelecionado}
      />
      <CadastroRapidoClienteDeliveryModal
        isOpen={showCadastroRapido}
        onClose={() => setShowCadastroRapido(false)}
        onClienteCriado={handleClienteCriado}
      />

      <main className="p-4 pb-8">
        {mode === 'abertos' && (
          <>
            {isLoading ? (
              <div className="flex flex-col items-center justify-center gap-4 py-12">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                <p className="text-sm text-muted-foreground">Carregando pedidos...</p>
              </div>
            ) : vendas.length === 0 ? (
              <div className="flex flex-col items-center justify-center gap-4 rounded-xl border border-dashed bg-muted/30 py-16">
                <Package className="h-12 w-12 text-muted-foreground" />
                <h2 className="text-base font-semibold text-foreground">Pedidos em aberto</h2>
                <p className="text-center text-muted-foreground text-sm">Nenhum pedido de delivery em aberto. Toque em &quot;Fazer novo pedido&quot; para cadastrar.</p>
                {onSwitchToNovo && (
                  <Button type="button" onClick={onSwitchToNovo} className="mt-2 gap-2">
                    <Plus className="h-4 w-4" />
                    Fazer novo pedido
                  </Button>
                )}
              </div>
            ) : (
              <>
                <h2 className="text-base font-semibold text-foreground mb-3">Pedidos em aberto</h2>
                
                {/* Legenda de cores */}
                <div className="mb-4 flex flex-wrap gap-3 text-xs">
                  <div className="flex items-center gap-2">
                    <div className="w-4 h-4 rounded border bg-card border-border"></div>
                    <span className="text-muted-foreground">Aguardando preparo</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-4 h-4 rounded bg-blue-50 border-2 border-blue-400"></div>
                    <span className="text-muted-foreground">Saiu para entrega</span>
                  </div>
                </div>

                <ul className="space-y-3">
                {vendas.map((v) => {
                  const saiuParaEntrega = v.lancado?.toUpperCase() === 'SAINDO';
                  const tempo = formatarTempoDecorrido(v.emissao)
                  return (
              <li key={v.nota}>
                <div className={`rounded-2xl overflow-hidden shadow-md border transition-all ${
                  saiuParaEntrega
                    ? 'border-blue-300 bg-gradient-to-r from-blue-50 to-blue-100/60'
                    : 'border-border bg-card hover:border-primary/30 hover:shadow-lg'
                }`}>
                  {/* Faixa de status */}
                  <div className={`h-1.5 w-full ${saiuParaEntrega ? 'bg-blue-400' : 'bg-primary/40'}`} />

                  <div className="p-4">
                    {/* Linha superior: número, status, tempo, valor */}
                    <div className="flex items-start justify-between gap-2 mb-2">
                      <div className="flex items-center gap-2 flex-wrap">
                        <span className={`text-base font-bold ${saiuParaEntrega ? 'text-blue-800' : 'text-text-primary'}`}>
                          Pedido #{v.nota}
                        </span>
                        {saiuParaEntrega && (
                          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold bg-blue-200 text-blue-800">
                            <Truck className="h-3 w-3" />
                            Em trânsito
                          </span>
                        )}
                        {tempo && (
                          <span className="inline-flex items-center gap-1 text-xs text-text-muted">
                            <Clock className="h-3 w-3" />
                            {tempo}
                          </span>
                        )}
                      </div>
                      <span className={`text-base font-bold whitespace-nowrap ${saiuParaEntrega ? 'text-blue-700' : 'text-primary'}`}>
                        {formatarMoeda(v.total ?? 0)}
                      </span>
                    </div>

                    {/* Cliente e endereço */}
                    <div className="space-y-1 mb-3">
                      {v.nomeCliente && (
                        <p className="text-sm font-medium text-text-primary flex items-center gap-1.5">
                          <span className="w-1.5 h-1.5 rounded-full bg-primary inline-block" />
                          {v.nomeCliente}
                        </p>
                      )}
                      {v.enderecoEntrega && (
                        <p className="text-xs text-text-muted flex items-center gap-1.5">
                          <MapPin className="h-3 w-3 shrink-0 text-text-muted" />
                          <span className="truncate">{v.enderecoEntrega}</span>
                        </p>
                      )}
                      {v.telefoneCliente && (
                        <p className="text-xs text-text-muted flex items-center gap-1.5">
                          <Phone className="h-3 w-3 shrink-0 text-text-muted" />
                          {v.telefoneCliente}
                        </p>
                      )}
                    </div>

                    {/* Botões de ação */}
                    <div className="flex flex-wrap items-center gap-1.5 pt-2 border-t border-border/60">
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        className="gap-1 h-8 text-xs px-2"
                        onClick={(e) => { e.stopPropagation(); onEditarPedido(v.nota); }}
                        title="Editar pedido"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                        Editar
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        className="gap-1 h-8 text-xs px-2"
                        onClick={(e) => handleReimprimir(e, v.nota)}
                        disabled={imprimindoNota === v.nota}
                        title="Reimprimir pedido"
                      >
                        {imprimindoNota === v.nota ? (
                          <Loader2 className="h-3.5 w-3.5 animate-spin" />
                        ) : (
                          <Printer className="h-3.5 w-3.5" />
                        )}
                        Imprimir
                      </Button>
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        className={`gap-1 h-8 text-xs px-2 ${
                          saiuParaEntrega
                            ? 'bg-blue-600 border-blue-600 text-white hover:bg-blue-700'
                            : 'bg-primary/10 border-primary/30 hover:bg-primary/20 text-primary'
                        }`}
                        onClick={(e) => handleSaiuParaEntrega(e, v.nota)}
                        disabled={enviandoNota === v.nota}
                        title="Marcar saiu para entrega"
                      >
                        {enviandoNota === v.nota ? (
                          <Loader2 className="h-3.5 w-3.5 animate-spin" />
                        ) : (
                          <Truck className="h-3.5 w-3.5" />
                        )}
                        {saiuParaEntrega ? 'Em trânsito' : 'Saiu entrega'}
                      </Button>
                      {v.telefoneCliente && (
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          className="gap-1 h-8 text-xs px-2 bg-green-50 border-green-300 hover:bg-green-100 text-green-700"
                          onClick={(e) => handleEnviarWhatsApp(v, e)}
                          disabled={enviandoWhatsApp === v.nota}
                          title="Enviar pedido por WhatsApp"
                        >
                          {enviandoWhatsApp === v.nota ? (
                            <Loader2 className="h-3.5 w-3.5 animate-spin" />
                          ) : (
                            <MessageCircle className="h-3.5 w-3.5" />
                          )}
                          WhatsApp
                        </Button>
                      )}
                      <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        className="gap-1 h-8 text-xs px-2 border-red-300 text-red-600 hover:bg-red-50 ml-auto"
                        onClick={(e) => handleAbrirCancelar(v, e)}
                        title="Cancelar pedido delivery"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                        Cancelar
                      </Button>
                    </div>
                  </div>
                </div>
              </li>
                )})}
                </ul>
                {onSwitchToNovo && (
                  <div className="mt-4">
                    <Button type="button" variant="outline" onClick={onSwitchToNovo} className="w-full gap-2">
                      <Plus className="h-4 w-4" />
                      Fazer novo pedido
                    </Button>
                  </div>
                )}
              </>
            )}
          </>
        )}

        {mode === 'novo' && (
          <div className="pt-4">
            {onSwitchToAbertos && (
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={onSwitchToAbertos}
                className="mb-4 -ml-2 text-muted-foreground"
              >
                ← Ver pedidos em aberto
              </Button>
            )}
            <h2 className="text-base font-semibold text-foreground mb-3">Cliente do pedido</h2>
            <p className="text-sm text-muted-foreground mb-4">Busque um cliente ou cadastre um novo para iniciar o pedido de delivery.</p>
            <div className="flex flex-col gap-3 max-w-md">
              <Button
                type="button"
                variant="outline"
                className="w-full justify-start gap-3 h-12"
                onClick={() => setShowClienteSearch(true)}
              >
                <Search className="h-5 w-5" />
                Buscar cliente
              </Button>
              <Button
                type="button"
                variant="outline"
                className="w-full justify-start gap-3 h-12"
                onClick={() => setShowCadastroRapido(true)}
              >
                <UserPlus className="h-5 w-5" />
                Cadastrar cliente (nome e endereço — telefone opcional)
              </Button>
            </div>
          </div>
        )}
      </main>

      {/* Modal de cancelamento delivery */}
      {showCancelarModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4">
          <div className="w-full max-w-sm rounded-2xl bg-white p-6 shadow-xl">
            <h2 className="text-lg font-bold text-gray-900 mb-1">Cancelar pedido delivery</h2>
            <p className="text-sm text-gray-500 mb-4">
              Pedido{' '}
              <span className="font-semibold text-gray-700">#{vendaParaCancelar?.nota}</span>
              {vendaParaCancelar?.nomeCliente && (
                <> &mdash; {vendaParaCancelar.nomeCliente}</>
              )}
            </p>

            <label className="block text-sm font-medium text-gray-700 mb-1">Justificativa *</label>
            <textarea
              rows={3}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400 mb-4 resize-none"
              placeholder="Motivo do cancelamento..."
              value={justificativaCancelar}
              onChange={e => setJustificativaCancelar(e.target.value)}
              autoFocus
            />

            <label className="block text-sm font-medium text-gray-700 mb-1">Senha de cancelamento *</label>
            <input
              type="password"
              className={`w-full rounded-lg border px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400 ${erroCancel ? 'border-red-500 mb-1' : 'border-gray-300 mb-6'}`}
              placeholder="Senha"
              value={senhaCancelar}
              onChange={e => { setSenhaCancelar(e.target.value); setErroCancel('') }}
              onKeyDown={e => { if (e.key === 'Enter') handleConfirmarCancelar() }}
            />
            {erroCancel && (
              <p className="text-xs text-red-600 font-medium mb-4">{erroCancel}</p>
            )}

            <div className="flex gap-3">
              <button
                type="button"
                onClick={() => { setShowCancelarModal(false); setVendaParaCancelar(null); setSenhaCancelar(''); setJustificativaCancelar(''); setErroCancel('') }}
                className="flex-1 rounded-lg border border-gray-300 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
                disabled={cancelando}
              >
                Cancelar
              </button>
              <button
                type="button"
                onClick={handleConfirmarCancelar}
                disabled={cancelando || !senhaCancelar.trim() || !justificativaCancelar.trim()}
                className="flex-1 rounded-lg bg-red-600 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50 transition-colors"
              >
                {cancelando ? 'Cancelando...' : 'Confirmar cancelamento'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default DeliveryPage
