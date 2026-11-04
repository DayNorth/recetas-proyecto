// Cliente único del API: agrega el Bearer, interpreta ProblemDetails y avisa cuando la sesión deja de servir.

const CLAVE_TOKEN = 'recetas.token'

export const guardarToken = (token) => {
  try {
    token ? localStorage.setItem(CLAVE_TOKEN, token) : localStorage.removeItem(CLAVE_TOKEN)
  } catch { /* sin almacenamiento: la sesión dura lo que dure la pestaña */ }
}
export const leerToken = () => {
  try { return localStorage.getItem(CLAVE_TOKEN) } catch { return null }
}

/** Error del API: status, title/detail del ProblemDetails y errores por campo { campo: [mensajes] }. */
export class ApiError extends Error {
  constructor(status, problema) {
    super(problema?.detail || problema?.title || `Error ${status}`)
    this.status = status
    this.titulo = problema?.title
    this.errores = problema?.errors ?? {}
  }
}

// AuthContext registra aquí qué hacer ante un 401 con sesión iniciada (cerrar sesión y volver al login).
let alExpirar = () => {}
export const alExpirarSesion = (fn) => { alExpirar = fn }

async function solicitar(metodo, url, cuerpo) {
  const cabeceras = {}
  const token = leerToken()
  if (token) cabeceras.Authorization = `Bearer ${token}`

  let body
  if (cuerpo instanceof FormData) body = cuerpo // el navegador pone el boundary
  else if (cuerpo !== undefined) {
    cabeceras['Content-Type'] = 'application/json'
    body = JSON.stringify(cuerpo)
  }

  let r
  try {
    r = await fetch(url, { method: metodo, headers: cabeceras, body })
  } catch {
    throw new ApiError(0, { title: 'Sin conexión', detail: 'No se pudo contactar al servidor. Intente de nuevo.' })
  }
  if (r.ok) return r
  const problema = await r.json().catch(() => null)
  if (r.status === 401 && token) alExpirar()
  throw new ApiError(r.status, problema)
}

const json = async (r) => (r.status === 204 ? null : r.json())

export const api = {
  get: (url) => solicitar('GET', url).then(json),
  post: (url, cuerpo) => solicitar('POST', url, cuerpo ?? undefined).then(json),
  put: (url, cuerpo) => solicitar('PUT', url, cuerpo ?? undefined).then(json),
  del: (url) => solicitar('DELETE', url).then(json),
  /** Descarga un archivo protegido (reportes) como Blob. */
  async blob(url) {
    const r = await solicitar('GET', url)
    return { blob: await r.blob(), nombre: nombreDeArchivo(r.headers.get('Content-Disposition')) }
  },
}

function nombreDeArchivo(cabecera) {
  const m = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(cabecera ?? '')
  return m ? decodeURIComponent(m[1]) : 'reporte'
}

/** Arma ?a=1&b=2 omitiendo vacíos; los arreglos repiten la clave. */
export function consulta(parametros) {
  const p = new URLSearchParams()
  for (const [k, v] of Object.entries(parametros)) {
    if (v === undefined || v === null || v === '') continue
    if (Array.isArray(v)) v.forEach((x) => p.append(k, x))
    else p.append(k, v)
  }
  const s = p.toString()
  return s ? `?${s}` : ''
}

/** Las subidas son rutas relativas (se sirven en /uploads/{ruta}); las recetas importadas traen una URL completa. */
export const urlArchivo = (ruta) => (!ruta ? null : /^https?:\/\//i.test(ruta) ? ruta : `/uploads/${ruta}`)
