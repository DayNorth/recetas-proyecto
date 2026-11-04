import { useState } from 'react'
import { Alert, Button, Col, Form, Row } from 'react-bootstrap'
import { api } from '../api/client.js'
import { useCarga } from '../hooks/useCarga.js'
import Campo, { errorDe } from './Campo.jsx'
import InputIngrediente from './InputIngrediente.jsx'

const ingredienteVacio = () => ({ nombre: '', cantidad: '', unidadId: '', nota: '' })

/** Valores iniciales vacíos (crear) o a partir del detalle de una receta (editar). */
export const recetaAFormulario = (r) => ({
  titulo: r.titulo,
  descripcion: r.descripcion ?? '',
  categoriaId: String(r.categoriaId),
  dificultadId: String(r.dificultadId),
  tiempoPrepMin: String(r.tiempoPrepMin),
  porcionesBase: String(r.porcionesBase),
  ingredientes: r.ingredientes.map((i) => ({ nombre: i.ingrediente, cantidad: String(i.cantidad), unidadId: String(i.unidadId), nota: i.nota ?? '' })),
  pasos: r.pasos.map((p) => p.descripcion),
})
export const formularioVacio = () => ({
  titulo: '', descripcion: '', categoriaId: '', dificultadId: '', tiempoPrepMin: '', porcionesBase: '4',
  ingredientes: [ingredienteVacio()], pasos: [''],
})

/** Cuerpo CrearRecetaRequest que espera el API (RC-05 / RC-06). */
export const formularioASolicitud = (f) => ({
  titulo: f.titulo.trim(),
  descripcion: f.descripcion.trim() || null,
  categoriaId: Number(f.categoriaId) || 0,
  dificultadId: Number(f.dificultadId) || 0,
  tiempoPrepMin: Number(f.tiempoPrepMin) || 0,
  porcionesBase: Number(f.porcionesBase) || 0,
  ingredientes: f.ingredientes.map((i) => ({
    nombre: i.nombre.trim(), unidadId: Number(i.unidadId) || 0, cantidad: Number(String(i.cantidad).replace(',', '.')) || 0, nota: i.nota.trim() || null,
  })),
  pasos: f.pasos.map((p) => p.trim()),
})

/**
 * Formulario de receta con ingredientes y pasos dinámicos.
 * onGuardar(formulario, imagen) debe lanzar el ApiError si falla; el formulario lo muestra por campo.
 */
