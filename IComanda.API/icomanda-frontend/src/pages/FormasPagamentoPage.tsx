import { useState, useEffect } from 'react';
import { ArrowLeft, Plus, Pencil, ToggleLeft, ToggleRight, Search, CreditCard } from 'lucide-react';
import { formasPagamentoService } from '../services/api';

interface FormasPagamentoPageProps {
  onClose: () => void;
}

interface FormaPagamentoItem {
  id: number;
  descricao: string;
  ativo: boolean;
  indice?: number | null;
  moeda: number;
  meioPagto: number;
  permiteTroco: boolean;
  tipo?: string | null;
}

type ModalMode = 'criar' | 'editar' | null;

const MEIO_PAGTO_LABELS: Record<number, string> = {
  0: 'Dinheiro',
  1: 'Crédito',
  2: 'Débito',
  3: 'Pix',
  4: 'Voucher',
  99: 'Outro',
};

const defaultForm = {
  descricao: '',
  indice: '' as string | number,
  meioPagto: 0,
  permiteTroco: false,
  tipo: '',
};

export default function FormasPagamentoPage({ onClose }: FormasPagamentoPageProps) {
  const [formas, setFormas] = useState<FormaPagamentoItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [selected, setSelected] = useState<FormaPagamentoItem | null>(null);
  const [form, setForm] = useState(defaultForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const carregar = async () => {
    setLoading(true);
    setError('');
    try {
      const data = await formasPagamentoService.listar();
      setFormas(data);
    } catch {
      setError('Erro ao carregar formas de pagamento.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { carregar(); }, []);

  const formasFiltradas = formas.filter(f =>
    f.descricao.toLowerCase().includes(busca.toLowerCase())
  );

  const abrirCriar = () => {
    setForm(defaultForm);
    setSelected(null);
    setError('');
    setModalMode('criar');
  };

  const abrirEditar = (f: FormaPagamentoItem) => {
    setForm({
      descricao: f.descricao,
      indice: f.indice ?? '',
      meioPagto: f.meioPagto,
      permiteTroco: f.permiteTroco,
      tipo: f.tipo ?? '',
    });
    setSelected(f);
    setError('');
    setModalMode('editar');
  };

  const fecharModal = () => {
    setModalMode(null);
    setSelected(null);
    setError('');
  };

  const handleToggleAtivo = async (f: FormaPagamentoItem) => {
    try {
      await formasPagamentoService.toggleAtivo(f.id);
      setFormas(prev => prev.map(x => x.id === f.id ? { ...x, ativo: !x.ativo } : x));
    } catch {
      setError('Erro ao alterar status.');
    }
  };

  const handleSalvar = async () => {
    if (!form.descricao.trim()) {
      setError('Descrição é obrigatória.');
      return;
    }
    setSaving(true);
    setError('');
    try {
      const payload = {
        descricao: form.descricao.trim(),
        indice: form.indice === '' ? null : Number(form.indice),
        meioPagto: form.meioPagto,
        permiteTroco: form.permiteTroco,
        tipo: form.tipo?.trim() || null,
      };

      if (modalMode === 'criar') {
        await formasPagamentoService.criar(payload);
      } else if (modalMode === 'editar' && selected) {
        await formasPagamentoService.atualizar(selected.id, payload);
      }
      await carregar();
      fecharModal();
    } catch {
      setError('Erro ao salvar. Verifique os dados e tente novamente.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 px-4 py-3 flex items-center gap-3">
        <button
          onClick={onClose}
          className="p-2 rounded-lg hover:bg-gray-100 transition-colors"
          title="Voltar"
        >
          <ArrowLeft size={20} className="text-gray-600" />
        </button>
        <CreditCard size={22} className="text-indigo-600" />
        <h1 className="text-lg font-semibold text-gray-800">Formas de Pagamento</h1>
        <div className="flex-1" />
        <button
          onClick={abrirCriar}
          className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition-colors"
        >
          <Plus size={16} /> Nova Forma
        </button>
      </div>

      <div className="p-4 max-w-3xl mx-auto">
        {/* Busca */}
        <div className="relative mb-4">
          <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar forma de pagamento..."
            value={busca}
            onChange={e => setBusca(e.target.value)}
            className="w-full pl-9 pr-4 py-2 border border-gray-200 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
          />
        </div>

        {error && !modalMode && (
          <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">{error}</div>
        )}

        {/* Tabela */}
        {loading ? (
          <div className="text-center py-10 text-gray-400">Carregando...</div>
        ) : formasFiltradas.length === 0 ? (
          <div className="text-center py-10 text-gray-400">Nenhuma forma de pagamento encontrada.</div>
        ) : (
          <div className="bg-white rounded-xl shadow-sm overflow-hidden border border-gray-100">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-gray-500 uppercase text-xs">
                <tr>
                  <th className="px-4 py-3 text-left">Descrição</th>
                  <th className="px-4 py-3 text-left">Meio</th>
                  <th className="px-4 py-3 text-center">Troco</th>
                  <th className="px-4 py-3 text-center">Ordem</th>
                  <th className="px-4 py-3 text-center">Ativo</th>
                  <th className="px-4 py-3 text-center">Ações</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {formasFiltradas.map(f => (
                  <tr key={f.id} className={`transition-colors ${f.ativo ? 'hover:bg-gray-50' : 'opacity-50 hover:bg-gray-50'}`}>
                    <td className="px-4 py-3 font-medium text-gray-800">{f.descricao}</td>
                    <td className="px-4 py-3 text-gray-600">{MEIO_PAGTO_LABELS[f.meioPagto] ?? `Código ${f.meioPagto}`}</td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${f.permiteTroco ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                        {f.permiteTroco ? 'Sim' : 'Não'}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center text-gray-500">{f.indice ?? '—'}</td>
                    <td className="px-4 py-3 text-center">
                      <button
                        onClick={() => handleToggleAtivo(f)}
                        title={f.ativo ? 'Desativar' : 'Ativar'}
                        className="inline-flex items-center justify-center hover:scale-110 transition-transform"
                      >
                        {f.ativo
                          ? <ToggleRight size={26} className="text-green-500" />
                          : <ToggleLeft size={26} className="text-gray-400" />}
                      </button>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <button
                        onClick={() => abrirEditar(f)}
                        className="p-1.5 rounded hover:bg-indigo-50 text-indigo-600 transition-colors"
                        title="Editar"
                      >
                        <Pencil size={15} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        <p className="text-xs text-gray-400 mt-3 text-right">
          {formasFiltradas.length} forma{formasFiltradas.length !== 1 ? 's' : ''} exibida{formasFiltradas.length !== 1 ? 's' : ''}
        </p>
      </div>

      {/* Modal Criar/Editar */}
      {modalMode && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-md">
            <div className="p-5 border-b border-gray-100 flex items-center gap-2">
              <CreditCard size={20} className="text-indigo-600" />
              <h2 className="text-base font-semibold text-gray-800">
                {modalMode === 'criar' ? 'Nova Forma de Pagamento' : 'Editar Forma de Pagamento'}
              </h2>
            </div>
            <div className="p-5 space-y-4">
              {error && (
                <div className="p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">{error}</div>
              )}

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Descrição *</label>
                <input
                  type="text"
                  maxLength={50}
                  value={form.descricao}
                  onChange={e => setForm(p => ({ ...p, descricao: e.target.value }))}
                  className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
                  placeholder="Ex: DINHEIRO, PIX, CARTÃO CRÉDITO"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Meio de Pagamento</label>
                  <select
                    value={form.meioPagto}
                    onChange={e => setForm(p => ({ ...p, meioPagto: Number(e.target.value) }))}
                    className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
                  >
                    {Object.entries(MEIO_PAGTO_LABELS).map(([k, v]) => (
                      <option key={k} value={k}>{v}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Ordem (Índice)</label>
                  <input
                    type="number"
                    min={0}
                    value={form.indice}
                    onChange={e => setForm(p => ({ ...p, indice: e.target.value }))}
                    className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
                    placeholder="0"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">Tipo</label>
                  <input
                    type="text"
                    maxLength={30}
                    value={form.tipo}
                    onChange={e => setForm(p => ({ ...p, tipo: e.target.value }))}
                    className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400"
                    placeholder="Opcional"
                  />
                </div>

                <div className="flex flex-col justify-end">
                  <label className="flex items-center gap-2 cursor-pointer select-none">
                    <input
                      type="checkbox"
                      checked={form.permiteTroco}
                      onChange={e => setForm(p => ({ ...p, permiteTroco: e.target.checked }))}
                      className="w-4 h-4 rounded accent-indigo-600"
                    />
                    <span className="text-sm text-gray-700">Permite troco</span>
                  </label>
                </div>
              </div>
            </div>
            <div className="p-5 border-t border-gray-100 flex justify-end gap-3">
              <button
                onClick={fecharModal}
                disabled={saving}
                className="px-4 py-2 text-sm text-gray-600 border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
              >
                Cancelar
              </button>
              <button
                onClick={handleSalvar}
                disabled={saving}
                className="px-4 py-2 text-sm font-medium bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg transition-colors disabled:opacity-50"
              >
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
