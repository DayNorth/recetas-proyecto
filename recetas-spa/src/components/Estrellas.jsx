import { useId } from 'react'

/**
 * Muestra una calificación de 0 a 5 con estrellas y relleno parcial.
 *
 * Props:
 * - promedio: número entre 0 y 5.
 * - total: cantidad de opiniones.
 */
export default function Estrellas({
  promedio,
  total = 0,
  mostrarTexto = true,
  tamano = 'md',
  tamaño,
  className = '',
}) {
  const id = useId().replace(/:/g, '')
  const valorNumerico = Number(promedio)

  if (promedio == null || !Number.isFinite(valorNumerico)) {
    return <span className={`text-muted small ${className}`.trim()}>Sin calificaciones</span>
  }

  const valor = Math.min(5, Math.max(0, valorNumerico))
  const cantidad = Number.isFinite(Number(total)) ? Number(total) : 0
  const tamanos = {
    sm: { estrella: 16, texto: 'small' },
    md: { estrella: 20, texto: 'small' },
    lg: { estrella: 26, texto: '' },
  }
  const medida = tamanos[tamaño ?? tamano] ?? tamanos.md

  return (
    <div
      className={`d-inline-flex align-items-center flex-wrap gap-2 ${className}`.trim()}
      aria-label={`Calificación ${valor.toFixed(1)} de 5, ${cantidad} ${cantidad === 1 ? 'opinión' : 'opiniones'}`}
    >
      <span className="d-inline-flex align-items-center gap-1" aria-hidden="true">
        {[0, 1, 2, 3, 4].map((indice) => (
          <Estrella
            key={indice}
            porcentaje={Math.min(1, Math.max(0, valor - indice))}
            size={medida.estrella}
            gradientId={`${id}-${indice}`}
          />
        ))}
      </span>

      {mostrarTexto && (
        <span className={`d-inline-flex align-items-baseline gap-1 ${medida.texto}`.trim()}>
          <strong className="text-dark">{valor.toFixed(1)}</strong>
          <span className="text-muted">
            ({cantidad} {cantidad === 1 ? 'opinión' : 'opiniones'})
          </span>
        </span>
      )}
    </div>
  )
}

function Estrella({ porcentaje, size, gradientId }) {
  const relleno = Math.round(porcentaje * 100)

  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      role="presentation"
      focusable="false"
      style={{ display: 'block', flex: '0 0 auto' }}
    >
      <defs>
        <linearGradient id={gradientId} x1="0" x2="1" y1="0" y2="0">
          <stop offset={`${relleno}%`} stopColor="#f4b400" />
          <stop offset={`${relleno}%`} stopColor="#dee2e6" />
        </linearGradient>
      </defs>
      <path
        d="M12 2.4l2.86 5.8 6.4.93-4.63 4.51 1.09 6.37L12 17l-5.72 3.01 1.09-6.37-4.63-4.51 6.4-.93L12 2.4z"
        fill={`url(#${gradientId})`}
        stroke="#d39e00"
        strokeWidth="0.65"
        strokeLinejoin="round"
      />
    </svg>
  )
}

