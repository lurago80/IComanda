import { AnimatePresence, motion } from 'framer-motion'
import { ArrowLeft, CheckCircle, Loader2, MessageCircle, Minus, Package, Plus, Trash2, User, UserPlus, X, ArrowRight } from 'lucide-react'
import React, { useState } from 'react'
import { useToast } from '../../hooks/useToast'
import { normalizarTexto } from '../../lib/utils'
import { vendasService, receberService, gruposService } from '../../services/api'
import { enviarWhatsApp, formatarMensagemComanda, abrirLinkWhatsApp } from '../../utils/whatsapp'
import { useCartStore } from '../../store/cartStore'
import { Cliente, ContasAberto, Grupo } from '../../types/api'
import ClienteSearch from '../ClienteSearch'
import ConfirmarImpressaoModal from '../ConfirmarImpressaoModal'
import TransferItemModal from '../TransferItemModal'
import { Button } from '../ui/button'

interface CartDrawerProps {
  onAfterSuccess?: (tipo: 'nova' | 'edicao') => void
  /** Chamado após criar venda em Caixa Rápido (PDV) – redireciona para recebimento com a nota */
  onCaixaRapidoFechar?: (nota: string) => void
  /** Chamado após salvar delivery – volta para tela de delivery */
  onDeliverySuccess?: () => void
  /** Habilita impressão automática de 2 cópias (configuração global) */
  habilitarImprimirDuasVias?: boolean
  /** Lista de grupos para verificar flag imprimirDuasVias */
  grupos?: Grupo[]
}

