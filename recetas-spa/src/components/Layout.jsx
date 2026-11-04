import { Container, Nav, Navbar, NavDropdown } from 'react-bootstrap'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'

// Menú por rol: visitante, USUARIO y ADMIN (el admin ve además lo de usuario).
export default function Layout() {
  const { usuario, esAdmin, cerrarSesion } = useAuth()
  const navegar = useNavigate()

  const salir = async () => {
    await cerrarSesion()
    navegar('/')
  }

  return (
    <>
      <Navbar expand="lg" bg="dark" data-bs-theme="dark" className="mb-4">
        <Container>
          <Navbar.Brand as={Link} to="/">🍲 Recetas</Navbar.Brand>
          <Navbar.Toggle aria-controls="menu" />
          <Navbar.Collapse id="menu">
            <Nav className="me-auto">
              <Nav.Link as={NavLink} to="/" end>Catálogo</Nav.Link>
              {usuario && (
                <>
                  <Nav.Link as={NavLink} to="/recetas/nueva">Crear receta</Nav.Link>
                  <Nav.Link as={NavLink} to="/mis-recetas">Mis recetas</Nav.Link>
                  <Nav.Link as={NavLink} to="/favoritos">Favoritos</Nav.Link>
                </>
              )}
              {esAdmin && (
                <NavDropdown title="Administración" id="menu-admin">
                  <NavDropdown.Item as={NavLink} to="/admin/categorias">Categorías</NavDropdown.Item>
                  <NavDropdown.Item as={NavLink} to="/admin/reportes">Reportes</NavDropdown.Item>
                  <NavDropdown.Item as={NavLink} to="/admin/tablero">Tablero de decisiones</NavDropdown.Item>
                </NavDropdown>
              )}
            </Nav>
            <Nav>
              {usuario ? (
                <NavDropdown title={`${usuario.nombre} (${usuario.rol === 'ADMIN' ? 'Admin' : 'Usuario'})`} align="end" id="menu-usuario">
                  <NavDropdown.Item as={NavLink} to="/perfil">Mi perfil</NavDropdown.Item>
                  <NavDropdown.Divider />
                  <NavDropdown.Item onClick={salir}>Cerrar sesión</NavDropdown.Item>
                </NavDropdown>
              ) : (
                <>
                  <Nav.Link as={NavLink} to="/login">Iniciar sesión</Nav.Link>
                  <Nav.Link as={NavLink} to="/registro">Registrarse</Nav.Link>
                </>
              )}
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Container as="main" className="pb-5">
        <Outlet />
      </Container>
    </>
  )
}
