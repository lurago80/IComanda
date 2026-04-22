import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useCallback, useEffect, useState } from 'react';
import AbrirComandaModal, { DadosComanda } from './components/AbrirComandaModal';
import ComandasAbertasModal from './components/ComandasAbertasModal';
import ExtratoModal from './components/ExtratoModal';
import ExtratoView from './components/ExtratoView';
import GruposList from './components/GruposList';
import HistoryModal from './components/HistoryModal';
import MenuPrincipal from './components/MenuPrincipal';
import ConectarWhatsAppModal from './components/ConectarWhatsAppModal';
import ProdutosList from './components/ProdutosList';
import SearchResults from './components/SearchResults';
import SuccessActionSheet from './components/SuccessActionSheet';
import ToastContainer from './components/ToastContainer';
import TotalComandasAbertasModal from './components/TotalComandasAbertasModal';
import CartDrawer from './components/cart/CartDrawer';
import CartFAB from './components/cart/CartFAB';
import TaxaEntregaModal from './components/TaxaEntregaModal';
import { useToast } from './hooks/useToast';
import Login from './pages/Login';
import ConferenciaMesaPage from './pages/ConferenciaMesa';
import GridComandas from './pages/GridComandas';
import CaixasPage from './pages/CaixasPage';
import RelatoriosPage from './pages/RelatoriosPage';
import RelatorioPeriodoPage from './pages/RelatorioPeriodoPage';
import MesasPage from './pages/MesasPage';
import ContasReceberPage from './pages/ContasReceberPage';
import HistoricoPage from './pages/HistoricoPage';
import NotificacoesPage from './pages/NotificacoesPage';
import RecebimentoPage from './pages/RecebimentoPage';
import RelatorioRecebimentosPage from './pages/RelatorioRecebimentosPage';
import ReimpressaoRecibosPage from './pages/ReimpressaoRecibosPage';
import ReceberContasReceberPage from './pages/ReceberContasReceberPage';
import ClientesPage from './pages/ClientesPage';
import CadastroClientePage from './pages/CadastroClientePage';
import ProdutosPage from './pages/ProdutosPage';
import CadastroProdutoPage from './pages/CadastroProdutoPage';
import GruposPage from './pages/GruposPage';
import TaxasEntregaPage from './pages/TaxasEntregaPage';
import DeliveryPage from './pages/DeliveryPage';
import ConfiguracoesPage from './pages/ConfiguracoesPage';
import ForcaVendasPage from './pages/ForcaVendasPage';
import CadastroVendedoresPage from './pages/CadastroVendedoresPage';
import RankingVendedoresPage from './pages/RankingVendedoresPage';
import NovoPedidoFVPage from './pages/NovoPedidoFVPage';
import DashboardVendedorPage from './pages/DashboardVendedorPage';
import RotaVisitasPage from './pages/RotaVisitasPage';
import { useCartStore } from './store/cartStore';
import { Cliente, Grupo, Produto, ProdutoCompleto, Venda } from './types/api';
import { vendasService, configuracoesService, gruposService } from './services/api';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      staleTime: 30_000,
      refetchOnWindowFocus: false,
    },
  },
});

type View = 'menu' | 'produtos' | 'conferencia' | 'pesquisa' | 'grid-comandas' | 'caixas' | 'relatorios' | 'relatorio-periodo' | 'mesas' | 'contas-receber' | 'historico' | 'notificacoes' | 'recebimento' | 'relatorio-recebimentos' | 'reimpressao-recibos' | 'receber-contas-receber' | 'clientes' | 'cadastro-cliente' | 'produtos-cadastro' | 'cadastro-produto' | 'grupos' | 'taxas-entrega' | 'delivery-abertos' | 'delivery-novo' | 'configuracoes' | 'fv-home' | 'fv-novo-pedido' | 'fv-dashboard-vendedor' | 'fv-rotas' | 'fv-cadastro-vendedores' | 'fv-ranking';

