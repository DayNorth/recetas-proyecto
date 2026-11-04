import { useEffect, useId, useState } from 'react'
import { Form } from 'react-bootstrap'
import { api, consulta } from '../api/client.js'

/**
 * Texto con sugerencias de ingredientes (CT-05, por prefijo). Usa <datalist> nativo.
 * onSeleccion(ingrediente) se llama cuando el texto coincide exactamente con una sugerencia.
 */
export default function InputIngrediente({ valor, onCambio, onSeleccion, ...resto }) {
  const lista = useId()
  const [sugerencias, setSugerencias] = useState([])

  useEffect(() => {
    if (valor.trim().length < 2) { setSugerencias([]); return }
    let vigente = true
    const espera = setTimeout(() => {
      api.get(`/api/ingredientes${consulta({ q: valor.trim() })}`)
        .then((r) => vigente && setSugerencias(r))
        .catch(() => vigente && setSugerencias([]))
    }, 250)
    return () => { vigente = false; clearTimeout(espera) }
  }, [valor])

  const cambiar = (e) => {
    const texto = e.target.value
    onCambio(texto)
    const exacta = sugerencias.find((s) => s.nombre.toLowerCase() === texto.trim().toLowerCase())
    if (exacta && onSeleccion) onSeleccion(exacta)
  }

  return (
    <>
      <Form.Control value={valor} onChange={cambiar} list={lista} autoComplete="off" {...resto} />
      <datalist id={lista}>
        {sugerencias.map((s) => <option key={s.ingredienteId} value={s.nombre} />)}
      </datalist>
    </>
  )
}
