import api from './api';

export interface ItemConferencia {
  codigo: number;
  descricao: string;
  qtd: number;
  precoUnitario: number;
  total: number;
  observacao?: string;
  /** Hora em que o item foi lançado (HH:mm:ss) - para conferência */
  horaLancamento?: string;
}

export interface ClienteConferencia {
  id: number;
  nome: string;
  cpfCnpj?: string;
  telefone?: string;
}

export interface ConferenciaMesa {
  nota: string;
  mesa: number | null;
  comanda: number | null;
  garcom: string | null;
  dataHora: string;
  itens: ItemConferencia[];
  subtotal: number;
  desconto: number;
  acrescimo: number;
  total: number;
  totalItens: number;
  cliente?: ClienteConferencia;
}

export const conferenciaService = {
  async getConferenciaMesa(mesa: number): Promise<ConferenciaMesa> {
    const response = await api.get(`/vendas/conferencia/mesa/${mesa}`);
    return response.data;
  },

  async getConferenciaComanda(comanda: number): Promise<ConferenciaMesa> {
    const response = await api.get(`/vendas/conferencia/comanda/${comanda}`);
    return response.data;
  }
};

