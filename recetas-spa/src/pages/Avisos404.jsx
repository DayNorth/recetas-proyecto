import { Link } from 'react-router-dom'

export function NoEncontrado() {
  return (
    <div className="text-center py-5">
      <h1 className="h3">Página no encontrada</h1>
      <p className="text-muted">La dirección no existe o fue movida.</p>
      <Link to="/">Volver al catálogo</Link>
    </div>
  )
}

export function SinPermiso() {
  return (
    <div className="text-center py-5">
      <h1 className="h3">Sin permiso</h1>
      <p className="text-muted">Su cuenta no tiene acceso a esta sección.</p>
      <Link to="/">Volver al catálogo</Link>
    </div>
  )
}
