import { ShoppingCartIcon } from '@heroicons/react/24/outline';
import React from 'react';

interface HeaderProps {
  totalItens: number;
  totalValor: number;
  onAbrirCarrinho: () => void;
}

const Header: React.FC<HeaderProps> = ({ totalItens, totalValor, onAbrirCarrinho }) => {
  return (
    <header className="bg-white shadow-sm border-b border-gray-200 sticky top-0 z-40">
      <div className="max-w-md mx-auto px-4 py-4">
        <div className="flex items-center justify-between">
          {/* Logo e Título */}
          <div className="flex items-center space-x-3">
            <div className="h-14 rounded-xl flex items-center justify-center overflow-hidden px-4 py-1">
              <img src="/iComanda.jpg" alt="iComanda Logo" className="h-full w-auto object-contain" />
            </div>
            <div>
              <p className="text-sm text-gray-500">Sistema de Pedidos</p>
            </div>
          </div>

          {/* Carrinho */}
          <button
            onClick={onAbrirCarrinho}
            className="relative bg-primary-600 text-white px-4 py-2 rounded-xl flex items-center space-x-2 hover:bg-primary-700 transition-colors"
          >
            <ShoppingCartIcon className="w-5 h-5" />
            <span className="font-medium">
              {totalItens > 0 ? `${totalItens} itens` : 'Carrinho'}
            </span>
            {totalItens > 0 && (
              <div className="absolute -top-2 -right-2 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
                {totalItens}
              </div>
            )}
          </button>
        </div>

        {/* Total do Carrinho */}
        {totalItens > 0 && (
          <div className="mt-3 p-3 bg-primary-50 rounded-lg">
            <div className="flex justify-between items-center">
              <span className="text-sm text-primary-700">Total do carrinho:</span>
              <span className="text-lg font-bold text-primary-900">
                R$ {totalValor.toFixed(2).replace('.', ',')}
              </span>
            </div>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;
