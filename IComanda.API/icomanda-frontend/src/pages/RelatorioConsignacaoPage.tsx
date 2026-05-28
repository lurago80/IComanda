import React, { useEffect, useState } from 'react';
import { ArrowLeft, FileText, Printer } from 'lucide-react';
import { Button } from '../components/ui/button';
import { gruposService, relatoriosService } from '../services/api';
import { useToast } from '../hooks/useToast';
import { Grupo, RelatorioConsignacao } from '../types/api';

interface RelatorioConsignacaoPageProps {
  onClose: () => void;
}

const RelatorioConsignacaoPage: React.FC<RelatorioConsignacaoPageProps> = ({ onClose }) => {
  const hoje = new Date().toISOString().split('T')[0];
  const primeiroDiaMes = new Date(new Date().getFullYear(), new Date().getMonth(), 1)
    .toISOString()
    .split('T')[0];

  const [grupos, setGrupos] = useState<Grupo[]>([]);
  const [grupoId, setGrupoId] = useState<number | ''>('');
  const [dataInicio, setDataInicio] = useState(primeiroDiaMes);
  const [dataFim, setDataFim] = useState(hoje);
  const [loading, setLoading] = useState(false);
  const [relatorio, setRelatorio] = useState<RelatorioConsignacao | null>(null);
  const { showError } = useToast();

  useEffect(() => {
    gruposService
      .getTodosComQuantidade()
      .then((data) => {
        // Exibe apenas grupos com percentual > 0
        const comPercentual = data.filter((g) => (g.percentual ?? 0) > 0);
        setGrupos(comPercentual);
      })
      .catch(() => {
        // fallback: carrega todos
        gruposService.getAll().then(setGrupos).catch(() => {});
      });
  }, []);

  const formatarMoeda = (valor: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);

  const formatarNumero = (valor: number) =>
    new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 3, maximumFractionDigits: 3 }).format(valor);

  const gerarRelatorio = async () => {
    if (!grupoId) {
      showError('Validação', 'Selecione um grupo');
      return;
    }
    if (!dataInicio || !dataFim) {
      showError('Validação', 'Informe o período');
      return;
    }
    if (dataInicio > dataFim) {
      showError('Validação', 'Data inicial não pode ser maior que data final');
      return;
    }

    try {
      setLoading(true);
      const data = await relatoriosService.getConsignacao(Number(grupoId), dataInicio, dataFim);
      setRelatorio(data);
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível gerar o relatório');
      setRelatorio(null);
    } finally {
      setLoading(false);
    }
  };

  const imprimir = () => {
    window.print();
  };

  return (
    <div className="flex flex-col h-full bg-bg-primary">
      {/* Cabeçalho */}
      <div className="flex items-center justify-between p-4 border-b border-border print:hidden">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="sm" onClick={onClose} className="flex items-center gap-1">
            <ArrowLeft className="w-4 h-4" />
            Voltar
          </Button>
          <div className="flex items-center gap-2">
            <FileText className="w-5 h-5 text-primary" />
            <h1 className="text-xl font-bold text-text-primary">Relatório de Consignação</h1>
          </div>
        </div>
        {relatorio && (
          <Button variant="outline" size="sm" onClick={imprimir} className="flex items-center gap-2">
            <Printer className="w-4 h-4" />
            Imprimir
          </Button>
        )}
      </div>

      {/* Filtros */}
      <div className="p-4 border-b border-border bg-bg-secondary print:hidden">
        <div className="flex flex-wrap gap-4 items-end">
          <div className="flex flex-col gap-1 min-w-[220px] flex-1">
            <label className="text-sm font-medium text-text-primary">Grupo</label>
            <select
              value={grupoId}
              onChange={(e) => setGrupoId(e.target.value === '' ? '' : Number(e.target.value))}
              className="px-3 py-2 rounded-lg border border-border bg-bg-primary text-text-primary text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
            >
              <option value="">Selecione um grupo...</option>
              {grupos.map((g) => (
                <option key={g.id} value={g.id}>
                  {g.descricao} ({g.percentual}%)
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-text-primary">Data Início</label>
            <input
              type="date"
              value={dataInicio}
              onChange={(e) => setDataInicio(e.target.value)}
              className="px-3 py-2 rounded-lg border border-border bg-bg-primary text-text-primary text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
            />
          </div>

          <div className="flex flex-col gap-1">
            <label className="text-sm font-medium text-text-primary">Data Fim</label>
            <input
              type="date"
              value={dataFim}
              onChange={(e) => setDataFim(e.target.value)}
              className="px-3 py-2 rounded-lg border border-border bg-bg-primary text-text-primary text-sm focus:outline-none focus:ring-2 focus:ring-primary/30"
            />
          </div>

          <Button onClick={gerarRelatorio} disabled={loading} className="self-end">
            {loading ? 'Gerando...' : 'Gerar Relatório'}
          </Button>
        </div>
      </div>

      {/* Resultado */}
      <div className="flex-1 overflow-auto p-4">
        {!relatorio && !loading && (
          <div className="flex flex-col items-center justify-center h-64 text-text-muted gap-3">
            <FileText className="w-12 h-12 opacity-30" />
            <p>Selecione um grupo e o período para gerar o relatório</p>
          </div>
        )}

        {relatorio && (
          <div className="max-w-4xl mx-auto">
            {/* Cabeçalho do relatório */}
            <div className="mb-6 print:mb-4">
              <h2 className="text-2xl font-bold text-text-primary print:text-black">
                Relatório de Consignação — {relatorio.grupoDescricao}
              </h2>
              <p className="text-text-muted print:text-gray-600 mt-1">
                Percentual: <strong>{relatorio.percentual}%</strong> &nbsp;|&nbsp; Período:{' '}
                <strong>
                  {new Date(relatorio.dataInicio.split('T')[0] + 'T12:00:00').toLocaleDateString('pt-BR')} até{' '}
                  {new Date(relatorio.dataFim.split('T')[0] + 'T12:00:00').toLocaleDateString('pt-BR')}
                </strong>
              </p>
            </div>

            {relatorio.itens.length === 0 ? (
              <div className="text-center py-12 text-text-muted">
                <p>Nenhuma venda encontrada para este grupo no período selecionado.</p>
              </div>
            ) : (
              <div className="overflow-x-auto rounded-lg border border-border print:border-gray-300">
                <table className="w-full text-sm print:text-xs">
                  <thead className="bg-bg-secondary print:bg-gray-100">
                    <tr>
                      <th className="px-4 py-3 text-left font-semibold text-text-primary print:text-black">
                        Descrição
                      </th>
                      <th className="px-4 py-3 text-right font-semibold text-text-primary print:text-black">
                        Qtd Vendida
                      </th>
                      <th className="px-4 py-3 text-right font-semibold text-text-primary print:text-black">
                        Valor Total
                      </th>
                      <th className="px-4 py-3 text-right font-semibold text-text-primary print:text-black">
                        Valor Consignação ({relatorio.percentual}%)
                      </th>
                    </tr>
                  </thead>
                  <tbody>
                    {relatorio.itens.map((item, idx) => (
                      <tr
                        key={item.produtoId}
                        className={idx % 2 === 0 ? 'bg-bg-primary' : 'bg-bg-secondary print:bg-gray-50'}
                      >
                        <td className="px-4 py-3 text-text-primary print:text-black">{item.descricao}</td>
                        <td className="px-4 py-3 text-right text-text-primary print:text-black">
                          {formatarNumero(item.quantidadeVendida)}
                        </td>
                        <td className="px-4 py-3 text-right text-text-primary print:text-black">
                          {formatarMoeda(item.valorTotal)}
                        </td>
                        <td className="px-4 py-3 text-right font-medium text-blue-700 print:text-blue-900">
                          {formatarMoeda(item.valorPercentual)}
                        </td>
                      </tr>
                    ))}
                    {/* Linha de totais */}
                    <tr className="border-t-2 border-border print:border-gray-400 bg-bg-secondary print:bg-gray-100 font-bold">
                      <td className="px-4 py-3 text-text-primary print:text-black">TOTAL</td>
                      <td className="px-4 py-3" />
                      <td className="px-4 py-3 text-right text-text-primary print:text-black">
                        {formatarMoeda(relatorio.totalValor)}
                      </td>
                      <td className="px-4 py-3 text-right text-blue-700 print:text-blue-900">
                        {formatarMoeda(relatorio.totalPercentual)}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default RelatorioConsignacaoPage;
