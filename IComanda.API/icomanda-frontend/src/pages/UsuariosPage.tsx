import { useState, useEffect } from 'react';
import { ArrowLeft, Plus, Pencil, Trash2, KeyRound, Search, UserCheck, UserX, ShieldCheck, ShieldOff } from 'lucide-react';
import { usuariosService, UsuarioDto, CreateUsuarioRequest, UpdateUsuarioRequest } from '../services/api';

interface UsuariosPageProps {
  onClose: () => void;
}

type ModalMode = 'criar' | 'editar' | 'senha' | null;

const TIPO_LABELS: Record<string, string> = {
  '0': 'Garçom',
  '1': 'Caixa',
  '2': 'Gerente',
};

const defaultForm: CreateUsuarioRequest = {
  nome: '',
  senha: '',
  ativo: true,
  bloqueado: false,
  podeVisualizar: false,
  podeVerTotal: true,
  podeCancelar: false,
  tipo: '0',
};

export default function UsuariosPage({ onClose }: UsuariosPageProps) {
  const [usuarios, setUsuarios] = useState<UsuarioDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [busca, setBusca] = useState('');
  const [modalMode, setModalMode] = useState<ModalMode>(null);
  const [selectedUser, setSelectedUser] = useState<UsuarioDto | null>(null);
  const [form, setForm] = useState<CreateUsuarioRequest>(defaultForm);
  const [novaSenha, setNovaSenha] = useState('');
  const [confirmSenha, setConfirmSenha] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [confirmDelete, setConfirmDelete] = useState<UsuarioDto | null>(null);

  const carregar = async () => {
    setLoading(true);
    try {
      const data = await usuariosService.listar();
      setUsuarios(data);
    } catch {
      setError('Erro ao carregar usuários');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { carregar(); }, []);

  const usuariosFiltrados = usuarios.filter(u =>
    u.nome.toLowerCase().includes(busca.toLowerCase())
  );

  const abrirCriar = () => {
    setForm(defaultForm);
    setError('');
    setModalMode('criar');
  };

  const abrirEditar = (u: UsuarioDto) => {
    setSelectedUser(u);
    setForm({
      nome: u.nome,
      senha: '',
      ativo: u.ativo,
      bloqueado: u.bloqueado,
      podeVisualizar: u.podeVisualizar,
      podeVerTotal: u.podeVerTotal,
      podeCancelar: u.podeCancelar,
      tipo: u.tipo,
    });
    setError('');
    setModalMode('editar');
  };

  const abrirSenha = (u: UsuarioDto) => {
    setSelectedUser(u);
    setNovaSenha('');
    setConfirmSenha('');
    setError('');
    setModalMode('senha');
  };

  const fecharModal = () => {
    setModalMode(null);
    setSelectedUser(null);
    setError('');
  };

  const handleSalvar = async () => {
    if (!form.nome.trim()) { setError('Nome é obrigatório'); return; }
    if (modalMode === 'criar' && !form.senha.trim()) { setError('Senha é obrigatória'); return; }

    setSaving(true);
    setError('');
    try {
      if (modalMode === 'criar') {
        await usuariosService.criar(form);
      } else if (modalMode === 'editar' && selectedUser) {
        const req: UpdateUsuarioRequest = {
          nome: form.nome,
          ativo: form.ativo,
          bloqueado: form.bloqueado,
          podeVisualizar: form.podeVisualizar,
          podeVerTotal: form.podeVerTotal,
          podeCancelar: form.podeCancelar,
          tipo: form.tipo,
        };
        await usuariosService.atualizar(selectedUser.id, req);
      }
      await carregar();
      fecharModal();
    } catch (e: unknown) {
      const msg = (e as { response?: { data?: { error?: string } } })?.response?.data?.error;
      setError(msg || 'Erro ao salvar usuário');
    } finally {
      setSaving(false);
    }
  };

  const handleAlterarSenha = async () => {
    if (!novaSenha.trim()) { setError('Nova senha é obrigatória'); return; }
    if (novaSenha !== confirmSenha) { setError('Senhas não conferem'); return; }
    if (!selectedUser) return;

    setSaving(true);
    setError('');
    try {
      await usuariosService.alterarSenha(selectedUser.id, novaSenha);
      fecharModal();
    } catch {
      setError('Erro ao alterar senha');
    } finally {
      setSaving(false);
    }
  };

  const handleExcluir = async (u: UsuarioDto) => {
    try {
      await usuariosService.excluir(u.id);
      await carregar();
      setConfirmDelete(null);
    } catch {
      setError('Erro ao desativar usuário');
    }
  };

  const getTipoLabel = (tipo: string) => TIPO_LABELS[tipo] ?? tipo;

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex flex-col">
      {/* Header */}
      <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-4 py-3 flex items-center gap-3">
        <button onClick={onClose} className="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700">
          <ArrowLeft className="w-5 h-5 text-gray-600 dark:text-gray-300" />
        </button>
        <h1 className="text-lg font-semibold text-gray-800 dark:text-white flex-1">Gestão de Usuários</h1>
        <button
          onClick={abrirCriar}
          className="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-3 py-2 rounded-lg text-sm font-medium"
        >
          <Plus className="w-4 h-4" />
          Novo usuário
        </button>
      </div>

      {/* Search */}
      <div className="px-4 py-3">
        <div className="relative">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" />
          <input
            type="text"
            placeholder="Buscar por nome..."
            value={busca}
            onChange={e => setBusca(e.target.value)}
            className="w-full pl-9 pr-3 py-2 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg text-sm text-gray-800 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Error */}
      {error && (
        <div className="mx-4 mb-2 p-3 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-700 rounded-lg text-red-700 dark:text-red-300 text-sm">
          {error}
        </div>
      )}

      {/* User list */}
      <div className="flex-1 px-4 pb-4 overflow-auto">
        {loading ? (
          <div className="flex justify-center py-12 text-gray-400">Carregando...</div>
        ) : usuariosFiltrados.length === 0 ? (
          <div className="flex flex-col items-center py-12 text-gray-400 gap-2">
            <UserX className="w-10 h-10" />
            <span>Nenhum usuário encontrado</span>
          </div>
        ) : (
          <div className="space-y-2">
            {usuariosFiltrados.map(u => (
              <div
                key={u.id}
                className="bg-white dark:bg-gray-800 rounded-xl border border-gray-200 dark:border-gray-700 p-4"
              >
                <div className="flex items-start justify-between gap-3">
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <span className="font-medium text-gray-800 dark:text-white truncate">{u.nome}</span>
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${u.ativo ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400' : 'bg-gray-100 text-gray-500 dark:bg-gray-700 dark:text-gray-400'}`}>
                        {u.ativo ? 'Ativo' : 'Inativo'}
                      </span>
                      {u.bloqueado && (
                        <span className="text-xs px-2 py-0.5 rounded-full bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400 font-medium">
                          Bloqueado
                        </span>
                      )}
                      <span className="text-xs px-2 py-0.5 rounded-full bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400 font-medium">
                        {getTipoLabel(u.tipo)}
                      </span>
                    </div>
                    <div className="mt-1 flex gap-3 flex-wrap">
                      <PermChip label="Visualizar" active={u.podeVisualizar} />
                      <PermChip label="Ver Total" active={u.podeVerTotal} />
                      <PermChip label="Cancelar" active={u.podeCancelar} />
                    </div>
                  </div>
                  <div className="flex gap-1 shrink-0">
                    <ActionBtn onClick={() => abrirSenha(u)} icon={<KeyRound className="w-4 h-4" />} title="Alterar senha" color="yellow" />
                    <ActionBtn onClick={() => abrirEditar(u)} icon={<Pencil className="w-4 h-4" />} title="Editar" color="blue" />
                    <ActionBtn onClick={() => setConfirmDelete(u)} icon={<Trash2 className="w-4 h-4" />} title="Desativar" color="red" />
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Modal: criar / editar */}
      {(modalMode === 'criar' || modalMode === 'editar') && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
          <div className="bg-white dark:bg-gray-800 rounded-2xl w-full max-w-md shadow-xl">
            <div className="p-4 border-b border-gray-200 dark:border-gray-700">
              <h2 className="text-base font-semibold text-gray-800 dark:text-white">
                {modalMode === 'criar' ? 'Novo Usuário' : 'Editar Usuário'}
              </h2>
            </div>
            <div className="p-4 space-y-3 max-h-[70vh] overflow-y-auto">
              <FormField label="Nome">
                <input
                  type="text"
                  value={form.nome}
                  onChange={e => setForm(f => ({ ...f, nome: e.target.value }))}
                  className="form-input"
                  placeholder="Nome do usuário"
                />
              </FormField>

              {modalMode === 'criar' && (
                <FormField label="Senha">
                  <input
                    type="password"
                    value={form.senha}
                    onChange={e => setForm(f => ({ ...f, senha: e.target.value }))}
                    className="form-input"
                    placeholder="Senha inicial"
                  />
                </FormField>
              )}

              <FormField label="Perfil">
                <select
                  value={form.tipo}
                  onChange={e => setForm(f => ({ ...f, tipo: e.target.value }))}
                  className="form-input"
                >
                  <option value="0">Garçom</option>
                  <option value="1">Caixa</option>
                  <option value="2">Gerente</option>
                </select>
              </FormField>

              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-700 dark:text-gray-300">Permissões</p>
                <ToggleRow label="Pode Visualizar" checked={form.podeVisualizar} onChange={v => setForm(f => ({ ...f, podeVisualizar: v }))} />
                <ToggleRow label="Pode Ver Total" checked={form.podeVerTotal} onChange={v => setForm(f => ({ ...f, podeVerTotal: v }))} />
                <ToggleRow label="Pode Cancelar" checked={form.podeCancelar} onChange={v => setForm(f => ({ ...f, podeCancelar: v }))} />
              </div>

              <div className="space-y-2">
                <p className="text-sm font-medium text-gray-700 dark:text-gray-300">Status</p>
                <ToggleRow label="Ativo" checked={form.ativo} onChange={v => setForm(f => ({ ...f, ativo: v }))} />
                <ToggleRow label="Bloqueado" checked={form.bloqueado} onChange={v => setForm(f => ({ ...f, bloqueado: v }))} />
              </div>

              {error && <p className="text-red-600 dark:text-red-400 text-sm">{error}</p>}
            </div>
            <div className="p-4 border-t border-gray-200 dark:border-gray-700 flex gap-2 justify-end">
              <button onClick={fecharModal} className="px-4 py-2 rounded-lg text-sm text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700">
                Cancelar
              </button>
              <button
                onClick={handleSalvar}
                disabled={saving}
                className="px-4 py-2 rounded-lg text-sm bg-blue-600 hover:bg-blue-700 text-white font-medium disabled:opacity-50"
              >
                {saving ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal: alterar senha */}
      {modalMode === 'senha' && selectedUser && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
          <div className="bg-white dark:bg-gray-800 rounded-2xl w-full max-w-sm shadow-xl">
            <div className="p-4 border-b border-gray-200 dark:border-gray-700">
              <h2 className="text-base font-semibold text-gray-800 dark:text-white">
                Alterar Senha — {selectedUser.nome}
              </h2>
            </div>
            <div className="p-4 space-y-3">
              <FormField label="Nova Senha">
                <input
                  type="password"
                  value={novaSenha}
                  onChange={e => setNovaSenha(e.target.value)}
                  className="form-input"
                  placeholder="Nova senha"
                />
              </FormField>
              <FormField label="Confirmar Senha">
                <input
                  type="password"
                  value={confirmSenha}
                  onChange={e => setConfirmSenha(e.target.value)}
                  className="form-input"
                  placeholder="Confirmar nova senha"
                />
              </FormField>
              {error && <p className="text-red-600 dark:text-red-400 text-sm">{error}</p>}
            </div>
            <div className="p-4 border-t border-gray-200 dark:border-gray-700 flex gap-2 justify-end">
              <button onClick={fecharModal} className="px-4 py-2 rounded-lg text-sm text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700">
                Cancelar
              </button>
              <button
                onClick={handleAlterarSenha}
                disabled={saving}
                className="px-4 py-2 rounded-lg text-sm bg-yellow-500 hover:bg-yellow-600 text-white font-medium disabled:opacity-50"
              >
                {saving ? 'Salvando...' : 'Alterar Senha'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Confirm delete */}
      {confirmDelete && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-center justify-center p-4">
          <div className="bg-white dark:bg-gray-800 rounded-2xl w-full max-w-sm shadow-xl p-5">
            <h2 className="text-base font-semibold text-gray-800 dark:text-white mb-2">Confirmar Desativação</h2>
            <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
              Desativar o usuário <strong>{confirmDelete.nome}</strong>? Ele não conseguirá mais fazer login.
            </p>
            <div className="flex gap-2 justify-end">
              <button onClick={() => setConfirmDelete(null)} className="px-4 py-2 rounded-lg text-sm text-gray-600 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700">
                Cancelar
              </button>
              <button
                onClick={() => handleExcluir(confirmDelete)}
                className="px-4 py-2 rounded-lg text-sm bg-red-600 hover:bg-red-700 text-white font-medium"
              >
                Desativar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

/* Sub-components */

function PermChip({ label, active }: { label: string; active: boolean }) {
  return (
    <span className={`flex items-center gap-1 text-xs ${active ? 'text-green-600 dark:text-green-400' : 'text-gray-400 dark:text-gray-500'}`}>
      {active ? <ShieldCheck className="w-3 h-3" /> : <ShieldOff className="w-3 h-3" />}
      {label}
    </span>
  );
}

function ActionBtn({ onClick, icon, title, color }: { onClick: () => void; icon: React.ReactNode; title: string; color: 'blue' | 'yellow' | 'red' }) {
  const cls = {
    blue: 'hover:bg-blue-50 dark:hover:bg-blue-900/20 text-blue-600 dark:text-blue-400',
    yellow: 'hover:bg-yellow-50 dark:hover:bg-yellow-900/20 text-yellow-600 dark:text-yellow-400',
    red: 'hover:bg-red-50 dark:hover:bg-red-900/20 text-red-600 dark:text-red-400',
  }[color];
  return (
    <button onClick={onClick} title={title} className={`p-1.5 rounded-lg ${cls}`}>
      {icon}
    </button>
  );
}

function FormField({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">{label}</label>
      {children}
    </div>
  );
}

function ToggleRow({ label, checked, onChange }: { label: string; checked: boolean; onChange: (v: boolean) => void }) {
  return (
    <label className="flex items-center justify-between py-1 cursor-pointer">
      <span className="text-sm text-gray-700 dark:text-gray-300">{label}</span>
      <div
        onClick={() => onChange(!checked)}
        className={`relative w-10 h-6 rounded-full transition-colors ${checked ? 'bg-blue-600' : 'bg-gray-300 dark:bg-gray-600'}`}
      >
        <div className={`absolute top-0.5 left-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform ${checked ? 'translate-x-4' : ''}`} />
      </div>
    </label>
  );
}

// Add global form-input style via className (should be in index.css, but using inline approach)
const style = document.createElement('style');
style.textContent = `.form-input { width: 100%; padding: 0.5rem 0.75rem; border: 1px solid #d1d5db; border-radius: 0.5rem; font-size: 0.875rem; background: white; color: #1f2937; outline: none; } .form-input:focus { ring: 2px solid #3b82f6; border-color: #3b82f6; } @media (prefers-color-scheme: dark) { .form-input { background: #374151; border-color: #4b5563; color: white; } }`;
if (!document.head.querySelector('#usuarios-form-style')) {
  style.id = 'usuarios-form-style';
  document.head.appendChild(style);
}
