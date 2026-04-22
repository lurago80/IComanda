import React, { useState, useEffect } from 'react';
import { X, ArrowRight, Search, Loader2 } from 'lucide-react';
import { vendasService } from '../services/api';
import { Venda } from '../types/api';
import { useCartStore } from '../store/cartStore';

interface TransferItemModalProps {
  isOpen: boolean;
  onClose: () => void;
  notaOrigem: string;
  itemOrigem: number;
  descricaoItem: string;
  onSuccess: () => void;
}

const TransferItemModal: React.FC<TransferItemModalProps> = ({
  isOpen,
  onClose,
  notaOrigem,
  itemOrigem,
  descricaoItem,
  onSuccess,
}) => {
  const [comandasAbertas, setComandasAbertas] = useState<Venda[]>([]);
  const [comandaSelecionada, setComandaSelecionada] = useState<number | null>(null);
  const [buscaComanda, setBuscaComanda] = useState('');
  const [carregando, setCarregando] = useState(false);
  const [transferindo, setTransferindo] = useState(false);
  const [erro, setErro] = useState<string | null>(null);
  const { vendaEmEdicao } = useCartStore();

  useEffect(() => {
    if (isOpen) {
      carregarComandasAbertas();
    }
  }, [isOpen]);

  const carregarComandasAbertas = async () => {
    setCarregando(true);
    setErro(null);
    try {
      const vendas = await vendasService.getAbertas();
      // Filtrar a venda de origem
      const vendasFiltradas = vendas.filter(v => v.nota !== notaOrigem);
      setComandasAbertas(vendasFiltradas);
    } catch (error: any) {
      setErro('Erro ao carregar comandas abertas: ' + (error.message || 'Erro desconhecido'));
    } finally {
      setCarregando(false);
    }
  };

  const handleTransferir = async () => {
    if (!comandaSelecionada) {
      setErro('Selecione uma comanda de destino');
      return;
    }

    // Buscar a nota da comanda selecionada
    const vendaDestino = comandasAbertas.find(v => v.comanda === comandaSelecionada);
    if (!vendaDestino) {
      setErro('Comanda selecionada não encontrada');
      return;
    }

    setTransferindo(true);
    setErro(null);

    try {
      // Obter operador (usar padrão 1, pois VendaEmEdicao não contém operador)
      const operador = 1;

      await vendasService.transferirItem({
        notaOrigem,
        itemOrigem,
        notaDestino: vendaDestino.nota,
        operador,
      });

      onSuccess();
      onClose();
    } catch (error: any) {
      setErro(error.response?.data?.mensagem || error.message || 'Erro ao transferir item');
    } finally {
      setTransferindo(false);
    }
  };

  const comandasFiltradas = comandasAbertas.filter(v => {
    if (!buscaComanda) return true;
    const busca = buscaComanda.toLowerCase();
    return (
      v.comanda?.toString().includes(busca) ||
      v.mesa?.toString().includes(busca) ||
      v.nota?.toLowerCase().includes(busca)
    );
  });

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md mx-4 max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <div>
            <h2 className="text-xl font-bold text-gray-900">Transferir Item</h2>
            <p className="text-sm text-gray-500 mt-1">{descricaoItem}</p>
          </div>
          <button
            onClick={onClose}
            className="w-8 h-8 flex items-center justify-center rounded-lg hover:bg-gray-100 transition-colors"
          >
            <X className="w-5 h-5 text-gray-500" />
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          {/* Informações do item */}
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-4">
            <p className="text-sm text-gray-600 mb-1">
              <span className="font-semibold">Item:</span> {descricaoItem}
            </p>
            <p className="text-sm text-gray-600">
              <span className="font-semibold">Comanda origem:</span> {vendaEmEdicao?.comanda || 'N/A'}
            </p>
          </div>

          {/* Busca */}
          <div className="relative mb-4">
            <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
            <input
              type="text"
              placeholder="Buscar por comanda, mesa ou cliente..."
              value={buscaComanda}
              onChange={(e) => setBuscaComanda(e.target.value)}
              className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            />
          </div>

          {/* Lista de comandas */}
          {carregando ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="w-6 h-6 animate-spin text-primary-600" />
              <span className="ml-2 text-gray-600">Carregando comandas...</span>
            </div>
          ) : erro && !comandasAbertas.length ? (
            <div className="bg-red-50 border border-red-200 rounded-lg p-4">
              <p className="text-sm text-red-600">{erro}</p>
            </div>
          ) : comandasFiltradas.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <p>Nenhuma comanda aberta encontrada</p>
            </div>
          ) : (
            <div className="space-y-2">
              {comandasFiltradas.map((venda, index) => (
                <button
                  key={venda.nota && String(venda.nota).trim() ? venda.nota : `venda-${index}`}
                  onClick={() => setComandaSelecionada(venda.comanda || null)}
                  className={`w-full text-left p-4 rounded-lg border-2 transition-all ${
                    comandaSelecionada === venda.comanda
                      ? 'border-primary-500 bg-primary-50'
                      : 'border-gray-200 hover:border-gray-300 hover:bg-gray-50'
                  }`}
                >
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="font-semibold text-gray-900">
                        Comanda {venda.comanda || 'N/A'}
                        {venda.mesa && ` • Mesa ${venda.mesa}`}
                      </p>
                      <p className="text-xs text-gray-500 mt-1">
                        Nota: {venda.nota}
                      </p>
                      <p className="text-sm text-gray-500 mt-1">
                        {venda.itens?.length || 0} itens • R$ {venda.total?.toFixed(2).replace('.', ',') || '0,00'}
                      </p>
                    </div>
                    {comandaSelecionada === venda.comanda && (
                      <div className="w-6 h-6 bg-primary-600 rounded-full flex items-center justify-center">
                        <div className="w-2 h-2 bg-white rounded-full" />
                      </div>
                    )}
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="border-t border-gray-200 p-6">
          {erro && comandasAbertas.length > 0 && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
              <p className="text-sm text-red-600">{erro}</p>
            </div>
          )}
          <div className="flex gap-3">
            <button
              onClick={onClose}
              disabled={transferindo}
              className="flex-1 px-4 py-3 border border-gray-300 rounded-lg font-semibold text-gray-700 hover:bg-gray-50 transition-colors disabled:opacity-50"
            >
              Cancelar
            </button>
            <button
              onClick={handleTransferir}
              disabled={!comandaSelecionada || transferindo}
              className="flex-1 px-4 py-3 bg-primary-600 text-white rounded-lg font-semibold hover:bg-primary-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center gap-2"
            >
              {transferindo ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  Transferindo...
                </>
              ) : (
                <>
                  <ArrowRight className="w-4 h-4" />
                  Transferir
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TransferItemModal;

