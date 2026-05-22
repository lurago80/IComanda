import {
  ClipboardList,
  FileText,
  LayoutGrid,
  LogOut,
  Receipt,
  Search,
  ShoppingBag,
  TrendingUp,
  DollarSign,
  BarChart3,
  Table,
  CreditCard,
  History,
  Bell,
  Sparkles,
  User,
  Package,
  Folder,
  Truck,
  MessageCircle,
  Zap,
  Bike,
  Plus,
  Settings,
  Briefcase,
  MapPin,
  Database,
  ChefHat,
  QrCode,
  Pizza,
  Users,
  FileX
} from 'lucide-react'
import React from 'react'
import { Button } from './ui/button'
import NavModulos from './NavModulos'
import { roleLabelPtBr } from '../hooks/useCurrentUser'

interface MenuPrincipalProps {
  onNovaComanda: () => void
  onCaixaRapido?: () => void
  onBuscarEditar: () => void
  onVerComandasAbertas: () => void
  onGerarExtrato: () => void
  onTotalComandasAbertas: () => void
  onPesquisarProdutos: () => void
  onGridComandas: () => void
  onSair: () => void
  onCaixas?: () => void
  onRelatorios?: () => void
  onMesas?: () => void
  onContasReceber?: () => void
  onHistorico?: () => void
  onNotificacoes?: () => void
  onRecebimento?: () => void
  onClientes?: () => void
  onProdutos?: () => void
  onGrupos?: () => void
  onTaxasEntrega?: () => void
  onDeliveryAbertos?: () => void
  onDeliveryNovo?: () => void
  onForcaVendas?: () => void
  onFVRotas?: () => void
  onConectarWhatsApp?: () => void
  onConfiguracoes?: () => void
  onBackup?: () => void
  onIrParaDelivery?: () => void
  onIrParaForcaVendas?: () => void
  onKds?: () => void
  onQrCodeMesas?: () => void
  onGerenciarPizza?: () => void
  onDashboard?: () => void
  onUsuarios?: () => void
  onRelCancelamentos?: () => void
  onFormasPagamento?: () => void
  userName?: string
  userRole?: string
  usarComanda?: boolean
  totalComandasAbertas?: number
  totalValorAberto?: number
  totalDeliveryAbertos?: number
  totalValorDelivery?: number
}

interface MenuItem {
  icon: React.ReactNode
  label: string
  onClick: () => void
  variant?: 'primary' | 'secondary' | 'danger'
  category?: string
}

