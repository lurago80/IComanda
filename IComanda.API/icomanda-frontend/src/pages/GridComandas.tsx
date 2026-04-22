import { AnimatePresence, motion } from 'framer-motion'
import { ArrowLeft, CheckCircle2, Circle, Filter, Loader2, Package, RefreshCw, Search, Trash2, XCircle, MessageCircle, Copy } from 'lucide-react'
import React, { useEffect, useState } from 'react'
import { useToast } from '../hooks/useToast'
import { vendasService } from '../services/api'
import { Venda } from '../types/api'
import ConectarWhatsAppModal from '../components/ConectarWhatsAppModal'
import { enviarWhatsApp, formatarMensagemComanda, formatarNumeroTelefone, copiarMensagemWhatsApp, abrirLinkWhatsApp } from '../utils/whatsapp'

interface GridComandasProps {
  onClose: () => void
  onEditarComanda: (nota: string) => void
}

type FiltroStatus = 'todas' | 'abertas' | 'fechadas' | 'excluidas'

/** Extrai número da comanda (API pode retornar number, string ou PascalCase). */
function getComandaNum(v: Venda): number | null {
  const obj = v as unknown as Record<string, unknown>
  const raw = obj.comanda ?? obj.Comanda
  if (raw == null) return null
  const n = typeof raw === 'number' ? raw : Number(raw)
  return Number.isNaN(n) ? null : n
}

