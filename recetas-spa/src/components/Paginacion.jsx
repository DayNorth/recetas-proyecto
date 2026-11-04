import { Pagination } from 'react-bootstrap'

/** Anterior / Siguiente con el número de página; no se muestra si cabe todo en una. */
export default function Paginacion({ pagina, tamano, total, onCambio }) {
  const paginas = Math.max(1, Math.ceil(total / tamano))
  if (paginas <= 1) return null
  return (
    <Pagination className="justify-content-center mt-4">
      <Pagination.Prev disabled={pagina <= 1} onClick={() => onCambio(pagina - 1)}>Anterior</Pagination.Prev>
      <Pagination.Item active>{pagina} de {paginas}</Pagination.Item>
      <Pagination.Next disabled={pagina >= paginas} onClick={() => onCambio(pagina + 1)}>Siguiente</Pagination.Next>
    </Pagination>
  )
}
