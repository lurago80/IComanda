import { ArrowLeft, Bike, Briefcase, Building2, ChefHat, CheckCircle2, ClipboardList, Database, Download, Eye, EyeOff, Mail, Printer, QrCode, Save, Settings } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { Button } from '../components/ui/button'
import { configuracoesService, emitenteService, Emitente } from '../services/api'

const Toggle = ({
  id, value, onChange, color = 'bg-orange-500',
}: { id: string; value: boolean; onChange: () => void; color?: string }) => (
  <div
    id={id}
    role="switch"
    aria-checked={value}
    tabIndex={0}
    onClick={onChange}
    onKeyDown={(e) => { if (e.key === ' ' || e.key === 'Enter') onChange() }}
    className={`relative inline-flex h-6 w-11 items-center rounded-full transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-primary/50 focus:ring-offset-2 cursor-pointer ${value ? color : 'bg-gray-300'}`}
  >
    <span className={`inline-block h-4 w-4 transform rounded-full bg-white shadow-md transition-transform duration-200 ${value ? 'translate-x-6' : 'translate-x-1'}`} />
  </div>
)

interface ConfiguracoesPageProps {
  onClose: () => void
  onSalvar?: (usarDelivery: boolean, usarForcaVendas: boolean, usarComanda: boolean, habilitarImprimirDuasVias: boolean, usarCozinha: boolean, usarCardapio: boolean) => void
}

