import { AnimatePresence, motion } from 'framer-motion'
import { Printer, X } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { useToast } from '../hooks/useToast'
import { conferenciaService, ConferenciaMesa } from '../services/conferenciaService'
import { vendasService, emitenteService, Emitente } from '../services/api'
import { Button } from './ui/button'

// Estilos para impressão em formato cupom (80 colunas)
const printStyles = `
  @media print {
    @page {
      margin: 0;
      size: 80mm auto;
    }
    * {
      box-sizing: border-box;
    }
    body * {
      visibility: hidden;
    }
    .print-content, .print-content * {
      visibility: visible;
    }
    .print-content {
      position: absolute;
      left: 0;
      top: 0;
      width: 80mm;
      max-width: 80mm;
      font-family: 'Courier New', 'Courier', monospace;
      font-size: 9pt;
      line-height: 1.1;
      padding: 3mm;
      background: white;
      color: black;
    }
    .no-print {
      display: none !important;
    }
    .cupom-header {
      text-align: center;
      border-bottom: 1px dashed #000;
      padding-bottom: 3px;
      margin-bottom: 3px;
      font-weight: bold;
    }
    .cupom-line {
      padding: 1px 0;
      font-size: 9pt;
    }
    .cupom-footer {
      border-top: 1px dashed #000;
      padding-top: 3px;
      margin-top: 3px;
    }
    .cupom-separator {
      border-top: 1px dashed #000;
      margin: 3px 0;
    }
  }
`

interface ExtratoViewProps {
  nota: string
  onClose: () => void
}

