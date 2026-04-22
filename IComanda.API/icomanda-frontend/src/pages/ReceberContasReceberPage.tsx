import { AnimatePresence, motion } from 'framer-motion';
import { 
  CheckCircle, 
  Loader2, 
  X, 
  Search, 
  DollarSign,
  CreditCard,
  Calendar,
  User,
  AlertCircle,
  FileText,
  Package,
  MessageCircle
} from 'lucide-react';
import React, { useEffect, useState } from 'react';
import { useToast } from '../hooks/useToast';
import { receberService, recebimentosService, clientesService, vendasService } from '../services/api';
import { Button } from '../components/ui/button';
import ClienteSearch from '../components/ClienteSearch';
import { Cliente, Venda, ItemVenda } from '../types/api';
import { useDebounce } from '../hooks/useDebounce';
import ConectarWhatsAppModal from '../components/ConectarWhatsAppModal';
import { enviarWhatsApp, formatarMensagemContaReceber, formatarNumeroTelefone, abrirLinkWhatsApp } from '../utils/whatsapp';

interface ReceberContasReceberPageProps {
  onClose: () => void;
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
  diasVencidos: number;
  notaFiscal?: string;
  controleNota?: string;
}

interface FormaPagamento {
  id: number;
  descricao: string;
  ativo: boolean;
}

interface RecebimentoItem {
  idFormaPagamento: number;
  formaPagamento: FormaPagamento;
  valor: number;
}

