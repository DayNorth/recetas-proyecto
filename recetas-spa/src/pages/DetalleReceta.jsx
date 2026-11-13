import { useCallback, useEffect, useRef, useState } from 'react'
import { Alert, Badge, Button, Card, Col, Dropdown, Form, Modal, Row } from 'react-bootstrap'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { api, consulta, urlArchivo } from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import { useAvisos } from '../components/Avisos.jsx'
import Cargando, { ErrorCarga } from '../components/Cargando.jsx'
import Estrellas from '../components/Estrellas.jsx'
import TarjetaReceta from '../components/TarjetaReceta.jsx'
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
  const { usuario, esAdmin } = useAuth()
  const avisar = useAvisos()
  const esAutor = usuario?.usuarioId === receta.autor.usuarioId
  const [ingredientes, setIngredientes] = useState(receta.ingredientes)
  const [porciones, setPorciones] = useState(receta.porcionesBase)
  const [meGusta, setMeGusta] = useState({ activo: receta.meGusta ?? false, total: receta.totalMeGusta })
  const [favorito, setFavorito] = useState(receta.favorito ?? false)
  const [imagenActiva, setImagenActiva] = useState(null)

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

  // Señales de interés que también deja un visitante sin cuenta: alimentan la conversión del tablero.
  const enviadas = useRef(new Set())
  const señal = useCallback((tipo) => {
    if (enviadas.current.has(tipo)) return
    enviadas.current.add(tipo)
    api.post(`/api/recetas/${receta.recetaId}/interacciones`, { tipo }).catch(() => {})
  }, [receta.recetaId])
  useEffect(() => {
    // LECTURA: 30 s acumulados con la pestaña visible (en segundo plano no suma).
    let segundos = 0
    const lectura = setInterval(() => {
      if (document.hidden || ++segundos < 30) return
      señal('LECTURA')
      clearInterval(lectura)
    }, 1000)
    const imprimio = () => señal('IMPRIMIR')
    window.addEventListener('afterprint', imprimio)
    return () => { clearInterval(lectura); window.removeEventListener('afterprint', imprimio) }
  }, [señal])

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

  const multimedia = receta.multimedia ?? []
  const imagenes = multimedia.filter((m) => m.tipo === 'IMAGEN')
  const portada = imagenes.find((m) => m.esPortada) ?? imagenes[0]
  const videos = multimedia.filter((m) => m.tipo === 'VIDEO')
  const documentos = multimedia.filter((m) => m.tipo === 'DOCUMENTO')
  const relacionadas = receta.relacionadas ?? []
  const imagenPrincipal = imagenActiva ?? portada

  return (
    <article className="detalle-receta pb-5">
      <style>{`
        .detalle-receta { --receta-verde:#198754; --receta-suave:#f4faf6; }
        .detalle-receta .hero-img { width:100%; height:390px; object-fit:cover; }
        .detalle-receta .portada-vacia { min-height:390px; display:grid; place-items:center; background:#f1f3f5; font-size:5rem; }
        .detalle-receta .dato { height:100%; padding:.85rem; border:1px solid #e5e7eb; border-radius:.75rem; background:#fff; }
        .detalle-receta .dato-label { display:block; color:#6c757d; font-size:.78rem; margin-bottom:.15rem; }
        .detalle-receta .panel { border:0; border-radius:1rem; box-shadow:0 .25rem 1rem rgba(0,0,0,.07); }
        .detalle-receta .ingrediente { padding:.65rem 0; border-bottom:1px solid #edf0f2; }
        .detalle-receta .paso { display:grid; grid-template-columns:2.25rem 1fr; gap:.75rem; margin-bottom:1rem; }
        .detalle-receta .paso-numero { width:2rem; height:2rem; border-radius:50%; display:grid; place-items:center; color:#fff; background:var(--receta-verde); font-weight:700; }
        .detalle-receta .miniatura { width:120px; height:82px; object-fit:cover; cursor:pointer; border:3px solid transparent; }
        .detalle-receta .miniatura.activa { border-color:var(--receta-verde); }
        .detalle-receta .galeria-item { width:210px; height:135px; object-fit:cover; flex:0 0 auto; }
        .detalle-receta .opinion-titulo { font-size:1.15rem; font-weight:700; margin-bottom:1rem; }
        .detalle-receta .rating-stars { display:flex; align-items:center; gap:.15rem; min-height:3rem; }
        .detalle-receta .star-btn { appearance:none; border:0; background:transparent; color:#cfd4da; cursor:pointer; font-size:2.25rem; line-height:1; padding:.1rem; transition:color .15s ease, transform .15s ease; }
        .detalle-receta .star-btn.active { color:#ffc107; }
        .detalle-receta .star-btn:hover { transform:scale(1.12); }
        .detalle-receta .star-btn:focus-visible { outline:3px solid rgba(25,135,84,.35); outline-offset:2px; border-radius:.25rem; }
        .detalle-receta .star-btn:disabled { cursor:not-allowed; opacity:.65; transform:none; }
        .detalle-receta .opinion-textarea { min-height:145px; resize:vertical; }
        .detalle-receta .comentario-avatar { width:42px; height:42px; border-radius:50%; display:grid; place-items:center; background:#e9f5ee; color:var(--receta-verde); font-weight:700; }
        @media (max-width:991.98px) { .detalle-receta .hero-img,.detalle-receta .portada-vacia { height:300px; min-height:300px; } }
        @media print { .detalle-receta .no-imprimir { display:none!important; } .detalle-receta .panel { box-shadow:none; border:1px solid #ddd; } }
      `}</style>

      {receta.estado !== 'PUBLICADA' &&
        <Alert variant="warning">Esta receta está en estado {receta.estado}: solo usted la puede ver.</Alert>}

      <Row className="g-4 align-items-start">
        <Col lg={5}>
          {imagenPrincipal
            ? <img src={urlArchivo(imagenPrincipal.ruta)} alt={receta.titulo} className="hero-img rounded-4 shadow-sm" />
            : <div className="portada-vacia rounded-4" aria-hidden="true">🍽️</div>}
          {imagenes.length > 1 && (
            <div className="d-flex gap-2 mt-2 overflow-auto pb-1 no-imprimir">
              {imagenes.map((m) => (
                <button key={m.multimediaId} type="button" className="border-0 bg-transparent p-0" onClick={() => setImagenActiva(m)} aria-label="Ver imagen">
                  <img src={urlArchivo(m.ruta)} alt={m.descripcion ?? ''} className={`rounded miniatura ${imagenPrincipal === m ? 'activa' : ''}`} loading="lazy" />
                </button>
              ))}
            </div>
          )}
        </Col>

        <Col lg={7}>
          <div className="d-flex flex-wrap gap-2 mb-2">
            <Badge bg="success">{receta.categoria}</Badge>
            {receta.estado !== 'PUBLICADA' && <Badge bg="warning" text="dark">{receta.estado}</Badge>}
          </div>
          <h1 className="display-6 fw-bold mb-2">{receta.titulo}</h1>
          <div className="mb-2"><Estrellas promedio={receta.calificacionPromedio} total={receta.totalCalificaciones} /></div>
          <p className="text-muted small">Por <strong>{receta.autor.nombre}</strong> · {new Date(receta.fechaCreacion).toLocaleDateString('es-CR')}</p>
          {receta.descripcion && <p className="lead fs-6">{receta.descripcion}</p>}

          <Row className="g-2 my-3">
            <Col xs={6} md={4}>
              <div className="dato">
                <span className="dato-label"><i className="bi bi-clock me-1" aria-hidden="true"></i>Preparación</span>
                <strong>{receta.tiempoPrepMin} min</strong>
              </div>
            </Col>
            {receta.tiempoCoccionMin != null && (
              <Col xs={6} md={4}>
                <div className="dato">
                  <span className="dato-label"><i className="bi bi-fire me-1" aria-hidden="true"></i>Cocción</span>
                  <strong>{receta.tiempoCoccionMin} min</strong>
                </div>
              </Col>
            )}
            <Col xs={6} md={4}>
              <div className="dato">
                <span className="dato-label"><i className="bi bi-people me-1" aria-hidden="true"></i>Porciones</span>
                <strong>{porciones}</strong>
              </div>
            </Col>
            <Col xs={6} md={4}>
              <div className="dato">
                <span className="dato-label"><i className="bi bi-bar-chart me-1" aria-hidden="true"></i>Dificultad</span>
                <strong>{receta.dificultad}</strong>
              </div>
            </Col>
          </Row>

          <div className="d-flex flex-wrap gap-2 no-imprimir">
            <Button variant={meGusta.activo ? 'danger' : 'outline-danger'} onClick={alternarMeGusta} aria-pressed={meGusta.activo}>
              <i className={`bi ${meGusta.activo ? 'bi-heart-fill' : 'bi-heart'} me-1`} aria-hidden="true"></i>
              Me gusta ({meGusta.total})
            </Button>
            <Button variant={favorito ? 'success' : 'outline-success'} onClick={alternarFavorito} aria-pressed={favorito}>
              <i className={`bi ${favorito ? 'bi-star-fill' : 'bi-star'} me-1`} aria-hidden="true"></i>
              {favorito ? 'Guardada' : 'Guardar'}
            </Button>
            <Compartir recetaId={receta.recetaId} />
            {esAutor && <Button as={Link} to={`/recetas/${receta.recetaId}/editar`} variant="outline-secondary">Editar</Button>}
            {esAdmin && <ModerarReceta receta={receta} />}
            <Button variant="outline-secondary" onClick={() => window.print()}>
              <i className="bi bi-printer me-1" aria-hidden="true"></i>Imprimir
            </Button>
          </div>
        </Col>
      </Row>

      <Row className="g-4 mt-3">
        <Col md={5}>
          <Card className="panel h-100"><Card.Body className="p-4">
            <div className="d-flex justify-content-between align-items-center gap-3 mb-3">
              <h2 className="h4 mb-0">Ingredientes</h2>
              <Form.Group className="d-flex align-items-center gap-2 no-imprimir" controlId="porciones">
                <Form.Label className="mb-0 small">Porciones</Form.Label>
                <Form.Control type="number" min={1} max={1000} value={porciones} onChange={(e) => { setPorciones(e.target.value); señal('PORCIONES') }} style={{ width: 82 }} />
              </Form.Group>
            </div>
            <ul className="list-unstyled mb-0">
              {ingredientes.map((i) => (
                <li key={i.ingredienteId} className="ingrediente d-flex gap-2">
                  <Form.Check aria-label={`Marcar ${i.ingrediente}`} className="no-imprimir" />
                  <span><strong>{fmt(i.cantidad)} {i.unidad}</strong> {i.ingrediente}{i.nota && <span className="text-muted"> ({i.nota})</span>}</span>
                </li>
              ))}
            </ul>
          </Card.Body></Card>
        </Col>
        <Col md={7}>
          <Card className="panel h-100"><Card.Body className="p-4">
            <h2 className="h4 mb-4">Preparación</h2>
            {receta.pasos.map((p) => (
              <div className="paso" key={p.numero}>
                <span className="paso-numero">{p.numero}</span>
                <p className="mb-0 pt-1">{p.descripcion}</p>
              </div>
            ))}
          </Card.Body></Card>
        </Col>
      </Row>

      {(imagenes.length > 0 || videos.length > 0 || documentos.length > 0) && (
        <section className="mt-5">
          <h2 className="h4 mb-3">Fotos y videos</h2>
          <div className="d-flex gap-3 overflow-auto pb-3">
            {imagenes.map((m) => <img key={m.multimediaId} src={urlArchivo(m.ruta)} alt={m.descripcion ?? receta.titulo} className="galeria-item rounded-3 shadow-sm" loading="lazy" />)}
          </div>
          {videos.map((v) => (
            <div key={v.multimediaId} className="ratio ratio-16x9 my-3 rounded overflow-hidden" style={{ maxWidth: 720 }}>
              <iframe src={v.ruta} title={v.descripcion ?? 'Video de la receta'} allowFullScreen />
            </div>
          ))}
          {documentos.length > 0 && <p>{documentos.map((d) => <a key={d.multimediaId} href={urlArchivo(d.ruta)} target="_blank" rel="noreferrer" className="me-3" onClick={() => señal('DOCUMENTO')}>📄 {d.descripcion ?? 'Documento'}</a>)}</p>}
        </section>
      )}

      <Comentarios recetaId={receta.recetaId} miCalificacion={receta.miCalificacion} esAutor={esAutor} />

      {relacionadas.length > 0 && (
        <section className="mt-5">
          <div className="d-flex justify-content-between align-items-center mb-3"><h2 className="h4 mb-0">Recetas relacionadas</h2><Link to="/recetas" className="text-success">Ver más recetas →</Link></div>
          <Row className="g-3">{relacionadas.map((r) => <Col sm={6} lg={3} key={r.recetaId}><TarjetaReceta receta={r} /></Col>)}</Row>
        </section>
      )}
    </article>
  )
}

