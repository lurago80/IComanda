import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Package } from 'lucide-react';
import React from 'react';
import { produtosService } from '../services/api';
import { Grupo } from '../types/api';
import ProductCard from './product/ProductCard';
import EmptyState from './states/EmptyState';
import { SkeletonProductCard } from './states/SkeletonCard';

interface ProdutosListProps {
  grupo: Grupo;
  onVoltar: () => void;
}

const ProdutosList: React.FC<ProdutosListProps> = ({ 
  grupo, 
  onVoltar
}) => {
  const { data: produtos, isLoading, error } = useQuery({
    queryKey: ['produtos', grupo.id],
    queryFn: () => produtosService.getByGrupo(grupo.id),
  });

  if (isLoading) {
    return (
      <div className="max-w-md mx-auto px-4 py-6">
        {/* Header Skeleton */}
        <div className="flex items-center space-x-4 mb-6">
          <div className="w-10 h-10 bg-bgMuted rounded-xl animate-pulse"></div>
          <div>
            <div className="h-6 bg-bgMuted rounded-lg w-32 mb-2 animate-pulse"></div>
            <div className="h-4 bg-bgMuted rounded-lg w-24 animate-pulse"></div>
          </div>
        </div>
        
        <div className="space-y-3">
          {[...Array(6)].map((_, i) => (
            <SkeletonProductCard key={i} />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-md mx-auto px-4 py-6">
        <EmptyState
          title="Erro ao carregar produtos"
          description="Não foi possível carregar os produtos desta categoria. Tente novamente."
          actionText="Tentar novamente"
          onAction={() => window.location.reload()}
          icon={<Package className="w-8 h-8 text-red-400" />}
        />
      </div>
    );
  }

  return (
    <div className="max-w-md mx-auto px-6 py-8">
      {/* Header com design moderno */}
      <div className="flex items-center space-x-6 mb-10">
        <button
          onClick={onVoltar}
          className="w-12 h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
        >
          <ArrowLeft className="w-6 h-6" />
        </button>
        <div className="flex-1">
          <h2 className="text-3xl font-bold text-gradient mb-2">{grupo.descricao}</h2>
          <div className="flex items-center space-x-3">
            <div className="w-2 h-2 bg-primary rounded-full animate-pulse-soft"></div>
            <p className="text-text-secondary text-lg">
              {produtos?.length || 0} {produtos?.length === 1 ? 'produto' : 'produtos'}
            </p>
          </div>
        </div>
      </div>

      {/* Lista de Produtos com animações */}
      <div className="space-y-6">
        {produtos?.map((produto, index) => (
          <div 
            key={produto.id}
            className={`animate-fade-in-up animate-stagger-${(index % 3) + 1}`}
          >
            <ProductCard produto={produto} />
          </div>
        ))}
      </div>

      {produtos?.length === 0 && (
        <div className="animate-fade-in-up">
          <EmptyState
            title="Nenhum produto encontrado"
            description="Não há produtos disponíveis nesta categoria."
            actionText="Voltar às categorias"
            onAction={onVoltar}
          />
        </div>
      )}
    </div>
  );
};

export default ProdutosList;
