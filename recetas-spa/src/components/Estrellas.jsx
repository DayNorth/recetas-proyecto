/** Promedio de calificación como texto: ★ 4.5 (12). */
export default function Estrellas({ promedio, total }) {
  if (promedio == null) return <span className="text-muted small">Sin calificaciones</span>
  return (
    <span className="small" aria-label={`Calificación ${promedio} de 5`}>
      <span className="text-warning">★</span> {Number(promedio).toFixed(1)} <span className="text-muted">({total})</span>
    </span>
  )
}
