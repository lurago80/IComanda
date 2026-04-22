/**
 * Utilitários para envio de mensagens via WhatsApp Web
 */

/**
 * Formata um número de telefone removendo caracteres não numéricos
 */
export const formatarNumeroTelefone = (telefone: string | null | undefined): string | null => {
  if (!telefone) return null;
  
  // Remove tudo exceto números
  const apenasNumeros = telefone.replace(/\D/g, '');
  
  // Se não tiver números suficientes, retorna null
  if (apenasNumeros.length < 10) return null;
  
  return apenasNumeros;
};

/**
 * Gera link direto do WhatsApp Web com mensagem pré-formatada
 * Usa web.whatsapp.com para abrir diretamente sem página intermediária
 */
export const gerarLinkWhatsApp = (telefone: string, mensagem: string): string => {
  const numeroFormatado = formatarNumeroTelefone(telefone);
  if (!numeroFormatado) {
    throw new Error('Número de telefone inválido');
  }

  // Formatar mensagem para URL (encode)
  const mensagemEncoded = encodeURIComponent(mensagem);
  
  // Link direto do WhatsApp Web (sem página intermediária)
  // Formato: https://web.whatsapp.com/send?phone=5511999999999&text=mensagem
  return `https://web.whatsapp.com/send?phone=${numeroFormatado}&text=${mensagemEncoded}`;
};

/** Resultado do envio: 'sent' = enviado direto pela API; 'link' = link gerado (abrir em nova aba) */
export type ResultadoEnvioWhatsApp = { method: 'sent' | 'link'; link?: string };

/**
 * Abre o link do WhatsApp. Tenta nova aba; se o navegador bloquear (pop-up), abre na mesma aba.
 */
export const abrirLinkWhatsApp = (link: string): void => {
  if (!link || !link.startsWith('http')) return;
  const novaAba = window.open(link, '_blank', 'noopener,noreferrer');
  if (!novaAba || novaAba.closed) {
    window.location.href = link;
  }
};

/**
 * Envia mensagem via WhatsApp.
 * - Se o backend enviar direto (Evolution API/Selenium): method 'sent' = mensagem já enviada.
 * - Se o backend retornar link (método link direto): method 'link' + link = abrir em nova aba para o usuário enviar.
 */
export const enviarWhatsApp = async (telefone: string, mensagem: string): Promise<ResultadoEnvioWhatsApp> => {
  const numeroFormatado = formatarNumeroTelefone(telefone);
  if (!numeroFormatado) {
    throw new Error('Número de telefone inválido');
  }

  const { whatsAppService } = await import('../services/api');
  const resultado = await whatsAppService.enviarMensagem(telefone, mensagem);

  if (resultado.sucesso) {
    const link = resultado.link ?? (resultado as { Link?: string }).Link;
    if (link) {
      return { method: 'link', link };
    }
    return { method: 'sent' };
  }

  throw new Error(resultado.mensagem || 'Falha ao enviar mensagem');
};


/**
 * Copia mensagem para área de transferência
 * Copia apenas a mensagem (sem link) para facilitar colar no WhatsApp Web já aberto
 */
export const copiarMensagemWhatsApp = async (telefone: string, mensagem: string): Promise<boolean> => {
  try {
    const numeroFormatado = formatarNumeroTelefone(telefone);
    if (!numeroFormatado) {
      throw new Error('Número de telefone inválido');
    }

    // Copiar apenas a mensagem (sem link) para facilitar colar no WhatsApp Web
    // O usuário já terá o WhatsApp Web aberto, então só precisa colar e enviar
    const mensagemParaCopiar = mensagem;
    
    // Usar Clipboard API moderna
    if (navigator.clipboard && navigator.clipboard.writeText) {
      await navigator.clipboard.writeText(mensagemParaCopiar);
      return true;
    } else {
      // Fallback para navegadores antigos
      const textArea = document.createElement('textarea');
      textArea.value = mensagemParaCopiar;
      textArea.style.position = 'fixed';
      textArea.style.left = '-999999px';
      textArea.style.top = '-999999px';
      document.body.appendChild(textArea);
      textArea.focus();
      textArea.select();
      
      try {
        const successful = document.execCommand('copy');
        document.body.removeChild(textArea);
        return successful;
      } catch (err) {
        document.body.removeChild(textArea);
        return false;
      }
    }
  } catch (error) {
    console.error('Erro ao copiar mensagem:', error);
    return false;
  }
};