function App() {
  const [view, setView] = useState<View>('menu');
  const [grupoSelecionado, setGrupoSelecionado] = useState<Grupo | null>(null);
  const [searchQuery, setSearchQuery] = useState<string>('');
  const [showHistory, setShowHistory] = useState<boolean>(false);
  const [showAbrirComanda, setShowAbrirComanda] = useState<boolean>(false);
  const [showComandasAbertas, setShowComandasAbertas] = useState<boolean>(false);
  const [showTotalComandas, setShowTotalComandas] = useState<boolean>(false);
  const [showExtratoModal, setShowExtratoModal] = useState<boolean>(false);
  const [showExtratoView, setShowExtratoView] = useState<boolean>(false);
  const [showConectarWhatsAppModal, setShowConectarWhatsAppModal] = useState<boolean>(false);
  const [notaExtrato, setNotaExtrato] = useState<string>('');
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [successContext, setSuccessContext] = useState<'nova' | 'edicao' | null>(null);
  const [totalComandasAbertas, setTotalComandasAbertas] = useState(0);
  const [totalValorAberto, setTotalValorAberto] = useState(0);
  const [totalDeliveryAbertos, setTotalDeliveryAbertos] = useState(0);
  const [totalValorDelivery, setTotalValorDelivery] = useState(0);
  const [clienteEditando, setClienteEditando] = useState<Cliente | null>(null);
  const [produtoEditando, setProdutoEditando] = useState<ProdutoCompleto | null>(null);
  const [notaCaixaRapido, setNotaCaixaRapido] = useState<string | null>(null);
  const { toasts, removeToast, showError, showSuccess } = useToast();
  const { setComandaAtiva, comandaAtiva, flowState, setFlowState, carregarPedidoParaEdicao, setCaixaRapidoMode, setDeliveryMode, setDeliveryClientePreSelecionado, clearCart, finalizarEdicao, fecharComanda, addItem, setCartOpen, items: cartItems, vendaEmEdicao } = useCartStore();
  const [showTaxaEntregaModal, setShowTaxaEntregaModal] = useState(false);
  const [produtoTaxaEntrega, setProdutoTaxaEntrega] = useState<Produto | null>(null);
  const [usarDelivery, setUsarDelivery] = useState(true);
  const [usarForcaVendas, setUsarForcaVendas] = useState(true);
  const [usarComanda, setUsarComanda] = useState(true);
  const [habilitarImprimirDuasVias, setHabilitarImprimirDuasVias] = useState(false);
  const [gruposParaImpressao, setGruposParaImpressao] = useState<Grupo[]>([]);
  const [idVendedorDash, setIdVendedorDash] = useState<number>(0);

  useEffect(() => {
    const usuario = localStorage.getItem('usuario_logado');
    const estaLogado = !!usuario;
    
    if (process.env.NODE_ENV !== 'production') {
      if (estaLogado) {
        console.log('✅ [App] Usuário autenticado');
      } else {
        console.log('⚠️ [App] Usuário não autenticado - Redirecionando para login');
      }
    }
    
    setIsLoggedIn(estaLogado);
  }, []);

  // Ouve o evento de sessão expirada emitido pelo interceptor de API
  useEffect(() => {
    const handleSessionExpired = () => {
      if (process.env.NODE_ENV !== 'production') console.log('⚠️ [App] Sessão expirada - exibindo tela de login');
      localStorage.removeItem('usuario_logado');
      localStorage.removeItem('refresh_token');
      localStorage.removeItem('jwt_expires_at');
      setIsLoggedIn(false);
    };
    window.addEventListener('session:expired', handleSessionExpired);
    return () => window.removeEventListener('session:expired', handleSessionExpired);
  }, []);

  // Função reutilizável para carregar totais
  const carregarTotais = useCallback(async () => {
    if (!isLoggedIn) return;
    
    try {
      const [vendas, deliveries] = await Promise.all([
        vendasService.getAbertas(),
        vendasService.getAbertas('DL').catch(() => [] as any[])
      ]);
      setTotalComandasAbertas(vendas.length);
      const total = vendas.reduce((acc, v) => acc + (v.total || 0), 0);
      setTotalValorAberto(total);
      setTotalDeliveryAbertos(deliveries.length);
      const totalDl = (deliveries as any[]).reduce<number>((acc, v) => acc + (v.total || 0), 0);
      setTotalValorDelivery(totalDl);
    } catch (error) {
      console.error('Erro ao carregar totais:', error);
    }
  }, [isLoggedIn]);

  // Carregar total de comandas abertas periodicamente
  useEffect(() => {
    if (!isLoggedIn) return;
    
    carregarTotais();
    const interval = setInterval(carregarTotais, 30000); // Atualizar a cada 30 segundos

    return () => clearInterval(interval);
  }, [carregarTotais, isLoggedIn]);

  // Carregar configurações do sistema ao fazer login
  useEffect(() => {
    if (!isLoggedIn) return;
    configuracoesService.getSistema()
      .then(cfg => {
        setUsarDelivery(cfg.usarDelivery);
        setUsarForcaVendas(cfg.usarForcaVendas ?? true);
        setUsarComanda(cfg.usarComanda ?? true);
        setHabilitarImprimirDuasVias(cfg.habilitarImprimirDuasVias ?? false);
      })
      .catch(() => { /* mantém padrões */ });
    gruposService.getTodosComQuantidade()
      .then(g => setGruposParaImpressao(g))
      .catch(() => { /* silencioso */ });
  }, [isLoggedIn]);  // eslint-disable-line react-hooks/exhaustive-deps

  // Ajustar view baseado no estado
  useEffect(() => {
    if (flowState === 'nova' && comandaAtiva) {
      setView('produtos');
    } else if (flowState === 'idle' && !comandaAtiva) {
      setView('menu');
    }
  }, [flowState, comandaAtiva]);

  // Ao entrar na tela de produtos (lançar na comanda), sempre mostrar todos os grupos primeiro
  useEffect(() => {
    if (view === 'produtos') {
      setGrupoSelecionado(null);
    }
  }, [view]);

  // Atualizar totais quando voltar para o menu
  useEffect(() => {
    if (view === 'menu') {
      carregarTotais();
    }
  }, [view, carregarTotais]);

  const handleSearch = (query: string) => {
    setSearchQuery(query);
    if (query.trim()) {
      setView('pesquisa');
      setGrupoSelecionado(null);
    } else {
      // Se a busca for limpa, voltar para grupos
      setView('produtos');
      setGrupoSelecionado(null);
    }
  };

  const handleOpenHistory = () => {
    setShowHistory(true);
  };

  const handleCloseHistory = () => {
    setShowHistory(false);
  };

  const handleOpenConferencia = () => {
    setView('conferencia');
  };

  const handleCloseConferencia = () => {
    setView('menu');
  };

  const handleOpenAbrirComanda = () => {
    setSuccessContext(null);
    setShowAbrirComanda(true);
  };

  const handleCloseAbrirComanda = () => {
    setShowAbrirComanda(false);
  };

  const handleComandaAberta = async (dados: DadosComanda) => {
    // Garantir que não há modo delivery/PDV ativo ao abrir comanda
    setDeliveryMode(false);
    setCaixaRapidoMode(false);
    setComandaAtiva({
      ...dados,
      dataAbertura: new Date()
    });
    setFlowState('nova');
    setShowAbrirComanda(false);
    setSuccessContext(null);
    setGrupoSelecionado(null); // Sempre mostrar todos os grupos ao abrir/ir para produtos
    setView('produtos');
    // Atualizar totais imediatamente após criar nova comanda
    await carregarTotais();
  };

  const handleAfterSuccess = async (tipo: 'nova' | 'edicao') => {
    if (process.env.NODE_ENV !== 'production') {
      console.log('🎉 handleAfterSuccess chamado com tipo:', tipo);
    }
    setSuccessContext(tipo);
    // Recarregar totais de comandas abertas após criar ou editar comanda
    await carregarTotais();
    // Não chamar tela de recebimento ao finalizar comanda normal
    // A tela de recebimento só é chamada via handleCaixaRapidoFechar (PDV)
  };

  const handleDeliverySuccess = async () => {
    if (process.env.NODE_ENV !== 'production') console.log('🚚 handleDeliverySuccess chamado - voltando para delivery-abertos');
    await carregarTotais();
    setView('delivery-abertos');
  };

  const handleNovaComandaCTA = () => {
    setSuccessContext(null);
    finalizarEdicao();
    clearCart();
    setShowAbrirComanda(true);
  };

  const handleCaixaRapido = () => {
    setSuccessContext(null);
    clearCart(); // Garante carrinho zerado ao iniciar Caixa Rápido
    finalizarEdicao();
    setCaixaRapidoMode(true);
    setComandaAtiva(null);
    setNotaCaixaRapido(null);
    setGrupoSelecionado(null);
    setView('produtos');
  };

  const handleCaixaRapidoFechar = (nota: string) => {
    // Garantir carrinho limpo ao ir para recebimento (evita itens “fantasma” na próxima abertura)
    finalizarEdicao();
    clearCart();
    setNotaCaixaRapido(nota);
    setView('recebimento');
  };

  const handleCloseRecebimento = () => {
    setView('menu');
    setNotaCaixaRapido(null);
    setCaixaRapidoMode(false);
  };

  const handleBuscarCTA = () => {
    setSuccessContext(null);
    finalizarEdicao();
    clearCart();
    setView('conferencia');
  };

  const handleVoltarMenuCTA = () => {
    setSuccessContext(null);
    finalizarEdicao();
    clearCart();
    setView('menu');
  };

  const handleBackToCategories = () => {
    setGrupoSelecionado(null);
    // Mantém searchQuery para pré-preencher o campo ao voltar para GruposList
    if (comandaAtiva) {
      setView('produtos');
    } else {
      setView('menu');
    }
  };

  const handleEditarComanda = async (nota: string) => {
    // Verificar se há pedido em andamento que seria sobrescrito
    const temComandaNova = flowState === 'nova' && comandaAtiva;
    // Se já está editando a MESMA comanda, não perguntar — apenas recarregar
    const mesmaComanda = flowState === 'edicao' && vendaEmEdicao?.nota === nota;
    const temEdicaoAtiva = flowState === 'edicao' && vendaEmEdicao && !mesmaComanda;

    if (temComandaNova) {
      const numComanda = comandaAtiva!.numeroComanda;
      const numItens = cartItems.length;
      const mensagem = numItens > 0
        ? `Você está lançando itens na Comanda #${numComanda} (${numItens} item${numItens !== 1 ? 's' : ''} no carrinho).\n\nSe importar a comanda agora, todos esses itens serão DESCARTADOS e o pedido atual será perdido.\n\nDeseja mesmo continuar?`
        : `Você tem a Comanda #${numComanda} aberta.\n\nImportar outra comanda irá descartá-la.\n\nDeseja continuar?`;

      const confirmado = window.confirm(mensagem);
      if (!confirmado) {
        return;
      }

      fecharComanda();
      clearCart();
    } else if (temEdicaoAtiva) {
      // Descarta edição atual e carrega a nova sem perguntar
      finalizarEdicao();
      clearCart();
    }

    // Garantir que não há modo delivery/PDV ativo ao editar comanda
    setDeliveryMode(false);
    setCaixaRapidoMode(false);
    try {
      // Buscar a venda e carregar para edição
      const venda = await vendasService.getByNota(nota);
      if (!venda) {
        showError('Erro', 'Venda não encontrada');
        return;
      }

      // Tentar conferência primeiro (itens + cliente); se falhar, usar itens da própria venda
      let itensParaEdicao: Array<{ codigo: number; descricao: string; qtd: number; precoUnitario: number; observacao?: string }> = [];
      let clienteParaEdicao: Cliente | undefined;

      const { conferenciaService } = await import('./services/conferenciaService');
      try {
        const conferencia = venda.comanda
          ? await conferenciaService.getConferenciaComanda(venda.comanda)
          : venda.mesa
          ? await conferenciaService.getConferenciaMesa(venda.mesa)
          : null;

        if (conferencia?.itens?.length) {
          itensParaEdicao = conferencia.itens.map((item) => ({
            codigo: item.codigo,
            descricao: item.descricao || '',
            qtd: item.qtd,
            precoUnitario: item.precoUnitario,
            observacao: item.observacao
          }));
          if (conferencia.cliente) {
            clienteParaEdicao = {
              id: conferencia.cliente.id,
              nome: conferencia.cliente.nome,
              nomeCompleto: conferencia.cliente.nome,
              cpfCnpj: conferencia.cliente.cpfCnpj || '',
              documento: conferencia.cliente.cpfCnpj || '',
              telefone: conferencia.cliente.telefone || '',
              celular: conferencia.cliente.telefone || '',
              ativo: true,
              bloqueado: false,
              idVendedor: 0,
              contato: conferencia.cliente.telefone || '',
              enderecoCompleto: ''
            };
          }
        }
      } catch (_) {
        // Conferência falhou (rede, 404, etc.); vamos usar venda.itens se existir
      }

      // Fallback: usar itens da venda quando conferência não retornou itens
      if (itensParaEdicao.length === 0 && venda.itens?.length) {
        itensParaEdicao = venda.itens.map((item) => ({
          codigo: item.codigo,
          descricao: item.descricao || `Item ${item.item}`,
          qtd: item.qtd,
          precoUnitario: item.preco,
          observacao: undefined
        }));
      }
      if (!clienteParaEdicao && (venda.nomeCliente || venda.cliente)) {
        clienteParaEdicao = {
          id: venda.cliente ?? 0,
          nome: venda.nomeCliente || '',
          nomeCompleto: venda.nomeCliente || `Cliente ${venda.cliente}`,
          cpfCnpj: '',
          documento: '',
          telefone: venda.telefoneCliente || '',
          celular: venda.telefoneCliente || '',
          ativo: true,
          bloqueado: false,
          idVendedor: 0,
          contato: venda.telefoneCliente || '',
          enderecoCompleto: ''
        };
      }

      if (itensParaEdicao.length === 0) {
        showError('Erro', 'Não foi possível carregar os dados da comanda (sem itens).');
        return;
      }

      carregarPedidoParaEdicao(
        {
          nota: venda.nota,
          mesa: venda.mesa,
          comanda: venda.comanda,
          nomeCliente: (venda.nomeCliente ?? '').trim() || undefined
        },
        itensParaEdicao,
        clienteParaEdicao
      );

      setView('produtos');
    } catch (error: any) {
      console.error('Erro ao editar comanda:', error);
      showError('Erro', 'Não foi possível carregar a comanda para edição');
    }
  };

  const handleGerarExtrato = () => {
    setShowExtratoModal(true);
  };

  const handleGerarExtratoNota = (nota: string) => {
    setNotaExtrato(nota);
    setShowExtratoView(true);
  };

  const handlePesquisarProdutos = () => {
    // Quando clicar em "Pesquisar Produtos", mostrar os grupos para selecionar
    // A busca será feita dentro do ProdutosList quando o usuário digitar
    setView('produtos');
    setSearchQuery('');
    setGrupoSelecionado(null);
  };

  const handleSair = () => {
    if (window.confirm('Deseja realmente sair do sistema?')) {
      localStorage.removeItem('usuario_logado');
      window.location.reload();
    }
  };

  const handleCaixas = () => {
    setView('caixas');
  };

  const handleRelatorios = () => {
    setView('relatorios');
  };

  const handleMesas = () => {
    setView('mesas');
  };

  const handleContasReceber = () => {
    setView('contas-receber');
  };

  const handleHistorico = () => {
    setView('historico');
  };

  const handleNotificacoes = () => {
    setView('notificacoes');
  };

  const handleRecebimento = () => {
    setView('recebimento');
  };

  const handleClientes = () => {
    setView('clientes');
  };

  const handleNovoCliente = () => {
    setClienteEditando(null);
    setView('cadastro-cliente');
  };

  const handleEditarCliente = (cliente: Cliente) => {
    setClienteEditando(cliente);
    setView('cadastro-cliente');
  };

  const handleVoltarClientes = () => {
    setClienteEditando(null);
    setView('clientes');
  };

  const handleProdutos = () => {
    setView('produtos-cadastro');
  };

  const handleGrupos = () => {
    setView('grupos');
  };

  const handleTaxasEntrega = () => {
    setView('taxas-entrega');
  };

  const handleDeliveryAbertos = () => {
    setView('delivery-abertos');
  };

  const handleDeliveryNovo = () => {
    setView('delivery-novo');
  };

  const handleNovoPedidoDelivery = (cliente?: Cliente) => {
    setSuccessContext(null);
    setDeliveryMode(true);
    setComandaAtiva(null);
    setCaixaRapidoMode(false);
    setFlowState('nova');
    setGrupoSelecionado(null);
    clearCart();
    if (cliente) setDeliveryClientePreSelecionado(cliente);
    else setDeliveryClientePreSelecionado(null);
    setView('produtos');
  };

  const handleEditarDelivery = async (nota: string) => {
    try {
      const venda = await vendasService.getByNota(nota);
      if (!venda) {
        showError('Erro', 'Pedido não encontrado');
        return;
      }
      // Vendas em aberto (ABERTO) ou saindo para entrega (SAINDO) têm itens na tabela temporária
      const isAberta = ['ABERTO', 'SAINDO'].includes(String(venda.lancado || '').toUpperCase());
      type ItemEdicao = { codigo: number; descricao: string; qtd: number; precoUnitario: number; observacao?: string };
      let itens: ItemEdicao[] = [];
      try {
        if (isAberta) {
          const tmp = await vendasService.getItensTemporariosByCupom(nota);
          itens = (tmp || []).map((item: { codigo: number; descricao?: string; qtd: number; preco: number; serial?: string }) => ({
            codigo: item.codigo,
            descricao: item.descricao ?? '',
            qtd: item.qtd,
            precoUnitario: item.preco,
            observacao: item.serial ?? ''
          }));
        } else {
          const def = await vendasService.getItensByNota(nota);
          itens = (def || []).map((item: { codigo: number; descricao?: string; qtd: number; preco: number }) => ({
            codigo: item.codigo,
            descricao: item.descricao ?? '',
            qtd: item.qtd,
            precoUnitario: item.preco,
            observacao: ''
          }));
        }
      } catch (_) {
        // 404 ou erro: abrir edição com lista vazia para incluir itens
        itens = [];
      }
      const clienteParaEdicao: Cliente | undefined = venda.cliente
        ? {
            id: venda.cliente,
            nome: venda.nomeCliente ?? '',
            nomeCompleto: venda.nomeCliente ?? '',
            cpfCnpj: '',
            documento: '',
            telefone: venda.telefoneCliente ?? '',
            celular: venda.telefoneCliente ?? '',
            ativo: true,
            bloqueado: false,
            idVendedor: 0,
            contato: venda.telefoneCliente ?? '',
            enderecoCompleto: ''
          }
        : undefined;
      carregarPedidoParaEdicao({ nota: venda.nota }, itens, clienteParaEdicao);
      setDeliveryMode(true);
      setView('produtos');
      setCartOpen(true);
    } catch (error: any) {
      console.error('Erro ao carregar pedido delivery:', error);
      showError('Erro', 'Não foi possível carregar o pedido para edição');
    }
  };

  const handleNovoProduto = () => {
    setProdutoEditando(null);
    setView('cadastro-produto');
  };

  const handleEditarProduto = (produto: ProdutoCompleto) => {
    setProdutoEditando(produto);
    setView('cadastro-produto');
  };

  const handleVoltarProdutos = () => {
    setProdutoEditando(null);
    setView('produtos-cadastro');
  };

  const handleRelatorioRecebimentos = () => {
    setView('relatorio-recebimentos');
  };

  const handleRelatorioPeriodo = () => {
    setView('relatorio-periodo');
  };

  const handleReimpressaoRecibos = () => {
    setView('reimpressao-recibos');
  };

  const handleReceberContasReceber = () => {
    setView('receber-contas-receber');
  };

  const handleConfiguracoes = () => {
    setView('configuracoes');
  };

  const handleBackup = async () => {
    try {
      const resultado = await configuracoesService.fazerBackup();
      showSuccess('Backup', `${resultado.mensagem} — ${resultado.arquivo} (${resultado.tamanhoMb} MB)`);
    } catch (err: any) {
      const msg = err?.response?.data?.error || err?.message || 'Erro ao fazer backup.';
      showError('Backup', msg);
    }
  };

  const handleSalvarConfiguracoes = (novoUsarDelivery: boolean, novoUsarForcaVendas: boolean, novoUsarComanda: boolean, novoHabilitarImprimirDuasVias: boolean) => {
    setUsarDelivery(novoUsarDelivery);
    setUsarForcaVendas(novoUsarForcaVendas);
    setUsarComanda(novoUsarComanda);
    setHabilitarImprimirDuasVias(novoHabilitarImprimirDuasVias);
  };

  // Handlers Força de Vendas
  const handleFVHome = () => setView('fv-home');
  const handleFVNovoPedido = () => setView('fv-novo-pedido');
  const handleFVDashboard = (id: number) => { setIdVendedorDash(id); setView('fv-dashboard-vendedor'); };
  const handleFVRotas = () => setView('fv-rotas');

  if (!isLoggedIn) {
    return <Login />;
  }

  if (view === 'conferencia') {
    return (
      <QueryClientProvider client={queryClient}>
        <ConferenciaMesaPage onClose={handleCloseConferencia} />
      </QueryClientProvider>
    );
  }

  if (view === 'grid-comandas') {
    return (
      <QueryClientProvider client={queryClient}>
        <GridComandas 
          onClose={() => setView('menu')}
          onEditarComanda={handleEditarComanda}
        />
      </QueryClientProvider>
    );
  }

  // Views para novas funcionalidades
  if (view === 'caixas') {
    return (
      <QueryClientProvider client={queryClient}>
        <CaixasPage onClose={() => setView('menu')} />
      </QueryClientProvider>
    );
  }

  if (view === 'relatorios') {
    return (
      <QueryClientProvider client={queryClient}>
        <RelatoriosPage 
          onClose={() => setView('menu')} 
          onRelatorioRecebimentos={handleRelatorioRecebimentos}
          onRelatorioPeriodo={handleRelatorioPeriodo}
          onReimpressaoRecibos={handleReimpressaoRecibos}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'relatorio-periodo') {
    return (
      <QueryClientProvider client={queryClient}>
        <RelatorioPeriodoPage onClose={() => setView('relatorios')} />
      </QueryClientProvider>
    );
  }

  if (view === 'mesas') {
    return (
      <QueryClientProvider client={queryClient}>
        <MesasPage onClose={() => setView('menu')} />
      </QueryClientProvider>
    );
  }

  if (view === 'contas-receber') {
    return (
      <QueryClientProvider client={queryClient}>
        <ContasReceberPage 
          onClose={() => setView('menu')} 
          onReceber={handleReceberContasReceber}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'historico') {
    return (
      <QueryClientProvider client={queryClient}>
        <HistoricoPage onClose={() => setView('menu')} />
      </QueryClientProvider>
    );
  }

  if (view === 'notificacoes') {
    return (
      <QueryClientProvider client={queryClient}>
        <NotificacoesPage onClose={() => setView('menu')} />
      </QueryClientProvider>
    );
  }

  if (view === 'recebimento') {
    return (
      <QueryClientProvider client={queryClient}>
        <RecebimentoPage
          onClose={handleCloseRecebimento}
          notaPreSelecionada={notaCaixaRapido ?? undefined}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'relatorio-recebimentos') {
    return (
      <QueryClientProvider client={queryClient}>
        <RelatorioRecebimentosPage onClose={() => setView('menu')} />
      </QueryClientProvider>
    );
  }

  if (view === 'reimpressao-recibos') {
    return (
      <QueryClientProvider client={queryClient}>
        <ReimpressaoRecibosPage onClose={() => setView('menu')} />
      </QueryClientProvider>
    );
  }

  if (view === 'receber-contas-receber') {
    return (
      <QueryClientProvider client={queryClient}>
        <ReceberContasReceberPage onClose={() => setView('contas-receber')} />
      </QueryClientProvider>
    );
  }

  if (view === 'clientes') {
    return (
      <QueryClientProvider client={queryClient}>
        <ClientesPage 
          onClose={() => setView('menu')} 
          onNovoCliente={handleNovoCliente}
          onEditarCliente={handleEditarCliente}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'cadastro-cliente') {
    return (
      <QueryClientProvider client={queryClient}>
        <CadastroClientePage 
          onClose={handleVoltarClientes}
          clienteId={clienteEditando?.id}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'produtos-cadastro') {
    return (
      <QueryClientProvider client={queryClient}>
        <ProdutosPage
          onClose={() => setView('menu')}
          onNovoProduto={handleNovoProduto}
          onEditarProduto={handleEditarProduto}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'cadastro-produto') {
    return (
      <QueryClientProvider client={queryClient}>
        <CadastroProdutoPage
          onClose={handleVoltarProdutos}
          produtoId={produtoEditando?.id}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'grupos') {
    return (
      <QueryClientProvider client={queryClient}>
        <GruposPage onClose={() => {
          // Recarregar grupos para atualizar flags imprimirDuasVias no CartDrawer
          gruposService.getTodosComQuantidade()
            .then(g => setGruposParaImpressao(g))
            .catch(() => {});
          setView('menu');
        }} />
      </QueryClientProvider>
    );
  }

  if (view === 'taxas-entrega') {
    return (
      <QueryClientProvider client={queryClient}>
        <TaxasEntregaPage onClose={() => setView('menu')} />
      </QueryClientProvider>
    );
  }

  if (view === 'configuracoes') {
    return (
      <QueryClientProvider client={queryClient}>
        <ConfiguracoesPage
          onClose={() => setView('menu')}
          onSalvar={handleSalvarConfiguracoes}
        />
      </QueryClientProvider>
    );
  }

  // ── Força de Vendas ──────────────────────────────────────
  if (view === 'fv-home') {
    return (
      <QueryClientProvider client={queryClient}>
        <ForcaVendasPage
          onClose={() => setView('menu')}
          onNovoPedido={handleFVNovoPedido}
          onDashboard={handleFVDashboard}
          onCadastroVendedores={() => setView('fv-cadastro-vendedores')}
          onRanking={() => setView('fv-ranking')}
          onIrParaComanda={() => setView('menu')}
          onIrParaDelivery={usarDelivery ? handleDeliveryAbertos : undefined}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'fv-novo-pedido') {
    return (
      <QueryClientProvider client={queryClient}>
        <NovoPedidoFVPage
          onClose={() => setView('fv-home')}
          onSucesso={() => setView('fv-home')}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'fv-dashboard-vendedor') {
    return (
      <QueryClientProvider client={queryClient}>
        <DashboardVendedorPage
          idVendedor={idVendedorDash}
          onClose={() => setView('fv-home')}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'fv-rotas') {
    return (
      <QueryClientProvider client={queryClient}>
        <RotaVisitasPage
          onClose={() => setView('fv-home')}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'fv-cadastro-vendedores') {
    return (
      <QueryClientProvider client={queryClient}>
        <CadastroVendedoresPage
          onClose={() => setView('fv-home')}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'fv-ranking') {
    return (
      <QueryClientProvider client={queryClient}>
        <RankingVendedoresPage
          onClose={() => setView('fv-home')}
        />
      </QueryClientProvider>
    );
  }
  // ─────────────────────────────────────────────────────────

  if (view === 'delivery-abertos') {
    return (
      <QueryClientProvider client={queryClient}>
        <DeliveryPage
          mode="abertos"
          onClose={() => setView('menu')}
          onNovoPedido={handleNovoPedidoDelivery}
          onEditarPedido={handleEditarDelivery}
          onSwitchToNovo={() => setView('delivery-novo')}
          onIrParaComanda={() => setView('menu')}
          onIrParaForcaVendas={usarForcaVendas ? handleFVHome : undefined}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'delivery-novo') {
    return (
      <QueryClientProvider client={queryClient}>
        <DeliveryPage
          mode="novo"
          onClose={() => setView('menu')}
          onNovoPedido={handleNovoPedidoDelivery}
          onEditarPedido={handleEditarDelivery}
          onSwitchToAbertos={() => setView('delivery-abertos')}
          onIrParaComanda={() => setView('menu')}
          onIrParaForcaVendas={usarForcaVendas ? handleFVHome : undefined}
        />
      </QueryClientProvider>
    );
  }

  if (view === 'menu') {
    return (
      <QueryClientProvider client={queryClient}>
        <div className="min-h-screen bg-background">
          <MenuPrincipal
            onNovaComanda={handleOpenAbrirComanda}
            onCaixaRapido={handleCaixaRapido}
            onBuscarEditar={handleOpenConferencia}
            onVerComandasAbertas={() => setShowComandasAbertas(true)}
            onGerarExtrato={handleGerarExtrato}
            onTotalComandasAbertas={() => setShowTotalComandas(true)}
            onPesquisarProdutos={handlePesquisarProdutos}
            onGridComandas={() => setView('grid-comandas')}
            onSair={handleSair}
            onCaixas={handleCaixas}
            onRelatorios={handleRelatorios}
            onMesas={handleMesas}
            onContasReceber={handleContasReceber}
            onHistorico={handleHistorico}
            onNotificacoes={handleNotificacoes}
            onRecebimento={handleRecebimento}
            onClientes={handleClientes}
            onProdutos={handleProdutos}
            onGrupos={handleGrupos}
            onTaxasEntrega={handleTaxasEntrega}
            onDeliveryAbertos={usarDelivery ? handleDeliveryAbertos : undefined}
            onDeliveryNovo={usarDelivery ? handleDeliveryNovo : undefined}
            onForcaVendas={usarForcaVendas ? handleFVHome : undefined}
            onFVRotas={usarForcaVendas ? handleFVRotas : undefined}
            onConectarWhatsApp={() => setShowConectarWhatsAppModal(true)}
            onConfiguracoes={handleConfiguracoes}
            onBackup={handleBackup}
            onIrParaDelivery={usarDelivery ? handleDeliveryAbertos : undefined}
            onIrParaForcaVendas={usarForcaVendas ? handleFVHome : undefined}
            usarComanda={usarComanda}
            totalComandasAbertas={totalComandasAbertas}
            totalValorAberto={totalValorAberto}
            totalDeliveryAbertos={totalDeliveryAbertos}
            totalValorDelivery={totalValorDelivery}
          />
          <ConectarWhatsAppModal
            isOpen={showConectarWhatsAppModal}
            onClose={() => setShowConectarWhatsAppModal(false)}
          />
          <AbrirComandaModal
            isOpen={showAbrirComanda}
            onClose={handleCloseAbrirComanda}
            onComandaAberta={handleComandaAberta}
          />
          <ComandasAbertasModal
            isOpen={showComandasAbertas}
            onClose={() => setShowComandasAbertas(false)}
            onEditarComanda={handleEditarComanda}
            onRefresh={() => {
              // Recarregar totais quando o modal for atualizado
              const carregarTotais = async () => {
                try {
                  const vendas = await vendasService.getAbertas();
                  setTotalComandasAbertas(vendas.length);
                  const total = vendas.reduce((acc, v) => acc + (v.total || 0), 0);
                  setTotalValorAberto(total);
                } catch (error) {
                  console.error('Erro ao recarregar totais:', error);
                }
              };
              carregarTotais();
            }}
          />
          <TotalComandasAbertasModal
            isOpen={showTotalComandas}
            onClose={() => setShowTotalComandas(false)}
            onEditarComanda={handleEditarComanda}
          />
          <ExtratoModal
            isOpen={showExtratoModal}
            onClose={() => setShowExtratoModal(false)}
            onGerarExtrato={handleGerarExtratoNota}
          />
          {showExtratoView && (
            <ExtratoView
              nota={notaExtrato}
              onClose={() => {
                setShowExtratoView(false);
                setNotaExtrato('');
              }}
            />
          )}
          <CartDrawer
            onAfterSuccess={handleAfterSuccess}
            onCaixaRapidoFechar={handleCaixaRapidoFechar}
            onDeliverySuccess={handleDeliverySuccess}
            habilitarImprimirDuasVias={habilitarImprimirDuasVias}
            grupos={gruposParaImpressao}
          />
          <SuccessActionSheet
            mode={successContext ?? 'nova'}
            isOpen={!!successContext}
            onClose={() => {
              console.log('❌ SuccessActionSheet fechado');
              setSuccessContext(null);
              finalizarEdicao();
              clearCart();
            }}
            onNovaComanda={handleNovaComandaCTA}
            onBuscarComanda={handleBuscarCTA}
            onVoltarMenu={handleVoltarMenuCTA}
          />
          <ToastContainer 
            toasts={toasts}
            onRemove={removeToast}
          />
        </div>
      </QueryClientProvider>
    );
  }

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-background">
        <main className="pb-24 sm:pb-20">
          {(view === 'pesquisa' && searchQuery.trim()) ? (
            <SearchResults 
              query={searchQuery}
              onBack={handleBackToCategories}
              onQueryChange={(q) => {
                setSearchQuery(q);
                if (!q.trim()) {
                  setView('produtos');
                  setGrupoSelecionado(null);
                } else {
                  setView('pesquisa');
                }
              }}
              onRequestTaxaEntrega={(p) => { setProdutoTaxaEntrega(p); setShowTaxaEntregaModal(true); }}
            />
          ) : !grupoSelecionado ? (
            <GruposList 
              onSelecionarGrupo={setGrupoSelecionado}
              onVoltar={() => {
                setCaixaRapidoMode(false);
                setView('menu');
              }}
              initialBusca={searchQuery}
              onBuscarTodos={(q) => {
                setSearchQuery(q);
                setView('pesquisa');
                setGrupoSelecionado(null);
              }}
            />
          ) : (
            <ProdutosList 
              grupo={grupoSelecionado}
              onVoltar={() => setGrupoSelecionado(null)}
              onRequestTaxaEntrega={(p) => { setProdutoTaxaEntrega(p); setShowTaxaEntregaModal(true); }}
            />
          )}
        </main>

        <TaxaEntregaModal
          isOpen={showTaxaEntregaModal}
          onClose={() => { setShowTaxaEntregaModal(false); setProdutoTaxaEntrega(null); }}
          produto={produtoTaxaEntrega}
          onSelect={(produto, taxa) => { addItem(produto, 1, taxa.valor); }}
        />
        <CartFAB />
        <CartDrawer
          onAfterSuccess={handleAfterSuccess}
          onCaixaRapidoFechar={handleCaixaRapidoFechar}
          onDeliverySuccess={handleDeliverySuccess}
          habilitarImprimirDuasVias={habilitarImprimirDuasVias}
          grupos={gruposParaImpressao}
        />
        <SuccessActionSheet
          mode={successContext ?? 'nova'}
          isOpen={!!successContext}
          onClose={() => {
            console.log('❌ SuccessActionSheet fechado');
            setSuccessContext(null);
            finalizarEdicao();
            clearCart();
          }}
          onNovaComanda={handleNovaComandaCTA}
          onBuscarComanda={handleBuscarCTA}
          onVoltarMenu={handleVoltarMenuCTA}
        />
        <HistoryModal 
          isOpen={showHistory}
          onClose={handleCloseHistory}
        />
        <AbrirComandaModal
          isOpen={showAbrirComanda}
          onClose={handleCloseAbrirComanda}
          onComandaAberta={handleComandaAberta}
        />
        <ToastContainer 
          toasts={toasts}
          onRemove={removeToast}
        />
      </div>
    </QueryClientProvider>
  );
}

export default App;
