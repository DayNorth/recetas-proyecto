import { Badge, Card } from 'react-bootstrap'
import { Link } from 'react-router-dom'
import { urlArchivo } from '../api/client.js'
import Estrellas from './Estrellas.jsx'

export default function TarjetaReceta({ receta, enlace, className = 'h-100' }) {
  const portada = urlArchivo(receta.portada)
  return (
    <Card className={`${className} shadow-sm`}>
      {portada
        ? <Card.Img variant="top" src={portada} alt="" className="portada" loading="lazy" />
        : <div className="portada portada-vacia" aria-hidden="true">🍽️</div>}
      <Card.Body className="d-flex flex-column">
        <Card.Title as="h3" className="h6">
          <Link to={enlace ?? `/recetas/${receta.recetaId}`} className="stretched-link text-decoration-none">{receta.titulo}</Link>
        </Card.Title>
        <div className="mb-2">
          <Badge bg="secondary" className="me-1">{receta.categoria}</Badge>
          <Badge bg="light" text="dark" className="border">{receta.dificultad}</Badge>
        </div>
        <div className="mt-auto small text-muted d-flex justify-content-between">
          <span>⏱ {receta.tiempoPrepMin} min · ♥ {receta.meGusta}</span>
          <Estrellas promedio={receta.calificacionPromedio} total={receta.totalCalificaciones} />
        </div>
      </Card.Body>
    </Card>
  )
}
