import React, { useState, useEffect } from 'react';
import { Table, ArrowLeft, RefreshCw, CheckCircle, XCircle, Clock, Users } from 'lucide-react';
import { Button } from '../components/ui/button';
import { mesasService } from '../services/api';

interface MesasPageProps {
  onClose: () => void;
}

interface MesaDto {
  numero: number;
  status: string; // LIVRE | OCUPADA
  comandaAtual?: number;
  notaAtual?: string;
  dataOcupacao?: string;
  horaOcupacao?: string;
  operador?: number;
  nomeOperador?: string;
  numeroPessoas?: number;
  cliente?: number;
  nomeCliente?: string;
  tempoOcupacao?: string;
  totalAtual?: number;
  quantidadeItens?: number;
}

const MesasPage: React.FC<MesasPageProps> = ({ onClose }) => {
  const [mesas, setMesas] = useState<MesaDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [erro, setErro] = useState<string | null>(null);

  useEffect(() => {
    carregarMesas();
  }, []);

  const carregarMesas = async () => {
    try {
      setLoading(true);
      setErro(null);
      const data = await mesasService.listar();
      setMesas(data);
    } catch (error: any) {
      setErro('Não foi possível carregar as mesas');
      console.error('Erro ao carregar mesas:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleLiberarMesa = async (numero: number) => {
    if (!window.confirm(`Confirma liberar a Mesa ${numero}?`)) return;
    try {
      await mesasService.liberar(numero);
      await carregarMesas();
    } catch (error: any) {
      setErro(`Erro ao liberar mesa ${numero}`);
    }
  };

  const formatarTempo = (tempo?: string): string => {
    if (!tempo) return '-';
    // tempo vem como "HH:MM:SS" ou TimeSpan
    const partes = tempo.split(':');
    if (partes.length < 2) return tempo;
    const horas = parseInt(partes[0]);
    const minutos = parseInt(partes[1]);
    if (horas > 0) return `${horas}h ${minutos}m`;
    return `${minutos}min`;
  };

  const mesasOcupadas = mesas.filter(m => m.status === 'OCUPADA');
  const mesasLivres = mesas.filter(m => m.status !== 'OCUPADA');

  return (
    <div className="min-h-screen bg-background p-4 md:p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <Table className="w-8 h-8 text-primary" />
            <div>
              <h1 className="text-2xl md:text-3xl font-bold">Gestão de Mesas</h1>
              <p className="text-sm text-text-secondary">{mesas.length} mesa(s) detectadas</p>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            <Button onClick={carregarMesas} variant="outline" size="sm">
              <RefreshCw className="w-4 h-4 mr-2" />
              Atualizar
            </Button>
            <Button onClick={onClose} variant="outline" size="sm">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Voltar
            </Button>
          </div>
        </div>

        {erro && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
            {erro}
          </div>
        )}

        {/* KPIs */}
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="bg-card rounded-lg p-4 shadow text-center">
            <p className="text-sm text-text-secondary">Total</p>
            <p className="text-3xl font-bold">{mesas.length}</p>
          </div>
          <div className="bg-card rounded-lg p-4 shadow text-center">
            <p className="text-sm text-text-secondary">Ocupadas</p>
            <p className="text-3xl font-bold text-red-600">{mesasOcupadas.length}</p>
          </div>
          <div className="bg-card rounded-lg p-4 shadow text-center">
            <p className="text-sm text-text-secondary">Livres</p>
            <p className="text-3xl font-bold text-green-600">{mesasLivres.length}</p>
          </div>
        </div>

        {loading ? (
          <div className="text-center py-16">
            <p className="text-text-secondary">Carregando mesas...</p>
          </div>
        ) : mesas.length === 0 ? (
          <div className="bg-card rounded-lg p-12 text-center shadow-lg">
            <Table className="w-12 h-12 text-text-secondary mx-auto mb-4" />
            <p className="text-text-secondary">Nenhuma mesa encontrada</p>
            <p className="text-sm text-text-secondary mt-1">As mesas aparecem conforme são utilizadas em comandas</p>
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3">
            {mesas.map((mesa) => (
              <div
                key={mesa.numero}
                className={`rounded-lg p-3 shadow border-2 transition-all hover:shadow-md ${
                  mesa.status === 'OCUPADA'
                    ? 'border-red-300 bg-red-50'
                    : 'border-green-300 bg-green-50'
                }`}
              >
                <div className="flex items-center justify-between mb-2">
                  <h3 className="text-lg font-bold">Mesa {mesa.numero}</h3>
                  {mesa.status === 'OCUPADA' ? (
                    <XCircle className="w-5 h-5 text-red-600" />
                  ) : (
                    <CheckCircle className="w-5 h-5 text-green-600" />
                  )}
                </div>

                <div className={`text-xs font-semibold px-2 py-0.5 rounded-full inline-block mb-2 ${
                  mesa.status === 'OCUPADA' ? 'bg-red-200 text-red-800' : 'bg-green-200 text-green-800'
                }`}>
                  {mesa.status}
                </div>

                {mesa.status === 'OCUPADA' && (
                  <div className="space-y-1 text-xs text-gray-600 mb-3">
                    {mesa.comandaAtual && (
                      <div className="flex justify-between">
                        <span>Comanda:</span>
                        <span className="font-semibold">{mesa.comandaAtual}</span>
                      </div>
                    )}
                    {mesa.numeroPessoas && (
                      <div className="flex items-center justify-between">
                        <span className="flex items-center gap-1"><Users className="w-3 h-3" />Pessoas:</span>
                        <span className="font-semibold">{mesa.numeroPessoas}</span>
                      </div>
                    )}
                    {mesa.tempoOcupacao && (
                      <div className="flex items-center justify-between">
                        <span className="flex items-center gap-1"><Clock className="w-3 h-3" />Tempo:</span>
                        <span className="font-semibold">{formatarTempo(mesa.tempoOcupacao)}</span>
                      </div>
                    )}
                    {mesa.totalAtual !== undefined && mesa.totalAtual !== null && (
                      <div className="flex justify-between">
                        <span>Total:</span>
                        <span className="font-semibold text-red-700">
                          R$ {mesa.totalAtual.toFixed(2)}
                        </span>
                      </div>
                    )}
                    {mesa.nomeOperador && (
                      <div className="flex justify-between">
                        <span>Operador:</span>
                        <span className="font-semibold truncate max-w-[80px]">{mesa.nomeOperador}</span>
                      </div>
                    )}
                  </div>
                )}

                {mesa.status === 'OCUPADA' && (
                  <Button
                    onClick={() => handleLiberarMesa(mesa.numero)}
                    className="w-full bg-green-600 hover:bg-green-700 text-white"
                    size="sm"
                  >
                    Liberar
                  </Button>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default MesasPage;