const GridComandas: React.FC<GridComandasProps> = ({ onClose, onEditarComanda }) => {
  const [vendas, setVendas] = useState<Venda[]>([])
  const [vendasAbertas, setVendasAbertas] = useState<Venda[]>([])
  const [vendasExcluidas, setVendasExcluidas] = useState<Venda[]>([])
  const [vendasFiltradas, setVendasFiltradas] = useState<Venda[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [filtroStatus, setFiltroStatus] = useState<FiltroStatus>('abertas')
  const [comandasUnicas, setComandasUnicas] = useState<Map<number, Venda>>(new Map())
  const [vendasComItens, setVendasComItens] = useState<Map<string, Venda>>(new Map())
  const [enviandoWhatsApp, setEnviandoWhatsApp] = useState<string | null>(null)
  const [showConectarWhatsAppModal, setShowConectarWhatsAppModal] = useState(false)
  const [showExcluirModal, setShowExcluirModal] = useState(false)
  const [vendaParaExcluir, setVendaParaExcluir] = useState<Venda | null>(null)
  const [senhaExcluir, setSenhaExcluir] = useState('')
  const [justificativaExcluir, setJustificativaExcluir] = useState('')
  const [excluindo, setExcluindo] = useState(false)
  const [erroExcluir, setErroExcluir] = useState('')
  const [buscaNome, setBuscaNome] = useState('')
  const [nomeEstabelecimento, setNomeEstabelecimento] = useState<string>('')
  const { showError, showSuccess, showWarning } = useToast()

  const handleEnviarWhatsApp = async (venda: Venda, e: React.MouseEvent) => {
    e.stopPropagation(); // Impede que o clique no botão dispare o clique no card
    
    if (!venda.telefoneCliente) {
      showError('Aviso', 'Cliente não possui telefone cadastrado');
      return;
    }

    // Verificar se já está enviando
    if (enviandoWhatsApp === venda.nota) {
      return; // Já está enviando, não fazer nada
    }

    setEnviandoWhatsApp(venda.nota);

    try {
      // Buscar itens da venda se ainda não foram carregados
      let vendaComItens = vendasComItens.get(venda.nota);
      if (!vendaComItens) {
        const vendaCompleta = await vendasService.getByNota(venda.nota);
        vendaComItens = vendaCompleta;
        setVendasComItens(new Map(vendasComItens.set(venda.nota, vendaCompleta)));
      }

      const mensagem = formatarMensagemComanda({ ...(vendaComItens || venda), nomeEstabelecimento });
      const resultado = await enviarWhatsApp(venda.telefoneCliente, mensagem);
      if (resultado.method === 'sent') {
        showSuccess('✅ Enviado', 'Mensagem enviada com sucesso!');
      } else if (resultado.method === 'link' && resultado.link) {
        abrirLinkWhatsApp(resultado.link);
        showSuccess('WhatsApp aberto', 'Envie a mensagem na janela que abriu (ou nesta aba).');
      } else {
        showWarning('Não enviou', 'WhatsApp não está conectado. Siga as instruções na tela para conectar e depois enviar direto.');
        setShowConectarWhatsAppModal(true);
      }
    } catch (error: any) {
      showWarning('Não enviou', error.message || 'Não foi possível enviar. Siga as instruções na tela para conectar o WhatsApp.');
      setShowConectarWhatsAppModal(true);
    } finally {
      setEnviandoWhatsApp(null);
    }
  }

  const handleCopiarMensagem = async (venda: Venda, e: React.MouseEvent) => {
    e.stopPropagation();
    
    if (!venda.telefoneCliente) {
      showError('Aviso', 'Cliente não possui telefone cadastrado');
      return;
    }

    try {
      // Buscar itens da venda se ainda não foram carregados
      let vendaComItens = vendasComItens.get(venda.nota);
      if (!vendaComItens) {
        const vendaCompleta = await vendasService.getByNota(venda.nota);
        vendaComItens = vendaCompleta;
        setVendasComItens(new Map(vendasComItens.set(venda.nota, vendaCompleta)));
      }

      const mensagem = formatarMensagemComanda({ ...(vendaComItens || venda), nomeEstabelecimento });
      const copiado = await copiarMensagemWhatsApp(venda.telefoneCliente, mensagem);
      
      if (copiado) {
        showSuccess('Sucesso', 'Mensagem copiada! Vá para o WhatsApp Web, cole (Ctrl+V) e envie.');
      } else {
        showError('Erro', 'Não foi possível copiar a mensagem');
      }
    } catch (error: any) {
      showError('Erro', error.message || 'Não foi possível copiar a mensagem');
    }
  }

  const handleAbrirModalExcluir = (venda: Venda, e: React.MouseEvent) => {
    e.stopPropagation();
    setVendaParaExcluir(venda);
    setSenhaExcluir('');
    setJustificativaExcluir('');
    setErroExcluir('');
    setShowExcluirModal(true);
  };

  const handleConfirmarExcluir = async () => {
    if (!vendaParaExcluir) return;
    if (!justificativaExcluir.trim()) {
      showError('Atenção', 'Informe a justificativa para o cancelamento.');
      return;
    }
    if (!senhaExcluir.trim()) {
      showError('Atenção', 'Digite a senha de cancelamento para confirmar.');
      return;
    }
    setExcluindo(true);
    setErroExcluir('');
    try {
      await vendasService.excluirComanda(vendaParaExcluir.nota, { justificativa: justificativaExcluir.trim(), senha: senhaExcluir.trim() });
      showSuccess('Comanda excluída', 'A comanda foi excluída e registrada no histórico.');
      setShowExcluirModal(false);
      setVendaParaExcluir(null);
      setSenhaExcluir('');
      setJustificativaExcluir('');
      setErroExcluir('');
      carregarVendas();
    } catch (err: any) {
      const msg = err?.response?.data?.mensagem || err?.message || 'Não foi possível excluir. Verifique a senha.';
      setErroExcluir(msg);
      setSenhaExcluir('');
    } finally {
      setExcluindo(false);
    }
  };

  useEffect(() => {
    carregarVendas()
    vendasService.getEmitente().then((e) => {
      const nome = e?.nomeFantasia?.trim() || (e as any)?.nome?.trim() || ''
      if (nome) setNomeEstabelecimento(nome)
    }).catch(() => {})
  }, [])

  useEffect(() => {
    console.log('🔄 [GridComandas] useEffect aplicarFiltro disparado:', {
      filtroStatus,
      vendas: vendas.length,
      vendasAbertas: vendasAbertas.length,
      vendasExcluidas: vendasExcluidas.length,
      comandasUnicas: comandasUnicas.size
    })
    aplicarFiltro()
  }, [filtroStatus, vendas, vendasAbertas, vendasExcluidas, comandasUnicas])

  const carregarExcluidas = async () => {
    try {
      setIsLoading(true)
      // Apenas o dia atual
      const hoje = new Date().toISOString().split('T')[0]
      const lista = await vendasService.getCanceladas(hoje, hoje)
      setVendasExcluidas(Array.isArray(lista) ? lista : [])
    } catch (e: any) {
      console.error('Erro ao carregar comandas excluídas:', e)
      showError('Erro', e?.response?.data?.mensagem || 'Não foi possível carregar comandas excluídas')
      setVendasExcluidas([])
    } finally {
      setIsLoading(false)
    }
  }

  const carregarVendas = async () => {
    setIsLoading(true)
    try {
      console.log('🔄 [GridComandas] Iniciando carregamento de vendas...')
      
      // Buscar todas as vendas de hoje e as abertas em paralelo
      // Se uma falhar, ainda tentar a outra
      let vendasHoje: Venda[] = []
      let vendasAbertasData: Venda[] = []
      
      try {
        console.log('🔄 [GridComandas] Buscando vendas de hoje...')
        vendasHoje = await vendasService.getHoje()
        console.log('✅ [GridComandas] Vendas de hoje carregadas:', vendasHoje.length)
      } catch (error: any) {
        console.error('❌ [GridComandas] Erro ao buscar vendas de hoje:', error)
        // Continuar mesmo se falhar
      }
      
      try {
        console.log('🔄 [GridComandas] Buscando vendas abertas...')
        vendasAbertasData = await vendasService.getAbertas()
        console.log('✅ [GridComandas] Vendas abertas carregadas:', vendasAbertasData.length)
      } catch (error: any) {
        console.error('❌ [GridComandas] Erro ao buscar vendas abertas:', error)
        showError('Erro', 'Não foi possível carregar as comandas abertas')
        // Se falhar nas abertas, ainda tentar usar as de hoje
      }
      
      setVendas(vendasHoje)
      // Manter no estado TODAS as abertas (qualquer data) — comandas de dias anteriores
      // ainda abertas devem aparecer como abertas, não como fechadas.
      setVendasAbertas(vendasAbertasData)

      // Criar mapa de comandas únicas (pegar a mais recente de cada comanda)
      // IMPORTANTE: Incluir APENAS as vendas do dia atual (abertas E fechadas)
      const mapaComandas = new Map<number, Venda>()
      
      // ESTRATÉGIA: Combinar vendasHoje e vendasAbertas para ter TODAS as comandas DO DIA
      // vendasHoje = vendas com data_saida = hoje (fechadas hoje)
      // vendasAbertas = vendas com lancado = 'ABERTO' (abertas, qualquer data) — filtrar só hoje
      
      // Primeiro adicionar todas as vendas de hoje (inclui fechadas de hoje)
      if (vendasHoje && vendasHoje.length > 0) {
        console.log('📋 [GridComandas] Processando vendas de hoje:', vendasHoje.map(v => ({
          comanda: v.comanda,
          lancado: v.lancado,
          nota: v.nota,
          emissao: v.emissao
        })))
        
        vendasHoje.forEach(venda => {
          const numComanda = venda.comanda != null ? Number(venda.comanda) : null
          if (numComanda != null && !Number.isNaN(numComanda)) {
            const comandaExistente = mapaComandas.get(numComanda)
            if (!comandaExistente || new Date(venda.emissao || 0) > new Date(comandaExistente.emissao || 0)) {
              mapaComandas.set(numComanda, venda)
            }
          }
        })
        console.log('✅ [GridComandas] Comandas de hoje (abertas + fechadas) adicionadas ao mapa:', mapaComandas.size)
      }
      
      // Depois adicionar/atualizar com TODAS as comandas abertas (qualquer data)
      // Comandas abertas de dias anteriores ainda vigentes devem ser tratadas como abertas.
      if (vendasAbertasData && vendasAbertasData.length > 0) {
        console.log('📋 [GridComandas] Processando vendas abertas (todas as datas):', vendasAbertasData.map(v => ({
          comanda: v.comanda,
          lancado: v.lancado,
          nota: v.nota,
          emissao: v.emissao
        })))
        
        vendasAbertasData.forEach(venda => {
          const numComanda = venda.comanda != null ? Number(venda.comanda) : null
          if (numComanda != null && !Number.isNaN(numComanda)) {
            const comandaExistente = mapaComandas.get(numComanda)
            if (!comandaExistente || new Date(venda.emissao || 0) >= new Date(comandaExistente.emissao || 0)) {
              mapaComandas.set(numComanda, venda)
            }
          }
        })
        console.log('✅ [GridComandas] Comandas abertas atualizadas no mapa. Total:', mapaComandas.size)
      }
      
      setComandasUnicas(mapaComandas)
      console.log('✅ [GridComandas] Carregamento concluído. Total de comandas únicas:', mapaComandas.size)
      
      // Log detalhado de todas as comandas no mapa
      const comandasDetalhadas = Array.from(mapaComandas.values()).map(v => ({
        comanda: v.comanda,
        lancado: v.lancado,
        nota: v.nota,
        total: v.total,
        emissao: v.emissao
      }))
      
      console.log('📊 [GridComandas] Resumo do mapa:', {
        totalComandas: mapaComandas.size,
        vendasHoje: vendasHoje.length,
        vendasAbertas: vendasAbertasData.length,
        comandasNoMapa: comandasDetalhadas
      })
      
      // Verificar quantas estão abertas vs fechadas
      const comandasAbertasNoMapa = comandasDetalhadas.filter(c => c.lancado === 'ABERTO')
      const comandasFechadasNoMapa = comandasDetalhadas.filter(c => c.lancado !== 'ABERTO')
      console.log('📊 [GridComandas] Distribuição:', {
        abertas: comandasAbertasNoMapa.length,
        fechadas: comandasFechadasNoMapa.length,
        abertasDetalhes: comandasAbertasNoMapa,
        fechadasDetalhes: comandasFechadasNoMapa
      })
    } catch (error: any) {
      console.error('❌ [GridComandas] Erro geral ao carregar vendas:', error)
      showError('Erro', `Não foi possível carregar as comandas: ${error.message || 'Erro desconhecido'}`)
      // Garantir que pelo menos temos um mapa vazio
      setComandasUnicas(new Map())
    } finally {
      setIsLoading(false)
    }
  }

  const aplicarFiltro = () => {
    // Verificar se temos dados disponíveis
    // Se comandasUnicas está vazio mas temos vendasAbertas, usar vendasAbertas para criar o mapa
    if (comandasUnicas.size === 0) {
      console.log('⚠️ [GridComandas] aplicarFiltro: comandasUnicas está vazio')
      console.log('⚠️ [GridComandas] Estado atual:', {
        vendas: vendas.length,
        vendasAbertas: vendasAbertas.length,
        comandasUnicas: comandasUnicas.size,
        filtroStatus
      })
      
      // Se temos vendas abertas mas o mapa está vazio, criar o mapa a partir delas
      if (vendasAbertas.length > 0) {
        console.log('🔄 [GridComandas] Criando mapa a partir de vendas abertas...')
        const mapaComandas = new Map<number, Venda>()
        vendasAbertas.forEach(venda => {
          const num = venda.comanda != null ? Number(venda.comanda) : null
          if (num != null && !Number.isNaN(num)) {
            mapaComandas.set(num, venda)
          }
        })
        setComandasUnicas(mapaComandas)
        console.log('✅ [GridComandas] Mapa criado com', mapaComandas.size, 'comandas')
        // Aplicar filtro imediatamente com o mapa criado
        const todasComandas = Array.from(mapaComandas.values()).filter(v => v.comanda != null)
        const comandasAbertasSet = new Set(
          vendasAbertas.map(v => v.comanda != null ? Number(v.comanda) : null).filter((n): n is number => n != null && !Number.isNaN(n))
        )
        let filtradas: Venda[]
        if (filtroStatus === 'todas') {
          filtradas = todasComandas
        } else if (filtroStatus === 'abertas') {
          filtradas = vendasAbertas.filter(v => v.comanda != null && !Number.isNaN(Number(v.comanda)))
        } else if (filtroStatus === 'excluidas') {
          filtradas = vendasExcluidas
        } else {
          filtradas = todasComandas.filter(v => v.comanda != null && !comandasAbertasSet.has(Number(v.comanda)))
        }
        
        setVendasFiltradas(filtradas)
        return
      } else if (vendas.length > 0) {
        // Se temos vendas de hoje, criar o mapa a partir delas
        console.log('🔄 [GridComandas] Criando mapa a partir de vendas de hoje...')
        const mapaComandas = new Map<number, Venda>()
        vendas.forEach(venda => {
          if (venda.comanda) {
            const comandaExistente = mapaComandas.get(venda.comanda)
            if (!comandaExistente || new Date(venda.emissao) > new Date(comandaExistente.emissao)) {
              mapaComandas.set(venda.comanda, venda)
            }
          }
        })
        setComandasUnicas(mapaComandas)
        console.log('✅ [GridComandas] Mapa criado com', mapaComandas.size, 'comandas')
        // Aplicar filtro imediatamente com o mapa criado
        const todasComandas = Array.from(mapaComandas.values()).filter(v => v.comanda != null)
        const comandasAbertasSet = new Set(
          vendasAbertas.map(v => v.comanda != null ? Number(v.comanda) : null).filter((n): n is number => n != null && !Number.isNaN(n))
        )
        let filtradas: Venda[]
        if (filtroStatus === 'todas') {
          filtradas = todasComandas
        } else if (filtroStatus === 'abertas') {
          filtradas = todasComandas.filter(v => v.comanda != null && comandasAbertasSet.has(Number(v.comanda)))
        } else if (filtroStatus === 'excluidas') {
          filtradas = vendasExcluidas
        } else {
          filtradas = todasComandas.filter(v => v.comanda != null && !comandasAbertasSet.has(Number(v.comanda)))
        }
        
        setVendasFiltradas(filtradas)
        return
      } else if (filtroStatus === 'excluidas') {
        setVendasFiltradas(vendasExcluidas)
        return
      } else {
        // Se não temos dados, NÃO tentar recarregar aqui (evitar loop infinito)
        // O recarregamento deve ser feito manualmente ou em outro momento
        console.log('⚠️ [GridComandas] Nenhum dado disponível. Aguardando carregamento inicial...')
        setVendasFiltradas([])
        return
      }
    }

    const todasComandas = Array.from(comandasUnicas.values()).filter(v => v.comanda != null)
    const comandasAbertasSet = new Set(
      vendasAbertas.map(v => v.comanda != null ? Number(v.comanda) : null).filter((n): n is number => n != null && !Number.isNaN(n))
    )

    let filtradas: Venda[]

    if (filtroStatus === 'excluidas') {
      filtradas = vendasExcluidas
    } else if (filtroStatus === 'todas') {
      // Para "todas", retornar todas as comandas do mapa (abertas e fechadas)
      // Garantir que está ordenado por número de comanda
      filtradas = todasComandas.sort((a, b) => (a.comanda || 0) - (b.comanda || 0))
      
      console.log('🔍 [GridComandas] Filtro "todas" aplicado:', {
        totalComandas: filtradas.length,
        comandasUnicas: comandasUnicas.size,
        vendasAbertas: vendasAbertas.length,
        todasComandas: todasComandas.length,
        filtroStatus,
        detalhes: filtradas.map(v => ({
          comanda: v.comanda,
          lancado: v.lancado,
          nota: v.nota
        }))
      })
    } else if (filtroStatus === 'abertas') {
      // Para comandas abertas, usar diretamente as vendas abertas (mais confiável)
      // Mas também incluir qualquer comanda do mapa que esteja aberta
      const comandasAbertasDoMapa = todasComandas.filter(v => 
        v.comanda != null && comandasAbertasSet.has(Number(v.comanda))
      )
      
      // Combinar com as vendas abertas diretas (pode haver diferenças)
      const todasAbertas = [...vendasAbertas]
      comandasAbertasDoMapa.forEach(venda => {
        const num = venda.comanda != null ? Number(venda.comanda) : null
        if (num != null && !todasAbertas.find(v => Number(v.comanda) === num)) {
          todasAbertas.push(venda)
        }
      })
      
      // Remover duplicatas mantendo a mais recente
      const mapaAbertas = new Map<number, Venda>()
      todasAbertas.forEach(v => {
        const num = v.comanda != null ? Number(v.comanda) : null
        if (num != null && !Number.isNaN(num)) {
          const existente = mapaAbertas.get(num)
          if (!existente || new Date(v.emissao || 0) > new Date(existente.emissao || 0)) {
            mapaAbertas.set(num, v)
          }
        }
      })
      
      filtradas = Array.from(mapaAbertas.values())
      
      console.log('🔍 [GridComandas] Filtro "abertas" aplicado:', {
        totalComandas: filtradas.length,
        vendasAbertas: vendasAbertas.length,
        comandasAbertasDoMapa: comandasAbertasDoMapa.length
      })
    } else {
      // Fechadas: todas que não estão abertas
      // IMPORTANTE: Uma comanda está fechada se não está no set de abertas
      filtradas = todasComandas.filter(v => 
        v.comanda != null && !comandasAbertasSet.has(Number(v.comanda))
      )
      
      console.log('🔍 [GridComandas] Filtro "fechadas" aplicado:', {
        totalComandas: filtradas.length,
        todasComandas: todasComandas.length,
        comandasAbertas: comandasAbertasSet.size,
        detalhes: filtradas.map(v => ({
          comanda: v.comanda,
          lancado: v.lancado,
          nota: v.nota
        }))
      })
    }

    setVendasFiltradas(filtradas)
  }


  const handleComandaClick = async (venda: Venda) => {
    const numComanda = getComandaNum(venda)
    // Se comanda é 0 ou ausente, abrir por nota (vendas com erro de cadastro)
    if (numComanda == null || numComanda === 0) {
      const porNota = vendasAbertas.find(v => v.nota && String(v.nota).trim() === String(venda.nota).trim())
      if (porNota) {
        onEditarComanda(porNota.nota)
        onClose()
      } else {
        showError('Aviso', 'Comanda não encontrada. Tente recarregar.')
      }
      return
    }

    const setAbertas = new Set(vendasAbertas.map(v => getComandaNum(v)).filter((n): n is number => n != null))
    const estaAberta = setAbertas.has(numComanda)

    if (estaAberta) {
      try {
        const vendaAberta = vendasAbertas.find(v => getComandaNum(v) === numComanda)
        if (vendaAberta) {
          onEditarComanda(vendaAberta.nota)
          onClose()
        } else {
          const vendasAbertasAtualizadas = await vendasService.getAbertas()
          const vendaAbertaAtualizada = vendasAbertasAtualizadas.find(v => getComandaNum(v) === numComanda)
          if (vendaAbertaAtualizada) {
            onEditarComanda(vendaAbertaAtualizada.nota)
            onClose()
          } else {
            showError('Aviso', 'Comanda não encontrada como aberta')
          }
        }
      } catch (error) {
        showError('Erro', 'Não foi possível abrir a comanda para edição')
      }
    } else {
      showError('Aviso', 'Esta comanda está fechada e não pode ser editada')
    }
  }

  const formatarPreco = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor)
  }

  // Set de comandas abertas sempre em número (aceita API em camelCase ou PascalCase)
  const comandasAbertasSet = new Set(
    vendasAbertas.map(v => getComandaNum(v)).filter((n): n is number => n != null)
  )

  // Obter comandas para exibir baseado no filtro
  const todasComandas = Array.from(comandasUnicas.values()).filter(v => v.comanda != null)
  
  const ordenarPorNome = (a: Venda, b: Venda) => {
    const nomeA = (a.nomeCliente || '').trim()
    const nomeB = (b.nomeCliente || '').trim()
    if (!nomeA && !nomeB) return (getComandaNum(a) ?? 0) - (getComandaNum(b) ?? 0)
    if (!nomeA) return 1
    if (!nomeB) return -1
    return nomeA.localeCompare(nomeB, 'pt-BR')
  }
  
  let comandasParaExibir: Venda[] = []
  
  if (filtroStatus === 'todas') {
    // Unir explicitamente: abertas + fechadas (do mapa) + excluídas; dedupe por nota
    const abertasComNumero = vendasAbertas.filter(v => getComandaNum(v) != null && getComandaNum(v)! > 0)
    const porComandaAbertas = new Map<number, Venda>()
    abertasComNumero.forEach(v => {
      const num = getComandaNum(v)!
      const existente = porComandaAbertas.get(num)
      if (!existente || new Date(v.emissao || 0) > new Date(existente.emissao || 0)) {
        porComandaAbertas.set(num, v)
      }
    })
    const abertasSemComanda = vendasAbertas.filter(v => getComandaNum(v) == null || getComandaNum(v) === 0)
    const fechadasParaTodas = todasComandas.filter(
      v => getComandaNum(v) != null && !comandasAbertasSet.has(getComandaNum(v)!)
    )
    const porNota = new Map<string, Venda>()
    const listaTodas = [
      ...Array.from(porComandaAbertas.values()),
      ...abertasSemComanda,
      ...fechadasParaTodas,
      ...vendasExcluidas
    ]
    listaTodas.forEach(v => {
      if (v.nota && String(v.nota).trim()) porNota.set(String(v.nota).trim(), v)
    })
    comandasParaExibir = Array.from(porNota.values()).sort(ordenarPorNome)
    console.log('🔍 [GridComandas] comandasParaExibir para "todas":', {
      total: comandasParaExibir.length,
      abertasComNumero: porComandaAbertas.size,
      abertasSemComanda: abertasSemComanda.length,
      fechadas: fechadasParaTodas.length,
      excluidas: vendasExcluidas.length
    })
  } else if (filtroStatus === 'abertas') {
    // Fonte única: vendasAbertas da API; aceita comanda em qualquer formato (number/string/PascalCase)
    const listaAbertas = vendasAbertas.filter(v => getComandaNum(v) != null)
    const porComanda = new Map<number, Venda>()
    listaAbertas.forEach(v => {
      const num = getComandaNum(v)!
      const existente = porComanda.get(num)
      if (!existente || new Date(v.emissao || 0) > new Date(existente.emissao || 0)) {
        porComanda.set(num, v)
      }
    })
    comandasParaExibir = Array.from(porComanda.values()).sort(ordenarPorNome)
    // Se ainda vazio mas a API retornou itens, exibir por nota (fallback)
    if (comandasParaExibir.length === 0 && vendasAbertas.length > 0) {
      const porNota = new Map<string, Venda>()
      vendasAbertas.forEach(v => {
        if (v.nota) porNota.set(v.nota, v)
      })
      comandasParaExibir = Array.from(porNota.values()).sort(ordenarPorNome)
    }
  } else if (filtroStatus === 'excluidas') {
    comandasParaExibir = [...vendasExcluidas].sort(ordenarPorNome)
  } else {
    // Fechadas: todas que não estão abertas
    comandasParaExibir = todasComandas
      .filter(v => getComandaNum(v) != null && !comandasAbertasSet.has(getComandaNum(v)!))
      .sort(ordenarPorNome)
  }

  // Aplicar busca por nome (filtra na lista já ordenada)
  const termoBusca = buscaNome.trim().toLowerCase()
  if (termoBusca) {
    comandasParaExibir = comandasParaExibir.filter(v =>
      (v.nomeCliente || '').toLowerCase().includes(termoBusca)
    )
  }

  // Criar grid de comandas (4 por linha)
  // Usar sempre comandasParaExibir que já está calculado corretamente
  let gridFiltrado: (Venda | null)[][] = []
  
  console.log('🔍 [GridComandas] Criando grid:', {
    filtroStatus,
    comandasParaExibir: comandasParaExibir.length,
    vendasFiltradas: vendasFiltradas.length,
    comandasUnicas: comandasUnicas.size
  })
  
  // Para todos os modos, criar grid compacto apenas com as comandas existentes
  // Isso evita criar muitas células vazias no modo "todas"
  const numLinhas = Math.ceil(comandasParaExibir.length / 4)
  
  for (let i = 0; i < numLinhas; i++) {
    const linha: (Venda | null)[] = []
    for (let j = 0; j < 4; j++) {
      const index = i * 4 + j
      if (index < comandasParaExibir.length) {
        linha.push(comandasParaExibir[index])
      } else {
        linha.push(null)
      }
    }
    // Só adicionar linha se tiver pelo menos uma comanda
    if (linha.some(c => c !== null)) {
      gridFiltrado.push(linha)
    }
  }
  
  console.log('✅ [GridComandas] Grid criado:', {
    numLinhas: gridFiltrado.length,
    totalCelulas: gridFiltrado.flat().length,
    celulasComComanda: gridFiltrado.flat().filter(c => c !== null).length
  })

  return (
  <>
    <div className="min-h-screen bg-background">
      {/* Header */}
      <div className="bg-primary text-primary-foreground p-4 sm:p-6 sticky top-0 z-50 shadow-lg">
        <div className="flex items-center justify-between">
          <button
            onClick={onClose}
            className="flex items-center space-x-2 hover:bg-white/20 rounded-xl p-2 transition-colors"
          >
            <ArrowLeft className="w-5 h-5" />
            <span className="font-semibold hidden sm:inline">Voltar</span>
          </button>

          <h1 className="text-xl sm:text-2xl font-bold text-center flex-1">Comandas</h1>

          <div className="flex items-center space-x-2">
            <button
              onClick={carregarVendas}
              disabled={isLoading}
              className="p-2 hover:bg-white/20 rounded-xl transition-colors disabled:opacity-50"
              title="Atualizar"
            >
              <RefreshCw className={`w-5 h-5 ${isLoading ? 'animate-spin' : ''}`} />
            </button>
          </div>
        </div>

        <p className="mt-2 text-center">
          <button
            type="button"
            onClick={() => setShowConectarWhatsAppModal(true)}
            className="text-sm text-green-300 hover:text-green-200 underline"
          >
            Como conectar WhatsApp para envio direto?
          </button>
        </p>

        {/* Busca por nome */}
        <div className="mt-4 flex justify-center">
          <div className="relative w-full max-w-xs">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-white/70" />
            <input
              type="text"
              value={buscaNome}
              onChange={(e) => setBuscaNome(e.target.value)}
              placeholder="Buscar por nome do cliente"
              className="w-full pl-9 pr-4 py-2 rounded-xl bg-white/20 text-white placeholder-white/60 border border-white/30 focus:outline-none focus:ring-2 focus:ring-white/50"
            />
          </div>
        </div>

        {/* Filtros */}
        <div className="mt-4 flex flex-wrap gap-2 justify-center">
          <button
            onClick={() => {
              setFiltroStatus('todas')
              if (comandasUnicas.size === 0) carregarVendas()
              // Carregar excluídas para que "Todas" as inclua (só se ainda não tiver)
              if (vendasExcluidas.length === 0) carregarExcluidas()
            }}
            className={`px-4 py-2 rounded-xl font-semibold transition-all ${
              filtroStatus === 'todas'
                ? 'bg-white text-primary shadow-lg'
                : 'bg-white/20 text-white hover:bg-white/30'
            }`}
          >
            <Filter className="w-4 h-4 inline mr-2" />
            Todas
          </button>
          <button
            onClick={() => setFiltroStatus('abertas')}
            className={`px-4 py-2 rounded-xl font-semibold transition-all ${
              filtroStatus === 'abertas'
                ? 'bg-white text-primary shadow-lg'
                : 'bg-white/20 text-white hover:bg-white/30'
            }`}
          >
            <Circle className="w-4 h-4 inline mr-2" />
            Abertas
          </button>
          <button
            onClick={() => {
              console.log('🔄 [GridComandas] Botão "Fechadas" clicado')
              setFiltroStatus('fechadas')
              if (comandasUnicas.size === 0) {
                console.log('🔄 [GridComandas] Mapa vazio, recarregando dados...')
                carregarVendas()
              }
            }}
            className={`px-4 py-2 rounded-xl font-semibold transition-all ${
              filtroStatus === 'fechadas'
                ? 'bg-white text-primary shadow-lg'
                : 'bg-white/20 text-white hover:bg-white/30'
            }`}
          >
            <XCircle className="w-4 h-4 inline mr-2" />
            Fechadas
          </button>
          <button
            onClick={() => {
              setFiltroStatus('excluidas')
              carregarExcluidas()
            }}
            className={`px-4 py-2 rounded-xl font-semibold transition-all ${
              filtroStatus === 'excluidas'
                ? 'bg-white text-primary shadow-lg'
                : 'bg-white/20 text-white hover:bg-white/30'
            }`}
          >
            <Trash2 className="w-4 h-4 inline mr-2" />
            Excluídas
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="p-4 sm:p-6">
        {isLoading && (vendas.length === 0 || filtroStatus === 'excluidas') ? (
          <div className="flex flex-col items-center justify-center py-20 space-y-4">
            <Loader2 className="w-12 h-12 text-primary animate-spin" />
            <p className="text-lg text-text-secondary">
              {filtroStatus === 'excluidas' ? 'Carregando comandas excluídas...' : 'Carregando comandas...'}
            </p>
          </div>
        ) : (gridFiltrado.length === 0 || comandasParaExibir.length === 0) ? (
          <div className="flex flex-col items-center justify-center py-20 space-y-4">
            <Package className="w-16 h-16 text-text-muted" />
            <p className="text-lg font-semibold text-text-primary">
              {termoBusca
                ? 'Nenhuma comanda encontrada com esse nome'
                : filtroStatus === 'abertas'
                ? 'Nenhuma comanda aberta'
                : filtroStatus === 'fechadas'
                ? 'Nenhuma comanda fechada'
                : filtroStatus === 'excluidas'
                ? 'Nenhuma comanda excluída'
                : 'Nenhuma comanda encontrada'}
            </p>
            <p className="text-sm text-text-secondary">
              {termoBusca
                ? 'Tente outro termo na busca'
                : filtroStatus === 'abertas'
                ? 'Todas as comandas foram finalizadas'
                : filtroStatus === 'excluidas'
                ? 'Não há comandas excluídas nos últimos 30 dias'
                : 'Não há comandas para exibir'}
            </p>
          </div>
        ) : (
          <div className="max-w-4xl mx-auto">
            {/* Grid de Comandas */}
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 sm:gap-4">
              {gridFiltrado.flat().map((venda, index) => {
                if (!venda) {
                  // Não mostrar células vazias em nenhum modo
                  return null
                }

                const numComanda = getComandaNum(venda) ?? 0
                // Na aba "Abertas", todo card é comanda aberta (permite editar e excluir mesmo se comanda = 0)
                const isAberta = filtroStatus === 'abertas' || (numComanda !== 0 && comandasAbertasSet.has(numComanda))
                const isExcluida = filtroStatus === 'excluidas'
                const isFiltroAbertas = filtroStatus === 'abertas'
                const isFiltroFechadas = filtroStatus === 'fechadas'

                const cardKey = (venda.nota && String(venda.nota).trim()) ? `${venda.nota}-${index}` : `comanda-${numComanda}-${index}`
                return (
                  <motion.div
                    key={cardKey}
                    initial={{ scale: 0.9, opacity: 0 }}
                    animate={{ scale: 1, opacity: 1 }}
                    whileHover={isExcluida ? undefined : { scale: 1.05, y: -2 }}
                    className={`aspect-square rounded-2xl p-3 sm:p-4 flex flex-col items-center justify-between shadow-lg relative overflow-hidden transition-all ${
                      isExcluida
                        ? 'bg-gradient-to-br from-gray-500 to-gray-600 text-white'
                        : isFiltroAbertas
                        ? 'bg-gradient-to-br from-red-500 to-rose-600 text-white hover:from-red-600 hover:to-rose-700 hover:shadow-xl'
                        : isFiltroFechadas
                        ? 'bg-gradient-to-br from-green-500 to-emerald-600 text-white hover:from-green-600 hover:to-emerald-700 hover:shadow-xl'
                        : isAberta
                        ? 'bg-gradient-to-br from-red-500 to-rose-600 text-white hover:from-red-600 hover:to-rose-700 hover:shadow-xl'
                        : 'bg-gradient-to-br from-green-500 to-emerald-600 text-white hover:from-green-600 hover:to-emerald-700 hover:shadow-xl'
                    }`}
                  >
                    {/* Reflexo sutil no canto superior */}
                    <div className="absolute top-0 right-0 w-16 h-16 bg-white/10 rounded-bl-full pointer-events-none" />
                    {/* Botão Excluir - apenas comandas abertas (não em excluídas) */}
                    {isAberta && !isExcluida && (
                      <div className="absolute top-2 left-2 z-10">
                        <button
                          onClick={(e) => handleAbrirModalExcluir(venda, e)}
                          className="p-1.5 bg-black/50 hover:bg-red-700 rounded-full shadow-lg"
                          title="Excluir comanda (pede senha)"
                        >
                          <Trash2 className="w-3 h-3 sm:w-4 sm:h-4" />
                        </button>
                      </div>
                    )}
                    {/* Botões WhatsApp - aparece apenas se tiver telefone e comanda estiver aberta */}
                    {isAberta && venda.telefoneCliente && formatarNumeroTelefone(venda.telefoneCliente) && (
                      <div className="absolute top-2 right-2 flex gap-1 z-10">
                        <button
                          onClick={(e) => handleEnviarWhatsApp(venda, e)}
                          disabled={enviandoWhatsApp === venda.nota}
                          className="p-1.5 bg-green-600 hover:bg-green-700 rounded-full shadow-lg disabled:opacity-50 disabled:cursor-not-allowed"
                          title={enviandoWhatsApp === venda.nota ? "Enviando..." : "Enviar via WhatsApp"}
                        >
                          {enviandoWhatsApp === venda.nota ? (
                            <Loader2 className="w-3 h-3 sm:w-4 sm:h-4 animate-spin" />
                          ) : (
                            <MessageCircle className="w-3 h-3 sm:w-4 sm:h-4" />
                          )}
                        </button>
                        <button
                          onClick={(e) => handleCopiarMensagem(venda, e)}
                          className="p-1.5 bg-blue-600 hover:bg-blue-700 rounded-full shadow-lg"
                          title="Copiar mensagem"
                        >
                          <Copy className="w-3 h-3 sm:w-4 sm:h-4" />
                        </button>
                      </div>
                    )}
                    
                    <motion.button
                      onClick={() => !isExcluida && handleComandaClick(venda)}
                      disabled={isExcluida}
                      className={`w-full h-full flex flex-col items-center justify-between ${isExcluida ? 'cursor-default' : 'cursor-pointer'}`}
                    >
                      <div className="flex items-center justify-between w-full">
                        {isExcluida ? (
                          <Trash2 className="w-4 h-4 sm:w-5 sm:h-5 opacity-80" />
                        ) : isAberta || !isFiltroAbertas ? (
                          isFiltroAbertas || isAberta ? (
                            <Circle className="w-4 h-4 sm:w-5 sm:h-5" />
                          ) : (
                            <CheckCircle2 className="w-4 h-4 sm:w-5 sm:h-5" />
                          )
                        ) : null}
                        <span className="text-xs sm:text-sm font-bold">
                          #{numComanda.toString().padStart(3, '0')}
                        </span>
                      </div>

                    <div className="text-center flex-1 flex flex-col justify-center">
                      <p className="text-xs sm:text-sm font-semibold mb-1">
                        {venda.mesa ? `Mesa ${venda.mesa}` : 'Comanda'}
                      </p>
                      {venda.nomeCliente && (
                        <p className="text-xs font-medium text-primary mb-1">
                          {venda.nomeCliente}
                        </p>
                      )}
                      {venda.numeroPessoas && (
                        <p className="text-xs opacity-90">
                          {venda.numeroPessoas} {venda.numeroPessoas === 1 ? 'pessoa' : 'pessoas'}
                        </p>
                      )}
                    </div>

                    <div className="text-center w-full">
                      <p className="text-xs sm:text-sm font-bold">
                        {formatarPreco(venda.total || 0)}
                      </p>
                      {isExcluida ? (
                        <p className="text-xs opacity-90 mt-1">Excluída</p>
                      ) : isAberta && filtroStatus === 'abertas' ? (
                        <p className="text-xs opacity-90 mt-1">Toque para editar</p>
                      ) : null}
                    </div>
                    </motion.button>
                  </motion.div>
                )
              })}
            </div>

            {/* Resumo */}
            <div className="mt-6 bg-card rounded-2xl p-4 shadow-lg border border-border">
              <div className="flex flex-wrap justify-between items-center gap-4 text-sm">
                <div>
                  <span className="text-text-secondary">Total de comandas: </span>
                  <span className="font-bold text-text-primary">{comandasParaExibir.length}</span>
                </div>
                <div>
                  <span className="text-text-secondary">Valor total: </span>
                  <span className="font-bold text-primary">
                    {formatarPreco(
                      comandasParaExibir.reduce((acc, v) => acc + (v.total || 0), 0)
                    )}
                  </span>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
    <ConectarWhatsAppModal
      isOpen={showConectarWhatsAppModal}
      onClose={() => setShowConectarWhatsAppModal(false)}
    />

    {/* Modal Excluir Comanda - pede justificativa e senha de cancelamento */}
    {showExcluirModal && vendaParaExcluir && (
      <div className="fixed inset-0 z-[200] flex items-center justify-center bg-black/50 p-4" onClick={() => !excluindo && setShowExcluirModal(false)}>
        <div className="bg-card border border-border rounded-2xl shadow-xl max-w-sm w-full p-6 space-y-4" onClick={e => e.stopPropagation()}>
          <h3 className="text-lg font-semibold text-text-primary">Cancelar comanda</h3>
          <p className="text-sm text-text-secondary">
            Comanda <strong>#{String(vendaParaExcluir.comanda || '').padStart(3, '0')}</strong> (nota {vendaParaExcluir.nota}). 
            O cancelamento ficará registrado no histórico. Preencha os campos abaixo.
          </p>
          <form onSubmit={(e) => { e.preventDefault(); handleConfirmarExcluir(); }}>
          <div className="space-y-1">
            <label className="text-xs font-medium text-text-secondary">Justificativa <span className="text-red-500">*</span></label>
            <textarea
              value={justificativaExcluir}
              onChange={e => setJustificativaExcluir(e.target.value)}
              placeholder="Motivo do cancelamento"
              rows={3}
              className="w-full px-4 py-3 bg-background border border-border rounded-xl text-text-primary placeholder-text-muted resize-none"
              autoFocus
              disabled={excluindo}
            />
          </div>
          <div className="space-y-1">
            <label className="text-xs font-medium text-text-secondary">Senha de cancelamento <span className="text-red-500">*</span></label>
            <input
              type="password"
              value={senhaExcluir}
              onChange={e => { setSenhaExcluir(e.target.value); setErroExcluir(''); }}
              placeholder="Senha configurada nos parâmetros"
              className={`w-full px-4 py-3 bg-background border rounded-xl text-text-primary placeholder-text-muted ${erroExcluir ? 'border-red-500' : 'border-border'}`}
              disabled={excluindo}
            />
            <p className={`text-xs text-red-500 mt-1 font-medium ${erroExcluir ? '' : 'hidden'}`}>{erroExcluir}</p>
          </div>
          <div className="flex gap-3 mt-4">
            <button
              type="button"
              onClick={() => { setShowExcluirModal(false); setVendaParaExcluir(null); setSenhaExcluir(''); setJustificativaExcluir(''); setErroExcluir(''); }}
              disabled={excluindo}
              className="flex-1 py-2.5 rounded-xl border border-border bg-muted text-text-primary font-medium hover:bg-muted/80"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={excluindo || !senhaExcluir.trim() || !justificativaExcluir.trim()}
              className="flex-1 py-2.5 rounded-xl bg-red-600 text-white font-medium hover:bg-red-700 disabled:opacity-50 flex items-center justify-center gap-2"
            >
              {excluindo ? <Loader2 className="w-4 h-4 animate-spin" /> : <Trash2 className="w-4 h-4" />}
              Confirmar
            </button>
          </div>
          </form>
        </div>
      </div>
    )}
  </>
  )
}

export default GridComandas

