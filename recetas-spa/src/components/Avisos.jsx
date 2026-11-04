import { createContext, useCallback, useContext, useState } from 'react'
import { Toast, ToastContainer } from 'react-bootstrap'

const AvisosContext = createContext(() => {})
/** const avisar = useAvisos(); avisar('Guardado'); avisar('Falló', 'danger') */
export const useAvisos = () => useContext(AvisosContext)

export function AvisosProvider({ children }) {
  const [avisos, setAvisos] = useState([])

  const avisar = useCallback((texto, tipo = 'success') => {
    const id = Date.now() + Math.random()
    setAvisos((a) => [...a, { id, texto, tipo }])
  }, [])
  const quitar = (id) => setAvisos((a) => a.filter((x) => x.id !== id))

  return (
    <AvisosContext.Provider value={avisar}>
      {children}
      <ToastContainer position="bottom-end" className="p-3" style={{ position: 'fixed', zIndex: 2000 }}>
        {avisos.map((a) => (
          <Toast key={a.id} bg={a.tipo} delay={5000} autohide onClose={() => quitar(a.id)}>
            <Toast.Body className={a.tipo === 'warning' ? '' : 'text-white'}>{a.texto}</Toast.Body>
          </Toast>
        ))}
      </ToastContainer>
    </AvisosContext.Provider>
  )
}
