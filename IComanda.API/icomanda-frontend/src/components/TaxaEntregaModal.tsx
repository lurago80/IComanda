import { Truck, Loader2, X } from 'lucide-react';
import React, { useEffect, useState } from 'react';
import { taxasEntregaService } from '../services/api';
import { Produto, TaxaEntrega } from '../types/api';
import { Button } from './ui/button';

interface TaxaEntregaModalProps {
  isOpen: boolean;
  onClose: () => void;
  produto: Produto | null;
  onSelect: (produto: Produto, taxa: TaxaEntrega) => void;
}

const TaxaEntregaModal: React.FC<TaxaEntregaModalProps> = ({
  isOpen,
  onClose,
  produto,
  onSelect,
}) => {
  const [taxas, setTaxas] = useState<TaxaEntrega[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!isOpen) return;
    setLoading(true);
    taxasEntregaService
      .getAll()
      .then(setTaxas)
      .catch(() => setTaxas([]))
      .finally(() => setLoading(false));
  }, [isOpen]);

  if (!isOpen) return null;

  const formatarValor = (v: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v);

  return (
    <div className="fixed inset-0 z-[110] flex items-center justify-center p-4 bg-black/50">
      <div className="bg-card border border-border rounded-2xl shadow-xl w-full max-w-md overflow-hidden">
        <div className="flex items-center justify-between p-4 border-b border-border">
          <div className="flex items-center gap-2">
            <Truck className="w-5 h-5 text-primary" />
            <h2 className="text-lg font-bold text-text-primary">Selecionar taxa de entrega</h2>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg text-text-muted hover:text-text-primary hover:bg-background-secondary"
          >
            <X className="w-5 h-5" />
          </button>
        </div>
        {produto && (
          <p className="px-4 py-2 text-sm text-text-secondary border-b border-border">
            Produto: <span className="font-medium text-text-primary">{produto.descricao}</span>
          </p>
        )}
        <div className="p-4 max-h-80 overflow-y-auto">
          {loading ? (
            <div className="flex justify-center py-8">
              <Loader2 className="w-8 h-8 text-primary animate-spin" />
            </div>
          ) : taxas.length === 0 ? (
            <p className="text-center text-text-secondary py-6">
              Nenhuma taxa de entrega cadastrada. Cadastre em Menu → Taxas de Entrega.
            </p>
          ) : (
            <div className="space-y-2">
              {taxas.map((taxa) => (
                <button
                  key={taxa.id}
                  onClick={() => {
                    if (produto) {
                      onSelect(produto, taxa);
                      onClose();
                    }
                  }}
                  className="w-full flex items-center justify-between p-4 rounded-xl border border-border hover:border-primary/50 hover:bg-primary/5 transition-all text-left"
                >
                  <span className="font-medium text-text-primary">{taxa.descricao}</span>
                  <span className="font-bold text-primary">{formatarValor(taxa.valor)}</span>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default TaxaEntregaModal;
