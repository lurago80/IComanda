import React, { useState } from 'react';
import { BarChart3, ArrowLeft, FileText, TrendingUp, User, Calendar, Download, Printer, Search, Wallet, ArrowDownCircle, ArrowUpCircle } from 'lucide-react';
import { Button } from '../components/ui/button';
import { relatoriosService, clientesService } from '../services/api';
import { useToast } from '../hooks/useToast';
import ClienteSearch from '../components/ClienteSearch';
import { exportToExcel, exportToExcelMultiSection } from '../utils/exportExcel';

interface RelatoriosPageProps {
  onClose: () => void;
  onRelatorioRecebimentos?: () => void;
  onRelatorioPeriodo?: () => void;
  onReimpressaoRecibos?: () => void;
}

type TipoRelatorio = 'vendas' | 'produtos' | 'cliente' | 'caixa';

const RelatoriosPage: React.FC<RelatoriosPageProps> = ({ onClose, onRelatorioRecebimentos, onRelatorioPeriodo, onReimpressaoRecibos }) => {
  const [tipoRelatorio, setTipoRelatorio] = useState<TipoRelatorio>('vendas');
  const [dataInicio, setDataInicio] = useState<string>(
    new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0]
  );
  const [dataFim, setDataFim] = useState<string>(new Date().toISOString().split('T')[0]);
  const [loading, setLoading] = useState(false);
  const [dadosVendas, setDadosVendas] = useState<any[]>([]);
  const [resumoVendas, setResumoVendas] = useState<{
    totalVendasComandas?: number;
    valorTotalComandas?: number;
    totalVendasDelivery?: number;
    valorTotalDelivery?: number;
  } | null>(null);
  const [dadosProdutos, setDadosProdutos] = useState<any[]>([]);
  const [filtroOrigemProdutos, setFiltroOrigemProdutos] = useState<string>('');
  const [dadosCliente, setDadosCliente] = useState<any>(null);
  const [dadosCaixaConsolidado, setDadosCaixaConsolidado] = useState<any>(null);
  const [codigoCliente, setCodigoCliente] = useState<string>('');
  const [nomeClienteSelecionado, setNomeClienteSelecionado] = useState<string>('');
  const [showClienteSearch, setShowClienteSearch] = useState(false);
  const { showError, showSuccess } = useToast();

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  };

  const formatarData = (data: string) => {
    return new Date(data).toLocaleDateString('pt-BR');
  };

  const temDadosRelatorio =
    (tipoRelatorio === 'vendas' && dadosVendas.length > 0) ||
    (tipoRelatorio === 'produtos' && dadosProdutos.length > 0) ||
    (tipoRelatorio === 'cliente' && dadosCliente != null) ||
    (tipoRelatorio === 'caixa' && dadosCaixaConsolidado != null);

  const handleImprimir = () => {
    window.print();
  };

  const handleExportarExcel = () => {
    const baseNome = `relatorio-${tipoRelatorio}-${dataInicio}-${dataFim}`;
    if (tipoRelatorio === 'vendas' && dadosVendas.length > 0) {
      const rows = dadosVendas.map((item: any) => ({
        data: item.data || item.Data || '',
        formaPagamento: item.formaPagamento ?? item.FormaPagamento ?? '',
        quantidade: item.quantidade ?? item.quantidadeItens ?? item.QuantidadeItens ?? 0,
        valorTotal: item.valorTotal ?? item.total ?? item.Total ?? 0
      }));
      exportToExcel(rows, `${baseNome}.csv`, [
        { key: 'data', label: 'Data' },
        { key: 'formaPagamento', label: 'Forma de Pagamento' },
        { key: 'quantidade', label: 'Quantidade' },
        { key: 'valorTotal', label: 'Valor Total' }
      ]);
      return;
    }
    if (tipoRelatorio === 'produtos' && dadosProdutos.length > 0) {
      const rows = dadosProdutos.map((item: any, i: number) => ({
        posicao: item.posicao ?? i + 1,
        descricao: item.descricao ?? item.Descricao ?? '',
        quantidade: item.quantidadeVendida ?? item.QuantidadeVendida ?? item.quantidadeTotal ?? 0,
        valorTotal: item.valorTotal ?? item.ValorTotal ?? 0
      }));
      exportToExcel(rows, `${baseNome}.csv`, [
        { key: 'posicao', label: '#' },
        { key: 'descricao', label: 'Produto' },
        { key: 'quantidade', label: 'Quantidade' },
        { key: 'valorTotal', label: 'Valor Total' }
      ]);
      return;
    }
    if (tipoRelatorio === 'cliente' && dadosCliente) {
      const compras = dadosCliente.compras ?? dadosCliente.Compras ?? [];
      if (compras.length > 0) {
        const rows = compras.map((c: any) => ({
          data: c.data ?? c.Data ?? '',
          nota: c.nota ?? c.Nota ?? '',
          total: c.total ?? c.Total ?? 0
        }));
        exportToExcel(rows, `${baseNome}.csv`, [
          { key: 'data', label: 'Data' },
          { key: 'nota', label: 'Nota' },
          { key: 'total', label: 'Total' }
        ]);
      }
      return;
    }
    if (tipoRelatorio === 'caixa' && dadosCaixaConsolidado) {
      const fmtMoeda = (v: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v ?? 0);
      const fmtData = (d: string | Date) => d ? new Date(d).toLocaleDateString('pt-BR') : '';
      const fmtDataHora = (d: string | Date, h?: string) => {
        if (!d) return '';
        const dt = new Date(d);
        const hora = h ? String(h).substring(0, 8) : '';
        return hora ? `${dt.toLocaleDateString('pt-BR')} ${hora}` : dt.toLocaleDateString('pt-BR');
      };
      const resumo = dadosCaixaConsolidado.resumo ?? {};
      const rows: (string | number)[][] = [];

      rows.push(['CAIXA CONSOLIDADO']);
      rows.push(['Período:', `${fmtData(dadosCaixaConsolidado.dataInicio)} a ${fmtData(dadosCaixaConsolidado.dataFim)}`]);
      rows.push([]);

      rows.push(['RESUMO']);
      rows.push(['Aberturas', fmtMoeda(resumo.totalAbertura ?? 0), `${resumo.quantidadeAberturas ?? 0} movimento(s)`]);
      rows.push(['Vendas (total)', fmtMoeda(resumo.totalVendas ?? 0), `${resumo.quantidadeVendas ?? 0} venda(s)`]);
      rows.push(['Comandas', `${resumo.quantidadeVendasComandas ?? 0} venda(s)`, fmtMoeda(resumo.totalVendasComandas ?? 0)]);
      rows.push(['Delivery', `${resumo.quantidadeVendasDelivery ?? 0} venda(s)`, fmtMoeda(resumo.totalVendasDelivery ?? 0)]);
      rows.push(['Pag. ao fechar (fechamento comanda)', fmtMoeda(resumo.totalRecebimentosVendas ?? 0), `${resumo.quantidadeRecebimentosVendas ?? 0} pagamento(s)`]);
      rows.push(['Recebimentos (contas a receber)', fmtMoeda(resumo.totalRecebimentosContasReceber ?? 0), `${resumo.quantidadeRecebimentosContasReceber ?? 0} quitamento(s)`]);
      rows.push(['Saídas', fmtMoeda(resumo.totalSaidas ?? 0), `${resumo.quantidadeSaidas ?? 0} saída(s)`]);
      rows.push(['Saldo período', fmtMoeda(resumo.saldoPeriodo ?? 0)]);
      rows.push([]);

      const totVendas = (resumo.totalPorFormaRecebimentosVendas ?? []) as { forma: string; valor: number }[];
      if (totVendas.length > 0) {
        rows.push(['Total por espécie – Pag. ao fechar comanda']);
        totVendas.forEach((item: { forma: string; valor: number }) => {
          rows.push([item.forma || 'Outros', fmtMoeda(item.valor ?? 0)]);
        });
        rows.push([]);
      }
      const totContas = (resumo.totalPorFormaRecebimentosContas ?? []) as { forma: string; valor: number }[];
      if (totContas.length > 0) {
        rows.push(['Total por espécie – Recebimentos (contas a receber)']);
        totContas.forEach((item: { forma: string; valor: number }) => {
          rows.push([item.forma || 'Outros', fmtMoeda(item.valor ?? 0)]);
        });
        rows.push([]);
      }

      const aberturas = dadosCaixaConsolidado.aberturas ?? [];
      if (aberturas.length > 0) {
        rows.push(['Aberturas de caixa']);
        rows.push(['Data/Hora', 'Terminal', 'Valor']);
        aberturas.forEach((a: any) => {
          rows.push([fmtDataHora(a.data, a.hora), String(a.terminal ?? ''), fmtMoeda(a.entrada ?? 0)]);
        });
        rows.push(['Total', '', fmtMoeda(aberturas.reduce((s: number, x: any) => s + (x.entrada ?? 0), 0))]);
        rows.push([]);
      }

      const vendas = dadosCaixaConsolidado.vendas ?? [];
      if (vendas.length > 0) {
        rows.push(['Vendas (comandas e delivery fechados)']);
        rows.push(['Data', 'Nota', 'Origem', 'Cliente', 'Total']);
        vendas.forEach((v: any) => {
          const origem = String(v.origem || v.Origem || 'BA').toUpperCase() === 'DL' ? 'Delivery' : 'Comanda';
          rows.push([fmtData(v.data), v.nota ?? '', origem, v.nomeCliente ?? v.cliente ?? '-', fmtMoeda(v.total ?? 0)]);
        });
        rows.push(['Total', '', '', '', fmtMoeda(vendas.reduce((s: number, x: any) => s + (x.total ?? 0), 0))]);
        rows.push([]);
      }

      const recVendas = dadosCaixaConsolidado.recebimentosVendas ?? [];
      if (recVendas.length > 0) {
        rows.push(['Pagamentos ao fechar comanda']);
        rows.push(['Data', 'Nota', 'Forma', 'Valor']);
        recVendas.forEach((r: any) => {
          rows.push([fmtData(r.dataVenda), r.nota ?? '', r.formaPagamento ?? '-', fmtMoeda(r.valor ?? 0)]);
        });
        rows.push(['Total', '', '', fmtMoeda(recVendas.reduce((s: number, x: any) => s + (x.valor ?? 0), 0))]);
        rows.push([]);
      }

      const recContas = dadosCaixaConsolidado.recebimentosContasReceber ?? [];
      if (recContas.length > 0) {
        rows.push(['Recebimentos (contas a receber quitadas)']);
        rows.push(['Data', 'Nº/Ordem', 'Forma', 'Histórico', 'Valor']);
        recContas.forEach((r: any) => {
          rows.push([fmtData(r.dataRecebimento), `${r.numero ?? ''}/${r.ordem ?? ''}`, r.formaPagamento ?? '-', r.historico ?? '-', fmtMoeda(r.valorRecebido ?? 0)]);
        });
        rows.push([]);
      }

      const saidas = dadosCaixaConsolidado.saidas ?? [];
      if (saidas.length > 0) {
        rows.push(['Saídas de caixa']);
        rows.push(['Data/Hora', 'Terminal', 'Histórico', 'Valor']);
        saidas.forEach((s: any) => {
          rows.push([fmtDataHora(s.data, s.hora), String(s.terminal ?? ''), s.historico ?? s.origem ?? '-', fmtMoeda(s.saida ?? 0)]);
        });
        rows.push(['Total', '', '', fmtMoeda(saidas.reduce((s0: number, x: any) => s0 + (x.saida ?? 0), 0))]);
      }

      if (rows.length > 3) exportToExcelMultiSection(rows, `${baseNome}.csv`);
      return;
    }
  };

  const buscarRelatorioVendas = async () => {
    try {
      setLoading(true);
      const response = await relatoriosService.getVendas(dataInicio, dataFim);
      const lista = Array.isArray(response) ? response : ((response as { vendas?: any[] })?.vendas ?? []);
      setDadosVendas(lista);
      setResumoVendas(response && typeof response === 'object' && !Array.isArray(response) ? {
        totalVendasComandas: (response as any).totalVendasComandas,
        valorTotalComandas: (response as any).valorTotalComandas,
        totalVendasDelivery: (response as any).totalVendasDelivery,
        valorTotalDelivery: (response as any).valorTotalDelivery
      } : null);
      if (lista.length === 0) {
        showSuccess('Info', 'Nenhuma venda encontrada no período');
      }
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível carregar o relatório de vendas');
      console.error('Erro:', error);
      setDadosVendas([]);
      setResumoVendas(null);
    } finally {
      setLoading(false);
    }
  };

  const buscarRelatorioProdutos = async () => {
    try {
      setLoading(true);
      const origem = filtroOrigemProdutos === 'BA' || filtroOrigemProdutos === 'DL' ? filtroOrigemProdutos : undefined;
      const response = await relatoriosService.getProdutosMaisVendidos(dataInicio, dataFim, 20, origem);
      const lista = Array.isArray(response) ? response : ((response as { produtos?: any[] })?.produtos ?? []);
      setDadosProdutos(lista);
      if (lista.length === 0) {
        showSuccess('Info', 'Nenhum produto vendido no período (comandas fechadas)');
      }
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível carregar o relatório de produtos');
      console.error('Erro:', error);
      setDadosProdutos([]);
    } finally {
      setLoading(false);
    }
  };

  const buscarRelatorioCliente = async () => {
    if (!codigoCliente) {
      showError('Erro', 'Escolha um cliente ou informe o código');
      return;
    }

    try {
      setLoading(true);
      const dados = await relatoriosService.getCliente(parseInt(codigoCliente), dataInicio || undefined, dataFim || undefined);
      setDadosCliente(dados);
      const compras = dados?.compras ?? dados?.Compras ?? [];
      if (!dados || compras.length === 0) {
        showSuccess('Info', 'Nenhuma compra encontrada para este cliente no período');
      }
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível carregar o relatório do cliente');
      console.error('Erro:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSelectCliente = (cliente: { id: number; nome?: string }) => {
    setCodigoCliente(String(cliente.id));
    setNomeClienteSelecionado(cliente.nome ?? '');
    setShowClienteSearch(false);
  };

  const buscarRelatorioCaixaConsolidado = async () => {
    try {
      setLoading(true);
      const dados = await relatoriosService.getCaixaConsolidado(dataInicio, dataFim);
      setDadosCaixaConsolidado(dados);
      if (!dados?.resumo) {
        showSuccess('Info', 'Nenhum movimento encontrado no período');
      }
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível carregar o relatório de caixa');
      setDadosCaixaConsolidado(null);
    } finally {
      setLoading(false);
    }
  };

  const handleBuscar = () => {
    if (tipoRelatorio === 'vendas') {
      buscarRelatorioVendas();
    } else if (tipoRelatorio === 'produtos') {
      buscarRelatorioProdutos();
    } else if (tipoRelatorio === 'cliente') {
      buscarRelatorioCliente();
    } else if (tipoRelatorio === 'caixa') {
      buscarRelatorioCaixaConsolidado();
    }
  };

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <BarChart3 className="w-8 h-8 text-primary" />
            <h1 className="text-3xl font-bold">Relatórios</h1>
          </div>
          <Button onClick={onClose} variant="outline">
            <ArrowLeft className="w-4 h-4 mr-2" />
            Voltar
          </Button>
        </div>

        {/* Seleção de Tipo de Relatório */}
        <div className="bg-card rounded-lg p-4 mb-6 shadow-lg no-print">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <button
              onClick={() => {
                setTipoRelatorio('vendas');
                setDadosVendas([]);
                setDadosProdutos([]);
                setDadosCliente(null);
              }}
              className={`p-4 rounded-lg border-2 transition-all ${
                tipoRelatorio === 'vendas'
                  ? 'border-primary bg-primary/10'
                  : 'border-border hover:border-primary/50'
              }`}
            >
              <FileText className="w-6 h-6 mb-2 text-primary" />
              <p className="font-semibold">Relatório de Vendas</p>
              <p className="text-sm text-text-secondary">Vendas por período e forma de pagamento</p>
            </button>

            <button
              onClick={() => {
                setTipoRelatorio('produtos');
                setDadosVendas([]);
                setDadosProdutos([]);
                setDadosCliente(null);
                setDadosCaixaConsolidado(null);
              }}
              className={`p-4 rounded-lg border-2 transition-all ${
                tipoRelatorio === 'produtos'
                  ? 'border-primary bg-primary/10'
                  : 'border-border hover:border-primary/50'
              }`}
            >
              <TrendingUp className="w-6 h-6 mb-2 text-primary" />
              <p className="font-semibold">Produtos Mais Vendidos</p>
              <p className="text-sm text-text-secondary">Top produtos por quantidade e valor</p>
            </button>

            <button
              onClick={() => {
                setTipoRelatorio('cliente');
                setDadosVendas([]);
                setDadosProdutos([]);
                setDadosCliente(null);
                setDadosCaixaConsolidado(null);
              }}
              className={`p-4 rounded-lg border-2 transition-all ${
                tipoRelatorio === 'cliente'
                  ? 'border-primary bg-primary/10'
                  : 'border-border hover:border-primary/50'
              }`}
            >
              <User className="w-6 h-6 mb-2 text-primary" />
              <p className="font-semibold">Histórico de Cliente</p>
              <p className="text-sm text-text-secondary">Compras e histórico do cliente</p>
            </button>

            <button
              onClick={() => {
                setTipoRelatorio('caixa');
                setDadosVendas([]);
                setDadosProdutos([]);
                setDadosCliente(null);
                setDadosCaixaConsolidado(null);
              }}
              className={`p-4 rounded-lg border-2 transition-all ${
                tipoRelatorio === 'caixa'
                  ? 'border-primary bg-primary/10'
                  : 'border-border hover:border-primary/50'
              }`}
            >
              <Wallet className="w-6 h-6 mb-2 text-primary" />
              <p className="font-semibold">Caixa Consolidado</p>
              <p className="text-sm text-text-secondary">Aberturas, vendas, recebimentos e saídas por período</p>
            </button>

            {onRelatorioRecebimentos && (
              <button
                onClick={onRelatorioRecebimentos}
                className="p-4 rounded-lg border-2 border-border hover:border-primary/50 transition-all hover:bg-primary/5"
              >
                <FileText className="w-6 h-6 mb-2 text-primary" />
                <p className="font-semibold">Recebimentos</p>
                <p className="text-sm text-text-secondary">Vendas e formas de pagamento</p>
              </button>
            )}
            {onRelatorioPeriodo && (
              <button
                onClick={onRelatorioPeriodo}
                className="p-4 rounded-lg border-2 border-border hover:border-primary/50 transition-all hover:bg-primary/5"
              >
                <BarChart3 className="w-6 h-6 mb-2 text-primary" />
                <p className="font-semibold">Relatório por Período</p>
                <p className="text-sm text-text-secondary">Itens vendidos e recebimentos</p>
              </button>
            )}
            {onReimpressaoRecibos && (
              <button
                onClick={onReimpressaoRecibos}
                className="p-4 rounded-lg border-2 border-border hover:border-primary/50 transition-all hover:bg-primary/5"
              >
                <Printer className="w-6 h-6 mb-2 text-primary" />
                <p className="font-semibold">Reimpressão de Recibos</p>
                <p className="text-sm text-text-secondary">Reimprimir recibo por comanda, nota ou período</p>
              </button>
            )}
          </div>
        </div>

        {/* Filtros */}
        <div className="bg-card rounded-lg p-4 mb-6 shadow-lg">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {tipoRelatorio === 'cliente' ? (
              <>
                <div>
                  <label className="block text-sm font-medium mb-2">Data Início</label>
                  <input
                    type="date"
                    value={dataInicio}
                    onChange={(e) => setDataInicio(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Data Fim</label>
                  <input
                    type="date"
                    value={dataFim}
                    onChange={(e) => setDataFim(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>
                <div className="md:col-span-2">
                  <label className="block text-sm font-medium mb-2">Cliente</label>
                  <div className="flex gap-2 flex-wrap">
                    <div className="flex-1 min-w-[200px] flex gap-2">
                      <input
                        type="text"
                        readOnly
                        value={nomeClienteSelecionado ? `${nomeClienteSelecionado}${codigoCliente ? ` (Cód. ${codigoCliente})` : ''}` : codigoCliente ? `Código ${codigoCliente}` : 'Nenhum cliente selecionado'}
                        className="flex-1 px-3 py-2 border rounded-lg bg-gray-50 text-gray-700"
                        placeholder="Clique em Escolher cliente"
                      />
                      <Button
                        type="button"
                        onClick={() => setShowClienteSearch(true)}
                        variant="outline"
                        className="flex items-center gap-2 shrink-0"
                      >
                        <User className="w-4 h-4" />
                        Escolher cliente
                      </Button>
                    </div>
                    <span className="text-sm text-text-muted self-center">ou</span>
                    <input
                      type="number"
                      value={codigoCliente}
                      onChange={(e) => {
                        setCodigoCliente(e.target.value);
                        if (!e.target.value) setNomeClienteSelecionado('');
                      }}
                      className="w-24 px-3 py-2 border rounded-lg"
                      placeholder="Código"
                    />
                  </div>
                </div>
                <div className="flex items-end">
                  <Button onClick={handleBuscar} className="w-full bg-primary" disabled={loading}>
                    {loading ? 'Buscando...' : 'Buscar'}
                  </Button>
                </div>
              </>
            ) : (
              <>
                <div>
                  <label className="block text-sm font-medium mb-2">Data Início</label>
                  <input
                    type="date"
                    value={dataInicio}
                    onChange={(e) => setDataInicio(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Data Fim</label>
                  <input
                    type="date"
                    value={dataFim}
                    onChange={(e) => setDataFim(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>
                {tipoRelatorio === 'produtos' && (
                  <div>
                    <label className="block text-sm font-medium mb-2">Origem</label>
                    <select
                      value={filtroOrigemProdutos}
                      onChange={(e) => setFiltroOrigemProdutos(e.target.value)}
                      className="w-full px-3 py-2 border rounded-lg"
                    >
                      <option value="">Todos (Comandas + Delivery)</option>
                      <option value="BA">Só Comandas</option>
                      <option value="DL">Só Delivery</option>
                    </select>
                  </div>
                )}
                <div className="md:col-span-2 flex items-end">
                  <Button onClick={handleBuscar} className="w-full bg-primary" disabled={loading}>
                    {loading ? 'Buscando...' : 'Buscar'}
                  </Button>
                </div>
              </>
            )}
          </div>
        </div>

        {/* Botões Imprimir e Exportar Excel - exibidos quando há dados */}
        {temDadosRelatorio && (
          <div className="flex flex-wrap gap-2 mb-4 no-print">
            <Button onClick={handleImprimir} variant="outline" className="flex items-center gap-2">
              <Printer className="w-4 h-4" />
              Imprimir
            </Button>
            <Button onClick={handleExportarExcel} variant="outline" className="flex items-center gap-2">
              <Download className="w-4 h-4" />
              Exportar Excel
            </Button>
          </div>
        )}

        {/* Resultados */}
        {loading ? (
          <div className="text-center py-12">
            <p className="text-text-secondary">Carregando...</p>
          </div>
        ) : (
          <>
            {/* Relatório de Vendas */}
            {tipoRelatorio === 'vendas' && dadosVendas.length > 0 && (
              <div className="bg-card rounded-lg p-6 shadow-lg">
                <h2 className="text-xl font-bold mb-4">Relatório de Vendas</h2>
                {(resumoVendas && (resumoVendas.totalVendasComandas !== undefined || resumoVendas.totalVendasDelivery !== undefined)) && (
                  <div className="grid grid-cols-2 gap-4 mb-4">
                    <div className="bg-slate-100 border border-slate-200 rounded-lg p-3">
                      <p className="text-xs text-slate-600 font-medium">Comandas (balcão)</p>
                      <p className="text-lg font-bold text-slate-800">{resumoVendas.totalVendasComandas ?? 0} venda(s)</p>
                      <p className="text-sm font-semibold text-green-700">{formatarMoeda(resumoVendas.valorTotalComandas ?? 0)}</p>
                    </div>
                    <div className="bg-orange-50 border border-orange-200 rounded-lg p-3">
                      <p className="text-xs text-orange-700 font-medium">Delivery</p>
                      <p className="text-lg font-bold text-orange-800">{resumoVendas.totalVendasDelivery ?? 0} venda(s)</p>
                      <p className="text-sm font-semibold text-orange-700">{formatarMoeda(resumoVendas.valorTotalDelivery ?? 0)}</p>
                    </div>
                  </div>
                )}
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left p-2">Data</th>
                        <th className="text-left p-2">Origem</th>
                        <th className="text-left p-2">Forma de Pagamento</th>
                        <th className="text-right p-2">Quantidade</th>
                        <th className="text-right p-2">Valor Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {dadosVendas.map((item: any, index: number) => (
                        <tr key={item.nota ? `${item.nota}-${index}` : index} className="border-b hover:bg-gray-50">
                          <td className="p-2">{formatarData(item.data || item.Data)}</td>
                          <td className="p-2">{String(item.origem || item.Origem || 'BA').toUpperCase() === 'DL' ? 'Delivery' : 'Comanda'}</td>
                          <td className="p-2">{item.formaPagamento ?? item.FormaPagamento ?? 'N/A'}</td>
                          <td className="p-2 text-right">{item.quantidade ?? item.quantidadeItens ?? item.QuantidadeItens ?? 1}</td>
                          <td className="p-2 text-right font-semibold">
                            {formatarMoeda(item.valorTotal ?? item.total ?? item.Total ?? 0)}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                    <tfoot>
                      <tr className="border-t-2 font-bold">
                        <td colSpan={3} className="p-2">Total</td>
                        <td className="p-2 text-right">
                          {dadosVendas.reduce((acc: number, item: any) => acc + (item.quantidade ?? item.quantidadeItens ?? item.QuantidadeItens ?? 1), 0)}
                        </td>
                        <td className="p-2 text-right text-primary">
                          {formatarMoeda(
                            dadosVendas.reduce((acc: number, item: any) => acc + (item.valorTotal ?? item.total ?? item.Total ?? 0), 0)
                          )}
                        </td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>
            )}

            {/* Relatório de Produtos */}
            {tipoRelatorio === 'produtos' && dadosProdutos.length > 0 && (() => {
              const totalValorProdutos = dadosProdutos.reduce((acc: number, item: any) => acc + (item.valorTotal ?? item.ValorTotal ?? 0), 0);
              return (
              <div className="bg-card rounded-lg p-6 shadow-lg">
                <h2 className="text-xl font-bold mb-4">Produtos Mais Vendidos</h2>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left p-2">#</th>
                        <th className="text-left p-2">Produto</th>
                        <th className="text-right p-2">Quantidade</th>
                        <th className="text-right p-2">% do Total</th>
                        <th className="text-right p-2">Valor Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {dadosProdutos.map((item: any, index: number) => {
                        const valorItem = item.valorTotal ?? item.ValorTotal ?? 0;
                        const percentual = totalValorProdutos > 0 ? (valorItem / totalValorProdutos) * 100 : 0;
                        return (
                        <tr key={item.codigo ? `${item.codigo}-${index}` : index} className="border-b hover:bg-gray-50">
                          <td className="p-2">{item.posicao ?? index + 1}</td>
                          <td className="p-2">{item.descricao ?? item.Descricao ?? 'N/A'}</td>
                          <td className="p-2 text-right">{item.quantidadeVendida ?? item.QuantidadeVendida ?? item.quantidadeTotal ?? 0}</td>
                          <td className="p-2 text-right">
                            <div className="flex items-center justify-end gap-2">
                              <div className="w-20 bg-gray-200 rounded-full h-2 hidden sm:block">
                                <div
                                  className="bg-primary h-2 rounded-full"
                                  style={{ width: `${percentual}%` }}
                                />
                              </div>
                              <span className="text-sm font-medium text-text-secondary w-12 text-right">
                                {percentual.toFixed(1)}%
                              </span>
                            </div>
                          </td>
                          <td className="p-2 text-right font-semibold">
                            {formatarMoeda(valorItem)}
                          </td>
                        </tr>
                        );
                      })}
                    </tbody>
                    <tfoot>
                      <tr className="border-t-2 font-bold bg-gray-50">
                        <td colSpan={2} className="p-2">Total</td>
                        <td className="p-2 text-right">
                          {dadosProdutos.reduce((acc: number, item: any) => acc + (item.quantidadeVendida ?? item.QuantidadeVendida ?? item.quantidadeTotal ?? 0), 0)}
                        </td>
                        <td className="p-2 text-right text-text-secondary text-sm">100%</td>
                        <td className="p-2 text-right text-primary">
                          {formatarMoeda(totalValorProdutos)}
                        </td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>
              );
            })()}

            {/* Relatório de Cliente */}
            {tipoRelatorio === 'cliente' && dadosCliente && (
              <div className="bg-card rounded-lg p-6 shadow-lg">
                <div className="mb-6">
                  <h2 className="text-xl font-bold">{dadosCliente.nomeCliente || 'Cliente'}</h2>
                  <p className="text-sm text-text-secondary">
                    Código: {dadosCliente.codigoCliente}
                  </p>
                </div>

                {(dadosCliente.compras ?? dadosCliente.Compras)?.length > 0 ? (
                  <>
                    <div className="grid grid-cols-3 gap-4 mb-6">
                      <div className="bg-primary/10 p-4 rounded-lg">
                        <p className="text-sm text-text-secondary">Total de Compras</p>
                        <p className="text-2xl font-bold">{(dadosCliente.compras ?? dadosCliente.Compras).length}</p>
                      </div>
                      <div className="bg-green-50 p-4 rounded-lg">
                        <p className="text-sm text-text-secondary">Valor Total</p>
                        <p className="text-2xl font-bold text-green-600">
                          {formatarMoeda(
                        (dadosCliente.compras ?? dadosCliente.Compras).reduce(
                            (acc: number, compra: any) => acc + (compra.total ?? compra.Total ?? compra.valorTotal ?? compra.ValorTotal ?? 0),
                            0
                          )
                          )}
                        </p>
                      </div>
                      <div className="bg-blue-50 p-4 rounded-lg">
                        <p className="text-sm text-text-secondary">Última Compra</p>
                        <p className="text-lg font-bold">
                          {dadosCliente.compras[0]?.data
                            ? formatarData(dadosCliente.compras[0].data)
                            : 'N/A'}
                        </p>
                      </div>
                    </div>

                    <div className="overflow-x-auto">
                      <table className="w-full">
                        <thead>
                          <tr className="border-b">
                            <th className="text-left p-2">Data</th>
                            <th className="text-left p-2">Nota</th>
                            <th className="text-right p-2">Valor Total</th>
                          </tr>
                        </thead>
                        <tbody>
                          {(dadosCliente.compras ?? dadosCliente.Compras).map((compra: any, index: number) => (
                            <tr key={index} className="border-b hover:bg-gray-50">
                              <td className="p-2">
                                {compra.data ? formatarData(compra.data) : 'N/A'}
                              </td>
                              <td className="p-2">{compra.nota || 'N/A'}</td>
                              <td className="p-2 text-right font-semibold">
                                {formatarMoeda(compra.total ?? compra.Total ?? compra.valorTotal ?? compra.ValorTotal ?? 0)}
                              </td>
                            </tr>
                          ))}
                        </tbody>
                        <tfoot>
                          <tr className="border-t-2 font-bold bg-gray-50">
                            <td colSpan={2} className="p-2">Total ({((dadosCliente.compras ?? dadosCliente.Compras).length)} compra(s))</td>
                            <td className="p-2 text-right text-primary">
                              {formatarMoeda(
                                (dadosCliente.compras ?? dadosCliente.Compras).reduce(
                                  (acc: number, c: any) => acc + (c.total ?? c.Total ?? c.valorTotal ?? c.ValorTotal ?? 0),
                                  0
                                )
                              )}
                            </td>
                          </tr>
                        </tfoot>
                      </table>
                    </div>
                  </>
                ) : (
                  <p className="text-text-secondary text-center py-8">
                    Nenhuma compra encontrada para este cliente
                  </p>
                )}
              </div>
            )}

            {/* Mensagem quando não há dados */}
            {tipoRelatorio === 'vendas' && dadosVendas.length === 0 && !loading && (
              <div className="bg-card rounded-lg p-12 text-center shadow-lg">
                <p className="text-text-secondary">Nenhum dado encontrado. Clique em "Buscar" para gerar o relatório.</p>
              </div>
            )}

            {tipoRelatorio === 'produtos' && dadosProdutos.length === 0 && !loading && (
              <div className="bg-card rounded-lg p-12 text-center shadow-lg">
                <p className="text-text-secondary">Nenhum dado encontrado. Clique em "Buscar" para gerar o relatório.</p>
              </div>
            )}

            {/* Relatório Caixa Consolidado */}
            {tipoRelatorio === 'caixa' && dadosCaixaConsolidado && (
              <div className="bg-card rounded-lg p-6 shadow-lg space-y-6">
                <h2 className="text-xl font-bold flex items-center gap-2">
                  <Wallet className="w-6 h-6 text-primary" />
                  Caixa Consolidado
                </h2>
                <p className="text-sm text-text-secondary">
                  Período: {formatarData(dadosCaixaConsolidado.dataInicio)} a {formatarData(dadosCaixaConsolidado.dataFim)}
                </p>

                {/* Resumo */}
                {dadosCaixaConsolidado.resumo && (
                  <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
                    <div className="bg-amber-50 border border-amber-200 rounded-xl p-4">
                      <p className="text-xs text-amber-800 font-medium">Aberturas</p>
                      <p className="text-lg font-bold text-amber-700">{formatarMoeda(dadosCaixaConsolidado.resumo.totalAbertura ?? 0)}</p>
                      <p className="text-xs text-amber-600">{dadosCaixaConsolidado.resumo.quantidadeAberturas ?? 0} movimento(s)</p>
                    </div>
                    <div className="bg-green-50 border border-green-200 rounded-xl p-4">
                      <p className="text-xs text-green-800 font-medium">Vendas (total)</p>
                      <p className="text-lg font-bold text-green-700">{formatarMoeda(dadosCaixaConsolidado.resumo.totalVendas ?? 0)}</p>
                      <p className="text-xs text-green-600">{dadosCaixaConsolidado.resumo.quantidadeVendas ?? 0} venda(s)</p>
                    </div>
                    {(dadosCaixaConsolidado.resumo.quantidadeVendasComandas !== undefined || dadosCaixaConsolidado.resumo.quantidadeVendasDelivery !== undefined) && (
                      <div className="col-span-2 sm:col-span-3 lg:col-span-6 flex flex-wrap gap-3 text-sm">
                        <span className="bg-slate-100 px-3 py-1.5 rounded font-medium">
                          Comandas: <strong>{dadosCaixaConsolidado.resumo.quantidadeVendasComandas ?? 0}</strong> venda(s) – {formatarMoeda(dadosCaixaConsolidado.resumo.totalVendasComandas ?? 0)}
                        </span>
                        <span className="bg-orange-50 px-3 py-1.5 rounded font-medium text-orange-800">
                          Delivery: <strong>{dadosCaixaConsolidado.resumo.quantidadeVendasDelivery ?? 0}</strong> venda(s) – {formatarMoeda(dadosCaixaConsolidado.resumo.totalVendasDelivery ?? 0)}
                        </span>
                      </div>
                    )}
                    <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
                      <p className="text-xs text-blue-800 font-medium">Pag. ao fechar</p>
                      <p className="text-xs text-blue-600/80">(fechamento comanda)</p>
                      <p className="text-lg font-bold text-blue-700">{formatarMoeda(dadosCaixaConsolidado.resumo.totalRecebimentosVendas ?? 0)}</p>
                      <p className="text-xs text-blue-600">{dadosCaixaConsolidado.resumo.quantidadeRecebimentosVendas ?? 0} pagamento(s)</p>
                    </div>
                    <div className="bg-indigo-50 border border-indigo-200 rounded-xl p-4">
                      <p className="text-xs text-indigo-800 font-medium">Recebimentos</p>
                      <p className="text-xs text-indigo-600/80">(contas a receber)</p>
                      <p className="text-lg font-bold text-indigo-700">{formatarMoeda(dadosCaixaConsolidado.resumo.totalRecebimentosContasReceber ?? 0)}</p>
                      <p className="text-xs text-indigo-600">{dadosCaixaConsolidado.resumo.quantidadeRecebimentosContasReceber ?? 0} quitamento(s)</p>
                    </div>
                    <div className="bg-red-50 border border-red-200 rounded-xl p-4">
                      <p className="text-xs text-red-800 font-medium">Saídas</p>
                      <p className="text-lg font-bold text-red-700">{formatarMoeda(dadosCaixaConsolidado.resumo.totalSaidas ?? 0)}</p>
                      <p className="text-xs text-red-600">{dadosCaixaConsolidado.resumo.quantidadeSaidas ?? 0} saída(s)</p>
                    </div>
                    <div className="bg-primary/10 border border-primary/30 rounded-xl p-4">
                      <p className="text-xs text-primary font-medium">Saldo período</p>
                      <p className="text-lg font-bold text-primary">{formatarMoeda(dadosCaixaConsolidado.resumo.saldoPeriodo ?? 0)}</p>
                    </div>
                  </div>
                )}

                {/* Total por espécie (forma de pagamento) */}
                {dadosCaixaConsolidado.resumo && (
                  (dadosCaixaConsolidado.resumo.totalPorFormaRecebimentosVendas?.length > 0 || dadosCaixaConsolidado.resumo.totalPorFormaRecebimentosContas?.length > 0) && (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                      <div className="bg-slate-50 border border-slate-200 rounded-xl p-4">
                        <h3 className="font-semibold text-slate-800 mb-3">Total por espécie – Pag. ao fechar comanda</h3>
                        {(dadosCaixaConsolidado.resumo.totalPorFormaRecebimentosVendas?.length ?? 0) > 0 ? (
                          <ul className="space-y-2 text-sm">
                            {(dadosCaixaConsolidado.resumo.totalPorFormaRecebimentosVendas ?? []).map((item: { forma: string; valor: number }, i: number) => (
                              <li key={i} className="flex justify-between">
                                <span className="text-slate-700">{item.forma || 'Outros'}</span>
                                <span className="font-semibold text-slate-900">{formatarMoeda(item.valor ?? 0)}</span>
                              </li>
                            ))}
                          </ul>
                        ) : (
                          <p className="text-slate-500 text-sm">Nenhum pagamento ao fechar comanda no período.</p>
                        )}
                      </div>
                      <div className="bg-slate-50 border border-slate-200 rounded-xl p-4">
                        <h3 className="font-semibold text-slate-800 mb-3">Total por espécie – Recebimentos (contas a receber)</h3>
                        {(dadosCaixaConsolidado.resumo.totalPorFormaRecebimentosContas?.length ?? 0) > 0 ? (
                          <ul className="space-y-2 text-sm">
                            {(dadosCaixaConsolidado.resumo.totalPorFormaRecebimentosContas ?? []).map((item: { forma: string; valor: number }, i: number) => (
                              <li key={i} className="flex justify-between">
                                <span className="text-slate-700">{item.forma || 'Outros'}</span>
                                <span className="font-semibold text-slate-900">{formatarMoeda(item.valor ?? 0)}</span>
                              </li>
                            ))}
                          </ul>
                        ) : (
                          <p className="text-slate-500 text-sm">Nenhum recebimento de contas a receber no período.</p>
                        )}
                      </div>
                    </div>
                  )
                )}

                {/* Aberturas */}
                {(dadosCaixaConsolidado.aberturas?.length ?? 0) > 0 && (
                  <div>
                    <h3 className="font-semibold text-amber-700 mb-2 flex items-center gap-2"><ArrowDownCircle className="w-4 h-4" /> Aberturas de caixa</h3>
                    <div className="overflow-x-auto border rounded-lg">
                      <table className="w-full text-sm">
                        <thead><tr className="border-b bg-amber-50"><th className="text-left p-2">Data/Hora</th><th className="text-left p-2">Terminal</th><th className="text-right p-2">Valor</th></tr></thead>
                        <tbody>
                          {(dadosCaixaConsolidado.aberturas ?? []).map((a: any, i: number) => (
                            <tr key={i} className="border-b hover:bg-amber-50/50"><td className="p-2">{formatarData(a.data)} {a.hora && String(a.hora).substring(0, 8)}</td><td className="p-2">{a.terminal}</td><td className="p-2 text-right font-semibold">{formatarMoeda(a.entrada ?? 0)}</td></tr>
                          ))}
                        </tbody>
                        <tfoot><tr className="border-t-2 font-bold bg-amber-50"><td colSpan={2} className="p-2">Total ({(dadosCaixaConsolidado.aberturas ?? []).length} movimento(s))</td><td className="p-2 text-right">{formatarMoeda((dadosCaixaConsolidado.aberturas ?? []).reduce((s: number, a: any) => s + (a.entrada ?? 0), 0))}</td></tr></tfoot>
                      </table>
                    </div>
                  </div>
                )}

                {/* Vendas */}
                {(dadosCaixaConsolidado.vendas?.length ?? 0) > 0 && (
                  <div>
                    <h3 className="font-semibold text-green-700 mb-2">Vendas (comandas e delivery fechados)</h3>
                    <div className="overflow-x-auto border rounded-lg max-h-48 overflow-y-auto">
                      <table className="w-full text-sm">
                        <thead><tr className="border-b bg-green-50"><th className="text-left p-2">Data</th><th className="text-left p-2">Nota</th><th className="text-left p-2">Origem</th><th className="text-left p-2">Cliente</th><th className="text-right p-2">Total</th></tr></thead>
                        <tbody>
                          {(dadosCaixaConsolidado.vendas ?? []).map((v: any, i: number) => (
                            <tr key={i} className="border-b hover:bg-green-50/50"><td className="p-2">{formatarData(v.data)}</td><td className="p-2">{v.nota}</td><td className="p-2">{String(v.origem || v.Origem || 'BA').toUpperCase() === 'DL' ? 'Delivery' : 'Comanda'}</td><td className="p-2">{v.nomeCliente ?? v.cliente ?? '-'}</td><td className="p-2 text-right font-semibold">{formatarMoeda(v.total ?? 0)}</td></tr>
                          ))}
                        </tbody>
                        <tfoot><tr className="border-t-2 font-bold bg-green-50"><td colSpan={4} className="p-2">Total ({(dadosCaixaConsolidado.vendas ?? []).length} venda(s))</td><td className="p-2 text-right">{formatarMoeda((dadosCaixaConsolidado.vendas ?? []).reduce((s: number, v: any) => s + (v.total ?? 0), 0))}</td></tr></tfoot>
                      </table>
                    </div>
                  </div>
                )}

                {/* Pagamentos ao fechar comanda */}
                {(dadosCaixaConsolidado.recebimentosVendas?.length ?? 0) > 0 && (
                  <div>
                    <h3 className="font-semibold text-blue-700 mb-2">Pagamentos ao fechar comanda</h3>
                    <div className="overflow-x-auto border rounded-lg max-h-48 overflow-y-auto">
                      <table className="w-full text-sm">
                        <thead><tr className="border-b bg-blue-50"><th className="text-left p-2">Data</th><th className="text-left p-2">Nota</th><th className="text-left p-2">Forma</th><th className="text-right p-2">Valor</th></tr></thead>
                        <tbody>
                          {(dadosCaixaConsolidado.recebimentosVendas ?? []).map((r: any, i: number) => (
                            <tr key={i} className="border-b hover:bg-blue-50/50"><td className="p-2">{formatarData(r.dataVenda)}</td><td className="p-2">{r.nota}</td><td className="p-2">{r.formaPagamento ?? '-'}</td><td className="p-2 text-right font-semibold">{formatarMoeda(r.valor ?? 0)}</td></tr>
                          ))}
                        </tbody>
                        <tfoot><tr className="border-t-2 font-bold bg-blue-50"><td colSpan={3} className="p-2">Total ({(dadosCaixaConsolidado.recebimentosVendas ?? []).length} pagamento(s))</td><td className="p-2 text-right">{formatarMoeda((dadosCaixaConsolidado.recebimentosVendas ?? []).reduce((s: number, r: any) => s + (r.valor ?? 0), 0))}</td></tr></tfoot>
                      </table>
                    </div>
                  </div>
                )}

                {/* Recebimentos = apenas contas a receber quitadas */}
                {(dadosCaixaConsolidado.recebimentosContasReceber?.length ?? 0) > 0 && (
                  <div>
                    <h3 className="font-semibold text-indigo-700 mb-2">Recebimentos (contas a receber quitadas)</h3>
                    <div className="overflow-x-auto border rounded-lg max-h-48 overflow-y-auto">
                      <table className="w-full text-sm">
                        <thead><tr className="border-b bg-indigo-50"><th className="text-left p-2">Data</th><th className="text-left p-2">Nº/Ordem</th><th className="text-left p-2">Forma</th><th className="text-left p-2">Histórico</th><th className="text-right p-2">Valor</th></tr></thead>
                        <tbody>
                          {(dadosCaixaConsolidado.recebimentosContasReceber ?? []).map((r: any, i: number) => (
                            <tr key={i} className="border-b hover:bg-indigo-50/50"><td className="p-2">{formatarData(r.dataRecebimento)}</td><td className="p-2">{r.numero}/{r.ordem}</td><td className="p-2">{r.formaPagamento ?? '-'}</td><td className="p-2">{r.historico ?? '-'}</td><td className="p-2 text-right font-semibold">{formatarMoeda(r.valorRecebido ?? 0)}</td></tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}

                {/* Saídas */}
                {(dadosCaixaConsolidado.saidas?.length ?? 0) > 0 && (
                  <div>
                    <h3 className="font-semibold text-red-700 mb-2 flex items-center gap-2"><ArrowUpCircle className="w-4 h-4" /> Saídas de caixa</h3>
                    <div className="overflow-x-auto border rounded-lg max-h-48 overflow-y-auto">
                      <table className="w-full text-sm">
                        <thead><tr className="border-b bg-red-50"><th className="text-left p-2">Data/Hora</th><th className="text-left p-2">Terminal</th><th className="text-left p-2">Histórico</th><th className="text-right p-2">Valor</th></tr></thead>
                        <tbody>
                          {(dadosCaixaConsolidado.saidas ?? []).map((s: any, i: number) => (
                            <tr key={i} className="border-b hover:bg-red-50/50"><td className="p-2">{formatarData(s.data)} {s.hora && String(s.hora).substring(0, 8)}</td><td className="p-2">{s.terminal}</td><td className="p-2">{s.historico ?? s.origem ?? '-'}</td><td className="p-2 text-right font-semibold text-red-700">{formatarMoeda(s.saida ?? 0)}</td></tr>
                          ))}
                        </tbody>
                        <tfoot><tr className="border-t-2 font-bold bg-red-50"><td colSpan={3} className="p-2">Total ({(dadosCaixaConsolidado.saidas ?? []).length} saída(s))</td><td className="p-2 text-right text-red-700">{formatarMoeda((dadosCaixaConsolidado.saidas ?? []).reduce((s: number, x: any) => s + (x.saida ?? 0), 0))}</td></tr></tfoot>
                      </table>
                    </div>
                  </div>
                )}

                {(!dadosCaixaConsolidado.resumo || (dadosCaixaConsolidado.resumo.quantidadeAberturas === 0 && dadosCaixaConsolidado.resumo.quantidadeVendas === 0 && dadosCaixaConsolidado.resumo.quantidadeRecebimentosVendas === 0 && dadosCaixaConsolidado.resumo.quantidadeRecebimentosContasReceber === 0 && dadosCaixaConsolidado.resumo.quantidadeSaidas === 0)) && (
                  <p className="text-text-secondary text-center py-6">Nenhum movimento no período.</p>
                )}
              </div>
            )}

            {tipoRelatorio === 'cliente' && !dadosCliente && !loading && (
              <div className="bg-card rounded-lg p-12 text-center shadow-lg">
                <p className="text-text-secondary">Escolha um cliente (botão &quot;Escolher cliente&quot;) ou informe o código, defina o período e clique em &quot;Buscar&quot; para gerar o relatório.</p>
              </div>
            )}

            {tipoRelatorio === 'caixa' && !dadosCaixaConsolidado && !loading && (
              <div className="bg-card rounded-lg p-12 text-center shadow-lg">
                <p className="text-text-secondary">Defina o período (Data Início e Data Fim) e clique em &quot;Buscar&quot; para gerar o relatório de caixa consolidado.</p>
              </div>
            )}
          </>
        )}

        <ClienteSearch
          isOpen={showClienteSearch}
          onClose={() => setShowClienteSearch(false)}
          onSelectCliente={handleSelectCliente}
        />
      </div>
    </div>
  );
};

export default RelatoriosPage;
