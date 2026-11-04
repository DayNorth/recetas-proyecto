-- =====================================================================
--  Genera 12 recetas, 40 usuarios demo y 6 meses de actividad
--  (mayo a octubre de 2026) con una "historia" para que los reportes
--  sirvan para tomar decisiones. Usa setseed(): siempre da lo mismo.
--
--  Historia incluida:
--    * Comida típica: 2 recetas y muchas visitas  -> demanda > oferta.
--    * Postres: 3 recetas con pocas visitas cada una -> oferta sobrada.
--    * "Arroz con pollo": muchas visitas, pocos me gusta y notas bajas
--      -> atrae pero no convence (revisar la receta).
--    * Búsquedas sin resultado recurrentes: arroz con leche, sopa negra,
--      chifrijo, tres leches -> recetas por crear.
--    * El uso crece de mayo a septiembre y cae en octubre.
--    * WhatsApp es la red por la que más se comparte.
--    * Una visita el 30-sep a las 20:00 (hora CR) = 1-oct en UTC: prueba
--      de que el corte de mes usa la zona horaria de Costa Rica.
--
--  Usuarios demo: usuario1@demo.com ... usuario40@demo.com / Usuario123*
-- =====================================================================

SET timezone = 'America/Costa_Rica';

-- ---------------------------------------------------------------------
-- Limpieza de datos de demostración (los catálogos y los 3 usuarios base
-- de 02_seed_catalogos.sql se conservan)
-- ---------------------------------------------------------------------
TRUNCATE TABLE moderaciones, reportes_contenido, visitas_receta, busquedas, me_gusta, favoritos, comentarios, compartidos,
               lista_compra_items, listas_compras, receta_multimedia, receta_pasos,
               receta_ingredientes, recetas, ingredientes, noticias
    RESTART IDENTITY;
DELETE FROM usuarios WHERE correo LIKE '%@demo.com';

-- ---------------------------------------------------------------------
-- Usuarios demo
-- ---------------------------------------------------------------------
INSERT INTO usuarios (rol_id, nombre, apellidos, correo, password_hash, fecha_registro)
SELECT 2, 'Usuario' || n, 'Demo', 'usuario' || n || '@demo.com',
       '$2b$10$BTsg7bkOlFk1ccdIQyOqz.ZchYIUDqvSy7pY6oabvQT1oQ6lWD7PW',
       timestamptz '2026-04-20 10:00'
FROM generate_series(1, 40) n;

-- ---------------------------------------------------------------------
-- Ingredientes (ids 1..30 en este orden)
-- ---------------------------------------------------------------------
INSERT INTO ingredientes (nombre) VALUES
('Arroz'),('Frijoles negros'),('Cebolla'),('Chile dulce'),('Culantro'),
('Salsa Lizano'),('Huevo'),('Harina'),('Azúcar'),('Leche'),('Mantequilla'),('Banano'),
('Pollo'),('Papa'),('Tomate'),('Aguacate'),('Limón'),('Chocolate'),('Cacao en polvo'),
('Vainilla'),('Coco rallado'),('Aceite'),('Sal'),('Ajo'),('Zanahoria'),('Lechuga'),
('Plátano maduro'),('Repollo'),('Polvo de hornear'),('Agua');

-- ---------------------------------------------------------------------
-- Recetas (ids 1..12). Categorías: 1 Desayunos, 2 Platos fuertes,
-- 3 Postres, 4 Bebidas, 5 Vegetariano, 6 Comida típica
-- ---------------------------------------------------------------------
INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion, tiempo_prep_min, porciones_base) VALUES
(1, 6, 1, 'Gallo pinto',               'Desayuno típico costarricense de arroz y frijoles.', 20, 4),
(1, 6, 2, 'Casado',                    'Plato completo con arroz, frijoles, pollo, plátano y ensalada.', 50, 4),
(2, 3, 2, 'Queque de banano',          'Queque esponjoso con banano maduro.', 60, 8),
(2, 3, 2, 'Flan de coco',              'Postre cremoso de huevo, leche y coco.', 75, 6),
(1, 3, 3, 'Brownies',                  'Brownies de chocolate con centro húmedo.', 45, 9),
(1, 1, 1, 'Panqueques',                'Panqueques suaves para el desayuno.', 25, 4),
(2, 1, 1, 'Huevos revueltos con tomate','Desayuno rápido y proteico.', 10, 2),
(1, 2, 2, 'Arroz con pollo',           'Arroz sazonado con pollo y vegetales.', 60, 6),
(2, 2, 2, 'Pollo al horno con papas',  'Pollo dorado con papas y ajo.', 80, 4),
(1, 4, 1, 'Limonada',                  'Limonada fría y refrescante.', 10, 6),
(2, 5, 1, 'Ensalada de aguacate',      'Ensalada fresca con aguacate y tomate.', 15, 2),
(1, 5, 1, 'Picadillo de papa',         'Picadillo tradicional de papa sin carne.', 35, 4);

INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota) VALUES
(1, 1, 5, 2,    'cocido del día anterior'),
(1, 2, 5, 1.5,  'cocidos con su caldo'),
(1, 3, 8, 1,    'picada fina'),
(1, 4, 8, 1,    'picado'),
(1, 5, 9, 1,    NULL),
(1, 6, 6, 2,    NULL),
(2, 1, 5, 2,    NULL),
(2, 2, 5, 1.5,  NULL),
(2, 13, 1, 600, 'en filetes'),
(2, 27, 8, 2,   'maduros, fritos'),
(2, 28, 1, 200, 'picado'),
(2, 15, 8, 2,   'en rodajas'),
(2, 23, 9, 1,   NULL),
(3, 8, 5, 2,    NULL),
(3, 9, 5, 1,    NULL),
(3, 7, 8, 3,    NULL),
(3, 11, 1, 100, 'derretida'),
(3, 12, 8, 3,   'maduros y majados'),
(3, 29, 7, 2,   NULL),
(4, 7, 8, 5,    NULL),
(4, 10, 5, 2,   NULL),
(4, 9, 5, 1,    NULL),
(4, 21, 5, 1,   NULL),
(4, 20, 7, 1,   NULL),
(5, 18, 1, 200, 'picado'),
(5, 11, 1, 150, NULL),
(5, 9, 5, 1,    NULL),
(5, 7, 8, 3,    NULL),
(5, 8, 5, 1,    NULL),
(5, 19, 6, 2,   NULL),
(6, 8, 5, 1.5,  NULL),
(6, 10, 5, 1.25,NULL),
(6, 7, 8, 1,    NULL),
(6, 9, 6, 2,    NULL),
(6, 11, 1, 30,  'derretida'),
(6, 29, 7, 2,   NULL),
(7, 7, 8, 4,    NULL),
(7, 15, 8, 2,   'picados'),
(7, 3, 8, 1,    'picada'),
(7, 22, 6, 1,   NULL),
(7, 23, 9, 1,   NULL),
(8, 1, 5, 3,    NULL),
(8, 13, 1, 800, 'en trozos'),
(8, 25, 8, 2,   'en cubos'),
(8, 4, 8, 1,    NULL),
(8, 3, 8, 1,    NULL),
(8, 24, 8, 3,   'dientes'),
(8, 23, 9, 1,   NULL),
(9, 13, 1, 1000,'en piezas'),
(9, 14, 8, 4,   'en cuartos'),
(9, 24, 8, 4,   NULL),
(9, 17, 8, 2,   'solo el jugo'),
(9, 22, 6, 3,   NULL),
(9, 23, 9, 1,   NULL),
(10, 17, 8, 8,  'exprimidos'),
(10, 9, 5, 0.5, NULL),
(10, 30, 4, 1.5,NULL),
(11, 16, 8, 2,  NULL),
(11, 26, 8, 1,  NULL),
(11, 15, 8, 2,  NULL),
(11, 17, 8, 1,  NULL),
(11, 22, 6, 2,  NULL),
(11, 23, 9, 1,  NULL),
(12, 14, 8, 5,  'en cubos'),
(12, 3, 8, 1,   NULL),
(12, 4, 8, 1,   NULL),
(12, 5, 9, 1,   NULL),
(12, 24, 8, 2,  NULL),
(12, 22, 6, 2,  NULL),
(12, 23, 9, 1,  NULL);