const ReceberContasReceberPage: React.FC<ReceberContasReceberPageProps> = ({ onClose }) => {
  const [contas, setContas] = useState<ContaReceber[]>([]);
  const [loading, setLoading] = useState(false);
  const [showQuitarModal, setShowQuitarModal] = useState(false);
  const [showProdutosModal, setShowProdutosModal] = useState(false);
  const [contaSelecionada, setContaSelecionada] = useState<ContaReceber | null>(null);
  const [produtosVenda, setProdutosVenda] = useState<ItemVenda[]>([]);
  const [carregandoProdutos, setCarregandoProdutos] = useState(false);
  const [vendaSelecionada, setVendaSelecionada] = useState<Venda | null>(null);
  const [buscaCliente, setBuscaCliente] = useState<string>('');
  const [codigoCliente, setCodigoCliente] = useState<string>('');
  const [nomeCliente, setNomeCliente] = useState<string>('');
  const [clienteSelecionado, setClienteSelecionado] = useState<Cliente | null>(null);
  const [showClienteSearch, setShowClienteSearch] = useState(false);
  const [buscandoCliente, setBuscandoCliente] = useState(false);
  
  const debouncedBuscaCliente = useDebounce(buscaCliente, 500);
  const [dataVencimentoInicio, setDataVencimentoInicio] = useState<string>('');
  const [dataVencimentoFim, setDataVencimentoFim] = useState<string>('');
  const [apenasPendentes, setApenasPendentes] = useState<boolean>(true);
  const [formasPagamento, setFormasPagamento] = useState<FormaPagamento[]>([]);

  // Formulário de quitação
  const [dataRecebimento, setDataRecebimento] = useState<string>(
    new Date().toISOString().split('T')[0]
  );
  const [desconto, setDesconto] = useState<string>('0.00');
  const [juros, setJuros] = useState<string>('0.00');
  const [recebimentos, setRecebimentos] = useState<RecebimentoItem[]>([]);
  const [processando, setProcessando] = useState(false);
  const [enviandoWhatsApp, setEnviandoWhatsApp] = useState<string | null>(null);
  const [showConectarWhatsAppModal, setShowConectarWhatsAppModal] = useState(false);
  const [nomeEstabelecimento, setNomeEstabelecimento] = useState<string>('');

  const { showError, showSuccess, showWarning } = useToast();

  useEffect(() => {
    carregarFormasPagamento();
    carregarContas();
    vendasService.getEmitente().then((e) => {
      const nome = e?.nomeFantasia?.trim() || (e as any)?.nome?.trim() || ''
      if (nome) setNomeEstabelecimento(nome)
    }).catch(() => {});
  }, [apenasPendentes]);

  // Buscar cliente automaticamente quando o usuário digitar
  useEffect(() => {
    const termo = debouncedBuscaCliente.trim();
    
    if (termo.length === 0) {
      // Limpar busca se o campo estiver vazio
      if (clienteSelecionado) {
        setClienteSelecionado(null);
        setCodigoCliente('');
        setNomeCliente('');
        carregarContas();
      }
      return;
    }
    
    // Se for um número (código), buscar imediatamente mesmo com 1 dígito
    const codigoNum = parseInt(termo);
    if (!isNaN(codigoNum) && codigoNum > 0) {
      buscarClientePorNomeOuCodigo(termo);
      return;
    }
    
    // Para nomes, esperar pelo menos 2 caracteres
    if (termo.length >= 2) {
      buscarClientePorNomeOuCodigo(termo);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedBuscaCliente]);

  const carregarFormasPagamento = async () => {
    try {
      const formas = await recebimentosService.getFormasPagamento();
      setFormasPagamento(formas.filter((f: FormaPagamento) => f.ativo));
    } catch (error: any) {
      console.error('Erro ao carregar formas de pagamento:', error);
    }
  };

  const calcularTotalRecebimentos = () => {
    return recebimentos.reduce((total, r) => total + r.valor, 0);
  };

  const calcularRestante = () => {
    if (!contaSelecionada) return 0;
    const valorPendente = contaSelecionada.valorPendente;
    const descontoValor = parseFloat(desconto.replace(',', '.')) || 0;
    const jurosValor = parseFloat(juros.replace(',', '.')) || 0;
    const totalRecebimentos = calcularTotalRecebimentos();
    return valorPendente - totalRecebimentos + descontoValor - jurosValor;
  };

  const adicionarRecebimento = () => {
    if (formasPagamento.length === 0) {
      showError('Erro', 'Nenhuma forma de pagamento disponível');
      return;
    }

    if (!contaSelecionada) {
      showError('Erro', 'Selecione uma conta primeiro');
      return;
    }

    const restante = calcularRestante();
    if (restante <= 0) {
      showError('Erro', 'Valor já foi totalmente distribuído');
      return;
    }

    const primeiraForma = formasPagamento[0];
    const novoRecebimento: RecebimentoItem = {
      idFormaPagamento: primeiraForma.id,
      formaPagamento: primeiraForma,
      valor: restante
    };

    setRecebimentos([...recebimentos, novoRecebimento]);
  };

  const removerRecebimento = (index: number) => {
    setRecebimentos(recebimentos.filter((_, i) => i !== index));
  };

  const atualizarRecebimento = (index: number, campo: 'idFormaPagamento' | 'valor', valor: number | string) => {
    const novosRecebimentos = [...recebimentos];
    if (campo === 'idFormaPagamento') {
      const forma = formasPagamento.find(f => f.id === valor);
      if (forma) {
        novosRecebimentos[index].idFormaPagamento = forma.id;
        novosRecebimentos[index].formaPagamento = forma;
      }
    } else if (campo === 'valor') {
      const valorNum = typeof valor === 'string' 
        ? parseFloat(valor.replace(',', '.')) || 0 
        : valor;
      novosRecebimentos[index].valor = valorNum;
    }
    setRecebimentos(novosRecebimentos);
  };

  const carregarContas = async () => {
    try {
      setLoading(true);
      let dados: ContaReceber[] = [];

      const codigoParaBusca = clienteSelecionado?.id || (codigoCliente ? parseInt(codigoCliente) : undefined);

      if (codigoParaBusca) {
        dados = await receberService.listarPorCliente(
          codigoParaBusca,
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
        const telefone = conta.telefoneCliente || conta.TelefoneCliente || null;
        const quitado = conta.isQuitado ?? conta.estaQuitado ?? (conta.valorPendente <= 0);
        return {
          ...conta,
          notaFiscal: conta.notaFiscal || conta.NotaFiscal,
          controleNota: conta.controleNota || conta.ControleNota,
          telefoneCliente: telefone,
          isQuitado: !!quitado
        };
      });

      // Ordenar por nome do cliente (alfabético; sem nome vai para o final)
      dadosNormalizados.sort((a: ContaReceber, b: ContaReceber) => {
        const nomeA = (a.nomeCliente || '').trim();
        const nomeB = (b.nomeCliente || '').trim();
        if (!nomeA && !nomeB) return 0;
        if (!nomeA) return 1;
        if (!nomeB) return -1;
        return nomeA.localeCompare(nomeB, 'pt-BR');
      });
      
      setContas(dadosNormalizados);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar as contas a receber');
      console.error('Erro ao carregar contas:', error);
    } finally {
      setLoading(false);
    }
  };

  const buscarClientePorNomeOuCodigo = async (termo: string) => {
    if (!termo || termo.trim().length === 0) {
      return;
    }

    try {
      setBuscandoCliente(true);
      
      // Verificar se é um número (código) - aceita qualquer número positivo
      const codigoNum = parseInt(termo);
      if (!isNaN(codigoNum) && codigoNum > 0 && termo === codigoNum.toString()) {
        // Buscar por código primeiro
        try {
          const cliente = await clientesService.getById(codigoNum);
          if (cliente) {
            setClienteSelecionado(cliente);
            setCodigoCliente(cliente.id.toString());
            setNomeCliente(cliente.nome || '');
            setBuscaCliente(cliente.nome || termo);
            carregarContas();
            return;
          }
        } catch (error) {
          // Cliente não encontrado por código, continuar busca por nome
        }
      }
      
      // Buscar por nome (ou código se não encontrou)
      const clientes = await clientesService.buscar({ 
        q: termo, 
        ativo: true, 
        naoBloqueado: true,
        itensPorPagina: 10 
      });
      
      if (clientes.length === 1) {
        // Se encontrou apenas um cliente, selecionar automaticamente
        const cliente = clientes[0];
        setClienteSelecionado(cliente);
        setCodigoCliente(cliente.id.toString());
        setNomeCliente(cliente.nome || '');
        setBuscaCliente(cliente.nome || termo);
        carregarContas();
      } else if (clientes.length > 1) {
        // Múltiplos clientes encontrados, abrir modal de seleção
        setShowClienteSearch(true);
        // Preencher o campo de busca do modal se possível
      } else {
        // Nenhum cliente encontrado
        setClienteSelecionado(null);
        setCodigoCliente('');
        setNomeCliente('');
        showError('Cliente não encontrado', `Nenhum cliente encontrado para "${termo}"`);
      }
    } catch (error: any) {
      console.error('Erro ao buscar cliente:', error);
      setClienteSelecionado(null);
      setCodigoCliente('');
      setNomeCliente('');
      showError('Erro', 'Não foi possível buscar o cliente');
    } finally {
      setBuscandoCliente(false);
    }
  };

  const handleSelecionarCliente = (cliente: Cliente) => {
    setClienteSelecionado(cliente);
    setCodigoCliente(cliente.id.toString());
    setNomeCliente(cliente.nome || '');
    setBuscaCliente(cliente.nome || '');
    setShowClienteSearch(false);
    carregarContas();
  };

  const handleLimparCliente = () => {
    setClienteSelecionado(null);
    setCodigoCliente('');
    setNomeCliente('');
    setBuscaCliente('');
    carregarContas();
  };

  const handleAbrirModalQuitar = (conta: ContaReceber) => {
    setContaSelecionada(conta);
    setDesconto('0.00');
    setJuros('0.00');
    setRecebimentos([]);
    setShowQuitarModal(true);
    // Adicionar primeira forma de pagamento automaticamente
    if (formasPagamento.length > 0) {
      const primeiraForma = formasPagamento[0];
      setRecebimentos([{
        idFormaPagamento: primeiraForma.id,
        formaPagamento: primeiraForma,
        valor: conta.valorPendente
      }]);
    }
  };

  const handleEnviarWhatsAppConta = async (conta: ContaReceber) => {
    if (!conta.telefoneCliente) {
      showError('Aviso', 'Cliente não possui telefone cadastrado');
      return;
    }

    const contaKey = `${conta.numero}/${conta.ordem}`;
    
    // Verificar se já está enviando
    if (enviandoWhatsApp === contaKey) {
      return; // Já está enviando, não fazer nada
    }

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
        diasVencidos: conta.diasVencidos,
        nomeCliente: conta.nomeCliente,
        nomeEstabelecimento: nomeParaMensagem
      });
      
      const resultado = await enviarWhatsApp(conta.telefoneCliente, mensagem);
      if (resultado.method === 'sent') {
        showSuccess('✅ Lembrete enviado', 'Lembrete enviado com sucesso por WhatsApp!');
      } else if (resultado.method === 'link' && resultado.link) {
        abrirLinkWhatsApp(resultado.link);
        showSuccess('WhatsApp aberto', 'Envie o lembrete na janela que abriu (ou nesta aba).');
      } else {
        showWarning('Não enviou', 'WhatsApp não está conectado. Siga as instruções na tela para conectar e depois enviar o lembrete.');
        setShowConectarWhatsAppModal(true);
      }
    } catch (error: any) {
      showWarning('Não enviou', error.message || 'Não foi possível enviar o lembrete. Siga as instruções na tela para conectar o WhatsApp.');
      setShowConectarWhatsAppModal(true);
    } finally {
      setEnviandoWhatsApp(null);
    }
  };

  const handleVisualizarProdutos = async (conta: ContaReceber) => {
    // Usar o número da conta como nota (formato: numero/ordem -> usar apenas o numero)
    // Ou usar notaFiscal/controleNota se disponível
    let nota = conta.notaFiscal || conta.controleNota;
    
    // Se não tiver notaFiscal/controleNota, usar o número da conta
    if (!nota || nota === '0' || nota.trim() === '') {
      // Extrair apenas o número (antes da barra)
      nota = conta.numero.split('/')[0].trim();
    }
    
    if (!nota || nota === '0' || nota.trim() === '') {
      showError('Aviso', 'Não foi possível identificar a nota da venda');
      return;
    }

    setContaSelecionada(conta);
    setCarregandoProdutos(true);
    setShowProdutosModal(true);
    setProdutosVenda([]);
    setVendaSelecionada(null);

    try {
      // Buscar itens diretamente da tabela itevendas
      const itens = await vendasService.getItensByNota(nota);
      if (itens && itens.length > 0) {
        setProdutosVenda(itens);
        // Calcular totais para exibir no modal
        const totalItens = itens.reduce((sum, item) => sum + (item.total || 0), 0);
        setVendaSelecionada({
          nota: nota,
          total: totalItens,
          desconto: 0,
          acrescimo: 0,
          itens: itens
        } as any);
      } else {
        showError('Aviso', 'Não foram encontrados produtos para esta conta');
      }
    } catch (error: any) {
      console.error('Erro ao carregar produtos:', error);
      if (error.response?.status === 404) {
        showError('Aviso', 'Não foram encontrados produtos para esta conta');
      } else {
        showError('Erro', 'Não foi possível carregar os produtos da venda');
      }
    } finally {
      setCarregandoProdutos(false);
    }
  };

  const handleQuitar = async () => {
    if (!contaSelecionada) return;

    if (recebimentos.length === 0) {
      showError('Erro', 'Adicione pelo menos uma forma de pagamento');
      return;
    }

    const descontoValor = parseFloat(desconto.replace(',', '.')) || 0;
    const jurosValor = parseFloat(juros.replace(',', '.')) || 0;
    const totalRecebimentos = calcularTotalRecebimentos();
    const valorFinal = contaSelecionada.valorPendente - descontoValor + jurosValor;

    if (totalRecebimentos <= 0) {
      showError('Erro', 'O valor total recebido deve ser maior que zero');
      return;
    }

    if (Math.abs(totalRecebimentos - valorFinal) > 0.01) {
      showError('Erro', `A soma das formas de pagamento (${formatarMoeda(totalRecebimentos)}) deve ser igual ao valor final (${formatarMoeda(valorFinal)})`);
      return;
    }

    setProcessando(true);
    try {
      const valorTotal = recebimentos.reduce((sum, r) => sum + r.valor, 0);

      // Preparar formas de pagamento para o backend
      const formasPagamento = recebimentos.map(r => ({
        idFormaPagamento: r.idFormaPagamento,
        valor: r.valor
      }));

      const resultado = await receberService.quitar({
        numero: contaSelecionada.numero,
        ordem: contaSelecionada.ordem,
        valorRecebido: valorTotal,
        dataRecebimento: dataRecebimento || undefined,
        desconto: descontoValor,
        juros: jurosValor,
        formasPagamento: formasPagamento
      });

      showSuccess('Sucesso', 'Conta quitada com sucesso!');
      
      // Verificar se há outras contas em aberto e exibir aviso
      if (resultado.contasAberto?.temContasAberto) {
        setTimeout(() => {
          showError('Atenção', resultado.contasAberto.mensagem);
        }, 1000);
      }
      
      setShowQuitarModal(false);
      setContaSelecionada(null);
      setRecebimentos([]);
      setDesconto('0.00');
      setJuros('0.00');
      carregarContas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível quitar a conta');
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
    <>
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <CreditCard className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Receber Contas a Receber</h1>
              <p className="text-sm text-text-muted">Quitar contas a receber pendentes</p>
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

        {/* Filtros */}
        <div className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-4">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-2">
                Cliente (Nome ou Código)
              </label>
              <div className="flex gap-2">
                <div className="flex-1 relative">
                  <input
                    type="text"
                    value={buscaCliente}
                    onChange={(e) => {
                      setBuscaCliente(e.target.value);
                      if (!e.target.value.trim()) {
                        handleLimparCliente();
                      }
                    }}
                    onKeyDown={(e) => {
                      // Buscar imediatamente ao pressionar Enter
                      if (e.key === 'Enter' && buscaCliente.trim()) {
                        e.preventDefault();
                        buscarClientePorNomeOuCodigo(buscaCliente.trim());
                      }
                    }}
                    className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                              focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                              text-text-primary pr-10"
                    placeholder="Digite o nome ou código do cliente"
                  />
                  {buscandoCliente && (
                    <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                      <Loader2 className="w-4 h-4 text-primary animate-spin" />
                    </div>
                  )}
                </div>
                <Button
                  type="button"
                  onClick={() => setShowClienteSearch(true)}
                  variant="outline"
                  className="px-4"
                  title="Busca avançada de cliente"
                >
                  <Search className="w-4 h-4" />
                </Button>
                {clienteSelecionado && (
                  <Button
                    type="button"
                    onClick={handleLimparCliente}
                    variant="outline"
                    className="px-4"
                    title="Limpar cliente"
                  >
                    <X className="w-4 h-4" />
                  </Button>
                )}
              </div>
              {clienteSelecionado && (
                <p className="text-xs text-text-muted mt-1">
                  Cliente selecionado: <span className="font-semibold">{nomeCliente}</span> (Cód: {codigoCliente})
                </p>
              )}
            </div>
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-2">
                Data Vencimento Início
              </label>
              <input
                type="date"
                value={dataVencimentoInicio}
                onChange={(e) => setDataVencimentoInicio(e.target.value)}
                className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                          focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                          text-text-primary"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-2">
                Data Vencimento Fim
              </label>
              <input
                type="date"
                value={dataVencimentoFim}
                onChange={(e) => setDataVencimentoFim(e.target.value)}
                className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                          focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                          text-text-primary"
              />
            </div>
            <div className="flex items-end">
              <Button
                onClick={carregarContas}
                disabled={loading}
                className="w-full px-6 py-3"
              >
                {loading ? (
                  <>
                    <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                    Carregando...
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
          <div className="flex items-center space-x-2">
            <input
              type="checkbox"
              id="apenasPendentes"
              checked={apenasPendentes}
              onChange={(e) => setApenasPendentes(e.target.checked)}
              className="w-4 h-4 text-primary rounded"
            />
            <label htmlFor="apenasPendentes" className="text-sm text-text-secondary cursor-pointer">
              Apenas contas pendentes
            </label>
          </div>
        </div>

        {/* Resumo */}
        {contas.length > 0 && (
          <div className="bg-primary/10 border-2 border-primary/30 rounded-xl p-4 mb-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-text-secondary">Total de Contas</p>
                <p className="text-2xl font-bold text-text-primary">{contas.length}</p>
              </div>
              <div className="text-right">
                <p className="text-sm text-text-secondary">Total Pendente</p>
                <p className="text-2xl font-bold text-primary">{formatarMoeda(totalPendente)}</p>
              </div>
            </div>
          </div>
        )}

        {/* Lista de Contas */}
        {loading ? (
          <div className="bg-card rounded-2xl p-12 text-center shadow-lg border border-border">
            <Loader2 className="w-12 h-12 text-primary mx-auto mb-4 animate-spin" />
            <p className="text-text-muted">Carregando contas...</p>
          </div>
        ) : contas.length === 0 ? (
          <div className="bg-card rounded-2xl p-12 text-center shadow-lg border border-border">
            <FileText className="w-16 h-16 text-text-muted mx-auto mb-4" />
            <p className="text-text-muted">Nenhuma conta encontrada</p>
          </div>
        ) : (
          <div className="space-y-4">
            {contas.map((conta, index) => (
              <motion.div
                key={`${conta.numero}-${conta.ordem}`}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.05 }}
                className="bg-card rounded-2xl p-6 shadow-lg border border-border hover:shadow-xl transition-shadow"
              >
                <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-2">
                      <div className={`w-3 h-3 rounded-full ${conta.isQuitado ? 'bg-green-500' : isVencida(conta.vencimento) ? 'bg-red-500' : 'bg-yellow-500'}`} />
                      <h3 className="font-bold text-lg text-text-primary">
                        Conta {conta.numero}/{conta.ordem}
                      </h3>
                      {conta.isQuitado && (
                        <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded">
                          Quitada
                        </span>
                      )}
                      {!conta.isQuitado && isVencida(conta.vencimento) && (
                        <span className="px-2 py-1 bg-red-100 text-red-800 text-xs font-medium rounded">
                          Vencida ({conta.diasVencidos} dias)
                        </span>
                      )}
                    </div>
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-sm">
                      <div>
                        <p className="text-text-secondary">Cliente</p>
                        <p className="font-semibold text-text-primary">
                          {conta.nomeCliente || `Cód. ${conta.codigo}`}
                        </p>
                      </div>
                      <div>
                        <p className="text-text-secondary">Vencimento</p>
                        <p className={`font-semibold ${isVencida(conta.vencimento) && !conta.isQuitado ? 'text-red-600' : 'text-text-primary'}`}>
                          {formatarData(conta.vencimento)}
                        </p>
                      </div>
                      <div>
                        <p className="text-text-secondary">Valor</p>
                        <p className="font-semibold text-text-primary">
                          {formatarMoeda(conta.valor)}
                        </p>
                      </div>
                      <div>
                        <p className="text-text-secondary">Pendente</p>
                        <p className="font-semibold text-primary">
                          {formatarMoeda(conta.valorPendente)}
                        </p>
                      </div>
                    </div>
                  </div>
                  <div className="flex gap-2 flex-wrap items-center w-full sm:w-auto">
                    {/* SEMPRE mostrar botão Produtos - SEM CONDIÇÕES */}
                    <Button
                      onClick={() => {
                        handleVisualizarProdutos(conta);
                      }}
                      variant="outline"
                      className="flex items-center justify-center gap-2 min-w-[120px]"
                      type="button"
                    >
                      <Package className="w-4 h-4" />
                      <span>Produtos</span>
                    </Button>
                    {/* Botão Lembrete por WhatsApp - lembrete amigável (não cobrança) */}
                    {conta.telefoneCliente && formatarNumeroTelefone(conta.telefoneCliente) && !conta.isQuitado && (
                      <Button
                        onClick={() => {
                          handleEnviarWhatsAppConta(conta);
                        }}
                        disabled={enviandoWhatsApp === `${conta.numero}/${conta.ordem}`}
                        variant="outline"
                        title="Enviar lembrete amigável por WhatsApp (valor em aberto)"
                        className="flex items-center justify-center gap-2 bg-green-50 hover:bg-green-100 text-green-700 border-green-300 min-w-[120px] disabled:opacity-50 disabled:cursor-not-allowed"
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
                    {/* Botão Quitar - apenas se não estiver quitado */}
                    {!conta.isQuitado && (
                      <Button
                        onClick={() => handleAbrirModalQuitar(conta)}
                        className="flex items-center justify-center gap-2 min-w-[120px]"
                      >
                        <DollarSign className="w-4 h-4" />
                        <span>Quitar</span>
                      </Button>
                    )}
                  </div>
                </div>
              </motion.div>
            ))}
          </div>
        )}

        {/* Modal de Quitação */}
        <AnimatePresence>
          {showQuitarModal && contaSelecionada && (
            <>
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="fixed inset-0 bg-black/50 z-50"
                onClick={() => !processando && setShowQuitarModal(false)}
              />
              <motion.div
                initial={{ scale: 0.95, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                exit={{ scale: 0.95, opacity: 0 }}
                className="fixed inset-0 z-50 flex items-center justify-center p-4"
                onClick={(e) => e.stopPropagation()}
              >
                <div className="bg-card rounded-2xl p-6 max-w-md w-full shadow-2xl border border-border">
                  <div className="flex items-center justify-between mb-4">
                    <h2 className="text-xl font-bold text-text-primary flex items-center">
                      <CreditCard className="w-5 h-5 mr-2 text-primary" />
                      Quitar Conta
                    </h2>
                    <button
                      onClick={() => !processando && setShowQuitarModal(false)}
                      disabled={processando}
                      className="text-text-muted hover:text-text-primary transition-colors disabled:opacity-50"
                    >
                      <X className="w-5 h-5" />
                    </button>
                  </div>

                  <div className="space-y-4 mb-6">
                    <div className="bg-background-secondary rounded-xl p-4">
                      <p className="text-sm text-text-secondary mb-1">Conta</p>
                      <p className="font-bold text-text-primary">
                        {contaSelecionada.numero}/{contaSelecionada.ordem}
                      </p>
                      <p className="text-sm text-text-secondary mt-2">Cliente</p>
                      <p className="font-semibold text-text-primary">
                        {contaSelecionada.nomeCliente || `Cód. ${contaSelecionada.codigo}`}
                      </p>
                      <p className="text-sm text-text-secondary mt-2">Valor Pendente</p>
                      <p className="text-xl font-bold text-primary">
                        {formatarMoeda(contaSelecionada.valorPendente)}
                      </p>
                    </div>

                    {/* Formas de Pagamento */}
                    <div>
                      <div className="flex items-center justify-between mb-2">
                        <label className="block text-sm font-medium text-text-secondary">
                          Formas de Pagamento *
                        </label>
                        <Button
                          type="button"
                          onClick={adicionarRecebimento}
                          variant="outline"
                          size="sm"
                          disabled={processando || calcularRestante() <= 0}
                          className="text-xs"
                        >
                          <CreditCard className="w-3 h-3 mr-1" />
                          Adicionar
                        </Button>
                      </div>
                      
                      {recebimentos.length === 0 ? (
                        <div className="text-center py-4 text-text-muted text-sm border border-border rounded-xl bg-background-secondary">
                          <p>Nenhuma forma de pagamento adicionada</p>
                          <p className="text-xs mt-1">Clique em "Adicionar" para incluir uma forma de pagamento</p>
                        </div>
                      ) : (
                        <div className="space-y-3">
                          {recebimentos.map((recebimento, index) => (
                            <div
                              key={index}
                              className="bg-background-secondary border border-border rounded-xl p-4"
                            >
                              <div className="flex items-start justify-between mb-3">
                                <div className="flex-1 mr-2">
                                  <label className="block text-xs font-medium text-text-secondary mb-1">
                                    Forma de Pagamento
                                  </label>
                                  <select
                                    value={recebimento.idFormaPagamento}
                                    onChange={(e) => atualizarRecebimento(index, 'idFormaPagamento', parseInt(e.target.value))}
                                    className="w-full px-3 py-2 bg-background border border-border rounded-lg
                                              focus:outline-none focus:ring-2 focus:ring-primary/50
                                              text-text-primary text-sm"
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
                                  disabled={processando || recebimentos.length === 1}
                                  className="mt-6 text-red-500 hover:text-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                                  title="Remover forma de pagamento"
                                >
                                  <X className="w-4 h-4" />
                                </button>
                              </div>
                              <div>
                                <label className="block text-xs font-medium text-text-secondary mb-1">
                                  Valor
                                </label>
                                <input
                                  type="text"
                                  value={recebimento.valor.toFixed(2).replace('.', ',')}
                                  onChange={(e) => {
                                    const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                                    atualizarRecebimento(index, 'valor', value);
                                  }}
                                  className="w-full px-3 py-2 bg-background border border-border rounded-lg
                                            focus:outline-none focus:ring-2 focus:ring-primary/50
                                            text-text-primary text-sm"
                                  placeholder="0,00"
                                  disabled={processando}
                                />
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                      
                      {/* Resumo dos valores */}
                      <div className="mt-4 p-3 bg-primary/10 border border-primary/30 rounded-xl">
                        <div className="flex justify-between text-sm mb-1">
                          <span className="text-text-secondary">Valor Pendente:</span>
                          <span className="font-semibold text-text-primary">
                            {formatarMoeda(contaSelecionada.valorPendente)}
                          </span>
                        </div>
                        <div className="flex justify-between text-sm mb-1">
                          <span className="text-text-secondary">Desconto:</span>
                          <span className="font-semibold text-green-600">
                            - {formatarMoeda(parseFloat(desconto.replace(',', '.')) || 0)}
                          </span>
                        </div>
                        <div className="flex justify-between text-sm mb-1">
                          <span className="text-text-secondary">Juros:</span>
                          <span className="font-semibold text-red-600">
                            + {formatarMoeda(parseFloat(juros.replace(',', '.')) || 0)}
                          </span>
                        </div>
                        <div className="flex justify-between text-sm font-bold border-t border-primary/30 pt-2 mt-2">
                          <span className="text-text-primary">Total Recebido:</span>
                          <span className={`${calcularRestante() === 0 ? 'text-green-600' : 'text-primary'}`}>
                            {formatarMoeda(calcularTotalRecebimentos())}
                          </span>
                        </div>
                        {calcularRestante() !== 0 && (
                          <div className="flex justify-between text-xs mt-1">
                            <span className="text-text-muted">Restante:</span>
                            <span className={`font-semibold ${calcularRestante() > 0 ? 'text-yellow-600' : 'text-red-600'}`}>
                              {formatarMoeda(Math.abs(calcularRestante()))}
                            </span>
                          </div>
                        )}
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-text-secondary mb-2">
                          Desconto
                        </label>
                        <input
                          type="text"
                          value={desconto}
                          onChange={(e) => {
                            const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                            setDesconto(value);
                          }}
                          className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                                    focus:outline-none focus:ring-2 focus:ring-primary/50
                                    text-text-primary"
                          placeholder="0.00"
                          disabled={processando}
                        />
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-text-secondary mb-2">
                          Juros
                        </label>
                        <input
                          type="text"
                          value={juros}
                          onChange={(e) => {
                            const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                            setJuros(value);
                          }}
                          className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                                    focus:outline-none focus:ring-2 focus:ring-primary/50
                                    text-text-primary"
                          placeholder="0.00"
                          disabled={processando}
                        />
                      </div>
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-text-secondary mb-2">
                        Data de Recebimento
                      </label>
                      <input
                        type="date"
                        value={dataRecebimento}
                        onChange={(e) => setDataRecebimento(e.target.value)}
                        className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                                  focus:outline-none focus:ring-2 focus:ring-primary/50
                                  text-text-primary"
                        disabled={processando}
                      />
                    </div>
                  </div>

                  <div className="flex space-x-3">
                    <Button
                      onClick={() => setShowQuitarModal(false)}
                      variant="outline"
                      className="flex-1"
                      disabled={processando}
                    >
                      Cancelar
                    </Button>
                    <Button
                      onClick={handleQuitar}
                      disabled={processando}
                      className="flex-1"
                    >
                      {processando ? (
                        <>
                          <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                          Processando...
                        </>
                      ) : (
                        <>
                          <CheckCircle className="w-4 h-4 mr-2" />
                          Quitar
                        </>
                      )}
                    </Button>
                  </div>
                </div>
              </motion.div>
            </>
          )}
        </AnimatePresence>

        {/* Modal de Busca de Cliente */}
        <ClienteSearch
          isOpen={showClienteSearch}
          onClose={() => setShowClienteSearch(false)}
          onSelectCliente={handleSelecionarCliente}
        />

        {/* Modal de Produtos da Venda */}
        <AnimatePresence>
          {showProdutosModal && contaSelecionada && (
            <>
              <motion.div
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                className="fixed inset-0 bg-black/50 z-50"
                onClick={() => setShowProdutosModal(false)}
              />
              <motion.div
                initial={{ scale: 0.95, opacity: 0 }}
                animate={{ scale: 1, opacity: 1 }}
                exit={{ scale: 0.95, opacity: 0 }}
                className="fixed inset-0 z-50 flex items-center justify-center p-4"
                onClick={(e) => e.stopPropagation()}
              >
                <div className="bg-card rounded-2xl p-6 max-w-3xl w-full max-h-[90vh] overflow-hidden flex flex-col shadow-2xl border border-border">
                  <div className="flex items-center justify-between mb-4 flex-shrink-0">
                    <h2 className="text-xl font-bold text-text-primary flex items-center">
                      <Package className="w-5 h-5 mr-2 text-primary" />
                      Produtos da Venda
                    </h2>
                    <button
                      onClick={() => setShowProdutosModal(false)}
                      className="text-text-muted hover:text-text-primary transition-colors"
                    >
                      <X className="w-5 h-5" />
                    </button>
                  </div>

                  <div className="mb-4 flex-shrink-0">
                    <div className="bg-background-secondary rounded-xl p-4">
                      <p className="text-sm text-text-secondary mb-1">Conta</p>
                      <p className="font-bold text-text-primary">
                        {contaSelecionada.numero}/{contaSelecionada.ordem}
                      </p>
                      {(contaSelecionada.notaFiscal || contaSelecionada.controleNota) && (
                        <>
                          <p className="text-sm text-text-secondary mt-2 mb-1">Nota Fiscal</p>
                          <p className="font-semibold text-text-primary">
                            {contaSelecionada.notaFiscal || contaSelecionada.controleNota}
                          </p>
                        </>
                      )}
                      {vendaSelecionada && (
                        <>
                          <p className="text-sm text-text-secondary mt-2 mb-1">Total da Venda</p>
                          <p className="text-xl font-bold text-primary">
                            {formatarMoeda(vendaSelecionada.total || 0)}
                          </p>
                        </>
                      )}
                    </div>
                  </div>

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
                      <div className="space-y-3">
                        {produtosVenda.map((item, index) => (
                          <motion.div
                            key={`${item.nota}-${item.item}`}
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                            transition={{ delay: index * 0.05 }}
                            className="bg-background-secondary rounded-xl p-4 border border-border hover:border-primary/30 transition-colors"
                          >
                            <div className="flex items-start justify-between">
                              <div className="flex-1">
                                <div className="flex items-center gap-2 mb-2">
                                  <span className="text-sm font-semibold text-text-secondary">Item {item.item}</span>
                                  {item.codigo && (
                                    <span className="text-xs text-text-muted">Cód: {item.codigo}</span>
                                  )}
                                </div>
                                <p className="font-semibold text-text-primary mb-1">
                                  {item.descricao || `Produto ${item.codigo}`}
                                </p>
                                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 text-sm mt-2">
                                  <div>
                                    <p className="text-text-secondary">Quantidade</p>
                                    <p className="font-medium text-text-primary">
                                      {item.qtd?.toFixed(2) || '0.00'} {item.und || 'UN'}
                                    </p>
                                  </div>
                                  <div>
                                    <p className="text-text-secondary">Preço Unit.</p>
                                    <p className="font-medium text-text-primary">
                                      {formatarMoeda(item.preco || 0)}
                                    </p>
                                  </div>
                                  {item.desconto > 0 && (
                                    <div>
                                      <p className="text-text-secondary">Desconto</p>
                                      <p className="font-medium text-red-500">
                                        - {formatarMoeda(item.desconto)}
                                      </p>
                                    </div>
                                  )}
                                  <div>
                                    <p className="text-text-secondary">Total</p>
                                    <p className="font-bold text-primary">
                                      {formatarMoeda(item.total || 0)}
                                    </p>
                                  </div>
                                </div>
                              </div>
                            </div>
                          </motion.div>
                        ))}
                      </div>
                    )}
                  </div>

                  {produtosVenda.length > 0 && (
                    <div className="mt-4 pt-4 border-t border-border flex-shrink-0">
                      <div className="flex justify-between items-center">
                        <span className="text-sm font-medium text-text-secondary">Total de Itens:</span>
                        <span className="font-bold text-text-primary">{produtosVenda.length}</span>
                      </div>
                      {vendaSelecionada && (
                        <>
                          {vendaSelecionada.desconto > 0 && (
                            <div className="flex justify-between items-center mt-2">
                              <span className="text-sm text-text-secondary">Desconto:</span>
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
                          <div className="flex justify-between items-center mt-3 pt-3 border-t border-border">
                            <span className="text-lg font-bold text-text-primary">Total da Venda:</span>
                            <span className="text-xl font-bold text-primary">
                              {formatarMoeda(vendaSelecionada.total || 0)}
                            </span>
                          </div>
                        </>
                      )}
                    </div>
                  )}
                </div>
              </motion.div>
            </>
          )}
        </AnimatePresence>
      </div>
    </div>
    <ConectarWhatsAppModal
      isOpen={showConectarWhatsAppModal}
      onClose={() => setShowConectarWhatsAppModal(false)}
    />
    </>
  );
};

export default ReceberContasReceberPage;
