import { useEffect, useRef, useState } from 'react'
import { ArrowLeft, Download, Printer, QrCode, RefreshCw } from 'lucide-react'
import { QRCodeSVG } from 'qrcode.react'
import { mesasService } from '../services/api'

interface Mesa { id?: number; numero: number; status?: string }

interface QrCodeMesasPageProps {
  onClose: () => void
}

function getCardapioUrl(mesa: number): string {
  const { protocol, hostname, port } = window.location
  const portStr = port ? `:${port}` : ''
  return `${protocol}//${hostname}${portStr}/cardapio/${mesa}`
}

export default function QrCodeMesasPage({ onClose }: QrCodeMesasPageProps) {
  const [mesas, setMesas] = useState<Mesa[]>([])
  const [carregando, setCarregando] = useState(true)
  const [mesasPersonalizadas, setMesasPersonalizadas] = useState('')
  const [usarPersonalizadas, setUsarPersonalizadas] = useState(false)
  const printRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    carregarMesas()
  }, [])

  const carregarMesas = async () => {
    setCarregando(true)
    try {
      const data = await mesasService.listar()
      setMesas(data)
    } catch {
      setMesas([])
    } finally {
      setCarregando(false)
    }
  }

  const mesasExibidas: Mesa[] = usarPersonalizadas
    ? mesasPersonalizadas
        .split(/[,\n]/)
        .map(s => parseInt(s.trim(), 10))
        .filter(n => !isNaN(n) && n > 0)
        .map(n => ({ numero: n }))
    : mesas

  const handleImprimir = () => {
    window.print()
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Cabeçalho — não aparece na impressão */}
      <header className="bg-white border-b border-gray-200 px-4 py-3 flex items-center gap-3 print:hidden sticky top-0 z-10 shadow-sm">
        <button
          onClick={onClose}
          className="p-2 rounded-lg hover:bg-gray-100 transition-colors text-gray-500"
        >
          <ArrowLeft className="w-5 h-5" />
        </button>
        <div className="flex items-center gap-2">
          <QrCode className="w-5 h-5 text-indigo-600" />
          <h1 className="font-bold text-gray-900 text-lg">QR Codes das Mesas</h1>
        </div>
        <div className="ml-auto flex items-center gap-2">
          <button
            onClick={carregarMesas}
            disabled={carregando}
            className="p-2 rounded-lg hover:bg-gray-100 text-gray-500"
          >
            <RefreshCw className={`w-4 h-4 ${carregando ? 'animate-spin' : ''}`} />
          </button>
          <button
            onClick={handleImprimir}
            className="flex items-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition-colors"
          >
            <Printer className="w-4 h-4" />
            Imprimir
          </button>
        </div>
      </header>

      {/* Painel de configuração — não aparece na impressão */}
      <div className="print:hidden max-w-2xl mx-auto px-4 py-4 space-y-4">
        {/* Toggle mesas do sistema vs personalizadas */}
        <div className="bg-white rounded-xl border border-gray-200 p-4 space-y-3">
          <p className="text-sm font-semibold text-gray-700">Quais mesas gerar QR Code?</p>
          <div className="flex gap-3">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                checked={!usarPersonalizadas}
                onChange={() => setUsarPersonalizadas(false)}
                className="accent-indigo-600"
              />
              <span className="text-sm text-gray-700">Mesas cadastradas no sistema ({mesas.length})</span>
            </label>
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="radio"
                checked={usarPersonalizadas}
                onChange={() => setUsarPersonalizadas(true)}
                className="accent-indigo-600"
              />
              <span className="text-sm text-gray-700">Personalizar</span>
            </label>
          </div>
          {usarPersonalizadas && (
            <div>
              <label className="text-xs text-gray-500 mb-1 block">Números das mesas (separados por vírgula ou linha)</label>
              <textarea
                value={mesasPersonalizadas}
                onChange={e => setMesasPersonalizadas(e.target.value)}
                placeholder="Ex: 1, 2, 3, 4, 5"
                rows={3}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-400 resize-none"
              />
            </div>
          )}
        </div>

        {/* Preview da URL */}
        {mesasExibidas.length > 0 && (
          <div className="bg-indigo-50 border border-indigo-200 rounded-xl p-3">
            <p className="text-xs text-indigo-600 font-medium mb-1">URL gerada para cada QR Code:</p>
            <p className="text-xs text-indigo-800 font-mono break-all">{getCardapioUrl(mesasExibidas[0].numero)}</p>
          </div>
        )}

        <p className="text-xs text-gray-400">
          {mesasExibidas.length} QR Code{mesasExibidas.length !== 1 ? 's' : ''} serão gerados. Clique em "Imprimir" para imprimir ou salvar como PDF.
        </p>
      </div>

      {/* Área de impressão */}
      <div ref={printRef} className="max-w-4xl mx-auto px-4 py-4">
        {carregando && !usarPersonalizadas ? (
          <div className="flex justify-center py-20 print:hidden">
            <RefreshCw className="w-8 h-8 animate-spin text-indigo-400" />
          </div>
        ) : mesasExibidas.length === 0 ? (
          <div className="text-center py-16 text-gray-400 print:hidden">
            <QrCode className="w-12 h-12 mx-auto mb-3 opacity-30" />
            <p>Nenhuma mesa para exibir.</p>
            {!usarPersonalizadas && <p className="text-sm mt-1">Cadastre mesas no sistema primeiro.</p>}
          </div>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4 print:grid-cols-4 print:gap-6">
            {mesasExibidas.map(mesa => {
              const url = getCardapioUrl(mesa.numero)
              return (
                <div
                  key={mesa.numero}
                  className="bg-white border border-gray-200 rounded-2xl p-4 flex flex-col items-center gap-3 shadow-sm print:rounded-none print:border print:shadow-none print:break-inside-avoid"
                >
                  {/* QR Code */}
                  <QRCodeSVG
                    value={url}
                    size={140}
                    level="M"
                    includeMargin={false}
                    className="print:w-[120px] print:h-[120px]"
                  />

                  {/* Identificação */}
                  <div className="text-center">
                    <p className="font-bold text-gray-900 text-lg leading-tight">Mesa {mesa.numero}</p>
                    <p className="text-xs text-gray-400 mt-0.5">Aponte a câmera para ver o cardápio</p>
                  </div>

                  {/* URL curta (print helper) */}
                  <p className="text-[9px] text-gray-300 break-all text-center leading-tight hidden print:block">
                    {url}
                  </p>

                  {/* Botão download individual (só no browser) */}
                  <button
                    onClick={() => downloadQrCode(mesa.numero, url)}
                    className="print:hidden flex items-center gap-1 text-xs text-indigo-500 hover:text-indigo-700 transition-colors"
                  >
                    <Download className="w-3 h-3" />
                    Baixar
                  </button>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* Estilo de impressão inline */}
      <style>{`
        @media print {
          body * { visibility: hidden; }
          #qr-print-area, #qr-print-area * { visibility: visible; }
          #qr-print-area { position: absolute; left: 0; top: 0; width: 100%; }
          .print\\:hidden { display: none !important; }
        }
      `}</style>
    </div>
  )
}

function downloadQrCode(mesa: number, url: string) {
  // Cria um canvas temporário, renderiza o QR e faz download como PNG
  const canvas = document.createElement('canvas')
  const size = 300
  canvas.width = size
  canvas.height = size + 40
  const ctx = canvas.getContext('2d')
  if (!ctx) return

  // Fundo branco
  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, 0, canvas.width, canvas.height)

  // Texto abaixo
  ctx.fillStyle = '#1f2937'
  ctx.font = 'bold 20px sans-serif'
  ctx.textAlign = 'center'
  ctx.fillText(`Mesa ${mesa}`, size / 2, size + 28)

  // QR via SVG → Image
  const svgEl = document.querySelector(`[data-mesa="${mesa}"] svg`) as SVGElement
  if (svgEl) {
    const svgData = new XMLSerializer().serializeToString(svgEl)
    const img = new Image()
    img.onload = () => {
      ctx.drawImage(img, 0, 0, size, size)
      const link = document.createElement('a')
      link.download = `qrcode-mesa-${mesa}.png`
      link.href = canvas.toDataURL('image/png')
      link.click()
    }
    img.src = `data:image/svg+xml;base64,${btoa(svgData)}`
  } else {
    // Fallback: abre a URL diretamente
    window.open(url, '_blank')
  }
}
