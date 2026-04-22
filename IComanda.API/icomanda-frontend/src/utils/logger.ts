/**
 * Sistema de Logging Profissional
 * 
 * Em desenvolvimento: Mostra logs no console
 * Em produção: Silencioso (pode ser integrado com serviço de monitoramento)
 * 
 * USO:
 * import { logger } from '@/utils/logger';
 * 
 * logger.info('Mensagem informativa');
 * logger.warn('Aviso');
 * logger.error('Erro', error);
 * logger.debug('Debug detalhado', { data });
 */

type LogLevel = 'debug' | 'info' | 'warn' | 'error';

interface LoggerConfig {
  enabled: boolean;
  level: LogLevel;
  sendToMonitoring: boolean;
  monitoringUrl?: string;
}

class Logger {
  private config: LoggerConfig;

  constructor() {
    // Detecta ambiente: development ou production
    const isDevelopment = 
      typeof window !== 'undefined' && 
      (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1');
    
    this.config = {
      enabled: isDevelopment,
      level: isDevelopment ? 'debug' : 'error',
      sendToMonitoring: false, // TODO: Habilitar em produção quando tiver serviço
      monitoringUrl: undefined, // TODO: Configurar URL de monitoramento
    };
  }

  /**
   * Verifica se deve logar baseado no nível
   */
  private shouldLog(level: LogLevel): boolean {
    if (!this.config.enabled && level !== 'error') {
      return false;
    }

    const levels: LogLevel[] = ['debug', 'info', 'warn', 'error'];
    const currentLevelIndex = levels.indexOf(this.config.level);
    const messageLevelIndex = levels.indexOf(level);

    return messageLevelIndex >= currentLevelIndex;
  }

  /**
   * Formata a mensagem com timestamp e nível
   */
  private formatMessage(level: LogLevel, message: string, data?: any): string {
    const timestamp = new Date().toISOString();
    const emoji = {
      debug: '🔍',
      info: 'ℹ️',
      warn: '⚠️',
      error: '❌',
    }[level];

    return `${emoji} [${timestamp}] [${level.toUpperCase()}] ${message}`;
  }

  /**
   * Envia log para serviço de monitoramento (futuro)
   */
  private async sendToMonitoring(level: LogLevel, message: string, data?: any): Promise<void> {
    if (!this.config.sendToMonitoring || !this.config.monitoringUrl) {
      return;
    }

    try {
      // TODO: Integrar com Sentry, LogRocket, DataDog, etc
      await fetch(this.config.monitoringUrl, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          level,
          message,
          data,
          timestamp: new Date().toISOString(),
          userAgent: navigator.userAgent,
          url: window.location.href,
        }),
      });
    } catch (error) {
      // Falha silenciosa - não queremos quebrar a aplicação por falha no log
      console.error('Falha ao enviar log para monitoramento:', error);
    }
  }

  /**
   * Log de debug - apenas desenvolvimento
   */
  debug(message: string, data?: any): void {
    if (this.shouldLog('debug')) {
      console.log(this.formatMessage('debug', message), data ?? '');
    }
  }

  /**
   * Log informativo
   */
  info(message: string, data?: any): void {
    if (this.shouldLog('info')) {
      console.info(this.formatMessage('info', message), data ?? '');
    }
  }

  /**
   * Log de aviso
   */
  warn(message: string, data?: any): void {
    if (this.shouldLog('warn')) {
      console.warn(this.formatMessage('warn', message), data ?? '');
    }
    
    if (this.config.sendToMonitoring) {
      this.sendToMonitoring('warn', message, data);
    }
  }

  /**
   * Log de erro - sempre enviado para monitoramento em produção
   */
  error(message: string, error?: any, data?: any): void {
    const errorData = {
      ...data,
      error: error instanceof Error ? {
        name: error.name,
        message: error.message,
        stack: error.stack,
      } : error,
    };

    console.error(this.formatMessage('error', message), errorData);
    
    // Sempre envia erros para monitoramento
    if (this.config.monitoringUrl) {
      this.sendToMonitoring('error', message, errorData);
    }
  }

  /**
   * Mede performance de uma operação
   */
  time(label: string): void {
    if (this.config.enabled) {
      console.time(label);
    }
  }

  /**
   * Finaliza medição de performance
   */
  timeEnd(label: string): void {
    if (this.config.enabled) {
      console.timeEnd(label);
    }
  }

  /**
   * Agrupa logs relacionados
   */
  group(label: string): void {
    if (this.config.enabled) {
      console.group(label);
    }
  }

  /**
   * Finaliza agrupamento
   */
  groupEnd(): void {
    if (this.config.enabled) {
      console.groupEnd();
    }
  }

  /**
   * Loga objeto em formato de tabela
   */
  table(data: any): void {
    if (this.config.enabled) {
      console.table(data);
    }
  }
}

// Singleton
export const logger = new Logger();

// Export default para compatibilidade
export default logger;

/**
 * EXEMPLOS DE USO:
 * 
 * // Debug detalhado (só desenvolvimento)
 * logger.debug('Carregando produtos', { busca, filtros });
 * 
 * // Informação
 * logger.info('Produtos carregados com sucesso', { total: produtos.length });
 * 
 * // Aviso
 * logger.warn('Cliente não encontrado', { clienteId });
 * 
 * // Erro
 * try {
 *   await api.post('/vendas', venda);
 * } catch (error) {
 *   logger.error('Falha ao criar venda', error, { venda });
 * }
 * 
 * // Performance
 * logger.time('loadProdutos');
 * await carregarProdutos();
 * logger.timeEnd('loadProdutos');
 * 
 * // Agrupamento
 * logger.group('Processamento de Venda');
 * logger.info('Validando itens...');
 * logger.info('Calculando totais...');
 * logger.info('Salvando no banco...');
 * logger.groupEnd();
 */
