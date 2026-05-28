import axios from 'axios';
import { BuscarProdutoRequest, Cliente, CriarVendaRequest, Grupo, ItemVenda, KdsPedido, Produto, ProdutoCompleto, RelatorioConsignacao, TaxaEntrega, Venda, VendaFechadaRecibo } from '../types/api';

// URL da API: use REACT_APP_API_URL se definido (ex: em .env), senão detecta pelo host
const getApiBaseUrl = (): string => {
  const envUrl = typeof process.env.REACT_APP_API_URL === 'string' && process.env.REACT_APP_API_URL.trim();
  if (envUrl) {
    return envUrl.replace(/\/+$/, ''); // remove barra final se houver
  }
  if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    return 'http://localhost:65375/api';
  }
  const hostname = window.location.hostname;
  return `http://${hostname}:65375/api`;
};

const API_BASE_URL = getApiBaseUrl();
export { getApiBaseUrl };

const api = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, // envia o cookie httpOnly automaticamente
  timeout: 30000, // 30 segundos — evita tela travada se o backend estiver lento
  headers: {
    'Content-Type': 'application/json; charset=utf-8',
  },
});


// Interceptor para tratar 401 (token expirado)
let isRefreshing = false;
let pendingRequests: Array<(retry: boolean) => void> = [];
// Evita múltiplos redirects simultâneos para login
let isRedirectingToLogin = false;

const redirectToLogin = async () => {
  if (isRedirectingToLogin) return;
  isRedirectingToLogin = true;

  try { await axios.post(`${API_BASE_URL}/auth/logout`, {}, { withCredentials: true }); } catch {}
  localStorage.removeItem('usuario_logado');
  localStorage.removeItem('refresh_token');
  localStorage.removeItem('jwt_expires_at');

  isRedirectingToLogin = false;
  window.dispatchEvent(new CustomEvent('session:expired'));
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      const url = error.config?.url || '';
      const isLoginRoute = url.includes('/auth/login');
      const isRefreshRoute = url.includes('/auth/refresh');
      const isBusinessRoute = url.includes('excluir-comanda');
      const isMeRoute = url.includes('/auth/me'); // não disparar session:expired no startup

      if (!isLoginRoute && !isRefreshRoute && !isBusinessRoute && !isMeRoute) {
        const refreshToken = localStorage.getItem('refresh_token');

        if (refreshToken) {
          // Se já está renovando, enfileirar a requisição
          if (isRefreshing) {
            return new Promise((resolve, reject) => {
              pendingRequests.push((retry: boolean) => {
                if (retry) resolve(api.request(error.config));
                else reject(error);
              });
            });
          }

          isRefreshing = true;

          try {
            const refreshResponse = await axios.post(
              `${API_BASE_URL}/auth/refresh`,
              { token: '', refreshToken },
              { withCredentials: true }
            );

            // Salvar novo refresh token e tempo de expiração
            if (refreshResponse.data?.refreshToken) {
              localStorage.setItem('refresh_token', refreshResponse.data.refreshToken);
            }
            if (refreshResponse.data?.expiresIn) {
              localStorage.setItem('jwt_expires_at', String(Date.now() + refreshResponse.data.expiresIn * 3600000));
            }

            // Liberar requisições enfileiradas
            pendingRequests.forEach(cb => cb(true));
            pendingRequests = [];
            isRefreshing = false;

            // Repetir a requisição original (novo cookie já foi setado)
            return api.request(error.config);
          } catch {
            // Refresh falhou - prosseguir para logout
            pendingRequests.forEach(cb => cb(false));
            pendingRequests = [];
            isRefreshing = false;
          }
        }

        // Sem refresh token ou refresh falhou → redirecionar para login
        redirectToLogin();
      }
    }

    return Promise.reject(error);
  }
);

// Serviços para Grupos
export const gruposService = {
  getAll: async (): Promise<Grupo[]> => {
    const response = await api.get('/grupos');
    return response.data;
  },

  getTodosComQuantidade: async (): Promise<Grupo[]> => {
    try {
      const response = await api.get('/grupos/todos-com-quantidade');
      return response.data;
    } catch (error: any) {
      console.error('❌ [gruposService] Erro ao buscar grupos:', error?.message || error);
      throw error;
    }
  },

  getById: async (id: number): Promise<Grupo> => {
    const response = await api.get(`/grupos/${id}`);
    return response.data;
  },

  criar: async (descricao: string, imprimirDuasVias = false, percentual = 0): Promise<Grupo> => {
    const response = await api.post('/grupos', { descricao, imprimirDuasVias, percentual });
    return response.data;
  },

  atualizar: async (id: number, descricao: string, imprimirDuasVias = false, percentual = 0): Promise<Grupo> => {
    const response = await api.put(`/grupos/${id}`, { descricao, imprimirDuasVias, percentual });
    return response.data;
  },

  excluir: async (id: number): Promise<void> => {
    await api.delete(`/grupos/${id}`);
  },
};

