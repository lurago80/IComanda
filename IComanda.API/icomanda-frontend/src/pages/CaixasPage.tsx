import React, { useState, useEffect } from 'react';
import { DollarSign, ArrowLeft, Plus, X, Eye, Calendar, Search, ArrowDown, ArrowUp, Receipt } from 'lucide-react';
import { Button } from '../components/ui/button';
import { caixasService, caixaMovimentosService } from '../services/api';
import { useToast } from '../hooks/useToast';

interface CaixasPageProps {
  onClose: () => void;
}

interface CaixaMovimento {
  codigo: number;
  data?: string;
  hora?: string;
  documento?: string;
  entrada: number;
  saida: number;
  origem?: string;
  historico?: string;
}

interface Caixa {
  id: number;
  numero: number;
  dataAbertura: string;
  dataFechamento?: string;
  operadorAbertura: number;
  nomeOperadorAbertura?: string;
  operadorFechamento?: number;
  nomeOperadorFechamento?: string;
  valorAbertura: number;
  valorFechamento?: number;
  status: string;
  observacoes?: string;
  totalVendas: number;
  totalRecebimentos: number;
  totalDinheiro: number;
  totalCartao: number;
  totalPix: number;
  saldoEsperado: number;
  diferenca: number;
  totalSaidas?: number;
  movimentos?: CaixaMovimento[];
}

