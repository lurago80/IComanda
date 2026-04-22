import React, { useState, useEffect } from 'react';
import { Bell, ArrowLeft, CheckCircle, AlertCircle, Info, X, Filter } from 'lucide-react';
import { Button } from '../components/ui/button';
import { notificacoesService } from '../services/api';
import { useToast } from '../hooks/useToast';

interface NotificacoesPageProps {
  onClose: () => void;
}

interface Notificacao {
  id: number;
  titulo: string;
  mensagem: string;
  categoria: string;
  tipo: string;
  dataCriacao: string;
  dataLeitura?: string;
  lida: boolean;
  prioridade: string;
}

const NotificacoesPage: React.FC<NotificacoesPageProps> = ({ onClose }) => {
  const [notificacoes, setNotificacoes] = useState<Notificacao[]>([]);
  const [loading, setLoading] = useState(true);
  const [apenasNaoLidas, setApenasNaoLidas] = useState<boolean>(false);
  const [categoriaFiltro, setCategoriaFiltro] = useState<string>('');
  const { showError, showSuccess } = useToast();

  useEffect(() => {
    carregarNotificacoes();
  }, [apenasNaoLidas, categoriaFiltro]);

  const carregarNotificacoes = async () => {
    try {
      setLoading(true);
      const dados = await notificacoesService.listar(
        apenasNaoLidas || undefined,
        categoriaFiltro || undefined
      );
      setNotificacoes(dados);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar as notificações');
      console.error('Erro ao carregar notificações:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleMarcarComoLida = async (id: number) => {
    try {
      await notificacoesService.marcarComoLida(id);
      showSuccess('Sucesso', 'Notificação marcada como lida');
      carregarNotificacoes();
    } catch (error: any) {
      showError('Erro', 'Não foi possível marcar a notificação como lida');
    }
  };

  const handleMarcarTodasComoLidas = async () => {
    try {
      await notificacoesService.marcarTodasComoLidas();
      showSuccess('Sucesso', 'Todas as notificações foram marcadas como lidas');
      carregarNotificacoes();
    } catch (error: any) {
      showError('Erro', 'Não foi possível marcar todas as notificações como lidas');
    }
  };

  const formatarData = (data: string) => {
    return new Date(data).toLocaleString('pt-BR');
  };

  const getTipoIcon = (tipo: string) => {
    switch (tipo?.toUpperCase()) {
      case 'ALERTA':
      case 'WARNING':
        return <AlertCircle className="w-5 h-5 text-yellow-600" />;
      case 'ERRO':
      case 'ERROR':
        return <X className="w-5 h-5 text-red-600" />;
      case 'INFO':
      case 'INFORMACAO':
        return <Info className="w-5 h-5 text-blue-600" />;
      default:
        return <Bell className="w-5 h-5 text-primary" />;
    }
  };

  const getPrioridadeColor = (prioridade: string) => {
    switch (prioridade?.toUpperCase()) {
      case 'ALTA':
      case 'HIGH':
        return 'border-red-500 bg-red-50';
      case 'MEDIA':
      case 'MEDIUM':
        return 'border-yellow-500 bg-yellow-50';
      case 'BAIXA':
      case 'LOW':
        return 'border-blue-500 bg-blue-50';
      default:
        return 'border-gray-300 bg-gray-50';
    }
  };

  const notificacoesNaoLidas = notificacoes.filter((n) => !n.lida).length;

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <Bell className="w-8 h-8 text-primary" />
            <div>
              <h1 className="text-3xl font-bold">Notificações</h1>
              {notificacoesNaoLidas > 0 && (
                <p className="text-sm text-text-secondary">
                  {notificacoesNaoLidas} não lida{notificacoesNaoLidas > 1 ? 's' : ''}
                </p>
              )}
            </div>
          </div>
          <div className="flex items-center space-x-2">
            {notificacoesNaoLidas > 0 && (
              <Button onClick={handleMarcarTodasComoLidas} variant="outline">
                <CheckCircle className="w-4 h-4 mr-2" />
                Marcar Todas como Lidas
              </Button>
            )}
            <Button onClick={onClose} variant="outline">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Voltar
            </Button>
          </div>
        </div>

        {/* Filtros */}
        <div className="bg-card rounded-lg p-4 mb-6 shadow-lg">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="flex items-center space-x-2">
                <input
                  type="checkbox"
                  checked={apenasNaoLidas}
                  onChange={(e) => setApenasNaoLidas(e.target.checked)}
                  className="rounded"
                />
                <span className="text-sm">Apenas não lidas</span>
              </label>
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Categoria</label>
              <select
                value={categoriaFiltro}
                onChange={(e) => setCategoriaFiltro(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              >
                <option value="">Todas</option>
                <option value="VENDA">Venda</option>
                <option value="ESTOQUE">Estoque</option>
                <option value="CAIXA">Caixa</option>
                <option value="CLIENTE">Cliente</option>
                <option value="SISTEMA">Sistema</option>
              </select>
            </div>
            <div className="flex items-end">
              <Button onClick={carregarNotificacoes} className="w-full bg-primary">
                <Filter className="w-4 h-4 mr-2" />
                Filtrar
              </Button>
            </div>
          </div>
        </div>

        {/* Lista de Notificações */}
        {loading ? (
          <div className="text-center py-12">
            <p className="text-text-secondary">Carregando...</p>
          </div>
        ) : notificacoes.length === 0 ? (
          <div className="bg-card rounded-lg p-12 text-center shadow-lg">
            <Bell className="w-16 h-16 mx-auto mb-4 text-text-muted" />
            <p className="text-text-secondary">Nenhuma notificação encontrada</p>
          </div>
        ) : (
          <div className="space-y-4">
            {notificacoes.map((notificacao) => (
              <div
                key={notificacao.id}
                className={`bg-card rounded-lg p-6 shadow-lg border-l-4 transition-all ${
                  notificacao.lida
                    ? 'border-gray-300 opacity-75'
                    : getPrioridadeColor(notificacao.prioridade)
                }`}
              >
                <div className="flex items-start justify-between">
                  <div className="flex items-start space-x-4 flex-1">
                    <div className="mt-1">{getTipoIcon(notificacao.tipo)}</div>
                    <div className="flex-1">
                      <div className="flex items-center space-x-3 mb-2">
                        <h3 className="text-lg font-bold">{notificacao.titulo}</h3>
                        {!notificacao.lida && (
                          <span className="px-2 py-1 bg-primary text-primary-foreground rounded-full text-xs font-semibold">
                            Nova
                          </span>
                        )}
                        {notificacao.categoria && (
                          <span className="px-2 py-1 bg-gray-200 text-gray-800 rounded-full text-xs">
                            {notificacao.categoria}
                          </span>
                        )}
                        {notificacao.prioridade && (
                          <span
                            className={`px-2 py-1 rounded-full text-xs font-semibold ${
                              notificacao.prioridade.toUpperCase() === 'ALTA'
                                ? 'bg-red-100 text-red-800'
                                : notificacao.prioridade.toUpperCase() === 'MEDIA'
                                ? 'bg-yellow-100 text-yellow-800'
                                : 'bg-blue-100 text-blue-800'
                            }`}
                          >
                            {notificacao.prioridade}
                          </span>
                        )}
                      </div>
                      <p className="text-text-secondary mb-3">{notificacao.mensagem}</p>
                      <p className="text-xs text-text-muted">
                        {formatarData(notificacao.dataCriacao)}
                      </p>
                    </div>
                  </div>
                  {!notificacao.lida && (
                    <Button
                      onClick={() => handleMarcarComoLida(notificacao.id)}
                      variant="outline"
                      size="sm"
                      className="ml-4"
                    >
                      <CheckCircle className="w-4 h-4 mr-2" />
                      Marcar como Lida
                    </Button>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Resumo */}
        {notificacoes.length > 0 && (
          <div className="mt-6 bg-card rounded-lg p-4 shadow-lg">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center">
                <p className="text-sm text-text-secondary">Total</p>
                <p className="text-2xl font-bold">{notificacoes.length}</p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Não Lidas</p>
                <p className="text-2xl font-bold text-primary">{notificacoesNaoLidas}</p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Lidas</p>
                <p className="text-2xl font-bold text-green-600">
                  {notificacoes.filter((n) => n.lida).length}
                </p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Taxa de Leitura</p>
                <p className="text-2xl font-bold">
                  {notificacoes.length > 0
                    ? ((notificacoes.filter((n) => n.lida).length / notificacoes.length) * 100).toFixed(0)
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

export default NotificacoesPage;