// Serviços para Taxas de Entrega
export const taxasEntregaService = {
  getAll: async (): Promise<TaxaEntrega[]> => {
    const response = await api.get('/taxasentrega');
    return response.data;
  },

  getById: async (id: number): Promise<TaxaEntrega> => {
    const response = await api.get(`/taxasentrega/${id}`);
    return response.data;
  },

  criar: async (descricao: string, valor: number): Promise<TaxaEntrega> => {
    const response = await api.post('/taxasentrega', { descricao, valor });
    return response.data;
  },

  atualizar: async (id: number, descricao: string, valor: number): Promise<TaxaEntrega> => {
    const response = await api.put(`/taxasentrega/${id}`, { descricao, valor });
    return response.data;
  },

  excluir: async (id: number): Promise<void> => {
    await api.delete(`/taxasentrega/${id}`);
  },
};

// Serviços para Produtos
export const produtosService = {
  buscar: async (request: BuscarProdutoRequest): Promise<Produto[]> => {
    try {
      const response = await api.get('/produtos/buscar', { params: request });
      return response.data;
    } catch (error: any) {
      console.error('❌ [produtosService] Erro ao buscar produtos:', error?.message || error);
      throw error;
    }
  },

  getByGrupo: async (grupoId: number, ativo: boolean = true): Promise<Produto[]> => {
    try {
      const response = await api.get(`/produtos/grupo/${grupoId}`, {
        params: { ativo }
      });
      return Array.isArray(response.data) ? response.data : [];
    } catch (error: any) {
      throw error;
    }
  },

  getByGrupoPaginado: async (
    grupoId: number, 
    pagina: number = 1, 
    itensPorPagina: number = 50
  ) => {
    const response = await api.get(`/produtos/grupo/${grupoId}/paginado`, {
      params: { pagina, itensPorPagina }
    });
    return response.data;
  },

  getById: async (id: number): Promise<Produto> => {
    const response = await api.get(`/produtos/${id}`);
    return response.data;
  },

  getByCodigoBarras: async (codigoBarras: string): Promise<Produto> => {
    const response = await api.get(`/produtos/codigo-barras/${codigoBarras}`);
    return response.data;
  },

  // Métodos CRUD completos
  getCompleto: async (id: number): Promise<ProdutoCompleto> => {
    const response = await api.get(`/produtos/${id}/completo`);
    return response.data;
  },

  buscarCompletos: async (termo?: string, ativo?: boolean, pagina: number = 1, itensPorPagina: number = 50): Promise<ProdutoCompleto[]> => {
    const response = await api.get('/produtos/completos', {
      params: { termo, ativo, pagina, itensPorPagina }
    });
    return response.data;
  },

  criar: async (produto: Partial<ProdutoCompleto>): Promise<number> => {
    const response = await api.post('/produtos', produto);
    return response.data;
  },

  atualizar: async (id: number, produto: Partial<ProdutoCompleto>): Promise<void> => {
    await api.put(`/produtos/${id}`, produto);
  },

  excluir: async (id: number): Promise<void> => {
    await api.delete(`/produtos/${id}`);
  },

  getImagemUrl: (id: number): string => `${getApiBaseUrl()}/produtos/${id}/imagem`,

  atualizarImagem: async (id: number, imagemBase64: string): Promise<void> => {
    await api.put(`/produtos/${id}/imagem`, { imagemBase64 });
  },

  removerImagem: async (id: number): Promise<void> => {
    await api.delete(`/produtos/${id}/imagem`);
  },
};

