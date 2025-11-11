import { AnimatePresence, motion } from 'framer-motion';
import { ArrowLeft, Edit, Printer, Receipt, X } from 'lucide-react';
import React, { useState } from 'react';
import { conferenciaService, type ConferenciaMesa } from '../services/conferenciaService';
import { useCartStore } from '../store/cartStore';
import '../styles/print.css';

interface ConferenciaMesaPageProps {
  onClose: () => void;
}

const ConferenciaMesaPage: React.FC<ConferenciaMesaPageProps> = ({ onClose }) => {
  const [tipo, setTipo] = useState<'mesa' | 'comanda'>('mesa');
  const [numero, setNumero] = useState('');
  const [conferencia, setConferencia] = useState<ConferenciaMesa | null>(null);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const carregarPedidoParaEdicao = useCartStore((state) => state.carregarPedidoParaEdicao);

  const handleBuscar = async () => {
    if (!numero || numero.trim() === '') {
      setErro('Digite o número da mesa ou comanda');
      return;
    }

    setLoading(true);
    setErro(null);
    setConferencia(null);

    try {
      const num = parseInt(numero);
      let result: ConferenciaMesa;

      if (tipo === 'mesa') {
        result = await conferenciaService.getConferenciaMesa(num);
      } else {
        result = await conferenciaService.getConferenciaComanda(num);
      }

      setConferencia(result);
    } catch (error: any) {
      console.error('Erro ao buscar conferência:', error);
      if (error.response?.status === 404) {
        setErro(`${tipo === 'mesa' ? 'Mesa' : 'Comanda'} ${numero} não possui venda aberta`);
      } else {
        setErro('Erro ao buscar conferência. Tente novamente.');
      }
    } finally {
      setLoading(false);
    }
  };

  const formatarData = (dataHora: string) => {
    const data = new Date(dataHora);
    return data.toLocaleString('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatarPreco = (preco: number) => {
    return preco.toFixed(2).replace('.', ',');
  };

  const handleImprimir = () => {
    window.print();
  };

  const handleEditar = () => {
    if (!conferencia) return;

    // Carregar os itens da conferência no carrinho
    const vendaInfo: { nota: string; mesa?: number; comanda?: number } = {
      nota: String(conferencia.comanda || conferencia.mesa || ''),
      ...(conferencia.mesa && { mesa: conferencia.mesa }),
      ...(conferencia.comanda && { comanda: conferencia.comanda })
    };

    carregarPedidoParaEdicao(vendaInfo, conferencia.itens);

    // Aguardar um momento antes de fechar para garantir que o carrinho foi atualizado
    setTimeout(() => {
      onClose();
    }, 100);
  };

  return (
    <div className="min-h-screen bg-background p-4">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6 no-print">
          <button
            onClick={onClose}
            className="flex items-center space-x-2 text-text-secondary hover:text-text-primary transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
            <span className="font-semibold">Voltar</span>
          </button>

          <div className="flex items-center space-x-2">
            <Receipt className="w-6 h-6 text-primary" />
            <h1 className="text-2xl font-bold text-text-primary">Conferência</h1>
          </div>

          <div className="w-20"></div> {/* Spacer */}
        </div>

        {/* Busca */}
        <div className="bg-card rounded-3xl shadow-xl p-6 mb-6 border border-border no-print">
          <div className="space-y-4">
            {/* Seletor de Tipo */}
            <div className="flex space-x-3">
              <button
                onClick={() => setTipo('mesa')}
                className={`flex-1 py-3 rounded-xl font-semibold transition-all ${
                  tipo === 'mesa'
                    ? 'bg-primary text-primary-foreground shadow-md'
                    : 'bg-background-secondary text-text-secondary hover:bg-background-tertiary'
                }`}
              >
                Mesa
              </button>
              <button
                onClick={() => setTipo('comanda')}
                className={`flex-1 py-3 rounded-xl font-semibold transition-all ${
                  tipo === 'comanda'
                    ? 'bg-primary text-primary-foreground shadow-md'
                    : 'bg-background-secondary text-text-secondary hover:bg-background-tertiary'
                }`}
              >
                Comanda
              </button>
            </div>

            {/* Input e Botão */}
            <div className="flex items-center space-x-3">
              <input
                type="number"
                value={numero}
                onChange={(e) => setNumero(e.target.value)}
                placeholder={`Número da ${tipo}`}
                className="flex-1 px-4 py-3 bg-background-secondary border-2 border-border rounded-xl focus:outline-none focus:border-primary focus:ring-4 focus:ring-primary/10 transition-all text-text-primary placeholder-text-muted"
                onKeyDown={(e) => e.key === 'Enter' && handleBuscar()}
                autoFocus
              />
              <button
                onClick={handleBuscar}
                disabled={loading}
                className="flex-1 py-3 bg-primary text-primary-foreground font-bold rounded-xl hover:bg-primary/90 transform hover:scale-[1.02] active:scale-[0.98] transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed whitespace-nowrap"
              >
                {loading ? 'Buscando...' : 'Buscar'}
              </button>
            </div>

            {/* Erro */}
            {erro && (
              <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                className="p-4 bg-error/10 border border-error/30 rounded-xl text-error text-sm"
              >
                {erro}
              </motion.div>
            )}
          </div>
        </div>

        {/* Cupom Visual */}
        <AnimatePresence>
          {conferencia && (
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              exit={{ opacity: 0, scale: 0.95 }}
              className="bg-card rounded-3xl shadow-2xl overflow-hidden border border-border"
            >
              {/* Cabeçalho do Cupom */}
              <div className="bg-primary p-6 text-primary-foreground text-center">
                <div className="text-4xl mb-2">📋</div>
                <h2 className="text-2xl font-bold">PADARIA ICOMANDA</h2>
                <p className="text-primary-foreground/80 text-sm mt-1">Conferência de Conta</p>
              </div>

              {/* Informações */}
              <div className="p-6 border-b border-border">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  {conferencia.mesa && (
                    <div>
                      <span className="text-text-secondary">Mesa:</span>
                      <span className="ml-2 font-bold text-text-primary">{conferencia.mesa}</span>
                    </div>
                  )}
                  {conferencia.comanda && (
                    <div>
                      <span className="text-text-secondary">Comanda:</span>
                      <span className="ml-2 font-bold text-text-primary">{String(conferencia.comanda).padStart(6, '0')}</span>
                    </div>
                  )}
                  {conferencia.garcom && (
                    <div>
                      <span className="text-text-secondary">Atendente:</span>
                      <span className="ml-2 font-semibold text-text-primary">{conferencia.garcom}</span>
                    </div>
                  )}
                  <div>
                    <span className="text-text-secondary">Data/Hora:</span>
                    <span className="ml-2 font-semibold text-text-primary">{formatarData(conferencia.dataHora)}</span>
                  </div>
                </div>
              </div>

              {/* Itens */}
              <div className="p-6">
                <h3 className="text-lg font-bold text-text-primary mb-4">Itens Consumidos</h3>
                <div className="space-y-3">
                  {conferencia.itens.map((item, index) => (
                    <div key={index} className="flex justify-between items-start py-2 border-b border-border last:border-0">
                      <div className="flex-1">
                        <p className="font-medium text-text-primary">{item.descricao}</p>
                        <p className="text-sm text-text-secondary">
                          {item.qtd} x R$ {formatarPreco(item.precoUnitario)}
                        </p>
                      </div>
                      <div className="font-bold text-text-primary">
                        R$ {formatarPreco(item.total)}
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Totais */}
              <div className="p-6 bg-background-secondary border-t border-border">
                <div className="space-y-2">
                  <div className="flex justify-between text-text-secondary">
                    <span>Subtotal:</span>
                    <span className="font-semibold">R$ {formatarPreco(conferencia.subtotal)}</span>
                  </div>
                  {conferencia.desconto > 0 && (
                    <div className="flex justify-between text-success">
                      <span>Desconto:</span>
                      <span className="font-semibold">- R$ {formatarPreco(conferencia.desconto)}</span>
                    </div>
                  )}
                  {conferencia.acrescimo > 0 && (
                    <div className="flex justify-between text-error">
                      <span>Acréscimo:</span>
                      <span className="font-semibold">+ R$ {formatarPreco(conferencia.acrescimo)}</span>
                    </div>
                  )}
                  <div className="flex justify-between text-2xl font-bold text-text-primary pt-3 border-t-2 border-primary">
                    <span>TOTAL:</span>
                    <span>R$ {formatarPreco(conferencia.total)}</span>
                  </div>
                </div>
              </div>

              {/* Botões de Ação */}
              <div className="p-6 flex gap-3 border-t border-border no-print">
                <button
                  onClick={handleEditar}
                  className="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-accent text-accent-foreground font-bold rounded-xl hover:bg-accent/90 transform hover:scale-[1.02] active:scale-[0.98] transition-all shadow-md"
                >
                  <Edit className="w-5 h-5" />
                  Editar
                </button>
                <button
                  onClick={handleImprimir}
                  className="flex-1 flex items-center justify-center gap-2 px-6 py-3 bg-primary text-primary-foreground font-bold rounded-xl hover:bg-primary/90 transform hover:scale-[1.02] active:scale-[0.98] transition-all shadow-md"
                >
                  <Printer className="w-5 h-5" />
                  Imprimir
                </button>
              </div>

              {/* Footer */}
              <div className="p-4 bg-background-secondary text-center text-sm text-text-secondary border-t border-border no-print">
                <p>Esta é uma pré-conta. A conta será fechada no caixa.</p>
                <p className="mt-1 text-xs">Obrigado pela preferência! 📋</p>
              </div>
            </motion.div>
          )}
        </AnimatePresence>

        {/* Cupom para Impressão (Oculto na tela, visível apenas na impressão) */}
        {conferencia && (
          <div id="cupom-print" className="hidden print:block">
            {/* Cabeçalho */}
            <div className="print-header">
              <h1>PADARIA ICOMANDA</h1>
              <p>CONFERENCIA DE CONTA</p>
              <p>{formatarData(conferencia.dataHora)}</p>
            </div>

            {/* Informações */}
            <div className="print-info">
              {conferencia.mesa && (
                <div className="print-info-row">
                  <span>Mesa:</span>
                  <span>{conferencia.mesa}</span>
                </div>
              )}
              {conferencia.comanda && (
                <div className="print-info-row">
                  <span>Comanda:</span>
                  <span>{String(conferencia.comanda).padStart(6, '0')}</span>
                </div>
              )}
              {conferencia.garcom && (
                <div className="print-info-row">
                  <span>Atendente:</span>
                  <span>{conferencia.garcom}</span>
                </div>
              )}
              <div className="print-info-row">
                <span>Itens:</span>
                <span>{conferencia.totalItens}</span>
              </div>
            </div>

            {/* Separador */}
            <div className="print-separator"></div>

            {/* Itens */}
            <div className="print-items">
              {conferencia.itens.map((item, index) => (
                <div key={index} className="print-item">
                  <div className="print-item-header">{item.descricao}</div>
                  <div className="print-item-details">
                    <span>{item.qtd} x R$ {formatarPreco(item.precoUnitario)}</span>
                    <span>R$ {formatarPreco(item.total)}</span>
                  </div>
                </div>
              ))}
            </div>

            {/* Separador */}
            <div className="print-separator"></div>

            {/* Totais */}
            <div className="print-totals">
              <div className="print-total-row">
                <span>Subtotal:</span>
                <span>R$ {formatarPreco(conferencia.subtotal)}</span>
              </div>
              {conferencia.desconto > 0 && (
                <div className="print-total-row">
                  <span>Desconto:</span>
                  <span>- R$ {formatarPreco(conferencia.desconto)}</span>
                </div>
              )}
              {conferencia.acrescimo > 0 && (
                <div className="print-total-row">
                  <span>Acrescimo:</span>
                  <span>+ R$ {formatarPreco(conferencia.acrescimo)}</span>
                </div>
              )}
            </div>

            {/* Separador Duplo */}
            <div className="print-separator-double"></div>

            {/* Total Final */}
            <div className="print-total-final">
              <div className="print-total-row">
                <span>TOTAL:</span>
                <span>R$ {formatarPreco(conferencia.total)}</span>
              </div>
            </div>

            {/* Separador */}
            <div className="print-separator"></div>

            {/* Rodapé */}
            <div className="print-footer">
              <p>Esta e uma pre-conta.</p>
              <p>A conta sera fechada no caixa.</p>
              <p>Obrigado pela preferencia!</p>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ConferenciaMesaPage;

