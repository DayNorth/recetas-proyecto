import { useState } from 'react'
import { Alert, Button, Card, Form } from 'react-bootstrap'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import Campo, { errorDe } from '../components/Campo.jsx'

const VACIO = { nombre: '', apellidos: '', correo: '', password: '', confirmacionPassword: '' }

// S-06 Registro (AU-01): crea la cuenta y entra de una vez.
export default function Registro() {
  const { entrar } = useAuth()
  const navegar = useNavigate()
  const [f, setF] = useState(VACIO)
  const [error, setError] = useState(null)
  const [enviando, setEnviando] = useState(false)
  const cambiar = (campo) => (e) => setF({ ...f, [campo]: e.target.value })

  const enviar = async (e) => {
    e.preventDefault()
    setError(null)
    setEnviando(true)
    try {
      await api.post('/api/auth/registro', { ...f, correo: f.correo.trim() })
      await entrar(f.correo.trim(), f.password)
      navegar('/', { replace: true })
    } catch (err) {
      setError(err)
      setEnviando(false)
    }
  }

  const campo = (id, etiqueta, tipo = 'text', auto) => (
    <Campo id={id} etiqueta={etiqueta} error={errorDe(error, id)}>
      <Form.Control type={tipo} autoComplete={auto} value={f[id]} onChange={cambiar(id)} required />
    </Campo>
  )

  return (
    <Card className="mx-auto" style={{ maxWidth: 480 }}>
      <Card.Body>
        <h1 className="h4 mb-3">Crear cuenta</h1>
        {error && !Object.keys(error.errores).length && <Alert variant="danger">{error.message}</Alert>}
        <Form onSubmit={enviar} noValidate>
          {campo('nombre', 'Nombre', 'text', 'given-name')}
          {campo('apellidos', 'Apellidos', 'text', 'family-name')}
          {campo('correo', 'Correo', 'email', 'email')}
          <Campo id="password" etiqueta="Contraseña" error={errorDe(error, 'password')} ayuda="Mínimo 8 caracteres, con una letra y un número.">
            <Form.Control type="password" autoComplete="new-password" value={f.password} onChange={cambiar('password')} required />
          </Campo>
          {campo('confirmacionPassword', 'Confirmar contraseña', 'password', 'new-password')}
          <Button type="submit" className="w-100" disabled={enviando}>{enviando ? 'Creando…' : 'Registrarme'}</Button>
        </Form>
        <p className="mt-3 mb-0 small">¿Ya tiene cuenta? <Link to="/login">Inicie sesión</Link></p>
      </Card.Body>
    </Card>
  )
}