// Serviços para Vendas
export const vendasService = {
  criar: async (request: CriarVendaRequest): Promise<Venda> => {
    const response = await api.post('/vendas', request);
    return response.data;
  },

  atualizar: async (nota: string, request: CriarVendaRequest): Promise<Venda> => {
    const response = await api.put(`/vendas/${nota}`, request);
    return response.data;
  },

  getByNota: async (nota: string): Promise<Venda> => {
    const response = await api.get(`/vendas/${nota}`);
    return response.data;
  },

  getItensByNota: async (nota: string): Promise<ItemVenda[]> => {
    const response = await api.get(`/vendas/${nota}/itens`);
    return response.data;
  },

  getItensTemporariosByCupom: async (cupom: string): Promise<ItemVenda[]> => {
    const response = await api.get(`/vendas/${cupom}/itens-temporarios`);
    return response.data;
  },

  getHoje: async (): Promise<Venda[]> => {
    const response = await api.get('/vendas/hoje');
    return response.data;
  },

  getByComanda: async (comanda: number): Promise<Venda[]> => {
    const response = await api.get(`/vendas/comanda/${comanda}`);
    return response.data;
  },

  getByMesa: async (mesa: number): Promise<Venda[]> => {
    const response = await api.get(`/vendas/mesa/${mesa}`);
    return response.data;
  },

  /** @param origem BA = comandas, DL = delivery. Se omitido, retorna comandas (BA). */
  getAbertas: async (origem?: string): Promise<Venda[]> => {
    const response = await api.get('/vendas/abertas', { params: origem ? { origem } : {} });
    return response.data;
  },

  /** Comandas/vendas excluídas (canceladas). Período opcional (últimos 30 dias se omitido). */
  getCanceladas: async (dataInicio?: string, dataFim?: string): Promise<Venda[]> => {
    const response = await api.get('/vendas/canceladas', { params: { dataInicio, dataFim } });
    return response.data;
  },

  /** Envia WhatsApp ao cliente informando que o pedido delivery saiu para entrega. */
  notificarSaiuParaEntrega: async (nota: string): Promise<{ enviado: boolean; mensagem: string }> => {
    const response = await api.post(`/vendas/delivery/${encodeURIComponent(nota)}/saiu-para-entrega`);
    return response.data;
  },

  /** Vendas fechadas/recebidas no período (para reimpressão de recibos). */
  getVendasFechadas: async (dataInicio?: string, dataFim?: string): Promise<VendaFechadaRecibo[]> => {
    const response = await api.get('/vendas/fechadas', {
      params: { dataInicio, dataFim }
    });
    return response.data;
  },

  verificarComandaAberta: async (comanda: number): Promise<boolean> => {
    const response = await api.get(`/vendas/comanda/${comanda}/aberta`);
    return response.data;
  },

  gerarProximoNumeroComanda: async (): Promise<number> => {
    const response = await api.get('/vendas/comanda/proximo-numero');
    return response.data;
  },

  /** Exclui (cancela) comanda em aberto. Exige justificativa e senha de cancelamento (PARAMETROS.SENHAD). Registra no histórico. */
  excluirComanda: async (nota: string, request: { justificativa: string; senha: string }): Promise<boolean> => {
    const response = await api.post(`/vendas/${encodeURIComponent(nota)}/excluir-comanda`, request);
    return response.data;
  },

  transferirItem: async (request: {
    notaOrigem: string;
    itemOrigem: number;
    notaDestino: string;
    operador: number;
  }): Promise<Venda> => {
    const response = await api.post('/vendas/transferir-item', request);
    return response.data;
  },

  imprimir: async (nota: string, request: {
    itens: Array<{codigo: number; descricao: string; quantidade: number; preco: number; observacao?: string}>;
    apenasNovosItens: boolean;
    comanda?: string;
    mesa?: string;
    clienteNome?: string;
    subtotal?: number;
    desconto?: number;
    acrescimo?: number;
    isExtrato?: boolean;
    /** Recibo do cliente: formas de pagamento e troco */
    isReciboCliente?: boolean;
    formasPagamento?: Array<{ descricao: string; valor: number }>;
    trocoTotal?: number;
    /** Incluir bloco delivery (endereço, telefone) no recibo */
    isCupomDelivery?: boolean;
    enderecoEntrega?: string;
    pontoReferencia?: string;
    telefoneEntrega?: string;
    observacoesPedido?: string;
    /** Forma de pagamento do delivery (ex: DINHEIRO, PIX, CARTÃO) */
    formaPgtoDelivery?: string;
    /** Indica se o pedido delivery já foi pago */
    jaPagoDelivery?: boolean;
    /** Imprime 2 cópias automaticamente (backend controla o loop) */
    imprimirDuasVias?: boolean;
    /** Título de seção exibido no cabeçalho do cupom (ex: "**** COZINHA ****") */
    tituloSecao?: string;
  }): Promise<boolean> => {
    try {
      const response = await api.post(`/vendas/${nota}/imprimir`, {
        itens: request.itens.map(item => ({
          codigo: item.codigo,
          descricao: item.descricao,
          quantidade: item.quantidade,
          preco: item.preco,
          observacao: item.observacao
        })),
        apenasNovosItens: request.apenasNovosItens,
        comanda: request.comanda,
        mesa: request.mesa,
        clienteNome: request.clienteNome,
        subtotal: request.subtotal,
        desconto: request.desconto,
        acrescimo: request.acrescimo,
        isExtrato: request.isExtrato || false,
        isReciboCliente: request.isReciboCliente || false,
        formasPagamento: request.formasPagamento,
        trocoTotal: request.trocoTotal,
        isCupomDelivery: request.isCupomDelivery || false,
        enderecoEntrega: request.enderecoEntrega,
        pontoReferencia: request.pontoReferencia,
        telefoneEntrega: request.telefoneEntrega,
        observacoesPedido: request.observacoesPedido,
        formaPgtoDelivery: request.formaPgtoDelivery,
        jaPagoDelivery: request.jaPagoDelivery ?? false,
        imprimirDuasVias: request.imprimirDuasVias ?? false,
        tituloSecao: request.tituloSecao
      });
      return response.data;
    } catch (error: any) {
      console.error('❌ [vendasService] Erro ao imprimir:', error?.message || error);
      throw error;
    }
  },

  getEmitente: async (): Promise<{ nome?: string; nomeFantasia?: string } | null> => {
    try {
      const response = await api.get('/vendas/emitente');
      return response.data;
    } catch {
      return null;
    }
  },
};

