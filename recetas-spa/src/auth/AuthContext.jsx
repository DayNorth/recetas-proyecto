import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import { alExpirarSesion, api, guardarToken, leerToken } from '../api/client.js'

const AuthContext = createContext(null)
export const useAuth = () => useContext(AuthContext)

export function AuthProvider({ children }) {
  const [usuario, setUsuario] = useState(null)
  const [listo, setListo] = useState(false) // false mientras se valida el token guardado

  const salir = useCallback(() => {
    guardarToken(null)
    setUsuario(null)
  }, [])

  // Al cargar: si hay token, GET /auth/me confirma que sigue vigente.
  useEffect(() => {
    alExpirarSesion(salir)
    if (!leerToken()) { setListo(true); return }
    api.get('/api/auth/me').then(setUsuario).catch(salir).finally(() => setListo(true))
  }, [salir])

  const entrar = useCallback(async (correo, password) => {
    const r = await api.post('/api/auth/login', { correo, password })
    guardarToken(r.token)
    setUsuario(r.usuario)
    return r.usuario
  }, [])

  const cerrarSesion = useCallback(async () => {
    try { await api.post('/api/auth/logout') } catch { /* igual se cierra en el cliente */ }
    salir()
  }, [salir])

  const valor = useMemo(
    () => ({ usuario, listo, entrar, cerrarSesion, actualizarUsuario: setUsuario, esAdmin: usuario?.rol === 'ADMIN' }),
    [usuario, listo, entrar, cerrarSesion],
  )
  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>
}
