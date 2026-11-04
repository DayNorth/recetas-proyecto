// Genera ../05_seed_themealdb_es.sql a partir de themealdb_es.json (sin dependencias).
// Uso:  node generar_seed.mjs
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const aqui = path.dirname(fileURLToPath(import.meta.url));
const recetas = JSON.parse(fs.readFileSync(path.join(aqui, 'themealdb_es.json'), 'utf8'));
const q = (s) => "'" + String(s).replace(/'/g, "''") + "'";
const num = (n) => String(n);

const ingredientes = [...new Map(recetas.flatMap((r) => r.ingredientes.map((i) => [i.nombre.toLowerCase(), i.nombre]))).values()].sort((a, b) => a.localeCompare(b, 'es'));

let sql = `-- =====================================================================
--  RECETAS IMPORTADAS DE TheMealDB, TRADUCIDAS AL ESPAÑOL   (RF-33)
--  ${recetas.length} recetas con origen = 'API' y api_referencia_id = idMeal.
--
--  Archivo OPCIONAL: ejecutar después de 01, 02 y 03. No se carga solo con
--  docker compose para no alterar la historia de los datos de demostración
--  (visitas por receta y oferta/demanda de cada categoría). Para cargarlo:
--    docker compose exec db psql -U recetas -d recetas_db -f /seeds/05_seed_themealdb_es.sql
--
--  Se puede ejecutar varias veces: una receta ya importada (mismo
--  api_referencia_id) se omite. Autor: admin@recetas.com.
--  Las imágenes y los videos son enlaces a TheMealDB y YouTube (no se copian).
--
--  GENERADO por db/data/generar_seed.mjs desde db/data/themealdb_es.json:
--  edita el JSON y vuelve a generar, no este archivo.
--  Fuente de los datos: https://www.themealdb.com (traducción y adaptación propias).
-- =====================================================================

SET timezone = 'America/Costa_Rica';

-- Ingredientes nuevos del catálogo (los que ya existen se omiten, sin distinguir mayúsculas)
INSERT INTO ingredientes (nombre) VALUES
${ingredientes.map((n) => '(' + q(n) + ')').join(',\n')}
ON CONFLICT DO NOTHING;
`;

for (const r of recetas) {
  const ing = r.ingredientes.map((i) => `(${q(i.nombre)}, ${q(i.unidad)}, ${num(i.cantidad)}, ${i.nota ? q(i.nota) : 'NULL'})`).join(',\n         ');
  const pasos = r.pasos.map((p, k) => `(${k + 1}, ${q(p)})`).join(',\n         ');
  sql += `
-- ${r.idMeal} · ${r.tituloOriginal}
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = ${q(r.categoria)}),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = ${q(r.dificultad)}),
           ${q(r.titulo)},
           ${q(r.descripcion)},
           ${r.tiempoPrepMin}, ${r.porcionesBase}, 'API', ${q(r.idMeal)}, 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = ${q(r.idMeal)})
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ${ing}
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         ${pasos}
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', ${q(r.imagen)}, ${q(r.titulo)}, TRUE FROM nueva${r.video ? `
UNION ALL
SELECT receta_id, 'VIDEO', ${q(r.video)}, 'Video de preparación', FALSE FROM nueva` : ''};
`;
}

sql += `
-- Verificación: debe devolver ${recetas.length}
-- SELECT count(*) FROM recetas WHERE origen = 'API';
`;
fs.writeFileSync(path.join(aqui, '..', '05_seed_themealdb_es.sql'), sql, 'utf8');
console.log(`05_seed_themealdb_es.sql: ${recetas.length} recetas, ${ingredientes.length} ingredientes, ${(sql.length / 1024).toFixed(0)} KB`);
