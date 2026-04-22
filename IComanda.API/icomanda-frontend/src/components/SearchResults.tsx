import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Search } from 'lucide-react';
import React, { useState } from 'react';
import { useDebounce } from '../hooks/useDebounce';
import { produtosService } from '../services/api';
import ProductCard from './product/ProductCard';
import EmptyState from './states/EmptyState';
import { SkeletonProductCard } from './states/SkeletonCard';

interface SearchResultsProps {
  query: string;
  onBack: () => void;
  onRequestTaxaEntrega?: (produto: import('../types/api').Produto) => void;
  /** Callback para alterar o termo de busca sem sair da tela. */
  onQueryChange?: (query: string) => void;
}

const SearchResults: React.FC<SearchResultsProps> = ({ query, onBack, onRequestTaxaEntrega, onQueryChange }) => {
  const [inputBusca, setInputBusca] = useState('');
  const debouncedQuery = useDebounce(query, 300);
  
  const { data: produtos, isLoading, error } = useQuery({
    queryKey: ['produtos', 'busca', debouncedQuery],
    queryFn: async () => {
      try {
        return await produtosService.buscar({ q: debouncedQuery, ativo: true });
      } catch (error) {
        console.error('❌ [SearchResults] Erro ao buscar produtos:', error);
        throw error;
      }
    },
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
    <div className="max-w-md mx-auto px-4 sm:px-6 py-4 sm:py-8 pb-24">
      {/* Header com design moderno */}
      <div className="flex items-start space-x-3 sm:space-x-6 mb-4 sm:mb-6">
        <button
          onClick={onBack}
          className="w-10 h-10 sm:w-12 sm:h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95 flex-shrink-0 mt-1"
        >
          <ArrowLeft className="w-5 h-5 sm:w-6 sm:h-6" />
        </button>
        <div className="flex-1 min-w-0 pr-2">
          <h2 className="text-xl sm:text-3xl font-bold text-gradient mb-2 break-words leading-tight">Resultados da busca</h2>
          <div className="flex items-start space-x-2 sm:space-x-3 flex-wrap gap-y-1">
            <div className="w-2 h-2 bg-primary rounded-full animate-pulse-soft flex-shrink-0 mt-1.5"></div>
            <p className="text-text-secondary text-sm sm:text-lg break-words leading-relaxed">
              "{query}" - {produtos?.length || 0} {produtos?.length === 1 ? 'resultado' : 'resultados'}
            </p>
          </div>
        </div>
      </div>

      {/* Campo de nova busca */}
      {onQueryChange && (
        <div className="mb-5">
          <div className="relative">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-text-muted" />
            <input
              type="text"
              value={inputBusca}
              onChange={(e) => setInputBusca(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && inputBusca.trim()) {
                  onQueryChange(inputBusca.trim());
                  setInputBusca('');
                }
              }}
              placeholder="Buscar outro produto..."
              className={`w-full pl-12 py-3 bg-card border border-border rounded-2xl text-text-primary placeholder:text-text-muted focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${
                inputBusca.trim() ? 'pr-24' : 'pr-4'
              }`}
            />
            {inputBusca.trim() && (
              <button
                type="button"
                onClick={() => { onQueryChange(inputBusca.trim()); setInputBusca(''); }}
                className="absolute right-3 top-1/2 -translate-y-1/2 px-3 py-1.5 bg-primary text-primary-foreground text-sm font-medium rounded-xl hover:bg-primary/90"
              >
                Buscar
              </button>
            )}
          </div>
        </div>
      )}

      {/* Lista de Produtos com animações */}
      <div className="space-y-4 sm:space-y-6">
        {produtos?.map((produto, index) => (
          <div 
            key={produto.id}
            className={`animate-fade-in-up animate-stagger-${(index % 3) + 1}`}
          >
            <ProductCard produto={produto} onRequestTaxaEntrega={onRequestTaxaEntrega} />
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
