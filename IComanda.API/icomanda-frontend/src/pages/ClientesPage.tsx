import { AnimatePresence, motion } from 'framer-motion';
import { 
  AlertCircle,
  Loader2, 
  Trash2,
  X, 
  Search, 
  User,
  UserPlus,
  Edit,
  FileText,
  Phone,
  Mail,
  MapPin
} from 'lucide-react';
import React, { useEffect, useState } from 'react';
import { useToast } from '../hooks/useToast';
import { clientesService } from '../services/api';
import { Button } from '../components/ui/button';
import { Cliente } from '../types/api';
import { useDebounce } from '../hooks/useDebounce';

interface ClientesPageProps {
  onClose: () => void;
  onNovoCliente?: () => void;
  onEditarCliente?: (cliente: Cliente) => void;
}

const ClientesPage: React.FC<ClientesPageProps> = ({ onClose, onNovoCliente, onEditarCliente }) => {
  const [clientes, setClientes] = useState<Cliente[]>([]);
  const [loading, setLoading] = useState(false);
  const [busca, setBusca] = useState<string>('');
  const [apenasAtivos, setApenasAtivos] = useState<boolean>(true);
  const debouncedBusca = useDebounce(busca, 500);

  const [clienteParaExcluir, setClienteParaExcluir] = useState<Cliente | null>(null);
  const [excluindo, setExcluindo] = useState(false);
  const [erroExclusao, setErroExclusao] = useState<string | null>(null);

  const { showError } = useToast();

  const carregarClientes = React.useCallback(async () => {
    try {
      setLoading(true);
      const dados = await clientesService.buscar({
        q: debouncedBusca || undefined,
        ativo: apenasAtivos ? true : undefined,
        naoBloqueado: true,
        itensPorPagina: 100
      });
      setClientes(dados);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar os clientes');
      console.error('Erro ao carregar clientes:', error);
    } finally {
      setLoading(false);
    }
  }, [debouncedBusca, apenasAtivos, showError]);

  useEffect(() => {
    carregarClientes();
  }, [carregarClientes]);

  const formatarDocumento = (doc?: string) => {
    if (!doc) return '';
    const limpo = doc.replace(/\D/g, '');
    if (limpo.length === 11) {
      return limpo.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } else if (limpo.length === 14) {
      return limpo.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }
    return doc;
  };

  const handleExcluir = async () => {
    if (!clienteParaExcluir) return;
    setExcluindo(true);
    setErroExclusao(null);
    try {
      await clientesService.excluir(clienteParaExcluir.id);
      setClienteParaExcluir(null);
      await carregarClientes();
    } catch (error: any) {
      const msg =
        error?.response?.data?.mensagem ||
        error?.message ||
        'Erro ao excluir cliente';
      setErroExclusao(msg);
    } finally {
      setExcluindo(false);
    }
  };

  const formatarTelefone = (tel?: string) => {
    if (!tel) return '';
    const limpo = tel.replace(/\D/g, '');
    if (limpo.length === 10) {
      return limpo.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
    } else if (limpo.length === 11) {
      return limpo.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    }
    return tel;
  };

  return (
    <>
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <User className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Clientes</h1>
              <p className="text-sm text-text-muted">Consulta e gerenciamento de clientes</p>
            </div>
          </div>
          <div className="flex gap-2">
            {onNovoCliente && (
              <Button
                onClick={onNovoCliente}
                className="flex items-center space-x-2"
              >
                <UserPlus className="w-4 h-4" />
                <span className="hidden sm:inline">Novo Cliente</span>
              </Button>
            )}
            <Button
              onClick={onClose}
              variant="outline"
              className="flex items-center space-x-2"
            >
              <X className="w-4 h-4" />
              <span className="hidden sm:inline">Fechar</span>
            </Button>
          </div>
        </div>

        {/* Filtros */}
        <div className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border">
          <div className="flex flex-col sm:flex-row gap-4 mb-4">
            <div className="flex-1">
              <label className="block text-sm font-medium text-text-secondary mb-2">
                Buscar Cliente
              </label>
              <div className="relative">
                <input
                  type="text"
                  value={busca}
                  onChange={(e) => setBusca(e.target.value)}
                  className="w-full px-4 py-3 pl-10 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Nome, CPF/CNPJ, telefone..."
                />
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-text-muted" />
              </div>
            </div>
            <div className="flex items-end">
              <label className="flex items-center space-x-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={apenasAtivos}
                  onChange={(e) => setApenasAtivos(e.target.checked)}
                  className="w-4 h-4 text-primary rounded"
                />
                <span className="text-sm text-text-secondary">Apenas ativos</span>
              </label>
            </div>
          </div>
        </div>

        {/* Lista de Clientes */}
        {loading ? (
          <div className="bg-card rounded-2xl p-12 text-center shadow-lg border border-border">
            <Loader2 className="w-12 h-12 text-primary mx-auto mb-4 animate-spin" />
            <p className="text-text-muted">Carregando clientes...</p>
          </div>
        ) : clientes.length === 0 ? (
          <div className="bg-card rounded-2xl p-12 text-center shadow-lg border border-border">
            <User className="w-16 h-16 text-text-muted mx-auto mb-4" />
            <p className="text-text-muted">Nenhum cliente encontrado</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {clientes.map((cliente, index) => (
              <motion.div
                key={cliente.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.05 }}
                className="bg-card rounded-2xl p-6 shadow-lg border border-border hover:shadow-xl transition-shadow"
              >
                <div className="flex items-start justify-between mb-4">
                  <div className="flex-1">
                    <h3 className="font-bold text-lg text-text-primary mb-1">
                      {cliente.nome}
                    </h3>
                    {cliente.fantasia && (
                      <p className="text-sm text-text-secondary italic">
                        {cliente.fantasia}
                      </p>
                    )}
                  </div>
                  <div className={`w-3 h-3 rounded-full ${cliente.ativo ? 'bg-green-500' : 'bg-gray-400'}`} />
                </div>

                <div className="space-y-2 mb-4">
                  {cliente.cpfCnpj && (
                    <div className="flex items-center text-sm text-text-secondary">
                      <FileText className="w-4 h-4 mr-2" />
                      {formatarDocumento(cliente.cpfCnpj)}
                    </div>
                  )}
                  {cliente.telefone && (
                    <div className="flex items-center text-sm text-text-secondary">
                      <Phone className="w-4 h-4 mr-2" />
                      {formatarTelefone(cliente.telefone)}
                    </div>
                  )}
                  {cliente.email && (
                    <div className="flex items-center text-sm text-text-secondary">
                      <Mail className="w-4 h-4 mr-2" />
                      {cliente.email}
                    </div>
                  )}
                  {cliente.cidade1 && (
                    <div className="flex items-center text-sm text-text-secondary">
                      <MapPin className="w-4 h-4 mr-2" />
                      {cliente.cidade1}{cliente.uf1 && `, ${cliente.uf1}`}
                    </div>
                  )}
                </div>

                <div className="flex gap-2">
                  {onEditarCliente && (
                    <Button
                      onClick={() => onEditarCliente(cliente)}
                      variant="outline"
                      className="flex-1 flex items-center justify-center space-x-2"
                    >
                      <Edit className="w-4 h-4" />
                      <span>Editar</span>
                    </Button>
                  )}
                  <Button
                    onClick={() => { setClienteParaExcluir(cliente); setErroExclusao(null); }}
                    variant="outline"
                    className="flex items-center justify-center px-3 text-red-500 border-red-300 hover:bg-red-50 hover:border-red-500 dark:hover:bg-red-950"
                    title="Excluir cliente"
                  >
                    <Trash2 className="w-4 h-4" />
                  </Button>
                </div>
              </motion.div>
            ))}
          </div>
        )}
      </div>
    </div>

      {/* Modal de confirmão de exclusão */}
      <AnimatePresence>
        {clienteParaExcluir && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4"
            onClick={(e) => { if (e.target === e.currentTarget && !excluindo) setClienteParaExcluir(null); }}
          >
            <motion.div
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className="bg-card rounded-2xl p-6 shadow-2xl border border-border w-full max-w-md"
            >
              <div className="flex items-center space-x-3 mb-4">
                <div className="w-12 h-12 bg-red-100 dark:bg-red-900/30 rounded-xl flex items-center justify-center">
                  <Trash2 className="w-6 h-6 text-red-500" />
                </div>
                <div>
                  <h2 className="text-xl font-bold text-text-primary">Excluir Cliente</h2>
                  <p className="text-sm text-text-muted">Esta ação não pode ser desfeita</p>
                </div>
              </div>

              <p className="text-text-secondary mb-2">
                Deseja excluir permanentemente o cliente:
              </p>
              <p className="font-bold text-text-primary mb-4">{clienteParaExcluir.nome}</p>

              {erroExclusao && (
                <div className="flex items-start space-x-2 bg-red-50 dark:bg-red-900/20 border border-red-300 rounded-xl p-3 mb-4">
                  <AlertCircle className="w-4 h-4 text-red-500 mt-0.5 flex-shrink-0" />
                  <p className="text-sm text-red-700 dark:text-red-400">{erroExclusao}</p>
                </div>
              )}

              <div className="flex gap-3">
                <Button
                  onClick={() => setClienteParaExcluir(null)}
                  variant="outline"
                  className="flex-1"
                  disabled={excluindo}
                >
                  Cancelar
                </Button>
                <Button
                  onClick={handleExcluir}
                  className="flex-1 bg-red-500 hover:bg-red-600 text-white flex items-center justify-center space-x-2"
                  disabled={excluindo}
                >
                  {excluindo ? (
                    <Loader2 className="w-4 h-4 animate-spin" />
                  ) : (
                    <Trash2 className="w-4 h-4" />
                  )}
                  <span>{excluindo ? 'Excluindo...' : 'Excluir'}</span>
                </Button>
              </div>
            </motion.div>
          </motion.div>
        )}
      </AnimatePresence>
    </>
  );
};

export default ClientesPage;
