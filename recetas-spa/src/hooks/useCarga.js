import { useCallback, useEffect, useState } from 'react'

/**
 * Carga datos al montar y cuando cambia `clave` (normalmente la URL).
 * Devuelve { datos, error, cargando, recargar }.
 */
export function useCarga(cargar, clave) {
  const [estado, setEstado] = useState({ datos: null, error: null, cargando: true })

  const ejecutar = useCallback(() => {
    let vigente = true
    setEstado((e) => ({ ...e, cargando: true, error: null }))
    cargar()
      .then((datos) => vigente && setEstado({ datos, error: null, cargando: false }))
      .catch((error) => vigente && setEstado({ datos: null, error, cargando: false }))
    return () => { vigente = false }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [clave])

  useEffect(ejecutar, [ejecutar])
  return { ...estado, recargar: ejecutar }
}
