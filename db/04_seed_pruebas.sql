-- =====================================================================
--  Archivo OPCIONAL: recetas para probar los caminos de falla de la
--  transacción "generar lista de compras" (ROLLBACK).
--  Ejecutar después de 03_seed_demo.sql. Para quitarlas:
--    DELETE FROM recetas WHERE titulo LIKE '[PRUEBA]%';
--
--  Casos (POST /listas con el receta_id que corresponda):
--    '[PRUEBA] Receta inactiva'          -> 0 filas por estado INACTIVA  -> 404 + ROLLBACK
--    '[PRUEBA] Receta sin ingredientes'  -> 0 filas                      -> 409 + ROLLBACK
--    '[PRUEBA] Cantidad mínima' con porciones = 1:
--        0.01 * 1 / 100 = 0.0001 -> redondea a 0.00 -> viola
--        chk_item_cantidad en la mitad del lote -> ROLLBACK: la lista y los
--        ítems ya insertados desaparecen.
--
--  Verificación posterior (no debe haber listas huérfanas ni vacías):
--    SELECT l.lista_id FROM listas_compras l
--    LEFT JOIN lista_compra_items i USING (lista_id) WHERE i.item_id IS NULL;
-- =====================================================================

SET timezone = 'America/Costa_Rica';

INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion, tiempo_prep_min, porciones_base, estado) VALUES
(1, 2, 1, '[PRUEBA] Receta inactiva',         'Para probar el rechazo de recetas inactivas.', 10, 4,   'INACTIVA'),
(1, 2, 1, '[PRUEBA] Receta sin ingredientes', 'Para probar una receta publicada sin ingredientes.', 10, 4, 'PUBLICADA'),
(1, 2, 1, '[PRUEBA] Cantidad mínima',         'Para probar el ROLLBACK por cantidad redondeada a 0.00.', 10, 100, 'PUBLICADA');

INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad)
SELECT r.receta_id, i.ingrediente_id, u.unidad_id, c.cant
FROM (VALUES ('[PRUEBA] Receta inactiva', 'Sal', 'g', 5.00),
             ('[PRUEBA] Cantidad mínima', 'Arroz', 'tz', 2.00),    -- se inserta bien
             ('[PRUEBA] Cantidad mínima', 'Sal',   'g',  0.01)     -- 0.01*1/100 -> 0.00 -> falla
     ) AS c(titulo, ingrediente, unidad, cant)
JOIN recetas r          ON r.titulo = c.titulo
JOIN ingredientes i     ON i.nombre = c.ingrediente
JOIN unidades_medida u  ON u.abreviatura = c.unidad;
