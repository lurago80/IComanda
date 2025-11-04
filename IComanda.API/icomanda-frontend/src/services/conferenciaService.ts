import axios from 'axios';

const API_URL = 'http://localhost:65375/api';

export interface ItemConferencia {
  descricao: string;
  qtd: number;
  precoUnitario: number;
  total: number;
}

export interface ConferenciaMesa {
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
}

export const conferenciaService = {
  async getConferenciaMesa(mesa: number): Promise<ConferenciaMesa> {
    const response = await axios.get(`${API_URL}/vendas/conferencia/mesa/${mesa}`);
    return response.data;
  },

  async getConferenciaComanda(comanda: number): Promise<ConferenciaMesa> {
    const response = await axios.get(`${API_URL}/vendas/conferencia/comanda/${comanda}`);
    return response.data;
  }
};