export default function FormularioReceta({ inicial, conImagen, textoBoton, onGuardar }) {
  const [f, setF] = useState(inicial)
  const [imagen, setImagen] = useState(null)
  const [error, setError] = useState(null)
  const [enviando, setEnviando] = useState(false)
  const categorias = useCarga(() => api.get('/api/categorias'), 'categorias')
  const dificultades = useCarga(() => api.get('/api/dificultades'), 'dificultades')
  const unidades = useCarga(() => api.get('/api/unidades'), 'unidades')

  const poner = (campo) => (e) => setF({ ...f, [campo]: e.target.value })
  const cambiarFila = (lista, i, valor) => setF({ ...f, [lista]: f[lista].map((x, k) => (k === i ? valor : x)) })
  const quitarFila = (lista, i) => setF({ ...f, [lista]: f[lista].filter((_, k) => k !== i) })

  const enviar = async (e) => {
    e.preventDefault()
    if (enviando) return // el servidor no es idempotente: un solo envío a la vez
    setError(null)
    setEnviando(true)
    try {
      await onGuardar(f, imagen)
    } catch (err) {
      setError(err)
      setEnviando(false)
      window.scrollTo({ top: 0, behavior: 'smooth' })
    }
  }

  const hayErroresPorCampo = error && Object.keys(error.errores).length > 0

  return (
    <Form onSubmit={enviar} noValidate>
      {error && (
        <Alert variant="danger">
          {hayErroresPorCampo ? 'Revise los campos marcados.' : error.message}
        </Alert>
      )}

      <Campo id="titulo" etiqueta="Título" error={errorDe(error, 'titulo')}>
        <Form.Control value={f.titulo} onChange={poner('titulo')} maxLength={120} />
      </Campo>
      <Campo id="descripcion" etiqueta="Descripción (opcional)" error={errorDe(error, 'descripcion')}>
        <Form.Control as="textarea" rows={3} value={f.descripcion} onChange={poner('descripcion')} maxLength={2000} />
      </Campo>
      <Row>
        <Col sm={6} lg={3}>
          <Campo id="categoriaId" etiqueta="Categoría" error={errorDe(error, 'categoriaId')}>
            <Form.Select value={f.categoriaId} onChange={poner('categoriaId')}>
              <option value="">Elija…</option>
              {categorias.datos?.map((c) => <option key={c.categoriaId} value={c.categoriaId}>{c.nombre}</option>)}
            </Form.Select>
          </Campo>
        </Col>
        <Col sm={6} lg={3}>
          <Campo id="dificultadId" etiqueta="Dificultad" error={errorDe(error, 'dificultadId')}>
            <Form.Select value={f.dificultadId} onChange={poner('dificultadId')}>
              <option value="">Elija…</option>
              {dificultades.datos?.map((d) => <option key={d.dificultadId} value={d.dificultadId}>{d.nombre}</option>)}
            </Form.Select>
          </Campo>
        </Col>
        <Col sm={6} lg={3}>
          <Campo id="tiempoPrepMin" etiqueta="Tiempo (min)" error={errorDe(error, 'tiempoPrepMin')}>
            <Form.Control type="number" min={1} value={f.tiempoPrepMin} onChange={poner('tiempoPrepMin')} />
          </Campo>
        </Col>
        <Col sm={6} lg={3}>
          <Campo id="porcionesBase" etiqueta="Porciones" error={errorDe(error, 'porcionesBase')}>
            <Form.Control type="number" min={1} max={1000} value={f.porcionesBase} onChange={poner('porcionesBase')} />
          </Campo>
        </Col>
      </Row>

      <fieldset className="mb-3">
        <legend className="h6">Ingredientes</legend>
        {errorDe(error, 'ingredientes') && <div className="text-danger small mb-1" role="alert">{errorDe(error, 'ingredientes')}</div>}
        {f.ingredientes.map((ing, i) => {
          const err = (c) => errorDe(error, `ingredientes[${i}].${c}`)
          const mensaje = err('nombre') || err('cantidad') || err('unidadId') || err('nota') || errorDe(error, `ingredientes[${i}]`)
          return (
            <div key={i} className="mb-2">
              <Row className="g-2">
                <Col xs={12} md={4}>
                  <InputIngrediente valor={ing.nombre} onCambio={(v) => cambiarFila('ingredientes', i, { ...ing, nombre: v })}
                    placeholder="Ingrediente" aria-label={`Ingrediente ${i + 1}`} />
                </Col>
                <Col xs={4} md={2}>
                  <Form.Control type="number" step="any" min="0" placeholder="Cant." aria-label={`Cantidad ${i + 1}`} value={ing.cantidad}
                    onChange={(e) => cambiarFila('ingredientes', i, { ...ing, cantidad: e.target.value })} />
                </Col>
                <Col xs={8} md={2}>
                  <Form.Select aria-label={`Unidad ${i + 1}`} value={ing.unidadId} onChange={(e) => cambiarFila('ingredientes', i, { ...ing, unidadId: e.target.value })}>
                    <option value="">Unidad</option>
                    {unidades.datos?.map((u) => <option key={u.unidadId} value={u.unidadId}>{u.abreviatura}</option>)}
                  </Form.Select>
                </Col>
                <Col xs={9} md={3}>
                  <Form.Control placeholder="Nota (opcional)" aria-label={`Nota ${i + 1}`} value={ing.nota} maxLength={100}
                    onChange={(e) => cambiarFila('ingredientes', i, { ...ing, nota: e.target.value })} />
                </Col>
                <Col xs={3} md={1}>
                  <Button variant="outline-danger" aria-label={`Quitar ingrediente ${i + 1}`} disabled={f.ingredientes.length === 1}
                    onClick={() => quitarFila('ingredientes', i)}>✕</Button>
                </Col>
              </Row>
              {mensaje && <div className="text-danger small mt-1" role="alert">{mensaje}</div>}
            </div>
          )
        })}
        <Button variant="outline-primary" size="sm" onClick={() => setF({ ...f, ingredientes: [...f.ingredientes, ingredienteVacio()] })}>
          + Agregar ingrediente
        </Button>
      </fieldset>

      <fieldset className="mb-3">
        <legend className="h6">Pasos</legend>
        {errorDe(error, 'pasos') && <div className="text-danger small mb-1" role="alert">{errorDe(error, 'pasos')}</div>}
        {f.pasos.map((paso, i) => (
          <div key={i} className="mb-2">
            <div className="d-flex gap-2">
              <span className="pt-2 fw-bold">{i + 1}.</span>
              <Form.Control as="textarea" rows={2} aria-label={`Paso ${i + 1}`} value={paso} maxLength={2000}
                onChange={(e) => cambiarFila('pasos', i, e.target.value)} />
              <Button variant="outline-danger" className="align-self-start" aria-label={`Quitar paso ${i + 1}`} disabled={f.pasos.length === 1}
                onClick={() => quitarFila('pasos', i)}>✕</Button>
            </div>
            {errorDe(error, `pasos[${i}]`) && <div className="text-danger small mt-1" role="alert">{errorDe(error, `pasos[${i}]`)}</div>}
          </div>
        ))}
        <Button variant="outline-primary" size="sm" onClick={() => setF({ ...f, pasos: [...f.pasos, ''] })}>+ Agregar paso</Button>
      </fieldset>

      {conImagen && (
        <Campo id="imagen" etiqueta="Imagen de portada (opcional)" ayuda="JPG, PNG o WebP.">
          <Form.Control type="file" accept="image/jpeg,image/png,image/webp" onChange={(e) => setImagen(e.target.files[0] ?? null)} />
        </Campo>
      )}

      <Button type="submit" disabled={enviando}>{enviando ? 'Guardando…' : textoBoton}</Button>
    </Form>
  )
}
