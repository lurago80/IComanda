import { AnimatePresence, motion } from 'framer-motion'
import { CheckCircle, Loader2, X } from 'lucide-react'
import React, { useState } from 'react'
import { useToast } from '../hooks/useToast'
import { clientesService } from '../services/api'
import { Cliente } from '../types/api'
import { Button } from './ui/button'

interface AbrirComandaModalProps {
  isOpen: boolean
  onClose: () => void
  onComandaAberta: (dados: DadosComanda) => void
}

export interface DadosComanda {
  numeroComanda: number
  numeroMesa?: number
  numeroPessoas?: number
  cliente: Cliente
}

const AbrirComandaModal: React.FC<AbrirComandaModalProps> = ({
  isOpen,
  onClose,
  onComandaAberta
}) => {
  const [step, setStep] = useState<'dados' | 'verificando' | 'sucesso'>('dados')
  const [numeroComanda, setNumeroComanda] = useState('')
  const [numeroMesa, setNumeroMesa] = useState('')
  const [numeroPessoas, setNumeroPessoas] = useState('')
  const [nomeCliente, setNomeCliente] = useState('')
  const [cpfCliente, setCpfCliente] = useState('')
  const [telefoneCliente, setTelefoneCliente] = useState('')
  const [isProcessing, setIsProcessing] = useState(false)
  const { showSuccess, showError } = useToast()

  // Máscaras
  const maskCpfCnpj = (value: string) => {
    value = value.replace(/\D/g, '')
    if (value.length <= 11) {
      // CPF: 000.000.000-00
      value = value.replace(/(\d{3})(\d)/, '$1.$2')
      value = value.replace(/(\d{3})(\d)/, '$1.$2')
      value = value.replace(/(\d{3})(\d{1,2})$/, '$1-$2')
    } else {
      // CNPJ: 00.000.000/0000-00
      value = value.replace(/^(\d{2})(\d)/, '$1.$2')
      value = value.replace(/^(\d{2})\.(\d{3})(\d)/, '$1.$2.$3')
      value = value.replace(/\.(\d{3})(\d)/, '.$1/$2')
      value = value.replace(/(\d{4})(\d)/, '$1-$2')
    }
    return value
  }

  const maskTelefone = (value: string) => {
    value = value.replace(/\D/g, '')
    if (value.length <= 10) {
      // (00) 0000-0000
      value = value.replace(/^(\d{2})(\d)/, '($1) $2')
      value = value.replace(/(\d{4})(\d)/, '$1-$2')
    } else {
      // (00) 00000-0000
      value = value.replace(/^(\d{2})(\d)/, '($1) $2')
      value = value.replace(/(\d{5})(\d)/, '$1-$2')
    }
    return value
  }

  const handleCpfChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setCpfCliente(maskCpfCnpj(e.target.value))
  }

  const handleTelefoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setTelefoneCliente(maskTelefone(e.target.value))
  }

  const validarCampos = (): boolean => {
    if (!numeroComanda || numeroComanda.trim() === '') {
      showError('Erro de validação', 'Número da comanda é obrigatório')
      return false
    }

    if (!nomeCliente || nomeCliente.trim() === '') {
      showError('Erro de validação', 'Nome do cliente é obrigatório')
      return false
    }

    if (!cpfCliente || cpfCliente.trim() === '') {
      showError('Erro de validação', 'CPF/CNPJ é obrigatório')
      return false
    }

    if (!telefoneCliente || telefoneCliente.trim() === '') {
      showError('Erro de validação', 'Telefone é obrigatório')
      return false
    }

    return true
  }

  const handleAbrirComanda = async () => {
    if (!validarCampos()) return

    setIsProcessing(true)
    setStep('verificando')

    try {
      // Verificar se cliente já existe por CPF ou Telefone
      const cpfLimpo = cpfCliente.replace(/\D/g, '')
      const verificacao = await clientesService.verificar(cpfLimpo)

      let clienteId: Cliente

      if (verificacao.existe && verificacao.cliente) {
        // Cliente já existe
        clienteId = verificacao.cliente
        showSuccess('Cliente encontrado!', `${clienteId.nome} já cadastrado`)
      } else {
        // Cadastrar novo cliente
        const telefoneLimpo = telefoneCliente.replace(/\D/g, '')
        
        const novoCliente = await clientesService.cadastroRapido({
          nome: nomeCliente,
          cpfCnpj: cpfLimpo,
          telefone: telefoneLimpo,
          celular: telefoneLimpo
        })

        clienteId = novoCliente
        showSuccess('Cliente cadastrado!', `${novoCliente.nome} cadastrado com sucesso`)
      }

      // Preparar dados da comanda
      const dadosComanda: DadosComanda = {
        numeroComanda: parseInt(numeroComanda),
        numeroMesa: numeroMesa ? parseInt(numeroMesa) : undefined,
        numeroPessoas: numeroPessoas ? parseInt(numeroPessoas) : undefined,
        cliente: clienteId
      }

      setStep('sucesso')
      
      // Aguardar 1 segundo e notificar abertura
      setTimeout(() => {
        onComandaAberta(dadosComanda)
        handleFechar()
      }, 1500)

    } catch (error: any) {
      console.error('Erro ao abrir comanda:', error)
      setStep('dados')
      
      if (error.response?.data) {
        showError('Erro ao abrir comanda', error.response.data)
      } else {
        showError('Erro ao abrir comanda', 'Não foi possível processar a abertura da comanda')
      }
    } finally {
      setIsProcessing(false)
    }
  }

  const handleFechar = () => {
    setStep('dados')
    setNumeroComanda('')
    setNumeroMesa('')
    setNumeroPessoas('')
    setNomeCliente('')
    setCpfCliente('')
    setTelefoneCliente('')
    setIsProcessing(false)
    onClose()
  }

  if (!isOpen) return null

  return (
    <AnimatePresence>
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        onClick={handleFechar}
      >
        <motion.div
          initial={{ scale: 0.95, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0.95, opacity: 0 }}
          className="bg-card w-full max-w-lg rounded-3xl shadow-large border border-border overflow-hidden"
          onClick={(e) => e.stopPropagation()}
        >
          {/* Header */}
          <div className="bg-primary text-primary-foreground p-6 flex justify-between items-center">
            <h2 className="text-2xl font-bold">Nova Comanda</h2>
            <button
              onClick={handleFechar}
              disabled={isProcessing}
              className="p-2 hover:bg-white/20 rounded-xl transition-colors disabled:opacity-50"
            >
              <X size={24} />
            </button>
          </div>

          {/* Content */}
          <div className="p-6 space-y-4">
            {step === 'dados' && (
              <>
                {/* Dados da Comanda */}
                <div className="space-y-4">
                  <h3 className="text-lg font-semibold text-text-primary">Dados da Comanda</h3>
                  
                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      Número da Comanda *
                    </label>
                    <input
                      type="number"
                      value={numeroComanda}
                      onChange={(e) => setNumeroComanda(e.target.value)}
                      className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                text-text-primary placeholder-text-muted transition-all"
                      placeholder="Ex: 1"
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium text-text-secondary mb-2">
                        Número da Mesa
                      </label>
                      <input
                        type="number"
                        value={numeroMesa}
                        onChange={(e) => setNumeroMesa(e.target.value)}
                        className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                  focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                  text-text-primary placeholder-text-muted transition-all"
                        placeholder="Ex: 10"
                      />
                    </div>

                    <div>
                      <label className="block text-sm font-medium text-text-secondary mb-2">
                        Nº de Pessoas
                      </label>
                      <input
                        type="number"
                        value={numeroPessoas}
                        onChange={(e) => setNumeroPessoas(e.target.value)}
                        className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                  focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                  text-text-primary placeholder-text-muted transition-all"
                        placeholder="Ex: 4"
                      />
                    </div>
                  </div>
                </div>

                {/* Dados do Cliente */}
                <div className="space-y-4 pt-4 border-t border-border">
                  <h3 className="text-lg font-semibold text-text-primary">Dados do Cliente</h3>
                  
                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      Nome Completo *
                    </label>
                    <input
                      type="text"
                      value={nomeCliente}
                      onChange={(e) => setNomeCliente(e.target.value)}
                      className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                text-text-primary placeholder-text-muted transition-all"
                      placeholder="Ex: João da Silva"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      CPF/CNPJ *
                    </label>
                    <input
                      type="text"
                      value={cpfCliente}
                      onChange={handleCpfChange}
                      maxLength={18}
                      className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                text-text-primary placeholder-text-muted transition-all"
                      placeholder="000.000.000-00"
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      Telefone *
                    </label>
                    <input
                      type="text"
                      value={telefoneCliente}
                      onChange={handleTelefoneChange}
                      maxLength={15}
                      className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                text-text-primary placeholder-text-muted transition-all"
                      placeholder="(00) 00000-0000"
                    />
                  </div>

                  <p className="text-xs text-text-muted">
                    * Campos obrigatórios
                  </p>
                </div>

                {/* Botão */}
                <Button
                  onClick={handleAbrirComanda}
                  disabled={isProcessing}
                  className="w-full py-4 text-lg font-semibold"
                >
                  Abrir Comanda
                </Button>
              </>
            )}

            {step === 'verificando' && (
              <div className="flex flex-col items-center justify-center py-12 space-y-4">
                <Loader2 size={48} className="text-primary animate-spin" />
                <p className="text-lg text-text-secondary">Verificando dados...</p>
              </div>
            )}

            {step === 'sucesso' && (
              <div className="flex flex-col items-center justify-center py-12 space-y-4">
                <motion.div
                  initial={{ scale: 0 }}
                  animate={{ scale: 1 }}
                  transition={{ type: 'spring', stiffness: 200, damping: 10 }}
                >
                  <CheckCircle size={64} className="text-success" />
                </motion.div>
                <p className="text-xl font-bold text-success">Comanda aberta!</p>
                <p className="text-text-muted">Redirecionando...</p>
              </div>
            )}
          </div>
        </motion.div>
      </motion.div>
    </AnimatePresence>
  )
}

export default AbrirComandaModal

