import React from 'react'
import { ClipboardList, Bike, Briefcase } from 'lucide-react'

export type ModuloAtivo = 'comanda' | 'delivery' | 'forca-vendas'

interface NavModulosProps {
  moduloAtivo: ModuloAtivo
  onComanda?: () => void
  onDelivery?: () => void
  onForcaVendas?: () => void
}

const NavModulos: React.FC<NavModulosProps> = ({
  moduloAtivo,
  onComanda,
  onDelivery,
  onForcaVendas,
}) => {
  const modulos = [
    ...(onComanda
      ? [{ id: 'comanda' as ModuloAtivo, label: 'Comanda', icon: <ClipboardList className="w-5 h-5" />, onClick: onComanda, activeColor: 'bg-primary text-white shadow-md', idleColor: 'bg-primary/10 text-primary hover:bg-primary/20' }]
      : []),
    ...(onDelivery
      ? [{ id: 'delivery' as ModuloAtivo, label: 'Delivery', icon: <Bike className="w-5 h-5" />, onClick: onDelivery, activeColor: 'bg-orange-500 text-white shadow-md', idleColor: 'bg-orange-100 text-orange-600 hover:bg-orange-200' }]
      : []),
    ...(onForcaVendas
      ? [{ id: 'forca-vendas' as ModuloAtivo, label: 'Força de Vendas', icon: <Briefcase className="w-5 h-5" />, onClick: onForcaVendas, activeColor: 'bg-indigo-600 text-white shadow-md', idleColor: 'bg-indigo-100 text-indigo-600 hover:bg-indigo-200' }]
      : []),
  ]

  if (modulos.length === 0) return null

  return (
    <div className="flex gap-2 p-3 bg-card border-b border-border">
      {modulos.map((m) => {
        const isAtivo = m.id === moduloAtivo
        return (
          <button
            key={m.id}
            type="button"
            onClick={isAtivo ? undefined : m.onClick}
            className={`flex-1 flex items-center justify-center gap-2 rounded-xl px-3 py-3 font-semibold text-sm transition-all duration-200 ${isAtivo ? m.activeColor + ' cursor-default' : m.idleColor + ' cursor-pointer'}`}
          >
            {m.icon}
            <span className="hidden sm:inline">{m.label}</span>
            <span className="sm:hidden text-xs">{m.label.split(' ')[0]}</span>
          </button>
        )
      })}
    </div>
  )
}

export default NavModulos