const CartDrawer: React.FC<CartDrawerProps> = ({ onAfterSuccess, onCaixaRapidoFechar, onDeliverySuccess, habilitarImprimirDuasVias = false, grupos = [] }) => {
  const { 
    isOpen, 
    setCartOpen, 
    items, 
    updateQuantity, 
    removeItem,
    removeItemAt,
    getTotalPrice,
    updateQuantityAt, 
    clearCart,
    comandaAtiva,
    fecharComanda,
    vendaEmEdicao,
    clienteEdicao,
    finalizarEdicao,
    getNovosItens,
    updateObservacao,
    caixaRapidoMode,
    setCaixaRapidoMode,
    deliveryMode,
    setDeliveryMode,
    deliveryClientePreSelecionado,
    setDeliveryClientePreSelecionado
  } = useCartStore()

  const [transferModalOpen, setTransferModalOpen] = useState(false)
  const [itemParaTransferir, setItemParaTransferir] = useState<{
    index: number
    itemNumero: number
    descricao: string
  } | null>(null)

  // Debug: log items quando drawer abre
  React.useEffect(() => {
    if (isOpen) {
      console.log('CartDrawer aberto - Items:', items.length, items)
      console.log('VendaEmEdicao:', vendaEmEdicao)
    }
  }, [isOpen, items, vendaEmEdicao])

  const [isProcessing, setIsProcessing] = useState(false)
  const [showSuccess, setShowSuccess] = useState(false)
  const [clienteSelecionado, setClienteSelecionado] = useState<Cliente | null>(null)
  const [showClienteSearch, setShowClienteSearch] = useState(false)
  const [showConfirmarImpressao, setShowConfirmarImpressao] = useState(false)
  const [vendaSalva, setVendaSalva] = useState<any>(null)
  const [contasAbertoCliente, setContasAbertoCliente] = useState<ContasAberto | null>(null)
  const [deliveryWhatsAppPendente, setDeliveryWhatsAppPendente] = useState<{
    nota: string;
    telefone: string;
    nomeCliente: string;
  } | null>(null)
  const [enviandoWhatsAppModal, setEnviandoWhatsAppModal] = useState(false)
  // Pagamento delivery: selecionado antes de finalizar
  const [showDeliveryPgtoModal, setShowDeliveryPgtoModal] = useState(false)
  const [deliveryFormaPgto, setDeliveryFormaPgto] = useState<string>('PIX')
  const [deliveryJaPago, setDeliveryJaPago] = useState<boolean>(false)
  const { showSuccess: showSuccessToast, showError } = useToast()

  // Carregar cliente quando entrar em modo de edição, quando drawer abrir ou quando veio cliente pré-selecionado (delivery)
  React.useEffect(() => {
    if (vendaEmEdicao && clienteEdicao) {
      setClienteSelecionado(clienteEdicao)
      console.log('✅ Cliente carregado para edição:', clienteEdicao)
    } else if (deliveryMode && deliveryClientePreSelecionado) {
      setClienteSelecionado(deliveryClientePreSelecionado)
      setDeliveryClientePreSelecionado(null)
    } else if (!vendaEmEdicao && comandaAtiva?.cliente) {
      setClienteSelecionado(comandaAtiva.cliente)
    } else if (!vendaEmEdicao && !comandaAtiva && !deliveryMode) {
      setClienteSelecionado(null)
    }
  }, [vendaEmEdicao, clienteEdicao, comandaAtiva, isOpen, deliveryMode, deliveryClientePreSelecionado])

  const totalPrice = getTotalPrice()

  const formatarPreco = (preco: number) => preco.toFixed(2).replace('.', ',')

  const formatarDescricao = (descricao?: string, fallback?: string) => {
    // Função auxiliar para limpar e normalizar texto
    const limparTexto = (texto: string): string => {
      if (!texto) return ''
      // Remove quebras de linha, tabs e outros caracteres de controle
      return texto
        .replace(/[\r\n\t]+/g, ' ')
        .replace(/\s+/g, ' ')
        .trim()
    }

    const descLimpa = descricao ? limparTexto(descricao) : ''
    if (descLimpa.length > 0) {
      return descLimpa
    }
    
    const fallbackLimpo = fallback ? limparTexto(fallback) : ''
    if (fallbackLimpo.length > 0) {
      return fallbackLimpo
    }
    
    return 'Produto sem descrição'
  }

  const handleFinalizarPedido = async () => {
    // Filtrar itens válidos antes de verificar
    const itensValidos = items.filter(item => item && item.produto && item.produto.id && item.quantidade > 0)
    
    if (itensValidos.length === 0) {
      showError('Carrinho vazio', 'Adicione pelo menos um item ao carrinho antes de finalizar.')
      return
    }

    // Em modo de edição, não precisa de comanda ativa (a venda já existe)
    // Caixa Rápido (PDV) e Delivery não usam comanda
    if (!vendaEmEdicao && !comandaAtiva && !caixaRapidoMode && !deliveryMode) {
      showError(
        'Comanda obrigatória', 
        'Por favor, abra uma comanda antes de finalizar o pedido.'
      )
      return
    }

    // Delivery: cliente obrigatório
    if (deliveryMode && !clienteSelecionado) {
      showError(
        'Cliente obrigatório', 
        'Selecione o cliente para o pedido de delivery.'
      )
      return
    }

    // Caixa Rápido: se impressão 2 vias habilitada, confirmar antes; caso contrário, finalizar direto
    if (caixaRapidoMode) {
      if (habilitarImprimirDuasVias) {
        setShowConfirmarImpressao(true)
        return
      }
      await handleConfirmarFinalizar(false, false)
      return
    }

    // Delivery em modo de edição: não precisa de pagamento, apenas confirmar impressão e salvar
    if (deliveryMode && vendaEmEdicao) {
      setShowConfirmarImpressao(true)
      return
    }

    // Delivery novo: pedir forma de pagamento antes de finalizar
    if (deliveryMode) {
      setShowDeliveryPgtoModal(true)
      return
    }

    // Mostrar modal de confirmação de impressão
    setShowConfirmarImpressao(true)
  }

  const handleConfirmarFinalizar = async (imprimir: boolean, imprimirCompleto?: boolean, vias: 1 | 2 = 1) => {
    // Busca grupos frescos da API para garantir que imprimirDuasVias esteja atualizado
    let gruposAtuais = grupos
    if (habilitarImprimirDuasVias) {
      try {
        gruposAtuais = await gruposService.getTodosComQuantidade()
      } catch (_) {
        // fallback para o prop se a busca falhar
      }
    }
    // Determina se deve imprimir 2 cópias automaticamente: config global ON + algum item é de grupo com imprimirDuasVias
    const shouldPrint2Copies = habilitarImprimirDuasVias &&
      gruposAtuais.some(g => g.imprimirDuasVias && items.some(i => i.produto.grupo === g.id));
    setShowConfirmarImpressao(false)
    setIsProcessing(true)
    
    try {
      // Recupera o usuário logado
      const usuarioLogadoStr = localStorage.getItem('usuario_logado');
      let operadorId = 1; // Padrão
      
      if (usuarioLogadoStr) {
        try {
          const usuarioInfo = JSON.parse(usuarioLogadoStr);
          operadorId = usuarioInfo.id || 1;
        } catch {
          // Se falhar o parse, mantém o padrão
          operadorId = 1;
        }
      }

      // Preparar request - em modo de edição, usar dados da venda existente
      // Se não houver cliente selecionado em modo de edição, usar 0 (cliente padrão)
      const itensFiltrados = items.filter(item => item && item.produto && item.produto.id && item.quantidade > 0)
      
      if (itensFiltrados.length === 0) {
        showError('Itens inválidos', 'Não há itens válidos no carrinho para finalizar.')
        setIsProcessing(false)
        return
      }
      
      console.log('📦 Preparando request:', {
        vendaEmEdicao: !!vendaEmEdicao,
        clienteSelecionado: clienteSelecionado?.id,
        itensCount: itensFiltrados.length,
        operadorId,
        imprimirCompleto: !!imprimirCompleto
      })
      
      const vendaRequest = vendaEmEdicao ? {
        cliente: clienteSelecionado?.id || 0,
        nomeCliente: clienteSelecionado?.nomeCompleto || '',
        cpfCnpjCliente: clienteSelecionado?.documento || '',
        operador: operadorId,
        vendedor: operadorId,
        comanda: vendaEmEdicao.comanda ?? undefined,
        mesa: vendaEmEdicao.mesa ?? undefined,
        numeroPessoas: undefined,
        ...(deliveryMode && { origem: 'DL' as const }),
        itens: itensFiltrados.map(item => ({
          codigo: item.produto.id,
          qtd: item.quantidade,
          preco: item.precoOverride ?? item.produto.precoVenda,
          observacao: item.observacoes || undefined,
          adicionadoEm: item.adicionadoEm
        }))
      } : caixaRapidoMode ? {
        cliente: 0,
        nomeCliente: '',
        cpfCnpjCliente: '',
        operador: operadorId,
        vendedor: operadorId,
        // PDV rápido: origem PD, não envia comanda
        origem: 'PD',
        comanda: undefined,
        mesa: undefined,
        numeroPessoas: undefined,
        itens: itensFiltrados.map(item => ({
          codigo: item.produto.id,
          qtd: item.quantidade,
          preco: item.precoOverride ?? item.produto.precoVenda,
          observacao: item.observacoes || undefined,
          adicionadoEm: item.adicionadoEm
        }))
      } : deliveryMode ? {
        cliente: clienteSelecionado!.id,
        nomeCliente: clienteSelecionado!.nomeCompleto || '',
        cpfCnpjCliente: clienteSelecionado!.documento || '',
        operador: operadorId,
        vendedor: operadorId,
        comanda: undefined,
        mesa: undefined,
        numeroPessoas: undefined,
        origem: 'DL',
        formasPgto: deliveryFormaPgto,
        jaPagoDelivery: deliveryJaPago,
        itens: itensFiltrados.map(item => ({
          codigo: item.produto.id,
          qtd: item.quantidade,
          preco: item.precoOverride ?? item.produto.precoVenda,
          observacao: item.observacoes || undefined,
          adicionadoEm: item.adicionadoEm
        }))
      } : {
        cliente: comandaAtiva!.cliente?.id ?? 0,
        nomeCliente: comandaAtiva!.cliente?.nomeCompleto || comandaAtiva!.cliente?.nome || comandaAtiva!.nomeClienteExibicao || '',
        cpfCnpjCliente: comandaAtiva!.cliente?.documento || '',
        operador: operadorId,
        vendedor: operadorId,
        comanda: comandaAtiva!.numeroComanda,
        mesa: comandaAtiva!.numeroMesa,
        numeroPessoas: comandaAtiva!.numeroPessoas,
        itens: itensFiltrados.map(item => ({
          codigo: item.produto.id,
          qtd: item.quantidade,
          preco: item.precoOverride ?? item.produto.precoVenda,
          observacao: item.observacoes || undefined,
          adicionadoEm: item.adicionadoEm
        }))
      }

      // Usar atualizar se estiver em modo de edição, criar se for nova venda
      // Usar a nota original do vendaEmEdicao (que vem do backend) - não formatar
      // O backend já faz a normalização necessária
      let venda
      if (vendaEmEdicao) {
        // Usar a nota exatamente como está no vendaEmEdicao (já vem formatada do backend)
        const notaParaAtualizar = vendaEmEdicao.nota
        console.log('🔄 Atualizando venda com nota:', notaParaAtualizar, 'vendaEmEdicao:', vendaEmEdicao)
        venda = await vendasService.atualizar(notaParaAtualizar, vendaRequest)
      } else {
        venda = await vendasService.criar(vendaRequest)
      }
      
      // Caixa Rápido (PDV): redirecionar para recebimento para formas de pagamento e imprimir cupom
      if (caixaRapidoMode && onCaixaRapidoFechar) {
        // Imprimir ticket de cozinha/bar se habilitarImprimirDuasVias e usuário confirmou impressão
        if (imprimir && habilitarImprimirDuasVias) {
          try {
            const itensNormaisPDV = itensFiltrados.filter(item =>
              !gruposAtuais.some(g => g.imprimirDuasVias && g.id === item.produto.grupo)
            )
            const itensCozinhaPDV = itensFiltrados.filter(item =>
              gruposAtuais.some(g => g.imprimirDuasVias && g.id === item.produto.grupo)
            )
            // 1ª via: TODOS os itens (comanda completa — sem título, cópia do garçom)
            // Só imprime se houver itens de grupos SEM imprimirDuasVias.
            // Se todos os itens são de grupos com 2 vias, pula a 1ª via.
            if (itensNormaisPDV.length > 0 && itensFiltrados.length > 0) {
              await vendasService.imprimir(venda.nota, {
                itens: itensFiltrados.map(item => ({
                  codigo: item.produto.id,
                  descricao: item.produto.descricao || 'Produto sem descrição',
                  quantidade: item.quantidade,
                  preco: item.precoOverride ?? item.produto.precoVenda,
                  observacao: item.observacoes || undefined
                })),
                apenasNovosItens: false,
                comanda: undefined,
                mesa: undefined,
                clienteNome: undefined
              })
            }
            // 2ª via: itens de grupos COM imprimirDuasVias — VIA CLIENTE (volta para a mesa)
            if (itensCozinhaPDV.length > 0) {
              const itensCozinhaPDVMapped = itensCozinhaPDV.map(item => ({
                codigo: item.produto.id,
                descricao: item.produto.descricao || 'Produto sem descrição',
                quantidade: item.quantidade,
                preco: item.precoOverride ?? item.produto.precoVenda,
                observacao: item.observacoes || undefined
              }))
              await vendasService.imprimir(venda.nota, {
                itens: itensCozinhaPDVMapped,
                apenasNovosItens: false,
                comanda: undefined as string | undefined,
                mesa: undefined as string | undefined,
                clienteNome: undefined as string | undefined,
                tituloSecao: '**** VIA CLIENTE ****'
              })
              // 3ª via: itens de grupos COM imprimirDuasVias — COZINHA (fica na cozinha/bar)
              await vendasService.imprimir(venda.nota, {
                itens: itensCozinhaPDVMapped,
                apenasNovosItens: false,
                comanda: undefined as string | undefined,
                mesa: undefined as string | undefined,
                clienteNome: undefined as string | undefined,
                tituloSecao: '**** COZINHA ****'
              })
            }
          } catch (printErr) {
            console.error('Erro ao imprimir ticket de cozinha/bar:', printErr)
          }
        }
        clearCart()
        setCartOpen(false)
        setClienteSelecionado(null)
        setVendaSalva(null)
        onCaixaRapidoFechar(venda.nota)
        setIsProcessing(false)
        return
      }

      // Delivery em edição: imprimir se solicitado e voltar para tela de delivery
      if (deliveryMode && vendaEmEdicao) {
        if (imprimir) {
          try {
            await vendasService.imprimir(venda.nota, {
              itens: itensFiltrados.map(item => ({
                codigo: item.produto.id,
                descricao: item.produto.descricao || 'Produto sem descrição',
                quantidade: item.quantidade,
                preco: item.precoOverride ?? item.produto.precoVenda,
                observacao: item.observacoes || undefined
              })),
              clienteNome: clienteSelecionado?.nomeCompleto || '',
              comanda: undefined,
              mesa: undefined,
              isCupomDelivery: true,
              apenasNovosItens: false,
              enderecoEntrega: clienteSelecionado?.enderecoCompleto || undefined,
              pontoReferencia: clienteSelecionado?.complemento1 || undefined,
              telefoneEntrega: clienteSelecionado?.celular || clienteSelecionado?.telefone || undefined
            })
          } catch (printErr) {
            console.error('Erro ao imprimir delivery editado:', printErr)
          }
        }
        clearCart()
        finalizarEdicao()
        setDeliveryMode(false)
        setClienteSelecionado(null)
        setVendaSalva(null)
        setCartOpen(false)
        setIsProcessing(false)
        setTimeout(() => { if (onDeliverySuccess) onDeliverySuccess() }, 300)
        return
      }

      // Delivery novo: salvar e voltar para tela de delivery (não vai para recebimento nem modal de sucesso)
      if (deliveryMode) {
        // Capturar telefone e nome ANTES de limpar o estado
        const telefoneDelivery = clienteSelecionado?.telefone || clienteSelecionado?.celular || ''
        const nomeClienteDelivery = (clienteSelecionado as any)?.nomeCompleto || clienteSelecionado?.nome || ''
        const notaDelivery = venda.nota

        clearCart()
        setCartOpen(false)
        setClienteSelecionado(null)
        setVendaSalva(null)
        setDeliveryMode(false)
        setIsProcessing(false)

        if (telefoneDelivery) {
          // Tem telefone: mostrar modal perguntando se quer enviar WhatsApp
          setDeliveryWhatsAppPendente({ nota: notaDelivery, telefone: telefoneDelivery, nomeCliente: nomeClienteDelivery })
        } else {
          // Sem telefone: voltar direto para tela de delivery
          setTimeout(() => { if (onDeliverySuccess) onDeliverySuccess() }, 300)
        }
        return
      }

      // Determinar tipo de sucesso
      const tipoSucesso: 'nova' | 'edicao' = vendaEmEdicao ? 'edicao' : 'nova'
      
      // Mensagens para toast
      const msgComanda = vendaEmEdicao 
        ? (vendaEmEdicao.comanda ? ` - Comanda ${vendaEmEdicao.comanda}` : '')
        : (comandaAtiva?.numeroComanda ? ` - Comanda ${comandaAtiva.numeroComanda}` : '')
      const msgMesa = vendaEmEdicao
        ? (vendaEmEdicao.mesa ? ` - Mesa ${vendaEmEdicao.mesa}` : '')
        : (comandaAtiva?.numeroMesa ? ` - Mesa ${comandaAtiva.numeroMesa}` : '')
      
      // Salvar venda para possível impressão
      setVendaSalva(venda)
      
      // Se confirmou impressão, imprimir
      if (imprimir) {
        try {
          const itensParaImprimir = vendaEmEdicao && !imprimirCompleto ? getNovosItens() : items
          
          if (itensParaImprimir.length > 0) {
            const comandaInfo = vendaEmEdicao 
              ? (vendaEmEdicao.comanda ? String(vendaEmEdicao.comanda) : undefined)
              : (comandaAtiva?.numeroComanda ? String(comandaAtiva.numeroComanda) : undefined)
            
            const mesaInfo = vendaEmEdicao
              ? (vendaEmEdicao.mesa ? String(vendaEmEdicao.mesa) : undefined)
              : (comandaAtiva?.numeroMesa ? String(comandaAtiva.numeroMesa) : undefined)
            
            const clienteNome = vendaEmEdicao
              ? (clienteSelecionado?.nomeCompleto || '')
              : (comandaAtiva?.cliente?.nomeCompleto || comandaAtiva?.cliente?.nome || comandaAtiva?.nomeClienteExibicao || '')
            
            // Separar itens: normais (sem flag) e cozinha (com flag)
            const itensNormais = itensParaImprimir.filter(item =>
              !gruposAtuais.some(g => g.imprimirDuasVias && g.id === item.produto.grupo)
            )
            const itensCozinha = itensParaImprimir.filter(item =>
              gruposAtuais.some(g => g.imprimirDuasVias && g.id === item.produto.grupo)
            )

            if (shouldPrint2Copies) {
              // 1ª via: TODOS os itens (comanda completa — sem título, cópia do garçom)
              // Só imprime se houver itens de grupos SEM imprimirDuasVias.
              // Se todos os itens são de grupos com 2 vias, pula a 1ª via.
              if (itensNormais.length > 0 && itensParaImprimir.length > 0) {
                await vendasService.imprimir(venda.nota, {
                  itens: itensParaImprimir.map(item => ({
                    codigo: item.produto.id,
                    descricao: item.produto.descricao || 'Produto sem descrição',
                    quantidade: item.quantidade,
                    preco: item.precoOverride ?? item.produto.precoVenda,
                    observacao: item.observacoes || undefined
                  })),
                  apenasNovosItens: vendaEmEdicao ? !imprimirCompleto : false,
                  comanda: comandaInfo,
                  mesa: mesaInfo,
                  clienteNome: clienteNome || undefined
                })
              }
              // 2ª via: itens de grupos COM imprimirDuasVias — VIA CLIENTE (volta para a mesa)
              if (itensCozinha.length > 0) {
                const itensCozinhaMapped = itensCozinha.map(item => ({
                  codigo: item.produto.id,
                  descricao: item.produto.descricao || 'Produto sem descrição',
                  quantidade: item.quantidade,
                  preco: item.precoOverride ?? item.produto.precoVenda,
                  observacao: item.observacoes || undefined
                }))
                await vendasService.imprimir(venda.nota, {
                  itens: itensCozinhaMapped,
                  apenasNovosItens: (vendaEmEdicao ? !imprimirCompleto : false) as boolean,
                  comanda: comandaInfo,
                  mesa: mesaInfo,
                  clienteNome: clienteNome || undefined,
                  tituloSecao: '**** VIA CLIENTE ****'
                })
                // 3ª via: itens de grupos COM imprimirDuasVias — COZINHA (fica na cozinha/bar)
                await vendasService.imprimir(venda.nota, {
                  itens: itensCozinhaMapped,
                  apenasNovosItens: (vendaEmEdicao ? !imprimirCompleto : false) as boolean,
                  comanda: comandaInfo,
                  mesa: mesaInfo,
                  clienteNome: clienteNome || undefined,
                  tituloSecao: '**** COZINHA ****'
                })
              }
            } else {
              // Sem 2 vias automático: imprimir todos os itens
              await vendasService.imprimir(venda.nota, {
                itens: itensParaImprimir.map(item => ({
                  codigo: item.produto.id,
                  descricao: item.produto.descricao || 'Produto sem descrição',
                  quantidade: item.quantidade,
                  preco: item.precoOverride ?? item.produto.precoVenda,
                  observacao: item.observacoes || undefined
                })),
                apenasNovosItens: vendaEmEdicao ? !imprimirCompleto : false,
                comanda: comandaInfo,
                mesa: mesaInfo,
                clienteNome: clienteNome || undefined
              })
              // 2ª via manual (usuário escolheu 2 vias explicitamente)
              if (vias === 2) {
                await vendasService.imprimir(venda.nota, {
                  itens: itensParaImprimir.map(item => ({
                    codigo: item.produto.id,
                    descricao: item.produto.descricao || 'Produto sem descrição',
                    quantidade: item.quantidade,
                    preco: item.precoOverride ?? item.produto.precoVenda,
                    observacao: item.observacoes || undefined
                  })),
                  apenasNovosItens: vendaEmEdicao ? !imprimirCompleto : false,
                  comanda: comandaInfo,
                  mesa: mesaInfo,
                  clienteNome: clienteNome || undefined
                })
              }
            }
            
            console.log('✅ Impressão solicitada com sucesso para pedido', venda.nota, 'imprimirCompleto:', !!imprimirCompleto)
          }
        } catch (error) {
          console.error('❌ Erro ao imprimir:', error)
          // Não bloquear o fluxo se a impressão falhar - apenas logar o erro
          showError('Aviso', 'Pedido salvo, mas não foi possível imprimir. Verifique a configuração da impressora.')
        }
      }
      
      // Mostrar toast de sucesso
      showSuccessToast(
        tipoSucesso === 'edicao' ? 'Pedido atualizado!' : 'Pedido finalizado!',
        `Pedido #${venda.nota}${msgComanda}${msgMesa}`
      )
      
      // Limpar estados primeiro
      if (vendaEmEdicao) {
        finalizarEdicao()
      } else {
        clearCart()
        fecharComanda()
      }
      if (deliveryMode) setDeliveryMode(false)
      setClienteSelecionado(null)
      setVendaSalva(null)
      
      // Fechar drawer imediatamente para liberar a tela
      setCartOpen(false)
      
      // Chamar onAfterSuccess para exibir o SuccessActionSheet
      // Aguardar um pouco para garantir que o drawer fechou completamente
      // e que a animação de fechamento terminou
      console.log('✅ Pedido salvo com sucesso, chamando onAfterSuccess com tipo:', tipoSucesso)
      setTimeout(() => {
        console.log('📢 Chamando onAfterSuccess...')
        if (onAfterSuccess) {
          onAfterSuccess(tipoSucesso)
          console.log('✅ onAfterSuccess chamado com sucesso')
        } else {
          console.warn('⚠️ onAfterSuccess não está definido')
        }
      }, 500)
      
    } catch (error: any) {
      console.error('❌ Erro ao finalizar pedido:', error)
      console.error('📋 Detalhes do erro:', {
        status: error?.response?.status,
        data: error?.response?.data,
        message: error?.message,
        stack: error?.stack
      })
      
      // Log mais detalhado do erro do backend
      if (error?.response?.data) {
        console.error('📦 Resposta do servidor:', JSON.stringify(error.response.data, null, 2))
      }
      
      // Mensagem de erro mais específica
      let mensagemErro = 'Não foi possível processar o pedido. Tente novamente.'
      
      if (error?.response?.status === 404) {
        mensagemErro = vendaEmEdicao 
          ? `Venda ${vendaEmEdicao.nota} não encontrada. Verifique se a venda existe e está com status ABERTO.`
          : 'Recurso não encontrado. Verifique os dados e tente novamente.'
      } else if (error?.response?.status === 500) {
        // Erro 500 - mostrar detalhes do backend se disponível
        const detalhes = error?.response?.data?.detalhes || error?.response?.data?.message || 'Erro interno do servidor'
        mensagemErro = vendaEmEdicao
          ? `Erro ao atualizar pedido: ${detalhes}. Verifique os logs do servidor.`
          : `Erro ao finalizar pedido: ${detalhes}. Verifique os logs do servidor.`
      } else if (error?.response?.data?.message) {
        mensagemErro = error.response.data.message
      } else if (error?.response?.data?.detalhes) {
        mensagemErro = error.response.data.detalhes
      } else if (error?.message) {
        mensagemErro = error.message
      }
      
      showError(
        vendaEmEdicao ? 'Erro ao atualizar pedido' : 'Erro ao finalizar pedido', 
        mensagemErro
      )
    } finally {
      setIsProcessing(false)
    }
  }

  const handleEnviarWhatsAppDelivery = async () => {
    if (!deliveryWhatsAppPendente) return
    setEnviandoWhatsAppModal(true)
    try {
      const vendaCompleta = await vendasService.getByNota(deliveryWhatsAppPendente.nota)
      const vendaComItens = vendaCompleta.itens?.length
        ? vendaCompleta
        : { ...vendaCompleta, itens: await vendasService.getItensByNota(deliveryWhatsAppPendente.nota) }
      const mensagem = formatarMensagemComanda(vendaComItens as any)
      const resultado = await enviarWhatsApp(deliveryWhatsAppPendente.telefone, mensagem)
      if (resultado.method === 'sent') {
        showSuccessToast('WhatsApp enviado', 'Mensagem enviada com sucesso para o cliente!')
      } else if (resultado.method === 'link' && resultado.link) {
        abrirLinkWhatsApp(resultado.link)
        showSuccessToast('WhatsApp', 'Envie a mensagem na janela que abriu.')
      } else {
        showError('WhatsApp', 'Não foi possível enviar. Use o link do WhatsApp se abriu.')
      }
    } catch (err: any) {
      showError('WhatsApp', err?.message || 'Não foi possível enviar a mensagem.')
    } finally {
      setEnviandoWhatsAppModal(false)
      setDeliveryWhatsAppPendente(null)
      setTimeout(() => { if (onDeliverySuccess) onDeliverySuccess() }, 200)
    }
  }

  const handleIgnorarWhatsAppDelivery = () => {
    setDeliveryWhatsAppPendente(null)
    setTimeout(() => { if (onDeliverySuccess) onDeliverySuccess() }, 200)
  }

  return (
    <AnimatePresence>
      {isOpen && (
        <>
          {/* Drawer - Fullscreen em todos os modos */}
          <motion.div
            initial={{ y: '100%' }}
            animate={{ y: 0 }}
            exit={{ y: '100%' }}
            transition={{ type: "spring", stiffness: 300, damping: 30 }}
            className="fixed inset-0 bg-card z-[100] flex flex-col overflow-hidden"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Header */}
            <div className={`flex items-center justify-between ${vendaEmEdicao ? 'p-2 sm:p-3' : 'p-2 sm:p-3'} border-b border-border flex-shrink-0`}>
              {vendaEmEdicao ? (
                <>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => setCartOpen(false)}
                      className="text-text-secondary hover:text-text-primary hover:bg-background-secondary rounded-xl"
                      title="Voltar para tela principal"
                    >
                      <ArrowLeft className="w-5 h-5" />
                    </Button>
                    <h2 className="text-base font-bold text-text-primary">
                      Editar Pedido
                    </h2>
                  </div>
                  <Button
                    variant="outline"
                    onClick={() => setCartOpen(false)}
                    className="text-sm font-semibold border-border hover:bg-background-secondary rounded-xl px-4"
                  >
                    <X className="w-4 h-4 mr-2" />
                    Cancelar
                  </Button>
                </>
              ) : (
                <>
                  <div className="flex items-center gap-2">
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => setCartOpen(false)}
                      className="text-text-secondary hover:text-text-primary hover:bg-background-secondary rounded-xl"
                      title="Fechar carrinho"
                    >
                      <ArrowLeft className="w-5 h-5" />
                    </Button>
                    <h2 className="text-xl font-bold text-text-primary">
                      Carrinho
                    </h2>
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setCartOpen(false)}
                    className="text-text-secondary hover:text-text-primary"
                  >
                    <X className="w-5 h-5" />
                  </Button>
                </>
              )}
            </div>

            {/* Banner de Edição - Compacto para Mobile */}
            {vendaEmEdicao && (
              <div className="mx-3 mt-1 p-1.5 bg-warning/15 border border-warning/50 rounded-xl">
                <div className="flex items-center gap-2">
                  <div className="w-5 h-5 bg-warning/20 rounded-md flex items-center justify-center flex-shrink-0">
                    <svg xmlns="http://www.w3.org/2000/svg" className="h-3 w-3 text-warning" viewBox="0 0 20 20" fill="currentColor">
                      <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                    </svg>
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="font-bold text-[11px] text-warning leading-none">MODO DE EDIÇÃO</p>
                    <p className="text-[11px] text-text-primary leading-tight mt-0.5 truncate">
                      {vendaEmEdicao.mesa && `Mesa ${vendaEmEdicao.mesa}`}
                      {vendaEmEdicao.comanda && ` • Comanda ${String(vendaEmEdicao.comanda).padStart(6, '0')}`}
                      {vendaEmEdicao.nota && ` • Nota: ${vendaEmEdicao.nota}`}
                    </p>
                  </div>
                  <Button
                    onClick={() => setCartOpen(false)}
                    variant="outline"
                    size="sm"
                    className="bg-warning/10 border-warning/50 text-warning hover:bg-warning/20 text-[11px] px-2 h-6 flex-shrink-0"
                  >
                    <Plus className="w-3 h-3 mr-1" />
                    <span>Adicionar</span>
                  </Button>
                </div>
              </div>
            )}

            {/* Lista de Itens */}
            <div className={`flex-1 overflow-y-auto overflow-x-hidden ${vendaEmEdicao ? 'px-3 sm:px-4 py-2 sm:py-3' : 'px-4 sm:px-6 py-2'} w-full`}>
              {/* Informações da Comanda Ativa - Dentro da área de scroll, no topo */}
              {items.length > 0 && comandaAtiva && !vendaEmEdicao && (
                <div className="mb-4 p-3 bg-success/5 border border-success/20 rounded-xl flex-shrink-0">
                  <div className="flex items-center gap-2 text-xs sm:text-sm flex-wrap">
                    <span className="text-text-secondary">Comanda:</span>
                    <span className="font-bold text-text-primary">#{comandaAtiva.numeroComanda}</span>
                    {comandaAtiva.numeroMesa && (
                      <>
                        <span className="text-text-muted">•</span>
                        <span className="text-text-secondary">Mesa:</span>
                        <span className="font-bold text-text-primary">{comandaAtiva.numeroMesa}</span>
                      </>
                    )}
                    {comandaAtiva.numeroPessoas && (
                      <>
                        <span className="text-text-muted">•</span>
                        <span className="text-text-secondary">Pessoas:</span>
                        <span className="font-bold text-text-primary">{comandaAtiva.numeroPessoas}</span>
                      </>
                    )}
                  </div>
                  {(comandaAtiva.cliente || comandaAtiva.nomeClienteExibicao) && (
                    <div className="mt-2 pt-2 border-t border-success/20">
                      <span className="text-xs text-text-secondary">Cliente: </span>
                      <span className="text-xs font-semibold text-text-primary">
                        {comandaAtiva.cliente?.nomeCompleto || comandaAtiva.cliente?.nome || comandaAtiva.nomeClienteExibicao || ''}
                      </span>
                    </div>
                  )}
                </div>
              )}
              {items.length === 0 ? (
                <div className="text-center py-12">
                  <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Package className="w-8 h-8 text-primary" />
                  </div>
                  <h3 className="text-lg font-medium text-text-primary mb-2">Carrinho vazio</h3>
                  <p className="text-text-secondary mb-6">Adicione produtos para começar seu pedido</p>
                  
                  {/* Botão para adicionar itens - especialmente importante em modo de edição */}
                  {vendaEmEdicao && (
                    <Button
                      onClick={() => setCartOpen(false)}
                      className="bg-primary text-primary-foreground px-6 py-3 rounded-xl font-semibold hover:bg-primary/90 transition-colors shadow-lg"
                    >
                      <Plus className="w-5 h-5 mr-2 inline" />
                      Adicionar Itens
                    </Button>
                  )}
                </div>
              ) : (
                <div className={`space-y-2 sm:space-y-3 ${vendaEmEdicao ? 'pb-2 sm:pb-4' : ''}`}>
                  {items.map((item, index) => {
                    // Pular itens inválidos mas manter índice real para updateObservacao
                    if (!item || !item.produto || !item.produto.id) {
                      console.warn(`⚠️ Item inválido no índice ${index}:`, item)
                      return null
                    }
                    // Key única usando índice + id do produto + quantidade para garantir unicidade total
                    const uniqueKey = `cart-item-${index}-${item.produto.id}-${item.quantidade}-${item.precoOverride || item.produto.precoVenda}`
                    return (
                    <div
                      key={uniqueKey}
                      className="bg-card-secondary rounded-xl sm:rounded-2xl p-3 sm:p-4 border border-border hover:border-primary/30 transition-colors w-full"
                    >
                      <div className="flex items-start gap-3 sm:gap-4 w-full">
                        {/* Imagem do Produto - Responsiva e com posicionamento fixo */}
                        <div className="w-12 h-12 sm:w-14 sm:h-14 bg-primary/10 rounded-xl flex items-center justify-center border border-primary/20 flex-shrink-0">
                          <Package className="w-6 h-6 sm:w-7 sm:h-7 text-primary flex-shrink-0" />
                        </div>

                          {/* Informações do Produto - Layout melhorado e responsivo */}
                          <div className="flex-1 min-w-0 overflow-hidden">
                            <h3 className="font-semibold text-sm sm:text-base text-text-primary mb-2 break-words leading-tight whitespace-normal">
                              {formatarDescricao(item.produto.descricao, item.produto.caracteristica)}
                            </h3>
                          <div className="flex items-center gap-3 flex-wrap mb-3">
                            <p className="text-sm text-text-secondary">
                              R$ {formatarPreco(item.precoOverride ?? item.produto.precoVenda)} cada
                            </p>
                            {item.produto.caracteristica && item.produto.descricao && item.produto.caracteristica !== item.produto.descricao && (
                              <span className="text-xs text-text-muted italic">
                                {item.produto.caracteristica}
                              </span>
                            )}
                          </div>

                          {/* Campo de Observação */}
                          <div className="mt-2 mb-3 bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-800/50 rounded-xl p-2.5">
                            <label className="flex items-center gap-1.5 text-xs font-medium text-amber-700 dark:text-amber-400 mb-1.5">
                              <MessageCircle className="w-3.5 h-3.5 flex-shrink-0" />
                              Observação
                            </label>
                            <textarea
                              placeholder="Ex: sem sal, bem passado, tirar a cebola..."
                              value={item.observacoes || ''}
                              onChange={(e) => updateObservacao(index, e.target.value)}
                              rows={5}
                              className="w-full px-3 py-2 text-sm rounded-lg border border-amber-200 dark:border-amber-700 bg-white dark:bg-amber-950/50 text-text-primary placeholder-text-muted focus:outline-none focus:ring-2 focus:ring-amber-400/60 focus:border-amber-400 resize-none leading-snug"
                              maxLength={150}
                            />
                            <div className="flex justify-end mt-1">
                              <span className="text-[10px] text-amber-500 dark:text-amber-600">
                                {(item.observacoes || '').length}/150
                              </span>
                            </div>
                          </div>

                          {/* Controles de Quantidade e Ações - Responsivo */}
                          <div className="flex items-center justify-between gap-2 sm:gap-3 mt-3 w-full">
                            <div className="flex items-center space-x-2 flex-shrink-0">
                              <Button
                                variant="outline"
                                size="icon"
                                onClick={() => updateQuantityAt(index, item.quantidade - 1)}
                                className="w-8 h-8 sm:w-9 sm:h-9 rounded-lg border-border hover:bg-background-tertiary flex-shrink-0"
                              >
                                <Minus className="w-3 h-3 sm:w-4 sm:h-4" />
                              </Button>
                              
                              <span className="w-8 sm:w-10 text-center font-bold text-sm sm:text-base text-text-primary flex-shrink-0">
                                {item.quantidade}
                              </span>
                              
                              <Button
                                size="icon"
                                onClick={() => updateQuantityAt(index, item.quantidade + 1)}
                                className="w-8 h-8 sm:w-9 sm:h-9 rounded-lg bg-primary hover:bg-primary/90 text-primary-foreground flex-shrink-0"
                              >
                                <Plus className="w-3 h-3 sm:w-4 sm:h-4" />
                              </Button>
                            </div>

                            {/* Preço Total e Ações */}
                            <div className="flex items-center gap-2 sm:gap-3 flex-shrink-0">
                              <div className="text-right">
                                <p className="font-bold text-sm sm:text-base text-text-primary whitespace-nowrap">
                                  R$ {formatarPreco((item.precoOverride ?? item.produto.precoVenda) * item.quantidade)}
                                </p>
                              </div>
                              {/* Botão Transferir - apenas em modo de edição */}
                              {vendaEmEdicao && (
                                <Button
                                  variant="outline"
                                  size="icon"
                                  onClick={async () => {
                                    // Buscar venda atualizada para obter número correto do item
                                    try {
                                      const venda = await vendasService.getByNota(vendaEmEdicao.nota)
                                      // Encontrar o item correspondente na venda
                                      const itemVenda = venda.itens?.find((iv, idx) => {
                                        // Tentar encontrar pelo código e posição
                                        return iv.codigo === item.produto.id && idx === index
                                      })
                                      const itemNumero = itemVenda?.item || (index + 1)
                                      
                                      setItemParaTransferir({
                                        index,
                                        itemNumero,
                                        descricao: formatarDescricao(item.produto.descricao, item.produto.caracteristica)
                                      })
                                      setTransferModalOpen(true)
                                    } catch (error) {
                                      console.error('Erro ao buscar venda:', error)
                                      // Usar índice + 1 como fallback
                                      setItemParaTransferir({
                                        index,
                                        itemNumero: index + 1,
                                        descricao: formatarDescricao(item.produto.descricao, item.produto.caracteristica)
                                      })
                                      setTransferModalOpen(true)
                                    }
                                  }}
                                  className="w-8 h-8 sm:w-9 sm:h-9 rounded-lg text-primary border-primary/30 hover:bg-primary/10 flex-shrink-0"
                                  title="Transferir item para outra comanda"
                                >
                                  <ArrowRight className="w-3 h-3 sm:w-4 sm:h-4" />
                                </Button>
                              )}
                              <Button
                                variant="outline"
                                size="icon"
                                onClick={() => removeItemAt(index)}
                                className="w-8 h-8 sm:w-9 sm:h-9 rounded-lg text-error border-error/30 hover:bg-error/10 flex-shrink-0"
                              >
                                <Trash2 className="w-3 h-3 sm:w-4 sm:h-4" />
                              </Button>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                    )
                  })}
                </div>
              )}
            </div>


            {/* Aviso: comanda obrigatória (balcão) ou cliente obrigatório (delivery) */}
            {items.length > 0 && !comandaAtiva && !vendaEmEdicao && !deliveryMode && !caixaRapidoMode && (
              <div className="border-t border-warning p-6 bg-warning/5">
                <div className="p-4 bg-warning/10 border border-warning/30 rounded-2xl text-center">
                  <p className="text-sm text-warning font-semibold">⚠️ Abra uma comanda antes de finalizar o pedido</p>
                </div>
              </div>
            )}
            {items.length > 0 && deliveryMode && !clienteSelecionado && (
              <div className="border-t border-warning px-3 py-2 bg-warning/5">
                <p className="text-xs text-warning font-semibold text-center">⚠️ Selecione o cliente para finalizar</p>
              </div>
            )}

            {/* Seleção de Cliente - Obrigatório em delivery; comanda: inserir ou trocar a qualquer momento */}
            {items.length > 0 && !caixaRapidoMode && (
              <div className={`border-t border-border ${deliveryMode ? 'p-2' : vendaEmEdicao ? 'p-2' : 'p-2 sm:p-2.5'}`}>
                {!deliveryMode && (
                  <h3 className={`font-semibold text-text-primary mb-1.5 ${vendaEmEdicao ? 'text-xs' : 'text-sm sm:text-base'}`}>
                    {vendaEmEdicao
                      ? 'Cliente da comanda (inserir ou trocar)'
                      : 'Cliente (Opcional)'}
                  </h3>
                )}
                
                {clienteSelecionado ? (
                  <div className={`bg-card-secondary border border-border rounded-xl ${deliveryMode ? 'p-2' : 'p-2 sm:p-3'}`}>
                    <div className="flex items-center justify-between gap-2">
                      <div className="flex items-center space-x-2 min-w-0 flex-1">
                        <div className={`${deliveryMode ? 'w-7 h-7' : 'w-8 h-8 sm:w-10 sm:h-10'} bg-primary/20 rounded-lg flex items-center justify-center flex-shrink-0`}>
                          <User className={`${deliveryMode ? 'w-3.5 h-3.5' : 'w-4 h-4 sm:w-5 sm:h-5'} text-primary`} />
                        </div>
                        <div className="min-w-0 flex-1">
                          <h4 className={`font-semibold ${deliveryMode ? 'text-xs' : 'text-xs sm:text-sm'} text-text-primary truncate`}>{clienteSelecionado.nomeCompleto}</h4>
                          {clienteSelecionado.documento && !deliveryMode && (
                            <p className="text-xs text-text-secondary truncate">{clienteSelecionado.documento}</p>
                          )}
                        </div>
                      </div>
                      <button
                        onClick={() => setClienteSelecionado(null)}
                        className={`${deliveryMode ? 'w-6 h-6' : 'w-7 h-7 sm:w-8 sm:h-8'} bg-error/10 text-error rounded-lg flex items-center justify-center hover:bg-error/20 transition-colors flex-shrink-0`}
                      >
                        <X className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>
                ) : (vendaEmEdicao && (vendaEmEdicao.nomeCliente ?? '').trim()) ? (
                  <div className={`bg-card-secondary border border-border rounded-xl ${deliveryMode ? 'p-2' : 'p-2 sm:p-3'}`}>\n                    <div className="flex items-center justify-between gap-2">
                      <div className="flex items-center space-x-2 min-w-0 flex-1">
                        <div className={`${deliveryMode ? 'w-7 h-7' : 'w-8 h-8 sm:w-10 sm:h-10'} bg-primary/20 rounded-lg flex items-center justify-center flex-shrink-0`}>
                          <User className={`${deliveryMode ? 'w-3.5 h-3.5' : 'w-4 h-4 sm:w-5 sm:h-5'} text-primary`} />
                        </div>
                        <div className="min-w-0 flex-1">
                          <h4 className={`font-semibold ${deliveryMode ? 'text-xs' : 'text-xs sm:text-sm'} text-text-primary truncate`}>{(vendaEmEdicao.nomeCliente ?? '').trim()}</h4>
                          {!deliveryMode && <p className="text-xs text-text-secondary">Cliente (não cadastrado) · Clique para trocar</p>}
                        </div>
                      </div>
                      <button
                        onClick={() => setShowClienteSearch(true)}
                        className={`${deliveryMode ? 'w-6 h-6' : 'w-7 h-7 sm:w-8 sm:h-8'} bg-primary/20 text-primary rounded-lg flex items-center justify-center hover:bg-primary/30 transition-colors flex-shrink-0`}
                        title="Trocar cliente"
                      >
                        <UserPlus className="w-3.5 h-3.5" />
                      </button>
                    </div>
                  </div>
                ) : (
                  <button
                    onClick={() => setShowClienteSearch(true)}
                    className={`w-full bg-card-secondary border ${!clienteSelecionado && deliveryMode ? 'border-warning/60' : 'border-border'} rounded-xl ${deliveryMode ? 'p-2' : 'p-2 sm:p-3'} hover:bg-card-hover transition-colors text-left`}
                  >
                    <div className="flex items-center space-x-2 sm:space-x-3">
                      <div className={`${deliveryMode ? 'w-7 h-7' : 'w-8 h-8 sm:w-10 sm:h-10'} bg-primary/20 rounded-lg flex items-center justify-center flex-shrink-0`}>
                        <UserPlus className={`${deliveryMode ? 'w-3.5 h-3.5' : 'w-4 h-4 sm:w-5 sm:h-5'} text-primary`} />
                      </div>
                      <div className="min-w-0 flex-1">
                        <h4 className={`font-semibold ${deliveryMode ? 'text-xs' : 'text-xs sm:text-sm'} text-text-primary`}>
                          {deliveryMode ? '👤 Selecionar cliente (obrigatório)' : 'Selecionar Cliente'}
                        </h4>
                        {!deliveryMode && <p className="text-xs text-text-secondary truncate">Buscar por nome, CPF/CNPJ ou telefone</p>}
                      </div>
                    </div>
                  </button>
                )}
              </div>
            )}

            {/* Footer com Total e Botão Finalizar */}
            {items.length > 0 && (
              <div className={`border-t border-border ${deliveryMode ? 'p-3' : vendaEmEdicao ? 'p-3' : 'p-3'} bg-background-secondary`}>
                {contasAbertoCliente?.temContasAberto && contasAbertoCliente.valorTotalPendente > 0 && (
                  <div className="mb-4 p-3 rounded-xl bg-red-50 border border-red-200">
                    <p className="text-sm font-semibold text-red-700">Cliente com valor em aberto</p>
                    <p className="text-sm text-red-600 mt-0.5">
                      Este cliente possui {contasAbertoCliente.quantidadeContas} conta(s) a receber em aberto: R$ {contasAbertoCliente.valorTotalPendente.toFixed(2).replace('.', ',')}
                    </p>
                  </div>
                )}
                <div className="flex justify-between items-center mb-2">
                  <span className={`${deliveryMode || vendaEmEdicao ? 'text-base' : 'text-base'} font-medium text-text-secondary`}>Total:</span>
                  <span className={`${deliveryMode || vendaEmEdicao ? 'text-xl' : 'text-xl'} font-bold text-primary`}>
                    R$ {formatarPreco(totalPrice)}
                  </span>
                </div>
                
                <Button 
                  onClick={handleFinalizarPedido}
                  disabled={isProcessing || showSuccess}
                  className={`w-full bg-primary text-primary-foreground ${deliveryMode ? 'py-3 text-base' : vendaEmEdicao ? 'py-2.5 text-sm' : 'py-3 text-base'} rounded-2xl font-semibold hover:bg-primary/90 transition-all shadow-lg disabled:opacity-50 disabled:cursor-not-allowed`}
                >
                  {isProcessing ? (
                    <div className="flex items-center space-x-2">
                      <div className="w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
                      <span>{vendaEmEdicao ? 'Atualizando...' : 'Processando...'}</span>
                    </div>
                  ) : showSuccess ? (
                    <div className="flex items-center space-x-2">
                      <CheckCircle className="w-5 h-5" />
                      <span>{vendaEmEdicao ? 'Pedido Atualizado!' : 'Pedido Finalizado!'}</span>
                    </div>
                  ) : (
                    vendaEmEdicao ? 'Atualizar Pedido' : 'Finalizar Pedido'
                  )}
                </Button>
                
                <p className={`text-center text-text-muted ${vendaEmEdicao ? 'text-xs mt-1' : 'text-sm mt-3'}`}>
                  {items.length} {items.length === 1 ? 'item' : 'itens'} no carrinho
                </p>

              </div>
            )}
          </motion.div>
        </>
      )}
      
      {/* Modal de Busca de Cliente */}
      <ClienteSearch
        isOpen={showClienteSearch}
        onClose={() => setShowClienteSearch(false)}
        onSelectCliente={setClienteSelecionado}
      />

      {/* Modal de Confirmação de Impressão */}
      <ConfirmarImpressaoModal
        isOpen={showConfirmarImpressao}
        onConfirm={(modo, vias) => handleConfirmarFinalizar(true, modo === 'completo', vias)}
        onCancel={() => handleConfirmarFinalizar(false)} // Finalizar sem imprimir
        isEdicao={!!vendaEmEdicao}
        quantidadeNovosItens={vendaEmEdicao ? getNovosItens().length : items.length}
        habilitarImprimirDuasVias={habilitarImprimirDuasVias}
      />


      {/* Modal de Transferência */}
      {vendaEmEdicao && itemParaTransferir && (
        <TransferItemModal
          isOpen={transferModalOpen}
          onClose={() => {
            setTransferModalOpen(false)
            setItemParaTransferir(null)
          }}
          notaOrigem={vendaEmEdicao.nota}
          itemOrigem={itemParaTransferir.itemNumero}
          descricaoItem={itemParaTransferir.descricao}
          onSuccess={() => {
            setTransferModalOpen(false)
            setItemParaTransferir(null)
          }}
        />
      )}
      {/* Modal de pagamento Delivery */}
      {showDeliveryPgtoModal && (
        <div className="fixed inset-0 z-[200] flex items-center justify-center bg-black/50">
          <div className="bg-card rounded-2xl p-5 shadow-2xl border border-border w-full max-w-xs mx-4">
            <h3 className="font-bold text-base text-text-primary mb-4 flex items-center gap-2">
              💳 Forma de Pagamento
            </h3>
            {(() => {
              const formasAutoPagas = ['PAGAMENTO ON LINE']
              const isAutoPago = formasAutoPagas.includes(deliveryFormaPgto)
              const selecionarForma = (forma: string) => {
                setDeliveryFormaPgto(forma)
                setDeliveryJaPago(formasAutoPagas.includes(forma))
              }
              return (
                <>
                  <div className="grid grid-cols-2 gap-2 mb-2">
                    {['PIX', 'DINHEIRO', 'CARTÃO DÉBITO', 'CARTÃO CRÉDITO'].map(forma => (
                      <button
                        key={forma}
                        onClick={() => selecionarForma(forma)}
                        className={`px-3 py-2 rounded-xl text-xs font-semibold border transition-colors ${
                          deliveryFormaPgto === forma
                            ? 'bg-primary text-primary-foreground border-primary'
                            : 'bg-card border-border text-text-primary hover:bg-muted/50'
                        }`}
                      >
                        {forma}
                      </button>
                    ))}
                  </div>
                  <button
                    onClick={() => selecionarForma('PAGAMENTO ON LINE')}
                    className={`w-full mb-4 px-3 py-2 rounded-xl text-xs font-semibold border transition-colors ${
                      deliveryFormaPgto === 'PAGAMENTO ON LINE'
                        ? 'bg-primary text-primary-foreground border-primary'
                        : 'bg-card border-border text-text-primary hover:bg-muted/50'
                    }`}
                  >
                    PAGAMENTO ON LINE
                  </button>
                  <div className="mb-5">
                    <p className="text-xs font-semibold text-text-secondary mb-2">Status do pagamento</p>
                    {isAutoPago ? (
                      <div className="w-full py-2 rounded-xl text-xs font-semibold text-center bg-green-500 text-white">
                        ✅ Pago automaticamente
                      </div>
                    ) : (
                      <div className="flex gap-2">
                        <button
                          onClick={() => setDeliveryJaPago(false)}
                          className={`flex-1 py-2 rounded-xl text-xs font-semibold border transition-colors ${
                            !deliveryJaPago
                              ? 'bg-orange-500 text-white border-orange-500'
                              : 'bg-card border-border text-text-secondary hover:bg-muted/50'
                          }`}
                        >
                          ⚠️ Cobrar na entrega
                        </button>
                        <button
                          onClick={() => setDeliveryJaPago(true)}
                          className={`flex-1 py-2 rounded-xl text-xs font-semibold border transition-colors ${
                            deliveryJaPago
                              ? 'bg-green-500 text-white border-green-500'
                              : 'bg-card border-border text-text-secondary hover:bg-muted/50'
                          }`}
                        >
                          ✅ Já pago
                        </button>
                      </div>
                    )}
                  </div>
                </>
              )
            })()}
            <div className="flex gap-2">
              <button
                onClick={() => setShowDeliveryPgtoModal(false)}
                className="flex-1 px-3 py-2.5 border border-border rounded-xl text-sm font-medium text-text-secondary hover:bg-muted/50"
              >
                Voltar
              </button>
              <button
                onClick={() => { setShowDeliveryPgtoModal(false); handleConfirmarFinalizar(false) }}
                className="flex-1 flex items-center justify-center gap-1.5 px-3 py-2.5 bg-primary text-primary-foreground rounded-xl text-sm font-semibold hover:bg-primary/90"
              >
                Confirmar
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Modal de confirmação WhatsApp Delivery */}
      {deliveryWhatsAppPendente && (
        <div
          className="fixed inset-0 z-[200] flex items-center justify-center bg-black/50"
          onClick={(e) => { if (e.target === e.currentTarget && !enviandoWhatsAppModal) handleIgnorarWhatsAppDelivery() }}
        >
          <div className="bg-card rounded-2xl p-6 shadow-2xl border border-border w-full max-w-sm mx-4">
            <div className="flex items-center gap-3 mb-4">
              <div className="w-12 h-12 bg-green-100 rounded-xl flex items-center justify-center">
                <MessageCircle className="w-6 h-6 text-green-600" />
              </div>
              <div>
                <h3 className="font-bold text-lg text-text-primary">Pedido salvo!</h3>
                <p className="text-sm text-text-muted">Deseja notificar o cliente?</p>
              </div>
            </div>
            {deliveryWhatsAppPendente.nomeCliente && (
              <p className="text-sm text-text-secondary mb-3 bg-muted/40 rounded-lg px-3 py-2">
                👤 {deliveryWhatsAppPendente.nomeCliente}
              </p>
            )}
            <p className="text-sm text-text-secondary mb-6">
              Enviar mensagem WhatsApp com o resumo do pedido para o cliente?
            </p>
            <div className="flex gap-3">
              <button
                onClick={handleIgnorarWhatsAppDelivery}
                disabled={enviandoWhatsAppModal}
                className="flex-1 px-4 py-3 border border-border rounded-xl text-sm font-medium text-text-secondary hover:bg-muted/50 transition-colors disabled:opacity-50"
              >
                Não enviar
              </button>
              <button
                onClick={handleEnviarWhatsAppDelivery}
                disabled={enviandoWhatsAppModal}
                className="flex-1 flex items-center justify-center gap-2 px-4 py-3 bg-green-500 hover:bg-green-600 text-white rounded-xl text-sm font-semibold transition-colors disabled:opacity-60"
              >
                {enviandoWhatsAppModal ? (
                  <Loader2 className="w-4 h-4 animate-spin" />
                ) : (
                  <MessageCircle className="w-4 h-4" />
                )}
                Enviar WhatsApp
              </button>
            </div>
          </div>
        </div>
      )}
    </AnimatePresence>
  )
}

export default CartDrawer

