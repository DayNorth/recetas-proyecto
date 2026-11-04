import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout.jsx'
import RutaProtegida from './components/RutaProtegida.jsx'
import AdminCategorias from './pages/admin/AdminCategorias.jsx'
import AdminReportes from './pages/admin/AdminReportes.jsx'
import AdminTablero from './pages/admin/AdminTablero.jsx'
import { NoEncontrado, SinPermiso } from './pages/Avisos404.jsx'
import Catalogo from './pages/Catalogo.jsx'
import CrearReceta from './pages/CrearReceta.jsx'
import DetalleReceta from './pages/DetalleReceta.jsx'
import EditarReceta from './pages/EditarReceta.jsx'
import Estado from './pages/Estado.jsx'
import Favoritos from './pages/Favoritos.jsx'
import Login from './pages/Login.jsx'
import MisRecetas from './pages/MisRecetas.jsx'
import Perfil from './pages/Perfil.jsx'
import Registro from './pages/Registro.jsx'

// Acceso por ruta: público · RutaProtegida (sesión) · RutaProtegida soloAdmin (rol ADMIN).
// El servidor vuelve a validar todo: esto solo evita mostrar pantallas que igual darían 401/403.
export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Catalogo />} />
        <Route path="/recetas/:id" element={<DetalleReceta />} />
        <Route path="/login" element={<Login />} />
        <Route path="/registro" element={<Registro />} />
        <Route path="/estado" element={<Estado />} />
        <Route path="/sin-permiso" element={<SinPermiso />} />

        <Route element={<RutaProtegida />}>
          <Route path="/recetas/nueva" element={<CrearReceta />} />
          <Route path="/recetas/:id/editar" element={<EditarReceta />} />
          <Route path="/mis-recetas" element={<MisRecetas />} />
          <Route path="/favoritos" element={<Favoritos />} />
          <Route path="/perfil" element={<Perfil />} />
        </Route>

        <Route element={<RutaProtegida soloAdmin />}>
          <Route path="/admin/categorias" element={<AdminCategorias />} />
          <Route path="/admin/reportes" element={<AdminReportes />} />
          <Route path="/admin/tablero" element={<AdminTablero />} />
        </Route>

        <Route path="*" element={<NoEncontrado />} />
      </Route>
    </Routes>
  )
}