// Serviços para Clientes
export const clientesService = {
  buscar: async (params: { q?: string; ativo?: boolean; naoBloqueado?: boolean; pagina?: number; itensPorPagina?: number }): Promise<Cliente[]> => {
    const response = await api.get('/clientes/buscar', { params });
    const data = response.data;
    if (Array.isArray(data)) return data;
    console.warn('[clientesService.buscar] Resposta não é array:', typeof data, data);
    return [];
  },

  getById: async (id: number): Promise<Cliente> => {
    const response = await api.get(`/clientes/${id}`);
    return response.data;
  },

  getByCpfCnpj: async (cpfCnpj: string): Promise<Cliente> => {
    const response = await api.get(`/clientes/cpf-cnpj/${cpfCnpj}`);
    return response.data;
  },

  getByVendedor: async (idVendedor: number): Promise<Cliente[]> => {
    const response = await api.get(`/clientes/vendedor/${idVendedor}`);
    return response.data;
  },

  contar: async (params: { q?: string; ativo?: boolean; naoBloqueado?: boolean }): Promise<number> => {
    const response = await api.get('/clientes/contar', { params });
    return response.data;
  },

  verificar: async (cpfCnpjOuTelefone: string): Promise<{existe: boolean; cliente: Cliente | null; mensagem: string}> => {
    const response = await api.get(`/clientes/verificar/${cpfCnpjOuTelefone}`);
    return response.data;
  },

  cadastroRapido: async (dados: {
    nome: string;
    cpfCnpj?: string;
    telefone: string;
    celular?: string;
    fantasia?: string;
    endereco1?: string;
    numero1?: string;
    complemento1?: string;
    bairro1?: string;
    cidade1?: string;
    uf1?: string;
    cep1?: string;
    gravarNoCadastro?: boolean;
  }): Promise<Cliente> => {
    const response = await api.post('/clientes/cadastro-rapido', dados);
    return response.data;
  },

  criar: async (dados: any): Promise<Cliente> => {
    const response = await api.post('/clientes', dados);
    return response.data;
  },

  atualizar: async (id: number, dados: any): Promise<Cliente> => {
    const response = await api.put(`/clientes/${id}`, dados);
    return response.data;
  },

  excluir: async (id: number): Promise<void> => {
    await api.delete(`/clientes/${id}`);
  }
};

// Serviços para Emitente
export interface Emitente {
  id: number;
  nome?: string;
  nomeFantasia?: string;
  cnpj?: string;
  inscricaoEstadual?: string;
  endereco?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  uf?: string;
  cep?: string;
  telefone?: string;
  email?: string;
  site?: string;
}

export const emitenteService = {
  getEmitente: async (): Promise<Emitente | null> => {
    try {
      console.log('🔍 [emitenteService] Buscando emitente na API...');
      // Tentar primeiro o endpoint específico, depois o alternativo
      let response;
      try {
        response = await api.get('/emitente');
      } catch (error1: any) {
        if (error1.response?.status === 404) {
          console.log('⚠️ [emitenteService] Endpoint /emitente não encontrado, tentando /vendas/emitente...');
          response = await api.get('/vendas/emitente');
        } else {
          throw error1;
        }
      }
      console.log('✅ [emitenteService] Emitente recebido:', response.data);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        console.warn('⚠️ [emitenteService] Emitente não encontrado (404)');
        return null;
      }
      console.error('❌ [emitenteService] Erro ao buscar emitente:', error?.message || error);
      if (error.response) {
        console.error('   Status:', error.response.status);
        console.error('   Data:', error.response.data);
      }
      return null;
    }
  },

  saveEmitente: async (emitente: Partial<Emitente>): Promise<Emitente> => {
    const response = await api.put('/emitente', emitente);
    return response.data;
  },
};

// Serviços para Caixas
export const caixasService = {
  abrir: async (request: { numero: number; valorInicial: number; operador: number }): Promise<any> => {
    const response = await api.post('/caixas/abrir', request);
    return response.data;
  },

  fechar: async (request: { id: number; operador: number; valorFechamento: number; observacoes?: string }): Promise<any> => {
    // Corrige para enviar todos os campos esperados pelo backend
    const response = await api.post('/caixas/fechar', {
      id: request.id,
      operador: request.operador,
      valorFechamento: request.valorFechamento,
      observacoes: request.observacoes ?? ''
    });
    return response.data;
  },

  getAberto: async (numero: number): Promise<any> => {
    const response = await api.get(`/caixas/aberto/${numero}`);
    return response.data;
  },

  listar: async (dataInicio?: string, dataFim?: string): Promise<any[]> => {
    const response = await api.get('/caixas', { params: { dataInicio, dataFim } });
    return response.data;
  },

  getRelatorio: async (id: number, dataInicio?: string, dataFim?: string): Promise<any> => {
    const response = await api.get(`/caixas/${id}/relatorio`, { params: { dataInicio, dataFim } });
    return response.data;
  },
};

