import { useState } from 'react'
import { Alert, Button, Card, Form } from 'react-bootstrap'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'
import Campo, { errorDe } from '../components/Campo.jsx'

// S-05 Iniciar sesión (AU-02)
export default function Login() {
  const { entrar } = useAuth()
  const navegar = useNavigate()
  const { state } = useLocation()
  const [correo, setCorreo] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [enviando, setEnviando] = useState(false)

  const enviar = async (e) => {
    e.preventDefault()
    setError(null)
    setEnviando(true)
    try {
      await entrar(correo.trim(), password)
      navegar(state?.desde ?? '/', { replace: true })
    } catch (err) {
      setError(err)
      setEnviando(false)
    }
  }

  return (
    <Card className="mx-auto" style={{ maxWidth: 420 }}>
      <Card.Body>
        <h1 className="h4 mb-3">Iniciar sesión</h1>
        {state?.desde && <Alert variant="info">Inicie sesión para continuar.</Alert>}
        {error && !Object.keys(error.errores).length && <Alert variant="danger">{error.message}</Alert>}
        <Form onSubmit={enviar} noValidate>
          <Campo id="correo" etiqueta="Correo" error={errorDe(error, 'correo')}>
            <Form.Control type="email" autoComplete="email" value={correo} onChange={(e) => setCorreo(e.target.value)} required />
          </Campo>
          <Campo id="password" etiqueta="Contraseña" error={errorDe(error, 'password')}>
            <Form.Control type="password" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} required />
          </Campo>
          <Button type="submit" className="w-100" disabled={enviando}>{enviando ? 'Ingresando…' : 'Ingresar'}</Button>
        </Form>
        <p className="mt-3 mb-0 small">¿No tiene cuenta? <Link to="/registro">Regístrese</Link></p>
      </Card.Body>
    </Card>
  )
}