INSERT INTO receta_pasos (receta_id, numero_paso, descripcion) VALUES
(1,1,'Sofreír la cebolla y el chile dulce en un poco de aceite.'),
(1,2,'Agregar los frijoles con un poco de caldo y la salsa Lizano.'),
(1,3,'Incorporar el arroz y mezclar hasta integrar. Terminar con culantro.'),
(2,1,'Cocinar el arroz y los frijoles por separado.'),
(2,2,'Sazonar el pollo y dorarlo en una sartén; freír el plátano maduro.'),
(2,3,'Servir todo junto con el repollo y el tomate en ensalada.'),
(3,1,'Precalentar el horno a 180 °C y engrasar el molde.'),
(3,2,'Batir mantequilla, azúcar y huevos; agregar el banano majado.'),
(3,3,'Incorporar la harina con el polvo de hornear y hornear 45 minutos.'),
(4,1,'Licuar los huevos, la leche, el azúcar, el coco y la vainilla.'),
(4,2,'Verter en un molde caramelizado y cocinar a baño maría.'),
(4,3,'Enfriar al menos 4 horas antes de desmoldar.'),
(5,1,'Derretir el chocolate con la mantequilla.'),
(5,2,'Mezclar con azúcar, huevos, harina y cacao.'),
(5,3,'Hornear 25 minutos a 180 °C y dejar enfriar.'),
(6,1,'Mezclar los ingredientes secos y agregar la leche, el huevo y la mantequilla.'),
(6,2,'Cocinar porciones de masa en una sartén caliente.'),
(6,3,'Voltear cuando salgan burbujas y servir caliente.'),
(7,1,'Sofreír la cebolla y el tomate en el aceite.'),
(7,2,'Agregar los huevos batidos con sal.'),
(7,3,'Revolver a fuego bajo hasta que cuajen.'),
(8,1,'Dorar el pollo con ajo, cebolla y chile dulce.'),
(8,2,'Agregar el arroz, la zanahoria y agua caliente.'),
(8,3,'Cocinar tapado hasta que el arroz esté en su punto.'),
(9,1,'Marinar el pollo con ajo, limón, aceite y sal.'),
(9,2,'Colocar el pollo y las papas en una bandeja.'),
(9,3,'Hornear 60 minutos a 200 °C.'),
(10,1,'Exprimir los limones y colar el jugo.'),
(10,2,'Mezclar con el azúcar y el agua.'),
(10,3,'Servir con hielo.'),
(11,1,'Cortar el aguacate, el tomate y la lechuga.'),
(11,2,'Mezclar con el jugo de limón, aceite y sal.'),
(11,3,'Servir de inmediato.'),
(12,1,'Cocinar la papa en cubos hasta que esté suave.'),
(12,2,'Sofreír cebolla, chile dulce y ajo.'),
(12,3,'Mezclar con la papa y terminar con culantro.');

-- Foto de portada de cada receta (ruta relativa; la subida real de archivos es del Avance 3)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'uploads/recetas/' || LOWER(REPLACE(titulo, ' ', '-')) || '.jpg', titulo, TRUE
FROM recetas;

INSERT INTO noticias (usuario_id, titulo, contenido, fecha_publicacion) VALUES
(1,'5 trucos para un arroz perfecto','Lavar el arroz antes de cocinarlo ayuda a eliminar el exceso de almidón y evita que se pegue.','2026-06-10 09:00'),
(1,'Cómo escoger bananos para el queque','Los bananos muy maduros aportan más dulzor y humedad a la masa.','2026-08-05 09:00'),
(1,'Guía rápida de equivalencias','Una taza equivale a unos 240 ml; una cucharada, a 15 ml; una cucharadita, a 5 ml.','2026-10-01 09:00');

-- ---------------------------------------------------------------------
-- Actividad sintética (mayo a octubre de 2026)
-- ---------------------------------------------------------------------
DO $$
DECLARE
    uids   INT[];
    pool_r INT[];
    pool_t INT[];
