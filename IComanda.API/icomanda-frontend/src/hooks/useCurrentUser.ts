/**
 * Hook para obter o usuário logado e verificar permissões por perfil
 */

export interface CurrentUser {
  id: number;
  nome: string;
  role: string;         // 'Admin' | 'Gerente' | 'Caixa' | 'Garcom' | 'Entregador'
  podeVisualizar: boolean;
  podeVerTotal: boolean;
  podeCancelar: boolean;
}

function getUsuarioLogado(): CurrentUser | null {
  try {
    const raw = localStorage.getItem('usuario_logado');
    if (!raw) return null;
    return JSON.parse(raw) as CurrentUser;
  } catch {
    return null;
  }
}

export function useCurrentUser(): CurrentUser | null {
  return getUsuarioLogado();
}

// Helpers de perfil
export function isAdmin(user: CurrentUser | null): boolean {
  return user?.role === 'Admin';
}

export function isGerente(user: CurrentUser | null): boolean {
  return user?.role === 'Admin' || user?.role === 'Gerente';
}

export function isCaixa(user: CurrentUser | null): boolean {
  return user?.role === 'Admin' || user?.role === 'Gerente' || user?.role === 'Caixa';
}

/**
 * Retorna label traduzido para exibição no menu
 */
export function roleLabelPtBr(role: string | undefined): string {
  const map: Record<string, string> = {
    Admin: 'Administrador',
    Gerente: 'Gerente',
    Caixa: 'Caixa',
    Garcom: 'Garçom',
    Entregador: 'Entregador',
  };
  return map[role ?? ''] ?? role ?? '';
}
