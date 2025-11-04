// Tipos para a API
export interface Grupo {
  id: number;
  descricao: string;
  codGrupo: number;
  quantidadeProdutos: number;
}

export interface Cliente {
  id: number;
  nome: string;
  cpfCnpj?: string;
  telefone?: string;
  celular?: string;
  email?: string;
  endereco1?: string;
  cidade1?: string;
  uf1?: string;
  cep1?: string;
  fantasia?: string;
  ativo: boolean;
  bloqueado: boolean;
  dataCadastro?: string;
  limite?: number;
  classificacao?: string;
  idVendedor: number;
  nomeCompleto: string;
  documento: string;
  contato: string;
  enderecoCompleto: string;
}

export interface Produto {
  id: number;
  codigoBarra: string;
  codigoInterno: string;
  descricao: string;
  caracteristica: string;
  quantidade: number;
  precoCusto: number;
  precoVenda: number;
  atacado: number;
  preco3: number;
  unMedida: string;
  ativo: boolean;
  grupo: number;
  pesavel: boolean;
}

export interface ItemCarrinho {
  id: number;
  cupom: string;
  operador: number;
  item: number;
  codigo: number;
  barras: string;
  descricao: string;
  qtd: number;
  preco: number;
  desconto: number;
  acrescimo: number;
  total: number;
  und: string;
  tipo: number;
  data: string;
  hora: string;
  precoCusto: number;
  serial: string;
  icms: number;
  sinalm: number;
}

export interface Venda {
  nota: string;
  modelo: string;
  serie: string;
  subserie: string;
  origem: string;
  emissao: string;
  hora: string;
  cliente: number;
  vendedor: number;
  total: number;
  desconto: number;
  acrescimo: number;
  subtotal: number;
  mesa: number;
  comanda: number;
  numeroPessoas: number;
}

// Tipos para requests
export interface BuscarProdutoRequest {
  q?: string;
  ativo?: boolean;
  grupo?: number;
  pagina?: number;
  itensPorPagina?: number;
}

export interface CriarVendaRequest {
  cliente: number;
  vendedor: number;
  mesa?: number;
  comanda?: number;
  numeroPessoas?: number;
  itens: Array<{
    codigo: number;
    qtd: number;
    preco: number;
    desconto?: number;
    acrescimo?: number;
  }>;
}