BEGIN
    PERFORM setseed(0.42);
    SELECT array_agg(usuario_id ORDER BY usuario_id) INTO uids FROM usuarios WHERE rol_id = 2;

    -- peso = cuánto tráfico directo recibe; atractivo = prob. de que un usuario le dé me gusta;
    -- calif = calificación media esperada
    CREATE TEMP TABLE tmp_params (receta_id INT, peso INT, atractivo DOUBLE PRECISION, calif DOUBLE PRECISION) ON COMMIT DROP;
    INSERT INTO tmp_params VALUES
    (1,12,0.80,4.8),(2,10,0.60,4.5),(3,8,0.65,4.3),(4,3,0.35,4.0),(5,5,0.50,4.6),(6,4,0.30,4.2),
    (7,3,0.20,3.8),(8,14,0.08,2.6),(9,6,0.35,4.0),(10,3,0.30,4.4),(11,3,0.25,4.1),(12,4,0.20,3.9);

    CREATE TEMP TABLE tmp_terminos (id INT, termino VARCHAR, categoria_id INT, resultados INT, receta_id INT, peso INT) ON COMMIT DROP;
    INSERT INTO tmp_terminos VALUES
    (1,'gallo pinto',6,1,1,10),(2,'casado',6,1,2,8),(3,'queque',3,1,3,6),(4,'brownies',3,1,5,4),
    (5,'flan',3,1,4,3),(6,'panqueques',1,1,6,4),(7,'huevos revueltos',1,1,7,2),(8,'arroz con pollo',2,1,8,12),
    (9,'pollo',2,2,9,6),(10,'limonada',4,1,10,3),(11,'ensalada',5,1,11,3),(12,'picadillo',5,1,12,3),
    (13,'arroz con leche',3,0,NULL,6),(14,'sopa negra',6,0,NULL,5),(15,'chifrijo',6,0,NULL,4),
    (16,'tres leches',3,0,NULL,2);

    -- visitas directas y búsquedas por mes (crece hasta septiembre y cae en octubre)
    CREATE TEMP TABLE tmp_meses (mes DATE, directas INT, busquedas INT) ON COMMIT DROP;
    INSERT INTO tmp_meses VALUES
    ('2026-05-01',25,30),('2026-06-01',35,40),('2026-07-01',45,55),
    ('2026-08-01',60,75),('2026-09-01',75,95),('2026-10-01',40,60);

    SELECT array_agg(p.receta_id ORDER BY p.receta_id) INTO pool_r
      FROM tmp_params p CROSS JOIN LATERAL generate_series(1, p.peso);
    SELECT array_agg(t.id ORDER BY t.id) INTO pool_t
      FROM tmp_terminos t CROSS JOIN LATERAL generate_series(1, t.peso);

    -- Búsquedas
    WITH sel AS MATERIALIZED (
        SELECT m.mes,
               pool_t[1 + floor(random() * cardinality(pool_t))::int] AS tid,
               random() AS r_user, random() AS r_cat, random() AS r_fecha,
               uids[1 + floor(random() * cardinality(uids))::int] AS uid
        FROM tmp_meses m CROSS JOIN LATERAL generate_series(1, m.busquedas) g
    )
    INSERT INTO busquedas (usuario_id, termino, categoria_id, cantidad_resultados, fecha)
    SELECT CASE WHEN s.r_user < 0.4 THEN NULL ELSE s.uid END,
           t.termino,
           CASE WHEN s.r_cat < 0.6 THEN t.categoria_id END,
           t.resultados,
           (s.mes::timestamp + s.r_fecha * interval '27 days 15 hours' + interval '6 hours')
               AT TIME ZONE 'America/Costa_Rica'
    FROM sel s JOIN tmp_terminos t ON t.id = s.tid
    ORDER BY 5;

    -- Visitas que nacen de una búsqueda con resultados (80 %): llenan busqueda_id
    INSERT INTO visitas_receta (receta_id, usuario_id, busqueda_id, fecha)
    SELECT t.receta_id, b.usuario_id, b.busqueda_id, b.fecha + interval '1 minute'
    FROM busquedas b JOIN tmp_terminos t ON t.termino = b.termino
    WHERE t.receta_id IS NOT NULL AND random() < 0.8;

    -- Visitas directas (portada, enlaces, redes)
    WITH sel AS MATERIALIZED (
        SELECT m.mes,
               pool_r[1 + floor(random() * cardinality(pool_r))::int] AS rid,
               random() AS r_user, random() AS r_fecha,
               uids[1 + floor(random() * cardinality(uids))::int] AS uid
        FROM tmp_meses m CROSS JOIN LATERAL generate_series(1, m.directas) g
    )
    INSERT INTO visitas_receta (receta_id, usuario_id, fecha)
    SELECT rid,
           CASE WHEN r_user < 0.4 THEN NULL ELSE uid END,
           (mes::timestamp + r_fecha * interval '27 days 15 hours' + interval '6 hours')
               AT TIME ZONE 'America/Costa_Rica'
    FROM sel;

    -- Me gusta, favoritos y calificaciones (más probables en las recetas "atractivas").
    -- Los azares se generan UNA vez por par usuario-receta en una tabla temporal:
    -- si random() va en un WHERE que solo mira la receta, Postgres lo evalúa una vez
    -- por receta y el resultado sale "todos o ninguno".
    CREATE TEMP TABLE tmp_pares ON COMMIT DROP AS
    SELECT u AS usuario_id, p.receta_id, p.atractivo, p.calif,
           random() AS r_like, random() AS r_fav, random() AS r_com, random() AS r_nota, random() AS r_txt,
           random() AS f_like, random() AS f_fav, random() AS f_com
    FROM unnest(uids) u CROSS JOIN tmp_params p;

    INSERT INTO me_gusta (usuario_id, receta_id, fecha)
    SELECT usuario_id, receta_id,
           (timestamp '2026-05-01' + interval '183 days 23 hours' * power(f_like, 0.6))
               AT TIME ZONE 'America/Costa_Rica'
    FROM tmp_pares
    WHERE r_like < atractivo;

    INSERT INTO favoritos (usuario_id, receta_id, fecha)
    SELECT usuario_id, receta_id,
           (timestamp '2026-05-01' + interval '183 days 23 hours' * power(f_fav, 0.6))
               AT TIME ZONE 'America/Costa_Rica'
    FROM tmp_pares
    WHERE r_fav < atractivo * 0.5;

    INSERT INTO comentarios (usuario_id, receta_id, calificacion, comentario, fecha)
    SELECT usuario_id, receta_id,
           GREATEST(1, LEAST(5, round(calif + (r_nota * 2 - 1))::int)),
           (ARRAY['Quedó bien','Fácil de preparar','Lo volvería a hacer','Le faltó sabor',NULL,NULL])
               [1 + floor(r_txt * 6)::int],
           (timestamp '2026-05-01' + interval '183 days 23 hours' * power(f_com, 0.6))
               AT TIME ZONE 'America/Costa_Rica'
    FROM tmp_pares
    WHERE r_com < 0.1 + atractivo * 0.4;

    -- Compartidos: WhatsApp (2) domina; 1 Facebook, 3 X, 4 Instagram
    INSERT INTO compartidos (receta_id, usuario_id, red_id, fecha)
    SELECT pool_r[1 + floor(random() * cardinality(pool_r))::int],
           CASE WHEN random() < 0.3 THEN NULL ELSE uids[1 + floor(random() * cardinality(uids))::int] END,
           (ARRAY[2,2,2,2,1,1,4,3])[1 + floor(random() * 8)::int],
           (timestamp '2026-05-01' + interval '183 days 23 hours' * power(random(), 0.6))
               AT TIME ZONE 'America/Costa_Rica'
    FROM generate_series(1, 80);
