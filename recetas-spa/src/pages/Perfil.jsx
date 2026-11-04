import { useState } from 'react'
import { Alert, Button, Card, Col, Form, Row } from 'react-bootstrap'
import { api, urlArchivo } from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import { useAvisos } from '../components/Avisos.jsx'
import Campo, { errorDe } from '../components/Campo.jsx'

// S-11 Mi perfil (PF-01 a PF-03): datos personales y foto.
export default function Perfil() {
  const { usuario, actualizarUsuario } = useAuth()
  const avisar = useAvisos()
  const [f, setF] = useState({ nombre: usuario.nombre, apellidos: usuario.apellidos, descripcion: usuario.descripcion ?? '' })
  const [error, setError] = useState(null)
  const [guardando, setGuardando] = useState(false)
  const [foto, setFoto] = useState(null)
  const [subiendo, setSubiendo] = useState(false)

  const guardar = async (e) => {
    e.preventDefault()
    setError(null)
    setGuardando(true)
    try {
      actualizarUsuario(await api.put('/api/perfil', { ...f, descripcion: f.descripcion.trim() || null }))
      avisar('Perfil actualizado.')
    } catch (err) { setError(err) } finally { setGuardando(false) }
  }

  const subirFoto = async (e) => {
    e.preventDefault()
    const cuerpo = new FormData()
    cuerpo.append('foto', foto)
    setSubiendo(true)
    try {
      actualizarUsuario(await api.post('/api/perfil/foto', cuerpo))
      setFoto(null)
      e.target.reset()
      avisar('Foto actualizada.')
    } catch (err) { avisar(err.message, 'danger') } finally { setSubiendo(false) }
  }

  const poner = (campo) => (e) => setF({ ...f, [campo]: e.target.value })

  return (
    <>
      <h1 className="h3 mb-3">Mi perfil</h1>
      <Row className="g-4">
        <Col md={4}>
          <Card><Card.Body className="text-center">
            {usuario.fotoPerfil
              ? <img src={urlArchivo(usuario.fotoPerfil)} alt="Foto de perfil" className="rounded-circle foto-perfil mb-3" />
              : <div className="foto-perfil rounded-circle bg-secondary-subtle d-inline-flex align-items-center justify-content-center mb-3" aria-hidden="true">👤</div>}
            <Form onSubmit={subirFoto}>
              <Form.Group controlId="foto">
                <Form.Label className="visually-hidden">Foto de perfil</Form.Label>
                <Form.Control type="file" size="sm" accept="image/jpeg,image/png,image/webp" onChange={(e) => setFoto(e.target.files[0] ?? null)} />
              </Form.Group>
              <Button type="submit" size="sm" className="mt-2" disabled={!foto || subiendo}>{subiendo ? 'Subiendo…' : 'Cambiar foto'}</Button>
            </Form>
          </Card.Body></Card>
        </Col>
        <Col md={8}>
          {error && !Object.keys(error.errores).length && <Alert variant="danger">{error.message}</Alert>}
          <Form onSubmit={guardar} noValidate>
            <Campo id="correo" etiqueta="Correo" ayuda="El correo no se puede cambiar.">
              <Form.Control value={usuario.correo} readOnly plaintext={false} disabled />
            </Campo>
            <Campo id="nombre" etiqueta="Nombre" error={errorDe(error, 'nombre')}>
              <Form.Control value={f.nombre} onChange={poner('nombre')} maxLength={60} />
            </Campo>
            <Campo id="apellidos" etiqueta="Apellidos" error={errorDe(error, 'apellidos')}>
              <Form.Control value={f.apellidos} onChange={poner('apellidos')} maxLength={80} />
            </Campo>
            <Campo id="descripcion" etiqueta="Acerca de mí" error={errorDe(error, 'descripcion')}>
              <Form.Control as="textarea" rows={3} value={f.descripcion} onChange={poner('descripcion')} maxLength={300} />
            </Campo>
            <Button type="submit" disabled={guardando}>{guardando ? 'Guardando…' : 'Guardar cambios'}</Button>
          </Form>
        </Col>
      </Row>
    </>
  )
}
