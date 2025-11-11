import axios from 'axios';
import { BuscarProdutoRequest, Cliente, CriarVendaRequest, Grupo, Produto, Venda } from '../types/api';

const API_BASE_URL = 'http://localhost:65375/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Serviços para Grupos
export const gruposService = {
  getAll: async (): Promise<Grupo[]> => {
    const response = await api.get('/grupos');
    return response.data;
  },

  getTodosComQuantidade: async (): Promise<Grupo[]> => {
    const response = await api.get('/grupos/todos-com-quantidade');
    return response.data;
  },

  getById: async (id: number): Promise<Grupo> => {
    const response = await api.get(`/grupos/${id}`);
    return response.data;
  },
};

// Serviços para Produtos
export const produtosService = {
  buscar: async (request: BuscarProdutoRequest): Promise<Produto[]> => {
    const response = await api.get('/produtos/buscar', { params: request });
    return response.data;
  },

  getByGrupo: async (grupoId: number, ativo: boolean = true): Promise<Produto[]> => {
    const response = await api.get(`/produtos/grupo/${grupoId}`, {
      params: { ativo }
    });
    return response.data;
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
};

// Serviços para Vendas
export const vendasService = {
  criar: async (request: CriarVendaRequest): Promise<Venda> => {
    const response = await api.post('/vendas', request);
    return response.data;
  },

  getByNota: async (nota: string): Promise<Venda> => {
    const response = await api.get(`/vendas/${nota}`);
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
};

// Serviços para Clientes
export const clientesService = {
  buscar: async (params: { q?: string; ativo?: boolean; naoBloqueado?: boolean; pagina?: number; itensPorPagina?: number }): Promise<Cliente[]> => {
    const response = await api.get('/clientes/buscar', { params });
    return response.data;
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

  cadastroRapido: async (dados: {nome: string; cpfCnpj: string; telefone: string; celular?: string; fantasia?: string}): Promise<Cliente> => {
    const response = await api.post('/clientes/cadastro-rapido', dados);
    return response.data;
  }
};

export default api;
