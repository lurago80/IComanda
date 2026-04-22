import { useQuery } from '@tanstack/react-query';
import { AnimatePresence, motion } from 'framer-motion';
import { Check, MapPin, Phone, Search, User, X } from 'lucide-react';
import React, { useState } from 'react';
import { useDebounce } from '../hooks/useDebounce';
import { useToast } from '../hooks/useToast';
import { clientesService, receberService } from '../services/api';
import { Cliente } from '../types/api';
import EmptyState from './states/EmptyState';

interface ClienteSearchProps {
  isOpen: boolean;
  onClose: () => void;
  onSelectCliente: (cliente: Cliente) => void;
}

const ClienteSearch: React.FC<ClienteSearchProps> = ({ isOpen, onClose, onSelectCliente }) => {
  const [searchQuery, setSearchQuery] = useState('');
  const debouncedQuery = useDebounce(searchQuery, 300);
  const { showWarning } = useToast();

  const { data: clientes, isLoading, error, isFetching, refetch } = useQuery({
    queryKey: ['clientes', 'busca', debouncedQuery.trim()],
    queryFn: () => clientesService.buscar({ 
      q: debouncedQuery.trim() || undefined, 
      ativo: true, 
      naoBloqueado: true,
      itensPorPagina: 50 
    }),
    enabled: isOpen && debouncedQuery.trim().length >= 1,
    staleTime: 30 * 1000,
    retry: 1,
  });

  const handleSelectCliente = async (cliente: Cliente) => {
    try {
      const contas = await receberService.verificarContasAberto(cliente.id);
      if (contas.temContasAberto && contas.valorTotalPendente > 0) {
        const valorFormatado = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(contas.valorTotalPendente);
        showWarning(
          'Cliente com valor em aberto',
          `${cliente.nomeCompleto} possui ${contas.quantidadeContas} conta(s) a receber em aberto: ${valorFormatado}. Lembrete: informe na hora de fechar a venda.`
        );
      }
    } catch {
      // Ignorar erro na verificação; não bloquear seleção
    }
    onSelectCliente(cliente);
    onClose();
  };

  const formatarDocumento = (documento?: string) => {
    if (!documento) return '';
    // Formatar CPF/CNPJ
    if (documento.length === 11) {
      return documento.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } else if (documento.length === 14) {
      return documento.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }
    return documento;
  };

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/50 z-[120]"
            onClick={onClose}
          />

          {/* Modal */}
          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: "spring", stiffness: 300, damping: 30 }}
            className="fixed bottom-0 left-0 right-0 bg-white border-t border-amber-200 rounded-t-3xl z-[120] max-h-[85vh] flex flex-col shadow-2xl"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-6 border-b border-amber-100">
              <div className="flex items-center space-x-3">
                <div className="w-10 h-10 bg-amber-100 rounded-2xl flex items-center justify-center">
                  <User className="w-5 h-5 text-amber-600" />
                </div>
                <div>
                  <h2 className="text-xl font-bold text-gray-800">Buscar Cliente</h2>
                  <p className="text-sm text-gray-600">Digite nome, CPF/CNPJ ou telefone</p>
                </div>
              </div>
              <button
                onClick={onClose}
                className="w-10 h-10 bg-amber-50 border border-amber-200 rounded-2xl flex items-center justify-center text-gray-600 hover:text-gray-800 hover:bg-amber-100 transition-all duration-300"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Barra de Busca */}
            <div className="p-6 border-b border-amber-100 bg-amber-50/30">
              <div className="relative">
                <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-400" />
                <input
                  type="text"
                  placeholder="Digite nome, CPF/CNPJ ou telefone..."
                  className="w-full pl-12 pr-4 py-3 bg-white border-2 border-amber-200 rounded-xl focus:outline-none focus:border-amber-500 focus:ring-4 focus:ring-amber-100 transition-all text-gray-800 placeholder-gray-400"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value.replace(/\s+/g, ' ').trimStart())}
                  autoFocus
                />
              </div>
            </div>

            {/* Lista de Clientes */}
            <div className="flex-1 overflow-y-auto px-6 py-4">
              {(isLoading || isFetching) && !clientes?.length ? (
                <div className="space-y-4">
                  {[...Array(3)].map((_, i) => (
                    <div key={i} className="bg-amber-50 rounded-2xl p-4 animate-pulse border border-amber-100">
                      <div className="h-4 bg-amber-200 rounded-lg w-3/4 mb-2"></div>
                      <div className="h-3 bg-amber-200 rounded-lg w-1/2"></div>
                    </div>
                  ))}
                </div>
              ) : error ? (
                (() => {
                  const err = error as { code?: string; message?: string };
                  const isConnectionRefused =
                    err?.code === 'ERR_NETWORK' ||
                    err?.message === 'Network Error' ||
                    (typeof err?.message === 'string' && err.message.includes('refused'));
                  return (
                    <EmptyState
                      title="Erro ao buscar clientes"
                      description={
                        isConnectionRefused
                          ? 'O backend não está rodando. Na pasta IComanda.API execute INICIAR_TUDO.bat ou, em um terminal: dotnet run. Depois clique em "Tentar novamente".'
                          : 'Não foi possível carregar a lista de clientes. Tente novamente.'
                      }
                      actionText="Tentar novamente"
                      onAction={() => refetch()}
                      icon={<User className="w-8 h-8 text-red-400" />}
                    />
                  );
                })()
              ) : clientes?.length === 0 ? (
                <EmptyState
                  title="Nenhum cliente encontrado"
                  description={searchQuery.trim() ? `Não encontramos clientes para "${searchQuery.trim()}".` : "Digite nome, CPF/CNPJ ou telefone para buscar."}
                  actionText="Limpar busca"
                  onAction={() => setSearchQuery('')}
                  icon={<Search className="w-8 h-8 text-text-muted" />}
                />
              ) : (
                <div className="space-y-3">
                  {clientes?.map((cliente, index) => (
                    <motion.div
                      key={cliente.id}
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: index * 0.05 }}
                      className="bg-amber-50 border border-amber-200 rounded-2xl p-4 hover:bg-amber-100 transition-colors cursor-pointer"
                      onClick={() => handleSelectCliente(cliente)}
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div className="flex-1 min-w-0">
                          {/* Nome e Documento */}
                          <div className="flex items-center space-x-3 mb-3">
                            <div className="w-8 h-8 bg-amber-200 rounded-xl flex items-center justify-center flex-shrink-0">
                              <User className="w-4 h-4 text-amber-700" />
                            </div>
                            <div className="flex-1 min-w-0">
                              <h3 className="font-semibold text-gray-800 truncate">
                                {cliente.nomeCompleto}
                              </h3>
                              {cliente.documento && (
                                <p className="text-sm text-gray-600 mt-0.5">
                                  {formatarDocumento(cliente.documento)}
                                </p>
                              )}
                            </div>
                          </div>

                          {/* Informações de Contato e Endereço */}
                          <div className="space-y-2 ml-11">
                            {cliente.contato && (
                              <div className="flex items-center space-x-2 text-sm text-gray-600">
                                <Phone className="w-3.5 h-3.5 flex-shrink-0 text-amber-600" />
                                <span>{cliente.contato}</span>
                              </div>
                            )}
                            
                            {/* Endereço Completo */}
                            {(cliente.endereco1 || cliente.bairro1 || cliente.cidade1 || cliente.uf1) && (
                              <div className="flex items-start space-x-2 text-sm text-gray-600">
                                <MapPin className="w-3.5 h-3.5 flex-shrink-0 text-amber-600 mt-0.5" />
                                <div className="flex-1 min-w-0">
                                  {/* Endereço com número e complemento */}
                                  {cliente.endereco1 && (
                                    <div className="font-medium text-gray-700">
                                      {cliente.endereco1}
                                      {cliente.numero1 && `, ${cliente.numero1}`}
                                      {cliente.complemento1 && ` - ${cliente.complemento1}`}
                                    </div>
                                  )}
                                  {/* Bairro, Cidade e UF */}
                                  {(cliente.bairro1 || cliente.cidade1 || cliente.uf1) && (
                                    <div className="text-gray-600 mt-0.5">
                                      {[
                                        cliente.bairro1,
                                        cliente.cidade1 ? (cliente.uf1 ? `${cliente.cidade1}/${cliente.uf1}` : cliente.cidade1) : cliente.uf1
                                      ].filter(Boolean).join(' - ')}
                                    </div>
                                  )}
                                </div>
                              </div>
                            )}
                          </div>
                        </div>

                        {/* Botão de Seleção */}
                        <div className="flex flex-col items-end space-y-2 flex-shrink-0">
                          {cliente.classificacao && (
                            <div className="px-2 py-1 bg-amber-200 text-amber-800 text-xs font-medium rounded-lg whitespace-nowrap">
                              {cliente.classificacao}
                            </div>
                          )}
                          <button className="w-8 h-8 bg-gradient-to-r from-amber-500 to-orange-500 rounded-xl flex items-center justify-center text-white hover:from-amber-600 hover:to-orange-600 transition-all shadow-md">
                            <Check className="w-4 h-4" />
                          </button>
                        </div>
                      </div>
                    </motion.div>
                  ))}
                </div>
              )}
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
};

export default ClienteSearch;
