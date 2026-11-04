import { Alert, Spinner } from 'react-bootstrap'

export default function Cargando() {
  return (
    <div className="text-center py-5" role="status">
      <Spinner animation="border" />
      <span className="visually-hidden">Cargando…</span>
    </div>
  )
}

/** Error de una carga: muestra el mensaje del API. */
export function ErrorCarga({ error }) {
  return <Alert variant="danger">{error?.message ?? 'Ocurrió un error.'}</Alert>
}
