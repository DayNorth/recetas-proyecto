import { useState } from 'react'
import { Alert, Badge, Button, Form, Modal, Table } from 'react-bootstrap'
import { api } from '../../api/client.js'
import { useAvisos } from '../../components/Avisos.jsx'
import Cargando, { ErrorCarga } from '../../components/Cargando.jsx'
import Campo, { errorDe } from '../../components/Campo.jsx'
import { useCarga } from '../../hooks/useCarga.js'

// S-17 Categorías (AD-01 a AD-04): crear, editar y activar/desactivar.
export default function AdminCategorias() {
  const avisar = useAvisos()
  const { datos, error, cargando, recargar } = useCarga(() => api.get('/api/admin/categorias'), 'cats')
  const [editando, setEditando] = useState(null) // null = cerrado; {} = nueva; {categoriaId,…} = editar

  const alternar = async (c) => {
    try {
      await api.put(`/api/admin/categorias/${c.categoriaId}/estado`, { activo: !c.activo })
      avisar(c.activo ? 'Categoría desactivada.' : 'Categoría activada.')
      recargar()
    } catch (e) { avisar(e.message, 'danger') }
  }

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h1 className="h3 mb-0">Categorías</h1>
        <Button onClick={() => setEditando({})}>Nueva categoría</Button>
      </div>
      <Alert variant="light" className="border small">Desactivar una categoría no toca sus recetas; solo impide publicar nuevas en ella.</Alert>
      {cargando && !datos && <Cargando />}
      {error && <ErrorCarga error={error} />}
      {datos && (
        <div className="table-responsive">
          <Table hover className="align-middle">
            <thead><tr><th>Nombre</th><th>Descripción</th><th className="text-end">Recetas</th><th>Estado</th><th /></tr></thead>
            <tbody>
              {datos.map((c) => (
                <tr key={c.categoriaId}>
                  <td>{c.nombre}</td>
                  <td className="text-muted">{c.descripcion}</td>
                  <td className="text-end">{c.totalRecetas}</td>
                  <td><Badge bg={c.activo ? 'success' : 'secondary'}>{c.activo ? 'Activa' : 'Inactiva'}</Badge></td>
                  <td className="text-end text-nowrap">
                    <Button size="sm" variant="outline-primary" className="me-1" onClick={() => setEditando(c)}>Editar</Button>
                    <Button size="sm" variant={c.activo ? 'outline-danger' : 'outline-success'} onClick={() => alternar(c)}>{c.activo ? 'Desactivar' : 'Activar'}</Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </div>
      )}
      {editando && <FormularioCategoria categoria={editando} onCerrar={() => setEditando(null)} onGuardada={() => { setEditando(null); recargar() }} />}
    </>
  )
}

function FormularioCategoria({ categoria, onCerrar, onGuardada }) {
  const avisar = useAvisos()
  const nueva = !categoria.categoriaId
  const [nombre, setNombre] = useState(categoria.nombre ?? '')
  const [descripcion, setDescripcion] = useState(categoria.descripcion ?? '')
  const [error, setError] = useState(null)
  const [enviando, setEnviando] = useState(false)

  const enviar = async (e) => {
    e.preventDefault()
    setError(null)
    setEnviando(true)
    try {
      const cuerpo = { nombre: nombre.trim(), descripcion: descripcion.trim() || null }
      if (nueva) await api.post('/api/admin/categorias', cuerpo)
      else await api.put(`/api/admin/categorias/${categoria.categoriaId}`, cuerpo)
      avisar('Categoría guardada.')
      onGuardada()
    } catch (err) { setError(err); setEnviando(false) }
  }

  return (
    <Modal show onHide={onCerrar}>
      <Form onSubmit={enviar} noValidate>
        <Modal.Header closeButton><Modal.Title>{nueva ? 'Nueva categoría' : 'Editar categoría'}</Modal.Title></Modal.Header>
        <Modal.Body>
          {error && !Object.keys(error.errores).length && <Alert variant="danger">{error.message}</Alert>}
          <Campo id="cat-nombre" etiqueta="Nombre" error={errorDe(error, 'nombre')}>
            <Form.Control value={nombre} onChange={(e) => setNombre(e.target.value)} maxLength={60} autoFocus />
          </Campo>
          <Campo id="cat-desc" etiqueta="Descripción (opcional)" error={errorDe(error, 'descripcion')}>
            <Form.Control value={descripcion} onChange={(e) => setDescripcion(e.target.value)} maxLength={200} />
          </Campo>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={onCerrar}>Cancelar</Button>
          <Button type="submit" disabled={enviando}>Guardar</Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}
