import { motion } from 'framer-motion';
import { 
  Loader2, 
  X, 
  Search, 
  Folder,
  FolderPlus,
  Edit,
  Trash2,
  ArrowLeft,
  Package,
  ChevronDown
} from 'lucide-react';
import React, { useEffect, useState, useCallback } from 'react';
import { useToast } from '../hooks/useToast';
import { gruposService, produtosService } from '../services/api';
import { Button } from '../components/ui/button';
import { Grupo, Produto, ProdutoCompleto } from '../types/api';
import { useDebounce } from '../hooks/useDebounce';
import { normalizarTexto } from '../lib/utils';

interface GruposPageProps {
  onClose: () => void;
}

const GruposPage: React.FC<GruposPageProps> = ({ onClose }) => {
  const [grupos, setGrupos] = useState<Grupo[]>([]);
  const [loading, setLoading] = useState(false);
  const [busca, setBusca] = useState<string>('');
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [grupoEditando, setGrupoEditando] = useState<Grupo | null>(null);
  const [descricao, setDescricao] = useState<string>('');
  const [imprimirDuasVias, setImprimirDuasVias] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [grupoSelecionado, setGrupoSelecionado] = useState<Grupo | null>(null);
  const [produtosGrupo, setProdutosGrupo] = useState<Produto[]>([]);
  const [carregandoProdutos, setCarregandoProdutos] = useState(false);
  const [atualizandoGrupo, setAtualizandoGrupo] = useState<number | null>(null);
  const debouncedBusca = useDebounce(busca, 500);

  const { showError, showSuccess } = useToast();

  const carregarGrupos = useCallback(async () => {
    try {
      setLoading(true);
      const dados = await gruposService.getTodosComQuantidade();
      setGrupos(dados);
    } catch (error: any) {
      console.error('❌ [GruposPage] Erro ao carregar grupos:', error);
      showError('Erro', error.response?.data?.error || 'Não foi possível carregar os grupos');
    } finally {
      setLoading(false);
    }
  }, [showError]);

  useEffect(() => {
    carregarGrupos();
  }, [carregarGrupos]);

  const gruposFiltrados = grupos.filter(grupo =>
    grupo.descricao.toLowerCase().includes(debouncedBusca.toLowerCase())
  );

  const abrirFormularioNovo = () => {
    setGrupoEditando(null);
    setDescricao('');
    setImprimirDuasVias(false);
    setMostrarFormulario(true);
  };

  const abrirFormularioEditar = (grupo: Grupo) => {
    setGrupoEditando(grupo);
    setDescricao(grupo.descricao);
    setImprimirDuasVias(grupo.imprimirDuasVias ?? false);
    setMostrarFormulario(true);
  };

  const fecharFormulario = () => {
    setMostrarFormulario(false);
    setGrupoEditando(null);
    setDescricao('');
    setImprimirDuasVias(false);
  };

  const salvarGrupo = async () => {
    if (!descricao.trim()) {
      showError('Validação', 'A descrição do grupo é obrigatória');
      return;
    }

    try {
      setSalvando(true);
      
      if (grupoEditando) {
        await gruposService.atualizar(grupoEditando.id, descricao, imprimirDuasVias);
        showSuccess('Sucesso', 'Grupo atualizado com sucesso');
      } else {
        await gruposService.criar(descricao, imprimirDuasVias);
        showSuccess('Sucesso', 'Grupo criado com sucesso');
      }

      fecharFormulario();
      carregarGrupos();
    } catch (error: any) {
      console.error('❌ [GruposPage] Erro ao salvar grupo:', error);
      showError('Erro', error.response?.data?.error || 'Não foi possível salvar o grupo');
    } finally {
      setSalvando(false);
    }
  };

  const excluirGrupo = async (id: number) => {
    const grupo = grupos.find(g => g.id === id);
    if (!grupo) return;

    if (!window.confirm(`Tem certeza que deseja excluir o grupo "${grupo.descricao}"?\n\n${grupo.quantidadeProdutos > 0 ? `⚠️ ATENÇÃO: Este grupo possui ${grupo.quantidadeProdutos} produto(s) associado(s).\nVocê precisará remover os produtos do grupo antes de excluí-lo.` : ''}`)) {
      return;
    }

    try {
      await gruposService.excluir(id);
      showSuccess('Sucesso', 'Grupo excluído com sucesso');
      carregarGrupos();
      if (grupoSelecionado?.id === id) {
        setGrupoSelecionado(null);
        setProdutosGrupo([]);
      }
    } catch (error: any) {
      console.error('❌ [GruposPage] Erro ao excluir grupo:', error);
      const mensagem = error.response?.data?.error || 'Não foi possível excluir o grupo';
      showError('Erro', mensagem);
    }
  };

  const selecionarGrupo = async (grupo: Grupo) => {
    // Se já está selecionado, apenas rolar para a seção
    if (grupoSelecionado?.id === grupo.id) {
      const produtosSection = document.getElementById('produtos-grupo-section');
      if (produtosSection) {
        produtosSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
      }
      return;
    }
    
    setGrupoSelecionado(grupo);
    setCarregandoProdutos(true);
    setProdutosGrupo([]); // Limpar produtos anteriores
    
    try {
      let produtos: Produto[] = [];
      
      try {
        // Primeiro tentar buscar apenas ativos (padrão mais comum)
        produtos = await produtosService.getByGrupo(grupo.id, true);
      } catch (error: any) {
        if (process.env.NODE_ENV !== 'production') {
          console.error('❌ [GruposPage] Erro ao buscar produtos:', error?.message || error);
          console.error('❌ [GruposPage] Status:', error?.response?.status);
        }
        showError('Erro', error?.response?.data?.error || 'Não foi possível carregar os produtos do grupo');
        produtos = [];
      }
      
      if (produtos.length === 0 && grupo.quantidadeProdutos > 0) {
        showError('Aviso', `O grupo tem ${grupo.quantidadeProdutos} produto(s) cadastrado(s), mas nenhum foi encontrado. Verifique os logs do servidor.`);
      }
      
      setProdutosGrupo(produtos);
      
      // Aguardar um pouco para garantir que o DOM foi atualizado
      setTimeout(() => {
        const produtosSection = document.getElementById('produtos-grupo-section');
        if (produtosSection) {
          produtosSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
      }, 100);
    } catch (error: any) {
      console.error('❌ [GruposPage] Erro ao carregar produtos do grupo:', error);
      showError('Erro', error.response?.data?.error || 'Não foi possível carregar os produtos do grupo');
      setProdutosGrupo([]);
    } finally {
      setCarregandoProdutos(false);
    }
  };

  const atualizarGrupoProduto = async (produtoId: number, novoGrupoId: number) => {
    try {
      setAtualizandoGrupo(produtoId);
      
      // Atualizar apenas o grupo (o backend aceita atualização parcial)
      await produtosService.atualizar(produtoId, {
        grupo: novoGrupoId
      } as Partial<ProdutoCompleto>);

      // Atualizar a lista local
      setProdutosGrupo(prev => prev.map(p => 
        p.id === produtoId ? { ...p, grupo: novoGrupoId } : p
      ));

      // Atualizar contagem de produtos nos grupos
      await carregarGrupos();
      
      // Se o produto foi movido para outro grupo, removê-lo da lista atual
      if (novoGrupoId !== grupoSelecionado?.id) {
        setProdutosGrupo(prev => prev.filter(p => p.id !== produtoId));
      }
      
      const novoGrupo = grupos.find(g => g.id === novoGrupoId);
      showSuccess('Sucesso', `Produto movido para o grupo "${novoGrupo?.descricao || novoGrupoId}"`);
    } catch (error: any) {
      console.error('❌ [GruposPage] Erro ao atualizar grupo do produto:', error);
      showError('Erro', error.response?.data?.error || 'Não foi possível atualizar o grupo do produto');
    } finally {
      setAtualizandoGrupo(null);
    }
  };

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <button
              onClick={onClose}
              className="w-12 h-12 bg-card border border-border rounded-xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover transition-all"
            >
              <ArrowLeft className="w-6 h-6" />
            </button>
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <Folder className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Grupos</h1>
              <p className="text-sm text-text-muted">Cadastro e consulta de grupos de produtos</p>
            </div>
          </div>
          <Button
            onClick={abrirFormularioNovo}
            className="flex items-center space-x-2"
          >
            <FolderPlus className="w-4 h-4" />
            <span>Novo Grupo</span>
          </Button>
        </div>

        {/* Campo de Busca */}
        <div className="mb-6">
          <div className="relative">
            <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-text-muted" />
            <input
              type="text"
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="Buscar por descrição..."
              className="w-full pl-12 pr-4 py-3 bg-background-secondary border border-border rounded-xl
                        focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                        text-text-primary placeholder-text-muted transition-all"
            />
            {busca && (
              <button
                onClick={() => setBusca('')}
                className="absolute right-4 top-1/2 transform -translate-y-1/2 w-8 h-8 flex items-center justify-center
                          text-text-muted hover:text-text-primary hover:bg-background-tertiary rounded-lg transition-all"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
        </div>

        {/* Lista de Grupos */}
        {loading ? (
          <div className="flex items-center justify-center py-20">
            <Loader2 className="w-8 h-8 text-primary animate-spin" />
          </div>
        ) : gruposFiltrados.length === 0 ? (
          <div className="text-center py-20">
            <Folder className="w-16 h-16 text-text-muted mx-auto mb-4" />
            <p className="text-text-secondary text-lg">
              {busca ? 'Nenhum grupo encontrado' : 'Nenhum grupo cadastrado'}
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {gruposFiltrados.map((grupo, index) => (
              <motion.div
                key={grupo.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.05 }}
                className="bg-card border border-border rounded-xl p-6 hover:border-border-secondary transition-all"
              >
                <div className="flex items-start justify-between mb-4">
                  <div className="flex-1">
                    <h3 className="text-lg font-semibold text-text-primary mb-1">
                      {grupo.descricao}
                    </h3>
                    <p className="text-sm text-text-muted">
                      {grupo.quantidadeProdutos} {grupo.quantidadeProdutos === 1 ? 'produto' : 'produtos'}
                    </p>
                    {grupo.imprimirDuasVias && (
                      <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-700 mt-1">
                        2 vias
                      </span>
                    )}
                  </div>
                </div>
                <div className="flex items-center space-x-2">
                  <button
                    onClick={(e) => {
                      e.preventDefault();
                      e.stopPropagation();

                      selecionarGrupo(grupo);
                    }}
                    className="flex-1 px-4 py-2 bg-primary/10 hover:bg-primary/20 
                              text-primary rounded-lg transition-all flex items-center justify-center space-x-2
                              active:bg-primary/30"
                    title="Ver produtos deste grupo"
                    type="button"
                  >
                    <Package className="w-4 h-4" />
                    <span>Ver Produtos</span>
                  </button>
                  <button
                    onClick={() => abrirFormularioEditar(grupo)}
                    className="px-4 py-2 bg-background-secondary hover:bg-background-tertiary 
                              text-text-primary rounded-lg transition-all"
                    title="Editar grupo"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => excluirGrupo(grupo.id)}
                    className="px-4 py-2 bg-red-500/10 hover:bg-red-500/20 
                              text-red-500 rounded-lg transition-all"
                    disabled={grupo.quantidadeProdutos > 0}
                    title={grupo.quantidadeProdutos > 0 ? 'Não é possível excluir grupo com produtos' : 'Excluir grupo'}
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </motion.div>
            ))}
          </div>
        )}

        {/* Seção de Produtos do Grupo Selecionado */}
        {grupoSelecionado && (
          <motion.div
            id="produtos-grupo-section"
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="mt-8 bg-card border border-border rounded-xl p-6"
          >
            <div className="flex items-center justify-between mb-4">
              <div className="flex items-center space-x-3">
                <Package className="w-6 h-6 text-primary" />
                <div>
                  <h2 className="text-xl font-bold text-text-primary">
                    Produtos do Grupo: {grupoSelecionado.descricao}
                  </h2>
                  <p className="text-sm text-text-muted">
                    {produtosGrupo.length} {produtosGrupo.length === 1 ? 'produto encontrado' : 'produtos encontrados'}
                  </p>
                </div>
              </div>
              <button
                onClick={() => {
                  setGrupoSelecionado(null);
                  setProdutosGrupo([]);
                }}
                className="w-10 h-10 flex items-center justify-center text-text-muted hover:text-text-primary 
                          hover:bg-background-secondary rounded-lg transition-all"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {carregandoProdutos ? (
              <div className="flex items-center justify-center py-12">
                <Loader2 className="w-8 h-8 text-primary animate-spin" />
              </div>
            ) : produtosGrupo.length === 0 ? (
              <div className="text-center py-12">
                <Package className="w-16 h-16 text-text-muted mx-auto mb-4" />
                <p className="text-text-secondary">Nenhum produto encontrado neste grupo</p>
              </div>
            ) : (
              <div className="space-y-2 max-h-96 overflow-y-auto">
                {produtosGrupo.map((produto) => (
                  <div
                    key={produto.id}
                    className="flex items-center space-x-3 p-3 bg-background-secondary rounded-lg hover:bg-background-tertiary transition-all"
                  >
                    <div className="flex-1 min-w-0">
                      <p className="font-medium text-text-primary break-words whitespace-normal">
                        {normalizarTexto(produto.descricao)}
                      </p>
                      {produto.codigoInterno && (
                        <p className="text-xs text-text-muted">
                          Código: {produto.codigoInterno}
                        </p>
                      )}
                    </div>
                    <div className="flex items-center space-x-2">
                      <select
                        value={produto.grupo || ''}
                        onChange={(e) => {
                          const novoGrupoId = parseInt(e.target.value);
                          if (novoGrupoId !== produto.grupo) {
                            atualizarGrupoProduto(produto.id, novoGrupoId);
                          }
                        }}
                        disabled={atualizandoGrupo === produto.id}
                        className="px-3 py-2 bg-background border border-border rounded-lg
                                  text-text-primary text-sm focus:outline-none focus:ring-2 
                                  focus:ring-primary/50 focus:border-primary/50 transition-all
                                  disabled:opacity-50 disabled:cursor-not-allowed min-w-[200px]"
                      >
                        <option value="">Selecione um grupo</option>
                        {grupos.map((grupo) => (
                          <option key={grupo.id} value={grupo.id}>
                            {grupo.descricao} ({grupo.quantidadeProdutos})
                          </option>
                        ))}
                      </select>
                      {atualizandoGrupo === produto.id && (
                        <Loader2 className="w-4 h-4 text-primary animate-spin" />
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </motion.div>
        )}

        {/* Modal de Formulário */}
        {mostrarFormulario && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <motion.div
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              className="bg-background border border-border rounded-2xl p-6 w-full max-w-md"
            >
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-2xl font-bold text-text-primary">
                  {grupoEditando ? 'Editar Grupo' : 'Novo Grupo'}
                </h2>
                <button
                  onClick={fecharFormulario}
                  className="w-10 h-10 flex items-center justify-center text-text-muted hover:text-text-primary 
                            hover:bg-background-secondary rounded-lg transition-all"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-text-secondary mb-2">
                    Descrição
                  </label>
                  <input
                    type="text"
                    value={descricao}
                    onChange={(e) => setDescricao(e.target.value)}
                    placeholder="Digite a descrição do grupo"
                    className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                              focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                              text-text-primary placeholder-text-muted transition-all"
                    autoFocus
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        salvarGrupo();
                      }
                    }}
                  />
                </div>

                <div className="flex items-center justify-between py-2">
                  <div>
                    <p className="text-sm font-medium text-text-primary">Imprimir 2 vias</p>
                    <p className="text-xs text-text-muted">Imprime automaticamente 2 cópias quando ativo globalmente</p>
                  </div>
                  <div
                    role="switch"
                    aria-checked={imprimirDuasVias}
                    tabIndex={0}
                    onClick={() => setImprimirDuasVias(p => !p)}
                    onKeyDown={(e) => { if (e.key === ' ' || e.key === 'Enter') setImprimirDuasVias(p => !p); }}
                    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-primary/50 cursor-pointer ${
                      imprimirDuasVias ? 'bg-green-500' : 'bg-gray-300'
                    }`}
                  >
                    <span className={`inline-block h-4 w-4 transform rounded-full bg-white shadow-md transition-transform duration-200 ${imprimirDuasVias ? 'translate-x-6' : 'translate-x-1'}`} />
                  </div>
                </div>

                <div className="flex items-center space-x-3 pt-4">
                  <Button
                    onClick={salvarGrupo}
                    disabled={salvando || !descricao.trim()}
                    className="flex-1"
                  >
                    {salvando ? (
                      <>
                        <Loader2 className="w-4 h-4 animate-spin mr-2" />
                        <span>Salvando...</span>
                      </>
                    ) : (
                      <span>{grupoEditando ? 'Atualizar' : 'Criar'}</span>
                    )}
                  </Button>
                  <Button
                    onClick={fecharFormulario}
                    variant="outline"
                    disabled={salvando}
                  >
                    Cancelar
                  </Button>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </div>
    </div>
  );
};

export default GruposPage;
