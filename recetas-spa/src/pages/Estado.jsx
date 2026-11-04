import { useEffect, useState } from 'react'

// Llama a GET /api/health: comprueba la cadena navegador -> nginx -> API -> PostgreSQL.
export default function Estado() {
  const [datos, setDatos] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    fetch('/api/health')
      .then((r) => (r.ok ? r.json() : Promise.reject(new Error(`HTTP ${r.status}`))))
      .then(setDatos)
      .catch((e) => setError(e.message))
  }, [])

  if (error) return <p role="alert">No se pudo contactar la API: {error}</p>
  if (!datos) return <p>Comprobando…</p>

  const bd = datos.baseDeDatos
  return (
    <section>
      <h2>Estado del sistema</h2>
      <ul>
        <li>API: <strong>{datos.estado}</strong></li>
        <li>Base de datos conectada: <strong>{bd.conectada ? 'sí' : 'no'}</strong></li>
        <li>Versión: {bd.version}</li>
        <li>Tablas: {bd.tablas}</li>
        <li>Recetas: {bd.recetas} · Usuarios: {bd.usuarios}</li>
        <li>Receta más vista (mayo a octubre): #{bd.recetaMasVistaEnElRango}</li>
      </ul>
    </section>
  )
}
