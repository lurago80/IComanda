import React, { useState, useEffect } from 'react';
import { History, ArrowLeft, Search, Calendar, User, FileText } from 'lucide-react';
import { Button } from '../components/ui/button';
import { historicoService } from '../services/api';
import { useToast } from '../hooks/useToast';

interface HistoricoPageProps {
  onClose: () => void;
}

interface HistoricoItem {
  id: number;
  entidade: string;
  idEntidade: number;
  acao: string;
  usuario: string;
  dataAlteracao: string;
  dadosAntigos?: string;
  dadosNovos?: string;
  observacoes?: string;
}

const HistoricoPage: React.FC<HistoricoPageProps> = ({ onClose }) => {
  const [historico, setHistorico] = useState<HistoricoItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [entidade, setEntidade] = useState<string>('');
  const [idEntidade, setIdEntidade] = useState<string>('');
  const [dataInicio, setDataInicio] = useState<string>(
    new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0]
  );
  const [dataFim, setDataFim] = useState<string>(new Date().toISOString().split('T')[0]);
  const { showError } = useToast();

  useEffect(() => {
    carregarHistorico();
  }, []);

  const carregarHistorico = async () => {
    try {
      setLoading(true);
      const dados = await historicoService.listar(
        entidade || undefined,
        idEntidade ? parseInt(idEntidade) : undefined,
        dataInicio || undefined,
        dataFim || undefined
      );
      setHistorico((Array.isArray(dados) ? dados : []).map((d: any) => ({
        id: d.id ?? 0,
        entidade: d.tipo ?? d.entidade ?? '',
        idEntidade: d.entidadeId ?? d.idEntidade ?? 0,
        acao: d.acao ?? '',
        usuario: d.nomeOperador ?? (d.usuario != null ? String(d.usuario) : String(d.operador ?? '')),
        dataAlteracao: d.dataHora ?? d.dataAlteracao ?? '',
        dadosAntigos: d.dadosAntigos,
        dadosNovos: d.dadosNovos,
        observacoes: d.descricao ?? d.observacoes
      })));
    } catch (error: any) {
      const msg = error?.response?.data?.mensagem || error?.message || 'Não foi possível carregar o histórico';
      showError('Erro ao carregar histórico', msg);
      console.error('Erro ao carregar histórico:', error);
    } finally {
      setLoading(false);
    }
  };

  const formatarData = (data: string) => {
    return new Date(data).toLocaleString('pt-BR');
  };

  const getAcaoColor = (acao: string) => {
    switch (acao?.toUpperCase()) {
      case 'CRIAR':
      case 'CREATE':
        return 'bg-green-100 text-green-800';
      case 'ATUALIZAR':
      case 'UPDATE':
        return 'bg-blue-100 text-blue-800';
      case 'DELETAR':
      case 'DELETE':
      case 'EXCLUIR':
        return 'bg-red-100 text-red-800';
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
            <History className="w-8 h-8 text-primary" />
            <h1 className="text-3xl font-bold">Histórico de Alterações</h1>
          </div>
          <Button onClick={onClose} variant="outline">
            <ArrowLeft className="w-4 h-4 mr-2" />
            Voltar
          </Button>
        </div>

        {/* Filtros */}
        <div className="bg-card rounded-lg p-4 mb-6 shadow-lg">
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">Entidade</label>
              <select
                value={entidade}
                onChange={(e) => setEntidade(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              >
                <option value="">Todas</option>
                <option value="VENDA">Venda</option>
                <option value="CLIENTE">Cliente</option>
                <option value="PRODUTO">Produto</option>
                <option value="CAIXA">Caixa</option>
                <option value="MESA">Mesa</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">ID da Entidade</label>
              <input
                type="number"
                value={idEntidade}
                onChange={(e) => setIdEntidade(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
                placeholder="Filtrar por ID"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Data Início</label>
              <input
                type="date"
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Data Fim</label>
              <input
                type="date"
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div className="flex items-end">
              <Button onClick={carregarHistorico} className="w-full bg-primary">
                <Search className="w-4 h-4 mr-2" />
                Buscar
              </Button>
            </div>
          </div>
        </div>

        {/* Lista de Histórico */}
        {loading ? (
          <div className="text-center py-12">
            <p className="text-text-secondary">Carregando...</p>
          </div>
        ) : historico.length === 0 ? (
          <div className="bg-card rounded-lg p-12 text-center shadow-lg">
            <p className="text-text-secondary mb-2">Nenhum registro encontrado no período.</p>
            <p className="text-sm text-text-muted">Se você esperava ver exclusões de comandas ou outras alterações, confira se o script <code className="bg-muted px-1 rounded">Scripts/criar_tabela_historico_alteracoes.sql</code> foi executado no banco Firebird.</p>
          </div>
        ) : (
          <div className="space-y-4">
            {historico.map((item) => (
              <div
                key={item.id}
                className="bg-card rounded-lg p-6 shadow-lg border-l-4 border-primary"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-4">
                      <span
                        className={`px-3 py-1 rounded-full text-sm font-semibold ${getAcaoColor(
                          item.acao
                        )}`}
                      >
                        {item.acao}
                      </span>
                      <span className="px-3 py-1 bg-gray-100 text-gray-800 rounded-full text-sm font-semibold">
                        {item.entidade}
                      </span>
                      <span className="text-sm text-text-secondary">ID: {item.idEntidade}</span>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                      <div>
                        <p className="text-sm text-text-secondary flex items-center">
                          <User className="w-4 h-4 mr-2" />
                          Usuário
                        </p>
                        <p className="font-semibold">{item.usuario || 'Sistema'}</p>
                      </div>
                      <div>
                        <p className="text-sm text-text-secondary flex items-center">
                          <Calendar className="w-4 h-4 mr-2" />
                          Data/Hora
                        </p>
                        <p className="font-semibold">{formatarData(item.dataAlteracao)}</p>
                      </div>
                    </div>

                    {item.observacoes && (
                      <div className="mb-4">
                        <p className="text-sm text-text-secondary">Observações</p>
                        <p className="text-sm">{item.observacoes}</p>
                      </div>
                    )}

                    {(item.dadosAntigos || item.dadosNovos) && (
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4 pt-4 border-t">
                        {item.dadosAntigos && (
                          <div>
                            <p className="text-sm text-text-secondary mb-2">Dados Antigos</p>
                            <div className="bg-red-50 p-3 rounded text-xs font-mono overflow-x-auto">
                              {item.dadosAntigos}
                            </div>
                          </div>
                        )}
                        {item.dadosNovos && (
                          <div>
                            <p className="text-sm text-text-secondary mb-2">Dados Novos</p>
                            <div className="bg-green-50 p-3 rounded text-xs font-mono overflow-x-auto">
                              {item.dadosNovos}
                            </div>
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Resumo */}
        {historico.length > 0 && (
          <div className="mt-6 bg-card rounded-lg p-4 shadow-lg">
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
              <div className="text-center">
                <p className="text-sm text-text-secondary">Total de Registros</p>
                <p className="text-2xl font-bold">{historico.length}</p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Criações</p>
                <p className="text-2xl font-bold text-green-600">
                  {historico.filter((h) => h.acao?.toUpperCase().includes('CRIAR') || h.acao?.toUpperCase().includes('CREATE')).length}
                </p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Atualizações</p>
                <p className="text-2xl font-bold text-blue-600">
                  {historico.filter((h) => h.acao?.toUpperCase().includes('ATUALIZAR') || h.acao?.toUpperCase().includes('UPDATE')).length}
                </p>
              </div>
              <div className="text-center">
                <p className="text-sm text-text-secondary">Exclusões</p>
                <p className="text-2xl font-bold text-red-600">
                  {historico.filter((h) => h.acao?.toUpperCase().includes('DELETAR') || h.acao?.toUpperCase().includes('DELETE')).length}
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default HistoricoPage;