const MenuPrincipal: React.FC<MenuPrincipalProps> = ({
  onNovaComanda,
  onCaixaRapido,
  onBuscarEditar,
  onVerComandasAbertas,
  onGerarExtrato,
  onTotalComandasAbertas,
  onPesquisarProdutos,
  onGridComandas,
  onSair,
  onCaixas,
  onRelatorios,
  onMesas,
  onContasReceber,
  onHistorico,
  onNotificacoes,
  onRecebimento,
  onClientes,
  onProdutos,
  onGrupos,
  onTaxasEntrega,
  onDeliveryAbertos,
  onDeliveryNovo,
  onForcaVendas,
  onFVRotas,
  onConectarWhatsApp,
  onConfiguracoes,
  onBackup,
  onIrParaDelivery,
  onIrParaForcaVendas,
  onKds,
  onQrCodeMesas,
  onGerenciarPizza,
  onDashboard,
  onUsuarios,
  onRelCancelamentos,
  onFormasPagamento,
  userName,
  userRole,
  totalComandasAbertas = 0,
  totalValorAberto = 0,
  totalDeliveryAbertos = 0,
  totalValorDelivery = 0,
  usarComanda = true
}) => {
  const formatarMoeda = (valor: number) => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL'
    }).format(valor)
  }

  // Seção: CADASTRO
  const itensCadastro: MenuItem[] = [
    {
      icon: <User className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Clientes',
      onClick: onClientes!,
      category: 'Cadastro'
    },
    {
      icon: <Package className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Produtos',
      onClick: onProdutos!,
      category: 'Cadastro'
    },
    {
      icon: <Folder className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Grupos',
      onClick: onGrupos!,
      category: 'Cadastro'
    },
    {
      icon: <Truck className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Taxas de Entrega',
      onClick: onTaxasEntrega!,
      category: 'Cadastro'
    }
  ].filter(item => item.onClick)

  // Seção: COMANDAS
  const itensComandas: MenuItem[] = [
    {
      icon: <ClipboardList className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Nova Comanda',
      onClick: onNovaComanda,
      variant: 'primary',
      category: 'Comandas'
    },
    ...(onCaixaRapido ? [{
      icon: <Zap className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Caixa Rápido (PDV)',
      onClick: onCaixaRapido,
      category: 'Comandas'
    }] : []),
    {
      icon: <Receipt className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Buscar/Editar',
      onClick: onBuscarEditar,
      category: 'Comandas'
    },
    {
      icon: <ShoppingBag className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Comandas Abertas',
      onClick: onVerComandasAbertas,
      category: 'Comandas'
    },
    {
      icon: <LayoutGrid className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Grid de Comandas',
      onClick: onGridComandas,
      category: 'Comandas'
    },
    {
      icon: <FileText className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Gerar Extrato',
      onClick: onGerarExtrato,
      category: 'Comandas'
    }
  ]

  // Seção: FORÇA DE VENDAS
  const itensForcaVendas: MenuItem[] = [
    ...(onForcaVendas ? [{
      icon: <Briefcase className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Pedidos FV',
      onClick: onForcaVendas,
      variant: 'primary' as const,
      category: 'ForcaVendas'
    }] : []),
    ...(onFVRotas ? [{
      icon: <MapPin className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Rota de Visitas',
      onClick: onFVRotas,
      category: 'ForcaVendas'
    }] : []),
  ]

  // Seção: DELIVERY (separado de Comandas) – duas telas: novo pedido e em aberto
  const itensDelivery: MenuItem[] = [
    ...(onDeliveryNovo ? [{
      icon: <Plus className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Novo pedido delivery',
      onClick: onDeliveryNovo,
      category: 'Delivery'
    }] : []),
    ...(onDeliveryAbertos ? [{
      icon: <Bike className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Delivery em aberto',
      onClick: onDeliveryAbertos,
      variant: 'primary' as const,
      category: 'Delivery'
    }] : [])
  ]

  // Seção: FINANCEIRO
  const itensFinanceiro: MenuItem[] = [
    {
      icon: <DollarSign className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Caixas',
      onClick: onCaixas!,
      category: 'Financeiro'
    },
    {
      icon: <CreditCard className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Contas a Receber',
      onClick: onContasReceber!,
      category: 'Financeiro'
    },
    {
      icon: <DollarSign className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Recebimento',
      onClick: onRecebimento!,
      category: 'Financeiro'
    },
    {
      icon: <BarChart3 className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Relatórios',
      onClick: onRelatorios!,
      category: 'Financeiro'
    },
    ...(onDashboard ? [{
      icon: <TrendingUp className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Dashboard',
      onClick: onDashboard,
      variant: 'primary' as const,
      category: 'Financeiro'
    }] : [])
  ].filter(item => item.onClick)

  // Seção: UTILITÁRIOS
  const itensUtilitarios: MenuItem[] = [
    {
      icon: <Table className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Mesas',
      onClick: onMesas!,
      category: 'Utilitários'
    },
    {
      icon: <History className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Histórico',
      onClick: onHistorico!,
      category: 'Utilitários'
    },
    {
      icon: <Bell className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Notificações',
      onClick: onNotificacoes!,
      category: 'Utilitários'
    },
    {
      icon: <MessageCircle className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Conectar WhatsApp',
      onClick: onConectarWhatsApp || (() => {}),
      category: 'Utilitários'
    },
    {
      icon: <Settings className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Configurações',
      onClick: onConfiguracoes || (() => {}),
      category: 'Utilitários'
    },
    ...(onUsuarios ? [{
      icon: <Users className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Usuários',
      onClick: onUsuarios,
      category: 'Utilitários'
    }] : []),
    ...(onRelCancelamentos ? [{
      icon: <FileX className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Cancelamentos',
      onClick: onRelCancelamentos,
      category: 'Utilitários'
    }] : []),
    ...(onFormasPagamento ? [{
      icon: <CreditCard className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Formas Pgto',
      onClick: onFormasPagamento,
      category: 'Utilitários'
    }] : []),
    ...(onBackup ? [{
      icon: <Database className="w-6 h-6 sm:w-7 sm:h-7" />,
      label: 'Backup',
      onClick: onBackup,
      category: 'Utilitários'
    }] : [])
  ].filter(item => item.onClick)

  const renderMenuItem = (item: MenuItem, index: number) => {
    const baseClasses = "h-24 sm:h-28 md:h-32 flex flex-col items-center justify-center space-y-2 rounded-xl sm:rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 hover:scale-105 active:scale-95"
    
    // Cores por categoria
    let categoryColor = "border-primary/50 hover:border-primary"
    if (item.category === 'Cadastro') {
      categoryColor = "border-blue-500/30 hover:border-blue-500/60 bg-blue-50/50 hover:bg-blue-50"
    } else if (item.category === 'Comandas') {
      categoryColor = "border-primary/30 hover:border-primary/60"
    } else if (item.category === 'Delivery') {
      categoryColor = "border-orange-500/30 hover:border-orange-500/60 bg-orange-50/50 hover:bg-orange-50"
    } else if (item.category === 'ForcaVendas') {
      categoryColor = "border-indigo-500/30 hover:border-indigo-500/60 bg-indigo-50/50 hover:bg-indigo-50"
    } else if (item.category === 'Financeiro') {
      categoryColor = "border-green-500/30 hover:border-green-500/60 bg-green-50/50 hover:bg-green-50"
    } else if (item.category === 'Cozinha') {
      categoryColor = "border-orange-600/30 hover:border-orange-600/60 bg-orange-50/50 hover:bg-orange-50"
    } else if (item.category === 'Utilitários') {
      categoryColor = "border-purple-500/30 hover:border-purple-500/60 bg-purple-50/50 hover:bg-purple-50"
    }
    
    if (item.variant === 'primary') {
      const isDelivery = item.category === 'Delivery'
      const isFV = item.category === 'ForcaVendas'
      return (
        <Button
          key={index}
          onClick={item.onClick}
          className={isDelivery
            ? `${baseClasses} bg-gradient-to-br from-orange-500 to-orange-700 hover:from-orange-600 hover:to-orange-800 text-white shadow-lg`
            : isFV
              ? `${baseClasses} bg-gradient-to-br from-indigo-500 to-indigo-700 hover:from-indigo-600 hover:to-indigo-800 text-white shadow-lg`
              : `${baseClasses} bg-gradient-to-br from-primary to-primary-700 hover:from-primary/90 hover:to-primary-600 text-primary-foreground shadow-lg`}
        >
          {item.icon}
          <span className="font-bold text-xs sm:text-sm md:text-base text-center px-2">
            {item.label}
          </span>
        </Button>
      )
    }

    return (
      <Button
        key={index}
        onClick={item.onClick}
        variant="outline"
        className={`${baseClasses} border-2 ${categoryColor} bg-card hover:bg-card-hover`}
      >
        <div className="text-primary">
          {item.icon}
        </div>
        <span className="font-semibold text-xs sm:text-sm md:text-base text-text-primary text-center px-2">
          {item.label}
        </span>
      </Button>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-primary/5">
      {/* Navegação entre módulos */}
      {(onIrParaDelivery || onIrParaForcaVendas) && (
        <NavModulos
          moduloAtivo="comanda"
          onComanda={usarComanda ? undefined : undefined}
          onDelivery={onIrParaDelivery}
          onForcaVendas={onIrParaForcaVendas}
        />
      )}
      <div className="p-4 sm:p-6 lg:p-8">
      <div className="max-w-7xl mx-auto space-y-6 sm:space-y-8">
        {/* Header */}
        <div className="text-center mb-6 sm:mb-8">
          <div className="flex items-center justify-center mb-3">
            <div className="h-32 sm:h-40 md:h-48 rounded-2xl flex items-center justify-center overflow-hidden px-8 py-4">
              <img src="/iComanda.jpg" alt="iComanda Logo" className="h-full w-auto object-contain" />
            </div>
          </div>
          <p className="text-sm sm:text-base text-text-muted">Sistema de Gestão de Comandas</p>
          {/* Badge de usuário logado */}
          {userName && (
            <div className="inline-flex items-center gap-2 mt-3 px-4 py-1.5 rounded-full bg-primary/10 border border-primary/20 text-sm font-medium text-primary">
              <User className="w-4 h-4" />
              <span>{userName}</span>
              {userRole && (
                <span className="px-2 py-0.5 rounded-full bg-primary/20 text-xs font-semibold text-primary">
                  {roleLabelPtBr(userRole)}
                </span>
              )}
            </div>
          )}
        </div>

        {/* Card de Resumo - Responsivo */}
        <div 
          onClick={onTotalComandasAbertas}
          className="bg-gradient-to-br from-primary/20 via-primary/10 to-accent/20 border-2 border-primary/30 rounded-2xl sm:rounded-3xl p-4 sm:p-6 cursor-pointer hover:border-primary/50 transition-all duration-300 hover:scale-[1.01] active:scale-[0.99] shadow-lg hover:shadow-xl"
        >
          <div className="flex flex-col sm:flex-row items-center sm:items-start justify-between gap-4">
            <div className="flex items-center space-x-3 sm:space-x-4">
              <div className="w-14 h-14 sm:w-16 sm:h-16 bg-primary/20 rounded-xl sm:rounded-2xl flex items-center justify-center shadow-md">
                <TrendingUp className="w-7 h-7 sm:w-8 sm:h-8 text-primary" />
              </div>
              <div>
                <p className="text-xs sm:text-sm font-medium text-text-secondary mb-1">
                  Comandas em Aberto
                </p>
                <p className="text-3xl sm:text-4xl md:text-5xl font-bold text-text-primary">
                  {totalComandasAbertas}
                </p>
              </div>
            </div>
            <div className="text-center sm:text-right">
              <p className="text-xs sm:text-sm font-medium text-text-secondary mb-1">
                Valor Total
              </p>
              <p className="text-2xl sm:text-3xl md:text-4xl font-bold text-primary">
                {formatarMoeda(totalValorAberto)}
              </p>
            </div>
          </div>
          <p className="text-xs sm:text-sm text-text-muted text-center mt-4 pt-4 border-t border-primary/20">
            Toque para ver detalhes
          </p>
        </div>

        {/* Seção: COMANDAS */}
        {usarComanda && (
        <div className="space-y-4 bg-primary/5 rounded-2xl p-4 sm:p-6 border border-primary/20">
          <div className="flex items-center space-x-2 mb-4">
            <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/40 to-transparent"></div>
            <h2 className="text-base sm:text-lg font-bold text-primary px-5 py-2 bg-primary/10 rounded-full border-2 border-primary/30 shadow-sm">
              🛒 COMANDAS
            </h2>
            <div className="h-px flex-1 bg-gradient-to-r from-transparent via-primary/40 to-transparent"></div>
          </div>
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3 sm:gap-4">
            {itensComandas.map((item, index) => renderMenuItem(item, index))}
          </div>
        </div>
        )}

        {/* Seção: COZINHA (KDS) + QR Code */}
        {(onKds || onQrCodeMesas || onGerenciarPizza) && (
          <div className="space-y-4 bg-orange-50/30 rounded-2xl p-4 sm:p-6 border border-orange-200/50">
            <div className="flex items-center space-x-2 mb-4">
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-orange-600/40 to-transparent"></div>
              <h2 className="text-base sm:text-lg font-bold text-orange-700 px-5 py-2 bg-orange-100 rounded-full border-2 border-orange-300 shadow-sm">
                🍳 COZINHA & CARDÁPIO
              </h2>
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-orange-600/40 to-transparent"></div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
              {onKds && (
                <Button
                  onClick={onKds}
                  className="h-24 sm:h-28 flex flex-col items-center justify-center space-y-2 rounded-xl sm:rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 hover:scale-105 active:scale-95 bg-gradient-to-br from-orange-600 to-orange-800 hover:from-orange-700 hover:to-orange-900 text-white"
                >
                  <ChefHat className="w-7 h-7 sm:w-8 sm:h-8" />
                  <span className="font-bold text-sm sm:text-base">Tela da Cozinha (KDS)</span>
                </Button>
              )}
              {onQrCodeMesas && (
                <Button
                  onClick={onQrCodeMesas}
                  variant="outline"
                  className="h-24 sm:h-28 flex flex-col items-center justify-center space-y-2 rounded-xl sm:rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 hover:scale-105 active:scale-95 border-2 border-indigo-400/50 hover:border-indigo-500 bg-indigo-50/50 hover:bg-indigo-50"
                >
                  <QrCode className="w-7 h-7 sm:w-8 sm:h-8 text-indigo-600" />
                  <span className="font-bold text-sm sm:text-base text-indigo-700">QR Code das Mesas</span>
                </Button>
              )}
              {onGerenciarPizza && (
                <Button
                  onClick={onGerenciarPizza}
                  variant="outline"
                  className="h-24 sm:h-28 flex flex-col items-center justify-center space-y-2 rounded-xl sm:rounded-2xl shadow-md hover:shadow-xl transition-all duration-300 hover:scale-105 active:scale-95 border-2 border-rose-400/50 hover:border-rose-500 bg-rose-50/50 hover:bg-rose-50"
                >
                  <Pizza className="w-7 h-7 sm:w-8 sm:h-8 text-rose-600" />
                  <span className="font-bold text-sm sm:text-base text-rose-700">Gerenciar Pizzas</span>
                </Button>
              )}
            </div>
          </div>
        )}

        {/* Card de Resumo Delivery */}
        {itensDelivery.length > 0 && (
          <div
            onClick={onDeliveryAbertos}
            className="bg-gradient-to-br from-orange-500/20 via-orange-400/10 to-amber-400/20 border-2 border-orange-400/30 rounded-2xl sm:rounded-3xl p-4 sm:p-6 cursor-pointer hover:border-orange-400/50 transition-all duration-300 hover:scale-[1.01] active:scale-[0.99] shadow-lg hover:shadow-xl"
          >
            <div className="flex flex-col sm:flex-row items-center sm:items-start justify-between gap-4">
              <div className="flex items-center space-x-3 sm:space-x-4">
                <div className="w-14 h-14 sm:w-16 sm:h-16 bg-orange-400/20 rounded-xl sm:rounded-2xl flex items-center justify-center shadow-md">
                  <Bike className="w-7 h-7 sm:w-8 sm:h-8 text-orange-600" />
                </div>
                <div>
                  <p className="text-xs sm:text-sm font-medium text-orange-700 mb-1">
                    Delivery em Aberto
                  </p>
                  <p className="text-3xl sm:text-4xl md:text-5xl font-bold text-text-primary">
                    {totalDeliveryAbertos}
                  </p>
                </div>
              </div>
              <div className="text-center sm:text-right">
                <p className="text-xs sm:text-sm font-medium text-orange-700 mb-1">
                  Valor Total
                </p>
                <p className="text-2xl sm:text-3xl md:text-4xl font-bold text-orange-600">
                  {formatarMoeda(totalValorDelivery)}
                </p>
              </div>
            </div>
            <p className="text-xs sm:text-sm text-orange-600/70 text-center mt-4 pt-4 border-t border-orange-400/20">
              Toque para ver detalhes
            </p>
          </div>
        )}

        {/* Seção: DELIVERY (separado de Comandas) */}
        {itensDelivery.length > 0 && (
          <div className="space-y-4 bg-orange-50/30 rounded-2xl p-4 sm:p-6 border border-orange-200/50">
            <div className="flex items-center space-x-2 mb-4">
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-orange-500/40 to-transparent"></div>
              <h2 className="text-base sm:text-lg font-bold text-orange-700 px-5 py-2 bg-orange-100 rounded-full border-2 border-orange-300 shadow-sm">
                🛵 DELIVERY
              </h2>
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-orange-500/40 to-transparent"></div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
              {itensDelivery.map((item, index) => renderMenuItem(item, index))}
            </div>
          </div>
        )}

        {/* Seção: FORÇA DE VENDAS */}
        {itensForcaVendas.length > 0 && (
          <div className="space-y-4 bg-indigo-50/30 rounded-2xl p-4 sm:p-6 border border-indigo-200/50">
            <div className="flex items-center space-x-2 mb-4">
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-indigo-500/40 to-transparent"></div>
              <h2 className="text-base sm:text-lg font-bold text-indigo-700 px-5 py-2 bg-indigo-100 rounded-full border-2 border-indigo-300 shadow-sm">
                💼 FORÇA DE VENDAS
              </h2>
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-indigo-500/40 to-transparent"></div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
              {itensForcaVendas.map((item, index) => renderMenuItem(item, index))}
            </div>
          </div>
        )}

        {/* Seção: CADASTRO */}
        {itensCadastro.length > 0 && (
          <div className="space-y-4 bg-blue-50/30 rounded-2xl p-4 sm:p-6 border border-blue-200/50">
            <div className="flex items-center space-x-2 mb-4">
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-blue-500/40 to-transparent"></div>
              <h2 className="text-base sm:text-lg font-bold text-blue-700 px-5 py-2 bg-blue-100 rounded-full border-2 border-blue-300 shadow-sm">
                📋 CADASTRO
              </h2>
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-blue-500/40 to-transparent"></div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4">
              {itensCadastro.map((item, index) => renderMenuItem(item, index))}
            </div>
          </div>
        )}

        {/* Seção: FINANCEIRO */}
        {itensFinanceiro.length > 0 && (
          <div className="space-y-4 bg-green-50/30 rounded-2xl p-4 sm:p-6 border border-green-200/50">
            <div className="flex items-center space-x-2 mb-4">
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-green-500/40 to-transparent"></div>
              <h2 className="text-base sm:text-lg font-bold text-green-700 px-5 py-2 bg-green-100 rounded-full border-2 border-green-300 shadow-sm">
                💰 FINANCEIRO
              </h2>
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-green-500/40 to-transparent"></div>
            </div>
            <div className="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-3 sm:gap-4">
              {itensFinanceiro.map((item, index) => renderMenuItem(item, index))}
            </div>
          </div>
        )}

        {/* Seção: UTILITÁRIOS */}
        {itensUtilitarios.length > 0 && (
          <div className="space-y-4 bg-purple-50/30 rounded-2xl p-4 sm:p-6 border border-purple-200/50">
            <div className="flex items-center space-x-2 mb-4">
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-purple-500/40 to-transparent"></div>
              <h2 className="text-base sm:text-lg font-bold text-purple-700 px-5 py-2 bg-purple-100 rounded-full border-2 border-purple-300 shadow-sm">
                ⚙️ UTILITÁRIOS
              </h2>
              <div className="h-px flex-1 bg-gradient-to-r from-transparent via-purple-500/40 to-transparent"></div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4 max-w-4xl mx-auto">
              {itensUtilitarios.map((item, index) => renderMenuItem(item, index))}
            </div>
          </div>
        )}

        {/* Botões Secundários */}
        <div className="space-y-3 sm:space-y-4 pt-4 border-t border-border">
          {/* Conectar WhatsApp - destaque para envio direto */}
          {onConectarWhatsApp && (
            <Button
              onClick={onConectarWhatsApp}
              variant="outline"
              className="w-full h-14 sm:h-16 flex items-center justify-center space-x-3 border-2 border-green-300 hover:border-green-500 bg-green-50 hover:bg-green-100 text-green-700 rounded-xl sm:rounded-2xl shadow-md hover:shadow-lg transition-all duration-300"
            >
              <MessageCircle className="w-5 h-5 sm:w-6 sm:h-6" />
              <span className="font-semibold text-sm sm:text-base">
                Conectar WhatsApp (envio direto)
              </span>
            </Button>
          )}

          {/* Pesquisar Produtos */}
          <Button
            onClick={onPesquisarProdutos}
            variant="outline"
            className="w-full h-14 sm:h-16 flex items-center justify-center space-x-3 border-2 border-border hover:border-primary/50 bg-card hover:bg-card-hover rounded-xl sm:rounded-2xl shadow-md hover:shadow-lg transition-all duration-300"
          >
            <Search className="w-5 h-5 sm:w-6 sm:h-6 text-primary" />
            <span className="font-semibold text-sm sm:text-base text-text-primary">
              Pesquisar Produtos
            </span>
          </Button>

          {/* Sair */}
          <Button
            onClick={onSair}
            variant="outline"
            className="w-full h-14 sm:h-16 flex items-center justify-center space-x-3 border-2 border-red-200 hover:border-red-300 bg-red-50 hover:bg-red-100 text-red-600 rounded-xl sm:rounded-2xl shadow-md hover:shadow-lg transition-all duration-300"
          >
            <LogOut className="w-5 h-5 sm:w-6 sm:h-6" />
            <span className="font-semibold text-sm sm:text-base">Sair do Sistema</span>
          </Button>
        </div>
      </div>
      </div>
    </div>
  )
}

export default MenuPrincipal
