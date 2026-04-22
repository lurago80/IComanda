const escapeCell = (val: unknown): string => {
  if (val == null) return '';
  const s = String(val);
  if (s.includes(';') || s.includes('"') || s.includes('\n')) return `"${s.replace(/"/g, '""')}"`;
  return s;
};

/**
 * Exporta uma matriz (linhas de células) para CSV. Útil para relatórios com várias seções e títulos.
 */
export function exportToExcelMultiSection(rows: (string | number)[][], filename: string): void {
  if (rows.length === 0) return;
  const lines = rows.map((row) => row.map((cell) => escapeCell(cell)).join(';'));
  const csv = lines.join('\r\n');
  const BOM = '\uFEFF';
  const blob = new Blob([BOM + csv], { type: 'text/csv;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename.endsWith('.csv') ? filename : `${filename}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

/**
 * Exporta dados para CSV compatível com Excel (UTF-8, separador ;).
 * O Excel abre o arquivo .csv corretamente em pt-BR.
 */
export function exportToExcel<T extends Record<string, unknown>>(
  rows: T[],
  filename: string,
  columns?: { key: keyof T; label: string }[]
): void {
  if (rows.length === 0) return;

  const keys = columns ? columns.map((c) => c.key) : (Object.keys(rows[0]) as (keyof T)[]);
  const headers = columns ? columns.map((c) => c.label) : (keys as string[]);

  const headerLine = headers.map((h) => escapeCell(h)).join(';');
  const dataLines = rows.map((row) => keys.map((k) => escapeCell(row[k])).join(';'));
  const csv = [headerLine, ...dataLines].join('\r\n');
  const BOM = '\uFEFF';
  const blob = new Blob([BOM + csv], { type: 'text/csv;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = filename.endsWith('.csv') ? filename : `${filename}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}
