import { AnimatePresence, motion } from 'framer-motion';
import { 
  Search, 
  X, 
  Receipt, 
  CreditCard, 
  DollarSign,
  FileText,
  Calendar,
  User,
  ShoppingBag,
  Download,
  Printer
} from 'lucide-react';
import React, { useState } from 'react';
import { useToast } from '../hooks/useToast';
import { recebimentosService, vendasService } from '../services/api';
import { Button } from '../components/ui/button';
import { Venda } from '../types/api';
import { exportToExcel } from '../utils/exportExcel';

interface RelatorioRecebimentosPageProps {
  onClose: () => void;
}

interface Recebimento {
  id: number;
  idFormaPagamento: number;
  formaPagamentoDescricao: string;
  nCaixa: number;
  nota: string;
  valor: number;
  troco: number;
}

const RelatorioRecebimentosPage: React.FC<RelatorioRecebimentosPageProps> = ({ onClose }) => {
  const [modoBusca, setModoBusca] = useState<'individual' | 'periodo'>('individual');
  const [tipoBusca, setTipoBusca] = useState<'comanda' | 'nota'>('comanda');
  const [valorBusca, setValorBusca] = useState<string>('');
  const [dataInicio, setDataInicio] = useState<string>(
    new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0]
  );
  const [dataFim, setDataFim] = useState<string>(new Date().toISOString().split('T')[0]);
  const [venda, setVenda] = useState<Venda | null>(null);
  const [vendas, setVendas] = useState<Venda[]>([]);
  const [recebimentos, setRecebimentos] = useState<Recebimento[]>([]);
  const [recebimentosPeriodo, setRecebimentosPeriodo] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const { showError, showSuccess } = useToast();

  const buscarRelatorio = async () => {
    setLoading(true);
    try {
      if (modoBusca === 'individual') {
        if (!valorBusca || valorBusca.trim() === '') {
          showError('Erro', 'Informe o número da comanda ou nota');
          return;
        }

        let vendaEncontrada: Venda | null = null;
        let recebimentosEncontrados: Recebimento[] = [];

        if (tipoBusca === 'comanda') {
          const comandaNum = parseInt(valorBusca);
          const vendasList = await vendasService.getByComanda(comandaNum);
          
          if (!vendasList || vendasList.length === 0) {
            showError('Erro', 'Comanda não encontrada');
            setVenda(null);
            setRecebimentos([]);
            return;
          }

          // Buscar a venda fechada (não aberta)
          const vendaFechada = vendasList.find(v => v.lancado !== 'ABERTO');
          if (!vendaFechada) {
            showError('Erro', 'Esta comanda ainda não foi fechada');
            setVenda(null);
            setRecebimentos([]);
            return;
          }

          vendaEncontrada = await vendasService.getByNota(vendaFechada.nota);
          recebimentosEncontrados = await recebimentosService.getRecebimentosPorNota(vendaFechada.nota);
        } else {
          // Buscar por nota
          vendaEncontrada = await vendasService.getByNota(valorBusca);
          if (!vendaEncontrada) {
            showError('Erro', 'Nota não encontrada');
            setVenda(null);
            setRecebimentos([]);
            return;
          }

          recebimentosEncontrados = await recebimentosService.getRecebimentosPorNota(valorBusca);
        }

        if (!vendaEncontrada) {
          showError('Erro', 'Venda não encontrada');
          setVenda(null);
          setRecebimentos([]);
          return;
        }

        setVenda(vendaEncontrada);
        setRecebimentos(recebimentosEncontrados);
        setVendas([]);
        setRecebimentosPeriodo([]);
        showSuccess('Sucesso', 'Relatório carregado com sucesso!');
      } else {
        // Buscar por período
        if (!dataInicio || !dataFim) {
          showError('Erro', 'Informe a data início e data fim');
          return;
        }

        const recebimentosList = await recebimentosService.getRecebimentosPorPeriodo(dataInicio, dataFim);
        
        // Agrupar por nota e buscar vendas
        const notasUnicas = Array.from(new Set(recebimentosList.map((r: any) => r.nota)));
        const vendasList: Venda[] = [];
        
        for (const nota of notasUnicas) {
          try {
            const venda = await vendasService.getByNota(nota);
            if (venda) {
              vendasList.push(venda);
            }
          } catch (error) {
            console.error(`Erro ao buscar venda ${nota}:`, error);
          }
        }

        setVendas(vendasList);
        setRecebimentosPeriodo(recebimentosList);
        setVenda(null);
        setRecebimentos([]);
        showSuccess('Sucesso', `${recebimentosList.length} recebimentos encontrados no período!`);
      }
    } catch (error: any) {
      console.error('Erro ao buscar relatório:', error);
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível buscar o relatório');
      setVenda(null);
      setVendas([]);
      setRecebimentos([]);
      setRecebimentosPeriodo([]);
    } finally {
      setLoading(false);
    }
  };

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  };

  const formatarData = (data: string) => {
    return new Date(data).toLocaleString('pt-BR');
  };

  const formatarDataSimples = (data: string) => {
    return new Date(data).toLocaleDateString('pt-BR');
  };

  const calcularTotalRecebimentos = () => {
    return recebimentos.reduce((acc, r) => acc + r.valor, 0);
  };

  const calcularTotalTroco = () => {
    return recebimentos.reduce((acc, r) => acc + r.troco, 0);
  };

  const imprimirRelatorio = () => {
    window.print();
  };

  const exportarExcel = () => {
    if (modoBusca === 'individual' && recebimentos.length > 0 && venda) {
      const rows = recebimentos.map(r => ({
        formaPagamento: r.formaPagamentoDescricao,
        valor: r.valor,
        troco: r.troco
      }));
      exportToExcel(rows, `recebimentos-nota-${venda.nota}.csv`, [
        { key: 'formaPagamento', label: 'Forma de Pagamento' },
        { key: 'valor', label: 'Valor' },
        { key: 'troco', label: 'Troco' }
      ]);
      return;
    }
    if (modoBusca === 'periodo' && recebimentosPeriodo.length > 0) {
      const rows = recebimentosPeriodo.map((r: any) => ({
        nota: r.nota ?? '',
        formaPagamento: r.formaPagamentoDescricao ?? r.forma ?? '',
        valor: r.valor ?? 0,
        troco: r.troco ?? 0,
        nCaixa: r.nCaixa ?? ''
      }));
      exportToExcel(rows, `recebimentos-periodo-${dataInicio}-${dataFim}.csv`, [
        { key: 'nota', label: 'Nota' },
        { key: 'formaPagamento', label: 'Forma' },
        { key: 'valor', label: 'Valor' },
        { key: 'troco', label: 'Troco' },
        { key: 'nCaixa', label: 'Caixa' }
      ]);
    }
  };

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6 no-print">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <FileText className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Relatório de Recebimentos</h1>
              <p className="text-sm text-text-muted">Visualize vendas e formas de pagamento</p>
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

        {/* Busca */}
        <div className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border no-print">
          {/* Escolha do modo: Comanda/Nota ou Por data */}
          <div className="flex flex-wrap items-center gap-4 mb-4">
            <span className="text-sm font-medium text-text-secondary">Buscar por:</span>
            <label className="flex items-center space-x-2">
              <input
                type="radio"
                value="individual"
                checked={modoBusca === 'individual'}
                onChange={() => setModoBusca('individual')}
                className="w-4 h-4 text-primary"
              />
              <span className="text-sm font-medium text-text-primary">Comanda / Nota</span>
            </label>
            <label className="flex items-center space-x-2">
              <input
                type="radio"
                value="periodo"
                checked={modoBusca === 'periodo'}
                onChange={() => setModoBusca('periodo')}
                className="w-4 h-4 text-primary"
              />
              <span className="text-sm font-medium text-text-primary">Por data</span>
            </label>
          </div>

          {modoBusca === 'individual' ? (
            <div className="flex flex-col sm:flex-row gap-4">
              <div className="flex items-center space-x-4">
                <label className="flex items-center space-x-2">
                  <input
                    type="radio"
                    value="comanda"
                    checked={tipoBusca === 'comanda'}
                    onChange={(e) => setTipoBusca(e.target.value as 'comanda' | 'nota')}
                    className="w-4 h-4 text-primary"
                  />
                  <span className="text-sm font-medium text-text-primary">Comanda</span>
                </label>
                <label className="flex items-center space-x-2">
                  <input
                    type="radio"
                    value="nota"
                    checked={tipoBusca === 'nota'}
                    onChange={(e) => setTipoBusca(e.target.value as 'comanda' | 'nota')}
                    className="w-4 h-4 text-primary"
                  />
                  <span className="text-sm font-medium text-text-primary">Nota</span>
                </label>
              </div>
              <div className="flex-1">
                <input
                  type={tipoBusca === 'comanda' ? 'number' : 'text'}
                  value={valorBusca}
                  onChange={(e) => setValorBusca(e.target.value)}
                  onKeyPress={(e) => e.key === 'Enter' && buscarRelatorio()}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder={tipoBusca === 'comanda' ? 'Digite o número da comanda' : 'Digite o número da nota'}
                  disabled={loading}
                />
              </div>
              <div className="flex items-end">
                <Button
                  onClick={buscarRelatorio}
                  disabled={loading || !valorBusca}
                  className="w-full sm:w-auto px-6 py-3"
                >
                  {loading ? (
                    <>
                      <Search className="w-4 h-4 mr-2 animate-pulse" />
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
          ) : (
            /* Por data: Data Início, Data Fim e Buscar */
            <div className="flex flex-col sm:flex-row gap-4 items-end">
              <div className="flex-1 grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium text-text-secondary mb-1">Data Início</label>
                  <input
                    type="date"
                    value={dataInicio}
                    onChange={(e) => setDataInicio(e.target.value)}
                    className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                              focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                              text-text-primary"
                    disabled={loading}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-text-secondary mb-1">Data Fim</label>
                  <input
                    type="date"
                    value={dataFim}
                    onChange={(e) => setDataFim(e.target.value)}
                    className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                              focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                              text-text-primary"
                    disabled={loading}
                  />
                </div>
              </div>
              <Button
                onClick={buscarRelatorio}
                disabled={loading || !dataInicio || !dataFim}
                className="w-full sm:w-auto px-6 py-3"
              >
                {loading ? (
                  <>
                    <Search className="w-4 h-4 mr-2 animate-pulse" />
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
          )}
        </div>

        {/* Relatório */}
        {venda && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="space-y-6"
          >
            {/* Cabeçalho do Relatório */}
            <div className="bg-card rounded-2xl p-6 shadow-lg border border-border print:shadow-none">
              <div className="flex items-center justify-between mb-4 print:mb-2">
                <h2 className="text-xl font-bold text-text-primary flex items-center">
                  <Receipt className="w-5 h-5 mr-2 text-primary" />
                  Relatório de Recebimento
                </h2>
                <div className="hidden sm:flex space-x-2 no-print">
                  <Button
                    onClick={imprimirRelatorio}
                    variant="outline"
                    size="sm"
                    className="flex items-center space-x-2"
                  >
                    <Printer className="w-4 h-4" />
                    <span>Imprimir</span>
                  </Button>
                  <Button
                    onClick={exportarExcel}
                    variant="outline"
                    size="sm"
                    className="flex items-center space-x-2"
                  >
                    <Download className="w-4 h-4" />
                    <span>Exportar Excel</span>
                  </Button>
                </div>
              </div>

              {/* Informações da Venda */}
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
                <div className="bg-background-secondary rounded-xl p-4">
                  <div className="flex items-center space-x-2 mb-2">
                    <ShoppingBag className="w-4 h-4 text-text-muted" />
                    <p className="text-xs text-text-secondary">Comanda</p>
                  </div>
                  <p className="text-lg font-bold text-text-primary">
                    {venda.comanda || 'N/A'}
                  </p>
                </div>
                <div className="bg-background-secondary rounded-xl p-4">
                  <div className="flex items-center space-x-2 mb-2">
                    <FileText className="w-4 h-4 text-text-muted" />
                    <p className="text-xs text-text-secondary">Nota</p>
                  </div>
                  <p className="text-lg font-bold text-text-primary">{venda.nota}</p>
                </div>
                <div className="bg-background-secondary rounded-xl p-4">
                  <div className="flex items-center space-x-2 mb-2">
                    <Calendar className="w-4 h-4 text-text-muted" />
                    <p className="text-xs text-text-secondary">Data</p>
                  </div>
                  <p className="text-sm font-semibold text-text-primary">
                    {formatarDataSimples(venda.emissao)}
                  </p>
                </div>
                <div className="bg-background-secondary rounded-xl p-4">
                  <div className="flex items-center space-x-2 mb-2">
                    <User className="w-4 h-4 text-text-muted" />
                    <p className="text-xs text-text-secondary">Cliente</p>
                  </div>
                  <p className="text-sm font-semibold text-text-primary truncate">
                    {venda.cliente || 'Não informado'}
                  </p>
                </div>
              </div>

              {/* Itens Vendidos */}
              <div className="mb-6">
                <h3 className="text-lg font-bold text-text-primary mb-4 flex items-center">
                  <ShoppingBag className="w-5 h-5 mr-2 text-primary" />
                  Itens Vendidos
                </h3>
                <div className="overflow-x-auto">
                  <table className="w-full border-collapse">
                    <thead>
                      <tr className="bg-background-secondary">
                        <th className="text-left p-3 text-sm font-semibold text-text-secondary border-b border-border">
                          Item
                        </th>
                        <th className="text-left p-3 text-sm font-semibold text-text-secondary border-b border-border">
                          Descrição
                        </th>
                        <th className="text-center p-3 text-sm font-semibold text-text-secondary border-b border-border">
                          Qtd
                        </th>
                        <th className="text-right p-3 text-sm font-semibold text-text-secondary border-b border-border">
                          Preço Unit.
                        </th>
                        <th className="text-right p-3 text-sm font-semibold text-text-secondary border-b border-border">
                          Total
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      {venda.itens && venda.itens.length > 0 ? (
                        venda.itens.map((item, index) => (
                          <tr key={index} className="border-b border-border hover:bg-background-secondary/50">
                            <td className="p-3 text-sm text-text-primary">{item.item}</td>
                            <td className="p-3 text-sm text-text-primary">
                              {item.descricao || `Produto ${item.codigo}`}
                            </td>
                            <td className="p-3 text-sm text-text-primary text-center">{item.qtd}</td>
                            <td className="p-3 text-sm text-text-primary text-right">
                              {formatarMoeda(item.preco)}
                            </td>
                            <td className="p-3 text-sm font-semibold text-text-primary text-right">
                              {formatarMoeda(item.total)}
                            </td>
                          </tr>
                        ))
                      ) : (
                        <tr>
                          <td colSpan={5} className="p-4 text-center text-text-muted">
                            Nenhum item encontrado
                          </td>
                        </tr>
                      )}
                    </tbody>
                    <tfoot>
                      <tr className="bg-primary/10">
                        <td colSpan={4} className="p-3 text-right font-bold text-text-primary">
                          Total:
                        </td>
                        <td className="p-3 font-bold text-primary text-right">
                          {formatarMoeda(venda.total || 0)}
                        </td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>

              {/* Formas de Pagamento */}
              <div>
                <h3 className="text-lg font-bold text-text-primary mb-4 flex items-center">
                  <CreditCard className="w-5 h-5 mr-2 text-primary" />
                  Formas de Pagamento
                </h3>
                {recebimentos.length > 0 ? (
                  <div className="space-y-3">
                    {recebimentos.map((recebimento, index) => (
                      <div
                        key={index}
                        className="bg-background-secondary border border-border rounded-xl p-4"
                      >
                        <div className="flex items-center justify-between">
                          <div className="flex items-center space-x-3">
                            <div className="w-10 h-10 bg-primary/20 rounded-lg flex items-center justify-center">
                              <DollarSign className="w-5 h-5 text-primary" />
                            </div>
                            <div>
                              <p className="font-semibold text-text-primary">
                                {recebimento.formaPagamentoDescricao}
                              </p>
                              <p className="text-xs text-text-muted">
                                Caixa: {recebimento.nCaixa}
                              </p>
                            </div>
                          </div>
                          <div className="text-right">
                            <p className="font-bold text-lg text-text-primary">
                              {formatarMoeda(recebimento.valor)}
                            </p>
                            {recebimento.troco > 0 && (
                              <p className="text-xs text-yellow-600">
                                Troco: {formatarMoeda(recebimento.troco)}
                              </p>
                            )}
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="bg-background-secondary border border-border rounded-xl p-6 text-center">
                    <p className="text-text-muted">Nenhum recebimento encontrado</p>
                  </div>
                )}

                {/* Resumo */}
                {recebimentos.length > 0 && (
                  <div className="mt-6 bg-primary/10 border-2 border-primary/30 rounded-xl p-4">
                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Total Recebido:</span>
                        <span className="font-semibold text-text-primary">
                          {formatarMoeda(calcularTotalRecebimentos())}
                        </span>
                      </div>
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Total da Venda:</span>
                        <span className="font-semibold text-text-primary">
                          {formatarMoeda(venda.total || 0)}
                        </span>
                      </div>
                      {calcularTotalTroco() > 0 && (
                        <div className="flex justify-between text-sm pt-2 border-t border-primary/20">
                          <span className="text-text-secondary">Troco Total:</span>
                          <span className="font-semibold text-yellow-600">
                            {formatarMoeda(calcularTotalTroco())}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </div>
            </div>
          </motion.div>
        )}

        {/* Relatório por Período */}
        {modoBusca === 'periodo' && recebimentosPeriodo.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="space-y-6"
          >
            <div className="bg-card rounded-2xl p-6 shadow-lg border border-border">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-xl font-bold text-text-primary flex items-center">
                  <Receipt className="w-5 h-5 mr-2 text-primary" />
                  Recebimentos do Período
                </h2>
                <div className="hidden sm:flex space-x-2 no-print">
                  <Button
                    onClick={imprimirRelatorio}
                    variant="outline"
                    size="sm"
                    className="flex items-center space-x-2"
                  >
                    <Printer className="w-4 h-4" />
                    <span>Imprimir</span>
                  </Button>
                  <Button
                    onClick={exportarExcel}
                    variant="outline"
                    size="sm"
                    className="flex items-center space-x-2"
                  >
                    <Download className="w-4 h-4" />
                    <span>Exportar Excel</span>
                  </Button>
                </div>
              </div>

              {/* Agrupar recebimentos por nota */}
              {(() => {
                const recebimentosPorNota = recebimentosPeriodo.reduce((acc: any, r: any) => {
                  if (!acc[r.nota]) {
                    acc[r.nota] = [];
                  }
                  acc[r.nota].push(r);
                  return acc;
                }, {});

                const totalGeral = recebimentosPeriodo.reduce((acc: number, r: any) => acc + r.valor, 0);
                const totalTrocoGeral = recebimentosPeriodo.reduce((acc: number, r: any) => acc + r.troco, 0);

                return (
                  <div className="space-y-4">
                    {Object.entries(recebimentosPorNota).map(([nota, recebs]: [string, any]) => {
                      const vendaNota = vendas.find((v: Venda) => v.nota === nota);
                      const totalNota = recebs.reduce((acc: number, r: any) => acc + r.valor, 0);
                      const totalTrocoNota = recebs.reduce((acc: number, r: any) => acc + r.troco, 0);

                      return (
                        <div key={nota} className="bg-background-secondary rounded-xl p-4 border border-border">
                          <div className="flex items-center justify-between mb-3">
                            <div>
                              <p className="font-bold text-text-primary">Nota: {nota}</p>
                              {vendaNota && (
                                <p className="text-sm text-text-secondary">
                                  Comanda: {vendaNota.comanda || 'N/A'} | 
                                  Cliente: {vendaNota.cliente || 'Não informado'} | 
                                  Data: {formatarDataSimples(vendaNota.emissao)}
                                </p>
                              )}
                            </div>
                            <div className="text-right">
                              <p className="text-sm text-text-secondary">Total Recebido</p>
                              <p className="font-bold text-lg text-primary">{formatarMoeda(totalNota)}</p>
                            </div>
                          </div>

                          <div className="space-y-2">
                            {recebs.map((r: any, idx: number) => (
                              <div key={idx} className="flex items-center justify-between bg-background rounded-lg p-2">
                                <div className="flex items-center space-x-3">
                                  <DollarSign className="w-4 h-4 text-text-muted" />
                                  <span className="text-sm text-text-primary">{r.formaPagamentoDescricao}</span>
                                  {r.troco > 0 && (
                                    <span className="text-xs text-yellow-600">Troco: {formatarMoeda(r.troco)}</span>
                                  )}
                                </div>
                                <span className="font-semibold text-text-primary">{formatarMoeda(r.valor)}</span>
                              </div>
                            ))}
                          </div>
                        </div>
                      );
                    })}

                    {/* Resumo Geral */}
                    <div className="bg-primary/10 border-2 border-primary/30 rounded-xl p-4 mt-6">
                      <div className="space-y-2">
                        <div className="flex justify-between text-sm">
                          <span className="text-text-secondary">Total Recebido no Período:</span>
                          <span className="font-semibold text-text-primary">
                            {formatarMoeda(totalGeral)}
                          </span>
                        </div>
                        {totalTrocoGeral > 0 && (
                          <div className="flex justify-between text-sm pt-2 border-t border-primary/20">
                            <span className="text-text-secondary">Troco Total:</span>
                            <span className="font-semibold text-yellow-600">
                              {formatarMoeda(totalTrocoGeral)}
                            </span>
                          </div>
                        )}
                        <div className="flex justify-between text-sm pt-2 border-t border-primary/20">
                          <span className="text-text-secondary">Total de Recebimentos:</span>
                          <span className="font-semibold text-text-primary">
                            {recebimentosPeriodo.length}
                          </span>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })()}
            </div>
          </motion.div>
        )}

        {/* Estado Vazio */}
        {!venda && !loading && modoBusca === 'individual' && recebimentosPeriodo.length === 0 && (
          <div className="bg-card rounded-2xl p-12 text-center shadow-lg border border-border">
            <Receipt className="w-16 h-16 text-text-muted mx-auto mb-4" />
            <p className="text-text-muted">Informe uma comanda ou nota para visualizar o relatório</p>
          </div>
        )}

        {modoBusca === 'periodo' && !loading && recebimentosPeriodo.length === 0 && (
          <div className="bg-card rounded-2xl p-12 text-center shadow-lg border border-border">
            <Calendar className="w-16 h-16 text-text-muted mx-auto mb-4" />
            <p className="text-text-muted">Selecione o período (Data Início e Data Fim) e clique em Buscar para visualizar os recebimentos</p>
            <p className="text-sm text-text-muted mt-2">Ou não há recebimentos no período informado.</p>
          </div>
        )}
      </div>

      {/* Estilos para impressão */}
      <style>{`
        @media print {
          .print\\:hidden {
            display: none !important;
          }
          .print\\:shadow-none {
            box-shadow: none !important;
          }
          .print\\:mb-2 {
            margin-bottom: 0.5rem !important;
          }
        }
      `}</style>
    </div>
  );
};

export default RelatorioRecebimentosPage;
