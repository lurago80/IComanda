import { useQuery } from '@tanstack/react-query';
import { AnimatePresence, motion } from 'framer-motion';
import { Calendar, Clock, Package, Receipt, X } from 'lucide-react';
import React from 'react';
import { vendasService } from '../services/api';
import EmptyState from './states/EmptyState';
import { Button } from './ui/button';

interface HistoryModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const HistoryModal: React.FC<HistoryModalProps> = ({ isOpen, onClose }) => {
  const { data: vendas, isLoading, error } = useQuery({
    queryKey: ['vendas', 'hoje'],
    queryFn: vendasService.getHoje,
    enabled: isOpen,
  });

  const formatarData = (data: string) => {
    return new Date(data).toLocaleDateString('pt-BR');
  };

  const formatarHora = (hora: string) => {
    return hora.substring(0, 5); // HH:MM
  };

  const formatarPreco = (preco: number | undefined | null) => {
    if (preco === undefined || preco === null || isNaN(preco)) {
      return '0,00';
    }
    return preco.toFixed(2).replace('.', ',');
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
            className="fixed inset-0 bg-black/50 z-50"
            onClick={onClose}
          />

          {/* Modal */}
          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: "spring", stiffness: 300, damping: 30 }}
            className="fixed bottom-0 left-0 right-0 bg-card border-t border-border rounded-t-3xl z-50 max-h-[85vh] flex flex-col shadow-2xl"
          >
            {/* Header */}
            <div className="flex items-center justify-between p-6 border-b border-border">
              <div className="flex items-center space-x-3">
                <div className="w-10 h-10 bg-primary/10 rounded-2xl flex items-center justify-center">
                  <Receipt className="w-5 h-5 text-primary" />
                </div>
                <div>
                  <h2 className="text-xl font-bold text-text-primary">Histórico de Pedidos</h2>
                  <p className="text-sm text-text-secondary">Vendas de hoje</p>
                </div>
              </div>
              <Button
                variant="ghost"
                size="icon"
                onClick={onClose}
                className="text-text-secondary hover:text-text-primary"
              >
                <X className="w-5 h-5" />
              </Button>
            </div>

            {/* Conteúdo */}
            <div className="flex-1 overflow-y-auto px-6 py-4">
              {isLoading ? (
                <div className="space-y-4">
                  {[...Array(3)].map((_, i) => (
                    <div key={i} className="bg-background-secondary rounded-2xl p-4 animate-pulse border border-border">
                      <div className="h-4 bg-border-secondary rounded-lg w-3/4 mb-2"></div>
                      <div className="h-3 bg-border-secondary rounded-lg w-1/2"></div>
                    </div>
                  ))}
                </div>
              ) : error ? (
                <EmptyState
                  title="Erro ao carregar histórico"
                  description="Não foi possível carregar o histórico de pedidos. Tente novamente."
                  actionText="Tentar novamente"
                  onAction={() => window.location.reload()}
                  icon={<Receipt className="w-8 h-8 text-red-400" />}
                />
              ) : vendas?.length === 0 ? (
                <EmptyState
                  title="Nenhum pedido hoje"
                  description="Ainda não há pedidos registrados hoje."
                  actionText="Fazer primeiro pedido"
                  onAction={onClose}
                  icon={<Package className="w-8 h-8 text-text-muted" />}
                />
              ) : (
                <div className="space-y-4">
                  {vendas?.map((venda, index) => (
                    <motion.div
                      key={venda.nota}
                      initial={{ opacity: 0, y: 20 }}
                      animate={{ opacity: 1, y: 0 }}
                      transition={{ delay: index * 0.1 }}
                      className="bg-card border border-border rounded-2xl p-4 hover:bg-card-hover transition-colors"
                    >
                        <div className="flex items-start justify-between mb-3">
                        <div className="flex items-center space-x-3">
                          <div className="w-8 h-8 bg-primary/10 rounded-xl flex items-center justify-center">
                            <Receipt className="w-4 h-4 text-primary" />
                          </div>
                          <div>
                            <h3 className="font-semibold text-text-primary">Pedido #{venda.nota}</h3>
                            <div className="flex items-center space-x-4 text-sm text-text-secondary">
                              <div className="flex items-center space-x-1">
                                <Calendar className="w-3 h-3" />
                                <span>{formatarData(venda.emissao)}</span>
                              </div>
                              <div className="flex items-center space-x-1">
                                <Clock className="w-3 h-3" />
                                <span>{formatarHora(venda.hora)}</span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div className="text-right">
                          <p className="text-lg font-bold text-primary">
                            R$ {formatarPreco(venda.total)}
                          </p>
                          {venda.mesa && (
                            <p className="text-xs text-text-secondary">Mesa {venda.mesa}</p>
                          )}
                        </div>
                      </div>
                      
                      {venda.desconto > 0 && (
                        <div className="flex justify-between text-sm text-success mb-2">
                          <span>Desconto:</span>
                          <span>- R$ {formatarPreco(venda.desconto)}</span>
                        </div>
                      )}
                      
                      {venda.acrescimo > 0 && (
                        <div className="flex justify-between text-sm text-error mb-2">
                          <span>Acréscimo:</span>
                          <span>+ R$ {formatarPreco(venda.acrescimo)}</span>
                        </div>
                      )}
                      
                      <div className="flex justify-between text-sm text-text-secondary">
                        <span>Subtotal:</span>
                        <span>R$ {formatarPreco(
                          // Subtotal = Total dos produtos (antes de desconto/acréscimo)
                          // Se subtotal não vier do backend, calcular: Total - Acrescimo + Desconto
                          venda.subtotal ?? 
                          (venda.total - (venda.acrescimo || 0) + (venda.desconto || 0))
                        )}</span>
                      </div>
                    </motion.div>
                  ))}
                </div>
              )}
            </div>

            {/* Footer com resumo */}
            {vendas && vendas.length > 0 && (
              <div className="border-t border-border p-6 bg-background-secondary">
                <div className="flex justify-between items-center">
                  <div>
                    <p className="text-sm text-text-secondary">Total de pedidos hoje:</p>
                    <p className="text-lg font-semibold text-text-primary">{vendas.length}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-text-secondary">Faturamento:</p>
                    <p className="text-xl font-bold text-primary">
                      R$ {formatarPreco(vendas.reduce((total, venda) => total + (venda.total || 0), 0))}
                    </p>
                  </div>
                </div>
              </div>
            )}
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
};

export default HistoryModal;
