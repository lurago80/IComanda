import { AnimatePresence, motion } from 'framer-motion';
import {
  CheckCircle,
  Loader2,
  X,
  Search,
  Plus,
  Trash2,
  Calculator,
  CreditCard,
  DollarSign,
  Receipt,
  Users,
  UserCheck
} from 'lucide-react';
import React, { useEffect, useState } from 'react';
import { useToast } from '../hooks/useToast';
import { recebimentosService, vendasService, clientesService, receberService } from '../services/api';
import { Button } from '../components/ui/button';
import { Venda } from '../types/api';

interface RecebimentoPageProps {
  onClose: () => void;
  /** Nota pré-selecionada (Caixa Rápido / PDV) – carrega a venda ao abrir */
  notaPreSelecionada?: string;
}

interface FormaPagamento {
  id: number;
  descricao: string;
  ativo: boolean;
  permiteTroco: boolean;
  tipo?: string;
}

interface RecebimentoItem {
  idFormaPagamento: number;
  formaPagamento: FormaPagamento;
  valor: number;
  valorRecebido: number;
  troco: number;
}

interface ComandaAberta {
  nota: string;
  comanda?: number;
  mesa?: number;
  cliente?: number;
  nomeCliente?: string;
  total: number;
  lancado: string;
  emissao: string;
  /** BA = comanda, DL = delivery */
  origem?: string;
  /** Preenchido quando o cliente possui valor em aberto (contas a receber) */
  contasAberto?: { temContasAberto: boolean; valorTotalPendente: number; quantidadeContas: number; mensagem: string };
}

