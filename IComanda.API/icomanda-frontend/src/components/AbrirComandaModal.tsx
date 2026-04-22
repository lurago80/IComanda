import { AnimatePresence, motion } from 'framer-motion'
import { CheckCircle, Loader2, X, User, Search, AlertTriangle, MapPin } from 'lucide-react'
import React, { useState, useEffect } from 'react'
import { useToast } from '../hooks/useToast'
import { clientesService, vendasService, receberService } from '../services/api'
import { Cliente } from '../types/api'
import { Button } from './ui/button'
import ClienteSearch from './ClienteSearch'

interface AbrirComandaModalProps {
  isOpen: boolean
  onClose: () => void
  onComandaAberta: (dados: DadosComanda) => void
}

export interface DadosComanda {
  numeroComanda: number
  numeroMesa?: number
  numeroPessoas?: number
  cliente?: Cliente
  /** Nome digitado para exibição na comanda quando cliente não foi cadastrado (id = 0) */
  nomeClienteExibicao?: string
}

/** Captura Enter globalmente enquanto montado */
const KeyEnterHandler: React.FC<{ onEnter: () => void; disabled?: boolean }> = ({ onEnter, disabled }) => {
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === 'Enter' && !disabled) {
        e.preventDefault()
        onEnter()
      }
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [onEnter, disabled])
  return null
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
  const [enderecoCliente, setEnderecoCliente] = useState('')
  const [numeroCliente, setNumeroCliente] = useState('')
  const [complementoCliente, setComplementoCliente] = useState('')
  const [bairroCliente, setBairroCliente] = useState('')
  const [cidadeCliente, setCidadeCliente] = useState('')
  const [ufCliente, setUfCliente] = useState('')
  const [cepCliente, setCepCliente] = useState('')
  const [clienteSelecionado, setClienteSelecionado] = useState<Cliente | null>(null)
  const [showClienteSearch, setShowClienteSearch] = useState(false)
  const [isProcessing, setIsProcessing] = useState(false)
  const [verificandoContas, setVerificandoContas] = useState(false)
  const [temValorAberto, setTemValorAberto] = useState(false)
  const [valorAberto, setValorAberto] = useState<number>(0)
  const [showPerguntaCadastrarCliente, setShowPerguntaCadastrarCliente] = useState(false)
  const [pendingAbrirComanda, setPendingAbrirComanda] = useState<{
    numeroComandaInt: number
    numeroMesa?: number
    numeroPessoas?: number
  } | null>(null)
  const { showSuccess, showError, showWarning } = useToast()

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

  const handleSelecionarCliente = async (cliente: Cliente) => {
    setClienteSelecionado(cliente)
    setNomeCliente(cliente.nomeCompleto || cliente.nome || '')
    setCpfCliente(cliente.documento || cliente.cpfCnpj || '')
    setTelefoneCliente(cliente.telefone || cliente.celular || cliente.contato || '')
    // Preencher dados de endereço
    setEnderecoCliente(cliente.endereco1 || '')
    setNumeroCliente(cliente.numero1 || '')
    setComplementoCliente(cliente.complemento1 || '')
    setBairroCliente(cliente.bairro1 || '')
    setCidadeCliente(cliente.cidade1 || '')
    setUfCliente(cliente.uf1 || '')
    setCepCliente(cliente.cep1 || '')
    setShowClienteSearch(false)
    
    // Verificar se tem valores em aberto
    await verificarValoresAberto(cliente.id)
  }

  const verificarValoresAberto = async (codigoCliente: number) => {
    try {
      setVerificandoContas(true)
      const contas = await receberService.listarPendentes(codigoCliente)
      
      if (contas && contas.length > 0) {
        // O endpoint já retorna apenas pendentes, então podemos somar diretamente
        const totalPendente = contas.reduce((acc: number, c: any) => {
          // ReceberDto tem ValorPendente como propriedade calculada (Valor - ValorRecebido)
          // Mas pode não vir do JSON, então calculamos manualmente
          const valor = c.valor || 0
          const valorRecebido = c.valorRecebido || 0
          const pendente = valor - valorRecebido
          return acc + (pendente > 0 ? pendente : 0)
        }, 0)
        
        if (totalPendente > 0) {
          setTemValorAberto(true)
          setValorAberto(totalPendente)
          showWarning(
            'Cliente com valores em aberto',
            `Este cliente possui ${formatarMoeda(totalPendente)} em contas a receber pendentes.`
          )
        } else {
          setTemValorAberto(false)
          setValorAberto(0)
        }
      } else {
        setTemValorAberto(false)
        setValorAberto(0)
      }
    } catch (error: any) {
      // Não mostrar erro, apenas não exibir o aviso
      console.error('Erro ao verificar valores em aberto:', error)
      setTemValorAberto(false)
      setValorAberto(0)
    } finally {
      setVerificandoContas(false)
    }
  }

  const handleLimparCliente = () => {
    setClienteSelecionado(null)
    setNomeCliente('')
    setCpfCliente('')
    setTelefoneCliente('')
    setEnderecoCliente('')
    setNumeroCliente('')
    setComplementoCliente('')
    setBairroCliente('')
    setCidadeCliente('')
    setUfCliente('')
    setCepCliente('')
    setTemValorAberto(false)
    setValorAberto(0)
  }

  const validarCampos = (): boolean => {
    // Número da comanda é opcional - se não informado, será gerado automaticamente
    // Se cliente foi selecionado via busca, não precisa validar campos (já está completo)
    if (clienteSelecionado) {
      return true
    }

    // Nome é obrigatório quando informar qualquer dado do cliente. CPF e telefone são opcionais.
    const temNome = nomeCliente.trim() !== ''
    const temCpf = cpfCliente.trim() !== ''
    const temTelefone = telefoneCliente.trim() !== ''

    if (temCpf || temTelefone) {
      if (!temNome) {
        showError('Erro de validação', 'Nome do cliente é obrigatório')
        return false
      }
    }

    return true
  }

  const handleAbrirComanda = async () => {
    if (!validarCampos()) return

    setIsProcessing(true)
    setStep('verificando')

    try {
      let numeroComandaInt: number

      // Regra: se o usuário informou um número, SEMPRE usa esse número
      // Só gera automaticamente se o campo estiver vazio
      if (!numeroComanda || numeroComanda.trim() === '') {
        console.log('🔢 [AbrirComandaModal] Gerando número de comanda automaticamente...')
        const gerado = await vendasService.gerarProximoNumeroComanda()
        numeroComandaInt = typeof gerado === 'number' && gerado > 0 ? gerado : 1
        setNumeroComanda(numeroComandaInt.toString())
        console.log('✅ [AbrirComandaModal] Número gerado:', numeroComandaInt)
        showSuccess('Número gerado', `Comanda ${numeroComandaInt} gerada automaticamente`)
      } else {
        numeroComandaInt = parseInt(numeroComanda, 10)
        if (Number.isNaN(numeroComandaInt) || numeroComandaInt <= 0) {
          showError('Número inválido', 'Informe um número de comanda válido (maior que zero).')
          setIsProcessing(false)
          setStep('dados')
          return
        }
        // Verificar se a comanda informada já está aberta
        const comandaAberta = await vendasService.verificarComandaAberta(numeroComandaInt)
        if (comandaAberta) {
          showError(
            'Comanda já está aberta',
            `A comanda ${numeroComandaInt} já possui uma venda em aberto. Feche ou exclua a comanda existente antes de abrir uma nova.`
          )
          setIsProcessing(false)
          setStep('dados')
          return
        }
      }

      let clienteId: Cliente | undefined

      // Se cliente foi selecionado via busca, usar diretamente
      if (clienteSelecionado) {
        clienteId = clienteSelecionado
      } else {
        // Processar cliente apenas se informado manualmente
        const temNome = nomeCliente.trim() !== ''
        const temCpf = cpfCliente.trim() !== ''
        const temTelefone = telefoneCliente.trim() !== ''
        
        if (temNome) {
          // Se tem CPF ou telefone, verificar se cliente já existe
          if (temCpf || temTelefone) {
            const cpfLimpo = cpfCliente.replace(/\D/g, '')
            
            // Se tem CPF, verificar por CPF
            if (cpfLimpo) {
              const verificacao = await clientesService.verificar(cpfLimpo)

              if (verificacao.existe && verificacao.cliente) {
                // Cliente já existe
                clienteId = verificacao.cliente
                showSuccess('Cliente encontrado!', `${clienteId.nome} já cadastrado`)
              } else {
                // Perguntar se deseja cadastrar antes de cadastrar
                setPendingAbrirComanda({
                  numeroComandaInt,
                  numeroMesa: numeroMesa ? parseInt(numeroMesa) : undefined,
                  numeroPessoas: numeroPessoas ? parseInt(numeroPessoas) : undefined
                })
                setShowPerguntaCadastrarCliente(true)
                setIsProcessing(false)
                setStep('dados')
                return
              }
            } else {
              // Tem telefone mas não tem CPF - perguntar se deseja cadastrar
              setPendingAbrirComanda({
                numeroComandaInt,
                numeroMesa: numeroMesa ? parseInt(numeroMesa) : undefined,
                numeroPessoas: numeroPessoas ? parseInt(numeroPessoas) : undefined
              })
              setShowPerguntaCadastrarCliente(true)
              setIsProcessing(false)
              setStep('dados')
              return
            }
          } else {
            // Apenas nome informado - perguntar se deseja cadastrar
            setPendingAbrirComanda({
              numeroComandaInt,
              numeroMesa: numeroMesa ? parseInt(numeroMesa) : undefined,
              numeroPessoas: numeroPessoas ? parseInt(numeroPessoas) : undefined
            })
            setShowPerguntaCadastrarCliente(true)
            setIsProcessing(false)
            setStep('dados')
            return
          }
        }
      }

      // Preparar dados da comanda
      const dadosComanda: DadosComanda = {
        numeroComanda: numeroComandaInt,
        numeroMesa: numeroMesa ? parseInt(numeroMesa) : undefined,
        numeroPessoas: numeroPessoas ? parseInt(numeroPessoas) : undefined,
        cliente: clienteId,
        // Se não tem cliente cadastrado mas tem nome digitado, passar para exibição
        ...((!clienteId && nomeCliente.trim()) ? { nomeClienteExibicao: nomeCliente.trim() } : {})
      }

      console.log('📋 [AbrirComandaModal] Dados da comanda preparados:', dadosComanda)
      console.log('📋 [AbrirComandaModal] Número da comanda:', numeroComandaInt)
      console.log('📋 [AbrirComandaModal] Cliente:', clienteId ? `${clienteId.nome} (ID: ${clienteId.id})` : 'Nenhum')
      
      setStep('sucesso')
      
      // Aguardar 1.5 segundos e notificar abertura
      setTimeout(() => {
        console.log('🚀 [AbrirComandaModal] Chamando onComandaAberta com:', dadosComanda)
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

  const concluirAberturaComanda = (clienteId: Cliente | undefined, nomeParaExibicao?: string) => {
    if (!pendingAbrirComanda) return
    const dadosComanda: DadosComanda = {
      numeroComanda: pendingAbrirComanda.numeroComandaInt,
      numeroMesa: pendingAbrirComanda.numeroMesa,
      numeroPessoas: pendingAbrirComanda.numeroPessoas,
      cliente: clienteId,
      ...(nomeParaExibicao?.trim() ? { nomeClienteExibicao: nomeParaExibicao.trim() } : {})
    }
    setShowPerguntaCadastrarCliente(false)
    setPendingAbrirComanda(null)
    setStep('sucesso')
    setTimeout(() => {
      onComandaAberta(dadosComanda)
      handleFechar()
    }, 1500)
  }

  const handleSimCadastrarCliente = async () => {
    if (!pendingAbrirComanda) return
    setIsProcessing(true)
    try {
      const cpfLimpo = cpfCliente.replace(/\D/g, '')
      const telefoneLimpo = telefoneCliente.replace(/\D/g, '')
      let novoCliente: Cliente

      if (cpfLimpo) {
        novoCliente = await clientesService.cadastroRapido({
          nome: nomeCliente,
          cpfCnpj: cpfLimpo,
          telefone: telefoneLimpo,
          celular: telefoneLimpo
        })
      } else if (telefoneLimpo) {
        novoCliente = await clientesService.cadastroRapido({
          nome: nomeCliente,
          cpfCnpj: '',
          telefone: telefoneLimpo,
          celular: telefoneLimpo
        })
      } else {
        novoCliente = await clientesService.cadastroRapido({
          nome: nomeCliente,
          cpfCnpj: '',
          telefone: '',
          celular: ''
        })
      }
      showSuccess('Cliente cadastrado!', `${novoCliente.nome} cadastrado com sucesso`)
      concluirAberturaComanda(novoCliente)
    } catch (error: any) {
      showError('Erro', error.response?.data || 'Não foi possível cadastrar o cliente')
    } finally {
      setIsProcessing(false)
    }
  }

  const handleNaoCadastrarCliente = () => {
    // Vincular cliente id = 0; passar nome digitado para exibir na comanda (saber de quem é)
    concluirAberturaComanda(undefined, nomeCliente.trim() || undefined)
  }

  const handleFechar = () => {
    setStep('dados')
    setShowPerguntaCadastrarCliente(false)
    setPendingAbrirComanda(null)
    setNumeroComanda('')
    setNumeroMesa('')
    setNumeroPessoas('')
    setNomeCliente('')
    setCpfCliente('')
    setTelefoneCliente('')
    setEnderecoCliente('')
    setNumeroCliente('')
    setComplementoCliente('')
    setBairroCliente('')
    setCidadeCliente('')
    setUfCliente('')
    setCepCliente('')
    setClienteSelecionado(null)
    setTemValorAberto(false)
    setValorAberto(0)
    setIsProcessing(false)
    onClose()
  }

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor)
  }

  if (!isOpen) return null

  return (
    <AnimatePresence>
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        className="fixed inset-0 z-[110] flex items-start sm:items-center justify-center p-0 sm:p-2 sm:p-4 bg-black/60 backdrop-blur-sm overflow-y-auto"
        onClick={handleFechar}
      >
        <motion.div
          initial={{ scale: 0.95, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0.95, opacity: 0 }}
          className="bg-card w-full sm:max-w-lg min-h-screen sm:min-h-0 max-h-screen sm:max-h-[95vh] rounded-none sm:rounded-3xl shadow-large border-0 sm:border border-border overflow-hidden flex flex-col my-0 sm:my-auto"
          onClick={(e) => e.stopPropagation()}
        >
          {/* Header - Fixo */}
          <div className="bg-primary text-primary-foreground p-4 sm:p-6 flex justify-between items-center flex-shrink-0">
            <h2 className="text-xl sm:text-2xl font-bold">Nova Comanda</h2>
            <button
              onClick={handleFechar}
              disabled={isProcessing}
              className="p-2 hover:bg-white/20 rounded-xl transition-colors disabled:opacity-50"
            >
              <X size={24} />
            </button>
          </div>

          {/* Content - Scrollável */}
          <div className="flex-1 overflow-y-auto p-4 sm:p-6 space-y-4 pb-6">
            {step === 'dados' && (
              <form onSubmit={(e) => { e.preventDefault(); handleAbrirComanda(); }}>
                {/* Dados da Comanda */}
                <div className="space-y-4">
                  <h3 className="text-base sm:text-lg font-semibold text-text-primary">Dados da Comanda</h3>
                  
                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      Número da Comanda
                      <span className="text-xs text-text-muted ml-2">(opcional - será gerado automaticamente se não informado)</span>
                    </label>
                    <input
                      type="number"
                      value={numeroComanda}
                      onChange={(e) => setNumeroComanda(e.target.value)}
                      className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                text-text-primary placeholder-text-muted transition-all"
                      placeholder="Deixe em branco para gerar automaticamente"
                    />
                    <p className="text-xs text-text-muted mt-1">
                      💡 Dica: Informe apenas o nome do cliente para criar uma comanda automática. O número será gerado automaticamente.
                    </p>
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

                {/* Dados do Cliente - Opcional */}
                <div className="space-y-4 pt-4 border-t border-border">
                  <div className="flex items-center justify-between mb-3">
                    <h3 className="text-base sm:text-lg font-semibold text-text-primary">
                      Dados do Cliente
                      <span className="text-xs font-normal text-text-muted ml-2">(opcional)</span>
                    </h3>
                    <Button
                      type="button"
                      onClick={() => setShowClienteSearch(true)}
                      variant="outline"
                      size="sm"
                      className="flex items-center space-x-2"
                    >
                      <Search className="w-4 h-4" />
                      <span>Buscar Cliente</span>
                    </Button>
                  </div>
                  
                  {clienteSelecionado && (
                    <div className="bg-primary/10 border border-primary/30 rounded-xl p-3 mb-3">
                      <div className="flex items-start justify-between">
                        <div className="flex-1">
                          <div className="flex items-center space-x-2 mb-2">
                            <User className="w-5 h-5 text-primary flex-shrink-0" />
                            <div className="flex-1 min-w-0">
                              <p className="font-semibold text-sm text-text-primary">
                                {clienteSelecionado.nomeCompleto || clienteSelecionado.nome}
                              </p>
                              {clienteSelecionado.documento && (
                                <p className="text-xs text-text-secondary">
                                  {clienteSelecionado.documento}
                                </p>
                              )}
                            </div>
                          </div>
                          {/* Endereço Completo */}
                          {(enderecoCliente || bairroCliente || cidadeCliente || ufCliente) && (
                            <div className="flex items-start space-x-2 mt-2 pt-2 border-t border-primary/20">
                              <MapPin className="w-4 h-4 text-primary flex-shrink-0 mt-0.5" />
                              <div className="flex-1 min-w-0">
                                {/* Endereço com número e complemento */}
                                {enderecoCliente && (
                                  <div className="text-xs font-medium text-text-primary">
                                    {enderecoCliente}
                                    {numeroCliente && `, ${numeroCliente}`}
                                    {complementoCliente && ` - ${complementoCliente}`}
                                  </div>
                                )}
                                {/* Bairro, Cidade e UF */}
                                {(bairroCliente || cidadeCliente || ufCliente) && (
                                  <div className="text-xs text-text-secondary mt-0.5">
                                    {[
                                      bairroCliente,
                                      cidadeCliente ? (ufCliente ? `${cidadeCliente}/${ufCliente}` : cidadeCliente) : ufCliente
                                    ].filter(Boolean).join(' - ')}
                                  </div>
                                )}
                                {/* CEP */}
                                {cepCliente && (
                                  <div className="text-xs text-text-muted mt-0.5">
                                    CEP: {cepCliente}
                                  </div>
                                )}
                              </div>
                            </div>
                          )}
                        </div>
                        <button
                          onClick={handleLimparCliente}
                          className="text-text-muted hover:text-text-primary transition-colors ml-2 flex-shrink-0"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    </div>
                  )}

                  {temValorAberto && (
                    <div className="bg-yellow-50 border-2 border-yellow-300 rounded-xl p-4 mb-3">
                      <div className="flex items-start space-x-3">
                        <AlertTriangle className="w-5 h-5 text-yellow-600 flex-shrink-0 mt-0.5" />
                        <div className="flex-1">
                          <p className="font-semibold text-sm text-yellow-800 mb-1">
                            Cliente com valores em aberto
                          </p>
                          <p className="text-xs text-yellow-700">
                            Este cliente possui <strong>{formatarMoeda(valorAberto)}</strong> em contas a receber pendentes.
                          </p>
                        </div>
                      </div>
                    </div>
                  )}

                  {verificandoContas && (
                    <div className="bg-blue-50 border border-blue-200 rounded-xl p-3 mb-3">
                      <div className="flex items-center space-x-2">
                        <Loader2 className="w-4 h-4 text-blue-600 animate-spin" />
                        <p className="text-xs text-blue-700">Verificando valores em aberto...</p>
                      </div>
                    </div>
                  )}

                  <p className="text-xs text-text-muted mb-3 sm:mb-4">
                    {clienteSelecionado 
                      ? 'Cliente selecionado. Você pode editar os dados abaixo ou limpar para escolher outro.'
                      : 'Clique em "Buscar Cliente" para selecionar ou preencha nome e/ou telefone. CPF/CNPJ é opcional.'}
                  </p>
                  
                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      Nome Completo
                    </label>
                    <input
                      type="text"
                      value={nomeCliente}
                      onChange={(e) => {
                        setNomeCliente(e.target.value)
                        if (!e.target.value) {
                          setClienteSelecionado(null)
                          setTemValorAberto(false)
                        }
                      }}
                      className="w-full px-4 py-3 bg-background-secondary border border-border rounded-2xl
                                focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                text-text-primary placeholder-text-muted transition-all"
                      placeholder="Ex: João da Silva"
                      disabled={!!clienteSelecionado}
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      CPF/CNPJ <span className="text-text-muted font-normal">(opcional)</span>
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
                      disabled={!!clienteSelecionado}
                    />
                  </div>

                  <div>
                    <label className="block text-sm font-medium text-text-secondary mb-2">
                      Telefone
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
                      disabled={!!clienteSelecionado}
                    />
                  </div>

                  {/* Endereço - Exibir quando cliente selecionado ou campos preenchidos */}
                  {(clienteSelecionado || enderecoCliente || bairroCliente || cidadeCliente) && (
                    <div className="space-y-3 pt-2 border-t border-border">
                      <div className="flex items-center space-x-2 mb-2">
                        <MapPin className="w-4 h-4 text-primary" />
                        <h4 className="text-sm font-semibold text-text-primary">Endereço para Delivery</h4>
                      </div>
                      
                      <div>
                        <label className="block text-xs font-medium text-text-secondary mb-1">
                          Endereço
                        </label>
                        <input
                          type="text"
                          value={enderecoCliente}
                          onChange={(e) => setEnderecoCliente(e.target.value)}
                          className="w-full px-3 py-2 bg-background-secondary border border-border rounded-xl
                                    focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                    text-text-primary placeholder-text-muted transition-all text-sm"
                          placeholder="Rua, Avenida, etc."
                          disabled={!!clienteSelecionado}
                        />
                      </div>

                      <div className="grid grid-cols-3 gap-3">
                        <div>
                          <label className="block text-xs font-medium text-text-secondary mb-1">
                            Nº
                          </label>
                          <input
                            type="text"
                            value={numeroCliente}
                            onChange={(e) => setNumeroCliente(e.target.value)}
                            className="w-full px-3 py-2 bg-background-secondary border border-border rounded-xl
                                      focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                      text-text-primary placeholder-text-muted transition-all text-sm"
                            placeholder="123"
                            disabled={!!clienteSelecionado}
                          />
                        </div>
                        <div className="col-span-2">
                          <label className="block text-xs font-medium text-text-secondary mb-1">
                            Complemento
                          </label>
                          <input
                            type="text"
                            value={complementoCliente}
                            onChange={(e) => setComplementoCliente(e.target.value)}
                            className="w-full px-3 py-2 bg-background-secondary border border-border rounded-xl
                                      focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                      text-text-primary placeholder-text-muted transition-all text-sm"
                            placeholder="Apto, Bloco, etc."
                            disabled={!!clienteSelecionado}
                          />
                        </div>
                      </div>

                      <div className="grid grid-cols-2 gap-3">
                        <div>
                          <label className="block text-xs font-medium text-text-secondary mb-1">
                            Bairro
                          </label>
                          <input
                            type="text"
                            value={bairroCliente}
                            onChange={(e) => setBairroCliente(e.target.value)}
                            className="w-full px-3 py-2 bg-background-secondary border border-border rounded-xl
                                      focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                      text-text-primary placeholder-text-muted transition-all text-sm"
                            placeholder="Bairro"
                            disabled={!!clienteSelecionado}
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-text-secondary mb-1">
                            CEP
                          </label>
                          <input
                            type="text"
                            value={cepCliente}
                            onChange={(e) => setCepCliente(e.target.value)}
                            className="w-full px-3 py-2 bg-background-secondary border border-border rounded-xl
                                      focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                      text-text-primary placeholder-text-muted transition-all text-sm"
                            placeholder="00000-000"
                            disabled={!!clienteSelecionado}
                          />
                        </div>
                      </div>

                      <div className="grid grid-cols-2 gap-3">
                        <div>
                          <label className="block text-xs font-medium text-text-secondary mb-1">
                            Cidade
                          </label>
                          <input
                            type="text"
                            value={cidadeCliente}
                            onChange={(e) => setCidadeCliente(e.target.value)}
                            className="w-full px-3 py-2 bg-background-secondary border border-border rounded-xl
                                      focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                      text-text-primary placeholder-text-muted transition-all text-sm"
                            placeholder="Cidade"
                            disabled={!!clienteSelecionado}
                          />
                        </div>
                        <div>
                          <label className="block text-xs font-medium text-text-secondary mb-1">
                            UF
                          </label>
                          <input
                            type="text"
                            value={ufCliente}
                            onChange={(e) => setUfCliente(e.target.value.toUpperCase())}
                            maxLength={2}
                            className="w-full px-3 py-2 bg-background-secondary border border-border rounded-xl
                                      focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary/50
                                      text-text-primary placeholder-text-muted transition-all text-sm"
                            placeholder="SP"
                            disabled={!!clienteSelecionado}
                          />
                        </div>
                      </div>
                    </div>
                  )}

                  <p className="text-xs text-text-muted mb-2">
                    💡 Dica: Informe o nome (obrigatório) e, se quiser, telefone e/ou CPF/CNPJ. O número da comanda será gerado automaticamente.
                  </p>
                </div>

                {/* Botão - Com padding extra para garantir visibilidade */}
                <div className="pt-4 pb-6 mt-4">
                  <Button
                    type="submit"
                    disabled={isProcessing}
                    className="w-full py-3 sm:py-4 text-base sm:text-lg font-semibold"
                  >
                    Abrir Comanda
                  </Button>
                </div>
              </form>
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

      {/* Listener de teclado para o dialog de cadastrar cliente */}
      {showPerguntaCadastrarCliente && pendingAbrirComanda && (
        <KeyEnterHandler onEnter={handleSimCadastrarCliente} disabled={isProcessing} />
      )}

      {/* Pergunta: Deseja cadastrar o cliente? */}
      {showPerguntaCadastrarCliente && pendingAbrirComanda && (
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          className="fixed inset-0 z-[130] flex items-center justify-center p-4 bg-black/50"
          onClick={(e) => e.stopPropagation()}
        >
          <motion.div
            initial={{ scale: 0.95 }}
            animate={{ scale: 1 }}
            className="bg-card rounded-2xl p-6 max-w-sm w-full shadow-xl border border-border"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center gap-3 mb-4">
              <div className="w-10 h-10 rounded-xl bg-primary/20 flex items-center justify-center">
                <User className="w-5 h-5 text-primary" />
              </div>
              <h3 className="text-lg font-bold text-text-primary">
                Cadastrar cliente?
              </h3>
            </div>
            <p className="text-sm text-text-secondary mb-3">
              Deseja cadastrar este cliente no sistema?
            </p>
            <div className="bg-background-secondary rounded-xl p-3 mb-4 text-sm text-text-primary">
              <p className="font-semibold">{nomeCliente}</p>
              {cpfCliente && <p className="text-text-secondary">{cpfCliente}</p>}
              {telefoneCliente && <p className="text-text-secondary">{telefoneCliente}</p>}
            </div>
            <div className="flex gap-3">
              <Button
                type="button"
                variant="outline"
                className="flex-1"
                onClick={handleNaoCadastrarCliente}
                disabled={isProcessing}
              >
                Não
              </Button>
              <Button
                type="button"
                className="flex-1"
                onClick={handleSimCadastrarCliente}
                disabled={isProcessing}
              >
                {isProcessing ? (
                  <Loader2 className="w-4 h-4 animate-spin mx-auto" />
                ) : (
                  'Sim'
                )}
              </Button>
            </div>
          </motion.div>
        </motion.div>
      )}

      {/* Modal de Busca de Cliente */}
      <ClienteSearch
        isOpen={showClienteSearch}
        onClose={() => setShowClienteSearch(false)}
        onSelectCliente={handleSelecionarCliente}
      />
    </AnimatePresence>
  )
}

export default AbrirComandaModal

