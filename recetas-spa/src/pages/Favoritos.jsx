import { Badge, Button, Card, Col, Row } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { api, urlArchivo } from '../api/client.js'
import { useAvisos } from '../components/Avisos.jsx'
import Cargando, { ErrorCarga } from '../components/Cargando.jsx'
import { useCarga } from '../hooks/useCarga.js'

// S-10 Favoritos (IN-05, IN-04). Las recetas que ya no están disponibles se muestran atenuadas y se pueden quitar.
export default function Favoritos() {
  const avisar = useAvisos()
  const { datos, error, cargando, recargar } = useCarga(() => api.get('/api/favoritos'), 'favoritos')

  const quitar = async (f) => {
    try {
      await api.del(`/api/recetas/${f.recetaId}/favorito`)
      recargar()
    } catch (e) { avisar(e.message, 'danger') }
  }

  const lista = Array.isArray(datos) ? datos : datos?.items

  return (
    <>
      <h1 className="h3 mb-3">Mis favoritos</h1>
      {cargando && <Cargando />}
      {error && <ErrorCarga error={error} />}
      {lista?.length === 0 && <p className="text-muted">Aún no tiene favoritos. Explore el <Link to="/">catálogo</Link>.</p>}
      <Row xs={1} sm={2} lg={3} xl={4} className="g-3">
        {lista?.map((f) => (
          <Col key={f.recetaId}>
            <Card className={`h-100 shadow-sm ${f.disponible ? '' : 'opacity-50'}`}>
              {f.portada
                ? <Card.Img variant="top" src={urlArchivo(f.portada)} alt="" className="portada" loading="lazy" />
                : <div className="portada portada-vacia" aria-hidden="true">🍽️</div>}
              <Card.Body className="d-flex flex-column">
                <Card.Title as="h3" className="h6">
                  {f.disponible ? <Link to={`/recetas/${f.recetaId}`}>{f.titulo}</Link> : f.titulo}
                </Card.Title>
                {f.categoria && <div className="mb-2"><Badge bg="secondary">{f.categoria}</Badge></div>}
                {!f.disponible && <p className="small text-muted mb-2">Ya no está disponible.</p>}
                <Button size="sm" variant="outline-danger" className="mt-auto align-self-start" onClick={() => quitar(f)}>Quitar de favoritos</Button>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </>
  )
}
