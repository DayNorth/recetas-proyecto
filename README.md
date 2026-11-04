# App de Recetas — entorno local (Docker)

Plataforma web de recetas (Proyecto Final, ULACIT). Todo el entorno corre con Docker Compose:

| Servicio | Tecnología | Dirección |
|----------|-----------|-----------|
| `db` | PostgreSQL 18 | `localhost:5433` (usuario `recetas`, base `recetas_db`) |
| `recetas-api` | ASP.NET Core 10 + EF Core 10 + Npgsql (carpeta `recetas-api/`) | http://localhost:5000/api/health |
| `recetas-spa` | React 19 + Vite (carpeta `recetas-spa/`; servido por nginx, que reenvía `/api` a la API) | http://localhost:8080 |

## Requisitos
Docker Desktop (o Docker Engine con Compose). Para desarrollar fuera de Docker: SDK de .NET 10 y Node 22.

## Levantar todo

```powershell
docker compose up -d --build
```

La primera vez, la base se crea y se cargan `db/01_schema.sql`, `02_seed_catalogos.sql` y `03_seed_demo.sql`. Después abrí http://localhost:8080/estado: debe mostrar PostgreSQL 18, 23 tablas, 12 recetas y 43 usuarios.

```powershell
docker compose ps                  # estado de los servicios
docker compose logs -f recetas-api # registros de la API
docker compose down                # detiene (conserva los datos)
docker compose down -v             # detiene y BORRA la base; la próxima vez se recarga desde cero
```

## Usuarios de prueba
| Correo | Contraseña | Rol |
|--------|-----------|-----|
| `admin@recetas.com` | `Admin123*` | ADMIN |
| `maria@correo.com` | `Maria123*` | USUARIO |
| `jose@correo.com` | `Jose123*` | USUARIO |
| `usuario1@demo.com` … `usuario40@demo.com` | `Usuario123*` | USUARIO |

Son credenciales de desarrollo. Los valores por defecto de la base y del JWT están en `docker-compose.yml`; se pueden cambiar con un archivo `.env` (ver `.env.example`).

## Base de datos
- Fuente de verdad: los scripts de `db/` (no hay migraciones de EF Core).
- `db/05_seed_themealdb_es.sql`: **40 recetas de TheMealDB traducidas al español** (datos en `db/data/themealdb_es.json`; se regenera con `node db/data/generar_seed.mjs`). Se cargan con un script, no con un endpoint de la API:
  `docker compose exec db psql -U recetas -d recetas_db -f /seeds/05_seed_themealdb_es.sql`
  No se carga solo porque cambia las cifras de los reportes de demostración.
- `db/04_seed_pruebas.sql` es opcional (recetas para probar casos de falla):
  `docker compose exec db psql -U recetas -d recetas_db -f /seeds/04_seed_pruebas.sql`
- Consultar a mano: `docker compose exec db psql -U recetas -d recetas_db`, o DBeaver en `localhost:5433`.
- Ejemplo de reporte: `SELECT * FROM sp_reporte_mas_vistas('2026-05-01','2026-10-31',5);`

## Pruebas
```powershell
cd recetas-api; dotnet test recetas-api.Tests   # necesita Docker: levanta un PostgreSQL 18 desechable
```
Documentación interactiva del API: http://localhost:5000/scalar. Ejemplos de cada endpoint: `recetas-api/http/`.

## Desarrollo sin reconstruir contenedores
```powershell
docker compose up -d db          # solo la base
cd recetas-api; dotnet run               # API en el puerto que indique launchSettings
cd recetas-spa; npm install; npm run dev # http://localhost:5173 (el proxy envía /api a localhost:5000)
```

## Entregables (carpeta `entregables/`)
- `diagramas/`: diagrama entidad-relación (`diagrama_er_recetas_db_v2`, 23 tablas), diagrama de flujo (`diagrama_flujo_v2`) y casos de uso (`Diagrama-CasosDeUso_v2`), en `.png` y `.svg`.

En la raíz quedan los insumos originales: `G3_Programacion_Web_Avance_1.pdf` y `Diagrama-CasosDeUso.png`.
