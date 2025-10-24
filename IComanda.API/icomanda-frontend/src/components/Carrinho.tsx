import { MinusIcon, PlusIcon, TrashIcon, XMarkIcon } from '@heroicons/react/24/outline';
import React from 'react';
import { Produto } from '../types/api';

interface ItemCarrinho {
  produto: Produto;
  quantidade: number;
}

interface CarrinhoProps {
  itens: ItemCarrinho[];
  total: number;
  onFechar: () => void;
  onAtualizarQuantidade: (produtoId: number, quantidade: number) => void;
  onRemover: (produtoId: number) => void;
}

const Carrinho: React.FC<CarrinhoProps> = ({
  itens,
  total,
  onFechar,
  onAtualizarQuantidade,
  onRemover,
}) => {
  const formatarPreco = (preco: number) => {
    return preco.toFixed(2).replace('.', ',');
  };

  const subtotal = itens.reduce((acc, item) => 
    acc + (item.produto.precoVenda * item.quantidade), 0
  );

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-end">
      <div className="bg-white w-full max-h-[80vh] rounded-t-3xl shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200">
          <h2 className="text-xl font-bold text-gray-900">Carrinho</h2>
          <button
            onClick={onFechar}
            className="w-8 h-8 bg-gray-100 rounded-full flex items-center justify-center hover:bg-gray-200 transition-colors"
          >
            <XMarkIcon className="w-5 h-5 text-gray-600" />
          </button>
        </div>

        {/* Lista de Itens */}
        <div className="flex-1 overflow-y-auto px-6 py-4">
          {itens.length === 0 ? (
            <div className="text-center py-12">
              <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <span className="text-2xl">🛒</span>
              </div>
              <h3 className="text-lg font-medium text-gray-900 mb-2">Carrinho vazio</h3>
              <p className="text-gray-500">Adicione produtos para começar seu pedido</p>
            </div>
          ) : (
            <div className="space-y-4">
              {itens.map((item) => (
                <div
                  key={item.produto.id}
                  className="bg-gray-50 rounded-xl p-4"
                >
                  <div className="flex items-center space-x-4">
                    {/* Imagem do Produto */}
                    <div className="w-12 h-12 bg-gradient-to-br from-primary-100 to-primary-200 rounded-lg flex items-center justify-center">
                      <span className="text-lg">📦</span>
                    </div>

                    {/* Informações do Produto */}
                    <div className="flex-1 min-w-0">
                      <h3 className="font-medium text-gray-900 truncate">
                        {item.produto.descricao}
                      </h3>
                      <p className="text-sm text-gray-500">
                        R$ {formatarPreco(item.produto.precoVenda)} cada
                      </p>
                    </div>

                    {/* Controles de Quantidade */}
                    <div className="flex items-center space-x-2">
                      <button
                        onClick={() => onAtualizarQuantidade(item.produto.id, item.quantidade - 1)}
                        className="w-8 h-8 bg-white border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-50 transition-colors"
                      >
                        <MinusIcon className="w-4 h-4 text-gray-600" />
                      </button>
                      
                      <span className="w-8 text-center font-medium text-gray-900">
                        {item.quantidade}
                      </span>
                      
                      <button
                        onClick={() => onAtualizarQuantidade(item.produto.id, item.quantidade + 1)}
                        className="w-8 h-8 bg-primary-600 text-white rounded-lg flex items-center justify-center hover:bg-primary-700 transition-colors"
                      >
                        <PlusIcon className="w-4 h-4" />
                      </button>
                    </div>

                    {/* Preço Total do Item */}
                    <div className="text-right">
                      <p className="font-semibold text-gray-900">
                        R$ {formatarPreco(item.produto.precoVenda * item.quantidade)}
                      </p>
                    </div>

                    {/* Botão Remover */}
                    <button
                      onClick={() => onRemover(item.produto.id)}
                      className="w-8 h-8 bg-red-100 text-red-600 rounded-lg flex items-center justify-center hover:bg-red-200 transition-colors"
                    >
                      <TrashIcon className="w-4 h-4" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Footer com Total e Botão Finalizar */}
        {itens.length > 0 && (
          <div className="border-t border-gray-200 p-6">
            <div className="flex justify-between items-center mb-4">
              <span className="text-lg font-medium text-gray-900">Total:</span>
              <span className="text-2xl font-bold text-primary-600">
                R$ {formatarPreco(subtotal)}
              </span>
            </div>
            
            <button className="w-full bg-primary-600 text-white py-4 rounded-xl font-semibold text-lg hover:bg-primary-700 transition-colors shadow-lg">
              Finalizar Pedido
            </button>
            
            <p className="text-center text-sm text-gray-500 mt-3">
              {itens.length} {itens.length === 1 ? 'item' : 'itens'} no carrinho
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default Carrinho;
