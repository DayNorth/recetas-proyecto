import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'
import Cargando from './Cargando.jsx'

/** <Route element={<RutaProtegida />}> exige sesión; con soloAdmin exige además el rol ADMIN. */
export default function RutaProtegida({ soloAdmin = false }) {
  const { usuario, listo, esAdmin } = useAuth()
  const ubicacion = useLocation()

  if (!listo) return <Cargando />
  if (!usuario) return <Navigate to="/login" replace state={{ desde: ubicacion.pathname + ubicacion.search }} />
  if (soloAdmin && !esAdmin) return <Navigate to="/sin-permiso" replace />
  return <Outlet />
}
