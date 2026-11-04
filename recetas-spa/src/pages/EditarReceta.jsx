import { useState } from 'react'
import { Alert, Button, Card, Form } from 'react-bootstrap'
import { Navigate, useNavigate, useParams } from 'react-router-dom'
import { api, urlArchivo } from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import { useAvisos } from '../components/Avisos.jsx'
import Cargando, { ErrorCarga } from '../components/Cargando.jsx'
import FormularioReceta, { formularioASolicitud, recetaAFormulario } from '../components/FormularioReceta.jsx'
import { useCarga } from '../hooks/useCarga.js'

// S-09 Editar receta, multimedia y desactivar (RC-02, RC-06, RC-07, MM-01 a MM-03). Solo el autor.
export default function EditarReceta() {
  const { id } = useParams()
  const { usuario } = useAuth()
  const navegar = useNavigate()
  const avisar = useAvisos()
  const { datos: receta, error, cargando } = useCarga(() => api.get(`/api/recetas/${id}`), id)

  if (cargando) return <Cargando />
  if (error) return <ErrorCarga error={error} />
  if (receta.autor.usuarioId !== usuario.usuarioId) return <Navigate to="/sin-permiso" replace />

  const guardar = async (formulario) => {
    await api.put(`/api/recetas/${id}`, formularioASolicitud(formulario))
    avisar('Cambios guardados.')
    navegar(`/recetas/${id}`)
  }
  const desactivar = async () => {
    if (!window.confirm('¿Desactivar la receta? Dejará de verse en el catálogo.')) return
    try {
      await api.del(`/api/recetas/${id}`)
      avisar('Receta desactivada.')
      navegar('/mis-recetas')
    } catch (e) { avisar(e.message, 'danger') }
  }

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h3 mb-0">Editar receta</h1>
        {receta.estado === 'PUBLICADA' && <Button variant="outline-danger" onClick={desactivar}>Desactivar</Button>}
      </div>
      {receta.estado === 'MODERADA' && <Alert variant="danger">Esta receta fue moderada por un administrador.</Alert>}
      <FormularioReceta inicial={recetaAFormulario(receta)} textoBoton="Guardar cambios" onGuardar={guardar} />
      <hr className="my-4" />
      <Multimedia recetaId={id} />
    </>
  )
}

// MM-01 a MM-03: se recarga solo esta sección, para no perder lo escrito en el formulario.
function Multimedia({ recetaId }) {
  const avisar = useAvisos()
  const { datos, error, cargando, recargar } = useCarga(() => api.get(`/api/recetas/${recetaId}`), `mm-${recetaId}`)
  const [archivo, setArchivo] = useState(null)
  const [video, setVideo] = useState('')
  const [descripcion, setDescripcion] = useState('')
  const [enviando, setEnviando] = useState(false)

  const ejecutar = async (accion, mensaje) => {
    try { await accion(); if (mensaje) avisar(mensaje); recargar() } catch (e) { avisar(e.message, 'danger') }
  }
  const agregar = async (e) => {
    e.preventDefault()
    const cuerpo = new FormData()
    if (archivo) cuerpo.append('archivo', archivo)
    else cuerpo.append('url', video.trim())
    if (descripcion.trim()) cuerpo.append('descripcion', descripcion.trim())
    setEnviando(true)
    await ejecutar(() => api.post(`/api/recetas/${recetaId}/multimedia`, cuerpo), 'Archivo agregado.')
    setEnviando(false)
    setArchivo(null); setVideo(''); setDescripcion('')
    e.target.reset()
  }

  return (
    <section>
      <h2 className="h5">Imágenes, documentos y videos</h2>
      {cargando && !datos && <Cargando />}
      {error && <ErrorCarga error={error} />}
      {datos?.multimedia.length === 0 && <p className="text-muted">Aún no hay archivos (máximo 10).</p>}
      <div className="d-flex flex-wrap gap-3 mb-3">
        {datos?.multimedia.map((m) => (
          <Card key={m.multimediaId} style={{ width: 180 }}>
            {m.tipo === 'IMAGEN' && <Card.Img variant="top" src={urlArchivo(m.ruta)} alt={m.descripcion ?? ''} className="miniatura-card" />}
            <Card.Body className="p-2 small">
              <div className="text-truncate">{m.tipo === 'IMAGEN' ? (m.esPortada ? '⭐ Portada' : 'Imagen') : m.tipo === 'VIDEO' ? '▶ Video' : '📄 Documento'}</div>
              {m.descripcion && <div className="text-muted text-truncate">{m.descripcion}</div>}
              <div className="d-flex gap-1 mt-1">
                {m.tipo === 'IMAGEN' && !m.esPortada && (
                  <Button size="sm" variant="outline-primary"
                    onClick={() => ejecutar(() => api.put(`/api/recetas/${recetaId}/multimedia/${m.multimediaId}/portada`), 'Portada actualizada.')}>Portada</Button>
                )}
                <Button size="sm" variant="outline-danger"
                  onClick={() => window.confirm('¿Borrar este archivo?') && ejecutar(() => api.del(`/api/recetas/${recetaId}/multimedia/${m.multimediaId}`), 'Archivo borrado.')}>Borrar</Button>
              </div>
            </Card.Body>
          </Card>
        ))}
      </div>

      <Form onSubmit={agregar}>
        <h3 className="h6">Agregar</h3>
        <Form.Group className="mb-2" controlId="mm-archivo">
          <Form.Label>Imagen (JPG, PNG, WebP) o PDF</Form.Label>
          <Form.Control type="file" accept="image/jpeg,image/png,image/webp,application/pdf"
            onChange={(e) => { setArchivo(e.target.files[0] ?? null); if (e.target.files[0]) setVideo('') }} />
        </Form.Group>
        <Form.Group className="mb-2" controlId="mm-video">
          <Form.Label>…o enlace de YouTube</Form.Label>
          <Form.Control value={video} disabled={!!archivo} placeholder="https://www.youtube.com/watch?v=…" onChange={(e) => setVideo(e.target.value)} />
        </Form.Group>
        <Form.Group className="mb-2" controlId="mm-desc">
          <Form.Label>Descripción (opcional)</Form.Label>
          <Form.Control value={descripcion} maxLength={150} onChange={(e) => setDescripcion(e.target.value)} />
        </Form.Group>
        <Button type="submit" disabled={enviando || (!archivo && !video.trim())}>Agregar</Button>
      </Form>
    </section>
  )
}
