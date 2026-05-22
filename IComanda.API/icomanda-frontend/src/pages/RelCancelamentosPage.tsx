import React, { useState, useCallback } from 'react';
import { ArrowLeft, Search, Download, FileX, AlertCircle } from 'lucide-react';
import { Button } from '../components/ui/button';
import { relatoriosService } from '../services/api';

interface RelCancelamentosPageProps {
  onClose: () => void;
}

interface Cancelamento {
  nota: string;
  emissao: string;
  hora: string;
  comanda: string | null;
  mesa: string | null;
  nomeCliente: string | null;
  operador: string | null;
  total: number;
  justificativa: string | null;
}

const fmt = (v: number) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v);

const hoje = () => new Date().toISOString().split('T')[0];

const RelCancelamentosPage: React.FC<RelCancelamentosPageProps> = ({ onClose }) => {
  const [de, setDe] = useState(hoje());
  const [ate, setAte] = useState(hoje());
  const [loading, setLoading] = useState(false);
  const [dados, setDados] = useState<Cancelamento[] | null>(null);
  const [erro, setErro] = useState<string | null>(null);

  const buscar = useCallback(async () => {
    if (!de || !ate) {
      setErro('Informe o período para buscar.');
      return;
    }
    setErro(null);
    setLoading(true);
    try {
      const resultado = await relatoriosService.getCancelamentos(de, ate);
      setDados(resultado ?? []);
    } catch {
      setErro('Erro ao buscar cancelamentos. Tente novamente.');
    } finally {
      setLoading(false);
    }
  }, [de, ate]);

  const exportarCSV = () => {
    if (!dados || dados.length === 0) return;
    const header = ['Nota', 'Data', 'Hora', 'Comanda', 'Mesa', 'Cliente', 'Operador', 'Total', 'Justificativa'];
    const rows = dados.map((c) => [
      c.nota ?? '',
      c.emissao ? new Date(c.emissao).toLocaleDateString('pt-BR') : '',
      c.hora ?? '',
      c.comanda ?? '',
      c.mesa ?? '',
      c.nomeCliente ?? '',
      c.operador ?? '',
      c.total?.toFixed(2).replace('.', ',') ?? '0,00',
      (c.justificativa ?? '').replace(/"/g, '""'),
    ]);
    const csv = [header, ...rows].map((r) => r.map((cell) => `"${cell}"`).join(';')).join('\n');
    const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `cancelamentos_${de}_${ate}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  };

  const totalQtd = dados?.length ?? 0;
  const totalValor = dados?.reduce((acc, c) => acc + (c.total ?? 0), 0) ?? 0;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center gap-3 sticky top-0 z-10">
        <Button variant="ghost" size="icon" onClick={onClose}>
          <ArrowLeft className="w-5 h-5" />
        </Button>
        <div className="flex items-center gap-2 flex-1">
          <FileX className="w-5 h-5 text-red-500" />
          <h1 className="text-lg font-semibold text-gray-900">Relatório de Cancelamentos</h1>
        </div>
      </div>

      <div className="max-w-5xl mx-auto p-4 space-y-4">
        {/* Filtros */}
        <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm">
          <div className="flex flex-wrap gap-3 items-end">
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-600">De</label>
              <input
                type="date"
                value={de}
                onChange={(e) => setDe(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
              />
            </div>
            <div className="flex flex-col gap-1">
              <label className="text-xs font-medium text-gray-600">Até</label>
              <input
                type="date"
                value={ate}
                onChange={(e) => setAte(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
              />
            </div>
            <Button onClick={buscar} disabled={loading} className="flex items-center gap-2">
              <Search className="w-4 h-4" />
              {loading ? 'Buscando...' : 'Buscar'}
            </Button>
            {dados && dados.length > 0 && (
              <Button variant="secondary" onClick={exportarCSV} className="flex items-center gap-2">
                <Download className="w-4 h-4" />
                Exportar CSV
              </Button>
            )}
          </div>
        </div>

        {/* Mensagem de erro */}
        {erro && (
          <div className="bg-red-50 border border-red-200 rounded-xl p-3 flex items-center gap-2 text-red-700 text-sm">
            <AlertCircle className="w-4 h-4 flex-shrink-0" />
            {erro}
          </div>
        )}

        {/* Totalizadores */}
        {dados !== null && (
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm text-center">
              <p className="text-xs text-gray-500 mb-1">Total de cancelamentos</p>
              <p className="text-2xl font-bold text-gray-800">{totalQtd}</p>
            </div>
            <div className="bg-white rounded-xl border border-gray-200 p-4 shadow-sm text-center">
              <p className="text-xs text-gray-500 mb-1">Valor total cancelado</p>
              <p className="text-2xl font-bold text-red-600">{fmt(totalValor)}</p>
            </div>
          </div>
        )}

        {/* Tabela */}
        {dados !== null && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden">
            {dados.length === 0 ? (
              <div className="py-16 text-center text-gray-400">
                <FileX className="w-10 h-10 mx-auto mb-2 opacity-40" />
                <p className="text-sm">Nenhum cancelamento encontrado no período.</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50 border-b border-gray-200">
                    <tr>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Nota</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Data</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Hora</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Comanda</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Mesa</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Cliente</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Operador</th>
                      <th className="px-4 py-3 text-right font-semibold text-gray-600">Total</th>
                      <th className="px-4 py-3 text-left font-semibold text-gray-600">Justificativa</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100">
                    {dados.map((c, i) => (
                      <tr key={i} className="hover:bg-gray-50 transition-colors">
                        <td className="px-4 py-3 font-mono text-gray-700">{c.nota}</td>
                        <td className="px-4 py-3 text-gray-600">
                          {c.emissao ? new Date(c.emissao).toLocaleDateString('pt-BR') : '-'}
                        </td>
                        <td className="px-4 py-3 text-gray-600">{c.hora ?? '-'}</td>
                        <td className="px-4 py-3 text-gray-600">{c.comanda ?? '-'}</td>
                        <td className="px-4 py-3 text-gray-600">{c.mesa ?? '-'}</td>
                        <td className="px-4 py-3 text-gray-700">{c.nomeCliente || '-'}</td>
                        <td className="px-4 py-3 text-gray-600">{c.operador ?? '-'}</td>
                        <td className="px-4 py-3 text-right font-semibold text-red-600">{fmt(c.total ?? 0)}</td>
                        <td className="px-4 py-3 text-gray-500 max-w-xs truncate" title={c.justificativa ?? ''}>
                          {c.justificativa || <span className="italic text-gray-300">—</span>}
                        </td>
                      </tr>
                    ))}
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

export default RelCancelamentosPage;
