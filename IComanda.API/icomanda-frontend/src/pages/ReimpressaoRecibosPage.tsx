import { motion } from 'framer-motion';
import { Search, X, Receipt, Printer, Loader2, FileText, Calendar } from 'lucide-react';
import React, { useState } from 'react';
import { useToast } from '../hooks/useToast';
import { recebimentosService, vendasService } from '../services/api';
import { Button } from '../components/ui/button';
import { Venda, VendaFechadaRecibo } from '../types/api';

interface ReimpressaoRecibosPageProps {
  onClose: () => void;
}

interface RecebimentoItem {
  id: number;
  idFormaPagamento: number;
  formaPagamentoDescricao: string;
  nCaixa: number;
  nota: string;
  valor: number;
  troco: number;
}

const ReimpressaoRecibosPage: React.FC<ReimpressaoRecibosPageProps> = ({ onClose }) => {
  const [modoBusca, setModoBusca] = useState<'individual' | 'periodo'>('individual');
  const [tipoBusca, setTipoBusca] = useState<'comanda' | 'nota'>('comanda');
  const [valorBusca, setValorBusca] = useState('');
  const [dataInicio, setDataInicio] = useState(
    new Date(new Date().setDate(new Date().getDate() - 7)).toISOString().split('T')[0]
  );
  const [dataFim, setDataFim] = useState(new Date().toISOString().split('T')[0]);
  const [venda, setVenda] = useState<Venda | null>(null);
  const [recebimentos, setRecebimentos] = useState<RecebimentoItem[]>([]);
  const [listaPeriodo, setListaPeriodo] = useState<VendaFechadaRecibo[]>([]);
  const [loading, setLoading] = useState(false);
  const [imprimindoNota, setImprimindoNota] = useState<string | null>(null);
  const { showError, showSuccess } = useToast();

  const buscar = async () => {
    setLoading(true);
    setVenda(null);
    setRecebimentos([]);
    setListaPeriodo([]);
    try {
      if (modoBusca === 'individual') {
        if (!valorBusca.trim()) {
          showError('Erro', 'Informe o número da comanda ou da nota');
          return;
        }
        let vendaEncontrada: Venda | null = null;
        let recs: RecebimentoItem[] = [];

        if (tipoBusca === 'comanda') {
          const comandaNum = parseInt(valorBusca);
          const vendasList = await vendasService.getByComanda(comandaNum);
          if (!vendasList?.length) {
            showError('Erro', 'Comanda não encontrada');
            return;
          }
          const vendaFechada = vendasList.find((v) => v.lancado !== 'ABERTO');
          if (!vendaFechada) {
            showError('Erro', 'Esta comanda ainda não foi fechada');
            return;
          }
          vendaEncontrada = await vendasService.getByNota(vendaFechada.nota);
          recs = await recebimentosService.getRecebimentosPorNota(vendaFechada.nota);
        } else {
          vendaEncontrada = await vendasService.getByNota(valorBusca.trim());
          if (!vendaEncontrada) {
            showError('Erro', 'Nota não encontrada');
            return;
          }
          recs = await recebimentosService.getRecebimentosPorNota(valorBusca.trim());
        }

        setVenda(vendaEncontrada);
        setRecebimentos(recs);
        showSuccess('Sucesso', 'Dados carregados. Clique em Reimprimir recibo para enviar à impressora.');
      } else {
        if (!dataInicio || !dataFim) {
          showError('Erro', 'Informe a data início e data fim');
          return;
        }
        const lista = await vendasService.getVendasFechadas(dataInicio, dataFim);
        setListaPeriodo(lista);
        showSuccess('Sucesso', `${lista.length} venda(s) fechada(s)/recebida(s) no período.`);
      }
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível buscar.');
    } finally {
      setLoading(false);
    }
  };

  const reimprimir = async (v: Venda, recs: RecebimentoItem[]) => {
    setImprimindoNota(v.nota);
    try {
      const itens = (v.itens || []).map((item) => ({
        codigo: item.codigo,
        descricao: item.descricao || `Item ${item.item}`,
        quantidade: item.qtd,
        preco: item.preco,
        observacao: undefined as string | undefined
      }));
      const isDelivery = (v.origem || '').toUpperCase() === 'DL';
      await vendasService.imprimir(v.nota, {
        itens,
        apenasNovosItens: false,
        comanda: v.comanda?.toString(),
        clienteNome: v.nomeCliente || (v.cliente ? `Cód. ${v.cliente}` : undefined),
        isReciboCliente: true,
        formasPagamento: recs.map((r) => ({ descricao: r.formaPagamentoDescricao, valor: r.valor })),
        trocoTotal: recs.reduce((s, r) => s + r.troco, 0) || undefined,
        isCupomDelivery: isDelivery,
        enderecoEntrega: isDelivery ? (v.enderecoEntrega || undefined) : undefined,
        pontoReferencia: isDelivery ? (v.pontoReferencia || undefined) : undefined,
        telefoneEntrega: isDelivery ? (v.telefoneCliente || undefined) : undefined,
        formaPgtoDelivery: isDelivery ? (v.formasPgto || undefined) : undefined,
        // Detectar "já pago": cobrar na entrega = false, qualquer outro pagamento = já pago
        jaPagoDelivery: isDelivery
          ? (v.formasPgto ? !v.formasPgto.toUpperCase().includes('COBRAR') : false)
          : false
      });
      showSuccess('Impressão', 'Recibo enviado para a impressora.');
    } catch (error: any) {
      showError(
        'Impressão',
        error.response?.data?.mensagem || 'Não foi possível imprimir. Verifique a impressora.'
      );
    } finally {
      setImprimindoNota(null);
    }
  };

  const formatarMoeda = (valor: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(valor);
  const formatarDataHora = (data: string) => {
    const d = new Date(data);
    return d.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
  };

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <Printer className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Reimpressão de Recibos</h1>
              <p className="text-sm text-text-muted">Vendas fechadas/recebidas — busque por comanda, nota ou período e reimprima o recibo</p>
            </div>
          </div>
          <Button onClick={onClose} variant="outline" className="flex items-center space-x-2">
            <X className="w-4 h-4" />
            <span className="hidden sm:inline">Fechar</span>
          </Button>
        </div>

        {/* Busca */}
        <motion.div
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border"
        >
          <div className="flex flex-wrap gap-4 items-end">
            <div className="flex items-center gap-4">
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  checked={modoBusca === 'individual'}
                  onChange={() => setModoBusca('individual')}
                  className="w-4 h-4 text-primary"
                />
                <span className="text-sm font-medium">Comanda / Nota</span>
              </label>
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="radio"
                  checked={modoBusca === 'periodo'}
                  onChange={() => setModoBusca('periodo')}
                  className="w-4 h-4 text-primary"
                />
                <span className="text-sm font-medium">Período</span>
              </label>
            </div>

            {modoBusca === 'individual' ? (
              <>
                <div className="flex items-center gap-2">
                  <label className="flex items-center gap-2">
                    <input
                      type="radio"
                      checked={tipoBusca === 'comanda'}
                      onChange={() => setTipoBusca('comanda')}
                      className="w-4 h-4 text-primary"
                    />
                    <span className="text-sm">Comanda</span>
                  </label>
                  <label className="flex items-center gap-2">
                    <input
                      type="radio"
                      checked={tipoBusca === 'nota'}
                      onChange={() => setTipoBusca('nota')}
                      className="w-4 h-4 text-primary"
                    />
                    <span className="text-sm">Nota</span>
                  </label>
                </div>
                <input
                  type="text"
                  value={valorBusca}
                  onChange={(e) => setValorBusca(e.target.value)}
                  placeholder={tipoBusca === 'comanda' ? 'Nº da comanda' : 'Nº da nota'}
                  className="w-32 px-3 py-2 border border-border rounded-lg bg-background"
                />
              </>
            ) : (
              <>
                <div className="flex items-center gap-2">
                  <Calendar className="w-4 h-4 text-text-muted" />
                  <input
                    type="date"
                    value={dataInicio}
                    onChange={(e) => setDataInicio(e.target.value)}
                    className="px-3 py-2 border border-border rounded-lg bg-background"
                  />
                  <span className="text-text-muted">até</span>
                  <input
                    type="date"
                    value={dataFim}
                    onChange={(e) => setDataFim(e.target.value)}
                    className="px-3 py-2 border border-border rounded-lg bg-background"
                  />
                </div>
              </>
            )}
            <Button onClick={buscar} disabled={loading} className="flex items-center gap-2">
              {loading ? <Loader2 className="w-4 h-4 animate-spin" /> : <Search className="w-4 h-4" />}
              Buscar
            </Button>
          </div>
        </motion.div>

        {/* Resultado individual */}
        {venda && recebimentos.length >= 0 && modoBusca === 'individual' && (
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-card rounded-2xl p-6 shadow-lg border border-border"
          >
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-lg font-bold text-text-primary flex items-center gap-2">
                <Receipt className="w-5 h-5 text-primary" />
                Recibo – Comanda #{venda.comanda ?? '–'} · Nota {venda.nota}
              </h2>
              <Button
                onClick={() => reimprimir(venda, recebimentos)}
                disabled={imprimindoNota === venda.nota}
                className="flex items-center gap-2"
              >
                {imprimindoNota === venda.nota ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : (
                  <Printer className="w-4 h-4" />
                )}
                {imprimindoNota === venda.nota ? 'Enviando...' : 'Reimprimir recibo'}
              </Button>
            </div>
            <p className="text-sm text-text-muted mb-2">{formatarDataHora(venda.emissao || '')}</p>
            {venda.nomeCliente && (
              <p className="text-sm text-text-primary mb-4">Cliente: {venda.nomeCliente}</p>
            )}
            <div className="border-t border-border pt-4 mt-4">
              <p className="text-sm font-semibold text-text-secondary mb-2">Itens</p>
              <ul className="space-y-1 mb-4">
                {venda.itens?.map((item, i) => (
                  <li key={i} className="flex justify-between text-sm">
                    <span>{item.qtd}x {item.descricao || `Item ${item.item}`}</span>
                    <span>{formatarMoeda(item.total)}</span>
                  </li>
                ))}
              </ul>
              <div className="flex justify-between font-semibold text-text-primary mb-2">
                <span>Total</span>
                <span>{formatarMoeda(venda.total || 0)}</span>
              </div>
              <p className="text-sm font-semibold text-text-secondary mt-3 mb-1">Formas de pagamento</p>
              <ul className="space-y-1">
                {recebimentos.map((r, i) => (
                  <li key={i} className="flex justify-between text-sm">
                    <span>{r.formaPagamentoDescricao}</span>
                    <span>{formatarMoeda(r.valor)}</span>
                  </li>
                ))}
              </ul>
              {recebimentos.some((r) => r.troco > 0) && (
                <div className="flex justify-between font-semibold text-text-primary mt-2">
                  <span>Troco</span>
                  <span>{formatarMoeda(recebimentos.reduce((s, r) => s + r.troco, 0))}</span>
                </div>
              )}
            </div>
          </motion.div>
        )}

        {/* Lista por período */}
        {modoBusca === 'periodo' && listaPeriodo.length > 0 && (
          <motion.div
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            className="space-y-4"
          >
            <h2 className="text-lg font-bold text-text-primary flex items-center gap-2">
              <FileText className="w-5 h-5 text-primary" />
              Vendas fechadas/recebidas no período ({listaPeriodo.length})
            </h2>
            {listaPeriodo.map((item) => (
              <div
                key={item.nota}
                className="bg-card rounded-xl p-4 shadow border border-border flex flex-wrap items-center justify-between gap-3"
              >
                <div>
                  <p className="font-semibold text-text-primary">
                    Comanda #{item.comanda ?? '–'} · Nota {item.nota}
                  </p>
                  <p className="text-sm text-text-muted">
                    {formatarDataHora(item.emissao)} · Total {formatarMoeda(item.total)}
                    {item.nomeCliente ? ` · ${item.nomeCliente}` : ''}
                  </p>
                </div>
                <Button
                  onClick={async () => {
                    const v = await vendasService.getByNota(item.nota);
                    if (v) await reimprimir(v, item.recebimentos);
                  }}
                  disabled={imprimindoNota === item.nota}
                  size="sm"
                  className="flex items-center gap-2"
                >
                  {imprimindoNota === item.nota ? (
                    <Loader2 className="w-4 h-4 animate-spin" />
                  ) : (
                    <Printer className="w-4 h-4" />
                  )}
                  {imprimindoNota === item.nota ? 'Enviando...' : 'Reimprimir'}
                </Button>
              </div>
            ))}
          </motion.div>
        )}

        {!loading && !venda && listaPeriodo.length === 0 && modoBusca === 'individual' && valorBusca.trim() && (
          <p className="text-center text-text-muted py-8">Busque por comanda ou nota e clique em Buscar.</p>
        )}
        {!loading && modoBusca === 'periodo' && listaPeriodo.length === 0 && dataInicio && dataFim && (
          <p className="text-center text-text-muted py-8">Nenhuma venda fechada/recebida encontrada no período.</p>
        )}
      </div>
    </div>
  );
};

export default ReimpressaoRecibosPage;