/**
 * Formata mensagem de comanda aberta ou pedido delivery
 */
export const formatarMensagemComanda = (venda: {
  nota: string;
  comanda?: number;
  mesa?: number;
  total: number;
  nomeCliente?: string;
  emissao?: string;
  hora?: string;
  origem?: string;
  nomeEstabelecimento?: string;
  itens?: Array<{ codigo?: number; descricao?: string; qtd: number; preco: number; total: number }>;
}): string => {
  const notaFormatada = venda.nota.padStart(6, '0');
  const saudacao = venda.nomeCliente ? `Olá ${venda.nomeCliente}!` : 'Olá!';
  
  // Formatar data e hora (sem milissegundos)
  let dataHora = '';
  if (venda.emissao) {
    try {
      const data = new Date(venda.emissao);
      const dataFormatada = data.toLocaleDateString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
      
      // Formatar hora com segundos (sem milissegundos)
      let horaFormatada = '';
      if (venda.hora) {
        // Se hora vem como string, tentar parsear
        try {
          const horaDate = new Date(`2000-01-01T${venda.hora}`);
          horaFormatada = horaDate.toLocaleTimeString('pt-BR', { 
            hour: '2-digit', 
            minute: '2-digit',
            second: '2-digit'
          });
        } catch {
          horaFormatada = venda.hora;
        }
      } else {
        horaFormatada = data.toLocaleTimeString('pt-BR', { 
          hour: '2-digit', 
          minute: '2-digit',
          second: '2-digit'
        });
      }
      
      dataHora = `${dataFormatada} às ${horaFormatada}`;
    } catch (e) {
      // Se falhar, tentar limpar milissegundos manualmente
      const dataLimpa = venda.emissao.toString().replace(/\.\d+/, '');
      dataHora = dataLimpa;
    }
  }
  
  // Largura fixa: todas as linhas e os traços no mesmo tamanho; tudo sempre centralizado
  const LARGURA = 42;
  const SEP = '─'.repeat(LARGURA);

  const calcularLargura = (texto: string): number => {
    return texto.replace(/\*/g, '').replace(/_/g, '').replace(/[^\x20-\x7E]/g, ' ').length;
  };

  /** Centraliza uma linha dentro de LARGURA (exatamente LARGURA caracteres) */
  const centralizarUmaLinha = (texto: string): string => {
    const s = texto.trim();
    const larguraReal = calcularLargura(s);
    const espacosEsq = Math.max(0, Math.floor((LARGURA - larguraReal) / 2));
    const espacosDir = Math.max(0, LARGURA - larguraReal - espacosEsq);
    return ' '.repeat(espacosEsq) + s + ' '.repeat(espacosDir);
  };

  /** Centraliza sempre: se o texto couber em uma linha, centraliza; se não, quebra e centraliza cada parte */
  const centralizar = (texto: string): string => {
    const t = texto.trim();
    if (calcularLargura(t) <= LARGURA) return centralizarUmaLinha(t);
    const partes = quebrarEmDuasLinhas(t, LARGURA);
    return partes.map((p) => centralizarUmaLinha(p)).join('\n');
  };

  /** Quebra descrição longa em duas linhas (respeitando LARGURA para centralizar depois) */
  const quebrarEmDuasLinhas = (texto: string, maxChars: number = LARGURA): string[] => {
    const t = texto.trim();
    if (calcularLargura(t) <= maxChars) return [t];
    let lastSpace = -1;
    for (let i = 0; i < t.length; i++) {
      if (t[i] === ' ') lastSpace = i;
      if (calcularLargura(t.slice(0, i + 1)) > maxChars) break;
    }
    if (lastSpace <= 0) return [t];
    return [t.slice(0, lastSpace).trim(), t.slice(lastSpace).trim()];
  };

  const totalFormatado = venda.total.toFixed(2).replace('.', ',');

  const isDelivery = (venda.origem ?? '').toUpperCase() === 'DL';
  const nomeRodape = venda.nomeEstabelecimento?.trim() || 'Estabelecimento';

  // Tudo centralizado; sem linhas em branco sobrando; emojis nos títulos
  let mensagem = centralizar(`👋 ${saudacao}`) + '\n';
  mensagem += centralizar(isDelivery ? '🛵 *PEDIDO DELIVERY*' : '📋 *COMANDA EM ABERTO*') + '\n';

  if (dataHora) mensagem += centralizar(`📅 ${dataHora}`) + '\n';
  mensagem += centralizar(`🧾 Nota ${notaFormatada}`) + '\n';
  if (venda.comanda) mensagem += centralizar(`🎫 Comanda ${venda.comanda}`) + '\n';
  if (venda.mesa) mensagem += centralizar(`🪑 Mesa ${venda.mesa}`) + '\n';

  mensagem += SEP + '\n';
  mensagem += centralizar('🛒 *PRODUTOS*') + '\n';

  if (venda.itens && venda.itens.length > 0) {
    venda.itens.forEach((item, index) => {
      let descricao: string = item.descricao ?? '';
      if (!descricao.trim()) {
        descricao = (item as any).descricaoProduto ?? (item as any).produto?.descricao ?? `Produto ${item.codigo ?? index + 1}`;
      }
      descricao = (descricao ?? '').trim();
      // Garantir texto em UTF-8 normalizado (evita ? no lugar de acentos no WhatsApp)
      try {
        descricao = descricao.normalize('NFC');
      } catch {
        /* manter como está */
      }

      const qtd = item.qtd.toFixed(2).replace('.', ',');
      const preco = item.preco.toFixed(2).replace('.', ',');
      const totalItem = item.total.toFixed(2).replace('.', ',');

      const linhasDescricao = quebrarEmDuasLinhas(descricao, LARGURA - 4);
      mensagem += centralizar(`📦 *${index + 1}. ${linhasDescricao[0]}*`) + '\n';
      if (linhasDescricao.length > 1) mensagem += centralizar(linhasDescricao[1]) + '\n';
      mensagem += centralizar(`${qtd} × R$ ${preco} = R$ ${totalItem}`) + '\n';
    });
  } else {
    mensagem += centralizar('Nenhum item.') + '\n';
  }

  mensagem += SEP + '\n';
  mensagem += centralizar(`💰 *TOTAL  R$ ${totalFormatado}*`) + '\n';
  mensagem += centralizar('🙏 Obrigado pela preferência.') + '\n';
  mensagem += centralizar('💬 Qualquer dúvida, estamos à disposição.') + '\n';
  mensagem += centralizar(`✨ _${nomeRodape}_`);

  return mensagem;
};