// Serviços para Movimentos de Caixa
export const caixaMovimentosService = {
  abrirCaixa: async (request: {
    terminal: number;
    valor: number;
    documento?: string;
    historico?: string;
    operador: number;
    custo?: number;
    conta?: number;
    codProf?: number;
    vendedor?: number;
  }): Promise<any> => {
    const response = await api.post('/caixas/movimentos/abertura', request);
    return response.data;
  },

  suprimento: async (request: {
    terminal: number;
    valor: number;
    documento?: string;
    historico?: string;
    operador: number;
    custo?: number;
    conta?: number;
    codProf?: number;
    vendedor?: number;
  }): Promise<any> => {
    const response = await api.post('/caixas/movimentos/suprimento', request);
    return response.data;
  },

  sangria: async (request: {
    terminal: number;
    valor: number;
    documento?: string;
    historico?: string;
    operador: number;
    custo?: number;
    conta?: number;
    codProf?: number;
    vendedor?: number;
  }): Promise<any> => {
    const response = await api.post('/caixas/movimentos/sangria', request);
    return response.data;
  },

  despesa: async (request: {
    terminal: number;
    valor: number;
    documento?: string;
    historico?: string;
    operador: number;
    custo?: number;
    conta?: number;
    codProf?: number;
    vendedor?: number;
  }): Promise<any> => {
    const response = await api.post('/caixas/movimentos/despesa', request);
    return response.data;
  },

  getResumo: async (terminal: number, dataInicio?: string, dataFim?: string): Promise<any> => {
    const response = await api.get(`/caixas/movimentos/resumo/${terminal}`, { params: { dataInicio, dataFim } });
    return response.data;
  },

  getMovimentos: async (terminal: number, dataInicio?: string, dataFim?: string): Promise<any[]> => {
    const response = await api.get(`/caixas/movimentos/${terminal}`, { params: { dataInicio, dataFim } });
    return response.data;
  },
};

// Serviços para Relatórios
export const relatoriosService = {
  getVendas: async (dataInicio?: string, dataFim?: string, formaPagamento?: string): Promise<any[] | { vendas?: any[] }> => {
    const response = await api.get('/relatorios/vendas', { params: { dataInicio, dataFim, formaPagamento } });
    return response.data;
  },

  /** origem: 'BA' = só comandas, 'DL' = só delivery, null/undefined = todos */
  getProdutosMaisVendidos: async (dataInicio?: string, dataFim?: string, limite?: number, origem?: string | null): Promise<any> => {
    const response = await api.get('/relatorios/produtos-mais-vendidos', { params: { dataInicio, dataFim, limite, origem } });
    return response.data;
  },

  getCliente: async (codigoCliente: number, dataInicio?: string, dataFim?: string): Promise<any> => {
    const response = await api.get(`/relatorios/cliente/${codigoCliente}`, { params: { dataInicio, dataFim } });
    return response.data;
  },

  getPeriodo: async (dataInicio: string, dataFim: string): Promise<any> => {
    const response = await api.get('/relatorios/periodo', { params: { dataInicio, dataFim } });
    return response.data;
  },

  /** Relatório consolidado: aberturas, vendas, recebimentos, saídas de caixa por período */
  getCaixaConsolidado: async (dataInicio: string, dataFim: string): Promise<any> => {
    const response = await api.get('/relatorios/caixa-consolidado', { params: { dataInicio, dataFim } });
    return response.data;
  },
  getDashboard: async (dataInicio: string, dataFim: string, origem?: string): Promise<any> => {
    const response = await api.get('/relatorios/dashboard', { params: { dataInicio, dataFim, origem } });
    return response.data;
  },

  getCancelamentos: async (de?: string, ate?: string, origem = 'BA'): Promise<any[]> => {
    const response = await api.get('/relatorios/cancelamentos', { params: { de, ate, origem } });
    return response.data;
  },

  getConsignacao: async (grupoId: number, dataInicio: string, dataFim: string): Promise<RelatorioConsignacao> => {
    const response = await api.get('/relatorios/consignacao', { params: { grupoId, dataInicio, dataFim } });
    return response.data;
  },
};

// Serviços para Formas de Pagamento (gestão)
export const formasPagamentoService = {
  listar: async (): Promise<any[]> => {
    const response = await api.get('/formas-pagamento');
    const d = response.data;
    return Array.isArray(d) ? d : (d?.$values ?? []);
  },
  atualizar: async (id: number, data: { descricao: string; indice?: number | null; meioPagto: number; permiteTroco: boolean; tipo?: string | null }): Promise<any> => {
    const response = await api.put(`/formas-pagamento/${id}`, data);
    return response.data;
  },
  toggleAtivo: async (id: number): Promise<{ id: number; ativo: boolean }> => {
    const response = await api.patch(`/formas-pagamento/${id}/ativo`);
    return response.data;
  },
  criar: async (data: { descricao: string; indice?: number | null; meioPagto: number; permiteTroco: boolean; tipo?: string | null }): Promise<any> => {
    const response = await api.post('/formas-pagamento', data);
    return response.data;
  },
};

