import React, { useEffect, useState, useCallback } from 'react';
import {
  ArrowLeft,
  TrendingUp,
  ShoppingBag,
  Bike,
  DollarSign,
  Clock,
  RefreshCw,
  BarChart2,
  Package,
} from 'lucide-react';
import { Button } from '../components/ui/button';
import { relatoriosService } from '../services/api';

interface DashboardPageProps {
  onClose: () => void;
  totalComandasAbertas?: number;
  totalValorAberto?: number;
  totalDeliveryAbertos?: number;
  totalValorDelivery?: number;
}

const fmt = (v: number) =>
  new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v);

const hoje = () => new Date().toISOString().split('T')[0];

const DashboardPage: React.FC<DashboardPageProps> = ({
  onClose,
  totalComandasAbertas = 0,
  totalValorAberto = 0,
  totalDeliveryAbertos = 0,
  totalValorDelivery = 0,
}) => {
  const [loading, setLoading] = useState(false);
  const [lastUpdate, setLastUpdate] = useState<Date>(new Date());
  const [dados, setDados] = useState<any>(null);
  const [produtos, setProdutos] = useState<any[]>([]);

  const carregar = useCallback(async () => {
    setLoading(true);
    try {
      const data = hoje();
      const [dash, prods] = await Promise.all([
        relatoriosService.getDashboard(data, data),
        relatoriosService.getProdutosMaisVendidos(data, data, 5),
      ]);
      setDados(dash);
      setProdutos(prods?.produtos ?? []);
      setLastUpdate(new Date());
    } catch (err) {
      console.error('Erro ao carregar dashboard:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    carregar();
    const interval = setInterval(carregar, 60000);
    return () => clearInterval(interval);
  }, [carregar]);

  const totalVendas: number = dados?.totalVendas ?? 0;
  const valorTotal: number = dados?.valorTotal ?? 0;
  const ticketMedio: number = dados?.ticketMedio ?? 0;
  const vendasPorHora: Array<{ hora: number; quantidade: number; valorTotal: number }> = dados?.vendasPorHora ?? [];

  // Hora de pico
  const horaPico = vendasPorHora.length
    ? vendasPorHora.reduce((a, b) => (b.quantidade > a.quantidade ? b : a), vendasPorHora[0])
    : null;

  const maxQtdHora = vendasPorHora.length
    ? Math.max(...vendasPorHora.map((h) => h.quantidade), 1)
    : 1;

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="icon" onClick={onClose}>
            <ArrowLeft className="h-5 w-5" />
          </Button>
          <div>
            <h1 className="text-xl font-bold text-gray-900">Dashboard</h1>
            <p className="text-sm text-gray-500">
              Operacional hoje — {new Date().toLocaleDateString('pt-BR')}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-xs text-gray-400">
            Atualizado {lastUpdate.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
          </span>
          <Button variant="outline" size="icon" onClick={carregar} disabled={loading}>
            <RefreshCw className={`h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
          </Button>
        </div>
      </div>

      <div className="p-4 space-y-4 max-w-5xl mx-auto">

        {/* KPIs principais */}
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
          <KpiCard
            icon={<TrendingUp className="h-5 w-5 text-green-600" />}
            bg="bg-green-50"
            label="Faturamento hoje"
            value={fmt(valorTotal)}
            sub={`${totalVendas} venda${totalVendas !== 1 ? 's' : ''} fechada${totalVendas !== 1 ? 's' : ''}`}
          />
          <KpiCard
            icon={<DollarSign className="h-5 w-5 text-blue-600" />}
            bg="bg-blue-50"
            label="Ticket médio"
            value={fmt(ticketMedio)}
            sub="por venda fechada"
          />
          <KpiCard
            icon={<ShoppingBag className="h-5 w-5 text-orange-600" />}
            bg="bg-orange-50"
            label="Comandas abertas"
            value={String(totalComandasAbertas)}
            sub={`${fmt(totalValorAberto)} em aberto`}
          />
          <KpiCard
            icon={<Bike className="h-5 w-5 text-purple-600" />}
            bg="bg-purple-50"
            label="Delivery pendente"
            value={String(totalDeliveryAbertos)}
            sub={`${fmt(totalValorDelivery)} aguardando`}
          />
        </div>

        {/* Gráfico de vendas por hora */}
        <div className="bg-white rounded-lg border border-gray-200 p-4">
          <div className="flex items-center justify-between mb-3">
            <h2 className="font-semibold text-gray-800 flex items-center gap-2">
              <BarChart2 className="h-4 w-4 text-indigo-500" />
              Vendas por hora (hoje)
            </h2>
            {horaPico && (
              <span className="text-xs text-gray-500 flex items-center gap-1">
                <Clock className="h-3 w-3" />
                Pico: {String(horaPico.hora).padStart(2, '0')}h ({horaPico.quantidade} vendas)
              </span>
            )}
          </div>
          {vendasPorHora.length === 0 ? (
            <p className="text-sm text-gray-400 text-center py-4">Nenhuma venda registrada hoje.</p>
          ) : (
            <div className="flex items-end gap-1 h-28">
              {Array.from({ length: 24 }, (_, h) => {
                const item = vendasPorHora.find((x) => x.hora === h);
                const qtd = item?.quantidade ?? 0;
                const height = maxQtdHora > 0 ? Math.round((qtd / maxQtdHora) * 100) : 0;
                const isPico = horaPico && h === horaPico.hora;
                return (
                  <div key={h} className="flex-1 flex flex-col items-center gap-0.5" title={`${String(h).padStart(2, '0')}h: ${qtd} vendas`}>
                    <div
                      className={`w-full rounded-t transition-all ${isPico ? 'bg-indigo-500' : 'bg-indigo-200'}`}
                      style={{ height: `${height}%`, minHeight: qtd > 0 ? 4 : 0 }}
                    />
                    {h % 4 === 0 && (
                      <span className="text-[9px] text-gray-400">{String(h).padStart(2, '0')}</span>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </div>

        {/* Top produtos + Vendas por tipo */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">

          {/* Top 5 produtos */}
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <h2 className="font-semibold text-gray-800 flex items-center gap-2 mb-3">
              <Package className="h-4 w-4 text-emerald-500" />
              Top 5 produtos (hoje)
            </h2>
            {produtos.length === 0 ? (
              <p className="text-sm text-gray-400">Nenhum produto vendido ainda.</p>
            ) : (
              <ul className="space-y-2">
                {produtos.map((p, i) => (
                  <li key={p.codigo ?? i} className="flex items-center justify-between text-sm">
                    <div className="flex items-center gap-2 min-w-0">
                      <span className="w-5 h-5 rounded-full bg-emerald-100 text-emerald-700 text-xs font-bold flex items-center justify-center flex-shrink-0">
                        {i + 1}
                      </span>
                      <span className="truncate text-gray-700">{p.descricao}</span>
                    </div>
                    <div className="flex flex-col items-end ml-2 flex-shrink-0">
                      <span className="font-semibold text-gray-800">{Number(p.quantidadeVendida ?? 0).toFixed(0)} un</span>
                      <span className="text-xs text-gray-400">{fmt(p.valorTotal ?? 0)}</span>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>

          {/* Resumo por tipo de origem */}
          <div className="bg-white rounded-lg border border-gray-200 p-4">
            <h2 className="font-semibold text-gray-800 mb-3">Resumo de vendas fechadas</h2>
            <div className="space-y-3">
              <ResumoRow
                label="Comandas"
                valor={valorTotal - (dados?.vendasPorHora ? 0 : 0)}
                icon={<ShoppingBag className="h-4 w-4 text-orange-500" />}
                cor="bg-orange-100 text-orange-700"
                info={`${totalVendas} venda${totalVendas !== 1 ? 's' : ''}`}
                valorReal={valorTotal}
              />
              <div className="border-t border-dashed border-gray-200 pt-3">
                <div className="flex justify-between text-sm font-semibold text-gray-800">
                  <span>Total faturado hoje</span>
                  <span className="text-green-700">{fmt(valorTotal)}</span>
                </div>
              </div>
              <div className="bg-gray-50 rounded-md p-2 text-xs text-gray-500">
                <p>* Dados das vendas fechadas com status <strong>EFETIVADO</strong></p>
                <p>* Atualizado automaticamente a cada minuto</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

// ── Componentes auxiliares ──────────────────────────────────────────────────

interface KpiCardProps {
  icon: React.ReactNode;
  bg: string;
  label: string;
  value: string;
  sub?: string;
}

const KpiCard: React.FC<KpiCardProps> = ({ icon, bg, label, value, sub }) => (
  <div className={`rounded-lg border border-gray-200 bg-white p-3 flex flex-col gap-2`}>
    <div className={`w-8 h-8 rounded-full ${bg} flex items-center justify-center`}>{icon}</div>
    <div>
      <p className="text-xs text-gray-500">{label}</p>
      <p className="text-lg font-bold text-gray-900 leading-tight">{value}</p>
      {sub && <p className="text-xs text-gray-400">{sub}</p>}
    </div>
  </div>
);

interface ResumoRowProps {
  label: string;
  valor: number;
  valorReal: number;
  icon: React.ReactNode;
  cor: string;
  info: string;
}

const ResumoRow: React.FC<ResumoRowProps> = ({ label, icon, cor, info, valorReal }) => (
  <div className="flex items-center justify-between text-sm">
    <div className="flex items-center gap-2">
      <span className={`w-6 h-6 rounded-full flex items-center justify-center ${cor}`}>{icon}</span>
      <div>
        <p className="font-medium text-gray-700">{label}</p>
        <p className="text-xs text-gray-400">{info}</p>
      </div>
    </div>
    <span className="font-semibold text-gray-800">{fmt(valorReal)}</span>
  </div>
);

export default DashboardPage;
