import React, { useCallback, useEffect, useState } from 'react'
import {
  ArrowLeft, Plus, Search, RefreshCw, UserCheck, UserX,
  Pencil, Loader2, X, Check, TrendingUp, KeyRound
} from 'lucide-react'
import { forcaVendasService } from '../services/api'
import { Vendedor } from '../types/api'
import { Button } from '../components/ui/button'
import { useToast } from '../hooks/useToast'

interface CadastroVendedoresPageProps {
  onClose: () => void
}

interface FormVendedor {
  nome: string
  email: string
  celular: string
  comissaoPerc: number
  metaMensal: number
  regiao: string
  obs: string
  ativo: boolean
}

const formVazio = (): FormVendedor => ({
  nome: '',
  email: '',
  celular: '',
  comissaoPerc: 0,
  metaMensal: 0,
  regiao: '',
  obs: '',
  ativo: true,
})

const CadastroVendedoresPage: React.FC<CadastroVendedoresPageProps> = ({ onClose }) => {
  const [vendedores, setVendedores] = useState<Vendedor[]>([])
  const [isLoading, setIsLoading]   = useState(true)
  const [busca, setBusca]           = useState('')
  const [filtroAtivo, setFiltroAtivo] = useState<'todos' | 'ativos' | 'inativos'>('todos')

  const [showModal, setShowModal]   = useState(false)
  const [editando, setEditando]     = useState<Vendedor | null>(null)
  const [form, setForm]             = useState<FormVendedor>(formVazio())
  const [salvando, setSalvando]     = useState(false)

  const [showSenhaModal, setShowSenhaModal] = useState(false)
  const [vendedorSenha, setVendedorSenha]   = useState<Vendedor | null>(null)
  const [novaSenha, setNovaSenha]           = useState('')
  const [confirmSenha, setConfirmSenha]     = useState('')
  const [salvandoSenha, setSalvandoSenha]   = useState(false)

  const { showSuccess, showError } = useToast()

  const carregar = useCallback(async () => {
    setIsLoading(true)
    try {
      const params: any = {}
      if (busca.trim()) params.q = busca.trim()
      if (filtroAtivo === 'ativos')   params.ativo = true
      if (filtroAtivo === 'inativos') params.ativo = false
      const lista = await forcaVendasService.getVendedores(params)
      setVendedores(lista)
    } catch {
      showError('Erro ao carregar vendedores')
    } finally {
      setIsLoading(false)
    }
  }, [busca, filtroAtivo, showError])

  useEffect(() => { carregar() }, [carregar])

  const abrirNovo = () => {
    setEditando(null)
    setForm(formVazio())
    setShowModal(true)
  }

  const abrirEditar = (v: Vendedor) => {
    setEditando(v)
    setForm({
      nome:         v.nome,
      email:        v.email ?? '',
      celular:      v.celular ?? '',
      comissaoPerc: v.comissaoPerc,
      metaMensal:   v.metaMensal,
      regiao:       v.regiao ?? '',
      obs:          v.obs ?? '',
      ativo:        v.ativo,
    })
    setShowModal(true)
  }

  const fecharModal = () => {
    setShowModal(false)
    setEditando(null)
  }

  const abrirSenhaModal = (v: Vendedor) => {
    setVendedorSenha(v)
    setNovaSenha('')
    setConfirmSenha('')
    setShowSenhaModal(true)
  }

  const fecharSenhaModal = () => {
    setShowSenhaModal(false)
    setVendedorSenha(null)
  }

  const handleSalvarSenha = async () => {
    if (!novaSenha.trim()) { showError('Informe a nova senha'); return }
    if (novaSenha.length < 4) { showError('Senha deve ter no mínimo 4 caracteres'); return }
    if (novaSenha !== confirmSenha) { showError('As senhas não conferem'); return }
    setSalvandoSenha(true)
    try {
      await forcaVendasService.alterarSenhaVendedor(vendedorSenha!.id, novaSenha)
      showSuccess('Senha alterada com sucesso!')
      fecharSenhaModal()
    } catch {
      showError('Erro ao alterar senha')
    } finally {
      setSalvandoSenha(false)
    }
  }

  const handleSalvar = async () => {
    if (!form.nome.trim()) {
      showError('Nome é obrigatório')
      return
    }
    setSalvando(true)
    try {
      const payload = {
        nome:         form.nome.trim(),
        email:        form.email.trim() || null,
        celular:      form.celular.trim() || null,
        comissaoPerc: form.comissaoPerc,
        metaMensal:   form.metaMensal,
        regiao:       form.regiao.trim() || null,
        obs:          form.obs.trim() || null,
        ativo:        form.ativo,
      }
      if (editando) {
        await forcaVendasService.atualizarVendedor(editando.id, payload)
        showSuccess('Vendedor atualizado com sucesso!')
      } else {
        await forcaVendasService.criarVendedor(payload)
        showSuccess('Vendedor criado com sucesso!')
      }
      fecharModal()
      carregar()
    } catch {
      showError('Erro ao salvar vendedor')
    } finally {
      setSalvando(false)
    }
  }

  const handleAlterarStatus = async (v: Vendedor) => {
    const acao = v.ativo ? 'desativar' : 'ativar'
    if (!window.confirm(`Deseja ${acao} o vendedor "${v.nome}"?`)) return
    try {
      await forcaVendasService.alterarStatusVendedor(v.id, !v.ativo)
      showSuccess(`Vendedor ${v.ativo ? 'desativado' : 'ativado'} com sucesso!`)
      carregar()
    } catch {
      showError('Erro ao alterar status do vendedor')
    }
  }

  const fmtMoeda = (v: number) =>
    new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header */}
      <div className="bg-white border-b px-4 py-3 flex items-center gap-3 shadow-sm">
        <button onClick={onClose} className="p-1.5 hover:bg-gray-100 rounded-lg">
          <ArrowLeft className="w-5 h-5 text-gray-600" />
        </button>
        <div className="flex-1">
          <h1 className="font-bold text-gray-900 text-lg">Cadastro de Vendedores</h1>
          <p className="text-xs text-gray-500">{vendedores.length} registro(s)</p>
        </div>
        <button onClick={carregar} className="p-1.5 hover:bg-gray-100 rounded-lg" title="Recarregar">
          <RefreshCw className="w-4 h-4 text-gray-500" />
        </button>
        <Button onClick={abrirNovo} className="flex items-center gap-1.5 text-sm">
          <Plus className="w-4 h-4" />
          Novo
        </Button>
      </div>

      {/* Filtros */}
      <div className="px-4 pt-4 pb-2 space-y-2">
        <div className="relative">
          <Search className="absolute left-3 top-2.5 w-4 h-4 text-gray-400" />
          <input
            type="text"
            value={busca}
            onChange={e => setBusca(e.target.value)}
            placeholder="Buscar por nome ou e-mail..."
            className="w-full text-sm border border-gray-300 rounded-lg pl-9 pr-3 py-2 bg-white"
          />
        </div>
        <div className="flex gap-2">
          {(['todos', 'ativos', 'inativos'] as const).map(f => (
            <button
              key={f}
              onClick={() => setFiltroAtivo(f)}
              className={`flex-1 text-xs py-1.5 rounded-lg font-medium border transition-colors ${
                filtroAtivo === f
                  ? 'bg-indigo-600 text-white border-indigo-600'
                  : 'bg-white text-gray-600 border-gray-300 hover:bg-gray-50'
              }`}
            >
              {f.charAt(0).toUpperCase() + f.slice(1)}
            </button>
          ))}
        </div>
      </div>

      {/* Lista */}
      <div className="flex-1 px-4 pb-6 space-y-3 overflow-y-auto">
        {isLoading ? (
          <div className="flex justify-center py-12">
            <RefreshCw className="w-8 h-8 text-gray-400 animate-spin" />
          </div>
        ) : vendedores.length === 0 ? (
          <div className="text-center py-12 text-gray-400">
            <Search className="w-10 h-10 mx-auto mb-2 opacity-40" />
            <p className="text-sm">Nenhum vendedor encontrado</p>
            <button onClick={abrirNovo} className="mt-3 text-sm text-indigo-600 underline">
              Cadastrar primeiro vendedor
            </button>
          </div>
        ) : (
          vendedores.map(v => (
            <div
              key={v.id}
              className={`bg-white rounded-xl border shadow-sm overflow-hidden ${
                v.ativo ? 'border-gray-200' : 'border-gray-100 opacity-60'
              }`}
            >
              <div className="px-4 pt-3 pb-2 flex items-start justify-between gap-2">
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <p className="font-semibold text-gray-900">{v.nome}</p>
                    {!v.ativo && (
                      <span className="text-xs bg-red-100 text-red-700 px-2 py-0.5 rounded-full">Inativo</span>
                    )}
                  </div>
                  {v.email && <p className="text-xs text-gray-500">{v.email}</p>}
                  {v.celular && <p className="text-xs text-gray-500">{v.celular}</p>}
                  {v.regiao && <p className="text-xs text-gray-400">Região: {v.regiao}</p>}
                </div>
                <div className="text-right shrink-0">
                  {v.comissaoPerc > 0 && (
                    <p className="text-xs text-indigo-700 font-medium">{v.comissaoPerc}% comissão</p>
                  )}
                  {v.metaMensal > 0 && (
                    <div className="flex items-center justify-end gap-1">
                      <TrendingUp className="w-3 h-3 text-green-600" />
                      <p className="text-xs text-green-700">Meta: {fmtMoeda(v.metaMensal)}</p>
                    </div>
                  )}
                </div>
              </div>
              <div className="px-4 pb-3 pt-1 flex gap-2 border-t border-gray-50">
                <button
                  onClick={() => abrirEditar(v)}
                  className="flex items-center gap-1 text-xs text-indigo-600 border border-indigo-200 px-3 py-1.5 rounded-lg hover:bg-indigo-50"
                >
                  <Pencil className="w-3 h-3" />
                  Editar
                </button>
                <button
                  onClick={() => handleAlterarStatus(v)}
                  className={`flex items-center gap-1 text-xs border px-3 py-1.5 rounded-lg ${
                    v.ativo
                      ? 'text-red-600 border-red-200 hover:bg-red-50'
                      : 'text-green-600 border-green-200 hover:bg-green-50'
                  }`}
                >
                  {v.ativo ? <><UserX className="w-3 h-3" /> Desativar</> : <><UserCheck className="w-3 h-3" /> Ativar</>}
                </button>
                <button
                  onClick={() => abrirSenhaModal(v)}
                  className="flex items-center gap-1 text-xs text-gray-600 border border-gray-200 px-3 py-1.5 rounded-lg hover:bg-gray-50"
                >
                  <KeyRound className="w-3 h-3" />
                  Senha
                </button>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Modal cadastro/edição */}
      {showModal && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-end sm:items-center justify-center p-0 sm:p-4">
          <div className="bg-white w-full sm:max-w-md rounded-t-2xl sm:rounded-2xl shadow-2xl max-h-[90vh] flex flex-col">
            {/* Header modal */}
            <div className="px-4 py-3 border-b flex items-center justify-between">
              <h2 className="font-bold text-gray-900">
                {editando ? 'Editar Vendedor' : 'Novo Vendedor'}
              </h2>
              <button onClick={fecharModal} className="p-1.5 hover:bg-gray-100 rounded-lg">
                <X className="w-5 h-5 text-gray-500" />
              </button>
            </div>

            {/* Corpo modal */}
            <div className="flex-1 overflow-y-auto p-4 space-y-4">
              {/* Nome */}
              <div>
                <label className="text-sm font-medium text-gray-700 block mb-1">Nome *</label>
                <input
                  type="text"
                  maxLength={40}
                  value={form.nome}
                  onChange={e => setForm(f => ({ ...f, nome: e.target.value }))}
                  placeholder="Nome do vendedor"
                  className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                />
              </div>

              {/* E-mail e Celular */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-sm font-medium text-gray-700 block mb-1">E-mail</label>
                  <input
                    type="email"
                    value={form.email}
                    onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
                    placeholder="email@exemplo.com"
                    className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700 block mb-1">Celular</label>
                  <input
                    type="tel"
                    value={form.celular}
                    onChange={e => setForm(f => ({ ...f, celular: e.target.value }))}
                    placeholder="(00) 00000-0000"
                    className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                  />
                </div>
              </div>

              {/* Comissão e Meta */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-sm font-medium text-gray-700 block mb-1">Comissão (%)</label>
                  <input
                    type="number"
                    min={0}
                    max={100}
                    step={0.1}
                    value={form.comissaoPerc}
                    onChange={e => setForm(f => ({ ...f, comissaoPerc: parseFloat(e.target.value) || 0 }))}
                    className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                  />
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-700 block mb-1">Meta mensal (R$)</label>
                  <input
                    type="number"
                    min={0}
                    step={100}
                    value={form.metaMensal}
                    onChange={e => setForm(f => ({ ...f, metaMensal: parseFloat(e.target.value) || 0 }))}
                    className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                  />
                </div>
              </div>

              {/* Região */}
              <div>
                <label className="text-sm font-medium text-gray-700 block mb-1">Região / Território</label>
                <input
                  type="text"
                  maxLength={80}
                  value={form.regiao}
                  onChange={e => setForm(f => ({ ...f, regiao: e.target.value }))}
                  placeholder="Ex: Sul, Centro-Oeste..."
                  className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                />
              </div>

              {/* Obs */}
              <div>
                <label className="text-sm font-medium text-gray-700 block mb-1">Observações</label>
                <textarea
                  rows={2}
                  maxLength={500}
                  value={form.obs}
                  onChange={e => setForm(f => ({ ...f, obs: e.target.value }))}
                  placeholder="Observações gerais..."
                  className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2 resize-none"
                />
              </div>

              {/* Ativo */}
              {editando && (
                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={() => setForm(f => ({ ...f, ativo: !f.ativo }))}
                    className={`relative w-12 h-6 rounded-full transition-colors duration-200 ${
                      form.ativo ? 'bg-green-500' : 'bg-gray-300'
                    }`}
                  >
                    <span className={`absolute top-0.5 left-0.5 w-5 h-5 bg-white rounded-full shadow transition-transform duration-200 ${
                      form.ativo ? 'translate-x-6' : 'translate-x-0'
                    }`} />
                  </button>
                  <span className="text-sm text-gray-700">
                    {form.ativo ? 'Vendedor ativo' : 'Vendedor inativo'}
                  </span>
                </div>
              )}
            </div>

            {/* Rodapé modal */}
            <div className="px-4 pb-5 pt-3 border-t flex gap-3">
              <button
                onClick={fecharModal}
                disabled={salvando}
                className="flex-1 py-2.5 text-sm text-gray-600 border border-gray-300 rounded-xl hover:bg-gray-50"
              >
                Cancelar
              </button>
              <Button
                onClick={handleSalvar}
                disabled={salvando}
                className="flex-1 py-2.5 text-sm flex items-center justify-center gap-2"
              >
                {salvando ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : (
                  <Check className="w-4 h-4" />
                )}
                {salvando ? 'Salvando...' : 'Salvar'}
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de senha */}
      {showSenhaModal && vendedorSenha && (
        <div className="fixed inset-0 z-50 bg-black/50 flex items-end sm:items-center justify-center p-0 sm:p-4">
          <div className="bg-white w-full sm:max-w-sm rounded-t-2xl sm:rounded-2xl shadow-2xl">
            <div className="px-4 py-3 border-b flex items-center justify-between">
              <div>
                <h2 className="font-bold text-gray-900">Definir Senha</h2>
                <p className="text-xs text-gray-500">{vendedorSenha.nome}</p>
              </div>
              <button onClick={fecharSenhaModal} className="p-1.5 hover:bg-gray-100 rounded-lg">
                <X className="w-5 h-5 text-gray-500" />
              </button>
            </div>
            <div className="p-4 space-y-3">
              <div>
                <label className="text-sm font-medium text-gray-700 block mb-1">Nova senha</label>
                <input
                  type="password"
                  value={novaSenha}
                  onChange={e => setNovaSenha(e.target.value)}
                  placeholder="Mínimo 4 caracteres"
                  className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                />
              </div>
              <div>
                <label className="text-sm font-medium text-gray-700 block mb-1">Confirmar senha</label>
                <input
                  type="password"
                  value={confirmSenha}
                  onChange={e => setConfirmSenha(e.target.value)}
                  placeholder="Repita a senha"
                  className="w-full text-sm border border-gray-300 rounded-lg px-3 py-2"
                />
              </div>
            </div>
            <div className="px-4 pb-5 pt-2 border-t flex gap-3">
              <button
                onClick={fecharSenhaModal}
                disabled={salvandoSenha}
                className="flex-1 py-2.5 text-sm text-gray-600 border border-gray-300 rounded-xl hover:bg-gray-50"
              >
                Cancelar
              </button>
              <Button
                onClick={handleSalvarSenha}
                disabled={salvandoSenha}
                className="flex-1 py-2.5 text-sm flex items-center justify-center gap-2"
              >
                {salvandoSenha ? <Loader2 className="w-4 h-4 animate-spin" /> : <KeyRound className="w-4 h-4" />}
                {salvandoSenha ? 'Salvando...' : 'Confirmar'}
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default CadastroVendedoresPage