// Serviços para Mesas
export const mesasService = {
  listar: async (): Promise<any[]> => {
    const response = await api.get('/mesas');
    return response.data;
  },

  getById: async (id: number): Promise<any> => {
    const response = await api.get(`/mesas/${id}`);
    return response.data;
  },

  liberar: async (numero: number): Promise<any> => {
    const response = await api.post(`/mesas/${numero}/liberar`);
    return response.data;
  },

  atualizarStatus: async (id: number, status: string): Promise<any> => {
    const response = await api.put(`/mesas/${id}/status`, { status });
    return response.data;
  },

  atualizarOcupacao: async (id: number, ocupada: boolean): Promise<any> => {
    const response = await api.put(`/mesas/${id}/ocupacao`, { ocupada });
    return response.data;
  },
};

// Serviços para Recebimentos
export const recebimentosService = {
  fecharComanda: async (request: {
    comanda?: number;
    /** Nota do cupom (Caixa Rápido / PDV). Quando informada, a venda é fechada pela nota. */
    nota?: string;
    recebimentos: Array<{
      idFormaPagamento: number;
      valor: number;
      troco?: number;
    }>;
    troco?: number;
  }): Promise<any> => {
    console.log('🔄 [api.ts] Chamando fecharComanda:', {
      comanda: request.comanda,
      recebimentos: request.recebimentos,
      troco: request.troco,
      url: '/recebimentos/fechar-comanda',
      baseURL: API_BASE_URL
    });
    try {
      const response = await api.post('/recebimentos/fechar-comanda', request);
      console.log('✅ [api.ts] fecharComanda sucesso:', response.data);
      return response.data;
    } catch (error: any) {
      console.error('❌ [api.ts] Erro em fecharComanda:', {
        message: error.message,
        status: error.response?.status,
        statusText: error.response?.statusText,
        data: error.response?.data,
        url: error.config?.url,
        baseURL: error.config?.baseURL
      });
      throw error;
    }
  },

  getFormasPagamento: async (): Promise<any[]> => {
    const response = await api.get('/recebimentos/formas-pagamento');
    return response.data;
  },

  getRecebimentosPorNota: async (nota: string): Promise<any[]> => {
    const response = await api.get(`/recebimentos/nota/${nota}`);
    return response.data;
  },

  getRecebimentosPorPeriodo: async (dataInicio?: string, dataFim?: string): Promise<any[]> => {
    const response = await api.get('/recebimentos/periodo', {
      params: { dataInicio, dataFim }
    });
    return response.data;
  },
};

// Serviços para Contas a Receber
export const receberService = {
  /** Verifica se o cliente possui contas a receber em aberto (valor devido) */
  verificarContasAberto: async (codigoCliente: number): Promise<{ temContasAberto: boolean; valorTotalPendente: number; quantidadeContas: number; mensagem: string }> => {
    const response = await api.get(`/receber/contas-aberto/${codigoCliente}`);
    return response.data;
  },

  buscar: async (numero: string, ordem: string): Promise<any> => {
    const response = await api.get(`/receber/${numero}/${ordem}`);
    return response.data;
  },

  listarPendentes: async (codigoCliente?: number, dataVencimentoInicio?: string, dataVencimentoFim?: string): Promise<any[]> => {
    const response = await api.get('/receber/pendentes', { params: { codigoCliente, dataVencimentoInicio, dataVencimentoFim } });
    return response.data;
  },

  listarPorCliente: async (codigoCliente: number, apenasPendentes?: boolean): Promise<any[]> => {
    const response = await api.get(`/receber/cliente/${codigoCliente}`, { params: { apenasPendentes } });
    return response.data;
  },

  quitar: async (request: { 
    numero: string; 
    ordem: string; 
    valorRecebido: number; 
    dataRecebimento?: string; 
    desconto?: number; 
    juros?: number; 
    idFormaPagamento?: number;
    formasPagamento?: Array<{ idFormaPagamento: number; valor: number }>;
  }): Promise<any> => {
    const response = await api.post('/receber/quitar', request);
    return response.data;
  },
};

// Serviços para Histórico
export const historicoService = {
  listar: async (entidade?: string, idEntidade?: number, dataInicio?: string, dataFim?: string): Promise<any[]> => {
    const response = await api.get('/historico', { params: { entidade, idEntidade, dataInicio, dataFim } });
    return response.data;
  },
};

// Serviços para Notificações
export const notificacoesService = {
  listar: async (apenasNaoLidas?: boolean, categoria?: string): Promise<any[]> => {
    const response = await api.get('/notificacoes', { params: { apenasNaoLidas, categoria } });
    return response.data;
  },

  getQuantidadeNaoLidas: async (): Promise<number> => {
    const response = await api.get('/notificacoes/quantidade-nao-lidas');
    return response.data;
  },

  marcarComoLida: async (id: number): Promise<void> => {
    await api.put(`/notificacoes/${id}/marcar-lida`);
  },

  marcarTodasComoLidas: async (): Promise<void> => {
    await api.put('/notificacoes/marcar-todas-lidas');
  },
};