END $$;

-- Visita de borde de mes: 30-sep 20:00 hora de Costa Rica (= 1-oct 02:00 UTC)
INSERT INTO visitas_receta (receta_id, usuario_id, fecha)
VALUES (1, NULL, '2026-09-30 20:00:00-06');

-- Moderación de ejemplo: 2 denuncias pendientes y 1 ya atendida (comentario oculto)
INSERT INTO reportes_contenido (usuario_id, receta_id, comentario_id, motivo, descripcion)
SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'usuario5@demo.com'), 8, NULL, 'INCORRECTO',
       'Las cantidades de arroz y pollo no corresponden a las porciones indicadas.';
INSERT INTO reportes_contenido (usuario_id, receta_id, comentario_id, motivo, descripcion)
SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'usuario6@demo.com'), NULL,
       (SELECT MIN(comentario_id) FROM comentarios WHERE receta_id = 8), 'OFENSIVO', NULL;

INSERT INTO comentarios (usuario_id, receta_id, calificacion, comentario, visible)
SELECT usuario_id, 2, 1, 'Mensaje de spam con enlaces externos', FALSE
FROM usuarios WHERE correo = 'usuario40@demo.com'
ON CONFLICT (usuario_id, receta_id) DO NOTHING;

INSERT INTO reportes_contenido (usuario_id, receta_id, comentario_id, motivo, estado, resolucion, atendido_por, fecha_atencion)
SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'usuario7@demo.com'), NULL, c.comentario_id, 'SPAM',
       'MODERADO', 'Comentario con enlaces publicitarios; se oculta.',
       (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'), now()
FROM comentarios c WHERE c.visible = FALSE LIMIT 1;

INSERT INTO moderaciones (admin_id, comentario_id, reporte_contenido_id, accion, motivo)
SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'), rc.comentario_id,
       rc.reporte_contenido_id, 'RETIRAR', 'Spam con enlaces externos'
FROM reportes_contenido rc WHERE rc.estado = 'MODERADO';

-- Comentario SIN calificación (la calificación es opcional)
INSERT INTO comentarios (usuario_id, receta_id, calificacion, comentario)
SELECT usuario_id, 1, NULL, 'Yo le agrego un poco de natilla al servir.'
FROM usuarios WHERE correo = 'usuario39@demo.com'
ON CONFLICT (usuario_id, receta_id) DO NOTHING;

-- Una lista de compras de ejemplo (la que generaría la transacción de la API)
INSERT INTO listas_compras (usuario_id, nombre)
SELECT usuario_id, 'Para el domingo' FROM usuarios WHERE correo = 'maria@correo.com';

INSERT INTO lista_compra_items (lista_id, ingrediente_id, unidad_id, cantidad, receta_origen_id)
SELECT (SELECT MAX(lista_id) FROM listas_compras), f.ingrediente_id, f.unidad_id, f.cantidad, 1
FROM sp_ingredientes_por_porciones(1, 8) f;

ANALYZE;
