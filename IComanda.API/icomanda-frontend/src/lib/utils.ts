import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/**
 * Normaliza texto para exibição: remove acentuação e caracteres especiais
 * (evita símbolos de encoding quebrado como e mantém texto legível).
 */
export function normalizarTexto(texto: string | null | undefined): string {
  if (texto == null || typeof texto !== 'string') return ''
  let s = texto
    .replace(/\s+/g, ' ')
    .trim()
  if (!s) return ''
  try {
    s = s.normalize('NFD').replace(/[\u0300-\u036f]/g, '')
  } catch {
    // fallback se normalize não existir
  }
  s = s
    .replace(/\uFFFD/g, '')
    .replace(/[\x00-\x1F\x7F-\x9F]/g, '')
  return s.trim() || texto.trim()
}

