import { useState } from 'react'
import { Badge, Button, Col, Form, Row } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { api, consulta } from '../api/client.js'
import Cargando, { ErrorCarga } from '../components/Cargando.jsx'
import Paginacion from '../components/Paginacion.jsx'
import TarjetaReceta from '../components/TarjetaReceta.jsx'
import { useCarga } from '../hooks/useCarga.js'

const COLOR = { PUBLICADA: 'success', INACTIVA: 'secondary', MODERADA: 'danger', BORRADOR: 'warning' }

// S-08 Mis recetas (RC-04)
export default function MisRecetas() {
  const [estado, setEstado] = useState('')
  const [pagina, setPagina] = useState(1)
  const url = `/api/recetas/mias${consulta({ estado, pagina, tamano: 12 })}`
  const { datos, error, cargando } = useCarga(() => api.get(url), url)

  return (
    <>
      <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
        <h1 className="h3 mb-0">Mis recetas</h1>
        <div className="d-flex gap-2">
          <Form.Select aria-label="Estado" value={estado} onChange={(e) => { setEstado(e.target.value); setPagina(1) }}>
            <option value="">Todas</option>
            <option value="PUBLICADA">Publicadas</option>
            <option value="INACTIVA">Inactivas</option>
            <option value="MODERADA">Moderadas</option>
          </Form.Select>
          <Button as={Link} to="/recetas/nueva" className="text-nowrap">Crear receta</Button>
        </div>
      </div>

      {cargando && <Cargando />}
      {error && <ErrorCarga error={error} />}
      {datos && (datos.items.length === 0
        ? <p className="text-muted">No tiene recetas en este estado.</p>
        : (
          <>
            <Row xs={1} sm={2} lg={3} xl={4} className="g-3">
              {datos.items.map((r) => (
                <Col key={r.recetaId}>
                  <TarjetaReceta receta={r} />
                  <div className="d-flex justify-content-between align-items-center mt-1">
                    <Badge bg={COLOR[r.estado] ?? 'secondary'}>{r.estado}</Badge>
                    <Link to={`/recetas/${r.recetaId}/editar`} className="small">Editar</Link>
                  </div>
                </Col>
              ))}
            </Row>
            <Paginacion pagina={datos.pagina} tamano={datos.tamano} total={datos.total} onCambio={setPagina} />
          </>
        ))}
    </>
  )
}