// El admin "elimina" retirando la receta (estado MODERADA, con motivo e historial); se puede restaurar.
function ModerarReceta({ receta }) {
  const avisar = useAvisos()
  const navegar = useNavigate()
  const retirada = receta.estado === 'MODERADA'
  const [abierto, setAbierto] = useState(false)
  const [motivo, setMotivo] = useState('')
  const [enviando, setEnviando] = useState(false)

  const ejecutar = async (e) => {
    e.preventDefault()
    setEnviando(true)
    try {
      await api.post(`/api/moderacion/recetas/${receta.recetaId}/${retirada ? 'restaurar' : 'retirar'}`, { motivo: motivo.trim() })
      avisar(retirada ? 'Receta restaurada.' : 'Receta eliminada.')
      navegar(retirada ? 0 : '/')
    } catch (err) { avisar(err.message, 'danger'); setEnviando(false) }
  }
  return (
    <>
      <Button variant={retirada ? 'outline-success' : 'danger'} onClick={() => setAbierto(true)}>{retirada ? 'Restaurar' : 'Eliminar'}</Button>
      <Modal show={abierto} onHide={() => setAbierto(false)} centered>
        <Form onSubmit={ejecutar}>
          <Modal.Header closeButton><Modal.Title as="h2" className="h5">{retirada ? 'Restaurar receta' : 'Eliminar receta'}</Modal.Title></Modal.Header>
          <Modal.Body>
            <p>{retirada ? 'La receta volverá a verse en el catálogo.' : <>«{receta.titulo}» dejará de verse en el catálogo. Podrá restaurarla después.</>}</p>
            <Form.Group controlId="motivo-moderacion">
              <Form.Label>Motivo</Form.Label>
              <Form.Control as="textarea" rows={3} maxLength={500} autoFocus required value={motivo} onChange={(e) => setMotivo(e.target.value)} />
            </Form.Group>
          </Modal.Body>
          <Modal.Footer>
            <Button variant="outline-secondary" onClick={() => setAbierto(false)}>Cancelar</Button>
            <Button type="submit" variant={retirada ? 'success' : 'danger'} disabled={enviando || !motivo.trim()}>{retirada ? 'Restaurar' : 'Eliminar'}</Button>
          </Modal.Footer>
        </Form>
      </Modal>
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

  const obtenerIconoRed = (nombre) => {
    const clave = nombre?.toLowerCase().trim() || ''
    if (clave.includes('facebook')) return 'bi-facebook text-primary'
    if (clave.includes('whatsapp')) return 'bi-whatsapp text-success'
    if (clave === 'x' || clave.includes('twitter')) return 'bi-twitter-x text-dark'
    if (clave.includes('instagram')) return 'bi-instagram text-danger'
    return 'bi-link-45deg text-secondary'
  }

  return (
    <Dropdown>
      <Dropdown.Toggle variant="outline-primary">
        <i className="bi bi-share me-1" aria-hidden="true"></i>
        Compartir
      </Dropdown.Toggle>
      <Dropdown.Menu className="shadow-sm">
        <Dropdown.Header>Compartir en</Dropdown.Header>
        {redes.datos?.map((r) => (
          <Dropdown.Item
            key={r.redId}
            onClick={() => compartir(r)}
            className="d-flex align-items-center gap-2 py-2"
          >
            <i className={`bi ${obtenerIconoRed(r.nombre)}`} aria-hidden="true"></i>
            <span>{r.nombre}</span>
          </Dropdown.Item>
        ))}
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
    () => api.get(`/api/recetas/${recetaId}/comentarios${consulta({ pagina, tamano: 10 })}`),
    clave
  )
  const [calificacion, setCalificacion] = useState(miCalificacion ?? '')
  const [hoverCalificacion, setHoverCalificacion] = useState(0)
  const [comentario, setComentario] = useState('')
  const [tieneOpinion, setTieneOpinion] = useState(miCalificacion != null)
  const [enviando, setEnviando] = useState(false)
  const limiteComentario = 1000
  const comentarioLimpio = comentario.trim()
  const formularioVacio = calificacion === '' && comentarioLimpio === ''
  const valorVisible = hoverCalificacion || Number(calificacion) || 0
  const etiquetas = ['', 'Muy mala', 'Mala', 'Regular', 'Buena', 'Excelente']

  useEffect(() => {
    setCalificacion(miCalificacion ?? '')
    setHoverCalificacion(0)
    setComentario('')
    setTieneOpinion(miCalificacion != null)
    setPagina(1)
  }, [recetaId, miCalificacion])

  const actualizarListado = () => {
    if (pagina === 1) recargar()
    else setPagina(1)
  }

  const enviar = async (e) => {
    e.preventDefault()
    if (formularioVacio || enviando) return

    const valor = calificacion === '' ? null : Number(calificacion)
    if (valor != null && (!Number.isInteger(valor) || valor < 1 || valor > 5)) {
      avisar('Seleccione una calificación válida entre 1 y 5.', 'warning')
      return
    }

    setEnviando(true)
    try {
      await api.put(`/api/recetas/${recetaId}/calificacion`, {
        calificacion: valor,
        comentario: comentarioLimpio || null
      })
      setComentario('')
      setTieneOpinion(true)
      actualizarListado()
      avisar('Su opinión se guardó correctamente.', 'success')
    } catch (err) {
      avisar(err?.message || 'No fue posible guardar su opinión.', 'danger')
    } finally {
      setEnviando(false)
    }
  }

  const borrar = async () => {
    if (enviando || !window.confirm('¿Desea eliminar su calificación y comentario?')) return
    setEnviando(true)
    try {
      await api.del(`/api/recetas/${recetaId}/calificacion`)
      setCalificacion('')
      setHoverCalificacion(0)
      setComentario('')
      setTieneOpinion(false)
      actualizarListado()
      avisar('Su opinión fue eliminada.', 'success')
    } catch (err) {
      avisar(err?.message || 'No fue posible eliminar su opinión.', 'danger')
    } finally {
      setEnviando(false)
    }
  }

  const seleccionarEstrella = (valor) => {
    if (!enviando) setCalificacion(valor)
  }

  const manejarTecladoEstrella = (e, valor) => {
    if (e.key === 'ArrowRight' || e.key === 'ArrowUp') {
      e.preventDefault()
      setCalificacion(Math.min(5, valor + 1))
    } else if (e.key === 'ArrowLeft' || e.key === 'ArrowDown') {
      e.preventDefault()
      setCalificacion(Math.max(1, valor - 1))
    }
  }

  return (
    <section className="mt-5" aria-labelledby="titulo-comentarios">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2 id="titulo-comentarios" className="h4 mb-0">
          Comentarios {datos && <small className="text-muted">({datos.total})</small>}
        </h2>
      </div>

      {usuario && !esAutor && (
        <Card className="panel mb-3 no-imprimir">
          <Card.Body className="p-4">
            <Form onSubmit={enviar} noValidate>
              <div className="opinion-titulo">Comparta su experiencia</div>

              <Form.Group className="mb-3">
                <Form.Label className="visually-hidden">Calificación</Form.Label>
                <div
                  className="rating-stars"
                  role="radiogroup"
                  aria-label="Calificación de la receta"
                  onMouseLeave={() => setHoverCalificacion(0)}
                >
                  {[1, 2, 3, 4, 5].map((estrella) => (
                    <button
                      key={estrella}
                      type="button"
                      role="radio"
                      aria-checked={Number(calificacion) === estrella}
                      aria-label={`${estrella} ${estrella === 1 ? 'estrella' : 'estrellas'} de 5`}
                      className={`star-btn ${estrella <= valorVisible ? 'active' : ''}`}
                      disabled={enviando}
                      onClick={() => seleccionarEstrella(estrella)}
                      onMouseEnter={() => setHoverCalificacion(estrella)}
                      onFocus={() => setHoverCalificacion(estrella)}
                      onBlur={() => setHoverCalificacion(0)}
                      onKeyDown={(e) => manejarTecladoEstrella(e, estrella)}
                    >
                      <span aria-hidden="true">★</span>
                    </button>
                  ))}
                </div>
                <div className="small text-muted" aria-live="polite">
                  {valorVisible ? etiquetas[valorVisible] : 'Seleccione una calificación'}
                </div>
              </Form.Group>

              <Form.Group>
                <Form.Label htmlFor="comentario">Comentario</Form.Label>
                <Form.Control
                  id="comentario"
                  className="opinion-textarea"
                  as="textarea"
                  rows={5}
                  maxLength={limiteComentario}
                  value={comentario}
                  disabled={enviando}
                  placeholder={'¿Qué le gustó de esta receta?\n¿Qué mejoraría?'}
                  onChange={(e) => setComentario(e.target.value)}
                  aria-describedby="contador-comentario"
                />
                <Form.Text id="contador-comentario" className="d-block text-end">
                  {comentario.length}/{limiteComentario}
                </Form.Text>
              </Form.Group>

              <div className="mt-3 d-flex flex-wrap gap-2">
                <Button variant="success" type="submit" disabled={enviando || formularioVacio}>
                  {enviando ? 'Publicando…' : tieneOpinion ? 'Actualizar opinión' : 'Publicar opinión'}
                </Button>
                {tieneOpinion && (
                  <Button variant="outline-danger" type="button" disabled={enviando} onClick={borrar}>
                    Eliminar opinión
                  </Button>
                )}
              </div>
            </Form>
          </Card.Body>
        </Card>
      )}

      {!usuario && <p className="small text-muted"><Link to="/login">Inicie sesión</Link> para calificar o comentar.</p>}
      {usuario && esAutor && <p className="small text-muted">No puede calificar su propia receta.</p>}
      <div aria-live="polite">{cargando && <Cargando />}</div>
      {error && <ErrorCarga error={error} />}
      {!cargando && !error && datos?.items.length === 0 && <p className="text-muted">Aún no hay comentarios.</p>}

      {!cargando && !error && datos?.items.length > 0 && (
        <Card className="panel">
          <Card.Body className="px-4 py-2">
            {datos.items.map((c) => (
              <div key={c.comentarioId} className="d-flex gap-3 border-bottom py-3">
                <div className="comentario-avatar" aria-hidden="true">{c.usuario?.charAt(0)?.toUpperCase() ?? 'U'}</div>
                <div className="flex-grow-1">
                  <div className="d-flex flex-wrap align-items-center gap-2">
                    <strong>{c.usuario}</strong>
                    {c.calificacion != null && (
                      <span className="text-warning" aria-label={`${c.calificacion} de 5 estrellas`}>
                        {'★'.repeat(c.calificacion)}<span aria-hidden="true">{'☆'.repeat(5 - c.calificacion)}</span>
                      </span>
                    )}
                    <time className="text-muted small" dateTime={c.fecha}>
                      {new Date(c.fecha).toLocaleDateString('es-CR')}
                    </time>
                  </div>
                  {c.comentario && <p className="mb-0 mt-1 text-break">{c.comentario}</p>}
                </div>
              </div>
            ))}
          </Card.Body>
        </Card>
      )}

      {datos && datos.total > datos.tamano && (
        <nav className="mt-3 d-flex gap-2 no-imprimir" aria-label="Paginación de comentarios">
          <Button size="sm" variant="outline-secondary" disabled={pagina <= 1 || cargando} onClick={() => setPagina((p) => p - 1)}>Anterior</Button>
          <Button size="sm" variant="outline-secondary" disabled={pagina * datos.tamano >= datos.total || cargando} onClick={() => setPagina((p) => p + 1)}>Siguiente</Button>
        </nav>
      )}
    </section>
  )
}
