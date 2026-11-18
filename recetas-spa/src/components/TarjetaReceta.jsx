import { Badge, Card } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { urlArchivo } from '../api/client.js'
import Estrellas from './Estrellas.jsx'

export default function TarjetaReceta({
  receta,
  enlace,
  className = 'h-100',
}) {
  const portada = urlArchivo(receta.portada)

  const urlReceta =
    enlace ?? `/recetas/${receta.recetaId}`

  return (
    <Card
      className={`${className} border shadow-sm rounded-3 overflow-hidden`}
    >

      {/* ================================
          IMAGEN
      ================================= */}

      <div className="position-relative ratio ratio-4x3 bg-light">

        {portada ? (

          <Card.Img
            src={portada}
            alt={receta.titulo ?? 'Receta'}
            loading="lazy"
            className="w-100 h-100 object-fit-cover rounded-0"
          />

        ) : (

          <div
            className="d-flex align-items-center justify-content-center w-100 h-100 bg-light text-secondary"
            aria-hidden="true"
          >
            <span className="display-4">
              🍽️
            </span>
          </div>

        )}

      </div>

      {/* ================================
          CONTENIDO
      ================================= */}

      <Card.Body className="d-flex flex-column p-3">

        {/* TÍTULO */}

        <Card.Title
          as="h3"
          className="h6 fw-bold mb-2"
        >

          <Link
            to={urlReceta}
            className="stretched-link text-decoration-none text-dark"
          >
            {receta.titulo}
          </Link>

        </Card.Title>

        {/* ================================
            CATEGORÍA Y DIFICULTAD
        ================================= */}

        <div className="d-flex flex-wrap gap-2 mb-3">
          {receta.categoria && (

            <Badge
              bg="dark"
              pill
              className="fw-normal px-3 py-2"
            >
              {receta.categoria}
            </Badge>

          )}
          {receta.dificultad && (() => {
            const dificultad = receta.dificultad.toLowerCase()
            let clase = 'bg-secondary-subtle text-secondary'
            if (
              dificultad === 'fácil'
            ) {
              clase = 'bg-success-subtle text-success'
            }
            if (
              dificultad === 'intermedio'
            ) {
              clase = 'bg-warning-subtle text-warning-emphasis'
            }
            if (
              dificultad === 'difícil'
            ) {
              clase = 'bg-danger-subtle text-danger'
            }

            return (
              <Badge
                pill
                className={`border fw-normal px-3 py-2 ${clase}`}
              >
                {receta.dificultad}
              </Badge>
            )
          })()}

        </div>

        {/* ================================
            INFORMACIÓN
        ================================= */}

        <div className="mt-auto">

          <div className="d-flex align-items-center justify-content-between gap-2 small text-secondary">

            {/* TIEMPO */}

            <span className="text-nowrap">
              <i className="bi-clock"></i> {receta.tiempoPrepMin ?? 0} min
            </span>

            {/* ME GUSTA */}

            <span className="text-nowrap">
              <i
                className={
                  (receta.meGusta ?? 0) > 0
                    ? 'bi bi-heart-fill text-danger'
                    : 'bi bi-heart'
                }
              ></i>

              {' '}
              {receta.meGusta ?? 0}
            </span>
          </div>

          {/* SEPARADOR */}

          <hr className="my-2" />

          {/* ================================
              CALIFICACIÓN
          ================================= */}

          <div className="d-flex align-items-center justify-content-between gap-2">

            <Estrellas
              promedio={receta.calificacionPromedio}
              total={receta.totalCalificaciones}
              tamano="sm"
              mostrarTexto={false}
            />
            <small className="text-secondary text-nowrap">

              {Number(
                receta.calificacionPromedio ?? 0
              ).toFixed(1)}

              {' '}
              ({receta.totalCalificaciones ?? 0})
            </small>

          </div>

        </div>

      </Card.Body>

    </Card>
  )
}