const ConfiguracoesPage: React.FC<ConfiguracoesPageProps> = ({ onClose, onSalvar }) => {
  const [usarDelivery, setUsarDelivery]                         = useState(true)
  const [usarForcaVendas, setUsarForcaVendas]                   = useState(true)
  const [usarComanda, setUsarComanda]                           = useState(true)
  const [habilitarImprimirDuasVias, setHabilitarImprimirDuasVias] = useState(false)
  const [usarCozinha, setUsarCozinha]                           = useState(true)
  const [usarCardapio, setUsarCardapio]                         = useState(true)
  const [carregando, setCarregando]             = useState(true)
  const [salvando, setSalvando]                 = useState(false)
  const [mensagem, setMensagem] = useState<{ tipo: 'sucesso' | 'erro'; texto: string } | null>(null)
  const [fazendoBackup, setFazendoBackup]       = useState(false)
  const [mensagemBackup, setMensagemBackup]     = useState<{ tipo: 'sucesso' | 'erro'; texto: string; detalhe?: string; emailInfo?: string } | null>(null)

  // Email backup config
  const [emailCfg, setEmailCfg] = useState({
    smtpHost: '', smtpPort: 587, smtpUseSsl: true,
    usuario: '', senha: '', remetente: '', nomeRemetente: 'IComanda Backup', destinatario: ''
  })
  const [senhaCadastrada, setSenhaCadastrada] = useState(false)
  const [mostrarSenha, setMostrarSenha] = useState(false)
  const [salvandoEmail, setSalvandoEmail] = useState(false)
  const [mensagemEmail, setMensagemEmail] = useState<{ tipo: 'sucesso' | 'erro'; texto: string } | null>(null)

  // Emitente
  const [emitente, setEmitente] = useState<Partial<Emitente>>({})
  const [salvandoEmitente, setSalvandoEmitente] = useState(false)
  const [mensagemEmitente, setMensagemEmitente] = useState<{ tipo: 'sucesso' | 'erro'; texto: string } | null>(null)

  useEffect(() => {
    const carregar = async () => {
      try {
        const [cfg, emit, emailBkp] = await Promise.all([
          configuracoesService.getSistema(),
          emitenteService.getEmitente(),
          configuracoesService.getBackupEmail().catch(() => null),
        ])
        setUsarDelivery(cfg.usarDelivery)
        setUsarForcaVendas(cfg.usarForcaVendas ?? true)
        setUsarComanda(cfg.usarComanda ?? true)
        setHabilitarImprimirDuasVias(cfg.habilitarImprimirDuasVias ?? false)
        setUsarCozinha(cfg.usarCozinha ?? true)
        setUsarCardapio(cfg.usarCardapio ?? true)
        if (emit) setEmitente(emit)
        if (emailBkp) {
          setEmailCfg({
            smtpHost: emailBkp.smtpHost, smtpPort: emailBkp.smtpPort, smtpUseSsl: emailBkp.smtpUseSsl,
            usuario: emailBkp.usuario, senha: '', remetente: emailBkp.remetente,
            nomeRemetente: emailBkp.nomeRemetente, destinatario: emailBkp.destinatario
          })
          setSenhaCadastrada(emailBkp.senhaCadastrada)
        }
      } catch {
        // silencioso — mantém padrões
      } finally {
        setCarregando(false)
      }
    }
    carregar()
  }, [])

  const handleSalvarEmitente = async () => {
    setSalvandoEmitente(true)
    setMensagemEmitente(null)
    try {
      await emitenteService.saveEmitente(emitente)
      setMensagemEmitente({ tipo: 'sucesso', texto: 'Dados do emitente salvos com sucesso!' })
    } catch {
      setMensagemEmitente({ tipo: 'erro', texto: 'Erro ao salvar dados do emitente.' })
    } finally {
      setSalvandoEmitente(false)
    }
  }

  const handleSalvarEmailCfg = async () => {
    setSalvandoEmail(true)
    setMensagemEmail(null)
    try {
      await configuracoesService.putBackupEmail({
        smtpHost: emailCfg.smtpHost,
        smtpPort: emailCfg.smtpPort,
        smtpUseSsl: emailCfg.smtpUseSsl,
        usuario: emailCfg.usuario,
        senha: emailCfg.senha || undefined,
        remetente: emailCfg.remetente,
        nomeRemetente: emailCfg.nomeRemetente,
        destinatario: emailCfg.destinatario,
      })
      if (emailCfg.senha) setSenhaCadastrada(true)
      setEmailCfg(p => ({ ...p, senha: '' }))
      setMensagemEmail({ tipo: 'sucesso', texto: 'Configurações de email salvas!' })
    } catch {
      setMensagemEmail({ tipo: 'erro', texto: 'Erro ao salvar configurações de email.' })
    } finally {
      setSalvandoEmail(false)
    }
  }

  const handleBackup = async () => {
    setFazendoBackup(true)
    setMensagemBackup(null)
    try {
      const resultado = await configuracoesService.fazerBackup()
      let emailInfo: string | undefined
      if (resultado.emailEnviado && resultado.emailDestino)
        emailInfo = `✉ Email enviado para ${resultado.emailDestino}`
      else if (resultado.emailErro)
        emailInfo = `⚠ Falha ao enviar email: ${resultado.emailErro}`
      setMensagemBackup({
        tipo: 'sucesso',
        texto: resultado.mensagem,
        detalhe: `${resultado.arquivo} (${resultado.tamanhoMb} MB) → ${resultado.caminho}`,
        emailInfo,
      })
    } catch (err: any) {
      const msg = err?.response?.data?.error || err?.message || 'Erro desconhecido ao fazer backup.'
      setMensagemBackup({ tipo: 'erro', texto: msg })
    } finally {
      setFazendoBackup(false)
    }
  }

  const handleSalvar = async () => {
    setSalvando(true)
    setMensagem(null)
    try {
      await configuracoesService.putSistema({ usarDelivery, usarForcaVendas, usarComanda, habilitarImprimirDuasVias, usarCozinha, usarCardapio })
      setMensagem({ tipo: 'sucesso', texto: 'Configurações salvas! Voltando ao menu...' })
      // Após 1,5s: atualiza estado no App e fecha — chamadas sincronas no mesmo setTimeout
      // garante que React processa tudo num único batch, evitando conflito de reconciliação
      setTimeout(() => {
        onSalvar?.(usarDelivery, usarForcaVendas, usarComanda, habilitarImprimirDuasVias, usarCozinha, usarCardapio)
        onClose()
      }, 1500)
    } catch {
      setMensagem({ tipo: 'erro', texto: 'Erro ao salvar configurações. Tente novamente.' })
    } finally {
      setSalvando(false)
    }
  }

  const modules = [
    {
      id: 'toggle-comanda',
      label: 'Módulo Comanda',
      desc: 'Exibe os botões de Comanda no menu principal (nova comanda, comandas abertas)',
      icon: <ClipboardList className="w-5 h-5 text-blue-600" />,
      bg: 'bg-blue-100',
      color: 'bg-blue-500',
      value: usarComanda,
      onChange: () => setUsarComanda(p => !p),
    },
    {
      id: 'toggle-delivery',
      label: 'Módulo Delivery',
      desc: 'Exibe os botões de Delivery no menu principal (novo pedido e em aberto)',
      icon: <Bike className="w-5 h-5 text-orange-600" />,
      bg: 'bg-orange-100',
      color: 'bg-orange-500',
      value: usarDelivery,
      onChange: () => setUsarDelivery(p => !p),
    },
    {
      id: 'toggle-forca-vendas',
      label: 'Módulo Força de Vendas',
      desc: 'Exibe os botões e navegação da Força de Vendas no sistema',
      icon: <Briefcase className="w-5 h-5 text-indigo-600" />,
      bg: 'bg-indigo-100',
      color: 'bg-indigo-500',
      value: usarForcaVendas,
      onChange: () => setUsarForcaVendas(p => !p),
    },
    {
      id: 'toggle-imprimir-2vias',
      label: 'Imprimir 2 Vias',
      desc: 'Grupos com "Imprimir 2 vias" marcado imprimem automaticamente 2 cópias, sem confirmação',
      icon: <Printer className="w-5 h-5 text-green-600" />,
      bg: 'bg-green-100',
      color: 'bg-green-500',
      value: habilitarImprimirDuasVias,
      onChange: () => setHabilitarImprimirDuasVias(p => !p),
    },
    {
      id: 'toggle-cozinha',
      label: 'Módulo Cozinha (KDS)',
      desc: 'Exibe os botões de Cozinha (KDS e Gerenciar Pizzas) no menu principal',
      icon: <ChefHat className="w-5 h-5 text-orange-700" />,
      bg: 'bg-orange-100',
      color: 'bg-orange-700',
      value: usarCozinha,
      onChange: () => setUsarCozinha(p => !p),
    },
    {
      id: 'toggle-cardapio',
      label: 'Módulo Cardápio (QR Code)',
      desc: 'Exibe o botão de QR Code das Mesas (cardápio digital) no menu principal',
      icon: <QrCode className="w-5 h-5 text-indigo-600" />,
      bg: 'bg-indigo-100',
      color: 'bg-indigo-500',
      value: usarCardapio,
      onChange: () => setUsarCardapio(p => !p),
    },
  ]

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <div className="bg-card border-b border-border px-4 py-3 flex items-center space-x-3 sticky top-0 z-10 shadow-sm">
        <Button variant="ghost" size="icon" onClick={onClose} className="h-9 w-9 rounded-full">
          <ArrowLeft className="w-5 h-5" />
        </Button>
        <Settings className="w-5 h-5 text-primary" />
        <h1 className="text-lg font-bold text-text-primary">Configurações do Sistema</h1>
      </div>

      {/* Conteúdo */}
      <div className="max-w-2xl mx-auto p-4 sm:p-6 space-y-6">
        {carregando ? (
          <div className="flex items-center justify-center py-16">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
          </div>
        ) : (
          <div className="space-y-6">
            {/* Seção: Módulos */}
            <div className="bg-card rounded-2xl border border-border shadow-sm overflow-hidden">
              <div className="px-5 py-4 bg-primary/5 border-b border-border">
                <h2 className="text-sm font-semibold text-primary uppercase tracking-wide">Módulos</h2>
                <p className="text-xs text-text-muted mt-0.5">Habilite ou desabilite funcionalidades do sistema</p>
              </div>

              <div className="divide-y divide-border">
                {modules.map(mod => (
                  <div key={mod.id} className="px-5 py-5">
                    <label htmlFor={mod.id} className="flex items-start justify-between gap-4 cursor-pointer">
                      <div className="flex items-center gap-3">
                        <div className={`w-10 h-10 rounded-xl ${mod.bg} flex items-center justify-center flex-shrink-0`}>
                          {mod.icon}
                        </div>
                        <div>
                          <p className="font-semibold text-text-primary text-sm sm:text-base">{mod.label}</p>
                          <p className="text-xs sm:text-sm text-text-muted mt-0.5">{mod.desc}</p>
                        </div>
                      </div>
                      <div className="flex-shrink-0 mt-0.5">
                        <Toggle id={mod.id} value={mod.value} onChange={mod.onChange} color={mod.color} />
                      </div>
                    </label>
                    <div className="mt-3 ml-13">
                      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${mod.value ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                        {mod.value ? 'Habilitado' : 'Desabilitado'}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* Seção: Emitente */}
            <div className="bg-card rounded-2xl border border-border shadow-sm overflow-hidden">
              <div className="px-5 py-4 bg-blue-500/10 border-b border-border">
                <h2 className="text-sm font-semibold text-blue-700 uppercase tracking-wide">Dados do Emitente</h2>
                <p className="text-xs text-text-muted mt-0.5">Razão social, CNPJ e contato da empresa exibidos nos recibos</p>
              </div>
              <div className="px-5 py-5 space-y-4">
                <div className="flex items-center gap-3 mb-2">
                  <div className="w-10 h-10 rounded-xl bg-blue-100 flex items-center justify-center flex-shrink-0">
                    <Building2 className="w-5 h-5 text-blue-600" />
                  </div>
                  <p className="text-sm text-text-muted">Preencha os dados da empresa. Caso não haja emitente cadastrado, um novo será criado.</p>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="sm:col-span-2">
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Razão Social</label>
                    <input
                      type="text"
                      value={emitente.nome ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, nome: e.target.value }))}
                      placeholder="Nome / Razão Social"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div className="sm:col-span-2">
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Nome Fantasia</label>
                    <input
                      type="text"
                      value={emitente.nomeFantasia ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, nomeFantasia: e.target.value }))}
                      placeholder="Nome Fantasia"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">CNPJ / CPF</label>
                    <input
                      type="text"
                      value={emitente.cnpj ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, cnpj: e.target.value }))}
                      placeholder="00.000.000/0001-00"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Inscrição Estadual</label>
                    <input
                      type="text"
                      value={emitente.inscricaoEstadual ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, inscricaoEstadual: e.target.value }))}
                      placeholder="Inscrição Estadual"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div className="sm:col-span-2">
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Endereço</label>
                    <input
                      type="text"
                      value={emitente.endereco ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, endereco: e.target.value }))}
                      placeholder="Rua / Avenida"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Número</label>
                    <input
                      type="text"
                      value={emitente.numero ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, numero: e.target.value }))}
                      placeholder="Número"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Complemento</label>
                    <input
                      type="text"
                      value={emitente.complemento ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, complemento: e.target.value }))}
                      placeholder="Sala, Bloco..."
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Bairro</label>
                    <input
                      type="text"
                      value={emitente.bairro ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, bairro: e.target.value }))}
                      placeholder="Bairro"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Cidade</label>
                    <input
                      type="text"
                      value={emitente.cidade ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, cidade: e.target.value }))}
                      placeholder="Cidade"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div className="grid grid-cols-2 gap-3">
                    <div>
                      <label className="block text-xs font-semibold text-text-secondary mb-1">UF</label>
                      <input
                        type="text"
                        value={emitente.uf ?? ''}
                        onChange={e => setEmitente(p => ({ ...p, uf: e.target.value.toUpperCase().slice(0, 2) }))}
                        placeholder="SP"
                        maxLength={2}
                        className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold text-text-secondary mb-1">CEP</label>
                      <input
                        type="text"
                        value={emitente.cep ?? ''}
                        onChange={e => setEmitente(p => ({ ...p, cep: e.target.value }))}
                        placeholder="00000-000"
                        className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Telefone</label>
                    <input
                      type="text"
                      value={emitente.telefone ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, telefone: e.target.value }))}
                      placeholder="(00) 00000-0000"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">E-mail</label>
                    <input
                      type="email"
                      value={emitente.email ?? ''}
                      onChange={e => setEmitente(p => ({ ...p, email: e.target.value }))}
                      placeholder="contato@empresa.com.br"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                </div>

                {/* mensagemEmitente — sempre no DOM, visibilidade via CSS (evita erro insertBefore do React) */}
                <div className={`px-4 py-3 rounded-xl text-sm font-medium flex items-center gap-2 ${
                  mensagemEmitente === null
                    ? 'hidden'
                    : mensagemEmitente.tipo === 'sucesso'
                      ? 'bg-green-50 border border-green-200 text-green-800'
                      : 'bg-red-50 border border-red-200 text-red-700'
                }`}>
                  <CheckCircle2 className={`w-4 h-4 flex-shrink-0 ${mensagemEmitente?.tipo === 'sucesso' ? '' : 'hidden'}`} />
                  {mensagemEmitente?.texto ?? ''}
                </div>

                <Button
                  onClick={handleSalvarEmitente}
                  disabled={salvandoEmitente}
                  className="mt-1 bg-blue-600 hover:bg-blue-700 text-white h-10 px-5 text-sm font-semibold"
                >
                  {salvandoEmitente ? (
                    <span className="flex items-center gap-2">
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white" />
                      Salvando...
                    </span>
                  ) : (
                    <span className="flex items-center gap-2">
                      <Save className="w-4 h-4" />
                      Salvar Emitente
                    </span>
                  )}
                </Button>
              </div>
            </div>

            {/* Seção: Backup */}
            <div className="bg-card rounded-2xl border border-border shadow-sm overflow-hidden">
              <div className="px-5 py-4 bg-amber-500/10 border-b border-border">
                <h2 className="text-sm font-semibold text-amber-700 uppercase tracking-wide" translate="no">Backup do Banco de Dados</h2>
                <p className="text-xs text-text-muted mt-0.5">Copia o banco de dados para <span className="font-mono text-xs bg-background-secondary px-1 rounded">C:\IComanda\Backup</span> e compacta em ZIP</p>
              </div>
              <div className="px-5 py-5">
                <div className="flex items-start gap-4">
                  <div className="w-10 h-10 rounded-xl bg-amber-100 flex items-center justify-center flex-shrink-0">
                    <Database className="w-5 h-5 text-amber-600" />
                  </div>
                  <div className="flex-1">
                    <p className="font-semibold text-text-primary text-sm" translate="no">Backup Manual</p>
                    <p className="text-xs text-text-muted mt-0.5">Gera uma cópia do arquivo <span className="font-mono">DADOSG5.FDB</span> com data e hora no nome. Recomendado antes de atualizações ou no fim do dia.</p>

                    {/* mensagemBackup — sempre no DOM, visibilidade via CSS */}
                    <div className={`mt-3 px-4 py-3 rounded-xl text-sm ${
                      mensagemBackup === null
                        ? 'hidden'
                        : mensagemBackup.tipo === 'sucesso'
                          ? 'bg-green-50 border border-green-200 text-green-800'
                          : 'bg-red-50 border border-red-200 text-red-700'
                    }`}>
                      <div className="flex items-center gap-2 font-medium">
                        <CheckCircle2 className={`w-4 h-4 ${mensagemBackup?.tipo === 'sucesso' ? '' : 'hidden'}`} />
                        {mensagemBackup?.texto ?? ''}
                      </div>
                      <p className={`text-xs mt-1 opacity-80 font-mono break-all ${mensagemBackup?.detalhe ? '' : 'hidden'}`}>
                        {mensagemBackup?.detalhe ?? ''}
                      </p>
                      <p className={`text-xs mt-1 font-medium ${mensagemBackup?.emailInfo ? '' : 'hidden'}`}>
                        {mensagemBackup?.emailInfo ?? ''}
                      </p>
                    </div>

                    <Button
                      onClick={handleBackup}
                      disabled={fazendoBackup}
                      className="mt-4 bg-amber-500 hover:bg-amber-600 text-white h-10 px-5 text-sm font-semibold"
                    >
                      {fazendoBackup ? (
                        <span className="flex items-center gap-2">
                          <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white" />
                          <span translate="no">Fazendo backup...</span>
                        </span>
                      ) : (
                        <span className="flex items-center gap-2">
                          <Download className="w-4 h-4" />
                          <span translate="no">Fazer Backup Agora</span>
                        </span>
                      )}
                    </Button>
                  </div>
                </div>
              </div>
            </div>

            {/* Seção: Email para Backup */}
            <div className="bg-card rounded-2xl border border-border shadow-sm overflow-hidden">
              <div className="px-5 py-4 bg-sky-500/10 border-b border-border">
                <h2 className="text-sm font-semibold text-sky-700 uppercase tracking-wide">Email para Backup</h2>
                <p className="text-xs text-text-muted mt-0.5">Configure o SMTP para envio automático do backup por email após cada execução</p>
              </div>
              <div className="px-5 py-5 space-y-4">
                <div className="flex items-center gap-3 mb-2">
                  <div className="w-10 h-10 rounded-xl bg-sky-100 flex items-center justify-center flex-shrink-0">
                    <Mail className="w-5 h-5 text-sky-600" />
                  </div>
                  <p className="text-sm text-text-muted">Preencha as configurações SMTP. Deixe em branco para desativar o envio por email.</p>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <div className="sm:col-span-2">
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Servidor SMTP</label>
                    <input
                      type="text"
                      value={emailCfg.smtpHost}
                      onChange={e => setEmailCfg(p => ({ ...p, smtpHost: e.target.value }))}
                      placeholder="smtp.gmail.com"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Porta</label>
                    <input
                      type="number"
                      value={emailCfg.smtpPort}
                      onChange={e => setEmailCfg(p => ({ ...p, smtpPort: Number(e.target.value) }))}
                      placeholder="587"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div className="flex items-center gap-3 pt-5">
                    <Toggle
                      id="toggle-smtp-ssl"
                      value={emailCfg.smtpUseSsl}
                      onChange={() => setEmailCfg(p => ({ ...p, smtpUseSsl: !p.smtpUseSsl }))}
                      color="bg-sky-500"
                    />
                    <label htmlFor="toggle-smtp-ssl" className="text-sm text-text-primary cursor-pointer">Usar SSL/TLS</label>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Usuário SMTP</label>
                    <input
                      type="text"
                      value={emailCfg.usuario}
                      onChange={e => setEmailCfg(p => ({ ...p, usuario: e.target.value }))}
                      placeholder="seu@email.com"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">
                      Senha SMTP {senhaCadastrada && <span className="text-green-600 font-normal">(cadastrada)</span>}
                    </label>
                    <div className="relative">
                      <input
                        type={mostrarSenha ? 'text' : 'password'}
                        value={emailCfg.senha}
                        onChange={e => setEmailCfg(p => ({ ...p, senha: e.target.value }))}
                        placeholder={senhaCadastrada ? '••••••••' : 'Senha de app'}
                        className="w-full px-3 py-2 pr-9 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                      />
                      <button
                        type="button"
                        onClick={() => setMostrarSenha(p => !p)}
                        className="absolute right-2 top-1/2 -translate-y-1/2 text-text-muted hover:text-text-primary"
                      >
                        {mostrarSenha ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                      </button>
                    </div>
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Email Remetente</label>
                    <input
                      type="email"
                      value={emailCfg.remetente}
                      onChange={e => setEmailCfg(p => ({ ...p, remetente: e.target.value }))}
                      placeholder="backup@empresa.com"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Nome Remetente</label>
                    <input
                      type="text"
                      value={emailCfg.nomeRemetente}
                      onChange={e => setEmailCfg(p => ({ ...p, nomeRemetente: e.target.value }))}
                      placeholder="IComanda Backup"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                  <div className="sm:col-span-2">
                    <label className="block text-xs font-semibold text-text-secondary mb-1">Email Destinatário (para quem enviar)</label>
                    <input
                      type="email"
                      value={emailCfg.destinatario}
                      onChange={e => setEmailCfg(p => ({ ...p, destinatario: e.target.value }))}
                      placeholder="gerencia@empresa.com"
                      className="w-full px-3 py-2 rounded-lg border border-border bg-background text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    />
                  </div>
                </div>

                <div className={`px-4 py-3 rounded-xl text-sm font-medium flex items-center gap-2 ${
                  mensagemEmail === null
                    ? 'hidden'
                    : mensagemEmail.tipo === 'sucesso'
                      ? 'bg-green-50 border border-green-200 text-green-800'
                      : 'bg-red-50 border border-red-200 text-red-700'
                }`}>
                  <CheckCircle2 className={`w-4 h-4 flex-shrink-0 ${mensagemEmail?.tipo === 'sucesso' ? '' : 'hidden'}`} />
                  {mensagemEmail?.texto ?? ''}
                </div>

                <Button
                  onClick={handleSalvarEmailCfg}
                  disabled={salvandoEmail}
                  className="mt-1 bg-sky-600 hover:bg-sky-700 text-white h-10 px-5 text-sm font-semibold"
                >
                  {salvandoEmail ? (
                    <span className="flex items-center gap-2">
                      <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white" />
                      Salvando...
                    </span>
                  ) : (
                    <span className="flex items-center gap-2">
                      <Save className="w-4 h-4" />
                      Salvar Config. de Email
                    </span>
                  )}
                </Button>
              </div>
            </div>

            {/* Mensagem de feedback - sempre no DOM, visibilidade via CSS */}
            <div className={`px-4 py-3 rounded-xl text-sm font-medium ${
              mensagem === null
                ? 'hidden'
                : mensagem.tipo === 'sucesso'
                  ? 'bg-green-50 text-green-700 border border-green-200'
                  : 'bg-red-50 text-red-700 border border-red-200'
            }`}>
              {mensagem?.texto ?? ''}
            </div>

            {/* Botões */}
            <Button onClick={handleSalvar} disabled={salvando} className="w-full h-12 text-base font-semibold flex items-center justify-center">
              {salvando ? (
                <span className="flex items-center">
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2" />
                  Salvando...
                </span>
              ) : (
                <span className="flex items-center">
                  <Save className="w-4 h-4 mr-2" />
                  Salvar Configurações
                </span>
              )}
            </Button>

            <Button variant="outline" onClick={onClose} className="w-full h-11">
              Voltar ao Menu
            </Button>
          </div>
        )}
      </div>
    </div>
  )
}

export default ConfiguracoesPage
