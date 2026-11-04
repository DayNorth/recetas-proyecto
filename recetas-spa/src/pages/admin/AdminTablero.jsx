import { useState } from 'react'
import { Alert, Badge, Card, Col, Form, Row } from 'react-bootstrap'
import { api, consulta } from '../../api/client.js'
import Cargando, { ErrorCarga } from '../../components/Cargando.jsx'
import { useCarga } from '../../hooks/useCarga.js'

const COLOR = { alta: 'danger', media: 'warning', info: 'info' }
const ROTULO = { alta: 'Prioridad alta', media: 'Atención', info: 'Para aprovechar' }
const ORDEN = { alta: 0, media: 1, info: 2 }

// S-23 Tablero de decisiones (RP-04): hallazgos con la acción sugerida, no solo números.
export default function AdminTablero() {
  const [mesInicio, setMesInicio] = useState('')
  const [mesFin, setMesFin] = useState('')
  const url = `/api/reportes/tablero${consulta({ mesInicio, mesFin })}`
  const { datos, error, cargando } = useCarga(() => api.get(url), url)

  const hallazgos = datos ? [...datos.hallazgos].sort((a, b) => (ORDEN[a.severidad] ?? 9) - (ORDEN[b.severidad] ?? 9)) : []

  return (
    <>
      <h1 className="h3 mb-3">Tablero de decisiones</h1>
      <Row className="g-2 mb-3">
        <Col xs={6} md={3}>
          <Form.Label htmlFor="t-desde">Desde (mes)</Form.Label>
          <Form.Control id="t-desde" type="month" value={mesInicio} onChange={(e) => setMesInicio(e.target.value)} />
        </Col>
        <Col xs={6} md={3}>
          <Form.Label htmlFor="t-hasta">Hasta (mes)</Form.Label>
          <Form.Control id="t-hasta" type="month" value={mesFin} onChange={(e) => setMesFin(e.target.value)} />
        </Col>
      </Row>

      {cargando && <Cargando />}
      {error && <ErrorCarga error={error} />}
      {datos && (
        <>
          <p className="text-muted small">Periodo analizado: {datos.rango.inicio} a {datos.rango.fin}.</p>
          {hallazgos.length === 0 && <Alert variant="success">Sin hallazgos en este periodo: ningún indicador supera sus umbrales.</Alert>}
          <Row xs={1} lg={2} className="g-3">
            {hallazgos.map((h) => (
              <Col key={h.codigo}>
                <Card className="h-100" border={COLOR[h.severidad]}>
                  <Card.Body>
                    <Badge bg={COLOR[h.severidad]} text={h.severidad === 'media' ? 'dark' : undefined} className="mb-2">{ROTULO[h.severidad] ?? h.severidad}</Badge>
                    <Card.Title as="h2" className="h6">{h.titulo}</Card.Title>
                    <p className="small">{h.detalle}</p>
                    {h.items.length > 0 && (
                      <ul className="small">
                        {h.items.map((i, k) => (
                          <li key={k}><strong>{i.etiqueta}</strong>{i.valor != null && `: ${Number(i.valor).toLocaleString('es-CR', { maximumFractionDigits: 2 })}`}{i.nota && <span className="text-muted"> — {i.nota}</span>}</li>
                        ))}
                      </ul>
                    )}
                    <p className="small mb-0"><strong>Qué hacer:</strong> {h.accion}</p>
                  </Card.Body>
                </Card>
              </Col>
            ))}
          </Row>
        </>
      )}
    </>
  )
}
