import { Form } from 'react-bootstrap'

/**
 * Mensaje de un campo a partir del ProblemDetails de validación ({ errors: { Nombre: [..] } }).
 * Compara sin distinguir mayúsculas: el API devuelve "Nombre", el formulario usa "nombre".
 */
export function errorDe(error, campo) {
  const errores = error?.errores ?? {}
  const clave = Object.keys(errores).find((k) => k.toLowerCase() === campo.toLowerCase())
  return clave ? errores[clave][0] : undefined
}

/** Etiqueta + control + mensaje de error. `children` es el control (Form.Control, Form.Select…). */
export default function Campo({ id, etiqueta, error, ayuda, children }) {
  return (
    <Form.Group className="mb-3" controlId={id}>
      {etiqueta && <Form.Label>{etiqueta}</Form.Label>}
      {children}
      {ayuda && !error && <Form.Text muted>{ayuda}</Form.Text>}
      {error && <div className="text-danger small mt-1" role="alert">{error}</div>}
    </Form.Group>
  )
}