const RecebimentoPage: React.FC<RecebimentoPageProps> = ({ onClose, notaPreSelecionada }) => {
  const [comanda, setComanda] = useState<string>('');
  const [comandaData, setComandaData] = useState<Venda | null>(null);
  const [formasPagamento, setFormasPagamento] = useState<FormaPagamento[]>([]);
  const [recebimentos, setRecebimentos] = useState<RecebimentoItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [buscando, setBuscando] = useState(false);
  const [processando, setProcessando] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);
  const [showImprimirReciboModal, setShowImprimirReciboModal] = useState(false);
  const [showReciboParaImpressao, setShowReciboParaImpressao] = useState(false);
  const [imprimindoRecibo, setImprimindoRecibo] = useState(false);
  const [resultadoFechamento, setResultadoFechamento] = useState<{
    nota: string;
    total: number;
    totalRecebido: number;
    troco: number;
    recebimentos: Array<{ formaPagamentoDescricao: string; valor: number; troco: number }>;
  } | null>(null);
  const [comandasAbertas, setComandasAbertas] = useState<ComandaAberta[]>([]);
  const [carregandoComandas, setCarregandoComandas] = useState(!notaPreSelecionada);
  /** null = não perguntado, true = sim (baixar), false = não */
  const [baixarValorEmAberto, setBaixarValorEmAberto] = useState<boolean | null>(null);
  const [contasEmAbertoLista, setContasEmAbertoLista] = useState<Array<{ numero: string; ordem: string; valorPendente: number; vencimento?: string }>>([]);
  const [carregandoContasAberto, setCarregandoContasAberto] = useState(false);
  const [showDivisaoModal, setShowDivisaoModal] = useState(false);
  const [numeroPessoas, setNumeroPessoas] = useState(2);
  const [modoDivisao, setModoDivisao] = useState(false);
  const { showSuccess: showSuccessToast, showError } = useToast();

  useEffect(() => {
    carregarFormasPagamento();
    // PDV (Caixa Rápido): não carregar lista de comandas abertas — venda já vem pré-selecionada
    if (!notaPreSelecionada) {
      carregarComandasAbertas();
    }
  }, []);

  // Ao trocar de comanda, resetar decisão e lista de contas em aberto
  useEffect(() => {
    setBaixarValorEmAberto(null);
    setContasEmAbertoLista([]);
    setModoDivisao(false);
    setNumeroPessoas(2);
  }, [comandaData?.nota, comandaData?.comanda]);

  // Caixa Rápido (PDV): carregar venda pela nota ao abrir
  useEffect(() => {
    if (!notaPreSelecionada || !notaPreSelecionada.trim()) return;
    const nota = notaPreSelecionada.trim();
    setComanda(nota);
    setComandaData(null);
    setRecebimentos([]);
    const carregar = async () => {
      setBuscando(true);
      try {
        const venda = await vendasService.getByNota(nota);
        if (venda && (venda.lancado === 'ABERTO' || venda.lancado === 'Aberto')) {
          setComandaData(venda);
          showSuccessToast('Sucesso', 'Venda carregada. Informe as formas de pagamento.');
        } else {
          showError('Erro', 'Venda não encontrada ou já foi fechada.');
        }
      } catch (e: any) {
        showError('Erro', e.response?.data?.mensagem || 'Não foi possível carregar a venda.');
      } finally {
        setBuscando(false);
      }
    };
    carregar();
  }, [notaPreSelecionada]);


  const carregarFormasPagamento = async () => {
    try {
      const formas = await recebimentosService.getFormasPagamento();
      setFormasPagamento(formas.filter((f: FormaPagamento) => f.ativo));
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar as formas de pagamento');
      console.error('Erro ao carregar formas de pagamento:', error);
    }
  };

  const getComandaNum = (v: { comanda?: number | string | null }): number | null => {
    const raw = (v as Record<string, unknown>).comanda ?? (v as Record<string, unknown>).Comanda;
    if (raw == null) return null;
    const n = typeof raw === 'number' ? raw : Number(raw);
    return Number.isNaN(n) ? null : n;
  };

  const carregarComandasAbertas = async () => {
    try {
      setCarregandoComandas(true);
      // Buscar comandas (BA) e delivery (DL) abertos para listar no recebimento
      const [vendasBA, vendasDL] = await Promise.all([
        vendasService.getAbertas('BA'),
        vendasService.getAbertas('DL').catch(() => [] as any[])
      ]);
      const vendas = [...vendasBA, ...vendasDL];

      // Incluir todas as vendas abertas: com número de comanda válido (agrupar por comanda) ou por nota (comanda 0/ausente)
      const comandasUnicas = new Map<number, ComandaAberta>();
      const porNota = new Map<string, ComandaAberta>();
      for (const venda of vendas) {
        const comandaNum = getComandaNum(venda);
        const origem = (venda as { origem?: string }).origem ?? 'BA';
        const item: ComandaAberta = {
          nota: venda.nota,
          comanda: comandaNum ?? undefined,
          mesa: venda.mesa,
          cliente: venda.cliente,
          nomeCliente: venda.nomeCliente,
          total: venda.total || 0,
          lancado: venda.lancado || 'ABERTO',
          emissao: venda.emissao || '',
          origem
        };
        if (comandaNum != null && comandaNum > 0) {
          const existente = comandasUnicas.get(comandaNum);
          if (existente) existente.total += item.total;
          else comandasUnicas.set(comandaNum, { ...item });
        } else {
          porNota.set(venda.nota, item);
        }
      }
      const comandasArray = [...Array.from(comandasUnicas.values()), ...Array.from(porNota.values())];
      
      // Buscar nomes dos clientes
      const clientesIds = comandasArray
        .map(c => c.cliente)
        .filter((id): id is number => id !== undefined && id > 0)
        .filter((id, index, self) => self.indexOf(id) === index); // Remover duplicatas
      
      const clientesMap = new Map<number, string>();
      const clienteResultados = await Promise.allSettled(
        clientesIds.map(id => clientesService.getById(id))
      );
      clienteResultados.forEach((resultado, i) => {
        if (resultado.status === 'fulfilled' && resultado.value?.nome) {
          clientesMap.set(clientesIds[i], resultado.value.nome);
        }
      });
      
      // Preencher nomes dos clientes
      comandasArray.forEach(comanda => {
        if (comanda.cliente && clientesMap.has(comanda.cliente)) {
          comanda.nomeCliente = clientesMap.get(comanda.cliente);
        }
      });

      // Contas em aberto são carregadas sob demanda ao selecionar a comanda (evita flood de requests)
      
      setComandasAbertas(comandasArray.sort((a, b) => {
        const nomeA = (a.nomeCliente || '').trim();
        const nomeB = (b.nomeCliente || '').trim();
        if (!nomeA && !nomeB) return (a.comanda || 0) - (b.comanda || 0);
        if (!nomeA) return 1;
        if (!nomeB) return -1;
        return nomeA.localeCompare(nomeB, 'pt-BR');
      }));
    } catch (error: any) {
      console.error('Erro ao carregar comandas abertas:', error);
      setComandasAbertas([]);
      showError('Erro', 'Não foi possível carregar as comandas abertas. Tente novamente.');
    } finally {
      setCarregandoComandas(false);
    }
  };

  const selecionarComandaOuNota = async (comandaAberta: ComandaAberta) => {
    setComanda(comandaAberta.comanda != null ? String(comandaAberta.comanda) : comandaAberta.nota);
    setComandaData(null);
    setRecebimentos([]);
    setBuscando(true);
    try {
      if (comandaAberta.comanda != null && comandaAberta.comanda > 0) {
        const vendas = await vendasService.getByComanda(comandaAberta.comanda);
        const vendaAberta = vendas.find(v => v.lancado === 'ABERTO' || v.lancado === 'SAINDO');
        if (vendaAberta) {
          const vendaDetalhada = await vendasService.getByNota(vendaAberta.nota);
          if (vendaDetalhada) {
            // Verificar contas em aberto do cliente on-demand (não na lista)
            if (comandaAberta.cliente && comandaAberta.cliente > 0) {
              try {
                const contas = await receberService.verificarContasAberto(comandaAberta.cliente);
                if (contas.temContasAberto) vendaDetalhada.contasAberto = contas;
              } catch { /* ignorar */ }
            }
            setComandaData(vendaDetalhada);
            showSuccessToast('Sucesso', 'Comanda selecionada!');
          } else {
            showError('Aviso', 'Não foi possível carregar os detalhes da comanda.');
          }
        } else {
          showError('Aviso', 'Esta comanda não possui vendas em aberto.');
        }
      } else {
        const vendaDetalhada = await vendasService.getByNota(comandaAberta.nota);
        const statusAberto = vendaDetalhada && (
          vendaDetalhada.lancado === 'ABERTO' || 
          vendaDetalhada.lancado === 'Aberto' || 
          vendaDetalhada.lancado === 'SAINDO'
        );
        if (statusAberto) {
          // Verificar contas em aberto do cliente on-demand
          if (comandaAberta.cliente && comandaAberta.cliente > 0) {
            try {
              const contas = await receberService.verificarContasAberto(comandaAberta.cliente);
              if (contas.temContasAberto) vendaDetalhada!.contasAberto = contas;
            } catch { /* ignorar */ }
          }
          setComandaData(vendaDetalhada);
          showSuccessToast('Sucesso', 'Comanda selecionada!');
        } else {
          showError('Aviso', 'Venda não encontrada ou já foi fechada.');
        }
      }
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar os detalhes da comanda');
    } finally {
      setBuscando(false);
    }
  };

  const selecionarComanda = async (comandaNum: number) => {
    selecionarComandaOuNota({ nota: '', comanda: comandaNum, total: 0, lancado: 'ABERTO', emissao: '' });
  };

  const buscarComanda = async () => {
    if (!comanda || comanda.trim() === '') {
      showError('Erro', 'Informe o número da comanda');
      return;
    }

    setBuscando(true);
    try {
      const comandaNum = parseInt(comanda);
      
      // Verificar se a comanda está aberta
      const estaAberta = await vendasService.verificarComandaAberta(comandaNum);
      if (!estaAberta) {
        showError('Erro', 'Esta comanda não possui vendas em aberto');
        setComandaData(null);
        return;
      }

      // Buscar vendas da comanda
      const vendas = await vendasService.getByComanda(comandaNum);
      
      if (!vendas || vendas.length === 0) {
        showError('Erro', 'Comanda não encontrada');
        setComandaData(null);
        return;
      }

      // Buscar a venda aberta
      const vendaAberta = vendas.find(v => v.lancado === 'ABERTO' || v.lancado === 'SAINDO');
      if (!vendaAberta) {
        showError('Erro', 'Esta comanda não possui vendas em aberto');
        setComandaData(null);
        return;
      }

      // Buscar detalhes completos da venda (com itens)
      const vendaDetalhada = await vendasService.getByNota(vendaAberta.nota);
      if (!vendaDetalhada) {
        showError('Erro', 'Não foi possível carregar os detalhes da comanda');
        setComandaData(null);
        return;
      }

      setComandaData(vendaDetalhada);
      setRecebimentos([]);
      showSuccessToast('Sucesso', 'Comanda encontrada!');
    } catch (error: any) {
      console.error('Erro ao buscar comanda:', error);
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível buscar a comanda');
      setComandaData(null);
    } finally {
      setBuscando(false);
    }
  };

  const dividirConta = () => {
    if (!comandaData || numeroPessoas < 2 || formasPagamento.length === 0) return;
    const total = comandaData.total || 0;
    const valorPorPessoa = Math.floor((total / numeroPessoas) * 100) / 100;
    const ultimoValor = Math.round((total - valorPorPessoa * (numeroPessoas - 1)) * 100) / 100;
    const primeiraForma = formasPagamento[0];

    const novasEntradas: RecebimentoItem[] = Array.from({ length: numeroPessoas }, (_, i) => ({
      idFormaPagamento: primeiraForma.id,
      formaPagamento: primeiraForma,
      valor: i === numeroPessoas - 1 ? ultimoValor : valorPorPessoa,
      valorRecebido: 0,
      troco: 0,
    }));

    setRecebimentos(novasEntradas);
    setModoDivisao(true);
    setShowDivisaoModal(false);
  };

  const adicionarRecebimento = () => {
    if (formasPagamento.length === 0) {
      showError('Erro', 'Nenhuma forma de pagamento disponível');
      return;
    }

    if (!comandaData) {
      showError('Erro', 'Selecione uma comanda primeiro');
      return;
    }

    const primeiraForma = formasPagamento[0];
    const restante = calcularRestante();
    const novoRecebimento: RecebimentoItem = {
      idFormaPagamento: primeiraForma.id,
      formaPagamento: primeiraForma,
      valor: restante > 0 ? restante : 0,
      valorRecebido: 0,
      troco: 0
    };

    setRecebimentos([...recebimentos, novoRecebimento]);
  };

  const removerRecebimento = (index: number) => {
    setRecebimentos(recebimentos.filter((_, i) => i !== index));
  };

  const atualizarRecebimento = (index: number, campo: keyof RecebimentoItem, valor: any) => {
    const novosRecebimentos = [...recebimentos];
    const recebimento = novosRecebimentos[index];

    if (campo === 'idFormaPagamento') {
      const forma = formasPagamento.find(f => f.id === valor);
      if (forma) {
        recebimento.idFormaPagamento = forma.id;
        recebimento.formaPagamento = forma;
        // Se a forma não permite troco, zerar o troco
        if (!forma.permiteTroco) {
          recebimento.troco = 0;
        }
        // Recalcular valor baseado no restante
        const restante = calcularRestante();
        if (restante > 0 && recebimento.valor === 0) {
          recebimento.valor = restante;
        }
      }
    } else if (campo === 'valorRecebido') {
      recebimento.valorRecebido = parseFloat(valor) || 0;
      // Calcular troco se a forma permite
      if (recebimento.formaPagamento.permiteTroco && recebimento.valorRecebido > recebimento.valor) {
        recebimento.troco = recebimento.valorRecebido - recebimento.valor;
      } else {
        recebimento.troco = 0;
      }
    } else if (campo === 'valor') {
      const novoValor = parseFloat(valor) || 0;
      const restante = calcularRestante();
      const maxValor = restante + recebimento.valor;
      recebimento.valor = Math.min(novoValor, maxValor);
      // Recalcular troco
      if (recebimento.formaPagamento.permiteTroco && recebimento.valorRecebido > recebimento.valor) {
        recebimento.troco = recebimento.valorRecebido - recebimento.valor;
      } else {
        recebimento.troco = 0;
      }
    } else {
      (recebimento as any)[campo] = valor;
    }

    setRecebimentos(novosRecebimentos);
  };

  const calcularTotalRecebimentos = () => {
    return recebimentos.reduce((acc, r) => acc + r.valor, 0);
  };

  const calcularTotalTroco = () => {
    return recebimentos.reduce((acc, r) => acc + r.troco, 0);
  };

  const calcularRestante = () => {
    if (!comandaData) return 0;
    const total = comandaData.total || 0;
    const totalRecebido = calcularTotalRecebimentos();
    return Math.max(0, total - totalRecebido);
  };

  const validarFechamento = (): boolean => {
    if (!comandaData) {
      showError('Erro', 'Nenhuma comanda selecionada');
      return false;
    }

    if (recebimentos.length === 0) {
      showError('Erro', 'Adicione pelo menos uma forma de pagamento');
      return false;
    }

    const total = comandaData.total || 0;
    const totalRecebido = calcularTotalRecebimentos();

    if (totalRecebido < total) {
      showError('Erro', `Valor recebido (${formatarMoeda(totalRecebido)}) é menor que o total (${formatarMoeda(total)})`);
      return false;
    }

    // Validar se todos os recebimentos têm valor
    for (const recebimento of recebimentos) {
      if (recebimento.valor <= 0) {
        showError('Erro', 'Todos os recebimentos devem ter valor maior que zero');
        return false;
      }
    }

    return true;
  };

  const fecharComanda = async () => {
    // Remover automaticamente formas de pagamento com valor zero antes de validar
    const recebimentosValidos = recebimentos.filter(r => r.valor > 0);
    if (recebimentosValidos.length !== recebimentos.length) {
      setRecebimentos(recebimentosValidos);
      // Reagendar após atualização do state
      setTimeout(fecharComanda, 0);
      return;
    }
    if (!validarFechamento()) return;

    setProcessando(true);
    try {
      const isPdV = !!notaPreSelecionada || (comandaData && (comandaData.comanda == null || comandaData.comanda === 0));
      const request: {
        comanda?: number;
        nota?: string;
        recebimentos: Array<{ idFormaPagamento: number; valor: number; troco?: number }>;
        troco?: number;
      } = {
        recebimentos: recebimentos.map(r => ({
          idFormaPagamento: r.idFormaPagamento,
          valor: r.valorRecebido > 0 ? r.valorRecebido : r.valor,
          troco: r.troco
        })),
        troco: calcularTotalTroco()
      };
      if (isPdV && comandaData?.nota) {
        request.nota = comandaData.nota;
        request.comanda = 0;
      } else {
        request.comanda = parseInt(comanda, 10) || 0;
      }

      const resultado = await recebimentosService.fecharComanda(request);
      
      setResultadoFechamento({
        nota: resultado.nota,
        total: resultado.total,
        totalRecebido: resultado.totalRecebido,
        troco: resultado.troco,
        recebimentos: resultado.recebimentos || []
      });
      setShowSuccess(true);
      setShowImprimirReciboModal(true);
      const isDelivery = String(comandaData?.origem || '').toUpperCase() === 'DL';
      showSuccessToast('Sucesso', isDelivery ? `Pedido delivery fechado! Nota: ${resultado.nota}` : `Comanda ${comanda} fechada com sucesso! Nota: ${resultado.nota}`);
      
      // Verificar se há contas em aberto e exibir aviso (não no modo PDV/Caixa Rápido)
      if (!notaPreSelecionada && resultado.contasAberto?.temContasAberto) {
        setTimeout(() => {
          showError('Atenção', resultado.contasAberto.mensagem);
        }, 1000);
      }
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível fechar a comanda');
    } finally {
      setProcessando(false);
    }
  };

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  };

  /** Formata data (e hora, se vier separada da API) para exibição em pt-BR */
  const formatarData = (emissao: string, hora?: string) => {
    if (!emissao) return '';
    const dataPart = emissao.includes('T') ? emissao.split('T')[0] : emissao.split(' ')[0];
    const horaPart = hora?.trim();
    if (horaPart) {
      const horaLimpa = horaPart.split('.')[0];
      return new Date(`${dataPart}T${horaLimpa}`).toLocaleString('pt-BR');
    }
    return new Date(emissao).toLocaleString('pt-BR');
  };

  const fecharSemImprimir = () => {
    setShowImprimirReciboModal(false);
    setShowSuccess(false);
    setComandaData(null);
    setComanda('');
    setRecebimentos([]);
    setResultadoFechamento(null);
    if (notaPreSelecionada) {
      onClose();
    } else {
      carregarComandasAbertas();
    }
  };

  const abrirReciboParaImpressao = () => {
    setShowImprimirReciboModal(false);
    setShowReciboParaImpressao(true);
  };

  const fecharReciboEVoltar = () => {
    setShowReciboParaImpressao(false);
    setShowSuccess(false);
    setComandaData(null);
    setComanda('');
    setRecebimentos([]);
    setResultadoFechamento(null);
    if (notaPreSelecionada) {
      onClose();
    } else {
      carregarComandasAbertas();
    }
  };

  const imprimirRecibo = async () => {
    if (!comandaData || !resultadoFechamento) return;
    setImprimindoRecibo(true);
    try {
      const itens = (comandaData.itens || []).map((item) => ({
        codigo: item.codigo,
        descricao: item.descricao || `Item ${item.item}`,
        quantidade: item.qtd,
        preco: item.preco,
        observacao: undefined as string | undefined
      }));
      await vendasService.imprimir(resultadoFechamento.nota, {
        itens,
        apenasNovosItens: false,
        comanda: comanda,
        clienteNome: comandaData.nomeCliente || (comandaData.cliente ? `Cód. ${comandaData.cliente}` : undefined),
        isReciboCliente: true,
        formasPagamento: (resultadoFechamento.recebimentos || []).map((r) => ({
          descricao: r.formaPagamentoDescricao,
          valor: r.valor
        })),
        trocoTotal: resultadoFechamento.troco > 0 ? resultadoFechamento.troco : undefined
      });
      showSuccessToast('Impressão', 'Recibo enviado para a impressora padrão.');
      fecharReciboEVoltar();
    } catch (error: any) {
      showError(
        'Impressão',
        error.response?.data?.mensagem || 'Não foi possível imprimir. Verifique a impressora em Configurações.'
      );
    } finally {
      setImprimindoRecibo(false);
    }
  };

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <Receipt className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Recebimento</h1>
              <p className="text-sm text-text-muted">Fechar comanda e processar pagamentos</p>
            </div>
          </div>
          <Button
            onClick={onClose}
            variant="outline"
            className="flex items-center space-x-2"
          >
            <X className="w-4 h-4" />
            <span className="hidden sm:inline">Fechar</span>
          </Button>
        </div>

        {/* Lista de Comandas Abertas — ocultar no modo PDV (nota pré-selecionada) */}
        {!comandaData && !notaPreSelecionada && (
          <>
            <div className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-bold text-text-primary flex items-center">
                  <Receipt className="w-5 h-5 mr-2 text-primary" />
                  Comandas em Aberto
                </h2>
                <Button
                  onClick={carregarComandasAbertas}
                  variant="outline"
                  size="sm"
                  disabled={carregandoComandas}
                >
                  {carregandoComandas ? (
                    <Loader2 className="w-4 h-4 animate-spin" />
                  ) : (
                    <Search className="w-4 h-4" />
                  )}
                </Button>
              </div>

              {carregandoComandas ? (
                <div className="text-center py-8">
                  <Loader2 className="w-8 h-8 text-primary mx-auto mb-2 animate-spin" />
                  <p className="text-text-muted">Carregando comandas...</p>
                </div>
              ) : comandasAbertas.length === 0 ? (
                <div className="text-center py-8 text-text-muted">
                  <Receipt className="w-12 h-12 mx-auto mb-2 opacity-50" />
                  <p>Nenhuma comanda em aberto</p>
                </div>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {comandasAbertas.map((comandaAberta, index) => {
                    const isDelivery = String(comandaAberta.origem || '').toUpperCase() === 'DL';
                    return (
                    <div
                      key={`${comandaAberta.nota}-${index}`}
                      className={`${
                        isDelivery 
                          ? 'bg-orange-50 border-2 border-orange-300' 
                          : 'bg-background-secondary border border-border'
                      } rounded-xl p-4 hover:shadow-lg transition-shadow cursor-pointer`}
                      onClick={() => selecionarComandaOuNota(comandaAberta)}
                    >
                      <div className="flex items-start justify-between mb-2">
                        <div className="flex-1">
                          <h3 className="font-bold text-lg text-text-primary">
                            {comandaAberta.comanda != null && comandaAberta.comanda > 0
                              ? `Comanda ${comandaAberta.comanda}`
                              : `Nota ${String(comandaAberta.nota || '').trim() || '—'}`}
                            {String(comandaAberta.origem || '').toUpperCase() === 'DL' && (
                              <span className="ml-2 text-xs font-semibold bg-orange-100 text-orange-800 px-2 py-0.5 rounded">Delivery</span>
                            )}
                          </h3>
                          {comandaAberta.nomeCliente && (
                            <p className="text-sm font-medium text-text-primary mt-1">
                              {comandaAberta.nomeCliente}
                            </p>
                          )}
                          {comandaAberta.mesa && (
                            <p className="text-sm text-text-secondary">
                              Mesa: {comandaAberta.mesa}
                            </p>
                          )}
                        </div>
                        <div className="flex items-center gap-2 flex-shrink-0 ml-2">
                          {comandaAberta.contasAberto?.temContasAberto && comandaAberta.contasAberto.valorTotalPendente > 0 && (
                            <span className="text-red-600 font-bold text-sm" title={`Cliente deve: R$ ${comandaAberta.contasAberto.valorTotalPendente.toFixed(2).replace('.', ',')}`}>
                              ⚠️ Deve
                            </span>
                          )}
                          <div className="w-3 h-3 bg-green-500 rounded-full"></div>
                        </div>
                      </div>
                      <div className="mt-3 pt-3 border-t border-border">
                        <div className="flex justify-between items-center">
                          <span className="text-sm text-text-secondary">Total</span>
                          <span className="text-lg font-bold text-primary">
                            {formatarMoeda(comandaAberta.total)}
                          </span>
                        </div>
                      </div>
                    </div>
                  )})}
                </div>
              )}
            </div>

            {/* Busca Manual */}
            <div className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border">
              <div className="flex flex-col sm:flex-row gap-4">
                <div className="flex-1">
                  <label className="block text-sm font-medium text-text-secondary mb-2">
                    Ou busque por número da comanda
                  </label>
                  <input
                    type="number"
                    value={comanda}
                    onChange={(e) => setComanda(e.target.value)}
                    onKeyPress={(e) => e.key === 'Enter' && buscarComanda()}
                    className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                              focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                              text-text-primary"
                    placeholder="Digite o número da comanda"
                    disabled={buscando || processando}
                  />
                </div>
                <div className="flex items-end">
                  <Button
                    onClick={buscarComanda}
                    disabled={buscando || processando || !comanda}
                    className="w-full sm:w-auto px-6 py-3"
                  >
                    {buscando ? (
                      <>
                        <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                        Buscando...
                      </>
                    ) : (
                      <>
                        <Search className="w-4 h-4 mr-2" />
                        Buscar
                      </>
                    )}
                  </Button>
                </div>
              </div>
            </div>
          </>
        )}

        {/* Dados da Comanda */}
        {comandaData && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border"
          >
            <h2 className="text-xl font-bold text-text-primary mb-4 flex items-center">
              <CreditCard className="w-5 h-5 mr-2 text-primary" />
              {String(comandaData.origem || '').toUpperCase() === 'DL' ? `Pedido delivery — Nota ${comandaData.nota || comanda}` : `Comanda #${comanda}`}
            </h2>

            {!notaPreSelecionada && comandaData.contasAberto?.temContasAberto && comandaData.contasAberto.valorTotalPendente > 0 && (
              <div className="mb-4 p-4 rounded-xl bg-red-50 border-2 border-red-300 space-y-3">
                <p className="text-base font-bold text-red-700">Cliente com valor em aberto</p>
                <p className="text-sm text-red-600">
                  Este cliente possui {comandaData.contasAberto.quantidadeContas} conta(s) a receber em aberto: {formatarMoeda(comandaData.contasAberto.valorTotalPendente)}
                </p>
                {baixarValorEmAberto === null && (
                  <div className="flex flex-wrap items-center gap-2 pt-2">
                    <span className="text-sm font-medium text-red-700">Deseja baixar o valor em aberto?</span>
                    <div className="flex gap-2">
                      <button
                        type="button"
                        onClick={async () => {
                          setBaixarValorEmAberto(true);
                          setCarregandoContasAberto(true);
                          try {
                            const codigo = Number(comandaData.cliente);
                            if (Number.isNaN(codigo)) {
                              setContasEmAbertoLista([]);
                              return;
                            }
                            const lista = await receberService.listarPorCliente(codigo, true);
                            setContasEmAbertoLista((lista || []).map((c: any) => ({
                              numero: c.numero ?? '',
                              ordem: c.ordem ?? '',
                              valorPendente: Number(c.valorPendente ?? c.valor ?? 0),
                              vencimento: c.vencimento,
                            })));
                          } catch (e) {
                            showError('Erro', 'Não foi possível carregar as contas em aberto.');
                            setContasEmAbertoLista([]);
                          } finally {
                            setCarregandoContasAberto(false);
                          }
                        }}
                        className="px-4 py-2 rounded-lg bg-green-600 text-white font-medium hover:bg-green-700 transition"
                      >
                        Sim
                      </button>
                      <button
                        type="button"
                        onClick={() => setBaixarValorEmAberto(false)}
                        className="px-4 py-2 rounded-lg bg-gray-600 text-white font-medium hover:bg-gray-700 transition"
                      >
                        Não
                      </button>
                    </div>
                  </div>
                )}
                {baixarValorEmAberto === true && (
                  <div className="pt-2 border-t border-red-200">
                    {carregandoContasAberto ? (
                      <p className="text-sm text-red-600">Carregando contas em aberto...</p>
                    ) : contasEmAbertoLista.length > 0 ? (
                      <div>
                        <p className="text-sm font-semibold text-red-700 mb-2">Contas em aberto:</p>
                        <ul className="space-y-1 max-h-40 overflow-y-auto text-sm">
                          {contasEmAbertoLista.map((c, i) => (
                            <li key={`${c.numero}-${c.ordem}-${i}`} className="flex justify-between items-center py-1">
                              <span>Nº {c.numero} / Ordem {c.ordem}{c.vencimento ? ` · Venc. ${c.vencimento}` : ''}</span>
                              <span className="font-medium">{formatarMoeda(c.valorPendente)}</span>
                            </li>
                          ))}
                        </ul>
                        <p className="text-sm font-bold text-red-700 mt-2">
                          Total em aberto: {formatarMoeda(contasEmAbertoLista.reduce((s, c) => s + c.valorPendente, 0))}
                        </p>
                        <p className="text-xs text-red-600 mt-1">
                          Para quitar, use o menu Receber (Contas a receber) após fechar esta comanda.
                        </p>
                      </div>
                    ) : (
                      <p className="text-sm text-red-600">Nenhuma conta pendente encontrada.</p>
                    )}
                  </div>
                )}
              </div>
            )}
            
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-4">
              <div>
                <p className="text-sm text-text-secondary">Nota</p>
                <p className="font-semibold text-text-primary">{comandaData.nota}</p>
              </div>
              <div>
                <p className="text-sm text-text-secondary">Data</p>
                <p className="font-semibold text-text-primary">
                  {formatarData(comandaData.emissao, comandaData.hora)}
                </p>
              </div>
              <div>
                <p className="text-sm text-text-secondary">Cliente</p>
                <p className="font-semibold text-text-primary">
                  {comandaData.cliente || 'Não informado'}
                </p>
              </div>
            </div>

            {/* Itens da Comanda */}
            {comandaData.itens && comandaData.itens.length > 0 && (
              <div className="mb-4">
                <h3 className="text-sm font-semibold text-text-secondary mb-2">Itens</h3>
                <div className="space-y-2 max-h-48 overflow-y-auto">
                  {comandaData.itens.map((item, index) => (
                    <div
                      key={index}
                      className="flex justify-between items-center p-2 bg-background-secondary rounded-lg"
                    >
                      <div className="flex-1">
                        <p className="text-sm font-medium text-text-primary">
                          {item.descricao || `Item ${item.item}`}
                        </p>
                        <p className="text-xs text-text-muted">
                          {item.qtd}x {formatarMoeda(item.preco)}
                        </p>
                      </div>
                      <p className="font-semibold text-text-primary">
                        {formatarMoeda(item.total)}
                      </p>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* Total */}
            <div className="bg-primary/10 border-2 border-primary/30 rounded-xl p-4">
              <div className="flex justify-between items-center">
                <span className="text-lg font-semibold text-text-primary">Total</span>
                <span className="text-2xl font-bold text-primary">
                  {formatarMoeda(comandaData.total || 0)}
                </span>
              </div>
            </div>
          </motion.div>
        )}

        {/* Formas de Pagamento */}
        {comandaData && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border"
          >
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-xl font-bold text-text-primary flex items-center">
                <DollarSign className="w-5 h-5 mr-2 text-primary" />
                Formas de Pagamento
              </h2>
              <div className="flex items-center gap-2">
                <Button
                  onClick={() => setShowDivisaoModal(true)}
                  variant="outline"
                  size="sm"
                  className="flex items-center space-x-2 border-primary/40 text-primary hover:bg-primary/10"
                  disabled={processando}
                  title="Dividir a conta entre várias pessoas"
                >
                  <Users className="w-4 h-4" />
                  <span>Dividir</span>
                </Button>
                <Button
                  onClick={() => { setModoDivisao(false); adicionarRecebimento(); }}
                  variant="outline"
                  size="sm"
                  className="flex items-center space-x-2"
                  disabled={processando}
                >
                  <Plus className="w-4 h-4" />
                  <span>Adicionar</span>
                </Button>
              </div>
            </div>

            {/* Banner modo divisão */}
            {modoDivisao && (
              <div className="mb-4 p-3 bg-primary/10 border border-primary/30 rounded-xl flex items-center justify-between">
                <div className="flex items-center gap-2 text-sm text-primary font-medium">
                  <Users className="w-4 h-4" />
                  <span>
                    Conta dividida entre {recebimentos.length} pessoas &mdash; {formatarMoeda((comandaData?.total || 0) / recebimentos.length)} por pessoa
                  </span>
                </div>
                <button
                  onClick={() => { setModoDivisao(false); setRecebimentos([]); }}
                  className="text-xs text-text-muted hover:text-red-500 transition-colors"
                  disabled={processando}
                >
                  Cancelar
                </button>
              </div>
            )}

            {recebimentos.length === 0 ? (
              <div className="text-center py-8 text-text-muted">
                <p>Nenhuma forma de pagamento adicionada</p>
                <p className="text-sm mt-2">Clique em "Adicionar" para incluir uma forma de pagamento</p>
              </div>
            ) : (
              <div className="space-y-4">
                {recebimentos.map((recebimento, index) => {
                  const pessoaConfirmada = recebimento.valorRecebido >= recebimento.valor && recebimento.valor > 0;
                  return (
                  <div
                    key={index}
                    className={`border rounded-xl p-4 transition-colors ${
                      modoDivisao
                        ? pessoaConfirmada
                          ? 'bg-green-50 border-green-300'
                          : 'bg-background-secondary border-border'
                        : 'bg-background-secondary border-border'
                    }`}
                  >
                    {modoDivisao && (
                      <div className="flex items-center justify-between mb-3">
                        <div className="flex items-center gap-2">
                          {pessoaConfirmada ? (
                            <UserCheck className="w-4 h-4 text-green-600" />
                          ) : (
                            <Users className="w-4 h-4 text-primary" />
                          )}
                          <span className={`text-sm font-semibold ${pessoaConfirmada ? 'text-green-700' : 'text-text-primary'}`}>
                            Pessoa {index + 1} de {recebimentos.length}
                          </span>
                          {pessoaConfirmada && (
                            <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded-full font-medium">Pago</span>
                          )}
                        </div>
                      </div>
                    )}
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex-1">
                        <label className="block text-sm font-medium text-text-secondary mb-2">
                          Forma de Pagamento
                        </label>
                        <select
                          value={recebimento.idFormaPagamento}
                          onChange={(e) => atualizarRecebimento(index, 'idFormaPagamento', parseInt(e.target.value))}
                          className="w-full px-3 py-2 bg-background border border-border rounded-lg
                                    focus:outline-none focus:ring-2 focus:ring-primary/50
                                    text-text-primary"
                          disabled={processando}
                        >
                          {formasPagamento.map((forma) => (
                            <option key={forma.id} value={forma.id}>
                              {forma.descricao}
                            </option>
                          ))}
                        </select>
                      </div>
                      <button
                        onClick={() => removerRecebimento(index)}
                        className="ml-3 p-2 text-red-500 hover:bg-red-50 rounded-lg transition-colors"
                        disabled={processando}
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>

                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-text-secondary mb-2">
                          Valor Recebido
                        </label>
                        <input
                          type="number"
                          step="0.01"
                          value={recebimento.valorRecebido || ''}
                          onChange={(e) => atualizarRecebimento(index, 'valorRecebido', e.target.value)}
                          className="w-full px-3 py-2 bg-background border border-border rounded-lg
                                    focus:outline-none focus:ring-2 focus:ring-primary/50
                                    text-text-primary"
                          placeholder="0.00"
                          disabled={processando}
                        />
                      </div>

                      <div>
                        <label className="block text-sm font-medium text-text-secondary mb-2">
                          Valor a Receber
                        </label>
                        <div className="relative">
                          <input
                            type="number"
                            step="0.01"
                            value={recebimento.valor || ''}
                            onChange={(e) => {
                              const valor = parseFloat(e.target.value) || 0;
                              const restante = calcularRestante();
                              const maxValor = restante + (recebimentos[index]?.valor || 0);
                              const novoValor = Math.min(valor, maxValor);
                              atualizarRecebimento(index, 'valor', novoValor);
                            }}
                            className="w-full px-3 py-2 bg-background border border-border rounded-lg
                                      focus:outline-none focus:ring-2 focus:ring-primary/50
                                      text-text-primary"
                            placeholder="0.00"
                            disabled={processando}
                          />
                          <button
                            onClick={() => {
                              const restante = calcularRestante();
                              const maxValor = restante + (recebimentos[index]?.valor || 0);
                              atualizarRecebimento(index, 'valor', maxValor);
                            }}
                            className="absolute right-2 top-1/2 transform -translate-y-1/2
                                      px-2 py-1 text-xs bg-primary/10 text-primary rounded
                                      hover:bg-primary/20 transition-colors"
                            disabled={processando}
                          >
                            <Calculator className="w-3 h-3" />
                          </button>
                        </div>
                      </div>
                    </div>

                    {recebimento.formaPagamento.permiteTroco && recebimento.troco > 0 && (
                      <div className="mt-3 p-2 bg-yellow-50 border border-yellow-200 rounded-lg">
                        <p className="text-sm text-yellow-800">
                          <strong>Troco:</strong> {formatarMoeda(recebimento.troco)}
                        </p>
                      </div>
                    )}
                  </div>
                  );
                })}
              </div>
            )}

            {/* Resumo */}
            {recebimentos.length > 0 && (
              <div className="mt-6 space-y-2 p-4 bg-background-secondary rounded-xl">
                {modoDivisao && (
                  <div className="flex justify-between text-sm mb-2 pb-2 border-b border-border">
                    <span className="text-text-secondary font-medium">Progresso:</span>
                    <span className="font-semibold text-primary">
                      {recebimentos.filter(r => r.valorRecebido >= r.valor && r.valor > 0).length} de {recebimentos.length} pessoas pagaram
                    </span>
                  </div>
                )}
                <div className="flex justify-between text-sm">
                  <span className="text-text-secondary">Total Recebido:</span>
                  <span className="font-semibold text-text-primary">
                    {formatarMoeda(calcularTotalRecebimentos())}
                  </span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-text-secondary">{String(comandaData.origem || '').toUpperCase() === 'DL' ? 'Total do pedido:' : 'Total da Comanda:'}</span>
                  <span className="font-semibold text-text-primary">
                    {formatarMoeda(comandaData?.total || 0)}
                  </span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-text-secondary">Restante:</span>
                  <span className={`font-semibold ${calcularRestante() > 0 ? 'text-red-500' : 'text-green-500'}`}>
                    {formatarMoeda(calcularRestante())}
                  </span>
                </div>
                {calcularTotalTroco() > 0 && (
                  <div className="flex justify-between text-sm pt-2 border-t border-border">
                    <span className="text-text-secondary">Troco Total:</span>
                    <span className="font-semibold text-yellow-600">
                      {formatarMoeda(calcularTotalTroco())}
                    </span>
                  </div>
                )}
              </div>
            )}
          </motion.div>
        )}

        {/* Botão Fechar Comanda */}
        {comandaData && recebimentos.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="flex justify-end"
          >
            <Button
              onClick={fecharComanda}
              disabled={processando || calcularRestante() > 0}
              className="px-8 py-4 text-lg font-semibold"
              size="lg"
            >
              {processando ? (
                <>
                  <Loader2 className="w-5 h-5 mr-2 animate-spin" />
                  Processando...
                </>
              ) : (
                <>
                  <CheckCircle className="w-5 h-5 mr-2" />
                  {String(comandaData?.origem || '').toUpperCase() === 'DL' ? 'Fechar pedido' : 'Fechar Comanda'}
                </>
              )}
            </Button>
          </motion.div>
        )}

        {/* Modal: Deseja imprimir o recibo? (após fechar comanda) */}
        <AnimatePresence>
          {showImprimirReciboModal && (
            <motion.div
              initial={{ opacity: 0, scale: 0.9 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.9 }}
              className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm"
            >
              <motion.div
                initial={{ y: 20 }}
                animate={{ y: 0 }}
                exit={{ y: 20 }}
                className="bg-card rounded-2xl p-8 max-w-md mx-4 shadow-2xl border border-border"
              >
                <div className="text-center">
                  <CheckCircle className="w-16 h-16 text-green-500 mx-auto mb-4" />
                  <h2 className="text-2xl font-bold text-text-primary mb-2">
                    Comanda Fechada!
                  </h2>
                  <p className="text-text-secondary mb-6">
                    A comanda foi fechada com sucesso.
                  </p>
                  <p className="text-text-primary font-medium mb-6">
                    Deseja imprimir o recibo do cliente?
                  </p>
                  <div className="flex gap-3 justify-center">
                    <Button
                      onClick={abrirReciboParaImpressao}
                      className="flex-1 sm:flex-none"
                    >
                      Sim
                    </Button>
                    <Button
                      onClick={fecharSemImprimir}
                      variant="outline"
                      className="flex-1 sm:flex-none"
                    >
                      Não
                    </Button>
                  </div>
                </div>
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Modal: Recibo na tela para impressão */}
        <AnimatePresence>
          {showReciboParaImpressao && comandaData && resultadoFechamento && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 overflow-y-auto"
            >
              <motion.div
                initial={{ scale: 0.95 }}
                animate={{ scale: 1 }}
                exit={{ scale: 0.95 }}
                className="bg-white rounded-xl shadow-2xl max-w-lg w-full my-8 print:shadow-none print:max-w-none"
              >
                {/* Área imprimível - esconder botões na impressão */}
                <div className="p-6 print:p-4" id="recibo-impressao">
                  <div className="text-center border-b border-gray-300 pb-4 mb-4">
                    <h1 className="text-xl font-bold text-gray-900">RECIBO DO CLIENTE</h1>
                    <p className="text-sm text-gray-600 mt-1">Comanda #{comanda} · Nota {resultadoFechamento.nota}</p>
                    <p className="text-sm text-gray-600">{formatarData(comandaData.emissao || '', comandaData.hora)}</p>
                  </div>
                  {(comandaData.nomeCliente || comandaData.cliente) && (
                    <div className="mb-3">
                      <span className="text-gray-600">Cliente: </span>
                      <span className="font-medium text-gray-900">
                        {comandaData.nomeCliente || (comandaData.cliente ? `Cód. ${comandaData.cliente}` : '')}
                      </span>
                    </div>
                  )}
                  <div className="mb-4">
                    <h2 className="text-sm font-semibold text-gray-700 mb-2">Itens</h2>
                    <ul className="space-y-2">
                      {comandaData.itens?.map((item, index) => (
                        <li key={index} className="flex justify-between text-sm">
                          <span className="text-gray-800">
                            {item.qtd}x {item.descricao || `Item ${item.item}`}
                          </span>
                          <span className="font-medium">{formatarMoeda(item.total)}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                  <div className="border-t border-gray-300 pt-3 mb-4">
                    <div className="flex justify-between font-semibold text-gray-900">
                      <span>Total</span>
                      <span>{formatarMoeda(comandaData.total || 0)}</span>
                    </div>
                  </div>
                  <div className="mb-4">
                    <h2 className="text-sm font-semibold text-gray-700 mb-2">Formas de pagamento</h2>
                    <ul className="space-y-1">
                      {resultadoFechamento.recebimentos.map((r, i) => (
                        <li key={i} className="flex justify-between text-sm">
                          <span className="text-gray-800">{r.formaPagamentoDescricao}</span>
                          <span>{formatarMoeda(r.valor)}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                  {resultadoFechamento.troco > 0 && (
                    <div className="flex justify-between font-semibold text-gray-900 border-t border-gray-300 pt-3">
                      <span>Troco</span>
                      <span>{formatarMoeda(resultadoFechamento.troco)}</span>
                    </div>
                  )}
                </div>
                {/* Botões */}
                <div className="p-4 pt-0 flex gap-3 justify-end border-t border-gray-200">
                  <Button onClick={imprimirRecibo} disabled={imprimindoRecibo}>
                    {imprimindoRecibo ? (
                      <>
                        <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                        Enviando...
                      </>
                    ) : (
                      'Imprimir'
                    )}
                  </Button>
                  <Button variant="outline" onClick={fecharReciboEVoltar} disabled={imprimindoRecibo}>
                    Fechar
                  </Button>
                </div>
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Modal: Dividir Conta */}
        <AnimatePresence>
          {showDivisaoModal && (
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm"
              onClick={() => setShowDivisaoModal(false)}
            >
              <motion.div
                initial={{ opacity: 0, scale: 0.9, y: 20 }}
                animate={{ opacity: 1, scale: 1, y: 0 }}
                exit={{ opacity: 0, scale: 0.9, y: 20 }}
                className="bg-card rounded-2xl p-8 max-w-sm w-full mx-4 shadow-2xl border border-border"
                onClick={(e) => e.stopPropagation()}
              >
                <div className="text-center mb-6">
                  <div className="w-14 h-14 bg-primary/20 rounded-full flex items-center justify-center mx-auto mb-3">
                    <Users className="w-7 h-7 text-primary" />
                  </div>
                  <h2 className="text-xl font-bold text-text-primary">Dividir Conta</h2>
                  <p className="text-sm text-text-muted mt-1">
                    Total: <span className="font-semibold text-primary">{formatarMoeda(comandaData?.total || 0)}</span>
                  </p>
                </div>

                <div className="mb-6">
                  <label className="block text-sm font-medium text-text-secondary mb-2 text-center">
                    Quantas pessoas vão dividir?
                  </label>
                  <div className="flex items-center justify-center gap-4">
                    <button
                      onClick={() => setNumeroPessoas(Math.max(2, numeroPessoas - 1))}
                      className="w-10 h-10 rounded-full bg-background-secondary border border-border text-text-primary text-xl font-bold hover:bg-primary/10 hover:border-primary transition-colors"
                    >
                      −
                    </button>
                    <input
                      type="number"
                      min={2}
                      max={20}
                      value={numeroPessoas}
                      onChange={(e) => setNumeroPessoas(Math.max(2, Math.min(20, parseInt(e.target.value) || 2)))}
                      className="w-20 text-center text-3xl font-bold bg-transparent text-text-primary focus:outline-none"
                    />
                    <button
                      onClick={() => setNumeroPessoas(Math.min(20, numeroPessoas + 1))}
                      className="w-10 h-10 rounded-full bg-background-secondary border border-border text-text-primary text-xl font-bold hover:bg-primary/10 hover:border-primary transition-colors"
                    >
                      +
                    </button>
                  </div>
                  <p className="text-center text-sm text-text-muted mt-3">
                    Cada pessoa paga:{' '}
                    <span className="font-bold text-primary text-base">
                      {formatarMoeda(Math.floor(((comandaData?.total || 0) / numeroPessoas) * 100) / 100)}
                    </span>
                  </p>
                </div>

                <div className="flex gap-3">
                  <Button
                    onClick={dividirConta}
                    className="flex-1"
                    disabled={formasPagamento.length === 0}
                  >
                    <Users className="w-4 h-4 mr-2" />
                    Dividir
                  </Button>
                  <Button
                    onClick={() => setShowDivisaoModal(false)}
                    variant="outline"
                    className="flex-1"
                  >
                    Cancelar
                  </Button>
                </div>
              </motion.div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
};

export default RecebimentoPage;
