import { useEffect, useState } from 'react'
import { Alert, Badge, Button, Card, Col, Dropdown, Form, Row } from 'react-bootstrap'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { api, consulta, urlArchivo } from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import { useAvisos } from '../components/Avisos.jsx'
import Cargando, { ErrorCarga } from '../components/Cargando.jsx'
import Estrellas from '../components/Estrellas.jsx'
import { useCarga } from '../hooks/useCarga.js'

const fmt = (n) => Number(n).toLocaleString('es-CR', { maximumFractionDigits: 2 })

// S-02 Detalle de receta (RC-02, RC-03, IN-01 a IN-04, IN-06 a IN-09, CT-04)
export default function DetalleReceta() {
  const { id } = useParams()
  const [params] = useSearchParams()
  const busquedaId = params.get('busquedaId')
  const url = `/api/recetas/${id}${consulta({ busquedaId })}`
  const { datos: receta, error, cargando } = useCarga(() => api.get(url), url)

  if (cargando) return <Cargando />
  if (error) return <ErrorCarga error={error} />
  return <Contenido key={receta.recetaId} receta={receta} />
}

function Contenido({ receta }) {
  const { usuario } = useAuth()
  const avisar = useAvisos()
  const esAutor = usuario?.usuarioId === receta.autor.usuarioId
  const [ingredientes, setIngredientes] = useState(receta.ingredientes)
  const [porciones, setPorciones] = useState(receta.porcionesBase)
  const [meGusta, setMeGusta] = useState({ activo: receta.meGusta ?? false, total: receta.totalMeGusta })
  const [favorito, setFavorito] = useState(receta.favorito ?? false)

  // Ingredientes para otras porciones: el servidor escala solo las unidades "escalables".
  useEffect(() => {
    const n = Number(porciones)
    if (!Number.isInteger(n) || n < 1 || n > 1000) return
    if (n === receta.porcionesBase) { setIngredientes(receta.ingredientes); return }
    let vigente = true
    const espera = setTimeout(() => {
      api.get(`/api/recetas/${receta.recetaId}/ingredientes${consulta({ porciones: n })}`)
        .then((r) => vigente && setIngredientes(r.ingredientes))
        .catch((e) => avisar(e.message, 'danger'))
    }, 300)
    return () => { vigente = false; clearTimeout(espera) }
  }, [porciones, receta, avisar])

  const conSesion = (accion) => async () => {
    if (!usuario) return avisar('Inicie sesión para usar esta opción.', 'warning')
    try { await accion() } catch (e) { avisar(e.message, 'danger') }
  }
  const alternarMeGusta = conSesion(async () =>
    setMeGusta(await (meGusta.activo ? api.del : api.put)(`/api/recetas/${receta.recetaId}/me-gusta`)))
  const alternarFavorito = conSesion(async () => {
    await (favorito ? api.del : api.put)(`/api/recetas/${receta.recetaId}/favorito`)
    setFavorito(!favorito)
  })

  const imagenes = receta.multimedia.filter((m) => m.tipo === 'IMAGEN')
  const portada = imagenes.find((m) => m.esPortada) ?? imagenes[0]
  const videos = receta.multimedia.filter((m) => m.tipo === 'VIDEO')
  const documentos = receta.multimedia.filter((m) => m.tipo === 'DOCUMENTO')

  return (
    <>
      {receta.estado !== 'PUBLICADA' && <Alert variant="warning">Esta receta está en estado {receta.estado}: solo usted la puede ver.</Alert>}
      <Row className="g-4">
        <Col lg={5}>
          {portada
            ? <img src={urlArchivo(portada.ruta)} alt={receta.titulo} className="img-fluid rounded w-100" />
            : <div className="portada portada-vacia rounded" aria-hidden="true">🍽️</div>}
          {imagenes.length > 1 && (
            <div className="d-flex gap-2 mt-2 flex-wrap">
              {imagenes.filter((m) => m !== portada).map((m) => (
                <img key={m.multimediaId} src={urlArchivo(m.ruta)} alt="" className="rounded miniatura" loading="lazy" />
              ))}
            </div>
          )}
        </Col>
        <Col lg={7}>
          <h1 className="h3">{receta.titulo}</h1>
          <p className="text-muted mb-2">Por {receta.autor.nombre} · {new Date(receta.fechaCreacion).toLocaleDateString('es-CR')}</p>
          <div className="mb-3">
            <Badge bg="secondary" className="me-1">{receta.categoria}</Badge>
            <Badge bg="light" text="dark" className="border me-1">{receta.dificultad}</Badge>
            <Badge bg="light" text="dark" className="border">⏱ {receta.tiempoPrepMin} min</Badge>
          </div>
          {receta.descripcion && <p>{receta.descripcion}</p>}
          <div className="mb-3"><Estrellas promedio={receta.calificacionPromedio} total={receta.totalCalificaciones} /></div>

          <div className="d-flex flex-wrap gap-2">
            <Button variant={meGusta.activo ? 'danger' : 'outline-danger'} onClick={alternarMeGusta} aria-pressed={meGusta.activo}>
              ♥ Me gusta ({meGusta.total})
            </Button>
            <Button variant={favorito ? 'warning' : 'outline-warning'} onClick={alternarFavorito} aria-pressed={favorito}>
              {favorito ? '★ En favoritos' : '☆ Favorito'}
            </Button>
            <Compartir recetaId={receta.recetaId} />
            {esAutor && <Button as={Link} to={`/recetas/${receta.recetaId}/editar`} variant="outline-secondary">Editar</Button>}
          </div>
        </Col>
      </Row>

      <Row className="g-4 mt-1">
        <Col md={5}>
          <h2 className="h5">Ingredientes</h2>
          <Form.Group className="mb-2 d-flex align-items-center gap-2" controlId="porciones">
            <Form.Label className="mb-0">Porciones</Form.Label>
            <Form.Control type="number" min={1} max={1000} value={porciones} onChange={(e) => setPorciones(e.target.value)} style={{ width: 90 }} />
          </Form.Group>
          <ul className="list-unstyled">
            {ingredientes.map((i) => (
              <li key={i.ingredienteId} className="border-bottom py-1">
                <strong>{fmt(i.cantidad)} {i.unidad}</strong> {i.ingrediente}
                {i.nota && <span className="text-muted"> ({i.nota})</span>}
              </li>
            ))}
          </ul>
        </Col>
        <Col md={7}>
          <h2 className="h5">Preparación</h2>
          <ol>{receta.pasos.map((p) => <li key={p.numero} className="mb-2">{p.descripcion}</li>)}</ol>
        </Col>
      </Row>

      {videos.map((v) => (
        <div key={v.multimediaId} className="ratio ratio-16x9 my-3" style={{ maxWidth: 640 }}>
          <iframe src={v.ruta} title={v.descripcion ?? 'Video de la receta'} allowFullScreen />
        </div>
      ))}
      {documentos.length > 0 && (
        <p>{documentos.map((d) => <a key={d.multimediaId} href={urlArchivo(d.ruta)} target="_blank" rel="noreferrer" className="me-3">📄 {d.descripcion ?? 'Documento'}</a>)}</p>
      )}

      <Comentarios recetaId={receta.recetaId} miCalificacion={receta.miCalificacion} esAutor={esAutor} />
    </>
  )
}

