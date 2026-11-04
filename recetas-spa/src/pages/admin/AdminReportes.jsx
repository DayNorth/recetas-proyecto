import { useState } from 'react'
import { Alert, Button, Card, Col, Form, Row, Table } from 'react-bootstrap'
import { Bar, BarChart, CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api, consulta } from '../../api/client.js'
import { useAvisos } from '../../components/Avisos.jsx'
import Cargando, { ErrorCarga } from '../../components/Cargando.jsx'
import { useCarga } from '../../hooks/useCarga.js'

const num = (v) => (typeof v === 'number' ? v.toLocaleString('es-CR', { maximumFractionDigits: 2 }) : (v ?? '—'))

// S-22 Reportes de actividad (RP-01 a RP-03): elegir reporte y meses, ver tabla y gráfico, descargar.
export default function AdminReportes() {
  const avisar = useAvisos()
  const catalogo = useCarga(() => api.get('/api/reportes'), 'catalogo')
  const [tipo, setTipo] = useState('')
  const [mesInicio, setMesInicio] = useState('')
  const [mesFin, setMesFin] = useState('')
  const [limite, setLimite] = useState('10')
  const [minVotos, setMinVotos] = useState('3')
  const [minVisitas, setMinVisitas] = useState('20')
  const [resultado, setResultado] = useState(null)
  const [error, setError] = useState(null)
  const [cargando, setCargando] = useState(false)
  const [medida, setMedida] = useState('')

  const definicion = catalogo.datos?.find((r) => r.tipo === tipo)
  const usa = (p) => definicion?.parametros.includes(p)
  const parametros = () => ({
    mesInicio, mesFin,
    limite: usa('limite') ? limite : undefined,
    minVotos: usa('minVotos') ? minVotos : undefined,
    minVisitas: usa('minVisitas') ? minVisitas : undefined,
  })

  const ejecutar = async (e) => {
    e.preventDefault()
    setError(null); setCargando(true)
    try {
      const r = await api.get(`/api/reportes/${tipo}${consulta(parametros())}`)
      setResultado(r)
      setMedida(r.columnas.find((c) => c.tipo !== 'texto')?.campo ?? '')
    } catch (err) { setError(err); setResultado(null) } finally { setCargando(false) }
  }

  const descargar = async (formato) => {
    try {
      const { blob, nombre } = await api.blob(`/api/reportes/${resultado.tipo}/descargar${consulta({ ...parametros(), formato })}`)
      const enlace = document.createElement('a')
      enlace.href = URL.createObjectURL(blob)
      enlace.download = nombre
      enlace.click()
      URL.revokeObjectURL(enlace.href)
    } catch (err) { avisar(err.message, 'danger') }
  }

  const columnaEtiqueta = resultado?.columnas.find((c) => c.tipo === 'texto')
  const numericas = resultado?.columnas.filter((c) => c.tipo !== 'texto') ?? []
  const datosGrafico = resultado && columnaEtiqueta && medida
    ? resultado.filas.map((f) => ({ etiqueta: String(f[columnaEtiqueta.campo] ?? ''), valor: Number(f[medida] ?? 0) }))
    : []
  const Grafico = resultado?.tipo === 'TENDENCIA_MENSUAL' ? LineChart : BarChart

  return (
    <>
      <h1 className="h3 mb-3">Reportes de actividad</h1>
      {catalogo.cargando && <Cargando />}
      {catalogo.error && <ErrorCarga error={catalogo.error} />}
      <Form onSubmit={ejecutar} className="mb-4">
        <Row className="g-2 align-items-end">
          <Col md={4}>
            <Form.Label htmlFor="tipo">Reporte</Form.Label>
            <Form.Select id="tipo" value={tipo} onChange={(e) => { setTipo(e.target.value); setResultado(null) }}>
              <option value="">Elija un reporte…</option>
              {catalogo.datos?.map((r) => <option key={r.tipo} value={r.tipo}>{r.titulo}</option>)}
            </Form.Select>
          </Col>
          <Col xs={6} md={2}>
            <Form.Label htmlFor="desde">Desde (mes)</Form.Label>
            <Form.Control id="desde" type="month" value={mesInicio} onChange={(e) => setMesInicio(e.target.value)} />
          </Col>
          <Col xs={6} md={2}>
            <Form.Label htmlFor="hasta">Hasta (mes)</Form.Label>
            <Form.Control id="hasta" type="month" value={mesFin} onChange={(e) => setMesFin(e.target.value)} />
          </Col>
          {usa('limite') && <Col xs={4} md={1}><Form.Label htmlFor="limite">Filas</Form.Label><Form.Control id="limite" type="number" min={1} max={100} value={limite} onChange={(e) => setLimite(e.target.value)} /></Col>}
          {usa('minVotos') && <Col xs={4} md={1}><Form.Label htmlFor="mv">Mín. votos</Form.Label><Form.Control id="mv" type="number" min={1} value={minVotos} onChange={(e) => setMinVotos(e.target.value)} /></Col>}
          {usa('minVisitas') && <Col xs={4} md={1}><Form.Label htmlFor="mvi">Mín. visitas</Form.Label><Form.Control id="mvi" type="number" min={1} value={minVisitas} onChange={(e) => setMinVisitas(e.target.value)} /></Col>}
          <Col xs="auto"><Button type="submit" disabled={!tipo || cargando}>{cargando ? 'Generando…' : 'Generar'}</Button></Col>
        </Row>
        <Form.Text muted>Sin meses, se usan los 6 meses completos anteriores al actual (máximo 24 meses).</Form.Text>
      </Form>

      {definicion && !resultado && (
        <Card className="mb-3"><Card.Body className="small">
          <p className="mb-1"><strong>Pregunta:</strong> {definicion.pregunta}</p>
        </Card.Body></Card>
      )}
      {error && <Alert variant="danger">{error.message}</Alert>}

      {resultado && (
        <>
          <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
            <h2 className="h5 mb-0">{resultado.titulo} <small className="text-muted">{resultado.rango.inicio} a {resultado.rango.fin}</small></h2>
            <div className="d-flex gap-2">
              {['csv', 'xlsx', 'pdf'].map((f) => <Button key={f} size="sm" variant="outline-primary" onClick={() => descargar(f)}>{f.toUpperCase()}</Button>)}
            </div>
          </div>

          <Card className="mb-3"><Card.Body className="small">
            <p className="mb-1"><strong>Pregunta:</strong> {resultado.decision.pregunta}</p>
            <p className="mb-1"><strong>Cómo leerlo:</strong> {resultado.decision.reglaLectura}</p>
            <p className="mb-0"><strong>Qué hacer:</strong> {resultado.decision.accion}</p>
          </Card.Body></Card>

          {resultado.filas.length === 0 ? <p className="text-muted">Sin datos en este rango.</p> : (
            <>
              {datosGrafico.length > 0 && (
                <div className="mb-3">
                  {numericas.length > 1 && (
                    <Form.Select size="sm" aria-label="Medida del gráfico" value={medida} onChange={(e) => setMedida(e.target.value)} className="mb-2" style={{ maxWidth: 260 }}>
                      {numericas.map((c) => <option key={c.campo} value={c.campo}>{c.titulo}</option>)}
                    </Form.Select>
                  )}
                  <div style={{ width: '100%', height: 280 }} role="img" aria-label={`Gráfico de ${resultado.titulo}`}>
                    <ResponsiveContainer>
                      <Grafico data={datosGrafico}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="etiqueta" interval={0} tick={{ fontSize: 11 }} tickFormatter={(t) => (t.length > 14 ? `${t.slice(0, 13)}…` : t)} />
                        <YAxis />
                        <Tooltip />
                        {resultado.tipo === 'TENDENCIA_MENSUAL'
                          ? <Line type="monotone" dataKey="valor" name={numericas.find((c) => c.campo === medida)?.titulo} stroke="#0d6efd" />
                          : <Bar dataKey="valor" name={numericas.find((c) => c.campo === medida)?.titulo} fill="#0d6efd" />}
                      </Grafico>
                    </ResponsiveContainer>
                  </div>
                </div>
              )}
              <div className="table-responsive">
                <Table size="sm" striped>
                  <thead><tr>{resultado.columnas.map((c) => <th key={c.campo} className={c.tipo === 'texto' ? '' : 'text-end'}>{c.titulo}</th>)}</tr></thead>
                  <tbody>
                    {resultado.filas.map((f, i) => (
                      <tr key={i}>{resultado.columnas.map((c) => <td key={c.campo} className={c.tipo === 'texto' ? '' : 'text-end'}>{num(f[c.campo])}</td>)}</tr>
                    ))}
                  </tbody>
                </Table>
              </div>
            </>
          )}
        </>
      )}
    </>
  )
}