const CaixasPage: React.FC<CaixasPageProps> = ({ onClose }) => {
  const [caixas, setCaixas] = useState<Caixa[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAbrirModal, setShowAbrirModal] = useState(false);
  const [showFecharModal, setShowFecharModal] = useState(false);
  const [showRelatorioModal, setShowRelatorioModal] = useState(false);
  const [showSuprimentoModal, setShowSuprimentoModal] = useState(false);
  const [showSangriaModal, setShowSangriaModal] = useState(false);
  const [showDespesaModal, setShowDespesaModal] = useState(false);
  const [caixaSelecionado, setCaixaSelecionado] = useState<Caixa | null>(null);
  const [terminalSelecionado, setTerminalSelecionado] = useState<number>(1);
  const hoje = new Date().toISOString().slice(0, 10);
  const [dataInicio, setDataInicio] = useState<string>(hoje);
  const [dataFim, setDataFim] = useState<string>(hoje);
  const { showError, showSuccess } = useToast();

  // Formulário de abertura
  const [numeroCaixa, setNumeroCaixa] = useState<number>(1);
  const [valorAbertura, setValorAbertura] = useState<string>('0.00');
  const [observacoesAbertura, setObservacoesAbertura] = useState<string>('');
  // Info "caixa já aberto hoje" (preenchida ao abrir o modal)
  const [caixaJaAbertoInfo, setCaixaJaAbertoInfo] = useState<{ numero: number; valorAbertura: number } | null>(null);

  // Formulário de fechamento
  const [valorFechamento, setValorFechamento] = useState<string>('0.00');
  const [observacoesFechamento, setObservacoesFechamento] = useState<string>('');

  // Formulários de movimentos
  const [valorMovimento, setValorMovimento] = useState<string>('0.00');
  const [documentoMovimento, setDocumentoMovimento] = useState<string>('');
  const [historicoMovimento, setHistoricoMovimento] = useState<string>('');

  useEffect(() => {
    carregarCaixas();
  }, [dataInicio, dataFim]);

  // Ao abrir o modal "Abrir Caixa", verificar se este caixa já está aberto hoje
  useEffect(() => {
    if (!showAbrirModal) {
      setCaixaJaAbertoInfo(null);
      return;
    }
    const hojeStr = new Date().toISOString().slice(0, 10);
    caixasService.listar(hojeStr, hojeStr)
      .then((lista: Caixa[]) => {
        const aberto = lista.find((c: Caixa) => c.numero === numeroCaixa && c.status === 'ABERTO');
        if (aberto != null) {
          setCaixaJaAbertoInfo({ numero: numeroCaixa, valorAbertura: aberto.valorAbertura ?? 0 });
        } else {
          setCaixaJaAbertoInfo(null);
        }
      })
      .catch(() => setCaixaJaAbertoInfo(null));
  }, [showAbrirModal, numeroCaixa]);

  const carregarCaixas = async () => {
    try {
      setLoading(true);
      const data = await caixasService.listar(
        dataInicio || undefined,
        dataFim || undefined
      );
      setCaixas(data);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar os caixas');
      console.error('Erro ao carregar caixas:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleAbrirCaixa = async () => {
    try {
      const usuario = localStorage.getItem('usuario_logado');
      if (!usuario) {
        showError('Erro', 'Usuário não identificado');
        return;
      }

      const valorNum = Number(String(valorAbertura).replace(',', '.'));
      if (Number.isNaN(valorNum) || valorNum < 0) {
        showError('Erro', 'Informe um valor inicial válido (número maior ou igual a zero).');
        return;
      }

      const operador = Number(JSON.parse(usuario).id) || 1;

      await caixaMovimentosService.abrirCaixa({
        terminal: numeroCaixa,
        valor: valorNum,
        operador: operador,
        historico: observacoesAbertura || undefined
      });

      showSuccess('Sucesso', 'Caixa aberto com sucesso!');
      setShowAbrirModal(false);
      setNumeroCaixa(1);
      setValorAbertura('0.00');
      setObservacoesAbertura('');
      carregarCaixas();
    } catch (error: any) {
      const msg = error.response?.data?.mensagem || error.message || 'Não foi possível abrir o caixa';
      showError('Erro ao abrir caixa', msg);
    }
  };

  const handleSuprimento = async () => {
    try {
      const usuario = localStorage.getItem('usuario_logado');
      if (!usuario) {
        showError('Erro', 'Usuário não identificado');
        return;
      }

      const operador = JSON.parse(usuario).id || 1;
      
      await caixaMovimentosService.suprimento({
        terminal: terminalSelecionado,
        valor: parseFloat(valorMovimento.replace(',', '.')),
        operador: operador,
        documento: documentoMovimento || undefined,
        historico: historicoMovimento || undefined
      });

      showSuccess('Sucesso', 'Suprimento registrado com sucesso!');
      setShowSuprimentoModal(false);
      limparFormularioMovimento();
      carregarCaixas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível registrar o suprimento');
    }
  };

  const handleSangria = async () => {
    try {
      const usuario = localStorage.getItem('usuario_logado');
      if (!usuario) {
        showError('Erro', 'Usuário não identificado');
        return;
      }

      const operador = JSON.parse(usuario).id || 1;
      
      await caixaMovimentosService.sangria({
        terminal: terminalSelecionado,
        valor: parseFloat(valorMovimento.replace(',', '.')),
        operador: operador,
        documento: documentoMovimento || undefined,
        historico: historicoMovimento || undefined
      });

      showSuccess('Sucesso', 'Sangria registrada com sucesso!');
      setShowSangriaModal(false);
      limparFormularioMovimento();
      carregarCaixas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível registrar a sangria');
    }
  };

  const handleDespesa = async () => {
    try {
      const usuario = localStorage.getItem('usuario_logado');
      if (!usuario) {
        showError('Erro', 'Usuário não identificado');
        return;
      }

      const operador = JSON.parse(usuario).id || 1;
      
      await caixaMovimentosService.despesa({
        terminal: terminalSelecionado,
        valor: parseFloat(valorMovimento.replace(',', '.')),
        operador: operador,
        documento: documentoMovimento || undefined,
        historico: historicoMovimento || undefined
      });

      showSuccess('Sucesso', 'Pagamento de despesa registrado com sucesso!');
      setShowDespesaModal(false);
      limparFormularioMovimento();
      carregarCaixas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível registrar o pagamento de despesa');
    }
  };

  const limparFormularioMovimento = () => {
    setValorMovimento('0.00');
    setDocumentoMovimento('');
    setHistoricoMovimento('');
    setTerminalSelecionado(1);
  };

  const handleFecharCaixa = async () => {
    if (!caixaSelecionado) return;

    try {
      const usuario = localStorage.getItem('usuario_logado');
      if (!usuario) {
        showError('Erro', 'Usuário não identificado');
        return;
      }

      const operador = JSON.parse(usuario).id || 1;

      await caixasService.fechar({
        id: caixaSelecionado.id,
        operador,
        valorFechamento: parseFloat(valorFechamento.replace(',', '.')),
        observacoes: observacoesFechamento || undefined
      });

      showSuccess('Sucesso', 'Caixa fechado com sucesso!');
      setShowFecharModal(false);
      setCaixaSelecionado(null);
      setValorFechamento('0.00');
      setObservacoesFechamento('');
      carregarCaixas();
    } catch (error: any) {
      showError('Erro', error.response?.data?.mensagem || 'Não foi possível fechar o caixa');
    }
  };

  const handleVerRelatorio = async (caixa: Caixa) => {
    try {
      const relatorio = await caixasService.getRelatorio(
        caixa.id,
        dataInicio || undefined,
        dataFim || undefined
      );
      setCaixaSelecionado(relatorio);
      setShowRelatorioModal(true);
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar o relatório');
    }
  };

  const formatarData = (data: string) => {
    return new Date(data).toLocaleString('pt-BR');
  };

  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor);
  };

  return (
    <div className="min-h-screen bg-background p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <DollarSign className="w-8 h-8 text-primary" />
            <h1 className="text-3xl font-bold">Gestão de Caixas</h1>
          </div>
          <div className="flex items-center space-x-2">
            <Button onClick={() => setShowAbrirModal(true)} className="bg-primary">
              <Plus className="w-4 h-4 mr-2" />
              Abrir Caixa
            </Button>
            <Button 
              onClick={() => {
                setTerminalSelecionado(1);
                setShowSuprimentoModal(true);
              }} 
              variant="outline"
              className="bg-green-50 hover:bg-green-100 text-green-700 border-green-300"
            >
              <ArrowDown className="w-4 h-4 mr-2" />
              Suprimento
            </Button>
            <Button 
              onClick={() => {
                setTerminalSelecionado(1);
                setShowSangriaModal(true);
              }} 
              variant="outline"
              className="bg-orange-50 hover:bg-orange-100 text-orange-700 border-orange-300"
            >
              <ArrowUp className="w-4 h-4 mr-2" />
              Sangria
            </Button>
            <Button 
              onClick={() => {
                setTerminalSelecionado(1);
                setShowDespesaModal(true);
              }} 
              variant="outline"
              className="bg-red-50 hover:bg-red-100 text-red-700 border-red-300"
            >
              <Receipt className="w-4 h-4 mr-2" />
              Despesa
            </Button>
            <Button onClick={onClose} variant="outline">
              <ArrowLeft className="w-4 h-4 mr-2" />
              Voltar
            </Button>
          </div>
        </div>

        {/* Filtros */}
        <div className="bg-card rounded-lg p-4 mb-6 shadow-lg">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium mb-2">Data Início</label>
              <input
                type="date"
                value={dataInicio}
                onChange={(e) => setDataInicio(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-2">Data Fim</label>
              <input
                type="date"
                value={dataFim}
                onChange={(e) => setDataFim(e.target.value)}
                className="w-full px-3 py-2 border rounded-lg"
              />
            </div>
            <div className="flex items-end">
              <Button onClick={carregarCaixas} variant="outline" className="w-full">
                <Search className="w-4 h-4 mr-2" />
                Filtrar
              </Button>
            </div>
          </div>
        </div>

        {/* Lista de Caixas */}
        {loading ? (
          <div className="text-center py-12">
            <p className="text-text-secondary">Carregando...</p>
          </div>
        ) : caixas.length === 0 ? (
          <div className="bg-card rounded-lg p-12 text-center shadow-lg">
            <p className="text-text-secondary">Nenhum caixa encontrado no período.</p>
            <p className="text-sm text-text-muted mt-2">Se você abriu um caixa hoje, use <strong>Data Início</strong> e <strong>Data Fim</strong> do dia de hoje nos filtros acima para ver o caixa aberto e o valor de abertura.</p>
          </div>
        ) : (
          <div className="space-y-4">
            {caixas.map((caixa) => (
              <div
                key={caixa.id}
                className="bg-card rounded-lg p-6 shadow-lg border-l-4 border-primary"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="flex items-center space-x-3 mb-4">
                      <h3 className="text-xl font-bold">Caixa #{caixa.numero}</h3>
                      <span
                        className={`px-3 py-1 rounded-full text-sm font-semibold ${
                          caixa.status === 'ABERTO'
                            ? 'bg-green-100 text-green-800'
                            : 'bg-gray-100 text-gray-800'
                        }`}
                      >
                        {caixa.status}
                      </span>
                    </div>

                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-4">
                      <div>
                        <p className="text-sm text-text-secondary">Abertura</p>
                        <p className="font-semibold">{formatarData(caixa.dataAbertura)}</p>
                        <p className="text-sm text-text-muted">
                          {caixa.nomeOperadorAbertura || `Operador ${caixa.operadorAbertura}`}
                        </p>
                      </div>
                      {caixa.dataFechamento && (
                        <div>
                          <p className="text-sm text-text-secondary">Fechamento</p>
                          <p className="font-semibold">{formatarData(caixa.dataFechamento)}</p>
                          <p className="text-sm text-text-muted">
                            {caixa.nomeOperadorFechamento || `Operador ${caixa.operadorFechamento}`}
                          </p>
                        </div>
                      )}
                      <div>
                        <p className="text-sm text-text-secondary">Valor Abertura</p>
                        <p className="font-semibold text-primary">{formatarMoeda(caixa.valorAbertura)}</p>
                      </div>
                      {(caixa.totalSaidas ?? 0) > 0 && (
                        <div>
                          <p className="text-sm text-text-secondary">Saídas (sangria/despesa)</p>
                          <p className="font-semibold text-red-600">{formatarMoeda(caixa.totalSaidas ?? 0)}</p>
                        </div>
                      )}
                      {caixa.valorFechamento !== undefined && (
                        <div>
                          <p className="text-sm text-text-secondary">Valor Fechamento</p>
                          <p className="font-semibold">{formatarMoeda(caixa.valorFechamento)}</p>
                        </div>
                      )}
                    </div>

                    {caixa.status === 'FECHADO' && (
                      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-4 pt-4 border-t">
                        <div>
                          <p className="text-sm text-text-secondary">Total Vendas</p>
                          <p className="font-semibold">{formatarMoeda(caixa.totalVendas)}</p>
                        </div>
                        <div>
                          <p className="text-sm text-text-secondary">Total Recebimentos</p>
                          <p className="font-semibold">{formatarMoeda(caixa.totalRecebimentos)}</p>
                        </div>
                        <div>
                          <p className="text-sm text-text-secondary">Saldo Esperado</p>
                          <p className="font-semibold">{formatarMoeda(caixa.saldoEsperado)}</p>
                        </div>
                        <div>
                          <p className="text-sm text-text-secondary">Diferença</p>
                          <p
                            className={`font-semibold ${
                              caixa.diferenca >= 0 ? 'text-green-600' : 'text-red-600'
                            }`}
                          >
                            {formatarMoeda(caixa.diferenca)}
                          </p>
                        </div>
                      </div>
                    )}
                  </div>

                  <div className="flex flex-col space-y-2 ml-4">
                    {caixa.status === 'ABERTO' && (
                      <Button
                        onClick={() => {
                          setCaixaSelecionado(caixa);
                          setValorFechamento(caixa.saldoEsperado.toFixed(2));
                          setShowFecharModal(true);
                        }}
                        variant="outline"
                        className="bg-red-50 hover:bg-red-100 text-red-600 border-red-200"
                      >
                        <X className="w-4 h-4 mr-2" />
                        Fechar
                      </Button>
                    )}
                    <Button
                      onClick={() => handleVerRelatorio(caixa)}
                      variant="outline"
                    >
                      <Eye className="w-4 h-4 mr-2" />
                      Relatório
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}

        {/* Modal Abrir Caixa */}
        {showAbrirModal && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-card rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
              <h2 className="text-2xl font-bold mb-4">Abrir Caixa</h2>
              {caixaJaAbertoInfo != null && (
                <div className="mb-4 p-4 rounded-lg bg-amber-50 border border-amber-200 text-amber-800">
                  <p className="font-semibold">O Caixa #{caixaJaAbertoInfo.numero} já está aberto hoje.</p>
                  <p className="mt-1 text-sm">Valor de abertura: <strong>{formatarMoeda(caixaJaAbertoInfo.valorAbertura)}</strong></p>
                  <p className="mt-2 text-sm">A informação também aparece na lista de caixas abaixo (filtre pela data de hoje).</p>
                </div>
              )}
              <form onSubmit={(e) => { e.preventDefault(); handleAbrirCaixa(); }}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-2">Número do Caixa</label>
                  <input
                    type="number"
                    value={numeroCaixa}
                    onChange={(e) => setNumeroCaixa(parseInt(e.target.value) || 1)}
                    className="w-full px-3 py-2 border rounded-lg"
                    min="1"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Valor Inicial (R$)</label>
                  <input
                    type="text"
                    value={valorAbertura}
                    onChange={(e) => {
                      const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                      setValorAbertura(value);
                    }}
                    className="w-full px-3 py-2 border rounded-lg"
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Observações (opcional)</label>
                  <textarea
                    value={observacoesAbertura}
                    onChange={(e) => setObservacoesAbertura(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                    rows={3}
                  />
                </div>
              </div>
              <div className="flex space-x-2 mt-6">
                <Button type="submit" className="flex-1 bg-primary">
                  Abrir Caixa
                </Button>
                <Button
                  type="button"
                  onClick={() => {
                    setShowAbrirModal(false);
                    setNumeroCaixa(1);
                    setValorAbertura('0.00');
                    setObservacoesAbertura('');
                  }}
                  variant="outline"
                  className="flex-1"
                >
                  Cancelar
                </Button>
              </div>
              </form>
            </div>
          </div>
        )}

        {/* Modal Fechar Caixa */}
        {showFecharModal && caixaSelecionado && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-card rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
              <h2 className="text-2xl font-bold mb-4">Fechar Caixa #{caixaSelecionado.numero}</h2>
              <form onSubmit={(e) => { e.preventDefault(); handleFecharCaixa(); }}>
              <div className="space-y-4">
                <div className="bg-primary/10 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary mb-1">Saldo Esperado</p>
                  <p className="text-2xl font-bold text-primary">
                    {formatarMoeda(caixaSelecionado.saldoEsperado)}
                  </p>
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Valor Contado (R$)</label>
                  <input
                    type="text"
                    value={valorFechamento}
                    onChange={(e) => {
                      const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                      setValorFechamento(value);
                    }}
                    className="w-full px-3 py-2 border rounded-lg"
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Observações (opcional)</label>
                  <textarea
                    value={observacoesFechamento}
                    onChange={(e) => setObservacoesFechamento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                    rows={3}
                  />
                </div>
              </div>
              <div className="flex space-x-2 mt-6">
                <Button type="submit" className="flex-1 bg-red-600 hover:bg-red-700">
                  Fechar Caixa
                </Button>
                <Button
                  type="button"
                  onClick={() => {
                    setShowFecharModal(false);
                    setCaixaSelecionado(null);
                    setValorFechamento('0.00');
                    setObservacoesFechamento('');
                  }}
                  variant="outline"
                  className="flex-1"
                >
                  Cancelar
                </Button>
              </div>
              </form>
            </div>
          </div>
        )}

        {/* Modal Relatório */}
        {showRelatorioModal && caixaSelecionado && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 overflow-y-auto">
            <div className="bg-card rounded-lg p-6 max-w-4xl w-full mx-4 my-8 shadow-xl">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-2xl font-bold">Relatório - Caixa #{caixaSelecionado.numero}</h2>
                <Button onClick={() => setShowRelatorioModal(false)} variant="outline">
                  <X className="w-4 h-4" />
                </Button>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                <div className="bg-primary/10 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary">Total Vendas</p>
                  <p className="text-xl font-bold">{formatarMoeda(caixaSelecionado.totalVendas)}</p>
                </div>
                <div className="bg-green-50 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary">Total Recebimentos</p>
                  <p className="text-xl font-bold text-green-600">
                    {formatarMoeda(caixaSelecionado.totalRecebimentos)}
                  </p>
                </div>
                <div className="bg-blue-50 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary">Dinheiro</p>
                  <p className="text-xl font-bold text-blue-600">
                    {formatarMoeda(caixaSelecionado.totalDinheiro)}
                  </p>
                </div>
                <div className="bg-purple-50 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary">Cartão</p>
                  <p className="text-xl font-bold text-purple-600">
                    {formatarMoeda(caixaSelecionado.totalCartao)}
                  </p>
                </div>
              </div>

              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                <div className="bg-yellow-50 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary">PIX</p>
                  <p className="text-xl font-bold text-yellow-600">
                    {formatarMoeda(caixaSelecionado.totalPix)}
                  </p>
                </div>
                <div className="bg-gray-50 p-4 rounded-lg">
                  <p className="text-sm text-text-secondary">Saldo Esperado</p>
                  <p className="text-xl font-bold">{formatarMoeda(caixaSelecionado.saldoEsperado)}</p>
                </div>
                <div
                  className={`p-4 rounded-lg ${
                    caixaSelecionado.diferenca >= 0 ? 'bg-green-50' : 'bg-red-50'
                  }`}
                >
                  <p className="text-sm text-text-secondary">Diferença</p>
                  <p
                    className={`text-xl font-bold ${
                      caixaSelecionado.diferenca >= 0 ? 'text-green-600' : 'text-red-600'
                    }`}
                  >
                    {formatarMoeda(caixaSelecionado.diferenca)}
                  </p>
                </div>
              </div>

              {/* Lista de movimentos (abertura, sangria, suprimento, despesa) */}
              {(caixaSelecionado.movimentos?.length ?? 0) > 0 && (
                <div className="mt-6 border-t pt-4">
                  <h3 className="font-semibold text-lg mb-3">Movimentos do caixa</h3>
                  <div className="overflow-x-auto border rounded-lg max-h-64 overflow-y-auto">
                    <table className="w-full text-sm">
                      <thead>
                        <tr className="border-b bg-slate-50">
                          <th className="text-left p-2">Data/Hora</th>
                          <th className="text-left p-2">Origem</th>
                          <th className="text-left p-2">Histórico</th>
                          <th className="text-right p-2">Entrada</th>
                          <th className="text-right p-2">Saída</th>
                        </tr>
                      </thead>
                      <tbody>
                        {(caixaSelecionado.movimentos ?? []).map((mov: any, i: number) => (
                          <tr key={mov.codigo ?? i} className="border-b hover:bg-slate-50/50">
                            <td className="p-2">
                              {mov.data ? formatarData(mov.data) : '-'}
                              {mov.hora != null && (
                                <span className="text-text-muted ml-1">
                                  {String(mov.hora).substring(0, 8)}
                                </span>
                              )}
                            </td>
                            <td className="p-2">{mov.origem ?? '-'}</td>
                            <td className="p-2">{mov.historico ?? '-'}</td>
                            <td className="p-2 text-right text-green-600">
                              {(mov.entrada ?? 0) > 0 ? formatarMoeda(mov.entrada) : '-'}
                            </td>
                            <td className="p-2 text-right text-red-600">
                              {(mov.saida ?? 0) > 0 ? formatarMoeda(mov.saida) : '-'}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              )}
            </div>
          </div>
        )}

        {/* Modal Suprimento */}
        {showSuprimentoModal && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-card rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
              <h2 className="text-2xl font-bold mb-4">Registrar Suprimento</h2>
              <form onSubmit={(e) => { e.preventDefault(); handleSuprimento(); }}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-2">Terminal</label>
                  <input
                    type="number"
                    value={terminalSelecionado}
                    onChange={(e) => setTerminalSelecionado(parseInt(e.target.value) || 1)}
                    className="w-full px-3 py-2 border rounded-lg"
                    min="1"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Valor (R$)</label>
                  <input
                    type="text"
                    value={valorMovimento}
                    onChange={(e) => {
                      const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                      setValorMovimento(value);
                    }}
                    className="w-full px-3 py-2 border rounded-lg"
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Documento (opcional)</label>
                  <input
                    type="text"
                    value={documentoMovimento}
                    onChange={(e) => setDocumentoMovimento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Histórico (opcional)</label>
                  <textarea
                    value={historicoMovimento}
                    onChange={(e) => setHistoricoMovimento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                    rows={3}
                  />
                </div>
              </div>
              <div className="flex space-x-2 mt-6">
                <Button type="submit" className="flex-1 bg-green-600 hover:bg-green-700">
                  Registrar
                </Button>
                <Button
                  type="button"
                  onClick={() => {
                    setShowSuprimentoModal(false);
                    limparFormularioMovimento();
                  }}
                  variant="outline"
                  className="flex-1"
                >
                  Cancelar
                </Button>
              </div>
              </form>
            </div>
          </div>
        )}

        {/* Modal Sangria */}
        {showSangriaModal && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-card rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
              <h2 className="text-2xl font-bold mb-4">Registrar Sangria</h2>
              <form onSubmit={(e) => { e.preventDefault(); handleSangria(); }}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-2">Terminal</label>
                  <input
                    type="number"
                    value={terminalSelecionado}
                    onChange={(e) => setTerminalSelecionado(parseInt(e.target.value) || 1)}
                    className="w-full px-3 py-2 border rounded-lg"
                    min="1"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Valor (R$)</label>
                  <input
                    type="text"
                    value={valorMovimento}
                    onChange={(e) => {
                      const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                      setValorMovimento(value);
                    }}
                    className="w-full px-3 py-2 border rounded-lg"
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Documento (opcional)</label>
                  <input
                    type="text"
                    value={documentoMovimento}
                    onChange={(e) => setDocumentoMovimento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Histórico (opcional)</label>
                  <textarea
                    value={historicoMovimento}
                    onChange={(e) => setHistoricoMovimento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                    rows={3}
                  />
                </div>
              </div>
              <div className="flex space-x-2 mt-6">
                <Button type="submit" className="flex-1 bg-orange-600 hover:bg-orange-700">
                  Registrar
                </Button>
                <Button
                  type="button"
                  onClick={() => {
                    setShowSangriaModal(false);
                    limparFormularioMovimento();
                  }}
                  variant="outline"
                  className="flex-1"
                >
                  Cancelar
                </Button>
              </div>
              </form>
            </div>
          </div>
        )}

        {/* Modal Despesa */}
        {showDespesaModal && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-card rounded-lg p-6 max-w-md w-full mx-4 shadow-xl">
              <h2 className="text-2xl font-bold mb-4">Registrar Pagamento de Despesa</h2>
              <form onSubmit={(e) => { e.preventDefault(); handleDespesa(); }}>
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium mb-2">Terminal</label>
                  <input
                    type="number"
                    value={terminalSelecionado}
                    onChange={(e) => setTerminalSelecionado(parseInt(e.target.value) || 1)}
                    className="w-full px-3 py-2 border rounded-lg"
                    min="1"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Valor (R$)</label>
                  <input
                    type="text"
                    value={valorMovimento}
                    onChange={(e) => {
                      const value = e.target.value.replace(/[^\d,]/g, '').replace(',', '.');
                      setValorMovimento(value);
                    }}
                    className="w-full px-3 py-2 border rounded-lg"
                    placeholder="0.00"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Documento (opcional)</label>
                  <input
                    type="text"
                    value={documentoMovimento}
                    onChange={(e) => setDocumentoMovimento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-2">Histórico (opcional)</label>
                  <textarea
                    value={historicoMovimento}
                    onChange={(e) => setHistoricoMovimento(e.target.value)}
                    className="w-full px-3 py-2 border rounded-lg"
                    rows={3}
                  />
                </div>
              </div>
              <div className="flex space-x-2 mt-6">
                <Button type="submit" className="flex-1 bg-red-600 hover:bg-red-700">
                  Registrar
                </Button>
                <Button
                  type="button"
                  onClick={() => {
                    setShowDespesaModal(false);
                    limparFormularioMovimento();
                  }}
                  variant="outline"
                  className="flex-1"
                >
                  Cancelar
                </Button>
              </div>
              </form>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default CaixasPage;
