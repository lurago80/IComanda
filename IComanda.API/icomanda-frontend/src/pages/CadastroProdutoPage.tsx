import { 
  Loader2, 
  X, 
  Save,
  Package,
  ChevronDown
} from 'lucide-react';
import React, { useEffect, useState, useCallback } from 'react';
import { useToast } from '../hooks/useToast';
import { produtosService, gruposService } from '../services/api';
import { Button } from '../components/ui/button';
import { Grupo } from '../types/api';

interface CadastroProdutoPageProps {
  onClose: () => void;
  produtoId?: number;
}

const CadastroProdutoPage: React.FC<CadastroProdutoPageProps> = ({ onClose, produtoId }) => {
  const [loading, setLoading] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [grupos, setGrupos] = useState<Grupo[]>([]);
  const [carregandoGrupos, setCarregandoGrupos] = useState(false);
  const { showSuccess, showError } = useToast();

  // Campos principais - PRODUTOESERVICO
  const [descricao, setDescricao] = useState('');
  const [caracteristica, setCaracteristica] = useState('');
  const [codigoInterno, setCodigoInterno] = useState('');
  const [ativo, setAtivo] = useState<number>(1);
  const [observacao, setObservacao] = useState('');
  const [tipoItemId, setTipoItemId] = useState<number>(1);
  const [unidadeMedidaId, setUnidadeMedidaId] = useState<number>(1);
  const [grupo, setGrupo] = useState<number | undefined>();
  const [unMedida, setUnMedida] = useState('');
  const [cfop, setCfop] = useState('');
  const [csosn, setCsosn] = useState('');
  const [cest, setCest] = useState('');
  const [ncm, setNcm] = useState('');
  const [cst, setCst] = useState('');
  const [cstOrigem, setCstOrigem] = useState('');
  const [icms, setIcms] = useState<number | undefined>();
  const [margem, setMargem] = useState<number | undefined>();
  const [marca, setMarca] = useState('');
  const [categoria, setCategoria] = useState('');
  const [cor, setCor] = useState('');
  const [tamanho, setTamanho] = useState('');
  const [tipo, setTipo] = useState('');

  // Campos - PRODUTO
  const [codigoBarra, setCodigoBarra] = useState('');
  const [pesavel, setPesavel] = useState<number | undefined>();

  // Campos - PRODUTOEMPRESA
  const [quantidade, setQuantidade] = useState<number>(0);
  const [quantidadeMinima, setQuantidadeMinima] = useState<number | undefined>();
  const [quantidadeMaxima, setQuantidadeMaxima] = useState<number | undefined>();
  const [localizacao, setLocalizacao] = useState('');
  const [fabricante, setFabricante] = useState('');

  // Campos - PRODUTOESERVICOEMPRESA
  const [precoVenda, setPrecoVenda] = useState<number | undefined>();
  const [precoCusto, setPrecoCusto] = useState<number | undefined>();
  const [precoCustoMedio, setPrecoCustoMedio] = useState<number | undefined>();
  const [atacado, setAtacado] = useState<number | undefined>();
  const [preco3, setPreco3] = useState<number | undefined>();
  const [pessoaId, setPessoaId] = useState<number>(1);

  const carregarProduto = useCallback(async () => {
    if (!produtoId) return;
    
    try {
      setLoading(true);
      const produto = await produtosService.getCompleto(produtoId);
      
      setDescricao(produto.descricao || '');
      setCaracteristica(produto.caracteristica || '');
      setCodigoInterno(produto.codigoInterno || '');
      setAtivo(produto.ativo ?? 1);
      setObservacao(produto.observacao || '');
      setGrupo(produto.grupo);
      setUnMedida(produto.unMedida || '');
      setCfop(produto.cfop || '');
      setCsosn(produto.csosn || '');
      setCest(produto.cest || '');
      setNcm(produto.ncm || '');
      setCst(produto.cst || '');
      setCstOrigem(produto.cstOrigem || '');
      setIcms(produto.icms);
      setMargem(produto.margem);
      setMarca(produto.marca || '');
      setCategoria(produto.categoria || '');
      setCor(produto.cor || '');
      setTamanho(produto.tamanho || '');
      setTipo(produto.tipo || '');
      setCodigoBarra(produto.codigoBarra || '');
      setPesavel(produto.pesavel);
      setQuantidade(produto.quantidade || 0);
      setQuantidadeMinima(produto.quantidadeMinima);
      setQuantidadeMaxima(produto.quantidadeMaxima);
      setLocalizacao(produto.localizacao || '');
      setFabricante(produto.fabricante || '');
      setPrecoVenda(produto.precoVenda);
      setPrecoCusto(produto.precoCusto);
      setPrecoCustoMedio(produto.precoCustoMedio);
      setAtacado(produto.atacado);
      setPreco3(produto.preco3);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar os dados do produto');
      console.error('Erro ao carregar produto:', error);
    } finally {
      setLoading(false);
    }
  }, [produtoId, showError]);

  const carregarGrupos = useCallback(async () => {
    try {
      setCarregandoGrupos(true);
      const listaGrupos = await gruposService.getAll();
      // Ordenar por descrição
      listaGrupos.sort((a, b) => a.descricao.localeCompare(b.descricao));
      setGrupos(listaGrupos);
    } catch (error: any) {
      console.error('❌ Erro ao carregar grupos:', error);
      showError('Erro', 'Não foi possível carregar a lista de grupos');
    } finally {
      setCarregandoGrupos(false);
    }
  }, [showError]);

  useEffect(() => {
    carregarProduto();
    carregarGrupos();
  }, [carregarProduto, carregarGrupos]);

  const handleSalvar = async () => {
    if (!descricao.trim()) {
      showError('Erro', 'A descrição do produto é obrigatória');
      return;
    }

    try {
      setSalvando(true);

      const dadosProduto: any = {
        descricao: descricao.trim(),
        caracteristica: caracteristica || undefined,
        codigoInterno: codigoInterno || undefined,
        ativo: ativo,
        observacao: observacao || undefined,
        tipoItemId,
        unidadeMedidaId,
        grupo: grupo || undefined,
        unMedida: unMedida || undefined,
        cfop: cfop || undefined,
        csosn: csosn || undefined,
        cest: cest || undefined,
        ncm: ncm || undefined,
        cst: cst || undefined,
        cstOrigem: cstOrigem || undefined,
        icms: icms || undefined,
        margem: margem || undefined,
        marca: marca || undefined,
        categoria: categoria || undefined,
        cor: cor || undefined,
        tamanho: tamanho || undefined,
        tipo: tipo || undefined,
        codigoBarra: codigoBarra || undefined,
        pesavel: pesavel || undefined,
        quantidade: quantidade || 0,
        quantidadeMinima: quantidadeMinima || undefined,
        quantidadeMaxima: quantidadeMaxima || undefined,
        localizacao: localizacao || undefined,
        fabricante: fabricante || undefined,
        precoVenda: precoVenda || undefined,
        precoCusto: precoCusto || undefined,
        precoCustoMedio: precoCustoMedio || undefined,
        atacado: atacado || undefined,
        preco3: preco3 || undefined,
        pessoaId
      };

      console.log('📤 Dados do produto a serem enviados:', dadosProduto);

      if (produtoId) {
        await produtosService.atualizar(produtoId, dadosProduto);
        showSuccess('Sucesso', 'Produto atualizado com sucesso');
      } else {
        const id = await produtosService.criar(dadosProduto);
        console.log('✅ Produto criado com ID:', id);
        showSuccess('Sucesso', 'Produto criado com sucesso');
      }

      setTimeout(() => {
        onClose();
      }, 1000);
    } catch (error: any) {
      console.error('❌ Erro completo ao salvar produto:', error);
      console.error('❌ Response:', error.response);
      console.error('❌ Response Data:', error.response?.data);
      console.error('❌ Response Status:', error.response?.status);
      
      const errorMessage = error.response?.data?.error || 
                         error.response?.data?.message || 
                         error.message || 
                         'Não foi possível salvar o produto';
      
      const errorDetails = error.response?.data?.details ? 
                          `\nDetalhes: ${error.response.data.details}` : '';
      
      showError('Erro', `${errorMessage}${errorDetails}`);
    } finally {
      setSalvando(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <Loader2 className="w-8 h-8 text-primary mx-auto mb-4 animate-spin" />
          <p className="text-text-muted">Carregando produto...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <Package className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">
                {produtoId ? 'Editar Produto' : 'Novo Produto'}
              </h1>
              <p className="text-sm text-text-muted">Cadastro completo de produto</p>
            </div>
          </div>
          <Button
            onClick={onClose}
            variant="outline"
            className="flex items-center space-x-2"
          >
            <X className="w-4 h-4" />
            <span className="hidden sm:inline">Fechar</span>
          </Button>
        </div>

        {/* Formulário */}
        <div className="bg-card rounded-2xl p-6 shadow-lg border border-border space-y-6">
          {/* Dados Básicos */}
          <div>
            <h2 className="text-lg font-bold text-text-primary mb-4">Dados Básicos</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Descrição <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={descricao}
                  onChange={(e) => setDescricao(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Descrição do produto"
                  required
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Código Interno
                </label>
                <input
                  type="text"
                  value={codigoInterno}
                  onChange={(e) => setCodigoInterno(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Código interno"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Código de Barras
                </label>
                <input
                  type="text"
                  value={codigoBarra}
                  onChange={(e) => setCodigoBarra(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Código de barras"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Característica
                </label>
                <input
                  type="text"
                  value={caracteristica}
                  onChange={(e) => setCaracteristica(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Característica do produto"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Grupo
                </label>
                <div className="relative">
                  <select
                    value={grupo || ''}
                    onChange={(e) => setGrupo(e.target.value ? parseInt(e.target.value) : undefined)}
                    className="w-full px-4 py-3 pr-10 bg-background-secondary border border-border rounded-xl
                              focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                              text-text-primary appearance-none cursor-pointer
                              hover:bg-background-secondary/80 transition-colors
                              disabled:opacity-50 disabled:cursor-not-allowed"
                    disabled={carregandoGrupos}
                  >
                    <option value="">Selecione um grupo</option>
                    {grupos.map((g) => (
                      <option key={g.id} value={g.id}>
                        {g.descricao} {g.quantidadeProdutos !== undefined ? `(${g.quantidadeProdutos})` : ''}
                      </option>
                    ))}
                  </select>
                  <ChevronDown className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-text-muted pointer-events-none" />
                </div>
                {carregandoGrupos && (
                  <p className="text-xs text-text-muted mt-1 flex items-center">
                    <Loader2 className="w-3 h-3 animate-spin mr-1" />
                    Carregando grupos...
                  </p>
                )}
                {!carregandoGrupos && grupos.length === 0 && (
                  <p className="text-xs text-text-muted mt-1">Nenhum grupo cadastrado</p>
                )}
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Unidade de Medida
                </label>
                <input
                  type="text"
                  value={unMedida}
                  onChange={(e) => setUnMedida(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Ex: UN, KG, LT"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Marca
                </label>
                <input
                  type="text"
                  value={marca}
                  onChange={(e) => setMarca(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Marca do produto"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Categoria
                </label>
                <input
                  type="text"
                  value={categoria}
                  onChange={(e) => setCategoria(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Categoria"
                />
              </div>
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Observação
                </label>
                <textarea
                  value={observacao}
                  onChange={(e) => setObservacao(e.target.value)}
                  rows={3}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Observações sobre o produto"
                />
              </div>
              <div>
                <label className="flex items-center space-x-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={ativo === 1}
                    onChange={(e) => setAtivo(e.target.checked ? 1 : 0)}
                    className="w-5 h-5 rounded border-border text-primary focus:ring-primary/50"
                  />
                  <span className="text-sm text-text-secondary">Produto Ativo</span>
                </label>
              </div>
            </div>
          </div>

          {/* Estoque */}
          <div>
            <h2 className="text-lg font-bold text-text-primary mb-4">Estoque</h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Quantidade
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={quantidade}
                  onChange={(e) => setQuantidade(parseFloat(e.target.value) || 0)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Quantidade Mínima
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={quantidadeMinima || ''}
                  onChange={(e) => setQuantidadeMinima(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Quantidade Máxima
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={quantidadeMaxima || ''}
                  onChange={(e) => setQuantidadeMaxima(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Localização
                </label>
                <input
                  type="text"
                  value={localizacao}
                  onChange={(e) => setLocalizacao(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Localização no estoque"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Fabricante
                </label>
                <input
                  type="text"
                  value={fabricante}
                  onChange={(e) => setFabricante(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="Fabricante"
                />
              </div>
            </div>
          </div>

          {/* Preços */}
          <div>
            <h2 className="text-lg font-bold text-text-primary mb-4">Preços</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Preço de Venda
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={precoVenda || ''}
                  onChange={(e) => setPrecoVenda(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="0.00"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Preço de Custo
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={precoCusto || ''}
                  onChange={(e) => setPrecoCusto(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="0.00"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Preço Atacado
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={atacado || ''}
                  onChange={(e) => setAtacado(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="0.00"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Preço 3
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={preco3 || ''}
                  onChange={(e) => setPreco3(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="0.00"
                />
              </div>
            </div>
          </div>

          {/* Tributação */}
          <div>
            <h2 className="text-lg font-bold text-text-primary mb-4">Tributação</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  NCM
                </label>
                <input
                  type="text"
                  value={ncm}
                  onChange={(e) => setNcm(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="NCM"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  CFOP
                </label>
                <input
                  type="text"
                  value={cfop}
                  onChange={(e) => setCfop(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="CFOP"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  CSOSN
                </label>
                <input
                  type="text"
                  value={csosn}
                  onChange={(e) => setCsosn(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="CSOSN"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  CEST
                </label>
                <input
                  type="text"
                  value={cest}
                  onChange={(e) => setCest(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="CEST"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  CST
                </label>
                <input
                  type="text"
                  value={cst}
                  onChange={(e) => setCst(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="CST"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  CST Origem
                </label>
                <input
                  type="text"
                  value={cstOrigem}
                  onChange={(e) => setCstOrigem(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="CST Origem"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  ICMS (%)
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={icms || ''}
                  onChange={(e) => setIcms(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="0.00"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Margem (%)
                </label>
                <input
                  type="number"
                  step="0.01"
                  value={margem || ''}
                  onChange={(e) => setMargem(e.target.value ? parseFloat(e.target.value) : undefined)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                            text-text-primary"
                  placeholder="0.00"
                />
              </div>
            </div>
          </div>

          {/* Botões */}
          <div className="flex justify-end gap-4 pt-4 border-t border-border">
            <Button
              onClick={onClose}
              variant="outline"
              disabled={salvando}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSalvar}
              disabled={salvando || !descricao.trim()}
              className="flex items-center space-x-2"
            >
              {salvando ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  <span>Salvando...</span>
                </>
              ) : (
                <>
                  <Save className="w-4 h-4" />
                  <span>Salvar</span>
                </>
              )}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CadastroProdutoPage;
