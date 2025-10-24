import { useQuery } from '@tanstack/react-query';
import { Package } from 'lucide-react';
import React from 'react';
import { gruposService } from '../services/api';
import { Grupo } from '../types/api';
import CategoryCard from './category/CategoryCard';
import EmptyState from './states/EmptyState';
import SkeletonCard from './states/SkeletonCard';

interface GruposListProps {
  onSelecionarGrupo: (grupo: Grupo) => void;
}

const GruposList: React.FC<GruposListProps> = ({ onSelecionarGrupo }) => {
  const { data: grupos, isLoading, error } = useQuery({
    queryKey: ['grupos'],
    queryFn: gruposService.getTodosComQuantidade,
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
      <div className="mb-10 text-center">
        <h2 className="text-4xl font-bold text-gradient mb-4 animate-fade-in-up">Categorias</h2>
        <p className="text-text-secondary text-lg animate-fade-in-up animate-stagger-1">
          Escolha uma categoria para ver os produtos
        </p>
      </div>

      {/* Lista de categorias com animações escalonadas */}
      <div className="space-y-6">
        {grupos?.map((grupo, index) => (
          <div 
            key={grupo.id}
            className={`animate-fade-in-up animate-stagger-${(index % 3) + 1}`}
          >
            <CategoryCard
              grupo={grupo}
              onClick={() => onSelecionarGrupo(grupo)}
            />
          </div>
        ))}
      </div>

      {grupos?.length === 0 && (
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