// IN-09 Compartir: registra el compartido y abre la red (Instagram no admite enlace: se copia la URL).
function Compartir({ recetaId }) {
  const redes = useCarga(() => api.get('/api/redes-sociales'), 'redes')
  const avisar = useAvisos()

  const compartir = async (red) => {
    try {
      const r = await api.post(`/api/recetas/${recetaId}/compartir`, { redId: red.redId })
      if (r.enlace) window.open(r.enlace, '_blank', 'noopener')
      else { await navigator.clipboard?.writeText(r.url); avisar('Enlace copiado: péguelo en la red social.') }
    } catch (e) { avisar(e.message, 'danger') }
  }

  return (
    <Dropdown>
      <Dropdown.Toggle variant="outline-primary">Compartir</Dropdown.Toggle>
      <Dropdown.Menu>
        {redes.datos?.map((r) => <Dropdown.Item key={r.redId} onClick={() => compartir(r)}>{r.nombre}</Dropdown.Item>)}
      </Dropdown.Menu>
    </Dropdown>
  )
}

// IN-06 a IN-08: comentarios visibles, promedio y mi calificación.
function Comentarios({ recetaId, miCalificacion, esAutor }) {
  const { usuario } = useAuth()
  const avisar = useAvisos()
  const [pagina, setPagina] = useState(1)
  const clave = `${recetaId}-${pagina}`
  const { datos, error, cargando, recargar } = useCarga(
    () => api.get(`/api/recetas/${recetaId}/comentarios${consulta({ pagina, tamano: 10 })}`), clave)
  const [calificacion, setCalificacion] = useState(miCalificacion ?? '')
  const [comentario, setComentario] = useState('')
  const [enviando, setEnviando] = useState(false)

  const enviar = async (e) => {
    e.preventDefault()
    setEnviando(true)
    try {
      await api.put(`/api/recetas/${recetaId}/calificacion`, {
        calificacion: calificacion === '' ? null : Number(calificacion),
        comentario: comentario.trim() || null,
      })
      setComentario('')
      setPagina(1)
      recargar()
      avisar('Gracias por su opinión.')
    } catch (err) { avisar(err.message, 'danger') } finally { setEnviando(false) }
  }
  const borrar = async () => {
    try {
      await api.del(`/api/recetas/${recetaId}/calificacion`)
      setCalificacion('')
      recargar()
    } catch (err) { avisar(err.message, 'danger') }
  }

  return (
    <section className="mt-4">
      <h2 className="h5">Comentarios {datos && <small className="text-muted">({datos.total})</small>}</h2>
      {usuario && !esAutor && (
        <Card className="mb-3"><Card.Body>
          <Form onSubmit={enviar}>
            <Row className="g-2 align-items-end">
              <Col xs={12} sm={3}>
                <Form.Label htmlFor="calificacion">Calificación</Form.Label>
                <Form.Select id="calificacion" value={calificacion} onChange={(e) => setCalificacion(e.target.value)}>
                  <option value="">Sin calificar</option>
                  {[5, 4, 3, 2, 1].map((n) => <option key={n} value={n}>{'★'.repeat(n)} ({n})</option>)}
                </Form.Select>
              </Col>
              <Col xs={12} sm={9}>
                <Form.Label htmlFor="comentario">Comentario</Form.Label>
                <Form.Control id="comentario" as="textarea" rows={2} maxLength={1000} value={comentario} onChange={(e) => setComentario(e.target.value)} />
              </Col>
            </Row>
            <div className="mt-2">
              <Button type="submit" disabled={enviando || (calificacion === '' && !comentario.trim())}>Enviar</Button>{' '}
              {miCalificacion != null && <Button variant="outline-secondary" onClick={borrar}>Borrar mi calificación</Button>}
            </div>
          </Form>
        </Card.Body></Card>
      )}
      {!usuario && <p className="small text-muted"><Link to="/login">Inicie sesión</Link> para calificar o comentar.</p>}

      {cargando && <Cargando />}
      {error && <ErrorCarga error={error} />}
      {datos?.items.length === 0 && <p className="text-muted">Aún no hay comentarios.</p>}
      {datos?.items.map((c) => (
        <div key={c.comentarioId} className="border-bottom py-2">
          <strong>{c.usuario}</strong> {c.calificacion != null && <span className="text-warning">{'★'.repeat(c.calificacion)}</span>}
          <span className="text-muted small ms-2">{new Date(c.fecha).toLocaleDateString('es-CR')}</span>
          {c.comentario && <p className="mb-0">{c.comentario}</p>}
        </div>
      ))}
      {datos && datos.total > datos.tamano && (
        <div className="mt-2 d-flex gap-2">
          <Button size="sm" variant="outline-secondary" disabled={pagina <= 1} onClick={() => setPagina(pagina - 1)}>Anterior</Button>
          <Button size="sm" variant="outline-secondary" disabled={pagina * datos.tamano >= datos.total} onClick={() => setPagina(pagina + 1)}>Siguiente</Button>
        </div>
      )}
    </section>
  )
}
