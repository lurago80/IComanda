// Tipos para a API
export interface Grupo {
  id: number;
  descricao: string;
  quantidadeProdutos: number;
  imprimirDuasVias: boolean;
}

export interface TaxaEntrega {
  id: number;
  descricao: string;
  valor: number;
}

export interface Cliente {
  id: number;
  nome: string;
  cpfCnpj?: string;
  rgIe?: string;
  telefone?: string;
  celular?: string;
  email?: string;
  email1?: string;
  endereco1?: string;
  numero1?: string;
  complemento1?: string;
  bairro1?: string;
  cidade1?: string;
  uf1?: string;
  cep1?: string;
  endereco2?: string;
  numero2?: string;
  complemento2?: string;
  bairro2?: string;
  cidade2?: string;
  uf2?: string;
  cep2?: string;
  fantasia?: string;
  ativo: boolean;
  bloqueado: boolean;
  dataCadastro?: string;
  dataNascimento?: string;
  limite?: number;
  classificacao?: string;
  obs?: string;
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

export interface ProdutoCompleto {
  id: number;
  codigoBarra?: string;
  codigoInterno?: string;
  descricao: string;
  caracteristica?: string;
  quantidade?: number;
  quantidadeMinima?: number;
  quantidadeMaxima?: number;
  localizacao?: string;
  precoCusto?: number;
  precoVenda?: number;
  precoCustoMedio?: number;
  atacado?: number;
  preco3?: number;
  precoDolar?: number;
  percentual?: number;
  unMedida?: string;
  ativo?: number;
  grupo?: number;
  pesavel?: number;
  marca?: string;
  categoria?: string;
  cor?: string;
  tamanho?: string;
  cfop?: string;
  csosn?: string;
  cest?: string;
  ncm?: string;
  cst?: string;
  cstOrigem?: string;
  icms?: number;
  margem?: number;
  dataInclusao?: string;
  dataUltimaVenda?: string;
  dataUltimaCompra?: string;
  observacao?: string;
  tipo?: string;
  fabricante?: string;
  // ... outros campos conforme necessário
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

export interface ItemVenda {
  nota: string;
  item: number;
  codigo: number;
  barras: string;
  qtd: number;
  preco: number;
  desconto: number;
  acrescimo: number;
  total: number;
  und: string;
  descricao?: string;
  precoCusto?: number;
  /** Data/hora de emissão do item (itens temporários). ISO string. */
  emissao?: string;
}

/** Informações de contas a receber em aberto do cliente (cliente deve) */
export interface ContasAberto {
  temContasAberto: boolean;
  valorTotalPendente: number;
  quantidadeContas: number;
  mensagem: string;
}

export interface Venda {
  nota: string;
  modelo: string;
  serie: string;
  subserie: string;
  /** BA = balcão, DL = delivery. Preenchido pela API. */
  origem?: string;
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
  nomeCliente?: string;
  telefoneCliente?: string;
  /** Endereço de entrega (preenchido para vendas delivery na reimpressão). */
  enderecoEntrega?: string;
  /** Ponto de referência do cliente (COMPL1), para reimpressão delivery. */
  pontoReferencia?: string;
  /** Nome fantasia do estabelecimento (preenchido pelo backend via getByNota). */
  nomeEstabelecimento?: string;
  /** Forma(s) de pgto associada à venda (ex: PIX, DINHEIRO). */
  formasPgto?: string;
  itens?: ItemVenda[];
  operador?: number;
  lancado?: string;
  /** Preenchido quando o cliente possui valor em aberto (contas a receber) */
  contasAberto?: ContasAberto;
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
  nomeCliente?: string;
  cpfCnpjCliente?: string;
  operador: number;
  vendedor: number;
  mesa?: number;
  comanda?: number;
  numeroPessoas?: number;
  /** BA = balcão/comanda, DL = delivery. Se omitido, usa BA. */
  origem?: string;
  itens: Array<{
    codigo: number;
    qtd: number;
    preco: number;
    desconto?: number;
    acrescimo?: number;
  }>;
}

// Tipos para relatório por período
export interface ItemVendidoDto {
  nota: string;
  emissao: string;
  hora: string;
  item: number;
  codigoProduto: number;
  descricaoProduto?: string;
  barras: string;
  und: string;
  qtd: number;
  preco: number;
  desconto: number;
  acrescimo: number;
  total: number;
  cliente: number;
  nomeCliente?: string;
  mesa?: number;
  comanda?: number;
}

export interface RecebimentoDetalheDto {
  id: number;
  nota: string;
  dataVenda: string;
  horaVenda: string;
  valor: number;
  troco: number;
  nCaixa: number;
  cliente: number;
  nomeCliente?: string;
}

export interface RecebimentoPorFormaPagamentoDto {
  idFormaPagamento: number;
  formaPagamento: string;
  quantidade: number;
  valorTotal: number;
  trocoTotal: number;
  detalhes: RecebimentoDetalheDto[];
}

export interface ResumoRelatorioPeriodoDto {
  totalItensVendidos: number;
  valorTotalItens: number;
  totalDesconto: number;
  totalAcrescimo: number;
  totalRecebimentos: number;
  valorTotalRecebimentos: number;
  valorTotalTroco: number;
  totalPorFormaPagamento: { [key: string]: number };
}

export interface RelatorioPeriodoDto {
  dataInicio: string;
  dataFim: string;
  itensVendidos: ItemVendidoDto[];
  recebimentosPorFormaPagamento: RecebimentoPorFormaPagamentoDto[];
  resumo: ResumoRelatorioPeriodoDto;
}

// Tipos para contas em aberto
export interface ContasAbertoDto {
  temContasAberto: boolean;
  valorTotalPendente: number;
  quantidadeContas: number;
  mensagem: string;
}

/* ============================================================
   MÓDULO FORÇA DE VENDAS
   ============================================================ */

export interface Vendedor {
  id: number;
  nome: string;
  email?: string;
  celular?: string;
  ativo: boolean;
  comissaoPerc: number;
  metaMensal: number;
  regiao?: string;
  obs?: string;
  dataCadastro?: string;
  vendasMesAtual: number;
  percentualMeta: number;
}

export interface ItemPedidoFV {
  id: number;
  idPedidoFV: number;
  idProduto: number;
  codigoProduto: string;
  descricaoProduto: string;
  quantidade: number;
  unidade: string;
  precoUnitario: number;
  desconto: number;
  total: number;
  obs?: string;
}

export interface PedidoFV {
  id: number;
  idVendedor: number;
  nomeVendedor: string;
  idCliente: number;
  nomeCliente: string;
  dataPedido: string;
  horaPedido: string;
  status: string;
  statusCodigo: number;
  subtotal: number;
  desconto: number;
  acrescimo: number;
  total: number;
  obs?: string;
  condicaoPgto?: string;
  tabelaPreco: number;
  dataAprovacao?: string;
  nomeAprovador?: string;
  motivoCancel?: string;
  notaFiscal?: string;
  dataFaturamento?: string;
  itens: ItemPedidoFV[];
}

export interface VisitaFV {
  id: number;
  idVendedor: number;
  nomeVendedor: string;
  idCliente: number;
  nomeCliente: string;
  dataAgendada: string;
  dataCheckin?: string;
  dataCheckout?: string;
  status: string;
  statusCodigo: number;
  obs?: string;
  resultado?: string;
  idPedidoFV?: number;
}

export interface MetaFV {
  id: number;
  idVendedor: number;
  nomeVendedor: string;
  mes: number;
  ano: number;
  valorMeta: number;
  valorRealizado: number;
  percentualAtingido: number;
  metaAtingida: boolean;
}

export interface DashboardVendedor {
  idVendedor: number;
  nomeVendedor: string;
  metaMes: number;
  vendasMes: number;
  percentualMeta: number;
  comissaoEstimada: number;
  totalPedidosPendentes: number;
  totalPedidosAprovados: number;
  totalPedidosFaturados: number;
  valorPedidosPendentes: number;
  valorPedidosAprovados: number;
  visitasAgendadasHoje: number;
  visitasRealizadasHoje: number;
  proximasVisitas: VisitaFV[];
  ultimosPedidos: PedidoFV[];
}

/** Venda fechada/recebida no período (reimpressão de recibos). */
export interface VendaFechadaRecibo {
  nota: string;
  comanda?: number;
  total: number;
  emissao: string;
  nomeCliente?: string;
  recebimentos: Array<{
    id: number;
    idFormaPagamento: number;
    formaPagamentoDescricao: string;
    nCaixa: number;
    nota: string;
    valor: number;
    troco: number;
  }>;
}
