import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Search } from 'lucide-react';
import React from 'react';
import { useDebounce } from '../hooks/useDebounce';
import { produtosService } from '../services/api';
import ProductCard from './product/ProductCard';
import EmptyState from './states/EmptyState';
import { SkeletonProductCard } from './states/SkeletonCard';

interface SearchResultsProps {
  query: string;
  onBack: () => void;
}

const SearchResults: React.FC<SearchResultsProps> = ({ query, onBack }) => {
  const debouncedQuery = useDebounce(query, 300);
  
  const { data: produtos, isLoading, error } = useQuery({
    queryKey: ['produtos', 'busca', debouncedQuery],
    queryFn: () => produtosService.buscar({ q: debouncedQuery }),
    enabled: debouncedQuery.length >= 2,
  });

  if (isLoading) {
    return (
      <div className="max-w-md mx-auto px-4 py-6">
        {/* Header Skeleton */}
        <div className="flex items-center space-x-4 mb-6">
          <div className="w-10 h-10 bg-bgMuted rounded-xl animate-pulse"></div>
          <div>
            <div className="h-6 bg-bgMuted rounded-lg w-48 mb-2 animate-pulse"></div>
            <div className="h-4 bg-bgMuted rounded-lg w-32 animate-pulse"></div>
          </div>
        </div>
        
        <div className="space-y-3">
          {[...Array(4)].map((_, i) => (
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
          title="Erro na busca"
          description="Não foi possível realizar a busca. Verifique sua conexão e tente novamente."
          actionText="Tentar novamente"
          onAction={() => window.location.reload()}
          icon={<Search className="w-8 h-8 text-red-400" />}
        />
      </div>
    );
  }

  return (
    <div className="max-w-md mx-auto px-6 py-8">
      {/* Header com design moderno */}
      <div className="flex items-center space-x-6 mb-10">
        <button
          onClick={onBack}
          className="w-12 h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
        >
          <ArrowLeft className="w-6 h-6" />
        </button>
        <div className="flex-1">
          <h2 className="text-3xl font-bold text-gradient mb-2">Resultados da busca</h2>
          <div className="flex items-center space-x-3">
            <div className="w-2 h-2 bg-primary rounded-full animate-pulse-soft"></div>
            <p className="text-text-secondary text-lg">
              "{query}" - {produtos?.length || 0} {produtos?.length === 1 ? 'resultado' : 'resultados'}
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
            description={`Não encontramos produtos para "${query}". Tente com outros termos.`}
            actionText="Limpar busca"
            onAction={onBack}
            icon={<Search className="w-8 h-8 text-text-muted" />}
          />
        </div>
      )}
    </div>
  );
};

export default SearchResults;
