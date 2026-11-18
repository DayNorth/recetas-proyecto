import { useState } from 'react'
import { Badge, Button, Col, Container, Form, InputGroup, Row } from 'react-bootstrap'
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
  const elegidos = params.getAll('ing').map((x) => ({
    id: Number(x.split(':')[0]),
    nombre: x.slice(x.indexOf(':') + 1),
  }))

  const pagina = Number(params.get('pagina') ?? 1)

  const filtros = {
    q: params.get('q'),
    categoriaId: params.get('categoriaId'),
    dificultadId: params.get('dificultadId'),
    tiempoMax: params.get('tiempoMax'),
    orden: params.get('orden'),
    ingredientes: elegidos.map((i) => i.id),

    registrar:
      params.get('q') && pagina === 1
        ? true
        : undefined,

    pagina,
    tamano: 12,
  }

  const url = `/api/recetas${consulta(filtros)}`

  const categorias = useCarga(
    () => api.get('/api/categorias'),
    'categorias'
  )

  const dificultades = useCarga(
    () => api.get('/api/dificultades'),
    'dificultades'
  )

  const { datos, error, cargando } = useCarga(
    () => api.get(url),
    url
  )

  // Manejo de filtros
  const poner = (cambios) => {
    const p = new URLSearchParams(params)

    for (const [k, v] of Object.entries(cambios)) {
      if (v) {
        p.set(k, v)
      } else {
        p.delete(k)
      }
    }

    // Cambiar un filtro vuelve a la página 1
    if (!('pagina' in cambios)) {
      p.delete('pagina')
    }

    setParams(p)
  }

  const agregarIngrediente = (i) => {
    if (
      elegidos.some(
        (e) => e.id === i.ingredienteId
      )
    ) {
      return
    }

    const p = new URLSearchParams(params)

    p.append(
      'ing',
      `${i.ingredienteId}:${i.nombre}`
    )

    p.delete('pagina')

    setParams(p)
    setIngrediente('')
  }

  const quitarIngrediente = (id) => {
    const p = new URLSearchParams(params)

    p.delete('ing')

    elegidos
      .filter((e) => e.id !== id)
      .forEach((e) => {
        p.append(
          'ing',
          `${e.id}:${e.nombre}`
        )
      })

    p.delete('pagina')

    setParams(p)
  }

  const buscar = (e) => {
    e.preventDefault()

    poner({
      q: texto.trim(),
    })
  }

  const limpiar = () => {
    setTexto('')
    setIngrediente('')
    setParams({})
  }

  return (
    <>
      {/* ==================================================
          HERO
      ================================================== */}

      <div className="position-relative overflow-hidden bg-dark text-white py-4">
        <img
          src="/images/hero-recetas.jpg"
          alt="Recetas"
          className="position-absolute top-0 start-0 w-100 h-100 object-fit-cover opacity-50 z-0"
        />
        <Container className="position-relative z-2 py-lg-5">

          <Row>
            <Col lg={11} xl={10}>

              {/* Título */}

              <h1 className="display-5 fw-bold lh-1 mb-3">
                Descubre, cocina y comparte
                <span className="d-block text-success">
                  recetas increíbles
                </span>
              </h1>

              <p className="lead text-white-50 mb-4">
                Explora cientos de recetas, encuentra
                nuevas ideas y lleva la cocina a otro nivel.
              </p>

            </Col>
          </Row>

          {/* ==============================================
              BUSCADOR PRINCIPAL
          ============================================== */}

          <Form
            onSubmit={buscar}
            className="mb-3"
          >
            <Row>
              <Col lg={8} xl={9}>

                <InputGroup
                  size="lg"
                  className="shadow"
                >

                  <InputGroup.Text className="bg-white border-0 ps-4 rounded-start-pill">
                    <i className="bi bi-search text-muted" aria-hidden="true"></i>
                  </InputGroup.Text>

                  <Form.Control
                    type="search"
                    placeholder="¿Qué te gustaría cocinar hoy?"
                    aria-label="Buscar recetas"
                    value={texto}
                    onChange={(e) =>
                      setTexto(e.target.value)
                    }
                    className="border-0 shadow-none"
                  />

                  <Button
                    type="submit"
                    variant="success"
                    className="px-4 px-md-5 rounded-end-pill"
                  >
                    Buscar
                  </Button>

                </InputGroup>

              </Col>
            </Row>
          </Form>

          {/* ==============================================
              FILTROS PRINCIPALES
          ============================================== */}

          <Row className="g-3 mt-2">

            {/* Categoría */}

            <Col xs={12} sm={6} md={2}>

              <InputGroup>

                <InputGroup.Text
                  className="
                    bg-white
                    border-0
                    rounded-start-pill
                    text-success
                    ps-3
                    pe-2
                  "
                >
                  <i className="bi bi-grid" />
                </InputGroup.Text>

                <Form.Select
                  aria-label="Categoría"
                  value={params.get('categoriaId') ?? ''}
                  onChange={(e) =>
                    poner({
                      categoriaId: e.target.value,
                    })
                  }
                  className="
                    bg-white
                    border-0
                    rounded-end-pill
                    shadow-none
                    ps-1
                  "
                >
                  <option value="" hidden>
                    Categoría
                  </option>

                  {categorias.datos?.map((c) => (
                    <option
                      key={c.categoriaId}
                      value={c.categoriaId}
                    >
                      {c.nombre}
                    </option>
                  ))}
                </Form.Select>

              </InputGroup>

            </Col>

            {/* ========================================
                DIFICULTAD
              ======================================== */}

            <Col xs={12} sm={6} md={2}>

              <InputGroup>

                <InputGroup.Text
                  className="
                    bg-white
                    border-0
                    rounded-start-pill
                    text-success
                    ps-3
                    pe-2
                  "
                >
                  <i className="bi bi-bar-chart" />
                </InputGroup.Text>

                <Form.Select
                  aria-label="Dificultad"
                  value={params.get('dificultadId') ?? ''}
                  onChange={(e) =>
                    poner({
                      dificultadId: e.target.value,
                    })
                  }
                  className="
                    bg-white
                    border-0
                    rounded-end-pill
                    shadow-none
                    ps-1
                  "
                >
                  <option value="" hidden>
                    Dificultad
                  </option>

                  {dificultades.datos?.map((d) => (
                    <option
                      key={d.dificultadId}
                      value={d.dificultadId}
                    >
                      {d.nombre}
                    </option>
                  ))}
                </Form.Select>

              </InputGroup>

            </Col>

            {/* ========================================
                TIEMPO
              ======================================== */}

            <Col xs={12} sm={6} md={2}>

              <InputGroup>

                <InputGroup.Text
                  className="
                    bg-white
                    border-0
                    rounded-start-pill
                    text-success
                    ps-3
                    pe-2
                  "
                >
                  <i className="bi bi-clock" />
                </InputGroup.Text>

                <Form.Select
                  aria-label="Tiempo máximo"
                  value={params.get('tiempoMax') ?? ''}
                  onChange={(e) =>
                    poner({
                      tiempoMax: e.target.value,
                    })
                  }
                  className="
                    bg-white
                    border-0
                    rounded-end-pill
                    shadow-none
                    ps-1
                  "
                >
                  <option value="" hidden>
                    Tiempo
                  </option>

                  {[15, 30, 60, 120].map((m) => (
                    <option
                      key={m}
                      value={m}
                    >
                      Hasta {m} min
                    </option>
                  ))}
                </Form.Select>

              </InputGroup>

            </Col>

            {/* ========================================
                INGREDIENTE
              ======================================== */}

            <Col xs={12} sm={6} md={3}>
              <div className=" bg-white border-0 shadow-none rounded-pill px-4 w-100">

                <InputIngrediente
                  valor={ingrediente}
                  onCambio={setIngrediente}
                  onSeleccion={agregarIngrediente}
                  placeholder="Ingrediente..."
                  aria-label="Filtrar por ingrediente"
                />

              </div>
            </Col>

            {/* Limpiar */}
            <Col xs="auto">
              <Button
                variant="outline-light"
                className="rounded-pill px-3"
                onClick={limpiar}
              >
                Limpiar
              </Button>
            </Col>
          </Row>
        </Container>
      </div>

      {/* ==================================================
              CATEGORÍAS VISUALES
          ================================================== */}

      <section className="bg-white border-bottom shadow-sm">
        <Container className="py-3">
          {/* Lista de categorías */}
          <div
            className="
                d-flex
                align-items-start
                justify-content-center
                gap-4
                overflow-x-auto
              "
          >
            <button
              type="button"
              className="
                  border-0
                  bg-transparent
                  text-center
                  flex-shrink-0
                  p-0
                "
              onClick={() =>
                poner({
                  categoriaId: '',
                })
              }
              aria-label="Mostrar todas las categorías"
            >
              <div
                className={`
                  rounded-circle
                  bg-light
                  d-flex
                  align-items-center
                  justify-content-center
                  mx-auto
                  mb-2
                  p-3
                  ${!params.get('categoriaId')
                    ? 'text-success'
                    : 'text-secondary'
                  }
                `}
              >
                <i className="bi bi-grid fs-4" aria-hidden="true" />

              </div>

              <small
                className={`
                    text-nowrap
                    ${!params.get('categoriaId')
                    ? 'text-success fw-bold'
                    : 'text-dark'
                  }
                  `}
              >
                Todas
              </small>

            </button>

            {categorias.datos?.map((c) => {

              const activa =
                String(params.get('categoriaId')) ===
                String(c.categoriaId)
              const nombre =
                c.nombre?.toLowerCase() ?? ''

              /* ----------------------------------------------
                Icono predeterminado
              ---------------------------------------------- */

              let icono = 'bi-fork-knife'

              /* ----------------------------------------------
                Icono según categoría
              ---------------------------------------------- */

              if (
                nombre.includes('bebida')
              ) {
                icono = 'bi-cup-straw'
              }

              else if (
                nombre.includes('desayuno')
              ) {
                icono = 'bi-cup-hot'
              }

              else if (
                nombre.includes('postre')
              ) {
                icono = 'bi-cake2'
              }

              else if (
                nombre.includes('vegetariano')
              ) {
                icono = 'bi-flower1'
              }

              else if (
                nombre.includes('plato')
              ) {
                icono = 'bi-fork-knife'
              }

              else if (
                nombre.includes('comida')
              ) {
                icono = 'bi-egg-fried'
              }

              else if (
                nombre.includes('ensalada')
              ) {
                icono = 'bi-flower2'
              }

              else if (
                nombre.includes('sopa')
              ) {
                icono = 'bi-cup-hot-fill'
              }

              return (

                <button
                  type="button"
                  key={c.categoriaId}
                  className="
                      border-0
                      bg-transparent
                      text-center
                      flex-shrink-0
                      p-0
                    "
                  onClick={() =>
                    poner({
                      categoriaId: c.categoriaId,
                    })
                  }
                  aria-label={`Filtrar por ${c.nombre}`}
                >
                  <div
                    className={`
                      rounded-circle
                      bg-light
                      d-flex
                      align-items-center
                      justify-content-center
                      mx-auto
                      mb-2
                      p-3
                      ${activa
                        ? 'text-success'
                        : 'text-secondary'
                      }
                    `}
                  >
                    <i className={`bi ${icono} fs-4`} aria-hidden="true" />

                  </div>

                  <small
                    className={`
                        text-nowrap
                        ${activa
                        ? 'text-success fw-bold'
                        : 'text-dark'
                      }
                      `}
                  >

                    {c.nombre}

                  </small>

                </button>

              )
            })}

          </div>

        </Container>
      </section>

      {/* ==================================================
          CONTENIDO
      ================================================== */}

      <Container className="py-4">

        {/* ================================================
            INGREDIENTES SELECCIONADOS
        ================================================ */}

        {elegidos.length > 0 && (

          <div
            className="
              d-flex
              flex-wrap
              align-items-center
              gap-2
              mb-4
            "
          >

            <small className="text-muted">
              Ingredientes:
            </small>

            {elegidos.map((i) => (

              <Badge
                key={i.id}
                bg="success"
                pill
                className="
                  d-inline-flex
                  align-items-center
                  gap-2
                  px-3
                  py-2
                  fw-normal
                "
              >

                {i.nombre}

                <button
                  type="button"
                  className="
                    btn-close
                    btn-close-white
                  "
                  aria-label={`Quitar ${i.nombre}`}
                  onClick={() =>
                    quitarIngrediente(i.id)
                  }
                />

              </Badge>

            ))}

            <Button
              variant="link"
              size="sm"
              className="
                text-success
                text-decoration-none
              "
              onClick={limpiar}
            >
              Limpiar filtros
            </Button>

          </div>

        )}

        {/* ================================================
            CARGANDO / ERROR
        ================================================ */}

        {cargando && <Cargando />}

        {error && (
          <ErrorCarga error={error} />
        )}

        {/* ================================================
            RESULTADOS
        ================================================ */}

        {datos && (

          datos.items.length === 0

            ? (
              /* Sin resultados */

              <div
                className="
                  text-center
                  bg-light
                  rounded-4
                  p-5
                  my-4
                "
              >
                <div className="display-5 mb-3">
                  🍽️
                </div>

                <h3 className="h4">
                  No encontramos recetas
                </h3>

                <p className="text-muted">
                  Prueba modificando los filtros o
                  realizando otra búsqueda.
                </p>

                <Button
                  variant="outline-success"
                  className="rounded-pill px-4"
                  onClick={limpiar}
                >
                  Limpiar filtros
                </Button>

              </div>
            )

            : (
              <>

                {/* ==========================================
                    CATÁLOGO
                ========================================== */}

                <div
                  className="
                    d-flex justify-content-between align-items-end mt-2 mb-3"
                >

                  <div>

                    <h2 className="h3 fw-bold mb-1">
                      Recetas
                    </h2>

                    <p className="text-muted small mb-0">
                      {datos.total}{' '}
                      receta
                      {datos.total === 1 ? '' : 's'}{' '}
                      encontrada
                      {datos.total === 1 ? '' : 's'}
                    </p>

                  </div>

                  {/* Orden */}

                  <div
                    className="
                      d-flex
                      align-items-center
                      gap-2
                    "
                  >
                    <small className="text-muted text-nowrap">
                      Ordenar por:
                    </small>

                    <Form.Select
                      size="sm"
                      aria-label="Ordenar recetas"
                      value={
                        params.get('orden') ?? ''
                      }
                      onChange={(e) =>
                        poner({
                          orden: e.target.value,
                        })
                      }
                    >

                      <option value="">
                        Más recientes
                      </option>

                      <option value="populares">
                        Más populares
                      </option>

                      <option value="tiempo">
                        Menor tiempo
                      </option>

                    </Form.Select>

                  </div>

                </div>

                {/* ==========================================
                    GRID DE RECETAS
                ========================================== */}

                <Row
                  xs={1}
                  sm={2}
                  lg={3}
                  xl={4}
                  className="g-4"
                >

                  {datos.items.map((r) => (

                    <Col
                      key={r.recetaId}
                      className="d-flex"
                    >

                      <div className="w-100">

                        <TarjetaReceta
                          receta={r}
                          enlace={
                            datos.busquedaId
                              ? `/recetas/${r.recetaId}?busquedaId=${datos.busquedaId}`
                              : undefined
                          }
                        />

                      </div>

                    </Col>

                  ))}

                </Row>

                {/* ==========================================
                    PAGINACIÓN
                ========================================== */}

                <div
                  className="
                    d-flex
                    justify-content-center
                    mt-5
                  "
                >

                  <Paginacion
                    pagina={datos.pagina}
                    tamano={datos.tamano}
                    total={datos.total}
                    onCambio={(n) =>
                      poner({ pagina: n })
                    }
                  />

                </div>

              </>
            )

        )}

      </Container>
    </>
  )
}