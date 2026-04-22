/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly DEV: boolean;
  readonly PROD: boolean;
  readonly MODE: string;
  readonly VITE_MONITORING_URL?: string;
  readonly VITE_API_URL?: string;
  // Adicione outras variáveis de ambiente aqui conforme necessário
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
