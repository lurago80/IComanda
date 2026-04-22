import { motion } from 'framer-motion';
import { Loader2, X, Search, Truck, Plus, Edit, Trash2, ArrowLeft } from 'lucide-react';
import React, { useEffect, useState, useCallback } from 'react';
import { useToast } from '../hooks/useToast';
import { taxasEntregaService } from '../services/api';
import { Button } from '../components/ui/button';
import { TaxaEntrega } from '../types/api';

interface TaxasEntregaPageProps {
  onClose: () => void;
}

const TaxasEntregaPage: React.FC<TaxasEntregaPageProps> = ({ onClose }) => {
  const [taxas, setTaxas] = useState<TaxaEntrega[]>([]);
  const [loading, setLoading] = useState(false);
  const [busca, setBusca] = useState('');
  const [mostrarFormulario, setMostrarFormulario] = useState(false);
  const [taxaEditando, setTaxaEditando] = useState<TaxaEntrega | null>(null);
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState<string>('');
  const [salvando, setSalvando] = useState(false);

  const { showError, showSuccess } = useToast();

  const carregarTaxas = useCallback(async () => {
    try {
      setLoading(true);
      const dados = await taxasEntregaService.getAll();
      setTaxas(dados);
    } catch (error: any) {
      console.error('Erro ao carregar taxas de entrega:', error);
      showError('Erro', error.response?.data?.error || 'Não foi possível carregar as taxas de entrega. Execute o script Scripts/criar_tabela_taxa_entrega.sql no Firebird se a tabela não existir.');
    } finally {
      setLoading(false);
    }
  }, [showError]);

  useEffect(() => {
    carregarTaxas();
  }, [carregarTaxas]);

  const taxasFiltradas = taxas.filter(
    (t) =>
      t.descricao.toLowerCase().includes(busca.toLowerCase()) ||
      String(t.valor).includes(busca)
  );

  const abrirFormularioNovo = () => {
    setTaxaEditando(null);
    setDescricao('');
    setValor('');
    setMostrarFormulario(true);
  };

  const abrirFormularioEditar = (taxa: TaxaEntrega) => {
    setTaxaEditando(taxa);
    setDescricao(taxa.descricao);
    setValor(String(taxa.valor));
    setMostrarFormulario(true);
  };

  const fecharFormulario = () => {
    setMostrarFormulario(false);
    setTaxaEditando(null);
    setDescricao('');
    setValor('');
  };

  const salvarTaxa = async () => {
    const desc = descricao.trim();
    if (!desc) {
      showError('Validação', 'A descrição é obrigatória');
      return;
    }
    const v = parseFloat(valor.replace(',', '.'));
    if (isNaN(v) || v < 0) {
      showError('Validação', 'Informe um valor válido');
      return;
    }

    try {
      setSalvando(true);
      if (taxaEditando) {
        await taxasEntregaService.atualizar(taxaEditando.id, desc, v);
        showSuccess('Sucesso', 'Taxa de entrega atualizada com sucesso');
      } else {
        await taxasEntregaService.criar(desc, v);
        showSuccess('Sucesso', 'Taxa de entrega criada com sucesso');
      }
      fecharFormulario();
      carregarTaxas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.error || 'Não foi possível salvar');
    } finally {
      setSalvando(false);
    }
  };

  const excluirTaxa = async (id: number) => {
    const taxa = taxas.find((t) => t.id === id);
    if (!taxa) return;
    if (!window.confirm(`Excluir a taxa "${taxa.descricao}"?`)) return;
    try {
      await taxasEntregaService.excluir(id);
      showSuccess('Sucesso', 'Taxa de entrega excluída');
      carregarTaxas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.error || 'Não foi possível excluir');
    }
  };

  const formatarValor = (v: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v);

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-4xl mx-auto">
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <button
              onClick={onClose}
              className="w-12 h-12 bg-card border border-border rounded-xl flex items-center justify-center text-text-secondary hover:text-text-primary hover:bg-card-hover transition-all"
            >
              <ArrowLeft className="w-6 h-6" />
            </button>
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <Truck className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">Taxas de Entrega</h1>
              <p className="text-sm text-text-muted">Cadastro de taxas para o produto TAXA DE ENTREGA</p>
            </div>
          </div>
          <Button onClick={abrirFormularioNovo} className="flex items-center space-x-2">
            <Plus className="w-4 h-4" />
            <span>Nova Taxa</span>
          </Button>
        </div>

        <div className="mb-6">
          <div className="relative">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-text-muted" />
            <input
              type="text"
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="Buscar por descrição ou valor..."
              className="w-full pl-12 pr-4 py-3 bg-background-secondary border border-border rounded-xl focus:outline-none focus:ring-2 focus:ring-primary/50 text-text-primary placeholder-text-muted"
            />
            {busca && (
              <button
                onClick={() => setBusca('')}
                className="absolute right-4 top-1/2 -translate-y-1/2 w-8 h-8 flex items-center justify-center text-text-muted hover:text-text-primary rounded-lg"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
        </div>

        {loading ? (
          <div className="flex justify-center py-20">
            <Loader2 className="w-8 h-8 text-primary animate-spin" />
          </div>
        ) : taxasFiltradas.length === 0 ? (
          <div className="text-center py-20">
            <Truck className="w-16 h-16 text-text-muted mx-auto mb-4" />
            <p className="text-text-secondary text-lg">
              {busca ? 'Nenhuma taxa encontrada' : 'Nenhuma taxa de entrega cadastrada'}
            </p>
            {!busca && (
              <Button onClick={abrirFormularioNovo} className="mt-4">
                Cadastrar primeira taxa
              </Button>
            )}
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {taxasFiltradas.map((taxa, index) => (
              <motion.div
                key={taxa.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: index * 0.05 }}
                className="bg-card border border-border rounded-xl p-5 hover:border-border-secondary transition-all"
              >
                <p className="font-medium text-text-primary mb-1">{taxa.descricao}</p>
                <p className="text-xl font-bold text-primary mb-4">{formatarValor(taxa.valor)}</p>
                <div className="flex gap-2">
                  <button
                    onClick={() => abrirFormularioEditar(taxa)}
                    className="flex-1 flex items-center justify-center gap-2 py-2 px-3 bg-background-secondary border border-border rounded-lg text-text-secondary hover:text-primary hover:border-primary/50 transition-all"
                  >
                    <Edit className="w-4 h-4" />
                    Editar
                  </button>
                  <button
                    onClick={() => excluirTaxa(taxa.id)}
                    className="flex items-center justify-center py-2 px-3 border border-border rounded-lg text-text-muted hover:text-danger hover:border-danger/50 transition-all"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>
              </motion.div>
            ))}
          </div>
        )}
      </div>

      {/* Modal Formulário */}
      {mostrarFormulario && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="bg-card border border-border rounded-2xl shadow-xl w-full max-w-md p-6"
          >
            <h2 className="text-xl font-bold text-text-primary mb-4">
              {taxaEditando ? 'Editar taxa de entrega' : 'Nova taxa de entrega'}
            </h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Descrição</label>
                <input
                  type="text"
                  value={descricao}
                  onChange={(e) => setDescricao(e.target.value)}
                  placeholder="Ex: Centro, Bairros próximos..."
                  className="w-full px-4 py-3 bg-background border border-border rounded-xl focus:outline-none focus:ring-2 focus:ring-primary/50 text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-1">Valor (R$)</label>
                <input
                  type="text"
                  value={valor}
                  onChange={(e) => setValor(e.target.value)}
                  placeholder="0,00"
                  className="w-full px-4 py-3 bg-background border border-border rounded-xl focus:outline-none focus:ring-2 focus:ring-primary/50 text-text-primary"
                />
              </div>
            </div>
            <div className="flex gap-3 mt-6">
              <Button variant="outline" onClick={fecharFormulario} className="flex-1">
                Cancelar
              </Button>
              <Button onClick={salvarTaxa} disabled={salvando} className="flex-1">
                {salvando ? <Loader2 className="w-4 h-4 animate-spin" /> : 'Salvar'}
              </Button>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
};

export default TaxasEntregaPage;
