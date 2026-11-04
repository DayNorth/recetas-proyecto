import { useNavigate } from 'react-router-dom'
import { api } from '../api/client.js'
import { useAvisos } from '../components/Avisos.jsx'
import FormularioReceta, { formularioASolicitud, formularioVacio } from '../components/FormularioReceta.jsx'

// S-07 Crear receta (RC-05): una sola transacción en el servidor (ingredientes nuevos + receta + detalle + portada).
export default function CrearReceta() {
  const navegar = useNavigate()
  const avisar = useAvisos()

  const guardar = async (formulario, imagen) => {
    const cuerpo = new FormData()
    cuerpo.append('datos', JSON.stringify(formularioASolicitud(formulario)))
    if (imagen) cuerpo.append('imagen', imagen)
    const r = await api.post('/api/recetas', cuerpo)
    avisar('Receta creada.')
    navegar(`/recetas/${r.recetaId}`)
  }

  return (
    <>
      <h1 className="h3 mb-3">Crear receta</h1>
      <FormularioReceta inicial={formularioVacio()} conImagen textoBoton="Crear receta" onGuardar={guardar} />
    </>
  )
}
