import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Package, Search, X } from 'lucide-react';
import React, { useState } from 'react';
import { useDebounce } from '../hooks/useDebounce';
import { produtosService } from '../services/api';
import { Grupo } from '../types/api';
import ProductCard from './product/ProductCard';
import EmptyState from './states/EmptyState';
import { SkeletonProductCard } from './states/SkeletonCard';

interface ProdutosListProps {
  grupo: Grupo;
  onVoltar: () => void;
  onRequestTaxaEntrega?: (produto: import('../types/api').Produto) => void;
}

const ProdutosList: React.FC<ProdutosListProps> = ({ 
  grupo, 
  onVoltar,
  onRequestTaxaEntrega
}) => {
  const [searchQuery, setSearchQuery] = useState('');
  const debouncedSearch = useDebounce(searchQuery, 300);

  // Buscar produtos da categoria quando não há busca
  const { data: produtos, isLoading: isLoadingCategoria, error: errorCategoria } = useQuery({
    queryKey: ['produtos', 'grupo', grupo.id],
    queryFn: async () => {
      if (!grupo || !grupo.id) {
        throw new Error('Grupo inválido');
      }

      try {
        const resultado = await produtosService.getByGrupo(grupo.id);
        return Array.isArray(resultado) ? resultado : [];
      } catch (error: any) {
        console.error('❌ [ProdutosList] Erro ao buscar produtos:', error?.message || error);
        throw error;
      }
    },
    enabled: !!grupo?.id && !debouncedSearch.trim(), // Só carrega quando grupo é válido e não há busca
    staleTime: 30000, // Cache por 30 segundos
  });

  // Buscar produtos via API quando há busca (busca em TODOS os produtos, sem filtro de grupo)
  const { data: produtosBusca, isLoading: isLoadingBusca, error: errorBusca } = useQuery({
    queryKey: ['produtos', 'busca', debouncedSearch],
    queryFn: () => {
      console.log('🔍 [ProdutosList] Buscando produtos com termo:', debouncedSearch);
      return produtosService.buscar({ q: debouncedSearch, ativo: true });
    },
    enabled: debouncedSearch.trim().length >= 2, // Só busca se tiver pelo menos 2 caracteres
  });

  // Usar produtos da busca se houver busca, senão usar produtos da categoria
  const produtosExibidos = debouncedSearch.trim() ? produtosBusca : produtos;
  const isLoading = debouncedSearch.trim() ? isLoadingBusca : isLoadingCategoria;
  const error = debouncedSearch.trim() ? errorBusca : errorCategoria;
  const isBuscando = debouncedSearch.trim().length >= 2;

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
      <div className="flex items-center space-x-6 mb-6">
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
              {isBuscando ? (
                <>
                  {produtosExibidos?.length || 0} {produtosExibidos?.length === 1 ? 'resultado' : 'resultados'}
                  {searchQuery && <span className="text-text-muted"> para "{searchQuery}"</span>}
                </>
              ) : (
                <>
                  {produtosExibidos?.length || 0} {produtosExibidos?.length === 1 ? 'produto' : 'produtos'}
                </>
              )}
            </p>
          </div>
        </div>
      </div>

      {/* Campo de Busca */}
      <div className="mb-6">
        <div className="relative">
          <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-text-muted" />
          <input
            type="text"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            placeholder="Buscar em todos os produtos (nome, código, código de barras)..."
            className="w-full pl-12 pr-12 py-4 bg-background-secondary border border-border rounded-2xl
                      focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                      text-text-primary placeholder-text-muted transition-all text-base"
          />
          {searchQuery && (
            <button
              onClick={() => setSearchQuery('')}
              className="absolute right-4 top-1/2 transform -translate-y-1/2 w-8 h-8 flex items-center justify-center
                        text-text-muted hover:text-text-primary hover:bg-background-tertiary rounded-xl transition-all"
            >
              <X className="w-4 h-4" />
            </button>
          )}
        </div>
      </div>

      {/* Lista de Produtos com animações */}
      {produtosExibidos && Array.isArray(produtosExibidos) && produtosExibidos.length > 0 && (
        <div className="space-y-6">
          {produtosExibidos.map((produto, index) => {
            if (!produto || !produto.id) {
              console.warn(`⚠️ [ProdutosList] Produto inválido no índice ${index}:`, produto);
              return null;
            }
            return (
              <div 
                key={produto.id ?? `prod-${index}`}
                className={`animate-fade-in-up animate-stagger-${(index % 3) + 1}`}
              >
                <ProductCard produto={produto} onRequestTaxaEntrega={onRequestTaxaEntrega} />
              </div>
            );
          })}
        </div>
      )}

      {(!produtosExibidos || !Array.isArray(produtosExibidos) || produtosExibidos.length === 0) && !isLoading && (
        <div className="animate-fade-in-up">
          <EmptyState
            title={isBuscando ? "Nenhum produto encontrado" : "Nenhum produto encontrado"}
            description={
              isBuscando
                ? `Não encontramos produtos para "${searchQuery}" em todo o sistema. Tente com outros termos.`
                : "Não há produtos disponíveis nesta categoria."
            }
            actionText={isBuscando ? "Limpar busca" : "Voltar às categorias"}
            onAction={isBuscando ? () => setSearchQuery('') : onVoltar}
            icon={<Search className="w-8 h-8 text-text-muted" />}
          />
        </div>
      )}

      {isBuscando && searchQuery.length < 2 && (
        <div className="animate-fade-in-up">
          <div className="text-center py-8">
            <p className="text-text-secondary">
              Digite pelo menos 2 caracteres para buscar
            </p>
          </div>
        </div>
      )}
    </div>
  );
};

export default ProdutosList;
