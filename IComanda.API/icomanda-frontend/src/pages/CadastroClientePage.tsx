import { 
  Loader2, 
  X, 
  Save,
  User,
  MapPin,
  Phone,
  FileText
} from 'lucide-react';
import React, { useEffect, useState, useCallback } from 'react';
import { useToast } from '../hooks/useToast';
import { clientesService } from '../services/api';
import { Button } from '../components/ui/button';

interface CadastroClientePageProps {
  onClose: () => void;
  clienteId?: number;
}

const CadastroClientePage: React.FC<CadastroClientePageProps> = ({ onClose, clienteId }) => {
  const [loading, setLoading] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const { showSuccess, showError } = useToast();

  // Dados Básicos
  const [nome, setNome] = useState('');
  const [fantasia, setFantasia] = useState('');
  const [cpfCnpj, setCpfCnpj] = useState('');
  const [rgIe, setRgIe] = useState('');
  const [dataNascimento, setDataNascimento] = useState('');
  const [ativo, setAtivo] = useState(true);
  const [bloqueado, setBloqueado] = useState(false);

  // Contato
  const [telefone, setTelefone] = useState('');
  const [celular, setCelular] = useState('');
  const [email, setEmail] = useState('');
  const [email1, setEmail1] = useState('');

  // Endereço Principal
  const [endereco1, setEndereco1] = useState('');
  const [numero1, setNumero1] = useState('');
  const [complemento1, setComplemento1] = useState('');
  const [bairro1, setBairro1] = useState('');
  const [cidade1, setCidade1] = useState('');
  const [uf1, setUf1] = useState('');
  const [cep1, setCep1] = useState('');

  // Endereço Secundário
  const [endereco2, setEndereco2] = useState('');
  const [numero2, setNumero2] = useState('');
  const [complemento2, setComplemento2] = useState('');
  const [bairro2, setBairro2] = useState('');
  const [cidade2, setCidade2] = useState('');
  const [uf2, setUf2] = useState('');
  const [cep2, setCep2] = useState('');

  // Outros
  const [obs, setObs] = useState('');
  const [limite, setLimite] = useState('');
  const [classificacao, setClassificacao] = useState('');

  const carregarCliente = useCallback(async () => {
    if (!clienteId) return;
    
    try {
      setLoading(true);
      const cliente = await clientesService.getById(clienteId);
      
      setNome(cliente.nome || '');
      setFantasia(cliente.fantasia || '');
      setCpfCnpj(cliente.cpfCnpj || '');
      setRgIe(cliente.rgIe || '');
      setDataNascimento(cliente.dataNascimento ? new Date(cliente.dataNascimento).toISOString().split('T')[0] : '');
      setAtivo(cliente.ativo);
      setBloqueado(cliente.bloqueado);
      setTelefone(cliente.telefone || '');
      setCelular(cliente.celular || '');
      setEmail(cliente.email || '');
      setEmail1(cliente.email1 || '');
      setEndereco1(cliente.endereco1 || '');
      setNumero1(cliente.numero1 || '');
      setComplemento1(cliente.complemento1 || '');
      setBairro1(cliente.bairro1 || '');
      setCidade1(cliente.cidade1 || '');
      setUf1(cliente.uf1 || '');
      setCep1(cliente.cep1 || '');
      setEndereco2(cliente.endereco2 || '');
      setNumero2(cliente.numero2 || '');
      setComplemento2(cliente.complemento2 || '');
      setBairro2(cliente.bairro2 || '');
      setCidade2(cliente.cidade2 || '');
      setUf2(cliente.uf2 || '');
      setCep2(cliente.cep2 || '');
      setObs(cliente.obs || '');
      setLimite(cliente.limite?.toString() || '');
      setClassificacao(cliente.classificacao || '');
    } catch (error: any) {
      showError('Erro', 'Não foi possível carregar os dados do cliente');
      console.error('Erro ao carregar cliente:', error);
    } finally {
      setLoading(false);
    }
  }, [clienteId, showError]);

  useEffect(() => {
    if (clienteId) {
      carregarCliente();
    }
  }, [clienteId, carregarCliente]);

  const formatarCpfCnpj = (value: string) => {
    const limpo = value.replace(/\D/g, '');
    if (limpo.length <= 11) {
      return limpo.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    } else {
      return limpo.replace(/(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})/, '$1.$2.$3/$4-$5');
    }
  };

  const formatarTelefone = (value: string) => {
    const limpo = value.replace(/\D/g, '');
    if (limpo.length <= 10) {
      return limpo.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
    } else {
      return limpo.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
    }
  };

  const formatarCep = (value: string) => {
    const limpo = value.replace(/\D/g, '');
    return limpo.replace(/(\d{5})(\d{3})/, '$1-$2');
  };

  const handleSalvar = async () => {
    if (!nome.trim()) {
      showError('Erro', 'Nome é obrigatório');
      return;
    }

    try {
      setSalvando(true);
      const dados = {
        nome: nome.trim(),
        fantasia: fantasia.trim() || undefined,
        cpfCnpj: cpfCnpj.replace(/\D/g, '') || undefined,
        rgIe: rgIe.trim() || undefined,
        dataNascimento: dataNascimento || undefined,
        ativo: ativo ? 1 : 0,
        bloqueado: bloqueado ? 1 : 0,
        telefone: telefone.replace(/\D/g, '') || undefined,
        celular: celular.replace(/\D/g, '') || undefined,
        email: email.trim() || undefined,
        email1: email1.trim() || undefined,
        endereco1: endereco1.trim() || undefined,
        numero1: numero1.trim() || undefined,
        complemento1: complemento1.trim() || undefined,
        bairro1: bairro1.trim() || undefined,
        cidade1: cidade1.trim() || undefined,
        uf1: uf1.trim() || undefined,
        cep1: cep1.replace(/\D/g, '') || undefined,
        endereco2: endereco2.trim() || undefined,
        numero2: numero2.trim() || undefined,
        complemento2: complemento2.trim() || undefined,
        bairro2: bairro2.trim() || undefined,
        cidade2: cidade2.trim() || undefined,
        uf2: uf2.trim() || undefined,
        cep2: cep2.replace(/\D/g, '') || undefined,
        obs: obs.trim() || undefined,
        limite: limite ? parseFloat(limite.replace(',', '.')) : undefined,
        classificacao: classificacao.trim() || undefined
      };

      if (clienteId) {
        await clientesService.atualizar(clienteId, dados);
        showSuccess('Sucesso', 'Cliente atualizado com sucesso!');
      } else {
        await clientesService.criar(dados);
        showSuccess('Sucesso', 'Cliente cadastrado com sucesso!');
      }

      setTimeout(() => {
        onClose();
      }, 1000);
    } catch (error: any) {
      showError('Erro', error.response?.data || 'Não foi possível salvar o cliente');
      console.error('Erro ao salvar cliente:', error);
    } finally {
      setSalvando(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <Loader2 className="w-12 h-12 text-primary animate-spin" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background p-4 sm:p-6">
      <div className="max-w-4xl mx-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-primary/20 rounded-xl flex items-center justify-center">
              <User className="w-6 h-6 text-primary" />
            </div>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-text-primary">
                {clienteId ? 'Editar Cliente' : 'Novo Cliente'}
              </h1>
              <p className="text-sm text-text-muted">
                {clienteId ? 'Edite os dados do cliente' : 'Cadastre um novo cliente'}
              </p>
            </div>
          </div>
          <Button
            onClick={onClose}
            variant="outline"
            className="flex items-center space-x-2"
          >
            <X className="w-4 h-4" />
            <span className="hidden sm:inline">Fechar</span>
          </Button>
        </div>

        <div className="bg-card rounded-2xl p-6 shadow-lg border border-border space-y-6">
          {/* Dados Básicos */}
          <section>
            <h2 className="text-xl font-bold text-text-primary mb-4 flex items-center">
              <User className="w-5 h-5 mr-2 text-primary" />
              Dados Básicos
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Nome *
                </label>
                <input
                  type="text"
                  value={nome}
                  onChange={(e) => setNome(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="Nome completo"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Nome Fantasia
                </label>
                <input
                  type="text"
                  value={fantasia}
                  onChange={(e) => setFantasia(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="Nome fantasia"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  CPF/CNPJ
                </label>
                <input
                  type="text"
                  value={cpfCnpj}
                  onChange={(e) => {
                    const formatted = formatarCpfCnpj(e.target.value);
                    setCpfCnpj(formatted);
                  }}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="000.000.000-00"
                  maxLength={18}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  RG/IE
                </label>
                <input
                  type="text"
                  value={rgIe}
                  onChange={(e) => setRgIe(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Data de Nascimento
                </label>
                <input
                  type="date"
                  value={dataNascimento}
                  onChange={(e) => setDataNascimento(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                />
              </div>
              <div className="flex items-center space-x-4">
                <label className="flex items-center space-x-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={ativo}
                    onChange={(e) => setAtivo(e.target.checked)}
                    className="w-4 h-4 text-primary rounded"
                  />
                  <span className="text-sm text-text-secondary">Ativo</span>
                </label>
                <label className="flex items-center space-x-2 cursor-pointer">
                  <input
                    type="checkbox"
                    checked={bloqueado}
                    onChange={(e) => setBloqueado(e.target.checked)}
                    className="w-4 h-4 text-primary rounded"
                  />
                  <span className="text-sm text-text-secondary">Bloqueado</span>
                </label>
              </div>
            </div>
          </section>

          {/* Contato */}
          <section>
            <h2 className="text-xl font-bold text-text-primary mb-4 flex items-center">
              <Phone className="w-5 h-5 mr-2 text-primary" />
              Contato
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Telefone
                </label>
                <input
                  type="text"
                  value={telefone}
                  onChange={(e) => {
                    const formatted = formatarTelefone(e.target.value);
                    setTelefone(formatted);
                  }}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="(00) 0000-0000"
                  maxLength={15}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Celular
                </label>
                <input
                  type="text"
                  value={celular}
                  onChange={(e) => {
                    const formatted = formatarTelefone(e.target.value);
                    setCelular(formatted);
                  }}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="(00) 00000-0000"
                  maxLength={15}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  E-mail
                </label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="email@exemplo.com"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  E-mail 2
                </label>
                <input
                  type="email"
                  value={email1}
                  onChange={(e) => setEmail1(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="email2@exemplo.com"
                />
              </div>
            </div>
          </section>

          {/* Endereço Principal */}
          <section>
            <h2 className="text-xl font-bold text-text-primary mb-4 flex items-center">
              <MapPin className="w-5 h-5 mr-2 text-primary" />
              Endereço Principal
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Endereço
                </label>
                <input
                  type="text"
                  value={endereco1}
                  onChange={(e) => setEndereco1(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="Rua, Avenida, etc"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Número
                </label>
                <input
                  type="text"
                  value={numero1}
                  onChange={(e) => setNumero1(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Ponto de Referência
                </label>
                <input
                  type="text"
                  value={complemento1}
                  onChange={(e) => setComplemento1(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Bairro
                </label>
                <input
                  type="text"
                  value={bairro1}
                  onChange={(e) => setBairro1(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Cidade
                </label>
                <input
                  type="text"
                  value={cidade1}
                  onChange={(e) => setCidade1(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  UF
                </label>
                <input
                  type="text"
                  value={uf1}
                  onChange={(e) => setUf1(e.target.value.toUpperCase())}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="SP"
                  maxLength={2}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  CEP
                </label>
                <input
                  type="text"
                  value={cep1}
                  onChange={(e) => {
                    const formatted = formatarCep(e.target.value);
                    setCep1(formatted);
                  }}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="00000-000"
                  maxLength={9}
                />
              </div>
            </div>
          </section>

          {/* Outros */}
          <section>
            <h2 className="text-xl font-bold text-text-primary mb-4 flex items-center">
              <FileText className="w-5 h-5 mr-2 text-primary" />
              Outros
            </h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Limite de Crédito
                </label>
                <input
                  type="text"
                  value={limite}
                  onChange={(e) => {
                    const value = e.target.value.replace(/[^\d,]/g, '');
                    setLimite(value);
                  }}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  placeholder="0,00"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Classificação
                </label>
                <input
                  type="text"
                  value={classificacao}
                  onChange={(e) => setClassificacao(e.target.value)}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary"
                  maxLength={1}
                />
              </div>
              <div className="md:col-span-2">
                <label className="block text-sm font-medium text-text-secondary mb-2">
                  Observações
                </label>
                <textarea
                  value={obs}
                  onChange={(e) => setObs(e.target.value)}
                  rows={4}
                  className="w-full px-4 py-3 bg-background-secondary border border-border rounded-xl
                            focus:outline-none focus:ring-2 focus:ring-primary/50
                            text-text-primary resize-none"
                  placeholder="Observações sobre o cliente"
                />
              </div>
            </div>
          </section>

          {/* Botões */}
          <div className="flex justify-end space-x-3 pt-4 border-t border-border">
            <Button
              onClick={onClose}
              variant="outline"
              disabled={salvando}
            >
              Cancelar
            </Button>
            <Button
              onClick={handleSalvar}
              disabled={salvando || !nome.trim()}
              className="flex items-center space-x-2"
            >
              {salvando ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  <span>Salvando...</span>
                </>
              ) : (
                <>
                  <Save className="w-4 h-4" />
                  <span>Salvar</span>
                </>
              )}
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CadastroClientePage;