// Serviços para WhatsApp
export const whatsAppService = {
  enviarMensagem: async (telefone: string, mensagem: string): Promise<{ sucesso: boolean; mensagem: string; link?: string; metodo?: string }> => {
    // Timeout de 20 segundos - se Evolution não responder, mostramos instruções
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 20000);
    
    try {
      const response = await api.post('/whatsapp/enviar', {
        telefone,
        mensagem
      }, {
        signal: controller.signal
      });
      clearTimeout(timeoutId);
      return response.data;
    } catch (error: any) {
      clearTimeout(timeoutId);
      const msg = error.response?.data?.mensagem || error.message;
      if (error.name === 'AbortError' || error.code === 'ECONNABORTED') {
        throw new Error('Tempo de espera excedido. WhatsApp não respondeu. Abra as instruções na tela para conectar.');
      }
      if (error.response?.status === 504 || error.response?.status === 503) {
        throw new Error(msg || 'WhatsApp não está disponível. Veja as instruções na tela para conectar.');
      }
      throw new Error(msg || 'Não foi possível enviar.');
    }
  },

  verificarStatus: async (): Promise<{ conectado: boolean }> => {
    const response = await api.get('/whatsapp/status');
    return response.data;
  },

  /** QR Code da Evolution API (Baileys). Escaneie uma vez; depois as mensagens são enviadas direto. */
  getQrCode: async (): Promise<{ base64?: string; conectado?: boolean; erro?: string }> => {
    const response = await api.get('/whatsapp/qrcode');
    return response.data;
  },

  /** Verifica se a instância Evolution está conectada (envio direto ativo). */
  getConectado: async (): Promise<{ conectado: boolean }> => {
    const response = await api.get('/whatsapp/conectado');
    return response.data;
  },

  inicializar: async (): Promise<{ mensagem: string }> => {
    const response = await api.post('/whatsapp/inicializar');
    return response.data;
  },

  resetarSessao: async (): Promise<{ sucesso: boolean; mensagem: string }> => {
    const response = await api.post('/whatsapp/reset');
    return response.data;
  },
};

// =============================================
// CONFIGURAÇÕES DO SISTEMA
// =============================================
/* ============================================================
   MÓDULO FORÇA DE VENDAS
   ============================================================ */
export const forcaVendasService = {
  // VENDEDORES
  getVendedores: async (params?: { q?: string; ativo?: boolean }) =>
    (await api.get('/forcavendas/vendedores', { params })).data,
  getVendedoresAtivos: async () =>
    (await api.get('/forcavendas/vendedores/ativos')).data,
  getVendedor: async (id: number) =>
    (await api.get(`/forcavendas/vendedores/${id}`)).data,
  criarVendedor: async (dados: any) =>
    (await api.post('/forcavendas/vendedores', dados)).data,
  atualizarVendedor: async (id: number, dados: any) =>
    (await api.put(`/forcavendas/vendedores/${id}`, dados)).data,
  alterarStatusVendedor: async (id: number, ativo: boolean) =>
    (await api.patch(`/forcavendas/vendedores/${id}/status`, null, { params: { ativo } })).data,
  alterarSenhaVendedor: async (id: number, novaSenha: string) =>
    (await api.patch(`/forcavendas/vendedores/${id}/senha`, { novaSenha })).data,

  // PEDIDOS
  getPedidos: async (params?: any) =>
    (await api.get('/forcavendas/pedidos', { params })).data,
  getPedidosPendentes: async () =>
    (await api.get('/forcavendas/pedidos/pendentes')).data,
  getPedidosPorVendedor: async (idVendedor: number, status?: number) =>
    (await api.get(`/forcavendas/pedidos/vendedor/${idVendedor}`, { params: { status } })).data,
  getPedido: async (id: number) =>
    (await api.get(`/forcavendas/pedidos/${id}`)).data,
  criarPedido: async (dados: any) =>
    (await api.post('/forcavendas/pedidos', dados)).data,
  atualizarStatusPedido: async (id: number, dados: { status: number; motivo?: string; notaFiscal?: string; idAprovador?: number }) =>
    (await api.patch(`/forcavendas/pedidos/${id}/status`, dados)).data,

  // ROTAS / VISITAS
  getVisitas: async (idVendedor: number, params?: { dataInicio?: string; dataFim?: string }) =>
    (await api.get(`/forcavendas/rotas/vendedor/${idVendedor}`, { params })).data,
  getAgendaHoje: async (idVendedor: number) =>
    (await api.get(`/forcavendas/rotas/vendedor/${idVendedor}/hoje`)).data,
  agendarVisita: async (dados: any) =>
    (await api.post('/forcavendas/rotas', dados)).data,
  checkin: async (id: number, dados?: { latitude?: number; longitude?: number; obs?: string }) =>
    (await api.post(`/forcavendas/rotas/${id}/checkin`, dados ?? {})).data,
  concluirVisita: async (id: number, dados: any) =>
    (await api.post(`/forcavendas/rotas/${id}/concluir`, dados)).data,
  marcarNaoRealizada: async (id: number, obs?: string) =>
    (await api.post(`/forcavendas/rotas/${id}/nao-realizada`, null, { params: { obs } })).data,

  // DASHBOARD
  getDashboard: async (idVendedor: number) =>
    (await api.get(`/forcavendas/dashboard/vendedor/${idVendedor}`)).data,
  getRanking: async (mes?: number, ano?: number) =>
    (await api.get('/forcavendas/dashboard/ranking', { params: { mes, ano } })).data,
  getMetas: async (idVendedor: number) =>
    (await api.get(`/forcavendas/dashboard/vendedor/${idVendedor}/metas`)).data,
  definirMeta: async (dados: { idVendedor: number; mes: number; ano: number; valorMeta: number }) =>
    (await api.post('/forcavendas/dashboard/metas', dados)).data,
};

