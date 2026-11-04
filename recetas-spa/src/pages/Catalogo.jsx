import { useState } from 'react'
import { Badge, Button, Col, Form, Row } from 'react-bootstrap'
import { useSearchParams } from 'react-router-dom'
import { api, consulta } from '../api/client.js'
import Cargando, { ErrorCarga } from '../components/Cargando.jsx'
import InputIngrediente from '../components/InputIngrediente.jsx'
import Paginacion from '../components/Paginacion.jsx'
import TarjetaReceta from '../components/TarjetaReceta.jsx'
import { useCarga } from '../hooks/useCarga.js'

// S-01 Catálogo y búsqueda (RC-01, CT-01, CT-02, CT-05). Los filtros viven en la URL: se puede compartir y volver atrás.
export default function Catalogo() {
  const [params, setParams] = useSearchParams()
  const [texto, setTexto] = useState(params.get('q') ?? '')
  const [ingrediente, setIngrediente] = useState('')
  // ingredientes elegidos: "id:nombre" en la URL para poder mostrar el chip sin otra consulta
  const elegidos = params.getAll('ing').map((x) => ({ id: Number(x.split(':')[0]), nombre: x.slice(x.indexOf(':') + 1) }))

  const pagina = Number(params.get('pagina') ?? 1)
  const filtros = {
    q: params.get('q'), categoriaId: params.get('categoriaId'), dificultadId: params.get('dificultadId'),
    tiempoMax: params.get('tiempoMax'), orden: params.get('orden'), ingredientes: elegidos.map((i) => i.id),
    registrar: params.get('q') && pagina === 1 ? true : undefined, // solo la búsqueda con texto (página 1) alimenta los reportes
    pagina, tamano: 12,
  }
  const url = `/api/recetas${consulta(filtros)}`

  const categorias = useCarga(() => api.get('/api/categorias'), 'categorias')
  const dificultades = useCarga(() => api.get('/api/dificultades'), 'dificultades')
  const { datos, error, cargando } = useCarga(() => api.get(url), url)

  const poner = (cambios) => {
    const p = new URLSearchParams(params)
    for (const [k, v] of Object.entries(cambios)) v ? p.set(k, v) : p.delete(k)
    if (!('pagina' in cambios)) p.delete('pagina') // cambiar un filtro vuelve a la página 1
    setParams(p)
  }
  const agregarIngrediente = (i) => {
    if (elegidos.some((e) => e.id === i.ingredienteId)) return
    const p = new URLSearchParams(params)
    p.append('ing', `${i.ingredienteId}:${i.nombre}`)
    p.delete('pagina')
    setParams(p)
    setIngrediente('')
  }
  const quitarIngrediente = (id) => {
    const p = new URLSearchParams(params)
    p.delete('ing')
    elegidos.filter((e) => e.id !== id).forEach((e) => p.append('ing', `${e.id}:${e.nombre}`))
    p.delete('pagina')
    setParams(p)
  }
  const buscar = (e) => { e.preventDefault(); poner({ q: texto.trim() }) }
  const limpiar = () => { setTexto(''); setParams({}) }

  return (
    <>
      <h1 className="h3 mb-3">Recetas</h1>
      <Form onSubmit={buscar} className="mb-4">
        <Row className="g-2">
          <Col md={6} lg={5}>
            <Form.Control type="search" placeholder="Buscar por título o descripción" aria-label="Buscar" value={texto} onChange={(e) => setTexto(e.target.value)} />
          </Col>
          <Col xs={6} md={3} lg={2}>
            <Form.Select aria-label="Categoría" value={params.get('categoriaId') ?? ''} onChange={(e) => poner({ categoriaId: e.target.value })}>
              <option value="">Categoría</option>
              {categorias.datos?.map((c) => <option key={c.categoriaId} value={c.categoriaId}>{c.nombre}</option>)}
            </Form.Select>
          </Col>
          <Col xs={6} md={3} lg={2}>
            <Form.Select aria-label="Dificultad" value={params.get('dificultadId') ?? ''} onChange={(e) => poner({ dificultadId: e.target.value })}>
              <option value="">Dificultad</option>
              {dificultades.datos?.map((d) => <option key={d.dificultadId} value={d.dificultadId}>{d.nombre}</option>)}
            </Form.Select>
          </Col>
          <Col xs={6} md={3} lg={2}>
            <Form.Select aria-label="Tiempo máximo" value={params.get('tiempoMax') ?? ''} onChange={(e) => poner({ tiempoMax: e.target.value })}>
              <option value="">Cualquier tiempo</option>
              {[15, 30, 60, 120].map((m) => <option key={m} value={m}>Hasta {m} min</option>)}
            </Form.Select>
          </Col>
          <Col xs={6} md={3} lg={2}>
            <Form.Select aria-label="Ordenar" value={params.get('orden') ?? ''} onChange={(e) => poner({ orden: e.target.value })}>
              <option value="">Más recientes</option>
              <option value="populares">Más populares</option>
              <option value="tiempo">Menor tiempo</option>
            </Form.Select>
          </Col>
          <Col md={6} lg={4}>
            <InputIngrediente valor={ingrediente} onCambio={setIngrediente} onSeleccion={agregarIngrediente}
              placeholder="Con el ingrediente…" aria-label="Filtrar por ingrediente" />
          </Col>
          <Col xs="auto">
            <Button type="submit">Buscar</Button>{' '}
            <Button variant="outline-secondary" onClick={limpiar}>Limpiar</Button>
          </Col>
        </Row>
        {elegidos.length > 0 && (
          <div className="mt-2">
            {elegidos.map((i) => (
              <Badge key={i.id} bg="primary" className="me-1 fw-normal">
                {i.nombre} <button type="button" className="btn-close btn-close-white ms-1" style={{ fontSize: '0.6em' }} aria-label={`Quitar ${i.nombre}`} onClick={() => quitarIngrediente(i.id)} />
              </Badge>
            ))}
          </div>
        )}
      </Form>

      {cargando && <Cargando />}
      {error && <ErrorCarga error={error} />}
      {datos && (datos.items.length === 0
        ? <p className="text-muted">No se encontraron recetas con esos criterios.</p>
        : (
          <>
            <p className="text-muted small">{datos.total} receta{datos.total === 1 ? '' : 's'}</p>
            <Row xs={1} sm={2} lg={3} xl={4} className="g-3">
              {datos.items.map((r) => <Col key={r.recetaId}><TarjetaReceta receta={r} enlace={datos.busquedaId ? `/recetas/${r.recetaId}?busquedaId=${datos.busquedaId}` : undefined} /></Col>)}
            </Row>
            <Paginacion pagina={datos.pagina} tamano={datos.tamano} total={datos.total} onCambio={(n) => poner({ pagina: n })} />
          </>
        ))}
    </>
  )
}