/**
 * Formata mensagem de LEMBRETE amigável (não é cobrança) para conta a receber.
 * Tom: lembrete de que existe valor em aberto, sem pressão.
 */
export const formatarMensagemContaReceber = (conta: {
  numero: string;
  ordem: string;
  vencimento: string;
  valorPendente: number;
  diasVencidos?: number;
  nomeCliente?: string;
  nomeEstabelecimento?: string;
}): string => {
  const saudacao = conta.nomeCliente ? `Olá ${conta.nomeCliente}!` : 'Olá!';
  const vencimentoFormatado = new Date(conta.vencimento).toLocaleDateString('pt-BR');
  const valorFormatado = conta.valorPendente.toFixed(2).replace('.', ',');
  
  let mensagem = `${saudacao}\n\n`;
  mensagem += `📋 *Lembrete: valor em aberto*\n\n`;
  mensagem += `Este é um lembrete amigável de que você possui um valor em aberto conosco.\n\n`;
  mensagem += `Conta: ${conta.numero}/${conta.ordem}\n`;
  mensagem += `Vencimento: ${vencimentoFormatado}\n`;
  
  if (conta.diasVencidos && conta.diasVencidos > 0) {
    mensagem += `(Vencida há ${conta.diasVencidos} dia(s))\n`;
  }
  
  mensagem += `\n*Valor em aberto: R$ ${valorFormatado}*\n\n`;
  mensagem += `Quando puder, entre em contato para regularizar. Qualquer dúvida, estamos à disposição. 🙂\n\n`;
  mensagem += (conta.nomeEstabelecimento?.trim()) || 'Estabelecimento';
  
  return mensagem;
};
