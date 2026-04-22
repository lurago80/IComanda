import React, { useState, useEffect } from 'react';
import { Table, ArrowLeft, RefreshCw, CheckCircle, XCircle } from 'lucide-react';
import { Button } from '../components/ui/button';
import { mesasService } from '../services/api';
import { useToast } from '../hooks/useToast';

interface MesasPageProps {
  onClose: () => void;
}

interface Mesa {
  id: number;
  numero: number;
  capacidade: number;
  status: string;
  ocupada: boolean;
  observacoes?: string;
}

const MesasPage: React.FC<MesasPageProps> = ({ onClose }) => {
  const [mesas, setMesas] = useState<Mesa[]>([]);
  const [loading, setLoading] = useState(true);
  const { showError, showSuccess } = useToast();

  useEffect(() => {
    carregarMesas();
  }, []);

  const carregarMesas = async () => {
    try {
      setLoading(true);
      const data = await mesasService.listar();
      setMesas(data);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar as mesas');
      console.error('Erro ao carregar mesas:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleToggleOcupacao = async (mesa: Mesa) => {
    try {
      await mesasService.atualizarOcupacao(mesa.id, !mesa.ocupada);
      showSuccess('Sucesso', `Mesa ${mesa.numero} ${!mesa.ocupada ? 'ocupada' : 'liberada'} com sucesso!`);
      carregarMesas();
    } catch (error: any) {
      showError('Erro', 'Não foi possível atualizar a ocupação da mesa');
    }
  };

  const handleAtualizarStatus = async (mesa: Mesa, novoStatus: string) => {
    try {
      await mesasService.atualizarStatus(mesa.id, novoStatus);
      showSuccess('Sucesso', `Status da mesa ${mesa.numero} atualizado!`);
      carregarMesas();
    } catch (error: any) {
      showError('Erro', 'Não foi possível atualizar o status da mesa');
    }
  };

  const getStatusColor = (status: string) => {
    switch (status?.toUpperCase()) {
      case 'DISPONIVEL':
        return 'bg-green-100 text-green-800';
      case 'OCUPADA':
        return 'bg-red-100 text-red-800';
      case 'RESERVADA':
        return 'bg-yellow-100 text-yellow-800';
      case 'MANUTENCAO':
        return 'bg-gray-100 text-gray-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <Table className="w-8 h-8 text-primary" />
            <h1 className="text-3xl font-bold">Gestão de Mesas</h1>
          </div>
          <div className="flex items-center space-x-2">
            <Button onClick={carregarMesas} variant="outline">
              <RefreshCw className="w-4 h-4 mr-2" />
              Atualizar
            </Button>
            <Button onClick={onClose} variant="outline">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Voltar
            </Button>
          </div>
        </div>

        {/* Grid de Mesas */}
        {loading ? (
          <div className="text-center py-12">
            <p className="text-text-secondary">Carregando...</p>
          </div>
        ) : mesas.length === 0 ? (
          <div className="bg-card rounded-lg p-12 text-center shadow-lg">
            <p className="text-text-secondary">Nenhuma mesa cadastrada</p>
          </div>
        ) : (
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4">
            {mesas.map((mesa) => (
              <div
                key={mesa.id}
                className={`bg-card rounded-lg p-4 shadow-lg border-2 transition-all hover:shadow-xl ${
                  mesa.ocupada
                    ? 'border-red-300 bg-red-50'
                    : 'border-green-300 bg-green-50'
                }`}
              >
                <div className="flex items-center justify-between mb-3">
                  <h3 className="text-xl font-bold">Mesa {mesa.numero}</h3>
                  {mesa.ocupada ? (
                    <XCircle className="w-5 h-5 text-red-600" />
                  ) : (
                    <CheckCircle className="w-5 h-5 text-green-600" />
                  )}
                </div>

                <div className="space-y-2 mb-4">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-text-secondary">Capacidade:</span>
                    <span className="font-semibold">{mesa.capacidade} pessoas</span>
                  </div>
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-text-secondary">Status:</span>
                    <span
                      className={`px-2 py-1 rounded-full text-xs font-semibold ${getStatusColor(
                        mesa.status
                      )}`}
                    >
                      {mesa.status || 'DISPONIVEL'}
                    </span>
                  </div>
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-text-secondary">Ocupada:</span>
                    <span className={`font-semibold ${mesa.ocupada ? 'text-red-600' : 'text-green-600'}`}>
                      {mesa.ocupada ? 'Sim' : 'Não'}
                    </span>
                  </div>
                </div>

                {mesa.observacoes && (
                  <div className="mb-4">
                    <p className="text-xs text-text-secondary italic">{mesa.observacoes}</p>
                  </div>
                )}

                <div className="space-y-2">
                  <Button
                    onClick={() => handleToggleOcupacao(mesa)}
                    className={`w-full ${
                      mesa.ocupada
                        ? 'bg-green-600 hover:bg-green-700'
                        : 'bg-red-600 hover:bg-red-700'
                    }`}
                    size="sm"
                  >
                    {mesa.ocupada ? 'Liberar Mesa' : 'Ocupar Mesa'}
                  </Button>

                  <div className="grid grid-cols-2 gap-2">
                    <Button
                      onClick={() => handleAtualizarStatus(mesa, 'DISPONIVEL')}
                      variant="outline"
                      size="sm"
                      className="text-xs"
                    >
                      Disponível
                    </Button>
                    <Button
                      onClick={() => handleAtualizarStatus(mesa, 'RESERVADA')}
                      variant="outline"
                      size="sm"
                      className="text-xs"
                    >
                      Reservar
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Resumo */}
        {mesas.length > 0 && (
          <div className="mt-6 bg-card rounded-lg p-4 shadow-lg">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center">
                <p className="text-sm text-text-secondary">Total de Mesas</p>
                <p className="text-2xl font-bold">{mesas.length}</p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Ocupadas</p>
                <p className="text-2xl font-bold text-red-600">
                  {mesas.filter((m) => m.ocupada).length}
                </p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Disponíveis</p>
                <p className="text-2xl font-bold text-green-600">
                  {mesas.filter((m) => !m.ocupada).length}
                </p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Taxa de Ocupação</p>
                <p className="text-2xl font-bold">
                  {mesas.length > 0
                    ? ((mesas.filter((m) => m.ocupada).length / mesas.length) * 100).toFixed(0)
                    : 0}
                  %
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default MesasPage;