export const configuracoesService = {
  /** Retorna as configurações gerais do sistema. */
  getSistema: async (): Promise<{ usarDelivery: boolean; usarForcaVendas: boolean; usarComanda: boolean; habilitarImprimirDuasVias: boolean; usarCozinha: boolean; usarCardapio: boolean }> => {
    const response = await api.get('/configuracoes/sistema');
    return response.data;
  },

  /** Salva as configurações gerais do sistema. */
  putSistema: async (dados: { usarDelivery: boolean; usarForcaVendas: boolean; usarComanda: boolean; habilitarImprimirDuasVias: boolean; usarCozinha: boolean; usarCardapio: boolean }): Promise<{ mensagem: string }> => {
    const response = await api.put('/configuracoes/sistema', dados);
    return response.data;
  },

  /** Realiza backup do banco de dados, compacta em ZIP e envia por email se configurado. */
  fazerBackup: async (): Promise<{
    mensagem: string;
    arquivo: string;
    caminho: string;
    tamanhoMb: number;
    emailEnviado: boolean;
    emailDestino?: string;
    emailErro?: string;
  }> => {
    const response = await api.post('/configuracoes/backup');
    return response.data;
  },

  /** Retorna as configurações de email para backup. */
  getBackupEmail: async (): Promise<{
    smtpHost: string;
    smtpPort: number;
    smtpUseSsl: boolean;
    usuario: string;
    senhaCadastrada: boolean;
    remetente: string;
    nomeRemetente: string;
    destinatario: string;
  }> => {
    const response = await api.get('/configuracoes/backup-email');
    return response.data;
  },

  /** Salva as configurações de email para backup. */
  putBackupEmail: async (dados: {
    smtpHost: string;
    smtpPort: number;
    smtpUseSsl: boolean;
    usuario: string;
    senha?: string;
    remetente: string;
    nomeRemetente: string;
    destinatario: string;
  }): Promise<{ mensagem: string }> => {
    const response = await api.put('/configuracoes/backup-email', dados);
    return response.data;
  },
};

// KDS - Kitchen Display System
export const kdsService = {
  getPedidos: async (): Promise<KdsPedido[]> => {
    const response = await api.get('/kds/pedidos');
    return response.data;
  },

  atualizarStatus: async (nota: string, statusCozinha: string): Promise<void> => {
    await api.put(`/kds/pedidos/${nota}/status`, { statusCozinha });
  },
};

export interface UsuarioDto {
  id: number;
  nome: string;
  ativo: boolean;
  bloqueado: boolean;
  tipo: string;
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
}

export interface CreateUsuarioRequest {
  nome: string;
  senha: string;
  ativo: boolean;
  bloqueado: boolean;
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
  tipo: string;
}

export interface UpdateUsuarioRequest {
  nome: string;
  ativo: boolean;
  bloqueado: boolean;
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
  tipo: string;
}

export const usuariosService = {
  listar: async (): Promise<UsuarioDto[]> => {
    const { data } = await api.get('/usuarios');
    return data;
  },
  buscarPorId: async (id: number): Promise<UsuarioDto> => {
    const { data } = await api.get(`/usuarios/${id}`);
    return data;
  },
  criar: async (req: CreateUsuarioRequest): Promise<UsuarioDto> => {
    const { data } = await api.post('/usuarios', req);
    return data;
  },
  atualizar: async (id: number, req: UpdateUsuarioRequest): Promise<UsuarioDto> => {
    const { data } = await api.put(`/usuarios/${id}`, req);
    return data;
  },
  alterarSenha: async (id: number, novaSenha: string): Promise<void> => {
    await api.put(`/usuarios/${id}/senha`, { novaSenha });
  },
  excluir: async (id: number): Promise<void> => {
    await api.delete(`/usuarios/${id}`);
  },
};

export default api;
