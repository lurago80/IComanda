import React, { useState } from 'react';
import { ArrowLeft, Calendar, Download, FileText, CreditCard, ShoppingCart, Printer } from 'lucide-react';
import { Button } from '../components/ui/button';
import { relatoriosService } from '../services/api';
import { useToast } from '../hooks/useToast';
import { RelatorioPeriodoDto } from '../types/api';
import { exportToExcel } from '../utils/exportExcel';

interface RelatorioPeriodoPageProps {
  onClose: () => void;
}

const RelatorioPeriodoPage: React.FC<RelatorioPeriodoPageProps> = ({ onClose }) => {
  const [dataInicio, setDataInicio] = useState<string>(
    new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0]
  );
  const [dataFim, setDataFim] = useState<string>(new Date().toISOString().split('T')[0]);
  const [loading, setLoading] = useState(false);
  const [relatorio, setRelatorio] = useState<RelatorioPeriodoDto | null>(null);
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

  const formatarHora = (hora: string) => {
    if (!hora) return '';
    // Se for TimeSpan do backend (HH:mm:ss), pegar apenas HH:mm
    if (hora.includes(':')) {
      const partes = hora.split(':');
      return `${partes[0]}:${partes[1]}`;
    }
    return hora;
  };

  const buscarRelatorio = async () => {
    if (!dataInicio || !dataFim) {
      showError('Erro', 'Por favor, preencha as datas de início e fim');
      return;
    }

    if (new Date(dataInicio) > new Date(dataFim)) {
      showError('Erro', 'Data inicial não pode ser maior que data final');
      return;
    }

    try {
      setLoading(true);
      const dados = await relatoriosService.getPeriodo(dataInicio, dataFim);
      const itens = dados?.itensVendidos ?? dados?.ItensVendidos ?? [];
      const recebimentos = dados?.recebimentosPorFormaPagamento ?? dados?.RecebimentosPorFormaPagamento ?? [];
      const resumo = dados?.resumo ?? dados?.Resumo ?? {};
      const normalizado: RelatorioPeriodoDto = {
        dataInicio: dados?.dataInicio ?? dados?.DataInicio ?? dataInicio,
        dataFim: dados?.dataFim ?? dados?.DataFim ?? dataFim,
        itensVendidos: Array.isArray(itens) ? itens : [],
        recebimentosPorFormaPagamento: Array.isArray(recebimentos) ? recebimentos : [],
        resumo: resumo as RelatorioPeriodoDto['resumo']
      };
      setRelatorio(normalizado);
      if (normalizado.itensVendidos.length === 0 && normalizado.recebimentosPorFormaPagamento.length === 0) {
        showSuccess('Info', 'Nenhum dado encontrado no período (comandas fechadas)');
      }
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível carregar o relatório');
      console.error('Erro:', error);
      setRelatorio(null);
    } finally {
      setLoading(false);
    }
  };

  const imprimirRelatorio = () => {
    window.print();
  };

  const exportarExcel = () => {
    if (!relatorio) return;
    const baseNome = `relatorio-periodo-${dataInicio}-${dataFim}`;
    if (relatorio.itensVendidos?.length > 0) {
      const rows = relatorio.itensVendidos.map((item: any) => ({
        descricao: item.descricao ?? item.Descricao ?? '',
        quantidade: item.quantidade ?? item.Quantidade ?? 0,
        valorTotal: item.valorTotal ?? item.ValorTotal ?? 0
      }));
      exportToExcel(rows, `${baseNome}-itens.csv`, [
        { key: 'descricao', label: 'Produto' },
        { key: 'quantidade', label: 'Quantidade' },
        { key: 'valorTotal', label: 'Valor Total' }
      ]);
    }
    if (relatorio.recebimentosPorFormaPagamento?.length > 0) {
      const rows = relatorio.recebimentosPorFormaPagamento.map((r: any) => ({
        forma: r.formaPagamento ?? r.FormaPagamento ?? '',
        quantidade: r.quantidade ?? r.Quantidade ?? 0,
        valorTotal: r.valorTotal ?? r.ValorTotal ?? 0
      }));
      exportToExcel(rows, `${baseNome}-recebimentos.csv`, [
        { key: 'forma', label: 'Forma de Pagamento' },
        { key: 'quantidade', label: 'Quantidade' },
        { key: 'valorTotal', label: 'Valor Total' }
      ]);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-white rounded-lg shadow-md p-6 mb-6 no-print">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-4">
              <Button
                onClick={onClose}
                variant="outline"
                className="flex items-center gap-2"
              >
                <ArrowLeft className="w-4 h-4" />
                Voltar
              </Button>
              <h1 className="text-2xl font-bold text-gray-800 flex items-center gap-2">
                <FileText className="w-6 h-6" />
                Relatório por Período
              </h1>
            </div>
            {relatorio && (
              <div className="flex gap-2 no-print">
                <Button
                  onClick={imprimirRelatorio}
                  variant="outline"
                  className="flex items-center gap-2"
                >
                  <Printer className="w-4 h-4" />
                  Imprimir
                </Button>
                <Button
                  onClick={exportarExcel}
                  variant="outline"
                  className="flex items-center gap-2"
                >
                  <Download className="w-4 h-4" />
                  Exportar Excel
                </Button>
              </div>
            )}
          </div>

          {/* Filtros */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Calendar className="w-4 h-4 inline mr-2" />
                Data Início
              </label>
              <input
                type="date"
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                onKeyPress={(e) => e.key === 'Enter' && buscarRelatorio()}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                <Calendar className="w-4 h-4 inline mr-2" />
                Data Fim
              </label>
              <input
                type="date"
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                onKeyPress={(e) => e.key === 'Enter' && buscarRelatorio()}
              />
            </div>
            <div className="flex items-end">
              <Button
                onClick={buscarRelatorio}
                disabled={loading}
                className="w-full flex items-center justify-center gap-2"
              >
                {loading ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
                    Buscando...
                  </>
                ) : (
                  <>
                    <FileText className="w-4 h-4" />
                    Buscar Relatório
                  </>
                )}
              </Button>
            </div>
          </div>
        </div>

        {/* Resumo */}
        {relatorio && relatorio.resumo && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h2 className="text-xl font-bold text-gray-800 mb-4">Resumo do Período</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div className="bg-blue-50 p-4 rounded-lg">
                <div className="text-sm text-gray-600">Total de Itens Vendidos</div>
                <div className="text-2xl font-bold text-blue-600">{(relatorio.resumo as any).totalItensVendidos ?? (relatorio.resumo as any).TotalItensVendidos ?? 0}</div>
              </div>
              <div className="bg-green-50 p-4 rounded-lg">
                <div className="text-sm text-gray-600">Valor Total dos Itens</div>
                <div className="text-2xl font-bold text-green-600">
                  {formatarMoeda((relatorio.resumo as any).valorTotalItens ?? (relatorio.resumo as any).ValorTotalItens ?? 0)}
                </div>
              </div>
              <div className="bg-yellow-50 p-4 rounded-lg">
                <div className="text-sm text-gray-600">Total de Descontos</div>
                <div className="text-2xl font-bold text-yellow-600">
                  {formatarMoeda((relatorio.resumo as any).totalDesconto ?? (relatorio.resumo as any).TotalDesconto ?? 0)}
                </div>
              </div>
              <div className="bg-purple-50 p-4 rounded-lg">
                <div className="text-sm text-gray-600">Total de Acréscimos</div>
                <div className="text-2xl font-bold text-purple-600">
                  {formatarMoeda((relatorio.resumo as any).totalAcrescimo ?? (relatorio.resumo as any).TotalAcrescimo ?? 0)}
                </div>
              </div>
              <div className="bg-indigo-50 p-4 rounded-lg">
                <div className="text-sm text-gray-600">Total de Recebimentos</div>
                <div className="text-2xl font-bold text-indigo-600">{(relatorio.resumo as any).totalRecebimentos ?? (relatorio.resumo as any).TotalRecebimentos ?? 0}</div>
              </div>
              <div className="bg-teal-50 p-4 rounded-lg">
                <div className="text-sm text-gray-600">Valor Total Recebido</div>
                <div className="text-2xl font-bold text-teal-600">
                  {formatarMoeda((relatorio.resumo as any).valorTotalRecebimentos ?? (relatorio.resumo as any).ValorTotalRecebimentos ?? 0)}
                </div>
              </div>
              <div className="bg-orange-50 p-4 rounded-lg">
                <div className="text-sm text-gray-600">Total de Troco</div>
                <div className="text-2xl font-bold text-orange-600">
                  {formatarMoeda((relatorio.resumo as any).valorTotalTroco ?? (relatorio.resumo as any).ValorTotalTroco ?? 0)}
                </div>
              </div>
              {((relatorio.resumo as any).valorTotalComandas !== undefined || (relatorio.resumo as any).valorTotalDelivery !== undefined) && (
                <>
                  <div className="bg-slate-100 p-4 rounded-lg">
                    <div className="text-sm text-gray-600">Valor Comandas (balcão)</div>
                    <div className="text-2xl font-bold text-slate-700">
                      {formatarMoeda((relatorio.resumo as any).valorTotalComandas ?? (relatorio.resumo as any).ValorTotalComandas ?? 0)}
                    </div>
                  </div>
                  <div className="bg-orange-100 p-4 rounded-lg">
                    <div className="text-sm text-gray-600">Valor Delivery</div>
                    <div className="text-2xl font-bold text-orange-700">
                      {formatarMoeda((relatorio.resumo as any).valorTotalDelivery ?? (relatorio.resumo as any).ValorTotalDelivery ?? 0)}
                    </div>
                  </div>
                </>
              )}
            </div>
          </div>
        )}

        {/* Vendas com itens dentro (agrupado por nota) */}
        {relatorio && relatorio.itensVendidos.length > 0 && (() => {
          const itensPorNota = (relatorio.itensVendidos as any[]).reduce((acc: Record<string, any[]>, item: any) => {
            const nota = item.nota ?? item.Nota ?? '';
            if (!acc[nota]) acc[nota] = [];
            acc[nota].push(item);
            return acc;
          }, {});
          const notas = Object.keys(itensPorNota).sort();
          return (
            <div className="bg-white rounded-lg shadow-md p-6 mb-6">
              <h2 className="text-xl font-bold text-gray-800 mb-4 flex items-center gap-2">
                <ShoppingCart className="w-5 h-5" />
                Vendas no Período ({notas.length} {notas.length === 1 ? 'venda' : 'vendas'})
              </h2>
              <div className="space-y-6">
                {notas.map((nota) => {
                  const itensVenda = itensPorNota[nota];
                  const primeiro = itensVenda[0];
                  const totalVenda = itensVenda.reduce((s: number, i: any) => s + (i.total ?? i.Total ?? 0), 0);
                  const dataHora = primeiro?.emissao ?? primeiro?.Emissao ?? '';
                  const cliente = primeiro?.nomeCliente ?? primeiro?.NomeCliente ?? `Código ${primeiro?.cliente ?? primeiro?.Cliente ?? ''}`;
                  const comanda = primeiro?.comanda ?? primeiro?.Comanda;
                  const mesa = primeiro?.mesa ?? primeiro?.Mesa;
                  return (
                    <div key={nota} className="border border-gray-200 rounded-lg overflow-hidden">
                      <div className="bg-gray-50 px-4 py-3 flex flex-wrap items-center justify-between gap-2">
                        <div>
                          <span className="font-semibold text-gray-800">Nota: {nota}</span>
                          <span className="text-gray-600 ml-3">{formatarData(dataHora)}</span>
                          {cliente && <span className="text-gray-600 ml-2">• {cliente}</span>}
                          {comanda != null && <span className="text-gray-500 ml-2">(Comanda {comanda})</span>}
                          {mesa != null && <span className="text-gray-500 ml-1">(Mesa {mesa})</span>}
                        </div>
                        <div className="font-bold text-green-600">
                          Total: {formatarMoeda(totalVenda)}
                        </div>
                      </div>
                      <div className="overflow-x-auto">
                        <table className="w-full text-sm">
                          <thead className="bg-gray-100">
                            <tr>
                              <th className="px-4 py-2 text-left">Item</th>
                              <th className="px-4 py-2 text-left">Descrição</th>
                              <th className="px-4 py-2 text-right">Qtd</th>
                              <th className="px-4 py-2 text-right">Preço</th>
                              <th className="px-4 py-2 text-right">Total</th>
                            </tr>
                          </thead>
                          <tbody>
                            {itensVenda.map((item: any, idx: number) => (
                              <tr key={idx} className="border-t hover:bg-gray-50">
                                <td className="px-4 py-2">{item.item ?? item.Item ?? idx + 1}</td>
                                <td className="px-4 py-2">{item.descricaoProduto ?? item.DescricaoProduto ?? '-'}</td>
                                <td className="px-4 py-2 text-right">{(item.qtd ?? item.Qtd ?? 0).toFixed(2)}</td>
                                <td className="px-4 py-2 text-right">{formatarMoeda(item.preco ?? item.Preco ?? 0)}</td>
                                <td className="px-4 py-2 text-right font-semibold">{formatarMoeda(item.total ?? item.Total ?? 0)}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          );
        })()}

        {/* Recebimentos por Forma de Pagamento */}
        {relatorio && relatorio.recebimentosPorFormaPagamento.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6 mb-6">
            <h2 className="text-xl font-bold text-gray-800 mb-4 flex items-center gap-2">
              <CreditCard className="w-5 h-5" />
              Recebimentos por Forma de Pagamento
            </h2>
            <div className="space-y-4">
              {relatorio.recebimentosPorFormaPagamento.map((recebimento, index) => (
                <div key={index} className="border border-gray-200 rounded-lg p-4">
                  <div className="flex items-center justify-between mb-3">
                    <h3 className="text-lg font-semibold text-gray-800">
                      {recebimento.formaPagamento}
                    </h3>
                    <div className="text-right">
                      <div className="text-sm text-gray-600">Quantidade: {recebimento.quantidade}</div>
                      <div className="text-lg font-bold text-green-600">
                        {formatarMoeda(recebimento.valorTotal)}
                      </div>
                      {recebimento.trocoTotal > 0 && (
                        <div className="text-sm text-gray-500">
                          Troco: {formatarMoeda(recebimento.trocoTotal)}
                        </div>
                      )}
                    </div>
                  </div>
                  <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-4 py-2 text-left">Nota</th>
                          <th className="px-4 py-2 text-left">Data/Hora</th>
                          <th className="px-4 py-2 text-left">Cliente</th>
                          <th className="px-4 py-2 text-right">Valor</th>
                          <th className="px-4 py-2 text-right">Troco</th>
                        </tr>
                      </thead>
                      <tbody>
                        {recebimento.detalhes.map((detalhe) => (
                          <tr key={detalhe.id} className="border-t">
                            <td className="px-4 py-2">{detalhe.nota}</td>
                            <td className="px-4 py-2">
                              {formatarData(detalhe.dataVenda)} {formatarHora(detalhe.horaVenda)}
                            </td>
                            <td className="px-4 py-2">
                              {detalhe.nomeCliente || `Código ${detalhe.cliente}`}
                            </td>
                            <td className="px-4 py-2 text-right">{formatarMoeda(detalhe.valor)}</td>
                            <td className="px-4 py-2 text-right">
                              {detalhe.troco > 0 ? formatarMoeda(detalhe.troco) : '-'}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              ))}
            </div>
            {/* Totalizador: quantidade e valor total dos recebimentos */}
            <div className="mt-4 p-4 bg-gray-50 rounded-lg border-2 border-gray-200">
              <div className="flex flex-wrap items-center justify-between gap-4">
                <span className="font-bold text-gray-800">Total Geral (Recebimentos):</span>
                <div className="flex gap-6">
                  <span className="text-gray-700">
                    <strong>Quantidade:</strong>{' '}
                    {relatorio.recebimentosPorFormaPagamento.reduce((acc: number, r: any) => acc + (r.quantidade ?? r.Quantidade ?? 0), 0)}
                  </span>
                  <span className="text-green-700 font-bold">
                    <strong>Valor Total:</strong>{' '}
                    {formatarMoeda(relatorio.recebimentosPorFormaPagamento.reduce((acc: number, r: any) => acc + (r.valorTotal ?? r.ValorTotal ?? 0), 0))}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Itens Vendidos */}
        {relatorio && relatorio.itensVendidos.length > 0 && (
          <div className="bg-white rounded-lg shadow-md p-6">
            <h2 className="text-xl font-bold text-gray-800 mb-4 flex items-center gap-2">
              <ShoppingCart className="w-5 h-5" />
              Itens Vendidos ({relatorio.itensVendidos.length})
            </h2>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-2 text-left">Nota</th>
                    <th className="px-4 py-2 text-left">Data/Hora</th>
                    <th className="px-4 py-2 text-left">Item</th>
                    <th className="px-4 py-2 text-left">Código</th>
                    <th className="px-4 py-2 text-left">Descrição</th>
                    <th className="px-4 py-2 text-left">Cliente</th>
                    <th className="px-4 py-2 text-right">Qtd</th>
                    <th className="px-4 py-2 text-left">Un</th>
                    <th className="px-4 py-2 text-right">Preço</th>
                    <th className="px-4 py-2 text-right">Desconto</th>
                    <th className="px-4 py-2 text-right">Acréscimo</th>
                    <th className="px-4 py-2 text-right">Total</th>
                  </tr>
                </thead>
                <tbody>
                  {relatorio.itensVendidos.map((item: any, index: number) => (
                    <tr key={index} className="border-t hover:bg-gray-50">
                      <td className="px-4 py-2">{item.nota ?? item.Nota}</td>
                      <td className="px-4 py-2">
                        {formatarData(item.emissao ?? item.Emissao ?? '')} {formatarHora(item.hora ?? item.Hora ?? '')}
                      </td>
                      <td className="px-4 py-2">{item.item ?? item.Item}</td>
                      <td className="px-4 py-2">{item.codigoProduto ?? item.CodigoProduto}</td>
                      <td className="px-4 py-2">{item.descricaoProduto ?? item.DescricaoProduto ?? '-'}</td>
                      <td className="px-4 py-2">
                        {item.nomeCliente ?? item.NomeCliente ?? `Código ${item.cliente ?? item.Cliente ?? ''}`}
                        {(item.mesa ?? item.Mesa) != null && ` (Mesa ${item.mesa ?? item.Mesa})`}
                        {(item.comanda ?? item.Comanda) != null && ` (Comanda ${item.comanda ?? item.Comanda})`}
                      </td>
                      <td className="px-4 py-2 text-right">{Number(item.qtd ?? item.Qtd ?? 0).toFixed(2)}</td>
                      <td className="px-4 py-2">{item.und ?? item.Und ?? ''}</td>
                      <td className="px-4 py-2 text-right">{formatarMoeda(item.preco ?? item.Preco ?? 0)}</td>
                      <td className="px-4 py-2 text-right">
                        {(item.desconto ?? item.Desconto ?? 0) > 0 ? formatarMoeda(item.desconto ?? item.Desconto) : '-'}
                      </td>
                      <td className="px-4 py-2 text-right">
                        {(item.acrescimo ?? item.Acrescimo ?? 0) > 0 ? formatarMoeda(item.acrescimo ?? item.Acrescimo) : '-'}
                      </td>
                      <td className="px-4 py-2 text-right font-semibold">
                        {formatarMoeda(item.total ?? item.Total ?? 0)}
                      </td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-gray-50 font-bold">
                  <tr>
                    <td colSpan={6} className="px-4 py-2 text-right">Total Geral:</td>
                    <td className="px-4 py-2 text-right">
                      {relatorio.itensVendidos.reduce((acc: number, i: any) => acc + Number(i.qtd ?? i.Qtd ?? 0), 0).toFixed(2)}
                    </td>
                    <td colSpan={4} className="px-4 py-2"></td>
                    <td className="px-4 py-2 text-right">
                      {formatarMoeda((relatorio.resumo as any)?.valorTotalItens ?? (relatorio.resumo as any)?.ValorTotalItens ?? relatorio.itensVendidos.reduce((acc: number, i: any) => acc + (i.total ?? i.Total ?? 0), 0))}
                    </td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </div>
        )}

        {/* Mensagem quando não há dados */}
        {relatorio && relatorio.itensVendidos.length === 0 && relatorio.recebimentosPorFormaPagamento.length === 0 && !loading && (
          <div className="bg-white rounded-lg shadow-md p-8 text-center">
            <FileText className="w-16 h-16 text-gray-400 mx-auto mb-4" />
            <p className="text-gray-600 text-lg">Nenhum dado encontrado no período selecionado</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default RelatorioPeriodoPage;
