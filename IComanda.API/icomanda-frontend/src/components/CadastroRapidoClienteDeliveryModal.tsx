import { AnimatePresence, motion } from 'framer-motion'
import { AlertCircle, Loader2, MapPin, Phone, User, X } from 'lucide-react'
import React, { useState } from 'react'
import { useToast } from '../hooks/useToast'
import { clientesService } from '../services/api'
import { Cliente } from '../types/api'
import { Button } from './ui/button'

interface CadastroRapidoClienteDeliveryModalProps {
  isOpen: boolean
  onClose: () => void
  onClienteCriado: (cliente: Cliente) => void
}

const CadastroRapidoClienteDeliveryModal: React.FC<CadastroRapidoClienteDeliveryModalProps> = ({
  isOpen,
  onClose,
  onClienteCriado
}) => {
  const [nome, setNome] = useState('')
  const [telefone, setTelefone] = useState('')
  const [enderecoCompleto, setEnderecoCompleto] = useState('')
  const [pontoReferencia, setPontoReferencia] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [mostrarPerguntaGravar, setMostrarPerguntaGravar] = useState(false)
  const [erroInline, setErroInline] = useState<string | null>(null)
  const [erroTelefoneDuplicado, setErroTelefoneDuplicado] = useState(false)
  const [ultimoGravarNoCadastro, setUltimoGravarNoCadastro] = useState(false)
  const { showSuccess } = useToast()

  const limpar = () => {
    setNome('')
    setTelefone('')
    setEnderecoCompleto('')
    setPontoReferencia('')
    setMostrarPerguntaGravar(false)
    setErroInline(null)
    setErroTelefoneDuplicado(false)
  }

  const handleClose = () => {
    limpar()
    onClose()
  }

  const validar = () => {
    const n = nome.trim()
    if (!n) {
      setErroInline('Informe o nome do cliente.')
      return false
    }
    if (!enderecoCompleto.trim()) {
      setErroInline('Informe o endereço completo para entrega.')
      return false
    }
    setErroInline(null)
    return true
  }

  const handleContinuar = () => {
    if (!validar()) return
    setErroInline(null)
    setMostrarPerguntaGravar(true)
  }

  const handleConfirmarGravar = async (gravarNoCadastro: boolean, forcarSemTelefone = false) => {
    setSalvando(true)
    setUltimoGravarNoCadastro(gravarNoCadastro)
    try {
      const t = forcarSemTelefone ? '' : telefone.trim().replace(/\D/g, '')
      const cliente = await clientesService.cadastroRapido({
        nome: nome.trim(),
        telefone: t || '',
        celular: t || undefined,
        endereco1: enderecoCompleto.trim(),
        complemento1: pontoReferencia.trim() || undefined,
        gravarNoCadastro
      })
      showSuccess('Cliente cadastrado', gravarNoCadastro ? 'Cliente gravado no cadastro.' : 'Dados usados apenas para este pedido.')
      onClienteCriado(cliente)
      handleClose()
    } catch (err: any) {
      // Extrair a mensagem real do erro em todos os formatos possíveis da API:
      // 1. String direta:            err.response.data = "mensagem"
      // 2. Campo message:            err.response.data = { message: "mensagem" }
      // 3. ExceptionMiddleware:      err.response.data = { error: { message: "...", details: "mensagem real" } }
      // 4. Validation (FluentVal):   err.response.data = { title: "...", errors: { Campo: ["msg"] } }
      const data = err?.response?.data
      const validationErrors: string[] = data?.errors
        ? Object.values(data.errors as Record<string, string[]>).flat()
        : []

      const msg: string =
        (typeof data === 'string' ? data : null) ??
        data?.message ??
        data?.error?.details ??
        data?.error?.message ??
        data?.title ??
        (validationErrors.length > 0 ? validationErrors.join(' | ') : null) ??
        err?.message ??
        'Não foi possível cadastrar o cliente.'

      const eTelefoneDuplicado =
        msg.toLowerCase().includes('telefone') &&
        (msg.toLowerCase().includes('cadastrado') || msg.toLowerCase().includes('existe'))
      setErroInline(msg)
      setErroTelefoneDuplicado(eTelefoneDuplicado)
      setMostrarPerguntaGravar(false)
    } finally {
      setSalvando(false)
    }
  }

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            className="fixed inset-0 bg-black/50 z-[130]"
            onClick={handleClose}
          />
          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: 'spring', stiffness: 300, damping: 30 }}
            className="fixed bottom-0 left-0 right-0 bg-card border-t rounded-t-3xl z-[130] max-h-[90vh] flex flex-col shadow-2xl"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between p-4 border-b">
              <h2 className="text-lg font-semibold text-foreground">
                {mostrarPerguntaGravar ? 'Gravar no cadastro?' : 'Cadastrar cliente (delivery)'}
              </h2>
              <button
                type="button"
                onClick={handleClose}
                className="p-2 rounded-lg text-muted-foreground hover:bg-muted"
                aria-label="Fechar"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <div className="flex-1 overflow-y-auto p-4">
              {erroInline && (
                <motion.div
                  initial={{ opacity: 0, y: -8 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="bg-destructive/10 border border-destructive/40 text-destructive rounded-xl px-4 py-3 mb-4"
                >
                  <div className="flex items-start gap-2">
                    <AlertCircle className="h-5 w-5 mt-0.5 shrink-0" />
                    <p className="text-sm font-medium">{erroInline}</p>
                  </div>
                  {erroTelefoneDuplicado && (
                    <button
                      type="button"
                      onClick={() => handleConfirmarGravar(ultimoGravarNoCadastro, true)}
                      disabled={salvando}
                      className="mt-3 w-full text-sm font-medium bg-destructive/20 hover:bg-destructive/30 text-destructive border border-destructive/40 rounded-lg px-3 py-2 transition-colors disabled:opacity-50"
                    >
                      {salvando ? <Loader2 className="h-4 w-4 animate-spin mx-auto" /> : 'Salvar sem telefone'}
                    </button>
                  )}
                </motion.div>
              )}
              {!mostrarPerguntaGravar ? (
                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Nome *</label>
                    <div className="relative">
                      <User className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <input
                        type="text"
                        value={nome}
                        onChange={(e) => setNome(e.target.value)}
                        placeholder="Nome completo"
                        className="w-full pl-10 pr-3 py-2 border rounded-xl bg-background text-foreground"
                        autoFocus
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Telefone (opcional)</label>
                    <div className="relative">
                      <Phone className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <input
                        type="tel"
                        value={telefone}
                        onChange={(e) => setTelefone(e.target.value)}
                        placeholder="(00) 00000-0000"
                        className="w-full pl-10 pr-3 py-2 border rounded-xl bg-background text-foreground"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Endereço completo *</label>
                    <div className="relative">
                      <MapPin className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                      <textarea
                        value={enderecoCompleto}
                        onChange={(e) => setEnderecoCompleto(e.target.value)}
                        placeholder="Rua, número, bairro, cidade, CEP..."
                        rows={3}
                        className="w-full pl-10 pr-3 py-2 border rounded-xl bg-background text-foreground resize-none"
                      />
                    </div>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-foreground mb-1">Ponto de Referência (opcional)</label>
                    <div className="relative">
                      <MapPin className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                      <input
                        type="text"
                        value={pontoReferencia}
                        onChange={(e) => setPontoReferencia(e.target.value)}
                        placeholder="Ex: Próximo ao mercado, portão azul..."
                        className="w-full pl-10 pr-3 py-2 border rounded-xl bg-background text-foreground"
                      />
                    </div>
                  </div>
                </div>
              ) : (
                <div className="py-4">
                  <p className="text-base text-foreground mb-6">
                    Deseja gravar o nome do cliente no cadastro de clientes?
                  </p>
                  <p className="text-sm text-muted-foreground mb-6">
                    Se sim, o cliente poderá ser encontrado na busca nas próximas vezes. Se não, os dados serão usados apenas para este pedido.
                  </p>
                  <div className="flex gap-3">
                    <Button
                      type="button"
                      variant="outline"
                      className="flex-1"
                      onClick={() => handleConfirmarGravar(false)}
                      disabled={salvando}
                    >
                      Não, só este pedido
                    </Button>
                    <Button
                      type="button"
                      className="flex-1"
                      onClick={() => handleConfirmarGravar(true)}
                      disabled={salvando}
                    >
                      {salvando ? <Loader2 className="h-4 w-4 animate-spin" /> : 'Sim, gravar no cadastro'}
                    </Button>
                  </div>
                </div>
              )}
            </div>

            {!mostrarPerguntaGravar && (
              <div className="p-4 border-t flex gap-2">
                <Button type="button" variant="outline" className="flex-1" onClick={handleClose}>
                  Cancelar
                </Button>
                <Button type="button" className="flex-1" onClick={handleContinuar}>
                  Continuar
                </Button>
              </div>
            )}
          </motion.div>
        </>
      )}
    </AnimatePresence>
  )
}

export default CadastroRapidoClienteDeliveryModal
