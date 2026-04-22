import {
    Beaker,
    Cloud,
    Cog,
    Flame,
    Gift,
    Lightbulb,
    Package,
    Sparkles,
    Wrench
} from 'lucide-react'
import React from 'react'
import { Grupo } from '../../types/api'

interface CategoryCardProps {
  grupo: Grupo
  onClick: () => void
}

const CategoryCard: React.FC<CategoryCardProps> = ({ grupo, onClick }) => {
  const getIconForGrupo = (descricao: string) => {
    const desc = descricao.toLowerCase()
    if (desc.includes('aditivo')) return <Beaker className="w-6 h-6" />
    if (desc.includes('ar condicionado') || desc.includes('filtro')) return <Cloud className="w-6 h-6" />
    if (desc.includes('limpeza') || desc.includes('desingripante')) return <Sparkles className="w-6 h-6" />
    if (desc.includes('lampada') || desc.includes('lâmpada')) return <Lightbulb className="w-6 h-6" />
    if (desc.includes('oleo') || desc.includes('óleo')) return <Wrench className="w-6 h-6" />
    if (desc.includes('servico') || desc.includes('serviço')) return <Cog className="w-6 h-6" />
    if (desc.includes('graxa')) return <Flame className="w-6 h-6" />
    if (desc.includes('acessorio') || desc.includes('acessório')) return <Gift className="w-6 h-6" />
    return <Package className="w-6 h-6" />
  }

  const getColorForGrupo = (index: number) => {
    const colors = [
      'bg-blue-500/20 text-blue-400 border-blue-500/30',
      'bg-green-500/20 text-green-400 border-green-500/30',
      'bg-purple-500/20 text-purple-400 border-purple-500/30',
      'bg-orange-500/20 text-orange-400 border-orange-500/30',
      'bg-pink-500/20 text-pink-400 border-pink-500/30',
      'bg-indigo-500/20 text-indigo-400 border-indigo-500/30',
      'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
      'bg-red-500/20 text-red-400 border-red-500/30',
      'bg-teal-500/20 text-teal-400 border-teal-500/30',
      'bg-gray-500/20 text-gray-400 border-gray-500/30',
    ]
    return colors[index % colors.length]
  }

  return (
    <div 
      className="card-modern cursor-pointer group animate-fade-in-up"
      onClick={onClick}
    >
      <div className="flex items-center gap-3">
        {/* Ícone */}
        <div className={`w-12 h-12 flex-shrink-0 rounded-2xl flex items-center justify-center transition-all duration-500 ${getColorForGrupo(grupo.id)}`}>
          {getIconForGrupo(grupo.descricao)}
        </div>

        {/* Informações */}
        <div className="flex-1 overflow-hidden">
          <h3 className="font-bold text-text-primary text-sm leading-snug mb-1 line-clamp-2">
            {grupo.descricao || 'Categoria'}
          </h3>
          <div className="flex items-center gap-2">
            <span className="text-xs text-text-secondary font-medium">
              {grupo.quantidadeProdutos} {grupo.quantidadeProdutos === 1 ? 'produto' : 'produtos'}
            </span>
            {grupo.quantidadeProdutos > 0 && (
              <span className="text-xs text-primary font-semibold bg-primary/10 border border-primary/20 rounded-full px-2 py-0.5 leading-none">
                Disponível
              </span>
            )}
          </div>
        </div>

        {/* Seta */}
        <div className="w-8 h-8 flex-shrink-0 bg-card-secondary border border-border rounded-xl flex items-center justify-center text-text-secondary group-hover:text-primary group-hover:bg-primary/10 group-hover:border-primary/30 transition-all duration-300">
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2.5} d="M9 5l7 7-7 7" />
          </svg>
        </div>
      </div>
    </div>
  )
}

export default CategoryCard
