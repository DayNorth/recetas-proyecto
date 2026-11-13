import { useState } from 'react'
import { Button, Container, Form, Nav, Navbar, NavDropdown } from 'react-bootstrap'
import "bootstrap-icons/font/bootstrap-icons.css";
import logoRecetas from '../assets/logo.png'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'

// Layout principal: cabecera, contenido de las rutas y pie de página.
export default function Layout() {
  const { usuario, esAdmin, cerrarSesion } = useAuth()
  const navegar = useNavigate()
  const [busqueda, setBusqueda] = useState('')

  const salir = async () => {
    await cerrarSesion()
    navegar('/')
  }

  const buscar = (evento) => {
    evento.preventDefault()
    const termino = busqueda.trim()
    navegar(termino ? `/?buscar=${encodeURIComponent(termino)}` : '/')
  }

  const enlaceActivo = ({ isActive }) =>
    `nav-link px-lg-3 ${isActive ? 'active fw-semibold' : ''}`

  return (
    <div className="app-shell">
      <style>{`
        :root {
          --recetas-verde: #198754;
          --recetas-verde-oscuro: #146c43;
          --recetas-tinta: #243129;
          --recetas-suave: #f5f8f6;
        }
        .app-shell { min-height: 100vh; display: flex; flex-direction: column; background: #fff; }
        .app-header { position: sticky; top: 0; z-index: 1030; background: #fff; box-shadow: 0 2px 14px rgba(24, 49, 34, .08); }
        .app-navbar { min-height: 70px; background: #fff; }
        .app-brand { color: var(--recetas-verde) !important; font-weight: 800; letter-spacing: -.02em; }
        .brand-mark { display: inline-grid; place-items: center; width: 2rem; height: 2rem; margin-right: .45rem; border-radius: 50%; background: #e8f5ed; }
        .app-navbar .nav-link { color: #48544c; border-radius: .5rem; }
        .app-navbar .nav-link:hover, .app-navbar .nav-link.active { color: var(--recetas-verde-oscuro); background: #edf7f1; }
        .header-search { width: min(100%, 360px); }
        .header-search .form-control:focus { border-color: #75b798; box-shadow: 0 0 0 .2rem rgba(25,135,84,.13); }
        .user-badge { display: inline-grid; place-items: center; width: 2rem; height: 2rem; border-radius: 50%; color: #fff; background: var(--recetas-verde); font-weight: 700; }
        .app-main { flex: 1 0 auto; width: 100%; padding-top: 2rem; padding-bottom: 4rem; }
        .app-footer { margin-top: auto; color: #dbe6df; background: #173126; }
        .footer-brand { color: #fff; font-weight: 800; }
        .footer-title { color: #fff; font-size: .9rem; font-weight: 700; text-transform: uppercase; letter-spacing: .05em; }
        .footer-link { color: #dbe6df; text-decoration: none; }
        .footer-link:hover { color: #fff; text-decoration: underline; }
        .footer-bottom { border-top: 1px solid rgba(255,255,255,.14); }
        @media (max-width: 991.98px) {
          .app-navbar { padding-block: .75rem; }
          .header-search { width: 100%; margin-block: .75rem; }
          .app-navbar .navbar-nav { gap: .2rem; }
        }
      `}</style>

      <header className="app-header">
        <Navbar expand="lg" className="app-navbar" aria-label="Navegación principal">
          <Container>
          <Navbar.Brand as={Link} to="/" className="app-brand d-flex align-items-center gap-2 py-0">
              <img
                src={logoRecetas}
                alt="Logo Recetas"
                width="70"
                height="70"
                className="rounded object-fit-contain"
              />
            <div className="d-flex flex-column lh-1">
              <span className="fw-bold fs-4 text-success">Recetas</span>
            </div>
          </Navbar.Brand>
            <Navbar.Toggle aria-controls="menu-principal" aria-label="Abrir menú de navegación" />
            <Navbar.Collapse id="menu-principal">
              <Nav className="ms-lg-3 me-lg-auto align-items-lg-center">
                <Nav.Link as={NavLink} className={enlaceActivo} to="/" end>Catálogo</Nav.Link>
                {usuario && (
                  <>
                    <Nav.Link as={NavLink} className={enlaceActivo} to="/recetas/nueva">Crear receta</Nav.Link>
                    <Nav.Link as={NavLink} className={enlaceActivo} to="/mis-recetas">Mis recetas</Nav.Link>
                    <Nav.Link as={NavLink} className={enlaceActivo} to="/favoritos">Favoritos</Nav.Link>
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

              <Form className="header-search d-flex me-lg-3" role="search" onSubmit={buscar}>
                <Form.Control
                  type="search"
                  value={busqueda}
                  onChange={(e) => setBusqueda(e.target.value)}
                  placeholder="Buscar recetas..."
                  aria-label="Buscar recetas"
                />
                <Button variant="success" type="submit" className="ms-2 d-flex align-items-center" aria-label="Buscar">
                  <i className="bi bi-search me-1" aria-hidden="true"></i>
                  Buscar
                </Button>
              </Form>

              <Nav className="align-items-lg-center">
                {usuario ? (
                  <NavDropdown
                    align="end"
                    id="menu-usuario"
                    title={<span><span className="user-badge me-2" aria-hidden="true">{usuario.nombre?.charAt(0)?.toUpperCase() || 'U'}</span>{usuario.nombre}</span>}
                  >
                    <NavDropdown.Header>{usuario.rol === 'ADMIN' ? 'Administrador' : 'Usuario'}</NavDropdown.Header>
                    <NavDropdown.Item as={NavLink} to="/perfil">Mi perfil</NavDropdown.Item>
                    <NavDropdown.Divider />
                    <NavDropdown.Item onClick={salir}>Cerrar sesión</NavDropdown.Item>
                  </NavDropdown>
                ) : (
                  <>
                    <Nav.Link as={NavLink} className={enlaceActivo} to="/login">Iniciar sesión</Nav.Link>
                    <Button as={Link} to="/registro" variant="success" className="ms-lg-2">Registrarse</Button>
                  </>
                )}
              </Nav>
            </Navbar.Collapse>
          </Container>
        </Navbar>
      </header>

      <Container as="main" className="app-main" id="contenido-principal">
        <Outlet />
      </Container>

      <footer className="app-footer" aria-label="Pie de página">
        <Container className="py-5">
          <div className="row g-4">
            <div className="col-12 col-lg-5">
              <div className="footer-brand d-flex align-items-center gap-2 mb-3">
                <img
                  src={logoRecetas}
                  alt="Logo Recetas"
                  width="100"
                  height="100"
                  className="rounded object-fit-contain bg-white p-1"
                />
                <div className="d-flex flex-column lh-1">
                  <span className="fw-normal fs-6 text-white">Descubra, comparta y guarde recetas para disfrutar la cocina todos los días.</span>
                </div>
              </div>
            </div>
            <div className="col-6 col-lg-2">
              <div className="footer-title mb-3">Explorar</div>
              <Nav className="flex-column gap-2">
                <Link className="footer-link" to="/">Catálogo</Link>
                {usuario && <Link className="footer-link" to="/favoritos">Favoritos</Link>}
              </Nav>
            </div>
            <div className="col-6 col-lg-2">
              <div className="footer-title mb-3">Mi cuenta</div>
              <Nav className="flex-column gap-2">
                {usuario ? (
                  <>
                    <Link className="footer-link" to="/perfil">Mi perfil</Link>
                    <Link className="footer-link" to="/mis-recetas">Mis recetas</Link>
                  </>
                ) : (
                  <>
                    <Link className="footer-link" to="/login">Iniciar sesión</Link>
                    <Link className="footer-link" to="/registro">Registrarse</Link>
                  </>
                )}
              </Nav>
            </div>
            <div className="col-12 col-lg-3">
              <div className="footer-title mb-3">Comunidad</div>
              <p className="mb-0 small">Comparta sus mejores recetas y descubra nuevas ideas de otros amantes de la cocina.</p>
            </div>
          </div>
          <div className="footer-bottom mt-4 pt-4 d-flex flex-column flex-md-row justify-content-between gap-2 small">
            <span>© {new Date().getFullYear()} Recetas. Todos los derechos reservados.</span>
            <span>Hecho con cariño para quienes disfrutan cocinar.</span>
          </div>
        </Container>
      </footer>
    </div>
  )
}
