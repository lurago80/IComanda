import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Package, Search } from 'lucide-react';
import React from 'react';
import { gruposService } from '../services/api';
import { Grupo } from '../types/api';
import CategoryCard from './category/CategoryCard';
import EmptyState from './states/EmptyState';
import SkeletonCard from './states/SkeletonCard';

interface GruposListProps {
  onSelecionarGrupo: (grupo: Grupo) => void;
  onVoltar?: () => void;
  /** Buscar em todos os produtos (sem escolher categoria). Chamado com o termo digitado. */
  onBuscarTodos?: (query: string) => void;
  /** Valor inicial do campo de busca (preservado ao voltar dos resultados). */
  initialBusca?: string;
}

const GruposList: React.FC<GruposListProps> = ({ onSelecionarGrupo, onVoltar, onBuscarTodos, initialBusca }) => {
  const [busca, setBusca] = React.useState(initialBusca ?? '');
  const { data: grupos, isLoading, error } = useQuery({
    queryKey: ['grupos'],
    queryFn: async () => {
      try {
        const resultado = await gruposService.getTodosComQuantidade();
        return Array.isArray(resultado) ? resultado : [];
      } catch (error: any) {
        console.error('❌ [GruposList] Erro ao buscar grupos:', error?.message || error);
        throw error;
      }
    },
  });

  if (isLoading) {
    return (
      <div className="max-w-md mx-auto px-4 py-6">
        <div className="mb-6">
          <div className="h-8 bg-bgMuted rounded-lg w-48 mb-2 animate-pulse"></div>
          <div className="h-4 bg-bgMuted rounded-lg w-64 animate-pulse"></div>
        </div>
        <div className="space-y-3">
          {[...Array(6)].map((_, i) => (
            <SkeletonCard key={i} />
          ))}
        </div>
      </div>
    );
  }

  if (error) {
    console.error('❌ [GruposList] Erro ao carregar grupos:', error);
    return (
      <div className="max-w-md mx-auto px-4 py-6">
        <EmptyState
          title="Erro ao carregar categorias"
          description="Não foi possível carregar as categorias. Verifique sua conexão e tente novamente."
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
        {onVoltar && (
          <button
            onClick={onVoltar}
            className="w-12 h-12 bg-card border border-border rounded-2xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover hover:border-border-secondary transition-all duration-300 hover:scale-105 active:scale-95"
          >
            <ArrowLeft className="w-6 h-6" />
          </button>
        )}
        <div className="flex-1">
          <h2 className="text-3xl font-bold text-gradient mb-2">Categorias</h2>
          <p className="text-text-secondary text-lg">
            Escolha uma categoria para ver os produtos
          </p>
        </div>
      </div>

      {/* Buscar todos os produtos (antes dos grupos) */}
      {onBuscarTodos && (
        <div className="mb-6">
          <div className="relative">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-text-muted" />
            <input
              type="text"
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && busca.trim()) {
                  onBuscarTodos(busca.trim());
                }
              }}
              placeholder="Buscar todos os produtos..."
              className={`w-full pl-12 py-3.5 bg-card border border-border rounded-2xl text-text-primary placeholder:text-text-muted focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent ${busca.trim() ? 'pr-24' : 'pr-4'}`}
            />
            {busca.trim() && (
              <button
                type="button"
                onClick={() => onBuscarTodos(busca.trim())}
                className="absolute right-3 top-1/2 -translate-y-1/2 px-3 py-1.5 bg-primary text-primary-foreground text-sm font-medium rounded-xl hover:bg-primary/90"
              >
                Buscar
              </button>
            )}
          </div>
          <p className="text-sm text-text-muted mt-2">
            Digite o nome do produto e pressione Enter ou clique em Buscar
          </p>
        </div>
      )}

      {/* Lista de categorias com animações escalonadas */}
      {grupos && Array.isArray(grupos) && grupos.length > 0 && (
        <div className="space-y-6">
          {grupos.map((grupo, index) => (
            <div 
              key={grupo.id || `grupo-${index}`}
              className={`animate-fade-in-up animate-stagger-${(index % 3) + 1}`}
            >
              <CategoryCard
                grupo={grupo}
                onClick={() => onSelecionarGrupo(grupo)}
              />
            </div>
          ))}
        </div>
      )}

      {(!grupos || !Array.isArray(grupos) || grupos.length === 0) && !isLoading && (
        <div className="animate-fade-in-up">
          <EmptyState
            title="Nenhuma categoria encontrada"
            description="Não há categorias disponíveis no momento."
            actionText="Atualizar catálogo"
            onAction={() => window.location.reload()}
          />
        </div>
      )}
    </div>
  );
};

export default GruposList;
