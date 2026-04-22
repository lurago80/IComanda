import { motion } from 'framer-motion';
import { 
  Loader2, 
  X, 
  Search, 
  Package,
  PackagePlus,
  Edit,
  Trash2,
  DollarSign,
  ShoppingCart
} from 'lucide-react';
import React, { useEffect, useState, useCallback } from 'react';
import { useToast } from '../hooks/useToast';
import { produtosService } from '../services/api';
import { Button } from '../components/ui/button';
import { ProdutoCompleto } from '../types/api';
import { useDebounce } from '../hooks/useDebounce';
import { normalizarTexto } from '../lib/utils';
import { logger } from '../utils/logger';

interface ProdutosPageProps {
  onClose: () => void;
  onNovoProduto?: () => void;
  onEditarProduto?: (produto: ProdutoCompleto) => void;
}

const ProdutosPage: React.FC<ProdutosPageProps> = ({ onClose, onNovoProduto, onEditarProduto }) => {
  const [produtos, setProdutos] = useState<ProdutoCompleto[]>([]);
  const [loading, setLoading] = useState(false);
  const [busca, setBusca] = useState<string>('');
  const [apenasAtivos, setApenasAtivos] = useState<boolean>(true);
  const debouncedBusca = useDebounce(busca, 500);

  const { showError, showSuccess } = useToast();

  const carregarProdutos = useCallback(async () => {
    try {
      setLoading(true);
      logger.debug('[ProdutosPage] Carregando produtos', { busca: debouncedBusca, apenasAtivos });
      const dados = await produtosService.buscarCompletos(
        debouncedBusca || undefined,
        apenasAtivos ? true : undefined,
        1,
        100
      );
      logger.info('[ProdutosPage] Produtos carregados', { total: dados.length });
      setProdutos(dados);
    } catch (error: any) {
      logger.error('[ProdutosPage] Erro ao carregar produtos', error, { response: error.response });
      showError('Erro', error.response?.data?.message || 'Não foi possível carregar os produtos');
    } finally {
      setLoading(false);
    }
  }, [debouncedBusca, apenasAtivos, showError]);

  useEffect(() => {
    carregarProdutos();
  }, [carregarProdutos]);

  const handleExcluir = async (id: number) => {
    if (!window.confirm('Tem certeza que deseja excluir este produto?')) {
      return;
    }

    try {
      await produtosService.excluir(id);
      showSuccess('Sucesso', 'Produto excluído com sucesso');
      carregarProdutos();
    } catch (error: any) {
      const mensagem = error.response?.data?.error ?? 'Não foi possível excluir o produto';
      showError('Erro', mensagem);
      logger.error('[ProdutosPage] Erro ao excluir produto', error, { id });
    }
  };

  const formatarMoeda = (valor?: number) => {
    if (!valor) return 'R$ 0,00';
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  };

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <Package className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Produtos</h1>
              <p className="text-sm text-text-muted">Consulta e gerenciamento de produtos</p>
            </div>
          </div>
          <div className="flex gap-2">
            {onNovoProduto && (
              <Button
                onClick={onNovoProduto}
                className="flex items-center space-x-2"
              >
                <PackagePlus className="w-4 h-4" />
                <span className="hidden sm:inline">Novo Produto</span>
              </Button>
            )}
            <Button
              onClick={onClose}
              variant="outline"
              className="flex items-center space-x-2"
            >
              <X className="w-4 h-4" />
              <span className="hidden sm:inline">Fechar</span>
            </Button>
          </div>
        </div>

        {/* Filtros */}
        <div className="bg-card rounded-2xl p-6 mb-6 shadow-lg border border-border">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-text-secondary mb-2">
                Buscar por código, código de barras, código interno ou descrição
              </label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-text-muted" />
                <input
                  type="text"
                  value={busca}
                  onChange={(e) => setBusca(e.target.value)}
                  className="w-full pl-10 pr-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Digite código, código de barras, código interno ou descrição..."
                />
              </div>
            </div>
            <div className="flex items-end">
              <label className="flex items-center space-x-2 cursor-pointer">
                <input
                  type="checkbox"
                  checked={apenasAtivos}
                  onChange={(e) => setApenasAtivos(e.target.checked)}
                  className="w-5 h-5 rounded border-border text-primary focus:ring-primary/50"
                />
                <span className="text-sm text-text-secondary">Apenas produtos ativos</span>
              </label>
            </div>
          </div>
        </div>

        {/* Lista de Produtos */}
        <div className="bg-card rounded-2xl p-6 shadow-lg border border-border">
          {loading ? (
            <div className="text-center py-12">
              <Loader2 className="w-8 h-8 text-primary mx-auto mb-4 animate-spin" />
              <p className="text-text-muted">Carregando produtos...</p>
            </div>
          ) : produtos.length === 0 ? (
            <div className="text-center py-12">
              <Package className="w-16 h-16 text-text-muted mx-auto mb-4 opacity-50" />
              <p className="text-text-muted text-lg">Nenhum produto encontrado</p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {produtos.map((produto) => (
                <motion.div
                  key={produto.id}
                  initial={{ opacity: 0, scale: 0.95 }}
                  animate={{ opacity: 1, scale: 1 }}
                  className="bg-background-secondary border border-border rounded-xl p-4 hover:shadow-lg transition-shadow"
                >
                  <div className="flex items-start justify-between mb-3">
                    <div className="flex-1 min-w-0">
                      <h3 className="font-bold text-lg text-text-primary mb-1 break-words whitespace-normal">
                        {normalizarTexto(produto.descricao)}
                      </h3>
                      {produto.caracteristica && (
                        <p className="text-sm text-text-secondary mb-2 break-words whitespace-normal">
                          {normalizarTexto(produto.caracteristica)}
                        </p>
                      )}
                    </div>
                    {produto.ativo === 1 ? (
                      <div className="w-3 h-3 bg-green-500 rounded-full flex-shrink-0 ml-2"></div>
                    ) : (
                      <div className="w-3 h-3 bg-red-500 rounded-full flex-shrink-0 ml-2"></div>
                    )}
                  </div>

                  <div className="space-y-2 mb-4">
                    {produto.codigoBarra && (
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Código de Barras:</span>
                        <span className="text-text-primary font-medium">{produto.codigoBarra}</span>
                      </div>
                    )}
                    {produto.codigoInterno && (
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Código Interno:</span>
                        <span className="text-text-primary font-medium">{produto.codigoInterno}</span>
                      </div>
                    )}
                    <div className="flex justify-between text-sm">
                      <span className="text-text-secondary">Código:</span>
                      <span className="text-text-primary font-medium">{produto.id}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-text-secondary">Quantidade:</span>
                      <span className="text-text-primary font-medium">{(produto.quantidade ?? 0).toFixed(2)}</span>
                    </div>
                    {produto.precoVenda && (
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Preço de Venda:</span>
                        <span className="text-primary font-bold">{formatarMoeda(produto.precoVenda)}</span>
                      </div>
                    )}
                    {produto.unMedida && (
                      <div className="flex justify-between text-sm">
                        <span className="text-text-secondary">Unidade:</span>
                        <span className="text-text-primary font-medium">{produto.unMedida}</span>
                      </div>
                    )}
                  </div>

                  <div className="flex gap-2 pt-3 border-t border-border">
                    {onEditarProduto && (
                      <Button
                        onClick={() => onEditarProduto(produto)}
                        variant="outline"
                        size="sm"
                        className="flex-1 flex items-center justify-center space-x-1"
                      >
                        <Edit className="w-4 h-4" />
                        <span>Editar</span>
                      </Button>
                    )}
                    <Button
                      onClick={() => handleExcluir(produto.id)}
                      variant="outline"
                      size="sm"
                      className="flex items-center justify-center space-x-1 text-red-500 hover:text-red-600 hover:bg-red-50"
                    >
                      <Trash2 className="w-4 h-4" />
                      <span>Excluir</span>
                    </Button>
                  </div>
                </motion.div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProdutosPage;