const ExtratoView: React.FC<ExtratoViewProps> = ({ nota, onClose }) => {
  const [conferencia, setConferencia] = useState<ConferenciaMesa | null>(null)
  const [numeroPessoas, setNumeroPessoas] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [isImprimindo, setIsImprimindo] = useState(false)
  const [venda, setVenda] = useState<any>(null)
  const [emitente, setEmitente] = useState<Emitente | null>(null)
  const [erro, setErro] = useState<string | null>(null)
  const { showError, showSuccess: showSuccessToast } = useToast()

  useEffect(() => {
    carregarDadosExtrato()
  }, [nota])

  const carregarDadosExtrato = async () => {
    setIsLoading(true)
    setErro(null)
    try {
      // Buscar dados do emitente em paralelo
      const [vendaData, emitenteData] = await Promise.all([
        vendasService.getByNota(nota),
        emitenteService.getEmitente()
      ])
      
      console.log('📦 Dados do emitente recebidos:', emitenteData)
      
      if (!vendaData) {
        setErro('Venda não encontrada')
        showError('Erro', 'Venda não encontrada')
        return
      }

      // Salvar emitente se encontrado
      if (emitenteData) {
        console.log('✅ Emitente encontrado, salvando no state:', emitenteData)
        setEmitente(emitenteData)
      } else {
        console.warn('⚠️ Emitente não encontrado ou retornou null')
      }

      // Salvar venda para usar na impressão
      setVenda(vendaData)

      // Salvar número de pessoas da venda
      setNumeroPessoas(vendaData.numeroPessoas || null)

      // Buscar conferência usando comanda ou mesa
      let dadosConferencia: ConferenciaMesa | null = null
      
      if (vendaData.comanda) {
        dadosConferencia = await conferenciaService.getConferenciaComanda(vendaData.comanda)
      } else if (vendaData.mesa) {
        dadosConferencia = await conferenciaService.getConferenciaMesa(vendaData.mesa)
      }

      if (!dadosConferencia) {
        setErro('Não foi possível carregar os dados da comanda')
        showError('Erro', 'Não foi possível carregar os dados da comanda')
        return
      }

      setConferencia(dadosConferencia)
    } catch (error: any) {
      console.error('Erro ao carregar extrato:', error)
      const mensagemErro = error.response?.data?.message || error.message || 'Não foi possível carregar o extrato'
      setErro(mensagemErro)
      showError('Erro', mensagemErro)
    } finally {
      setIsLoading(false)
    }
  }

  const formatarPreco = (valor: number) => {
    return valor.toFixed(2).replace('.', ',')
  }

  const formatarDataHora = (dataHora: string) => {
    try {
      const date = new Date(dataHora)
      return date.toLocaleString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      })
    } catch {
      return dataHora
    }
  }

  const handleImprimir = async () => {
    if (!conferencia || !venda) {
      showError('Erro', 'Dados do extrato não carregados')
      return
    }

    setIsImprimindo(true)
    try {
      // Preparar itens para impressão
      const itensImpressao = conferencia.itens.map(item => ({
        codigo: item.codigo,
        descricao: item.descricao || 'Produto sem descrição',
        quantidade: item.qtd,
        preco: item.precoUnitario,
        observacao: undefined // Extrato não tem observações por item
      }))

      const comandaInfo = conferencia.comanda ? String(conferencia.comanda) : undefined
      const mesaInfo = conferencia.mesa ? String(conferencia.mesa) : undefined
      const clienteNome = conferencia.cliente?.nome || undefined

      await vendasService.imprimir(nota, {
        itens: itensImpressao,
        apenasNovosItens: false, // Extrato sempre imprime tudo
        comanda: comandaInfo,
        mesa: mesaInfo,
        clienteNome: clienteNome,
        subtotal: conferencia.subtotal,
        desconto: conferencia.desconto > 0 ? conferencia.desconto : undefined,
        acrescimo: conferencia.acrescimo > 0 ? conferencia.acrescimo : undefined,
        isExtrato: true
      })

      showSuccessToast('Sucesso', 'Extrato enviado para impressão')
    } catch (error: any) {
      console.error('Erro ao imprimir extrato:', error)
      showError('Erro', 'Não foi possível imprimir o extrato. Verifique a configuração da impressora.')
    } finally {
      setIsImprimindo(false)
    }
  }

  if (isLoading) {
    return (
      <div className="fixed inset-0 z-[120] flex items-center justify-center bg-black/60 backdrop-blur-sm">
        <div className="bg-card rounded-3xl p-8 text-center">
          <p className="text-lg text-text-primary">Carregando extrato...</p>
        </div>
      </div>
    )
  }

  if (erro) {
    return (
      <div className="fixed inset-0 z-[120] flex items-center justify-center bg-black/60 backdrop-blur-sm" onClick={onClose}>
        <motion.div
          initial={{ scale: 0.95, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          exit={{ scale: 0.95, opacity: 0 }}
          className="bg-card rounded-3xl p-8 max-w-md mx-4"
          onClick={(e) => e.stopPropagation()}
        >
          <div className="text-center">
            <div className="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <X className="w-8 h-8 text-red-600" />
            </div>
            <h3 className="text-xl font-bold text-text-primary mb-2">Erro ao carregar extrato</h3>
            <p className="text-text-secondary mb-6">{erro}</p>
            <Button onClick={onClose} className="w-full">
              Fechar
            </Button>
          </div>
        </motion.div>
      </div>
    )
  }

  if (!conferencia) {
    return null
  }

  return (
    <>
      <style>{printStyles}</style>
      <AnimatePresence>
        <motion.div
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          exit={{ opacity: 0 }}
          className="fixed inset-0 z-[120] flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm print:bg-white print:fixed print:inset-0"
          onClick={onClose}
        >
          <motion.div
            initial={{ scale: 0.95, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            exit={{ scale: 0.95, opacity: 0 }}
            className="bg-white w-full max-w-2xl max-h-[90vh] rounded-3xl shadow-large border border-border overflow-hidden flex flex-col print:max-w-none print:rounded-none print:shadow-none print:border-none print:max-h-none print-content"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Header - Oculto na impressão */}
            <div className="bg-primary text-primary-foreground p-6 flex justify-between items-center flex-shrink-0 no-print">
            <h2 className="text-2xl font-bold">Extrato de Comanda</h2>
            <div className="flex items-center gap-2">
              <Button
                onClick={handleImprimir}
                variant="outline"
                disabled={isImprimindo}
                className="bg-white/20 border-white/30 text-white hover:bg-white/30 disabled:opacity-50"
              >
                {isImprimindo ? (
                  <>
                    <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></div>
                    Imprimindo...
                  </>
                ) : (
                  <>
                    <Printer className="w-4 h-4 mr-2" />
                    Imprimir
                  </>
                )}
              </Button>
              <button
                onClick={onClose}
                className="p-2 hover:bg-white/20 rounded-xl transition-colors"
              >
                <X size={24} />
              </button>
            </div>
          </div>

          {/* Conteúdo do Extrato - Formato Cupom */}
          <div className="flex-1 overflow-y-auto p-6 print:p-0">
            <div className="max-w-2xl mx-auto print:max-w-none print:mx-0" style={{ fontFamily: 'monospace' }}>
              {/* Cabeçalho da Empresa */}
              {emitente ? (
                <div className="text-center mb-4 print:mb-2 border-b-2 border-dashed border-gray-400 pb-3 print:pb-2">
                  {emitente.nome && (
                    <h2 className="text-xl font-bold text-gray-900 mb-1 print:text-base print:font-bold uppercase">
                      {emitente.nome}
                    </h2>
                  )}
                  {emitente.nomeFantasia && (
                    <p className="text-sm text-gray-700 mb-2 print:text-xs">{emitente.nomeFantasia}</p>
                  )}
                  {emitente.cnpj && (
                    <p className="text-xs text-gray-600 mb-1 print:text-xs">CNPJ: {emitente.cnpj}</p>
                  )}
                  {emitente.endereco && (
                    <p className="text-xs text-gray-600 mb-1 print:text-xs">
                      {emitente.endereco}
                      {emitente.numero && `, ${emitente.numero}`}
                      {emitente.bairro && ` - ${emitente.bairro}`}
                    </p>
                  )}
                  {emitente.cidade && (
                    <p className="text-xs text-gray-600 mb-1 print:text-xs">
                      {emitente.cidade}
                      {emitente.uf && ` - ${emitente.uf}`}
                      {emitente.cep && ` - CEP: ${emitente.cep}`}
                    </p>
                  )}
                  {emitente.telefone && (
                    <p className="text-xs text-gray-600 mb-1 print:text-xs">Tel: {emitente.telefone}</p>
                  )}
                  {emitente.email && (
                    <p className="text-xs text-gray-600 print:text-xs">{emitente.email}</p>
                  )}
                </div>
              ) : (
                <div className="text-center mb-4 print:mb-2 border-b-2 border-dashed border-gray-400 pb-3 print:pb-2">
                  <p className="text-xs text-gray-500 italic">Carregando dados da empresa...</p>
                </div>
              )}

              {/* Cabeçalho - Formato Cupom */}
              <div className="text-center mb-4 print:mb-2 cupom-header">
                <h1 className="text-2xl font-bold text-gray-900 mb-1 print:text-lg print:font-bold">EXTRATO DE COMANDA</h1>
                <div className="text-xs text-gray-600 print:text-xs">{formatarDataHora(conferencia.dataHora)}</div>
              </div>

              {/* Informações da Comanda/Mesa - Formato Cupom */}
              <div className="mb-4 print:mb-2 space-y-1 print:space-y-0 text-xs print:text-xs">
                <div className="cupom-line flex justify-between">
                  <span>NOTA:</span>
                  <span className="font-bold">{conferencia.nota.padStart(6, '0')}</span>
                </div>
                {conferencia.comanda && (
                  <div className="cupom-line flex justify-between">
                    <span>COMANDA:</span>
                    <span className="font-bold">{conferencia.comanda}</span>
                  </div>
                )}
                {conferencia.mesa && (
                  <div className="cupom-line flex justify-between">
                    <span>MESA:</span>
                    <span className="font-bold">{conferencia.mesa}</span>
                  </div>
                )}
                {numeroPessoas && (
                  <div className="cupom-line flex justify-between">
                    <span>PESSOAS:</span>
                    <span className="font-bold">{numeroPessoas}</span>
                  </div>
                )}
                {conferencia.cliente && (
                  <div className="cupom-line flex justify-between">
                    <span>CLIENTE:</span>
                    <span className="font-bold text-right" style={{ maxWidth: '60%', wordBreak: 'break-word' }}>
                      {conferencia.cliente.nome.length > 30 
                        ? conferencia.cliente.nome.substring(0, 30) + '...' 
                        : conferencia.cliente.nome}
                    </span>
                  </div>
                )}
                {conferencia.garcom && (
                  <div className="cupom-line flex justify-between">
                    <span>GARCOM:</span>
                    <span className="font-bold">{conferencia.garcom}</span>
                  </div>
                )}
              </div>

              {/* Linha separadora */}
              <div className="border-t border-dashed border-gray-400 my-2 print:my-1"></div>

              {/* Itens - Formato Cupom Compacto */}
              <div className="mb-4 print:mb-2">
                <div className="text-center text-xs font-bold mb-2 print:mb-1 print:text-xs">ITENS DO PEDIDO</div>
                <div className="space-y-1 print:space-y-0">
                  {conferencia.itens.map((item, index) => {
                    const descricao = item.descricao || 'Produto sem descricao'
                    // Para cupom de 80 colunas, limitar descrição a ~40 caracteres por linha
                    const descricaoLinha1 = descricao.length > 40 ? descricao.substring(0, 40) : descricao
                    const descricaoLinha2 = descricao.length > 40 ? descricao.substring(40, 80) : null
                    
                    return (
                      <div key={index} className="cupom-line text-xs print:text-xs" style={{ marginBottom: '2px' }}>
                        <div className="flex items-start">
                          <span className="font-mono mr-1 text-gray-600" style={{ minWidth: '52px' }}>{item.horaLancamento ?? '--:--:--'}</span>
                          <span className="font-bold mr-1" style={{ minWidth: '20px' }}>{String(index + 1).padStart(2, '0')}.</span>
                          <div className="flex-1">
                            <div>{descricaoLinha1}</div>
                            {descricaoLinha2 && <div>{descricaoLinha2}</div>}
                          </div>
                        </div>
                        <div className="flex justify-between mt-0.5">
                          <span>{item.qtd}x R$ {formatarPreco(item.precoUnitario)}</span>
                          <span className="font-bold">R$ {formatarPreco(item.total)}</span>
                        </div>
                      </div>
                    )
                  })}
                </div>
              </div>

              {/* Linha separadora */}
              <div className="border-t border-dashed border-gray-400 my-2 print:my-1"></div>

              {/* Totalizadores - Formato Cupom */}
              <div className="space-y-1 print:space-y-0 text-xs print:text-xs cupom-footer">
                <div className="flex justify-between">
                  <span>SUBTOTAL:</span>
                  <span className="font-bold">R$ {formatarPreco(conferencia.subtotal)}</span>
                </div>
                {conferencia.desconto > 0 && (
                  <div className="flex justify-between">
                    <span>DESCONTO:</span>
                    <span className="font-bold">- R$ {formatarPreco(conferencia.desconto)}</span>
                  </div>
                )}
                {conferencia.acrescimo > 0 && (
                  <div className="flex justify-between">
                    <span>ACRESCIMO:</span>
                    <span className="font-bold">+ R$ {formatarPreco(conferencia.acrescimo)}</span>
                  </div>
                )}
                <div className="border-t border-dashed border-gray-400 pt-1 print:pt-0 mt-1 print:mt-0 flex justify-between font-bold text-base print:text-sm">
                  <span>TOTAL:</span>
                  <span>R$ {formatarPreco(conferencia.total)}</span>
                </div>
              </div>

              {/* Rodapé - Formato Cupom */}
              <div className="mt-4 print:mt-2 pt-2 print:pt-1 border-t border-dashed border-gray-400 text-center text-xs print:text-xs text-gray-600">
                <div>Total de itens: {conferencia.totalItens}</div>
                <div className="mt-2 print:mt-1">--------------------------------</div>
                <div className="mt-1 print:mt-0">Documento de conferencia</div>
                <div>Nao e documento fiscal</div>
              </div>
            </div>
          </div>
        </motion.div>
      </motion.div>
    </AnimatePresence>
    </>
  )
}

export default ExtratoView

