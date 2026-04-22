import { AnimatePresence, motion } from 'framer-motion'
import { Loader2, Package, RefreshCw, Search, Trash2, X, MessageCircle, Copy, ClipboardList } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { useToast } from '../hooks/useToast'
import { vendasService, receberService } from '../services/api'
import { Venda, ContasAberto, ItemVenda } from '../types/api'
import { Button } from './ui/button'
import ConectarWhatsAppModal from './ConectarWhatsAppModal'
import { enviarWhatsApp, formatarMensagemComanda, formatarNumeroTelefone, abrirLinkWhatsApp } from '../utils/whatsapp'

interface ComandasAbertasModalProps {
  isOpen: boolean
  onClose: () => void
  onEditarComanda?: (nota: string) => void
  onRefresh?: () => void
}

const ComandasAbertasModal: React.FC<ComandasAbertasModalProps> = ({
  isOpen,
  onClose,
  onEditarComanda,
  onRefresh
}) => {
  const [vendas, setVendas] = useState<Venda[]>([])
  const [buscaNome, setBuscaNome] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [vendasComItens, setVendasComItens] = useState<Map<string, Venda>>(new Map())
  const [enviandoWhatsApp, setEnviandoWhatsApp] = useState<string | null>(null)
  const [showConectarWhatsAppModal, setShowConectarWhatsAppModal] = useState(false)
  const [contasAbertoPorNota, setContasAbertoPorNota] = useState<Map<string, ContasAberto>>(new Map())
  const [verificacaoVenda, setVerificacaoVenda] = useState<Venda | null>(null)
  const [verificacaoItens, setVerificacaoItens] = useState<ItemVenda[]>([])
  const [loadingVerificacao, setLoadingVerificacao] = useState(false)
  const [nomeEstabelecimento, setNomeEstabelecimento] = useState<string>('')
  const { showError, showSuccess, showWarning } = useToast()

  // Estados do modal de exclusão
  const [showExcluirModal, setShowExcluirModal] = useState(false)
  const [vendaParaExcluir, setVendaParaExcluir] = useState<Venda | null>(null)
  const [senhaExcluir, setSenhaExcluir] = useState('')
  const [justificativaExcluir, setJustificativaExcluir] = useState('')
  const [erroExcluir, setErroExcluir] = useState('')
  const [excluindo, setExcluindo] = useState(false)

  useEffect(() => {
    if (isOpen) {
      carregarVendasAbertas()
      if (!nomeEstabelecimento) {
        vendasService.getEmitente().then((e) => {
          const nome = e?.nomeFantasia?.trim() || (e as any)?.nome?.trim() || ''
          if (nome) setNomeEstabelecimento(nome)
        }).catch(() => {})
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isOpen])

  const handleEnviarWhatsApp = async (venda: Venda) => {
    // Verificar se tem cliente vinculado
    if (!venda.cliente || venda.cliente === 0) {
      showError('Aviso', 'Esta comanda não possui cliente vinculado');
      return;
    }

    // Verificar se tem telefone
    if (!venda.telefoneCliente) {
      showError('Aviso', 'Cliente não possui telefone cadastrado');
      return;
    }

    // Verificar se telefone é válido
    const telefoneValido = formatarNumeroTelefone(venda.telefoneCliente);
    if (!telefoneValido) {
      showError('Aviso', 'Telefone do cliente é inválido');
      return;
    }

    // Verificar se já está enviando
    if (enviandoWhatsApp === venda.nota) {
      return; // Já está enviando, não fazer nada
    }

    setEnviandoWhatsApp(venda.nota);

    try {
      // Buscar itens da venda se ainda não foram carregados
      let vendaComItens = vendasComItens.get(venda.nota);
      if (!vendaComItens || !vendaComItens.itens || vendaComItens.itens.length === 0) {
        console.log('[ComandasAbertasModal] Buscando itens da venda:', venda.nota);
        const vendaCompleta = await vendasService.getByNota(venda.nota);
        vendaComItens = vendaCompleta;
        
        // Se ainda não tiver itens, buscar diretamente
        if (!vendaCompleta.itens || vendaCompleta.itens.length === 0) {
          console.log('[ComandasAbertasModal] Buscando itens diretamente da API');
          const itens = await vendasService.getItensByNota(venda.nota);
          vendaComItens = {
            ...vendaCompleta,
            itens: itens
          };
        }
        
        setVendasComItens(new Map(vendasComItens.set(venda.nota, vendaComItens)));
      }

      console.log('[ComandasAbertasModal] Venda com itens:', vendaComItens);
      console.log('[ComandasAbertasModal] Itens detalhados:', vendaComItens?.itens?.map(item => ({
        codigo: item.codigo,
        descricao: item.descricao,
        qtd: item.qtd,
        preco: item.preco
      })));
      const nomeParaMsg = nomeEstabelecimento || (vendaComItens || venda).nomeEstabelecimento || '';
      const mensagem = formatarMensagemComanda({ ...(vendaComItens || venda), nomeEstabelecimento: nomeParaMsg });
      console.log('[ComandasAbertasModal] Mensagem formatada:', mensagem);
      
      try {
        const resultado = await enviarWhatsApp(venda.telefoneCliente, mensagem);
        if (resultado.method === 'sent') {
          showSuccess('✅ Enviado', 'Mensagem enviada com sucesso!');
        } else if (resultado.method === 'link' && resultado.link) {
          abrirLinkWhatsApp(resultado.link);
          showSuccess('WhatsApp aberto', 'Envie a mensagem na janela que abriu (ou nesta aba).');
        } else {
          showWarning('Não enviou', 'WhatsApp não está conectado. Siga as instruções na tela para conectar e depois enviar direto.');
          setShowConectarWhatsAppModal(true);
        }
      } catch (error: any) {
        // Tratar erros do enviarWhatsApp - sempre mostrar modal de instruções quando falhar
        if (error.message?.includes('ERRO_CONFIGURACAO')) {
          showError('⚠️ Configuração Necessária', 
            'WhatsApp Web não está conectado ao backend.\n\n' +
            'Para envio automático:\n' +
            '1. Execute: iniciar-chrome-whatsapp.bat\n' +
            '2. Abra: https://web.whatsapp.com\n' +
            '3. Faça o login\n' +
            '4. Tente novamente'
          );
        } else {
          console.error('[ComandasAbertasModal] Erro ao enviar WhatsApp:', error);
          showWarning('Não enviou', error.message || 'Não foi possível enviar. Siga as instruções na tela para conectar o WhatsApp.');
          setShowConectarWhatsAppModal(true);
        }
      }
    } catch (error: any) {
      console.error('[ComandasAbertasModal] Erro ao enviar WhatsApp:', error);
      showWarning('Não enviou', error.message || 'Não foi possível enviar. Siga as instruções na tela para conectar o WhatsApp.');
      setShowConectarWhatsAppModal(true);
    } finally {
      setEnviandoWhatsApp(null);
    }
  }

  const handleCopiarMensagem = async (venda: Venda) => {
    // Verificar se tem cliente vinculado
    if (!venda.cliente || venda.cliente === 0) {
      showError('Aviso', 'Esta comanda não possui cliente vinculado');
      return;
    }

    // Verificar se tem telefone
    if (!venda.telefoneCliente) {
      showError('Aviso', 'Cliente não possui telefone cadastrado');
      return;
    }

    // Verificar se telefone é válido
    const telefoneValido = formatarNumeroTelefone(venda.telefoneCliente);
    if (!telefoneValido) {
      showError('Aviso', 'Telefone do cliente é inválido');
      return;
    }

    try {
      // Buscar itens da venda se ainda não foram carregados
      let vendaComItens = vendasComItens.get(venda.nota);
      if (!vendaComItens || !vendaComItens.itens || vendaComItens.itens.length === 0) {
        console.log('[ComandasAbertasModal] Buscando itens da venda:', venda.nota);
        const vendaCompleta = await vendasService.getByNota(venda.nota);
        vendaComItens = vendaCompleta;
        
        // Se ainda não tiver itens, buscar diretamente
        if (!vendaCompleta.itens || vendaCompleta.itens.length === 0) {
          console.log('[ComandasAbertasModal] Buscando itens diretamente da API');
          const itens = await vendasService.getItensByNota(venda.nota);
          vendaComItens = {
            ...vendaCompleta,
            itens: itens
          };
        }
        
        setVendasComItens(new Map(vendasComItens.set(venda.nota, vendaComItens)));
      }

      const nomeParaMsg = nomeEstabelecimento || (vendaComItens || venda).nomeEstabelecimento || '';
      const mensagem = formatarMensagemComanda({ ...(vendaComItens || venda), nomeEstabelecimento: nomeParaMsg });
      const resultado = await enviarWhatsApp(venda.telefoneCliente, mensagem);
      if (resultado.method === 'sent') {
        showSuccess('✅ Enviado', 'Mensagem enviada com sucesso!');
      } else if (resultado.method === 'link' && resultado.link) {
        abrirLinkWhatsApp(resultado.link);
        showSuccess('WhatsApp aberto', 'Envie a mensagem na janela que abriu (ou nesta aba).');
      } else {
        setShowConectarWhatsAppModal(true);
      }
    } catch (error: any) {
      console.error('[ComandasAbertasModal] Erro ao copiar/enviar WhatsApp:', error);
      
      // Tratar erros do enviarWhatsApp
      if (error.message?.includes('ERRO_CONFIGURACAO')) {
        showError('⚠️ Configuração Necessária', 
          'WhatsApp Web não está conectado ao backend.\n\n' +
          'Para envio automático:\n' +
          '1. Execute: iniciar-chrome-whatsapp.bat\n' +
          '2. Abra: https://web.whatsapp.com\n' +
          '3. Faça o login\n' +
          '4. Tente novamente'
        );
      } else {
        showError('❌ Erro ao Enviar', error.message || 'Não foi possível enviar a mensagem automaticamente. Verifique se o WhatsApp Web está aberto e conectado.');
      }
    }
  }

  const handleAbrirExcluir = (venda: Venda) => {
    setVendaParaExcluir(venda)
    setSenhaExcluir('')
    setJustificativaExcluir('')
    setErroExcluir('')
    setShowExcluirModal(true)
  }

  const handleConfirmarExcluir = async () => {
    if (!vendaParaExcluir) return
    if (!justificativaExcluir.trim()) {
      showError('Atenção', 'Informe a justificativa para o cancelamento.')
      return
    }
    if (!senhaExcluir.trim()) {
      showError('Atenção', 'Digite a senha de cancelamento para confirmar.')
      return
    }
    setExcluindo(true)
    setErroExcluir('')
    try {
      await vendasService.excluirComanda(vendaParaExcluir.nota, { justificativa: justificativaExcluir.trim(), senha: senhaExcluir.trim() })
      showSuccess('Comanda excluída', 'A comanda foi excluída e registrada no histórico.')
      setShowExcluirModal(false)
      setVendaParaExcluir(null)
      setSenhaExcluir('')
      setJustificativaExcluir('')
      setErroExcluir('')
      carregarVendasAbertas()
      onRefresh?.()
    } catch (err: any) {
      const msg = err?.response?.data?.mensagem || err?.message || 'Não foi possível excluir. Verifique a senha.'
      setErroExcluir(msg)
      setSenhaExcluir('')
    } finally {
      setExcluindo(false)
    }
  }

  const handleAbrirVerificacao = async (venda: Venda) => {
    setLoadingVerificacao(true)
    setVerificacaoVenda(venda)
    setVerificacaoItens([])
    try {
      const itens = await vendasService.getItensTemporariosByCupom(venda.nota)
      setVerificacaoItens(Array.isArray(itens) ? itens : [])
    } catch (e: any) {
      console.error('[ComandasAbertasModal] Erro ao carregar itens para verificação:', e)
      showError('Erro', e?.response?.data || 'Não foi possível carregar os lançamentos.')
      setVerificacaoItens([])
    } finally {
      setLoadingVerificacao(false)
    }
  }

  const formatarDataHora = (emissao?: string): { data: string; hora: string } => {
    const vazio = { data: '—', hora: '—' }
    if (!emissao) return vazio
    try {
      const d = new Date(emissao)
      if (Number.isNaN(d.getTime())) return vazio
      const data = d.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' })
      const hora = d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
      return { data, hora }
    } catch {
      return vazio
    }
  }

  const carregarVendasAbertas = async () => {
    setIsLoading(true)
    try {
      const vendasAbertas = await vendasService.getAbertas()
      console.log('[ComandasAbertasModal] Vendas abertas carregadas:', vendasAbertas.length)
      console.log('[ComandasAbertasModal] Primeira venda:', vendasAbertas[0])
      vendasAbertas.forEach((venda, index) => {
        if (venda.telefoneCliente) {
          console.log(`[ComandasAbertasModal] Venda ${index + 1} (${venda.nota}) - Telefone: ${venda.telefoneCliente}`)
        }
      })
      setVendas(vendasAbertas)
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

  const termoBusca = buscaNome.trim().toLowerCase()
  const vendasOrdenadasEFiltradas = [...vendas]
    .sort((a, b) => {
      const nomeA = (a.nomeCliente || '').trim()
      const nomeB = (b.nomeCliente || '').trim()
      if (!nomeA && !nomeB) return 0
      if (!nomeA) return 1
      if (!nomeB) return -1
      return nomeA.localeCompare(nomeB, 'pt-BR')
    })
    .filter(v => !termoBusca || (v.nomeCliente || '').toLowerCase().includes(termoBusca))

  if (!isOpen) return null

  return (
  <>
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
          <div className="bg-primary text-primary-foreground p-6 flex-shrink-0">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-2xl font-bold">Comandas em Aberto</h2>
              <div className="flex items-center gap-2">
              <button
                onClick={carregarVendasAbertas}
                disabled={isLoading}
                className="p-2 hover:bg-white/20 rounded-xl transition-colors disabled:opacity-50"
                title="Atualizar lista"
              >
                <RefreshCw size={20} className={isLoading ? 'animate-spin' : ''} />
              </button>
              <button
                onClick={onClose}
                className="p-2 hover:bg-white/20 rounded-xl transition-colors"
              >
                <X size={24} />
              </button>
              </div>
            </div>
            <div className="relative w-full">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-primary-foreground/70" />
              <input
                type="text"
                value={buscaNome}
                onChange={(e) => setBuscaNome(e.target.value)}
                placeholder="Buscar por nome do cliente"
                className="w-full pl-9 pr-4 py-2 rounded-xl bg-white/20 text-primary-foreground placeholder-primary-foreground/60 border border-white/30 focus:outline-none focus:ring-2 focus:ring-white/50"
              />
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
            ) : vendasOrdenadasEFiltradas.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-12 space-y-4">
                <Search className="w-16 h-16 text-text-muted" />
                <p className="text-lg font-semibold text-text-primary">Nenhum resultado</p>
                <p className="text-sm text-text-secondary">Nenhuma comanda com esse nome. Tente outro termo.</p>
              </div>
            ) : (
              <div className="space-y-4">
                {vendasOrdenadasEFiltradas.map((venda, index) => (
                  <div
                    key={`venda-${String(venda.nota ?? '').trim() || 'sem-nota'}-${index}`}
                    className="bg-card-secondary rounded-2xl p-5 border border-border hover:border-primary/30 transition-all"
                  >
                    <div className="flex items-start justify-between gap-4 mb-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2 flex-wrap">
                          <span className="text-sm font-semibold text-text-secondary">Nota:</span>
                          <span className="text-lg font-bold text-primary">
                            {formatarNota(venda.nota)}
                          </span>
                          {venda.comanda != null && (
                            <>
                              <span className="text-text-muted">·</span>
                              <span className="text-sm text-text-secondary">Comanda:</span>
                              <span className="font-bold text-text-primary">{venda.comanda}</span>
                            </>
                          )}
                          {((venda.nomeCliente ?? '').trim()) !== '' && (
                            <>
                              <span className="text-text-muted">·</span>
                              <span className="text-sm text-text-secondary">Cliente:</span>
                              <span className="font-bold text-primary">{(venda.nomeCliente ?? '').trim()}</span>
                            </>
                          )}
                          {contasAbertoPorNota.get(venda.nota)?.temContasAberto && contasAbertoPorNota.get(venda.nota)!.valorTotalPendente > 0 && (
                            <span className="text-red-600 font-bold text-sm px-2 py-0.5 rounded-md bg-red-50 border border-red-200" title={contasAbertoPorNota.get(venda.nota)?.mensagem}>
                              ⚠️ Cliente deve
                            </span>
                          )}
                        </div>
                        <div className="space-y-1 text-sm text-text-secondary">
                          {venda.mesa && (
                            <p>Mesa: <span className="font-semibold text-text-primary">{venda.mesa}</span></p>
                          )}
                          {venda.numeroPessoas && (
                            <p>Pessoas: <span className="font-semibold text-text-primary">{venda.numeroPessoas}</span></p>
                          )}
                        </div>
                      </div>
                      <div className="flex flex-col items-end gap-2">
                        <div className="text-right">
                          <p className="text-sm text-text-secondary mb-1">Total</p>
                          <p className="text-2xl font-bold text-primary">
                            R$ {formatarPreco(venda.total)}
                          </p>
                        </div>
                        {/* Botões WhatsApp - lado direito, apenas se cliente tiver telefone */}
                        {(() => {
                          const temCliente = venda.cliente && venda.cliente > 0;
                          const temTelefone = venda.telefoneCliente && formatarNumeroTelefone(venda.telefoneCliente);
                          const deveMostrar = temCliente && temTelefone;
                          
                          console.log(`[ComandasAbertasModal] Venda ${venda.nota} - Cliente: ${venda.cliente}, Telefone: ${venda.telefoneCliente}, Mostrar botão: ${deveMostrar}`);
                          
                          return deveMostrar ? (
                            <div className="flex gap-2">
                              <Button
                                onClick={async () => {
                                  console.log('[ComandasAbertasModal] Clicou em WhatsApp para venda:', venda.nota);
                                  await handleEnviarWhatsApp(venda);
                                }}
                                disabled={enviandoWhatsApp === venda.nota}
                                className="bg-green-600 hover:bg-green-700 text-white flex items-center justify-center p-2 rounded-xl disabled:opacity-50 disabled:cursor-not-allowed"
                                variant="outline"
                                title={enviandoWhatsApp === venda.nota ? "Enviando..." : "Enviar comanda via WhatsApp"}
                              >
                                {enviandoWhatsApp === venda.nota ? (
                                  <Loader2 className="w-5 h-5 animate-spin" />
                                ) : (
                                  <MessageCircle className="w-5 h-5" />
                                )}
                              </Button>
                              <Button
                                onClick={async () => {
                                  console.log('[ComandasAbertasModal] Clicou em Copiar mensagem para venda:', venda.nota);
                                  await handleCopiarMensagem(venda);
                                }}
                                className="bg-blue-600 hover:bg-blue-700 text-white flex items-center justify-center p-2 rounded-xl"
                                variant="outline"
                                title="Copiar mensagem para área de transferência"
                              >
                                <Copy className="w-5 h-5" />
                              </Button>
                            </div>
                          ) : null;
                        })()}
                      </div>
                    </div>
                    <div className="flex gap-2 mt-3 flex-wrap">
                      <Button
                        onClick={() => handleAbrirVerificacao(venda)}
                        className="flex-1 min-w-[140px]"
                        variant="outline"
                        title="Ver data/hora, quantidade e valor de cada item lançado"
                      >
                        <ClipboardList className="w-4 h-4 mr-2" />
                        Verificação de lançamentos
                      </Button>
                      {onEditarComanda && (
                        <Button
                          onClick={() => {
                            onEditarComanda(venda.nota)
                            onClose()
                          }}
                          className="flex-1 min-w-[140px]"
                          variant="outline"
                        >
                          Editar Comanda
                        </Button>
                      )}
                      <Button
                        onClick={() => handleAbrirExcluir(venda)}
                        className="flex-1 min-w-[120px] bg-red-600 hover:bg-red-700 text-white border-transparent"
                        variant="outline"
                        title="Excluir esta comanda"
                      >
                        <Trash2 className="w-4 h-4 mr-2" />
                        Excluir
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </motion.div>
      </motion.div>
    </AnimatePresence>

    {/* Modal Verificação de lançamentos */}
    {verificacaoVenda && (
      <div className="fixed inset-0 z-[110] flex items-center justify-center p-4 bg-black/60" onClick={() => setVerificacaoVenda(null)}>
        <div className="bg-card rounded-2xl shadow-xl max-w-4xl w-full max-h-[90vh] flex flex-col" onClick={e => e.stopPropagation()}>
          <div className="p-4 border-b border-border flex items-center justify-between">
            <h3 className="text-lg font-semibold text-text-primary">
              Verificação de lançamentos — Nota {formatarNota(verificacaoVenda.nota)}
              {verificacaoVenda.nomeCliente && ` · ${verificacaoVenda.nomeCliente}`}
            </h3>
            <button onClick={() => setVerificacaoVenda(null)} className="p-2 rounded-lg hover:bg-muted transition-colors">
              <X className="w-5 h-5" />
            </button>
          </div>
          <div className="p-4 overflow-auto flex-1">
            {loadingVerificacao ? (
              <div className="flex flex-col items-center justify-center py-12">
                <Loader2 className="w-10 h-10 text-primary animate-spin" />
                <p className="mt-2 text-text-secondary">Carregando itens...</p>
              </div>
            ) : verificacaoItens.length === 0 ? (
              <p className="text-text-secondary text-center py-8">Nenhum item lançado nesta comanda.</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm border-collapse">
                  <thead>
                    <tr className="border-b border-border bg-muted/50">
                      <th className="text-left p-2 font-semibold text-text-primary">#</th>
                      <th className="text-left p-2 font-semibold text-text-primary">Produto</th>
                      <th className="text-left p-2 font-semibold text-text-primary">Data</th>
                      <th className="text-left p-2 font-semibold text-text-primary">Hora</th>
                      <th className="text-right p-2 font-semibold text-text-primary">Qtd</th>
                      <th className="text-right p-2 font-semibold text-text-primary">Valor unit.</th>
                      <th className="text-right p-2 font-semibold text-text-primary">Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {verificacaoItens.map((item, idx) => {
                      const { data, hora } = formatarDataHora(item.emissao)
                      return (
                        <tr key={`${item.nota}-${item.item}-${idx}`} className="border-b border-border/50 hover:bg-muted/30">
                          <td className="p-2 text-text-secondary">{item.item}</td>
                          <td className="p-2 text-text-primary">{item.descricao || `Produto ${item.codigo}`}</td>
                          <td className="p-2 text-text-secondary">{data}</td>
                          <td className="p-2 text-text-secondary">{hora}</td>
                          <td className="p-2 text-right text-text-primary">{Number(item.qtd).toLocaleString('pt-BR')}</td>
                          <td className="p-2 text-right text-text-primary">R$ {Number(item.preco).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                          <td className="p-2 text-right font-medium text-primary">R$ {Number(item.total).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
          {verificacaoItens.length > 0 && (
            <div className="p-4 border-t border-border flex justify-end">
              <p className="text-sm text-text-secondary">
                Total da comanda: <span className="font-bold text-primary">R$ {formatarPreco(verificacaoVenda.total)}</span>
              </p>
            </div>
          )}
        </div>
      </div>
    )}

    <ConectarWhatsAppModal
      isOpen={showConectarWhatsAppModal}
      onClose={() => setShowConectarWhatsAppModal(false)}
    />

    {/* Modal Excluir Comanda */}
    {showExcluirModal && vendaParaExcluir && (
      <div
        className="fixed inset-0 z-[200] flex items-center justify-center bg-black/60 p-4"
        onClick={() => { if (!excluindo) { setShowExcluirModal(false); setVendaParaExcluir(null) } }}
      >
        <div className="bg-card border border-border rounded-2xl shadow-xl max-w-sm w-full p-6 space-y-4" onClick={e => e.stopPropagation()}>
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-full bg-red-100 flex items-center justify-center flex-shrink-0">
              <Trash2 className="w-5 h-5 text-red-600" />
            </div>
            <h3 className="text-lg font-semibold text-text-primary">Cancelar comanda</h3>
          </div>
          <p className="text-sm text-text-secondary">
            Comanda <strong>#{String(vendaParaExcluir.comanda || '').padStart(3, '0')}</strong> (nota {vendaParaExcluir.nota}).
            O cancelamento ficará registrado no histórico. Preencha os campos abaixo.
          </p>
          <form onSubmit={(e) => { e.preventDefault(); handleConfirmarExcluir(); }}>
          <div className="space-y-1">
            <label className="text-xs font-medium text-text-secondary">Justificativa <span className="text-red-500">*</span></label>
            <textarea
              value={justificativaExcluir}
              onChange={e => setJustificativaExcluir(e.target.value)}
              placeholder="Motivo do cancelamento"
              rows={3}
              className="w-full px-4 py-3 bg-background border border-border rounded-xl text-text-primary placeholder-text-muted resize-none"
              autoFocus
              disabled={excluindo}
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-medium text-text-secondary">Senha de cancelamento <span className="text-red-500">*</span></label>
            <input
              type="password"
              value={senhaExcluir}
              onChange={e => { setSenhaExcluir(e.target.value); setErroExcluir('') }}
              placeholder="Senha configurada nos parâmetros"
              className={`w-full px-4 py-3 bg-background border rounded-xl text-text-primary placeholder-text-muted ${
                erroExcluir ? 'border-red-500' : 'border-border'
              }`}
              disabled={excluindo}
            />
            <p className={`text-xs text-red-500 mt-1 font-medium ${erroExcluir ? '' : 'hidden'}`}>{erroExcluir}</p>
          </div>
          <div className="flex gap-3 mt-4">
            <button
              type="button"
              onClick={() => { setShowExcluirModal(false); setVendaParaExcluir(null); setSenhaExcluir(''); setJustificativaExcluir(''); setErroExcluir('') }}
              disabled={excluindo}
              className="flex-1 py-2.5 rounded-xl border border-border bg-muted text-text-primary font-medium hover:bg-muted/80"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={excluindo || !senhaExcluir.trim() || !justificativaExcluir.trim()}
              className="flex-1 py-2.5 rounded-xl bg-red-600 text-white font-medium hover:bg-red-700 disabled:opacity-50 flex items-center justify-center gap-2"
            >
              {excluindo ? <Loader2 className="w-4 h-4 animate-spin" /> : <Trash2 className="w-4 h-4" />}
              Confirmar
            </button>
          </div>
          </form>
        </div>
      </div>
    )}
  </>
  )
}

export default ComandasAbertasModal

