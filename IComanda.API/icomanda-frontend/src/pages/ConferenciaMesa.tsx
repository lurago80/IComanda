import { AnimatePresence, motion } from 'framer-motion';
import { ArrowLeft, Receipt, X } from 'lucide-react';
import React, { useState } from 'react';
import { conferenciaService, type ConferenciaMesa } from '../services/conferenciaService';

interface ConferenciaMesaPageProps {
  onClose: () => void;
}

const ConferenciaMesaPage: React.FC<ConferenciaMesaPageProps> = ({ onClose }) => {
  const [tipo, setTipo] = useState<'mesa' | 'comanda'>('mesa');
  const [numero, setNumero] = useState('');
  const [conferencia, setConferencia] = useState<ConferenciaMesa | null>(null);
  const [loading, setLoading] = useState(false);
  const [erro, setErro] = useState<string | null>(null);

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

  return (
    <div className="min-h-screen bg-gradient-to-br from-amber-50 via-orange-50 to-yellow-50 p-4">
      <div className="max-w-2xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <button
            onClick={onClose}
            className="flex items-center space-x-2 text-gray-700 hover:text-gray-900 transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
            <span className="font-semibold">Voltar</span>
          </button>

          <div className="flex items-center space-x-2">
            <Receipt className="w-6 h-6 text-amber-600" />
            <h1 className="text-2xl font-bold text-gray-800">Conferência</h1>
          </div>

          <div className="w-20"></div> {/* Spacer */}
        </div>

        {/* Busca */}
        <div className="bg-white rounded-3xl shadow-xl p-6 mb-6">
          <div className="space-y-4">
            {/* Seletor de Tipo */}
            <div className="flex space-x-3">
              <button
                onClick={() => setTipo('mesa')}
                className={`flex-1 py-3 rounded-xl font-semibold transition-all ${
                  tipo === 'mesa'
                    ? 'bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-md'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                Mesa
              </button>
              <button
                onClick={() => setTipo('comanda')}
                className={`flex-1 py-3 rounded-xl font-semibold transition-all ${
                  tipo === 'comanda'
                    ? 'bg-gradient-to-r from-amber-500 to-orange-500 text-white shadow-md'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                Comanda
              </button>
            </div>

            {/* Input e Botão */}
            <div className="flex space-x-3">
              <input
                type="number"
                value={numero}
                onChange={(e) => setNumero(e.target.value)}
                placeholder={`Número da ${tipo}`}
                className="flex-1 px-4 py-3 bg-gray-50 border-2 border-gray-200 rounded-xl focus:outline-none focus:border-amber-500 focus:ring-4 focus:ring-amber-100 transition-all text-gray-800 placeholder-gray-400"
                onKeyDown={(e) => e.key === 'Enter' && handleBuscar()}
                autoFocus
              />
              <button
                onClick={handleBuscar}
                disabled={loading}
                className="px-8 py-3 bg-gradient-to-r from-amber-500 to-orange-500 text-white font-bold rounded-xl hover:from-amber-600 hover:to-orange-600 transform hover:scale-[1.02] active:scale-[0.98] transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {loading ? 'Buscando...' : 'Buscar'}
              </button>
            </div>

            {/* Erro */}
            {erro && (
              <motion.div
                initial={{ opacity: 0, y: -10 }}
                animate={{ opacity: 1, y: 0 }}
                className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm"
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
              className="bg-white rounded-3xl shadow-2xl overflow-hidden"
            >
              {/* Cabeçalho do Cupom */}
              <div className="bg-gradient-to-r from-amber-500 to-orange-500 p-6 text-white text-center">
                <div className="text-4xl mb-2">🥐</div>
                <h2 className="text-2xl font-bold">PADARIA ICOMANDA</h2>
                <p className="text-amber-100 text-sm mt-1">Conferência de Conta</p>
              </div>

              {/* Informações */}
              <div className="p-6 border-b border-gray-200">
                <div className="grid grid-cols-2 gap-4 text-sm">
                  {conferencia.mesa && (
                    <div>
                      <span className="text-gray-600">Mesa:</span>
                      <span className="ml-2 font-bold text-gray-800">{conferencia.mesa}</span>
                    </div>
                  )}
                  {conferencia.comanda && (
                    <div>
                      <span className="text-gray-600">Comanda:</span>
                      <span className="ml-2 font-bold text-gray-800">{String(conferencia.comanda).padStart(6, '0')}</span>
                    </div>
                  )}
                  {conferencia.garcom && (
                    <div>
                      <span className="text-gray-600">Atendente:</span>
                      <span className="ml-2 font-semibold text-gray-800">{conferencia.garcom}</span>
                    </div>
                  )}
                  <div>
                    <span className="text-gray-600">Data/Hora:</span>
                    <span className="ml-2 font-semibold text-gray-800">{formatarData(conferencia.dataHora)}</span>
                  </div>
                </div>
              </div>

              {/* Itens */}
              <div className="p-6">
                <h3 className="text-lg font-bold text-gray-800 mb-4">Itens Consumidos</h3>
                <div className="space-y-3">
                  {conferencia.itens.map((item, index) => (
                    <div key={index} className="flex justify-between items-start py-2 border-b border-gray-100 last:border-0">
                      <div className="flex-1">
                        <p className="font-medium text-gray-800">{item.descricao}</p>
                        <p className="text-sm text-gray-600">
                          {item.qtd} x R$ {formatarPreco(item.precoUnitario)}
                        </p>
                      </div>
                      <div className="font-bold text-gray-800">
                        R$ {formatarPreco(item.total)}
                      </div>
                    </div>
                  ))}
                </div>
              </div>

              {/* Totais */}
              <div className="p-6 bg-amber-50 border-t border-amber-100">
                <div className="space-y-2">
                  <div className="flex justify-between text-gray-700">
                    <span>Subtotal:</span>
                    <span className="font-semibold">R$ {formatarPreco(conferencia.subtotal)}</span>
                  </div>
                  {conferencia.desconto > 0 && (
                    <div className="flex justify-between text-green-600">
                      <span>Desconto:</span>
                      <span className="font-semibold">- R$ {formatarPreco(conferencia.desconto)}</span>
                    </div>
                  )}
                  {conferencia.acrescimo > 0 && (
                    <div className="flex justify-between text-orange-600">
                      <span>Acréscimo:</span>
                      <span className="font-semibold">+ R$ {formatarPreco(conferencia.acrescimo)}</span>
                    </div>
                  )}
                  <div className="flex justify-between text-2xl font-bold text-gray-800 pt-3 border-t-2 border-amber-300">
                    <span>TOTAL:</span>
                    <span>R$ {formatarPreco(conferencia.total)}</span>
                  </div>
                </div>
              </div>

              {/* Footer */}
              <div className="p-4 bg-gray-50 text-center text-sm text-gray-600">
                <p>Esta é uma pré-conta. A conta será fechada no caixa.</p>
                <p className="mt-1 text-xs">Obrigado pela preferência! 🥐</p>
              </div>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
};

export default ConferenciaMesaPage;

