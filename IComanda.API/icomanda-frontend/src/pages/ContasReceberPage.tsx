import React, { useState, useEffect, useRef } from 'react';
import { CreditCard, ArrowLeft, Search, CheckCircle, Calendar, DollarSign, Package, X, Loader2, Printer, MessageCircle } from 'lucide-react';
import { Button } from '../components/ui/button';
import { receberService, vendasService, clientesService } from '../services/api';
import { useToast } from '../hooks/useToast';
import { Cliente, ItemVenda, Venda } from '../types/api';
import { formatarMensagemContaReceber, formatarNumeroTelefone, enviarWhatsApp, abrirLinkWhatsApp } from '../utils/whatsapp';

interface ContasReceberPageProps {
  onClose: () => void;
  onReceber?: () => void;
}

interface ContaReceber {
  numero: string;
  ordem: string;
  codigo: number;
  nomeCliente?: string;
  telefoneCliente?: string;
  emissao: string;
  vencimento: string;
  valor: number;
  valorRecebido: number;
  valorPendente: number;
  isQuitado: boolean;
  recebimento?: string;
  notaFiscal?: string;
  controleNota?: string;
  diasVencidos?: number;
}

const ContasReceberPage: React.FC<ContasReceberPageProps> = ({ onClose, onReceber }) => {
  const [contas, setContas] = useState<ContaReceber[]>([]);
  const [loading, setLoading] = useState(true);
  const [showQuitarModal, setShowQuitarModal] = useState(false);
  const [showProdutosModal, setShowProdutosModal] = useState(false);
  const [contaSelecionada, setContaSelecionada] = useState<ContaReceber | null>(null);
  const [produtosVenda, setProdutosVenda] = useState<ItemVenda[]>([]);
  const [carregandoProdutos, setCarregandoProdutos] = useState(false);
  const [imprimindo, setImprimindo] = useState(false);
  const [vendaSelecionada, setVendaSelecionada] = useState<Venda | null>(null);
  const [nomeClienteFiltro, setNomeClienteFiltro] = useState<string>('');
  const [clienteSelecionadoId, setClienteSelecionadoId] = useState<number | null>(null);
  const [sugestoesCliente, setSugestoesCliente] = useState<Cliente[]>([]);
  const [showSugestoes, setShowSugestoes] = useState(false);
  const [buscandoCliente, setBuscandoCliente] = useState(false);
  const [dataVencimentoInicio, setDataVencimentoInicio] = useState<string>('');
  const [dataVencimentoFim, setDataVencimentoFim] = useState<string>('');
  const [apenasPendentes, setApenasPendentes] = useState<boolean>(true);
  const [enviandoWhatsApp, setEnviandoWhatsApp] = useState<string | null>(null);
  const [nomeEstabelecimento, setNomeEstabelecimento] = useState<string>('');
  const filtroClienteRef = useRef<HTMLDivElement>(null);

  // Formulário de quitação
  const [valorRecebido, setValorRecebido] = useState<string>('0.00');
  const [dataRecebimento, setDataRecebimento] = useState<string>(
    new Date().toISOString().split('T')[0]
  );
  const [desconto, setDesconto] = useState<string>('0.00');
  const [juros, setJuros] = useState<string>('0.00');

  const { showError, showSuccess } = useToast();

  useEffect(() => {
    vendasService.getEmitente().then((e) => {
      const nome = e?.nomeFantasia?.trim() || (e as any)?.nome?.trim() || ''
      if (nome) setNomeEstabelecimento(nome)
    }).catch(() => {})
  }, []);

  // Fechar dropdown ao clicar fora
  useEffect(() => {
    const handleClickFora = (e: MouseEvent) => {
      if (filtroClienteRef.current && !filtroClienteRef.current.contains(e.target as Node)) {
        setShowSugestoes(false);
      }
    };
    document.addEventListener('mousedown', handleClickFora);
    return () => document.removeEventListener('mousedown', handleClickFora);
  }, []);

  // Buscar sugestões de cliente conforme digita
  useEffect(() => {
    if (!nomeClienteFiltro.trim() || clienteSelecionadoId !== null) {
      setSugestoesCliente([]);
      setShowSugestoes(false);
      return;
    }
    if (nomeClienteFiltro.trim().length < 2) return;
    const timer = setTimeout(async () => {
      try {
        setBuscandoCliente(true);
        const resultado = await clientesService.buscar({ q: nomeClienteFiltro.trim(), itensPorPagina: 8 });
        setSugestoesCliente(resultado);
        setShowSugestoes(resultado.length > 0);
      } catch {
        setSugestoesCliente([]);
      } finally {
        setBuscandoCliente(false);
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [nomeClienteFiltro, clienteSelecionadoId]);

  useEffect(() => {
    carregarContas();
  }, [apenasPendentes]);

  const carregarContas = async () => {
    try {
      setLoading(true);
      let dados: ContaReceber[] = [];

      if (clienteSelecionadoId !== null) {
        dados = await receberService.listarPorCliente(
          clienteSelecionadoId,
          apenasPendentes
        );
      } else {
        dados = await receberService.listarPendentes(
          undefined,
          dataVencimentoInicio || undefined,
          dataVencimentoFim || undefined
        );
      }

      // Normalizar campos que podem vir em PascalCase do backend
      const dadosNormalizados = dados.map((conta: any) => {
        const venc = conta.vencimento ? new Date(conta.vencimento) : new Date();
        const quitada = conta.valorRecebido >= (conta.valor ?? 0) || conta.EstaQuitado === true;
        const diasVencidos = quitada ? 0 : Math.max(0, Math.floor((Date.now() - venc.getTime()) / (1000 * 60 * 60 * 24)));
        const contaNormalizada = {
          ...conta,
          notaFiscal: conta.notaFiscal || conta.NotaFiscal || null,
          controleNota: conta.controleNota || conta.ControleNota || null,
          isQuitado: conta.isQuitado !== undefined ? conta.isQuitado : (conta.EstaQuitado !== undefined ? conta.EstaQuitado : false),
          telefoneCliente: conta.telefoneCliente ?? conta.TelefoneCliente ?? undefined,
          diasVencidos
        };
        
        // Log para debug
        if (contaNormalizada.notaFiscal || contaNormalizada.controleNota) {
          console.log(`[ContasReceberPage] Conta ${conta.numero}/${conta.ordem} - NotaFiscal: ${contaNormalizada.notaFiscal}, ControleNota: ${contaNormalizada.controleNota}`);
        }
        
        return contaNormalizada;
      });

      const dadosOrdenados = dadosNormalizados.sort((a: ContaReceber, b: ContaReceber) => {
        const nomeA = (a.nomeCliente || '').trim();
        const nomeB = (b.nomeCliente || '').trim();
        if (!nomeA && !nomeB) return 0;
        if (!nomeA) return 1;
        if (!nomeB) return -1;
        return nomeA.localeCompare(nomeB, 'pt-BR');
      });
      setContas(dadosOrdenados);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar as contas a receber');
      console.error('Erro ao carregar contas:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleVisualizarProdutos = async (conta: ContaReceber) => {
    console.log('[ContasReceberPage] Visualizando produtos para conta:', conta);
    console.log('[ContasReceberPage] notaFiscal:', conta.notaFiscal);
    console.log('[ContasReceberPage] controleNota:', conta.controleNota);
    console.log('[ContasReceberPage] numero:', conta.numero);
    
    // Usar o número da conta como nota (formato: numero/ordem -> usar apenas o numero)
    // Ou usar notaFiscal/controleNota se disponível
    let nota = conta.notaFiscal || conta.controleNota;
    
    // Se não tiver notaFiscal/controleNota, usar o número da conta
    if (!nota || nota === '0' || nota.trim() === '') {
      // Extrair apenas o número (antes da barra)
      nota = conta.numero.split('/')[0].trim();
    }
    
    // Remover zeros à esquerda da nota (normalizar)
    const notaOriginal = nota;
    const notaSemZeros = nota.replace(/^0+/, '') || '0';
    const notaComZeros = nota.padStart(6, '0');
    
    console.log('[ContasReceberPage] Nota original:', notaOriginal);
    console.log('[ContasReceberPage] Nota sem zeros:', notaSemZeros);
    console.log('[ContasReceberPage] Nota com zeros (6 dígitos):', notaComZeros);
    
    if (!nota || nota === '0' || nota.trim() === '') {
      showError('Aviso', 'Não foi possível identificar a nota da venda');
      return;
    }

    setContaSelecionada(conta);
    setCarregandoProdutos(true);
    setShowProdutosModal(true);
    setProdutosVenda([]);
    setVendaSelecionada(null);

    // Lista de notas para tentar (em ordem de prioridade)
    const notasParaTentar = [
      notaOriginal,      // Nota original (como veio)
      notaSemZeros,      // Sem zeros à esquerda
      notaComZeros,      // Com zeros (6 dígitos)
    ].filter((n, index, self) => n && n !== '0' && self.indexOf(n) === index); // Remover duplicatas e zeros

    console.log('[ContasReceberPage] Tentando buscar com notas:', notasParaTentar);

    let itensEncontrados: ItemVenda[] | null = null;
    let notaUsada: string | null = null;

    // PRIMEIRO: Tentar buscar na tabela frente_tmpitvendas (itens temporários)
    for (const notaTentativa of notasParaTentar) {
      try {
        console.log(`[ContasReceberPage] Tentando buscar itens TEMPORÁRIOS para cupom: "${notaTentativa}"`);
        const itensTemp = await vendasService.getItensTemporariosByCupom(notaTentativa);
        
        if (itensTemp && itensTemp.length > 0) {
          console.log(`[ContasReceberPage] ✅ Itens TEMPORÁRIOS encontrados com cupom "${notaTentativa}":`, itensTemp.length);
          itensEncontrados = itensTemp;
          notaUsada = notaTentativa;
          break; // Encontrou, parar de tentar
        } else {
          console.log(`[ContasReceberPage] ⚠️ Nenhum item temporário encontrado com cupom "${notaTentativa}"`);
        }
      } catch (error: any) {
        console.log(`[ContasReceberPage] ❌ Erro ao buscar itens temporários com cupom "${notaTentativa}":`, error.response?.status, error.message);
        // Continuar tentando outras variações
      }
    }

    // SEGUNDO: Se não encontrou temporários, tentar buscar na tabela itevendas (itens finais)
    if (!itensEncontrados || itensEncontrados.length === 0) {
      console.log('[ContasReceberPage] Nenhum item temporário encontrado, tentando buscar itens FINAIS...');
      for (const notaTentativa of notasParaTentar) {
        try {
          console.log(`[ContasReceberPage] Tentando buscar itens FINAIS para nota: "${notaTentativa}"`);
          const itens = await vendasService.getItensByNota(notaTentativa);
          
          if (itens && itens.length > 0) {
            console.log(`[ContasReceberPage] ✅ Itens FINAIS encontrados com nota "${notaTentativa}":`, itens.length);
            itensEncontrados = itens;
            notaUsada = notaTentativa;
            break; // Encontrou, parar de tentar
          } else {
            console.log(`[ContasReceberPage] ⚠️ Nenhum item final encontrado com nota "${notaTentativa}"`);
          }
        } catch (error: any) {
          console.log(`[ContasReceberPage] ❌ Erro ao buscar itens finais com nota "${notaTentativa}":`, error.response?.status, error.message);
          // Continuar tentando outras variações
        }
      }
    }

    try {
      if (itensEncontrados && itensEncontrados.length > 0) {
        setProdutosVenda(itensEncontrados);
        // Calcular totais para exibir no modal
        const totalItens = itensEncontrados.reduce((sum, item) => sum + (item.total || 0), 0);
        setVendaSelecionada({
          nota: notaUsada || notaOriginal,
          total: totalItens,
          desconto: 0,
          acrescimo: 0,
          itens: itensEncontrados
        } as any);
      } else {
        console.error('[ContasReceberPage] ❌ Nenhum produto encontrado após tentar todas as variações de nota');
        showError('Aviso', `Não foram encontrados produtos para esta conta. Nota tentada: ${notasParaTentar.join(', ')}`);
      }
    } catch (error: any) {
      console.error('[ContasReceberPage] Erro ao processar produtos:', error);
      showError('Erro', 'Não foi possível carregar os produtos da venda');
    } finally {
      setCarregandoProdutos(false);
    }
  };

  const handleImprimir = async () => {
    if (!contaSelecionada || !vendaSelecionada || produtosVenda.length === 0) {
      showError('Aviso', 'Não há produtos para imprimir');
      return;
    }

    setImprimindo(true);
    try {
      const nota = contaSelecionada.notaFiscal || contaSelecionada.controleNota || contaSelecionada.numero.split('/')[0].trim();
      
      await vendasService.imprimir(nota, {
        itens: produtosVenda.map(item => ({
          codigo: item.codigo || 0,
          descricao: item.descricao || `Produto ${item.codigo}`,
          quantidade: item.qtd || 0,
          preco: item.preco || 0,
          observacao: undefined
        })),
        apenasNovosItens: false,
        clienteNome: contaSelecionada.nomeCliente || undefined,
        subtotal: vendaSelecionada.total || 0,
        desconto: vendaSelecionada.desconto || 0,
        acrescimo: vendaSelecionada.acrescimo || 0,
        isExtrato: true // Marcar como extrato para impressão de conta
      });

      showSuccess('Sucesso', 'Impressão enviada com sucesso!');
    } catch (error: any) {
      console.error('[ContasReceberPage] Erro ao imprimir:', error);
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível imprimir. Verifique a configuração da impressora.');
    } finally {
      setImprimindo(false);
    }
  };

  const handleQuitar = async () => {
    if (!contaSelecionada) return;

    try {
      await receberService.quitar({
        numero: contaSelecionada.numero,
        ordem: contaSelecionada.ordem,
        valorRecebido: parseFloat(valorRecebido.replace(',', '.')),
        dataRecebimento: dataRecebimento || undefined,
        desconto: parseFloat(desconto.replace(',', '.')),
        juros: parseFloat(juros.replace(',', '.'))
      });

      showSuccess('Sucesso', 'Conta quitada com sucesso!');
      setShowQuitarModal(false);
      setContaSelecionada(null);
      setValorRecebido('0.00');
      setDesconto('0.00');
      setJuros('0.00');
      carregarContas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível quitar a conta');
    }
  };

  const handleEnviarLembreteWhatsApp = async (conta: ContaReceber) => {
    if (!conta.telefoneCliente || !formatarNumeroTelefone(conta.telefoneCliente)) {
      showError('Aviso', 'Cliente não possui telefone cadastrado para envio de lembrete.');
      return;
    }
    const contaKey = `${conta.numero}/${conta.ordem}`;
    if (enviandoWhatsApp === contaKey) return;
    setEnviandoWhatsApp(contaKey);
    try {
      // Garantir que temos o nome fantasia antes de montar a mensagem
      let nomeParaMensagem = nomeEstabelecimento;
      if (!nomeParaMensagem) {
        try {
          const emitente = await vendasService.getEmitente();
          nomeParaMensagem = emitente?.nomeFantasia?.trim() || emitente?.nome?.trim() || '';
          if (nomeParaMensagem) setNomeEstabelecimento(nomeParaMensagem);
        } catch {}
      }
      const mensagem = formatarMensagemContaReceber({
        numero: conta.numero,
        ordem: conta.ordem,
        vencimento: conta.vencimento,
        valorPendente: conta.valorPendente,
        diasVencidos: conta.diasVencidos ?? 0,
        nomeCliente: conta.nomeCliente,
        nomeEstabelecimento: nomeParaMensagem
      });
      const resultado = await enviarWhatsApp(conta.telefoneCliente, mensagem);
      if (resultado.method === 'sent') {
        showSuccess('Lembrete enviado', 'Lembrete enviado com sucesso por WhatsApp!');
      } else if (resultado.method === 'link' && resultado.link) {
        abrirLinkWhatsApp(resultado.link);
        showSuccess('WhatsApp aberto', 'Envie o lembrete na janela que abriu.');
      } else {
        showError('Aviso', 'Não foi possível enviar. Abra o WhatsApp manualmente ou configure o envio automático.');
      }
    } catch (error: any) {
      showError('Erro', error?.message || 'Não foi possível enviar o lembrete.');
    } finally {
      setEnviandoWhatsApp(null);
    }
  };

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  };

  const formatarData = (data: string) => {
    return new Date(data).toLocaleDateString('pt-BR');
  };

  const isVencida = (dataVencimento: string) => {
    return new Date(dataVencimento) < new Date();
  };

  const totalPendente = contas
    .filter((c) => !c.isQuitado)
    .reduce((acc, c) => acc + c.valorPendente, 0);

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <CreditCard className="w-8 h-8 text-primary" />
            <h1 className="text-3xl font-bold">Contas a Receber</h1>
          </div>
          <div className="flex items-center space-x-2">
            {onReceber && (
              <Button onClick={onReceber} className="bg-primary">
                <DollarSign className="w-4 h-4 mr-2" />
                Receber Contas
              </Button>
            )}
            <Button onClick={onClose} variant="outline">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Voltar
            </Button>
          </div>
        </div>

        {/* Filtros */}
        <div className="bg-card rounded-lg p-4 mb-6 shadow-lg">
          <form onSubmit={(e) => { e.preventDefault(); carregarContas(); }}>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div ref={filtroClienteRef} className="relative">
              <label className="block text-sm font-medium mb-2">Nome do Cliente (opcional)</label>
              <div className="relative">
                <input
                  type="text"
                  value={nomeClienteFiltro}
                  onChange={(e) => {
                    setNomeClienteFiltro(e.target.value);
                    setClienteSelecionadoId(null);
                  }}
                  onFocus={() => { if (sugestoesCliente.length > 0) setShowSugestoes(true); }}
                  className="w-full px-3 py-2 pr-8 border rounded-lg"
                  placeholder="Digite o nome do cliente..."
                />
                {buscandoCliente && (
                  <Loader2 className="absolute right-2 top-1/2 -translate-y-1/2 w-4 h-4 animate-spin text-gray-400" />
                )}
                {nomeClienteFiltro && !buscandoCliente && (
                  <button
                    onClick={() => { setNomeClienteFiltro(''); setClienteSelecionadoId(null); setSugestoesCliente([]); setShowSugestoes(false); }}
                    className="absolute right-2 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    <X className="w-4 h-4" />
                  </button>
                )}
              </div>
              {showSugestoes && sugestoesCliente.length > 0 && (
                <ul className="absolute z-50 w-full bg-white border border-gray-200 rounded-lg shadow-lg mt-1 max-h-48 overflow-y-auto">
                  {sugestoesCliente.map((c) => (
                    <li
                      key={c.id}
                      className="px-3 py-2 hover:bg-amber-50 cursor-pointer text-sm"
                      onMouseDown={() => {
                        setNomeClienteFiltro(c.nomeCompleto || c.nome || '');
                        setClienteSelecionadoId(c.id);
                        setShowSugestoes(false);
                      }}
                    >
                      <span className="font-medium">{c.nomeCompleto || c.nome}</span>
                      {c.documento && <span className="text-gray-400 ml-2 text-xs">{c.documento}</span>}
                    </li>
                  ))}
                </ul>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Vencimento Início</label>
              <input
                type="date"
                value={dataVencimentoInicio}
                onChange={(e) => setDataVencimentoInicio(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Vencimento Fim</label>
              <input
                type="date"
                value={dataVencimentoFim}
                onChange={(e) => setDataVencimentoFim(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div className="flex items-end space-x-2">
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={apenasPendentes}
                  onChange={(e) => setApenasPendentes(e.target.checked)}
                  className="rounded"
                />
                <span className="text-sm">Apenas Pendentes</span>
              </label>
              <Button type="submit" className="flex-1 bg-primary">
                <Search className="w-4 h-4 mr-2" />
                Buscar
              </Button>
            </div>
          </div>
          </form>
        </div>

        {/* Resumo */}
        {contas.length > 0 && (
          <div className="bg-card rounded-lg p-4 mb-6 shadow-lg">
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="bg-primary/10 p-4 rounded-lg">
                <p className="text-sm text-text-secondary">Total de Contas</p>
                <p className="text-2xl font-bold">{contas.length}</p>
              </div>
              <div className="bg-red-50 p-4 rounded-lg">
                <p className="text-sm text-text-secondary">Contas Pendentes</p>
                <p className="text-2xl font-bold text-red-600">
                  {contas.filter((c) => !c.isQuitado).length}
                </p>
              </div>
              <div className="bg-yellow-50 p-4 rounded-lg">
                <p className="text-sm text-text-secondary">Total Pendente</p>
                <p className="text-2xl font-bold text-yellow-600">{formatarMoeda(totalPendente)}</p>
              </div>
            </div>
          </div>
        )}

        {/* Lista de Contas */}
        {loading ? (
          <div className="text-center py-12">
            <p className="text-text-secondary">Carregando...</p>
          </div>
        ) : contas.length === 0 ? (
          <div className="bg-card rounded-lg p-12 text-center shadow-lg">
            <p className="text-text-secondary">Nenhuma conta encontrada</p>
          </div>
        ) : (
          <div className="space-y-4">
            {contas.map((conta) => (
              <div
                key={`${conta.numero}-${conta.ordem}`}
                className={`bg-card rounded-lg p-6 shadow-lg border-l-4 ${
                  conta.isQuitado
                    ? 'border-green-500'
                    : isVencida(conta.vencimento)
                    ? 'border-red-500'
                    : 'border-yellow-500'
                }`}
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-4">
                      <h3 className="text-xl font-bold">
                        Conta {conta.numero}/{conta.ordem}
                      </h3>
                      {conta.isQuitado ? (
                        <span className="px-3 py-1 bg-green-100 text-green-800 rounded-full text-sm font-semibold">
                          Quitada
                        </span>
                      ) : isVencida(conta.vencimento) ? (
                        <span className="px-3 py-1 bg-red-100 text-red-800 rounded-full text-sm font-semibold">
                          Vencida
                        </span>
                      ) : (
                        <span className="px-3 py-1 bg-yellow-100 text-yellow-800 rounded-full text-sm font-semibold">
                          Pendente
                        </span>
                      )}
                    </div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                      <div>
                        <p className="text-sm text-text-secondary">Cliente</p>
                        <p className="font-semibold">
                          {conta.nomeCliente || `Código ${conta.codigo}`}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm text-text-secondary">Emissão</p>
                        <p className="font-semibold">{formatarData(conta.emissao)}</p>
                      </div>
                      <div>
                        <p className="text-sm text-text-secondary">Vencimento</p>
                        <p
                          className={`font-semibold ${
                            isVencida(conta.vencimento) ? 'text-red-600' : ''
                          }`}
                        >
                          {formatarData(conta.vencimento)}
                        </p>
                      </div>
                      <div>
                        <p className="text-sm text-text-secondary">Valor</p>
                        <p className="font-semibold text-primary">{formatarMoeda(conta.valor)}</p>
                      </div>
                    </div>

                    {!conta.isQuitado && (
                      <div className="mt-4 pt-4 border-t grid grid-cols-2 md:grid-cols-3 gap-4">
                        <div>
                          <p className="text-sm text-text-secondary">Valor Recebido</p>
                          <p className="font-semibold">{formatarMoeda(conta.valorRecebido)}</p>
                        </div>
                        <div>
                          <p className="text-sm text-text-secondary">Valor Pendente</p>
                          <p className="font-semibold text-red-600">
                            {formatarMoeda(conta.valorPendente)}
                          </p>
                        </div>
                      </div>
                    )}

                    {conta.recebimento && (
                      <div className="mt-4 pt-4 border-t">
                        <p className="text-sm text-text-secondary">Data de Recebimento</p>
                        <p className="font-semibold">{formatarData(conta.recebimento)}</p>
                      </div>
                    )}
                  </div>

                  <div className="ml-4 flex flex-col gap-2">
                    <Button
                      onClick={() => handleVisualizarProdutos(conta)}
                      variant="outline"
                      className="flex items-center justify-center"
                    >
                      <Package className="w-4 h-4 mr-2" />
                      Produtos
                    </Button>
                    {conta.telefoneCliente && formatarNumeroTelefone(conta.telefoneCliente) && !conta.isQuitado && (
                      <Button
                        onClick={() => handleEnviarLembreteWhatsApp(conta)}
                        disabled={enviandoWhatsApp === `${conta.numero}/${conta.ordem}`}
                        variant="outline"
                        title="Enviar lembrete amigável por WhatsApp (valor em aberto)"
                        className="flex items-center justify-center gap-2 bg-green-50 hover:bg-green-100 text-green-700 border-green-300 disabled:opacity-50"
                      >
                        {enviandoWhatsApp === `${conta.numero}/${conta.ordem}` ? (
                          <>
                            <Loader2 className="w-4 h-4 animate-spin" />
                            <span>Enviando...</span>
                          </>
                        ) : (
                          <>
                            <MessageCircle className="w-4 h-4" />
                            <span>Lembrete</span>
                          </>
                        )}
                      </Button>
                    )}
                    {!conta.isQuitado && (
                      <Button
                        onClick={() => {
                          setContaSelecionada(conta);
                          setValorRecebido(conta.valorPendente.toFixed(2));
                          setShowQuitarModal(true);
                        }}
                        className="bg-green-600 hover:bg-green-700"
                      >
                        <CheckCircle className="w-4 h-4 mr-2" />
                        Quitar
                      </Button>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Modal Quitar */}
        {showQuitarModal && contaSelecionada && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-card rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
              <h2 className="text-2xl font-bold mb-4">
                Quitar Conta {contaSelecionada.numero}/{contaSelecionada.ordem}
              </h2>
              <form onSubmit={(e) => { e.preventDefault(); handleQuitar(); }}>
              <div className="space-y-4">
                <div className="bg-primary/10 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary mb-1">Valor Pendente</p>
                  <p className="text-2xl font-bold text-primary">
                    {formatarMoeda(contaSelecionada.valorPendente)}
                  </p>
                </div>

                <div>
                  <label className="block text-sm font-medium mb-2">Valor Recebido (R$)</label>
                  <input
                    type="text"
                    value={valorRecebido}
                    onChange={(e) => {
                      const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                      setValorRecebido(value);
                    }}
                    className="w-full px-3 py-2 border rounded-lg"
                    placeholder="0.00"
                  />
                </div>

                <div>
                  <label className="block text-sm font-medium mb-2">Data de Recebimento</label>
                  <input
                    type="date"
                    value={dataRecebimento}
                    onChange={(e) => setDataRecebimento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium mb-2">Desconto (R$)</label>
                    <input
                      type="text"
                      value={desconto}
                      onChange={(e) => {
                        const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                        setDesconto(value);
                      }}
                      className="w-full px-3 py-2 border rounded-lg"
                      placeholder="0.00"
                    />
                  </div>
                  <div>
                    <label className="block text-sm font-medium mb-2">Juros (R$)</label>
                    <input
                      type="text"
                      value={juros}
                      onChange={(e) => {
                        const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                        setJuros(value);
                      }}
                      className="w-full px-3 py-2 border rounded-lg"
                      placeholder="0.00"
                    />
                  </div>
                </div>
              </div>

              <div className="flex space-x-2 mt-6">
                <Button type="submit" className="flex-1 bg-green-600 hover:bg-green-700">
                  Quitar Conta
                </Button>
                <Button
                  type="button"
                  onClick={() => {
                    setShowQuitarModal(false);
                    setContaSelecionada(null);
                    setValorRecebido('0.00');
                    setDesconto('0.00');
                    setJuros('0.00');
                  }}
                  variant="outline"
                  className="flex-1"
                >
                  Cancelar
                </Button>
              </div>
              </form>
            </div>
          </div>
        )}

        {/* Modal de Produtos da Venda */}
        {showProdutosModal && contaSelecionada && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-card rounded-lg max-w-3xl w-full max-h-[92vh] overflow-hidden flex flex-col shadow-xl mx-4">

              {/* Header compacto */}
              <div className="flex items-center justify-between px-4 py-3 border-b border-border flex-shrink-0">
                <div className="flex items-center gap-2">
                  <Package className="w-4 h-4 text-primary" />
                  <span className="font-bold text-base">Produtos da Venda</span>
                </div>
                <div className="flex items-center gap-4 text-sm">
                  <span className="text-text-secondary">
                    Conta: <span className="font-semibold text-text-primary">{contaSelecionada.numero}/{contaSelecionada.ordem}</span>
                  </span>
                  {(contaSelecionada.notaFiscal || contaSelecionada.controleNota) && (
                    <span className="text-text-secondary">
                      NF: <span className="font-semibold text-text-primary">{contaSelecionada.notaFiscal || contaSelecionada.controleNota}</span>
                    </span>
                  )}
                  {vendaSelecionada && (
                    <span className="text-text-secondary">
                      Total: <span className="font-bold text-primary">{formatarMoeda(vendaSelecionada.total || 0)}</span>
                    </span>
                  )}
                  <button onClick={() => setShowProdutosModal(false)} className="text-text-muted hover:text-text-primary transition-colors ml-2">
                    <X className="w-5 h-5" />
                  </button>
                </div>
              </div>

              {/* Lista de produtos */}
              <div className="flex-1 overflow-y-auto">
                {carregandoProdutos ? (
                  <div className="flex flex-col items-center justify-center py-12 space-y-4">
                    <Loader2 size={48} className="text-primary animate-spin" />
                    <p className="text-lg text-text-secondary">Carregando produtos...</p>
                  </div>
                ) : produtosVenda.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-12 space-y-4">
                    <Package className="w-16 h-16 text-text-muted" />
                    <p className="text-lg font-semibold text-text-primary">Nenhum produto encontrado</p>
                    <p className="text-sm text-text-secondary">Esta venda não possui produtos cadastrados</p>
                  </div>
                ) : (
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="bg-primary/10 text-text-secondary">
                        <th className="text-left px-4 py-2 font-medium">#</th>
                        <th className="text-left px-4 py-2 font-medium">Produto</th>
                        <th className="text-center px-3 py-2 font-medium">Qtd</th>
                        <th className="text-right px-3 py-2 font-medium">Preço Unit.</th>
                        {produtosVenda.some(i => i.desconto > 0) && (
                          <th className="text-right px-3 py-2 font-medium">Desconto</th>
                        )}
                        <th className="text-right px-4 py-2 font-medium">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {produtosVenda.map((item, index) => (
                        <tr
                          key={`${item.nota}-${item.item}`}
                          className={`border-b border-border/50 hover:bg-primary/5 transition-colors ${index % 2 === 0 ? '' : 'bg-background-secondary/40'}`}
                        >
                          <td className="px-4 py-2 text-text-muted">{item.item}</td>
                          <td className="px-4 py-2">
                            <p className="font-medium text-text-primary">{item.descricao || `Produto ${item.codigo}`}</p>
                            <p className="text-xs text-text-muted">Cód: {item.codigo}</p>
                          </td>
                          <td className="px-3 py-2 text-center text-text-primary">{item.qtd?.toFixed(2) || '0.00'} {item.und || 'UN'}</td>
                          <td className="px-3 py-2 text-right text-text-primary">{formatarMoeda(item.preco || 0)}</td>
                          {produtosVenda.some(i => i.desconto > 0) && (
                            <td className="px-3 py-2 text-right text-red-500">{item.desconto > 0 ? `- ${formatarMoeda(item.desconto)}` : '—'}</td>
                          )}
                          <td className="px-4 py-2 text-right font-bold text-primary">{formatarMoeda(item.total || 0)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>

              {produtosVenda.length > 0 && (
                <div className="px-4 py-3 border-t border-border flex-shrink-0 bg-background-secondary/30">
                  <div className="flex justify-between items-center text-sm">
                    <span className="text-text-secondary">{produtosVenda.length} {produtosVenda.length === 1 ? 'item' : 'itens'}</span>
                  </div>
                  {vendaSelecionada && (
                    <>
                      {vendaSelecionada.desconto > 0 && (
                        <div className="flex justify-between items-center mt-1 text-sm">
                          <span className="text-text-secondary">Desconto:</span>
                          <span className="font-semibold text-red-500">
                            - {formatarMoeda(vendaSelecionada.desconto)}
                          </span>
                        </div>
                      )}
                      {vendaSelecionada.acrescimo > 0 && (
                        <div className="flex justify-between items-center mt-2">
                          <span className="text-sm text-text-secondary">Acréscimo:</span>
                          <span className="font-semibold text-green-500">
                            + {formatarMoeda(vendaSelecionada.acrescimo)}
                          </span>
                        </div>
                      )}
                      <div className="flex justify-between items-center mt-2 pt-2 border-t border-border">
                        <span className="font-bold text-text-primary">Total da Venda:</span>
                        <span className="text-base font-bold text-primary">
                          {formatarMoeda(vendaSelecionada.total || 0)}
                        </span>
                      </div>
                    </>
                  )}
                  <div className="mt-2 pt-2 border-t border-border">
                    <Button
                      onClick={handleImprimir}
                      disabled={imprimindo}
                      className="w-full bg-primary hover:bg-primary/90 flex items-center justify-center gap-2"
                    >
                      {imprimindo ? (
                        <>
                          <Loader2 className="w-4 h-4 animate-spin" />
                          Imprimindo...
                        </>
                      ) : (
                        <>
                          <Printer className="w-4 h-4" />
                          Imprimir
                        </>
                      )}
                    </Button>
                  </div>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ContasReceberPage;
