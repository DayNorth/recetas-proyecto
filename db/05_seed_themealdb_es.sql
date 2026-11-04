-- =====================================================================
--  RECETAS DE TheMealDB, TRADUCIDAS AL ESPAÑOL
--  40 recetas con origen = 'API' y api_referencia_id = idMeal.
--
--  Se ejecuta después de 01, 02 y 03. docker compose lo carga solo la primera
--  vez que se crea la base; para cargarlo a mano en una base existente:
--    docker compose exec db psql -U recetas -d recetas_db -f /seeds/05_seed_themealdb_es.sql
--  Fuente de los datos: https://www.themealdb.com.
-- =====================================================================

SET timezone = 'America/Costa_Rica';

-- Ingredientes nuevos del catálogo (los que ya existen se omiten, sin distinguir mayúsculas)
INSERT INTO ingredientes (nombre) VALUES
('Aceite'),
('Aceite de girasol'),
('Aceite de oliva'),
('Aceite de oliva extra virgen'),
('Aceite vegetal'),
('Aceitunas rellenas de pimiento'),
('Aceitunas verdes'),
('Agua'),
('Ajo'),
('Albahaca'),
('Almíbar'),
('Alubias'),
('Arroz'),
('Arroz para paella'),
('Avena molida'),
('Azafrán'),
('Azúcar'),
('Azúcar morena'),
('Baguette'),
('Banano'),
('Berenjena'),
('Bicarbonato de sodio'),
('Calabaza'),
('Calamar'),
('Caldo de mariscos'),
('Caldo de pollo'),
('Caldo de res'),
('Camarones tigre'),
('Canela'),
('Canela en rama'),
('Carne de res'),
('Carne molida'),
('Cebolla'),
('Cebollino'),
('Champiñones'),
('Chile banana'),
('Chile dulce'),
('Chile dulce amarillo'),
('Chile dulce rojo'),
('Chile en polvo'),
('Chile habanero'),
('Chile picante'),
('Chile rojo en polvo'),
('Chile verde'),
('Chiles dulces variados'),
('Chocolate amargo'),
('Chorizo'),
('Chuletas de cerdo'),
('Cilantro molido'),
('Coco'),
('Coco rallado'),
('Comino'),
('Crema para batir'),
('Culantro'),
('Cúrcuma'),
('Dulce de leche'),
('Espagueti'),
('Falda de res'),
('Fettuccine'),
('Filete de res'),
('Frambuesas'),
('Fresas'),
('Frijoles blancos'),
('Frijoles horneados'),
('Frijoles lima'),
('Frijoles negros'),
('Frijoles rojos'),
('Garam masala'),
('Garbanzos'),
('Ghee'),
('Habas'),
('Harina'),
('Hoja de laurel'),
('Huevo'),
('Jarrete de res'),
('Jengibre'),
('Jerez seco'),
('Jugo de limón'),
('Jugo de naranja'),
('Langostinos'),
('Leche'),
('Leche agria'),
('Leche condensada'),
('Leche de coco'),
('Leche entera'),
('Lenteja toor dal'),
('Limón'),
('Maicena'),
('Maíz dulce'),
('Maní tostado'),
('Mantequilla'),
('Manzana'),
('Mejillones'),
('Miel'),
('Morcilla'),
('Mostaza'),
('Muslos de pollo'),
('Naranja'),
('Nueces'),
('Nueces pecanas'),
('Ñame'),
('Orégano'),
('Pan'),
('Pan molido'),
('Pan rústico'),
('Papa'),
('Papaya verde'),
('Paprika'),
('Paprika ahumada'),
('Pasas'),
('Pasta de achiote'),
('Perejil'),
('Piernas de pollo'),
('Pimienta'),
('Pimienta gorda'),
('Pimienta negra'),
('Pollo'),
('Polvo de hornear'),
('Puerro'),
('Puré de tomate'),
('Queso crema'),
('Queso parmesano'),
('Queso pecorino'),
('Ralladura de limón'),
('Sal'),
('Salchichas'),
('Salsa chimichurri'),
('Salsa de aliños'),
('Salsa inglesa'),
('Semillas de comino'),
('Semillas de mostaza'),
('Sirope de arce'),
('Sirope dorado'),
('Tocino'),
('Tomate'),
('Tomate cherry'),
('Tomate en lata'),
('Tomate en trozos'),
('Tomillo'),
('Vaina de vainilla'),
('Vainicas'),
('Vainilla'),
('Vinagre de vino tinto'),
('Vino blanco'),
('Vino blanco seco'),
('Yautía'),
('Yemas de huevo'),
('Yuca'),
('Zanahoria'),
('Zucchini')
ON CONFLICT DO NOTHING;

-- 53415 · Torrijas
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Desayunos'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Torrijas',
           'Rebanadas de pan remojadas en leche aromatizada con cítricos y canela, fritas y servidas con miel. Fuente: TheMealDB.',
           40, 6, 'API', '53415', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53415')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Naranja', 'u', 1, 'mediano'),
         ('Limón', 'u', 1, 'mediano'),
         ('Leche entera', 'tz', 4, NULL),
         ('Azúcar', 'tz', 0.5, NULL),
         ('Canela en rama', 'u', 1, NULL),
         ('Jerez seco', 'tz', 0.33, NULL),
         ('Baguette', 'u', 1, NULL),
         ('Aceite de oliva', 'tz', 1, NULL),
         ('Huevo', 'u', 4, 'grande'),
         ('Miel', 'ag', 1, 'para servir')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Ralla finamente la cáscara de 1 naranja mediana (unas 2 cucharaditas) y de 1 limón mediano (1 cucharadita) dentro de una olla mediana.'),
         (2, 'Agrega 1 litro de leche entera, 1/2 taza de azúcar y 1 rama de canela. Lleva a ebullición a fuego medio. Retira del fuego, agrega 1/3 de taza de jerez seco y deja entibiar. Mientras tanto, corta 1 baguette del día anterior (unos 340 g) en rebanadas de 2,5 cm de grosor (unas 16) y acomódalas en una sola capa sobre una bandeja con borde.'),
         (3, 'Vierte la mezcla de leche sobre el pan y déjalo remojar 5 minutos. Voltea cada rebanada y deja reposar unos 5 minutos más, hasta que absorba casi todo el líquido sin deshacerse.'),
         (4, 'Calienta 1 taza de aceite de oliva en una sartén grande a fuego medio-alto hasta que brille. Mientras, bate los 4 huevos en un tazón mediano.'),
         (5, 'Pasa las rebanadas de una en una por el huevo y deja escurrir el exceso; fríelas en una sola capa hasta que estén doradas por debajo (unos 4 minutos). Voltea y fríe el otro lado de 3 a 4 minutos más. Escurre sobre papel absorbente y repite con el resto del pan. Sirve con un chorrito de miel.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/927vvp1779732018.jpg', 'Torrijas', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/ympLYopSKow', 'Video de preparación', FALSE FROM nueva;

-- 53331 · Oatmeal pancakes
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Desayunos'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Panqueques de avena',
           'Panqueques rápidos de avena molida, servidos con caramelo y fresas. Fuente: TheMealDB.',
           25, 2, 'API', '53331', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53331')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Huevo', 'u', 1, 'mediano'),
         ('Leche', 'ml', 80, NULL),
         ('Azúcar', 'cda', 1, NULL),
         ('Polvo de hornear', 'cdta', 1, NULL),
         ('Sal', 'ag', 1, 'una pizca'),
         ('Vainilla', 'ag', 1, 'pizca'),
         ('Avena molida', 'g', 150, NULL),
         ('Mantequilla', 'g', 150, NULL),
         ('Almíbar', 'cda', 1, NULL),
         ('Fresas', 'u', 3, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Coloca todos los ingredientes en el vaso de la licuadora y bate.'),
         (2, 'Deja reposar la mezcla 10 minutos.'),
         (3, 'Engrasa una sartén caliente con un poco de mantequilla y vierte una porción pequeña de la mezcla.'),
         (4, 'Cuando empiecen a salir burbujas en la superficie, voltea con una espátula. Cocina a fuego medio-bajo para que no se quemen.'),
         (5, 'Para terminar, agrega el caramelo y las fresas.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/c400ok1764439058.jpg', 'Panqueques de avena', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/kkrRFWN4LG4', 'Video de preparación', FALSE FROM nueva;

-- 53076 · Bread omelette
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Desayunos'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Pan con huevo',
           'Desayuno sencillo de pan dorado con huevo, listo en minutos. Fuente: TheMealDB.',
           10, 1, 'API', '53076', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53076')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Pan', 'u', 2, NULL),
         ('Huevo', 'u', 2, NULL),
         ('Sal', 'u', 0.5, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Bate los huevos con la sal en un plato hondo.'),
         (2, 'Pasa las rebanadas de pan por el huevo batido, de modo que se empapen bien.'),
         (3, 'Dora el pan en una sartén caliente con un poco de aceite, de ambos lados, y sirve de inmediato.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/hqaejl1695738653.jpg', 'Pan con huevo', TRUE FROM nueva;

-- 52965 · Breakfast Potatoes
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Desayunos'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Papas del desayuno con tocino',
           'Papas doradas con tocino crujiente, ajo y un toque de sirope de arce. Fuente: TheMealDB.',
           35, 3, 'API', '52965', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52965')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Papa', 'u', 3, 'mediano'),
         ('Aceite de oliva', 'cda', 1, NULL),
         ('Tocino', 'u', 2, 'tiras'),
         ('Ajo', 'ag', 1, 'picado fino'),
         ('Sirope de arce', 'cda', 1, NULL),
         ('Perejil', 'ag', 1, 'para decorar'),
         ('Sal', 'ag', 1, 'una pizca'),
         ('Pimienta', 'ag', 1, 'una pizca'),
         ('Pimienta gorda', 'ag', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Antes de empezar, congela las rebanadas de tocino: así será mucho más fácil picarlas.'),
         (2, 'Lava las papas y córtalas en cubos medianos. Para que no se oscurezcan, colócalas en un tazón con agua.'),
         (3, 'Mientras tanto, calienta 1 o 2 cucharadas de aceite en una sartén grande a fuego medio-alto e inclínala para repartirlo.'),
         (4, 'Cuando el aceite esté caliente, escurre las papas y agrégalas. Sazona con sal, pimienta y pimienta gorda al gusto.'),
         (5, 'Cocina 10 minutos, revolviendo a menudo, hasta que doren. Si hace falta, agrega una cucharada más de aceite.'),
         (6, 'Pica el tocino y añádelo a las papas. Soltará su grasa y terminará de cocinarlas. Revuelve de vez en cuando; el tocino tarda de 5 a 6 minutos en quedar crujiente.'),
         (7, 'Cuando el tocino esté listo, baja el fuego a medio-bajo, agrega el ajo picado y mezcla. Vuelve a sazonar y añade perejil seco o fresco.'),
         (8, 'Deja cocinar el ajo hasta que suelte aroma, aproximadamente un minuto.'),
         (9, 'Justo antes de servir, agrega el sirope de arce, revuelve y cocina un minuto más para caramelizar las papas.'),
         (10, 'Sirve en un plato caliente, acompañado de un huevo frito.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/1550441882.jpg', 'Papas del desayuno con tocino', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/BoD0TIO9nE4', 'Video de preparación', FALSE FROM nueva;

-- 52896 · Full English Breakfast
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Desayunos'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Desayuno inglés completo',
           'Salchichas, tocino, champiñones, tomate, morcilla, huevo, pan frito y frijoles horneados. Fuente: TheMealDB.',
           50, 2, 'API', '52896', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52896')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Salchichas', 'u', 4, NULL),
         ('Tocino', 'u', 4, NULL),
         ('Champiñones', 'u', 4, NULL),
         ('Tomate', 'u', 3, NULL),
         ('Morcilla', 'u', 2, 'en rodajas'),
         ('Huevo', 'u', 2, NULL),
         ('Pan', 'u', 1, 'rebanada'),
         ('Frijoles horneados', 'g', 100, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Calienta a fuego bajo una plancha o parrilla plana y úntala con muy poco aceite de oliva ligero.'),
         (2, 'Cocina primero las salchichas: ponlas en la parte menos caliente de la plancha y déjalas de 15 a 20 minutos, dándoles vuelta, hasta que doren. A los 10 minutos sube el fuego a medio para empezar con el resto. Si falta espacio, termina de cocinarlas y mantenlas calientes en el horno.'),
         (3, 'Haz unos cortes pequeños en el borde graso del tocino. Colócalo directamente en la plancha y fríelo de 2 a 4 minutos por lado, según lo prefieras de crujiente. Puedes mantenerlo caliente en el horno.'),
         (4, 'Limpia los champiñones con una brocha, corta el tallo al nivel del sombrero, sazona con sal y pimienta y rocía un poco de aceite. Cocínalos con el tallo hacia arriba 1 o 2 minutos, voltea y cocina de 3 a 4 minutos más. Muévelos poco para que no suelten sus jugos y no se ablanden.'),
         (5, 'Corta los tomates por la mitad, quítales el ojo verde, sazona y rocía un poco de aceite. Cocínalos con el corte hacia abajo 2 minutos sin moverlos; voltea con cuidado, sazona otra vez y cocina de 2 a 3 minutos más, hasta que estén tiernos pero conserven la forma.'),
         (6, 'Corta la morcilla en 3 o 4 rebanadas, quítale la piel y cocínala en la plancha de 1 1/2 a 2 minutos por lado, hasta que esté ligeramente crujiente.'),
         (7, 'El pan frito queda mejor en una sartén aparte y con pan de un par de días. Calienta la sartén a fuego medio, cubre el fondo con aceite y fríe el pan de 2 a 3 minutos por lado hasta dorar. Si se seca, agrega un poco más de aceite; para más sabor, añade un trozo de mantequilla al voltearlo.'),
         (8, 'Para los huevos, rómpelos directamente en la sartén del pan frito y déjalos 30 segundos. Agrega un buen trozo de mantequilla y báñalos con ella al derretirse. Cocínalos al punto que prefieras, sazona y retíralos con una espátula.'),
         (9, 'Cuando todo esté listo, sirve en platos calientes y disfruta de inmediato, con un buen chorro de salsa de tomate o salsa marrón. Calienta los frijoles horneados aparte y acompaña.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/sqrtwu1511721265.jpg', 'Desayuno inglés completo', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/FXjYU2Ensck', 'Video de preparación', FALSE FROM nueva;

-- 53522 · Arroz con Leche Recipe
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Arroz con leche',
           'Postre cremoso de arroz cocido en leche con canela, vainilla y leche condensada. Fuente: TheMealDB.',
           120, 8, 'API', '53522', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53522')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Arroz', 'tz', 1, NULL),
         ('Canela en rama', 'u', 4, NULL),
         ('Agua', 'tz', 2, NULL),
         ('Leche', 'tz', 4, NULL),
         ('Sal', 'ag', 1, 'una pizca'),
         ('Mantequilla', 'cda', 2, NULL),
         ('Vainilla', 'cda', 2, NULL),
         ('Azúcar', 'tz', 1, NULL),
         ('Leche condensada', 'g', 340, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'En una olla pequeña, coloca el agua y las ramas de canela, lleva a ebullición y cocina 10 minutos. Reserva y descarta la canela.'),
         (2, 'Coloca el arroz y el agua de canela en una olla grande a fuego medio y cocina 5 minutos.'),
         (3, 'Agrega la sal, la mantequilla, la vainilla, 2 tazas de leche y el azúcar. Mezcla bien y cocina sin tapar unos 15 minutos.'),
         (4, 'Baja el fuego a medio-bajo. Añade el resto de la leche y la leche condensada y revuelve con una cuchara de madera. Cocina 1 hora y 15 minutos, o hasta que el arroz espese a la consistencia deseada.'),
         (5, 'Revuelve bien, retira del fuego y deja enfriar a temperatura ambiente. Debe quedar muy cremoso. Refrigera al menos 1 hora o toda la noche.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/jp09191782856005.jpg', 'Arroz con leche', TRUE FROM nueva;

-- 53322 · Flan
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Flan uruguayo',
           'Flan de vainilla con caramelo, horneado a baño maría y servido con dulce de leche. Fuente: TheMealDB.',
           90, 4, 'API', '53322', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53322')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Azúcar', 'g', 145, NULL),
         ('Leche', 'g', 350, NULL),
         ('Vaina de vainilla', 'u', 1, NULL),
         ('Yemas de huevo', 'u', 4, NULL),
         ('Huevo', 'u', 2, NULL),
         ('Dulce de leche', 'ag', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Para el caramelo, pon 100 gramos de azúcar en una sartén sin grasa y derrítela a fuego medio. Procura no revolver con cuchara: mueve la sartén en círculos. Cuando tenga color ámbar y esté totalmente derretida, reparte el caramelo en el fondo de cuatro flaneras (unos 120 ml cada una). El caramelo endurece enseguida, así que hay que ser rápido.'),
         (2, 'Precalienta el horno a 150 °C y busca una bandeja honda o refractaria donde quepan las cuatro flaneras.'),
         (3, 'Calienta la leche con 45 gramos de azúcar y la vaina de vainilla abierta y raspada. No necesita hervir, solo calentarse. Saca la vaina después de unos minutos. Mientras, mezcla suavemente las yemas y los huevos sin incorporar aire. Vierte la leche sobre los huevos y vuelve a mezclar muy suavemente. Reparte en las flaneras y colócalas en la bandeja. Hierve agua y viértela en la bandeja, sin tocar la mezcla del flan. Hornea en la rejilla del medio de 35 a 40 minutos: un palillo insertado debe salir limpio.'),
         (4, 'Deja enfriar en las flaneras durante una hora y luego pasa al refrigerador toda la noche. Al día siguiente, suelta los bordes con cuidado y voltea sobre un plato. Sirve con dulce de leche al lado.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/0s80wo1764374393.jpg', 'Flan uruguayo', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/rpv_PXIoysE', 'Video de preparación', FALSE FROM nueva;

-- 53153 · Churros
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Churros con salsa de chocolate',
           'Churros fritos y espolvoreados con azúcar y canela, para mojar en salsa de chocolate. Fuente: TheMealDB.',
           50, 6, 'API', '53153', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53153')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Mantequilla', 'g', 50, NULL),
         ('Vainilla', 'cdta', 1, NULL),
         ('Harina', 'g', 250, NULL),
         ('Polvo de hornear', 'cdta', 1, NULL),
         ('Aceite de girasol', 'l', 1, NULL),
         ('Pan', 'u', 2, 'picado'),
         ('Chocolate amargo', 'g', 200, NULL),
         ('Crema para batir', 'ml', 100, NULL),
         ('Leche', 'ml', 100, NULL),
         ('Sirope dorado', 'cda', 3, NULL),
         ('Azúcar', 'g', 100, NULL),
         ('Canela', 'cdta', 2, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Hierve agua y mide 300 ml en una jarra; agrega la mantequilla derretida y la vainilla. Tamiza la harina y el polvo de hornear en un tazón grande con una buena pizca de sal. Haz un hueco en el centro, vierte el contenido de la jarra y bate muy rápido con una cuchara de madera hasta que no queden grumos. Deja reposar de 10 a 15 minutos mientras preparas la salsa.'),
         (2, 'Coloca todos los ingredientes de la salsa (chocolate, crema, leche y sirope dorado) en una olla y derrite a fuego suave, revolviendo de vez en cuando, hasta obtener una salsa lisa y brillante. Mantén caliente a fuego bajo.'),
         (3, 'Llena un tercio de una olla grande y honda con aceite. Calienta hasta que un cubo de pan se dore en 45 segundos a 1 minuto. Cubre una bandeja con papel de cocina y mezcla el azúcar fina con la canela. Cocinar con aceite caliente es peligroso: infórmate antes sobre cómo freír en abundante aceite de forma segura.'),
         (4, 'Coloca una boquilla de estrella (de 1,5 a 2 cm) en una manga pastelera y llénala con la masa reposada. Forma 2 o 3 tiras directamente en el aceite, cortando cada una con tijeras de cocina. Fríe hasta que estén doradas y crujientes. Cuidado: si se forman burbujas de aire dentro del churro pueden reventar, sobre todo si el aceite se sobrecalienta o la harina es vieja. Mantén a los niños fuera de la cocina, usa mangas largas y protección para los ojos, y aleja la cara de la olla.'),
         (5, 'Cuando estén crujientes y dorados, retira los churros con una espumadera y escúrrelos sobre la bandeja con papel. Sigue con el resto de la masa por tandas, espolvoreando los churros con azúcar y canela. Al final, mézclalos con el azúcar con canela que quede y sirve con la salsa de chocolate para mojar.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/nxnny61763250596.jpg', 'Churros con salsa de chocolate', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/453mpKHVaBs', 'Video de preparación', FALSE FROM nueva;

-- 53138 · Alfajores
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Alfajores',
           'Galletas de maicena rellenas de dulce de leche y pasadas por coco rallado. Fuente: TheMealDB.',
           90, 12, 'API', '53138', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53138')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Harina', 'g', 300, NULL),
         ('Maicena', 'g', 200, NULL),
         ('Mantequilla', 'g', 200, NULL),
         ('Azúcar', 'g', 100, NULL),
         ('Yemas de huevo', 'u', 2, NULL),
         ('Ralladura de limón', 'cdta', 1, NULL),
         ('Dulce de leche', 'ag', 1, NULL),
         ('Coco rallado', 'ag', 1, 'un poco')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Prepara la masa: bate la mantequilla con el azúcar. Agrega las yemas y la ralladura de limón. Incorpora poco a poco la harina y la maicena hasta formar una masa. Refrigera 1 hora.'),
         (2, 'Hornea las galletas: estira la masa, corta círculos y hornea a 180 °C (350 °F) de 12 a 15 minutos. Deja enfriar.'),
         (3, 'Arma los alfajores: unta dulce de leche en una galleta y cubre con otra. Pasa los bordes por coco rallado.'),
         (4, 'Consejos: refrigera la masa antes de estirarla para que sea más fácil de manejar y las galletas no se extiendan al hornearse. Para un toque más goloso, baña los alfajores en chocolate derretido y deja que cuajen sobre una rejilla.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/a4kgf21763075288.jpg', 'Alfajores', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/9WR40GHpsyo', 'Video de preparación', FALSE FROM nueva;

-- 53524 · Coconut Natilla Recipe
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Natilla de coco',
           'Postre cremoso de leche, coco y canela, espesado con maicena. Fuente: TheMealDB.',
           50, 8, 'API', '53524', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53524')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Leche', 'tz', 3, NULL),
         ('Maicena', 'tz', 1, 'más 2 cda'),
         ('Leche de coco', 'tz', 1, NULL),
         ('Coco', 'tz', 0.5, NULL),
         ('Canela en rama', 'u', 2, NULL),
         ('Leche condensada', 'u', 1, 'en lata'),
         ('Vainilla', 'cdta', 0.5, NULL),
         ('Azúcar', 'tz', 0.33, NULL),
         ('Sal', 'ag', 1, 'una pizca'),
         ('Mantequilla', 'cda', 1, NULL),
         ('Canela', 'ag', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Coloca 1 taza de leche en un tazón pequeño, agrega la maicena y revuelve hasta disolverla. Reserva.'),
         (2, 'Licúa la leche de coco con el coco rallado hasta que quede liso. Reserva.'),
         (3, 'Pon el resto de la leche y las ramas de canela en una olla mediana y lleva a hervir a fuego medio-bajo. Cuando la leche esté tibia, pero sin hervir, agrega la mezcla de coco.'),
         (4, 'Cuando la leche hierva, agrega la leche condensada, el azúcar y la sal. Revuelve bien con una cuchara de madera. Añade la mezcla de leche con maicena y sigue revolviendo sin parar.'),
         (5, 'Agrega la vainilla. Baja el fuego y continúa revolviendo hasta que la natilla espese, de 10 a 20 minutos. Añade la mantequilla, revuelve y retira del fuego. Descarta las ramas de canela. Sirve en un platón o en tacitas individuales, espolvorea canela molida y deja enfriar a temperatura ambiente al menos 2 horas. Refrigera hasta el momento de servir.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/gp1sj51783799928.jpg', 'Natilla de coco', TRUE FROM nueva;

-- 53575 · Apple Pie
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Pastel de manzana',
           'Pastel esponjoso de manzana con una capa de azúcar y canela por encima. Fuente: TheMealDB.',
           50, 8, 'API', '53575', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53575')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Manzana', 'u', 4, NULL),
         ('Huevo', 'u', 2, NULL),
         ('Azúcar', 'ml', 125, NULL),
         ('Harina', 'tz', 1.5, NULL),
         ('Sal', 'cdta', 0.5, NULL),
         ('Bicarbonato de sodio', 'cdta', 1, NULL),
         ('Leche agria', 'ml', 250, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Pela y limpia las manzanas y córtalas en rebanadas.'),
         (2, 'Bate los huevos con el azúcar hasta obtener una espuma clara.'),
         (3, 'Mezcla la harina, la sal y el bicarbonato.'),
         (4, 'Agrega los ingredientes secos a la mezcla de huevo y añade la leche agria poco a poco, revolviendo. Incorpora las manzanas.'),
         (5, 'Vierte la masa en un molde engrasado con mantequilla. Espolvorea azúcar (y canela) por encima.'),
         (6, 'Hornea a 180 °C durante 30 minutos.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/stnxzp1784835840.jpg', 'Pastel de manzana', TRUE FROM nueva;

-- 52855 · Banana Pancakes
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Panqueques de banano',
           'Panqueques de banano y huevo, sin harina, con nueces pecanas y frambuesas. Fuente: TheMealDB.',
           15, 2, 'API', '52855', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52855')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Banano', 'u', 1, 'grande'),
         ('Huevo', 'u', 2, 'mediano'),
         ('Polvo de hornear', 'ag', 1, 'una pizca'),
         ('Vainilla', 'ag', 1, 'un poco'),
         ('Aceite', 'cdta', 1, NULL),
         ('Nueces pecanas', 'g', 25, NULL),
         ('Frambuesas', 'g', 125, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'En un tazón, machaca el banano con un tenedor hasta obtener un puré espeso. Agrega los huevos, el polvo de hornear y la vainilla.'),
         (2, 'Calienta una sartén antiadherente grande a fuego medio y úntala con la mitad del aceite. Con la mitad de la masa, vierte dos panqueques, cocina de 1 a 2 minutos por lado y pasa a un plato. Repite con el aceite y la masa restantes. Cubre los panqueques con las nueces pecanas y las frambuesas.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/sywswr1511383814.jpg', 'Panqueques de banano', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/kSKtb2Sv-_U', 'Video de preparación', FALSE FROM nueva;

-- 52897 · Carrot Cake
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Postres'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Pastel de zanahoria',
           'Pastel húmedo de zanahoria y nueces con cobertura de queso crema. Fuente: TheMealDB.',
           120, 12, 'API', '52897', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52897')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Aceite vegetal', 'ml', 450, NULL),
         ('Harina', 'g', 400, NULL),
         ('Bicarbonato de sodio', 'cdta', 2, NULL),
         ('Azúcar', 'ml', 550, 'más 150 g'),
         ('Huevo', 'u', 5, NULL),
         ('Sal', 'cdta', 0.5, NULL),
         ('Canela', 'cdta', 2, NULL),
         ('Zanahoria', 'g', 500, 'rallado'),
         ('Nueces', 'g', 150, NULL),
         ('Queso crema', 'g', 200, NULL),
         ('Mantequilla', 'g', 100, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Para el pastel, precalienta el horno a 160 °C (325 °F). Engrasa y forra un molde desmontable de 26 cm.'),
         (2, 'Mezcla en un tazón todos los ingredientes del pastel, excepto las zanahorias y las nueces, hasta integrarlos bien. Agrega las zanahorias y las nueces.'),
         (3, 'Vierte la mezcla en el molde y hornea 1 hora y 15 minutos, o hasta que un palillo insertado en el centro salga limpio. Saca del horno y deja enfriar 10 minutos; luego desmolda con cuidado y deja enfriar por completo sobre una rejilla.'),
         (4, 'Mientras tanto, para la cobertura, bate el queso crema, el azúcar fina y la mantequilla hasta que quede esponjoso. Extiéndela sobre el pastel con una espátula.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/vrspxv1511722107.jpg', 'Pastel de zanahoria', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/asjZ7iTrGKA', 'Video de preparación', FALSE FROM nueva;

-- 53160 · Pisto con huevos
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Pisto con huevos',
           'Guiso de verduras al estilo español con huevos cocidos directamente en la salsa. Fuente: TheMealDB.',
           60, 4, 'API', '53160', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53160')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Aceite de oliva', 'cda', 2, NULL),
         ('Cebolla', 'u', 2, NULL),
         ('Ajo', 'u', 4, 'picado'),
         ('Chiles dulces variados', 'u', 5, NULL),
         ('Orégano', 'cdta', 1, NULL),
         ('Tomillo', 'ag', 1, 'ramitas frescas'),
         ('Hoja de laurel', 'u', 4, 'hojas'),
         ('Zucchini', 'u', 2, 'picado'),
         ('Berenjena', 'u', 1, 'picado'),
         ('Tomate', 'u', 4, 'grande'),
         ('Huevo', 'u', 4, 'grande'),
         ('Perejil', 'ag', 1, 'un puñado')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Calienta el aceite a fuego bajo en una cacerola grande resistente al fuego o en una sartén de hierro. Agrega las cebollas y una pizca de sal, tapa y cocina suavemente 15 minutos, revolviendo de vez en cuando. Añade el ajo y cocina 2 minutos más.'),
         (2, 'Incorpora los chiles dulces y cocina a fuego medio, tapado, unos 5 minutos, revolviendo de vez en cuando, hasta que estén apenas tiernos.'),
         (3, 'Mezcla el orégano, el tomillo, el laurel, un poco de pimienta negra y sal si hace falta. Agrega el zucchini y la berenjena, mezcla bien y cocina a fuego medio, tapado, 10 minutos. Añade los tomates, tapa y cocina 20 minutos, revolviendo de vez en cuando.'),
         (4, 'Rompe los huevos con cuidado sobre el pisto, procurando no romper las yemas. Cocina en la salsa a fuego medio de 5 a 6 minutos, hasta que los huevos estén cocidos pero con la yema aún blanda. Espolvorea con perejil antes de servir.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/0nswfe1763279040.jpg', 'Pisto con huevos', TRUE FROM nueva;

-- 52872 · Spanish Tortilla
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Tortilla española',
           'Tortilla de papa y cebolla cocida lentamente, acompañada de pan con tomate. Fuente: TheMealDB.',
           60, 4, 'API', '52872', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52872')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Cebolla', 'u', 1, 'en rodajas'),
         ('Aceite de oliva', 'cda', 4, NULL),
         ('Mantequilla', 'g', 25, NULL),
         ('Papa', 'g', 400, NULL),
         ('Ajo', 'u', 6, 'dientes'),
         ('Huevo', 'u', 8, NULL),
         ('Perejil', 'ag', 1, 'un puñado'),
         ('Baguette', 'u', 1, NULL),
         ('Tomate', 'u', 4, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Pon una sartén antiadherente grande a fuego bajo. Cocina la cebolla lentamente en el aceite y la mantequilla hasta que esté suave pero sin dorar, unos 15 minutos. Agrega las papas, tapa la sartén y cocina de 15 a 20 minutos más, revolviendo de vez en cuando para que se frían de manera pareja.'),
         (2, 'Cuando las papas estén blandas y la cebolla brillante, machaca 2 dientes de ajo, agrégalos y mezcla; después añade los huevos batidos.'),
         (3, 'Tapa la sartén y deja cocinar la tortilla suavemente. A los 20 minutos, los bordes y la base deben estar dorados, la parte de arriba cuajada y el centro todavía un poco tembloroso. Para voltearla, deslízala sobre un plato, coloca otro plato encima, voltea todo y devuélvela a la sartén para terminar de cocinarla. Pasa a un plato y sirve la tortilla tibia o fría, con el perejil picado por encima.'),
         (4, 'Para acompañar, toma rebanadas de baguette calientes, pínchalas con un tenedor y frótalas con el ajo restante; cubre con tomate rallado y sazona con sal marina y un chorrito de aceite de oliva.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/quuxsx1511476154.jpg', 'Tortilla española', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/JceGMNG7rpU', 'Video de preparación', FALSE FROM nueva;

-- 53172 · Patatas bravas
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Papas bravas',
           'Papas asadas crujientes con una salsa de tomate picante, típica de las tapas españolas. Fuente: TheMealDB.',
           65, 4, 'API', '53172', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53172')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Aceite de oliva', 'cda', 5, NULL),
         ('Cebolla', 'u', 1, 'pequeño, finamente, en cubos'),
         ('Ajo', 'u', 2, 'dientes, picado'),
         ('Tomate en lata', 'g', 225, NULL),
         ('Puré de tomate', 'cda', 1, NULL),
         ('Paprika', 'cdta', 2, NULL),
         ('Chile en polvo', 'ag', 1, 'una pizca'),
         ('Azúcar', 'ag', 1, 'una pizca'),
         ('Perejil', 'ag', 1, 'picado'),
         ('Papa', 'g', 900, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Calienta el aceite en una olla y sofríe la cebolla unos 5 minutos hasta que se ablande. Agrega el ajo, el tomate en trozos, el puré de tomate, la paprika dulce, el chile en polvo, el azúcar y una pizca de sal; lleva a ebullición revolviendo de vez en cuando. Baja a fuego lento y cocina 10 minutos hasta que quede espesa. Se puede guardar en el refrigerador hasta 24 horas.'),
         (2, 'Calienta el horno a 200 °C (180 °C con ventilador). Seca las papas con papel de cocina, colócalas en una bandeja para asar y mézclalas con el aceite de oliva y sal. Hornea de 40 a 50 minutos, hasta que estén crujientes y doradas. Pasa las papas a platos de servir y báñalas con la salsa de tomate. Espolvorea con perejil fresco y sirve.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/bvg8sn1763298713.jpg', 'Papas bravas', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/2Hup2nypG2w', 'Video de preparación', FALSE FROM nueva;

-- 53266 · Falafel
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Falafel',
           'Croquetas de garbanzo con cebolla, ajo y especias, doradas en la sartén. Fuente: TheMealDB.',
           25, 2, 'API', '53266', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53266')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Aceite de girasol', 'cda', 2, NULL),
         ('Cebolla', 'u', 1, 'picado'),
         ('Ajo', 'u', 1, 'diente, pelado, machacado'),
         ('Garbanzos', 'g', 400, NULL),
         ('Comino', 'cdta', 1, NULL),
         ('Cilantro molido', 'cdta', 1, NULL),
         ('Perejil', 'ag', 1, 'un puñado'),
         ('Huevo', 'u', 1, 'batido')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Calienta 1 cucharada de aceite en una sartén grande y sofríe la cebolla y el ajo a fuego bajo 5 minutos, hasta que se ablanden. Pasa a un tazón grande con los garbanzos y las especias y machaca con un tenedor o un prensapapas hasta que los garbanzos queden totalmente deshechos. Agrega el perejil o hierbas secas y sazona al gusto. Añade el huevo y amasa la mezcla con las manos.'),
         (2, 'Forma 6 bolas con la mezcla y aplánalas hasta formar tortitas. Calienta el aceite restante en la sartén y fríe los falafel a fuego medio 3 minutos por lado, hasta que estén dorados y firmes. Sirve calientes o fríos con cuscús, pan pita o ensalada.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/u5e9qq1763795441.jpg', 'Falafel', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/NZcWedPKysk', 'Video de preparación', FALSE FROM nueva;

-- 53506 · Rice and Beans
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Arroz con frijoles y leche de coco',
           'Arroz y frijoles cocidos en leche de coco con cebolla y chile habanero, al estilo caribeño. Fuente: TheMealDB.',
           60, 6, 'API', '53506', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53506')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Frijoles rojos', 'u', 0.5, 'bolsa'),
         ('Leche de coco', 'u', 1, 'en lata'),
         ('Cebolla', 'u', 1, 'en cubos'),
         ('Chile habanero', 'u', 3, NULL),
         ('Pimienta negra', 'cda', 1, NULL),
         ('Sal', 'cda', 0.5, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Cocina los frijoles hasta que estén tiernos. Agrega el arroz, la leche de coco, la cebolla, la pimienta negra y la sal, y añade suficiente agua para cubrir apenas los ingredientes.'),
         (2, 'Coloca los chiles habaneros encima, con cuidado de no reventarlos. Cocina a fuego lento hasta que el arroz esté tierno. Retira los chiles antes de servir.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/j8c1d51782772399.jpg', 'Arroz con frijoles y leche de coco', TRUE FROM nueva;

-- 52908 · Ratatouille
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Ratatouille',
           'Guiso provenzal de berenjena, zucchini, chile dulce y tomate, cocinado por etapas. Fuente: TheMealDB.',
           60, 4, 'API', '52908', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52908')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Berenjena', 'u', 2, 'grande'),
         ('Zucchini', 'u', 4, NULL),
         ('Chile dulce amarillo', 'u', 2, NULL),
         ('Tomate', 'u', 4, 'grande'),
         ('Aceite de oliva', 'cda', 5, NULL),
         ('Albahaca', 'ag', 1, 'un manojo'),
         ('Cebolla', 'u', 1, 'mediano'),
         ('Ajo', 'u', 3, 'finamente, picado'),
         ('Vinagre de vino tinto', 'cdta', 1, NULL),
         ('Azúcar', 'cdta', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Corta las berenjenas por la mitad a lo largo. Colócalas con el corte hacia abajo, vuelve a cortarlas por la mitad a lo largo y luego en trozos de 1,5 cm. Quita las puntas de los zucchinis y córtalos en rodajas de 1,5 cm. Pela los chiles dulces de arriba abajo; sosteniéndolos de pie, corta alrededor del tallo y luego en 3 partes. Retira las membranas y pica en trozos del tamaño de un bocado.'),
         (2, 'Haz una pequeña cruz en la base de cada tomate y colócalos en un recipiente resistente al calor. Cúbrelos con agua hirviendo, déjalos 20 segundos y sácalos. Tira el agua, devuelve los tomates y cúbrelos con agua fría. Cuando se enfríen, quita la piel. Pártelos en cuartos, retira las semillas con una cuchara y pica la pulpa en trozos grandes.'),
         (3, 'Calienta una sartén honda a fuego medio y, cuando esté caliente, agrega 2 cucharadas de aceite de oliva. Dora las berenjenas 5 minutos por lado, hasta que estén blandas. Resérvalas y fríe los zucchinis en otra cucharada de aceite 5 minutos, hasta que doren por ambos lados. Repite con los chiles dulces. No cocines de más las verduras en esta etapa: aún tienen que cocinarse en el paso siguiente.'),
         (4, 'Rompe las hojas de albahaca con las manos y resérvalas. Cocina la cebolla en la sartén 5 minutos. Agrega el ajo y fríe 1 minuto más. Incorpora el vinagre y el azúcar, y luego los tomates y la mitad de la albahaca. Devuelve las verduras a la sartén con sal y pimienta y cocina 5 minutos. Sirve con albahaca fresca.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/wrpwuu1511786491.jpg', 'Ratatouille', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/BFdQUgAFtGU', 'Video de preparación', FALSE FROM nueva;

-- 52785 · Dal fry
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Dal fry (lentejas al estilo indio)',
           'Lentejas toor dal cocidas hasta espesar y sazonadas con un sofrito de especias, tomate y ghee. Fuente: TheMealDB.',
           60, 4, 'API', '52785', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52785')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Lenteja toor dal', 'tz', 1, NULL),
         ('Agua', 'tz', 2.5, NULL),
         ('Sal', 'cdta', 1.5, NULL),
         ('Cúrcuma', 'cdta', 0.25, NULL),
         ('Ghee', 'cda', 3, NULL),
         ('Tomate en trozos', 'tz', 1, NULL),
         ('Semillas de comino', 'cdta', 0.5, NULL),
         ('Semillas de mostaza', 'cdta', 0.5, NULL),
         ('Hoja de laurel', 'u', 2, NULL),
         ('Chile verde', 'cda', 1, 'picado'),
         ('Jengibre', 'cdta', 2, 'desmenuzado'),
         ('Culantro', 'cda', 2, NULL),
         ('Chile rojo en polvo', 'cdta', 0.5, NULL),
         ('Azúcar', 'cdta', 1, NULL),
         ('Garam masala', 'cdta', 0.25, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Lava el toor dal y remójalo en unas 3 tazas de agua al menos una hora. Duplicará su volumen. Escurre el agua.'),
         (2, 'Cocina el dal con 2 1/2 tazas de agua, la sal y la cúrcuma a fuego medio-alto hasta que esté suave (unos 30 minutos); debe quedar como una sopa espesa.'),
         (3, 'En una sartén, calienta el ghee. Agrega las semillas de comino y de mostaza. Cuando revienten, añade el laurel, el chile verde, el jengibre y el chile en polvo. Revuelve unos segundos.'),
         (4, 'Agrega los tomates, la sal y el azúcar; revuelve y cocina hasta que los tomates estén tiernos y deshechos.'),
         (5, 'Añade el culantro y el garam masala y cocina cerca de un minuto.'),
         (6, 'Vierte el sofrito sobre el dal, mezcla bien y cocina un minuto más.'),
         (7, 'Sirve con pan naan.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/wuxrtu1483564410.jpg', 'Dal fry (lentejas al estilo indio)', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/J4D855Q9-jg', 'Video de preparación', FALSE FROM nueva;

-- 53573 · Papaya salad
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Vegetariano'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Ensalada de papaya verde',
           'Ensalada fresca de papaya verde rallada, zanahoria, tomate cherry y maní. Fuente: TheMealDB.',
           20, 4, 'API', '53573', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53573')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Papaya verde', 'u', 0.5, NULL),
         ('Jugo de limón', 'cda', 1, NULL),
         ('Sal', 'ag', 1, NULL),
         ('Zanahoria', 'u', 1, NULL),
         ('Tomate cherry', 'tz', 1, NULL),
         ('Cebollino', 'tz', 0.5, NULL),
         ('Maní tostado', 'tz', 0.25, NULL),
         ('Ajo', 'cda', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Corta, pela y ralla la papaya. Puedes usar un rallador de juliana; si no tienes, usa un rallador común.'),
         (2, 'Pela y ralla las zanahorias.'),
         (3, 'Pica el cebollino bien pequeño.'),
         (4, 'Corta los tomates cherry por la mitad.'),
         (5, 'Combina todos los ingredientes con el jugo de limón, el ajo y la sal.'),
         (6, 'Termina con el maní por encima.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/7ytdtz1784833420.jpg', 'Ensalada de papaya verde', TRUE FROM nueva;

-- 53531 · Chicken in Orange Sauce Recipe (Pollo a la Naranja)
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Pollo a la naranja',
           'Piernas de pollo guisadas en una salsa de naranja y azúcar morena, servidas sobre arroz blanco. Fuente: TheMealDB.',
           65, 4, 'API', '53531', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53531')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Piernas de pollo', 'u', 4, NULL),
         ('Aceite de oliva', 'cda', 2, NULL),
         ('Mantequilla', 'cda', 2, NULL),
         ('Cebolla', 'tz', 0.5, NULL),
         ('Chile dulce rojo', 'u', 1, NULL),
         ('Ajo', 'u', 3, 'dientes, picado'),
         ('Sal', 'cdta', 1, NULL),
         ('Pimienta negra', 'cdta', 0.5, NULL),
         ('Caldo de pollo', 'tz', 1, NULL),
         ('Jugo de naranja', 'tz', 0.5, NULL),
         ('Azúcar morena', 'cda', 3, NULL),
         ('Culantro', 'tz', 0.25, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Coloca el aceite de oliva y la mantequilla en una olla grande a fuego medio. Agrega la cebolla picada, el chile dulce rojo y el ajo. Cocina unos 3 minutos, revolviendo seguido, hasta que las verduras estén suaves.'),
         (2, 'Agrega las piernas de pollo y sazona con sal y pimienta. Cocina cerca de 1 minuto por lado.'),
         (3, 'Añade el caldo de pollo y el jugo de naranja y cocina unos 2 minutos, raspando el fondo de la olla con una cuchara de madera. Agrega el azúcar morena y tapa la olla. Baja el fuego a medio y cocina unos 45 minutos o hasta que el pollo esté cocido.'),
         (4, 'Baña el pollo con la salsa y sirve sobre arroz blanco. Decora con culantro fresco.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/uuns781783804024.jpg', 'Pollo a la naranja', TRUE FROM nueva;

-- 53540 · Pollo en Salsa
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Pollo en salsa',
           'Muslos de pollo guisados con achiote, comino y orégano en olla de presión, ideales con arroz y frijoles. Fuente: TheMealDB.',
           45, 4, 'API', '53540', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53540')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Muslos de pollo', 'u', 4, NULL),
         ('Orégano', 'cdta', 1, NULL),
         ('Comino', 'cdta', 1, NULL),
         ('Sal', 'cdta', 1, NULL),
         ('Pimienta', 'cdta', 1, NULL),
         ('Pasta de achiote', 'cdta', 1.5, NULL),
         ('Ajo', 'u', 4, 'dientes, machacado'),
         ('Cebolla', 'u', 1, 'mediano'),
         ('Tomate', 'u', 1, 'grande'),
         ('Chile dulce rojo', 'u', 0.5, NULL),
         ('Caldo de pollo', 'tz', 2, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Pica el chile dulce, la cebolla y el tomate y reserva. Enciende la olla de presión eléctrica (Instant Pot) en la función de sofreír durante 8 minutos. Agrega un poco de aceite de oliva, la cebolla, el ajo, el tomate, el chile dulce, el comino, el orégano, la sal y la pimienta, y sofríe 4 minutos.'),
         (2, 'Añade el pollo y cocina 2 minutos por lado, mezclando seguido. Agrega el caldo de pollo, tapa y programa la olla en alta presión por 20 minutos. Al terminar, libera la presión y, si hace falta, agrega un poco más de sal. Se sirve con parte del caldo sobre arroz con frijoles.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/4o13oe1784396585.jpg', 'Pollo en salsa', TRUE FROM nueva;

-- 53520 · Pastel de Choclo
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Pastel de choclo',
           'Pastel chileno de maíz dulce cremoso sobre un pino de carne, pollo, pasas, aceitunas y huevo. Fuente: TheMealDB.',
           90, 6, 'API', '53520', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53520')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Huevo', 'u', 2, NULL),
         ('Cebolla', 'u', 2, 'picado'),
         ('Ajo', 'u', 2, 'dientes, picado fino'),
         ('Sal', 'cdta', 1, NULL),
         ('Paprika', 'cdta', 0.5, NULL),
         ('Pimienta negra', 'ag', 1, NULL),
         ('Carne molida', 'g', 454, NULL),
         ('Pollo', 'u', 0.5, NULL),
         ('Pasas', 'cda', 2, NULL),
         ('Aceitunas verdes', 'cda', 2, NULL),
         ('Maíz dulce', 'u', 3, 'en lata'),
         ('Albahaca', 'cdta', 2, NULL),
         ('Mantequilla', 'cda', 1, NULL),
         ('Leche', 'tz', 1, NULL),
         ('Azúcar', 'cdta', 2, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Coloca los huevos en una olla y cúbrelos con agua. Lleva a ebullición, retira del fuego y deja los huevos en el agua caliente 15 minutos.'),
         (2, 'Mientras tanto, calienta aceite en una sartén a fuego medio. Agrega las cebollas, el ajo, una pizca de sal, 1/4 de cucharadita de paprika y pimienta negra; cocina hasta que suelte aroma, unos 2 minutos. Añade la carne, el resto de la cucharadita de sal, el resto de la paprika y pimienta negra; cocina y revuelve hasta que la carne esté casi dorada, unos 5 minutos. Agrega el pollo, las pasas y las aceitunas.'),
         (3, 'Saca los huevos del agua caliente, enfríalos bajo el chorro de agua fría y pélalos. Pícalos grueso y agrégalos a la mezcla de carne. Mantén el pino caliente.'),
         (4, 'Licúa el maíz con la albahaca en una licuadora o procesador hasta que quede liso. Vierte en una olla a fuego medio. Agrega la mantequilla y cocina revolviendo hasta que espese, de 7 a 10 minutos. Incorpora la leche, 2 cucharaditas de azúcar y sal; cocina hasta que forme una pasta espesa, unos 15 minutos más.'),
         (5, 'Precalienta el horno a 175 °C (350 °F).'),
         (6, 'Extiende el pino en un molde para hornear. Cubre con la pasta de maíz y espolvorea un poco de azúcar encima.'),
         (7, 'Hornea hasta que la superficie esté dorada, unos 20 minutos.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/47abic1782854326.jpg', 'Pastel de choclo', TRUE FROM nueva;

-- 53143 · Easy Spanish chicken
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Pollo español fácil con chorizo y alubias',
           'Muslos de pollo al horno sobre un sofrito de chorizo, chiles dulces, tomate, aceitunas y alubias. Fuente: TheMealDB.',
           70, 4, 'API', '53143', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53143')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Aceite de oliva extra virgen', 'cda', 2, NULL),
         ('Cebolla', 'u', 1, NULL),
         ('Chorizo', 'g', 150, NULL),
         ('Chile dulce rojo', 'u', 1, NULL),
         ('Chile dulce amarillo', 'u', 1, NULL),
         ('Paprika ahumada', 'cdta', 1, NULL),
         ('Ajo', 'u', 4, 'dientes'),
         ('Tomate en lata', 'g', 400, NULL),
         ('Aceitunas verdes', 'g', 150, NULL),
         ('Alubias', 'g', 400, NULL),
         ('Muslos de pollo', 'u', 8, NULL),
         ('Perejil', 'ag', 1, 'puñado')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Calienta el horno a 200 °C (180 °C con ventilador). Calienta el aceite de oliva en una sartén grande apta para horno a fuego medio y sofríe la cebolla, el chorizo y los chiles dulces con una pizca de sal y pimienta durante 15 minutos, hasta que las verduras se ablanden y el chorizo suelte su grasa. Agrega la paprika y el ajo y cocina unos minutos más hasta que suelte aroma.'),
         (2, 'Incorpora el tomate en trozos, las aceitunas y las alubias, mezcla y sazona. Acomoda los muslos de pollo sobre la mezcla y sazónalos bien. Pasa al horno y hornea 40 minutos, hasta que la piel del pollo esté crujiente y la carne cocida y tierna. Esparce el perejil y sirve.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/kzxflc1763194887.jpg', 'Pollo español fácil con chorizo y alubias', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/EusTpzrqYK4', 'Video de preparación', FALSE FROM nueva;

-- 53533 · Breaded Steak Recipe (Lomo de Res Apanado)
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Lomo de res apanado',
           'Filetes de res macerados en salsa de aliños, empanizados y fritos hasta quedar dorados. Fuente: TheMealDB.',
           60, 4, 'API', '53533', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53533')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Carne de res', 'g', 907, NULL),
         ('Salsa de aliños', 'tz', 1, NULL),
         ('Harina', 'tz', 1, NULL),
         ('Pan molido', 'tz', 2, NULL),
         ('Comino', 'cdta', 1, NULL),
         ('Huevo', 'u', 3, NULL),
         ('Sal', 'ag', 1, NULL),
         ('Pimienta negra', 'ag', 1, NULL),
         ('Perejil', 'cda', 2, NULL),
         ('Aceite', 'ag', 1, 'para freír')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Coloca cada trozo de carne entre dos láminas grandes de plástico y golpéalo con un mazo hasta que tenga de 0,6 a 1,3 cm de grosor. Pasa la carne aplanada a un recipiente de vidrio, agrega la salsa de aliños y déjala reposar unas 3 horas o toda la noche en el refrigerador.'),
         (2, 'Coloca la harina, el pan molido, las especias y el perejil en un plato o un tazón (la harina se usa aparte para el primer y el último paso).'),
         (3, 'Cubre la carne por todos los lados con la harina, luego con el huevo batido y por último otra vez con la mezcla de harina y pan molido.'),
         (4, 'Una vez empanizados, puedes pasar los filetes directamente a la sartén o colocarlos en capas sobre papel encerado hasta el momento de freír.'),
         (5, 'Calienta el aceite en una sartén grande a fuego medio. En cuanto empiece a humear ligeramente, agrega la carne empanizada y fríela de 3 a 4 minutos por lado, hasta que esté dorada. Retírala y colócala sobre un plato con papel absorbente para eliminar el exceso de aceite; sirve.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/lmwaf61783805434.jpg', 'Lomo de res apanado', TRUE FROM nueva;

-- 53545 · Ropa Veija
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Ropa vieja',
           'Carne de res cocida lentamente en salsa de tomate con chiles dulces y aceitunas, deshebrada y servida con arroz y frijoles. Fuente: TheMealDB.',
           210, 6, 'API', '53545', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53545')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Falda de res', 'g', 907, NULL),
         ('Sal', 'cdta', 3, NULL),
         ('Aceite vegetal', 'cda', 4, NULL),
         ('Chile dulce rojo', 'u', 1, NULL),
         ('Chile dulce amarillo', 'u', 1, NULL),
         ('Cebolla', 'u', 1, NULL),
         ('Ajo', 'u', 6, 'dientes'),
         ('Puré de tomate', 'cda', 2, NULL),
         ('Paprika ahumada', 'cda', 1, NULL),
         ('Comino', 'cdta', 2, NULL),
         ('Orégano', 'cdta', 2, NULL),
         ('Vino blanco seco', 'tz', 0.5, NULL),
         ('Tomate en lata', 'u', 1, 'en lata'),
         ('Hoja de laurel', 'u', 2, NULL),
         ('Aceitunas rellenas de pimiento', 'tz', 1, NULL),
         ('Arroz', 'ag', 1, 'para servir'),
         ('Frijoles negros', 'ag', 1, 'para servir'),
         ('Culantro', 'tz', 0.25, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Corta la falda de res en 4 trozos rectangulares. Sécalos con papel absorbente y sazónalos por todos lados con 1 1/2 cucharaditas de sal.'),
         (2, 'En una olla grande a fuego medio-alto, calienta 2 cucharadas de aceite. Por tandas, dora la carne de 5 a 6 minutos por ambos lados. Pasa a un plato.'),
         (3, 'En la misma olla a fuego medio, calienta las 2 cucharadas de aceite restantes. Cocina los chiles dulces, la cebolla y 1 1/2 cucharaditas de sal, revolviendo de vez en cuando, hasta que se ablanden un poco, de 4 a 5 minutos. Agrega el ajo, la pasta de tomate, la paprika, el comino y el orégano, y cocina, revolviendo de vez en cuando, hasta que las verduras estén blandas y las especias suelten aroma, de 1 a 2 minutos más.'),
         (4, 'Añade el vino, raspando los trocitos dorados del fondo, y cocina, revolviendo de vez en cuando, hasta que se reduzca a la mitad, unos 2 minutos. Agrega los tomates con su jugo y el laurel. Aplasta los tomates para deshacerlos, acomoda la carne en el guiso y, si hace falta, agrega agua hasta cubrirla; lleva a hervor suave. Baja el fuego a medio-bajo, tapa y cocina hasta que la carne se deshaga fácilmente, de 2 1/2 a 3 horas. Pasa la carne a un plato y deshébrala finamente con 2 tenedores.'),
         (5, 'Devuelve la carne deshebrada al guiso. Lleva a hervor suave y cocina, revolviendo, hasta que el líquido se reduzca a una salsa que cubra la carne, de 5 a 10 minutos. Agrega las aceitunas y sazona con sal.'),
         (6, 'Sirve la ropa vieja sobre arroz con frijoles negros. Decora con culantro.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/thvft91784576451.jpg', 'Ropa vieja', TRUE FROM nueva;

-- 53135 · Milanesa
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Milanesa',
           'Filetes de res empanizados con ajo y perejil, fritos hasta quedar dorados. Fuente: TheMealDB.',
           30, 4, 'API', '53135', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53135')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Filete de res', 'u', 4, NULL),
         ('Huevo', 'u', 2, 'batido'),
         ('Pan molido', 'g', 200, NULL),
         ('Perejil', 'cda', 2, 'picado'),
         ('Ajo', 'u', 2, 'dientes, picado fino'),
         ('Sal', 'ag', 1, NULL),
         ('Pimienta', 'ag', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Sazona los filetes: sala y pimienta los filetes.'),
         (2, 'Empaniza los filetes: pasa cada filete por el huevo y luego cúbrelo con una mezcla de pan molido, perejil y ajo.'),
         (3, 'Fríe: calienta aceite en una sartén grande a fuego medio. Fríe los filetes hasta que estén dorados y cocidos, de 3 a 4 minutos por lado.'),
         (4, 'Sirve: sirve caliente con rodajas de limón o una ensalada.'),
         (5, 'Consejos: para una costra extra crujiente, usa pan molido tipo panko mezclado con queso parmesano rallado fino. Aplana los filetes con suavidad hasta que tengan un grosor parejo para que se cocinen de manera uniforme.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/wdfa171763065079.jpg', 'Milanesa', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/_G3cyucbB3o', 'Video de preparación', FALSE FROM nueva;

-- 53539 · Picadillo de Vainicas (Green Beans Picadillo)
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Picadillo de vainicas',
           'Picadillo de vainicas con carne deshebrada, cebolla, ajo y tomillo. Fuente: TheMealDB.',
           40, 4, 'API', '53539', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53539')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Vainicas', 'g', 454, NULL),
         ('Falda de res', 'tz', 2, NULL),
         ('Cebolla', 'tz', 0.5, NULL),
         ('Chile dulce', 'tz', 0.5, NULL),
         ('Ajo', 'u', 3, 'dientes'),
         ('Tomillo', 'cdta', 1, NULL),
         ('Sal', 'cdta', 1, NULL),
         ('Pimienta negra', 'cdta', 0.5, NULL),
         ('Agua', 'tz', 0.5, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'En una olla mediana a fuego medio, agrega un poco de aceite de oliva, la cebolla, el chile dulce y el ajo, y cocina hasta que la cebolla esté transparente.'),
         (2, 'Mientras tanto, pica finamente las vainicas.'),
         (3, 'Agrega a la olla la carne deshebrada, las vainicas, el tomillo, la sal y la pimienta, y mezcla bien. Añade el agua, tapa y cocina 15 minutos; destapa y cocina 5 minutos más, o hasta que las vainicas estén blandas.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/2ncui81784396094.jpg', 'Picadillo de vainicas', TRUE FROM nueva;

-- 53326 · Venezuelan Sancocho
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Sancocho venezolano',
           'Sopa espesa de res con yuca, yautía, ñame, calabaza y maíz, servida con arepas o casabe. Fuente: TheMealDB.',
           120, 6, 'API', '53326', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53326')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Jarrete de res', 'g', 907, NULL),
         ('Cebolla', 'u', 2, 'pequeño'),
         ('Chile dulce', 'u', 0.5, NULL),
         ('Ajo', 'u', 8, 'dientes, picado'),
         ('Puerro', 'u', 1, NULL),
         ('Caldo de res', 'tz', 8, NULL),
         ('Chile dulce rojo', 'tz', 0.5, NULL),
         ('Cebollino', 'tz', 0.5, NULL),
         ('Maíz dulce', 'u', 2, NULL),
         ('Yuca', 'g', 227, NULL),
         ('Yautía', 'g', 227, NULL),
         ('Ñame', 'g', 227, NULL),
         ('Calabaza', 'g', 227, NULL),
         ('Sal', 'ag', 1, NULL),
         ('Pimienta', 'ag', 1, NULL),
         ('Culantro', 'ag', 1, 'picado')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Coloca el jarrete, 1 cebolla pequeña partida por la mitad, el chile dulce partido por la mitad, 4 dientes de ajo enteros, las hojas verde oscuro del puerro y el caldo en una olla grande. Cocina de 45 a 60 minutos hasta que la carne esté tierna al pincharla con un tenedor. Nota: la carne puede tardar un poco más; no hay problema, solo agrega más tiempo si hace falta.'),
         (2, 'Cuando la carne esté tierna, retira los trozos grandes de verdura y los huesos, y deséchalos.'),
         (3, 'Agrega la cebolla picada, 2 dientes de ajo picados, los chiles dulces pequeños, el puerro rebanado (la parte verde claro), el cebollino, la yuca y el maíz. Mezcla y cocina tapado a fuego medio de 5 a 8 minutos, hasta que la yuca empiece a ablandarse. Nota: la yuca puede tardar un poco más; solo agrega más tiempo si es necesario.'),
         (4, 'Añade la yautía, el ñame y la calabaza. Mezcla. Cocina tapado a fuego medio de 5 a 6 minutos, hasta que todos los tubérculos estén tiernos. Nota: no los cocines de más o se deshacen.'),
         (5, 'Prueba y agrega sal si hace falta. Añade el culantro y los 2 dientes de ajo restantes, picados. Mezcla y deja hervir suavemente 2 minutos más.'),
         (6, 'Sirve caliente en tazones grandes, repartiendo la carne y las verduras de manera pareja. Agrega un chorrito de jugo de limón o salsa picante si lo deseas. Acompaña con arepas o casabe (pan de yuca).')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/8859m71764377470.jpg', 'Sancocho venezolano', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/w6KNOKkcRBs', 'Video de preparación', FALSE FROM nueva;

-- 52770 · Spaghetti Bolognese
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Espagueti a la boloñesa',
           'Espagueti con una salsa de carne molida, tomate, champiñones y orégano cocida a fuego lento. Fuente: TheMealDB.',
           50, 4, 'API', '52770', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52770')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Cebolla', 'u', 2, NULL),
         ('Aceite de oliva', 'cda', 1, NULL),
         ('Ajo', 'u', 1, 'diente'),
         ('Carne molida', 'g', 500, NULL),
         ('Champiñones', 'g', 90, NULL),
         ('Orégano', 'cdta', 1, NULL),
         ('Tomate', 'g', 400, 'en lata'),
         ('Caldo de res', 'ml', 300, NULL),
         ('Puré de tomate', 'cda', 1, NULL),
         ('Salsa inglesa', 'cda', 1, NULL),
         ('Espagueti', 'g', 350, NULL),
         ('Queso parmesano', 'ag', 1, 'para cubrir')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Pon la cebolla y el aceite en una olla grande y fríe a fuego bastante alto de 3 a 4 minutos. Agrega el ajo y la carne molida y fríe hasta que ambos se doren. Añade los champiñones y las hierbas y cocina un par de minutos más.'),
         (2, 'Incorpora los tomates, el caldo de res, el puré de tomate (o salsa de tomate), la salsa inglesa y la sazón. Lleva a ebullición, baja el fuego, tapa y cocina a fuego lento 30 minutos, revolviendo de vez en cuando.'),
         (3, 'Mientras tanto, cocina el espagueti en una olla grande con agua hirviendo con sal, según las instrucciones del paquete. Escurre bien, pásale agua caliente, devuélvelo a la olla y, si quieres, agrégale un chorrito de aceite de oliva; luego mézclalo con la salsa de carne. Sirve en platos calientes y pasa queso parmesano para espolvorear por encima.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/sutysw1468247559.jpg', 'Espagueti a la boloñesa', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/-gF8d-fitkU', 'Video de preparación', FALSE FROM nueva;

-- 52982 · Spaghetti alla Carbonara
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Espagueti a la carbonara',
           'Pasta con tocino crujiente, yemas y queso pecorino, sin crema. Fuente: TheMealDB.',
           30, 4, 'API', '52982', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '52982')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Espagueti', 'g', 320, NULL),
         ('Yemas de huevo', 'u', 6, NULL),
         ('Sal', 'ag', 1, 'según se necesite'),
         ('Tocino', 'g', 150, NULL),
         ('Queso pecorino', 'g', 50, NULL),
         ('Pimienta negra', 'ag', 1, 'según se necesite')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Pon una olla grande con agua a hervir.'),
         (2, 'Pica finamente la panceta (o el tocino), 150 g en total, después de quitarle la corteza. Ralla finamente el queso pecorino (50 g); si quieres, mezcla con un poco de parmesano rallado.'),
         (3, 'Bate las yemas en un tazón mediano y sazona con un poco de pimienta negra recién molida. Reserva todo.'),
         (4, 'Agrega 1 cucharadita de sal al agua hirviendo y añade el espagueti (320 g). Cuando el agua vuelva a hervir, cocina a hervor constante, tapado, 10 minutos o hasta que esté al dente (recién cocido).'),
         (5, 'Machaca 2 dientes de ajo pelados con la hoja de un cuchillo, solo para magullarlos.'),
         (6, 'Mientras se cocina el espagueti, fríe la panceta con el ajo. Derrite 50 g de mantequilla sin sal en una sartén grande o wok y, en cuanto se derrita, agrega la panceta y el ajo.'),
         (7, 'Cocina a fuego medio unos 5 minutos, revolviendo seguido, hasta que la panceta esté dorada y crujiente. El ajo ya habrá dado su sabor: sácalo con una espumadera y deséchalo.'),
         (8, 'Mantén el fuego bajo bajo la panceta. Cuando la pasta esté lista, sácala del agua con unas pinzas y pásala a la sartén con la panceta. No te preocupes si cae un poco de agua (es lo que se busca) y no tires aún el agua de la pasta.'),
         (9, 'Mezcla la mayor parte del queso con los huevos, reservando un puñado para espolvorear al final.'),
         (10, 'Retira la sartén del fuego. Vierte rápidamente la mezcla de huevo y queso. Con las pinzas o un tenedor largo, levanta el espagueti para que se mezcle con el huevo, que espesa pero no se cuaja, y todo quede cubierto.'),
         (11, 'Agrega agua de cocción de la pasta para mantener la salsa cremosa (unas cucharadas bastan). No debe quedar mojado, solo húmedo. Sazona con un poco de sal si hace falta.'),
         (12, 'Con un tenedor largo, enrolla la pasta sobre el plato. Sirve de inmediato con el queso restante y un poco de pimienta negra molida. Si el plato se seca antes de servir, agrega un chorrito de agua caliente de la pasta para devolverle el brillo.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/llcbn01574260722.jpg', 'Espagueti a la carbonara', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/_T6jkRvhlkk', 'Video de preparación', FALSE FROM nueva;

-- 53064 · Fettuccine Alfredo
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Fettuccine Alfredo',
           'Pasta en una salsa cremosa de mantequilla, crema y queso parmesano. Fuente: TheMealDB.',
           25, 4, 'API', '53064', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53064')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Fettuccine', 'g', 454, NULL),
         ('Crema para batir', 'tz', 0.5, NULL),
         ('Mantequilla', 'tz', 0.5, NULL),
         ('Queso parmesano', 'tz', 0.5, NULL),
         ('Perejil', 'cda', 2, NULL),
         ('Pimienta negra', 'ag', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Cocina la pasta según las instrucciones del paquete en una olla grande con agua hirviendo y sal. Coloca la crema para batir y la mantequilla en una sartén grande a fuego medio hasta que la crema burbujee y la mantequilla se derrita. Incorpora el queso parmesano batiendo y sazona con sal y pimienta negra. Deja que la salsa espese un poco, agrega la pasta y mezcla hasta que quede cubierta de salsa. Decora con perejil y está lista.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/0jv5gx1661040802.jpg', 'Fettuccine Alfredo', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/LPPcNPdq_j4', 'Video de preparación', FALSE FROM nueva;

-- 53147 · Arroz con gambas y calamar
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Arroz con langostinos y calamar',
           'Arroz estilo paella con azafrán, calamar y langostinos, cocinado en una sola sartén. Fuente: TheMealDB.',
           50, 4, 'API', '53147', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53147')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Langostinos', 'u', 24, NULL),
         ('Aceite de oliva', 'cda', 2, NULL),
         ('Cebolla', 'u', 1, 'pequeño'),
         ('Hoja de laurel', 'u', 1, NULL),
         ('Azafrán', 'ag', 1, 'pizca'),
         ('Arroz para paella', 'g', 450, NULL),
         ('Puré de tomate', 'cdta', 2, NULL),
         ('Vino blanco', 'ml', 200, NULL),
         ('Caldo de mariscos', 'ml', 650, NULL),
         ('Calamar', 'u', 3, 'mediano')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Pela y quita el hilo intestinal a la mayoría de los langostinos (la pescadería puede hacerlo por ti) y deja unos pocos enteros para decorar, si quieres. Calienta el aceite de oliva en una sartén grande o cacerola baja resistente al fuego a fuego medio-bajo y sofríe la cebolla 5 minutos hasta que se ablande. Agrega el laurel, el azafrán, el arroz y el puré de tomate, y cocina de 1 a 2 minutos más, revolviendo.'),
         (2, 'Vierte el vino y deja burbujear de 1 a 2 minutos; luego añade el caldo de mariscos y 150 ml de agua. Cocina 5 minutos, agrega el calamar, sazona bien y mezcla. Lleva a ebullición, tapa y baja a fuego muy suave. Cocina 12 minutos más, añadiendo un poco más de agua si la mezcla se ve seca.'),
         (3, 'Destapa y mezcla los langostinos pelados; coloca los enteros sobre el arroz. Tapa de nuevo y cocina a fuego suave de 5 a 6 minutos más, hasta que los langostinos estén rosados y bien cocidos. Deja reposar un par de minutos y sirve directamente de la sartén.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/jc6oub1763196663.jpg', 'Arroz con langostinos y calamar', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/nFFp0ebOlOo', 'Video de preparación', FALSE FROM nueva;

-- 53144 · Gambas al ajillo
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Gambas al ajillo',
           'Langostinos salteados en aceite de oliva con ajo y chile picante. Fuente: TheMealDB.',
           15, 2, 'API', '53144', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53144')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Langostinos', 'u', 12, NULL),
         ('Ajo', 'u', 3, NULL),
         ('Aceite de oliva', 'ml', 100, NULL),
         ('Chile picante', 'u', 3, NULL),
         ('Perejil', 'cda', 1, 'picado')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Pela los langostinos dejando las colas y, con un palillo, retírales el hilo intestinal. Si usas una sartén en lugar de una cazuela de barro, puedes cocinarlos con cáscara. Sazona con un poco de sal marina.'),
         (2, 'Pon el ajo, el aceite de oliva y los chiles en una cazuela de barro resistente al fuego o en una sartén a fuego alto. Cuando el ajo empiece a dorarse, agrega los langostinos y cocina de 1 a 2 minutos por lado, hasta que apenas estén rosados. Espolvorea con el perejil picado y un poco de pimienta negra recién molida y sirve de inmediato. Si usas una cazuela de barro, puedes llevarla directamente a la mesa, pero con cuidado: el aceite y el barro siguen calientes varios minutos.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/ze8uwg1763196123.jpg', 'Gambas al ajillo', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/2tZlO8vNMtk', 'Video de preparación', FALSE FROM nueva;

-- 53151 · Paella
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Difícil'),
           'Paella',
           'Paella con chorizo, calamar, langostinos, mejillones y azafrán, cocinada en una sola sartén ancha. Fuente: TheMealDB.',
           75, 4, 'API', '53151', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53151')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Aceite de oliva', 'cda', 3, NULL),
         ('Camarones tigre', 'u', 10, NULL),
         ('Perejil', 'ag', 1, 'un manojo'),
         ('Jerez seco', 'ml', 100, NULL),
         ('Mejillones', 'g', 500, NULL),
         ('Azafrán', 'ag', 1, 'una pizca'),
         ('Chorizo', 'g', 150, NULL),
         ('Cebolla', 'u', 1, 'picado'),
         ('Ajo', 'u', 3, 'dientes'),
         ('Calamar', 'u', 1, 'mediano'),
         ('Tomate', 'u', 2, NULL),
         ('Arroz para paella', 'g', 250, NULL),
         ('Habas', 'g', 100, NULL),
         ('Ralladura de limón', 'u', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Calienta 1 cucharada del aceite en una sartén ancha y baja. Agrega las cabezas de los langostinos y los tallos del perejil y fríe hasta que las cabezas se pongan rosadas; luego macháncalas con un prensapapas. Vierte el jerez (o vino) y 300 ml de agua, sazona con sal y cocina a fuego suave 10 minutos para hacer un caldo, machacando las cabezas mientras se cocinan.'),
         (2, 'Esparce los mejillones en la sartén, tápala sin ajustar con una tapa o un paño de cocina y ponla a fuego alto de 3 a 4 minutos, hasta que los mejillones apenas se abran. Revuelve para soltar sus jugos y vierte el contenido en un colador sobre un tazón grande que contenga el azafrán. Deja que el azafrán infusione en el caldo: necesitarás 700 ml en total, así que completa con agua si hace falta y revuelve bien. Saca los mejillones del colador y resérvalos.'),
         (3, 'Limpia la sartén y agrega el resto del aceite. Sofríe el chorizo hasta que suelte su grasa, añade la cebolla y el ajo y cocina hasta que se ablanden. Agrega el calamar y dale vuelta hasta que se ponga blanco. Añade los tomates y cocina un minuto; vierte la mayor parte del caldo, revuelve bien y lleva a ebullición. Esparce el arroz sobre el caldo, revuelve bien una sola vez y hierve con fuerza 5 minutos. Baja el fuego al mínimo y cocina lentamente 10 minutos sin revolver, hasta que el arroz absorba casi todo el líquido.'),
         (4, 'Acomoda las colas de los langostinos en el arroz y cocina 5 minutos, dándoles vuelta, hasta que estén cocidas. Mezcla los mejillones y las habas (o arvejas). Prueba el arroz: si aún está algo crudo pero la sartén está seca, agrega un chorrito más de caldo y sigue cocinando; si está muy caldoso, sube el fuego para evaporar el resto del caldo.'),
         (5, 'Cuando el arroz esté recién cocido, apaga el fuego y cubre con un paño de cocina unos minutos. Esparce las hojas de perejil y la ralladura de limón, y sazona con sal ahumada si quieres. Mezcla todo una sola vez y sirve directamente de la sartén, con rodajas de limón a un lado.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/9bl20p1763248192.jpg', 'Paella', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/vNBuXsTY15E', 'Video de preparación', FALSE FROM nueva;

-- 53528 · Chuletas de Cerdo a la Criolla (Pork Chops in Creole Sauce)
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Chuletas de cerdo a la criolla',
           'Chuletas marinadas con mostaza, comino y ajo, guisadas con cebolla y tomate. Fuente: TheMealDB.',
           45, 4, 'API', '53528', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53528')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Chuletas de cerdo', 'u', 4, NULL),
         ('Aceite vegetal', 'cda', 2, NULL),
         ('Ajo', 'u', 3, 'dientes, machacado'),
         ('Mostaza', 'ag', 1, 'cantidad suficiente para cubrir las chuletas'),
         ('Comino', 'cda', 1, NULL),
         ('Tomate', 'u', 3, 'en cubos'),
         ('Cebolla', 'u', 2, 'en rodajas'),
         ('Culantro', 'tz', 0.25, NULL),
         ('Sal', 'ag', 1, NULL),
         ('Pimienta negra', 'ag', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Coloca las chuletas en una bolsa plástica con cierre. Agrega la mostaza, el comino, el ajo, la sal y la pimienta.'),
         (2, 'Refrigera 1 hora o toda la noche, procurando que las chuletas queden cubiertas de manera pareja.'),
         (3, 'En una sartén grande, calienta el aceite a fuego medio. Coloca las chuletas y cocina 3 minutos por lado. Retira la carne de la sartén y resérvala en un plato caliente.'),
         (4, 'Agrega las cebollas y los tomates a la misma sartén donde se doró la carne, con una cucharada más de aceite si hace falta. Cocina a fuego alto unos 5 a 7 minutos, revolviendo seguido para que no se quemen, hasta que estén dorados y blandos. Devuelve las chuletas, tapa, baja el fuego a medio-bajo y cocina a fuego suave hasta que la carne esté tierna y apenas rosada en el centro, unos 15 minutos. Reparte las chuletas en los platos, baña con la salsa, agrega el culantro y sirve de inmediato.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/087fu71783802698.jpg', 'Chuletas de cerdo a la criolla', TRUE FROM nueva;

-- 53136 · Choripán
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Choripán',
           'Sándwich argentino de chorizo a la parrilla con salsa chimichurri. Fuente: TheMealDB.',
           25, 4, 'API', '53136', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53136')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Chorizo', 'u', 4, NULL),
         ('Pan rústico', 'u', 4, NULL),
         ('Salsa chimichurri', 'ag', 1, 'un chorrito')
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Asa los chorizos: cocínalos en la parrilla o en una sartén hasta que estén bien cocidos, de 10 a 15 minutos.'),
         (2, 'Prepara los panes: corta los panes y tuéstalos ligeramente en la parrilla o en una sartén.'),
         (3, 'Arma el sándwich: corta cada chorizo a lo largo y colócalo en un pan. Cubre con una buena cantidad de salsa chimichurri.'),
         (4, 'Sirve: disfruta de inmediato, bien caliente.'),
         (5, 'Consejos: asa el chorizo lentamente a fuego medio para que la piel no reviente y se cocine parejo por dentro. Tuesta el pan en la parrilla para que absorba parte del sabor del chorizo.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/5i25sg1763075353.jpg', 'Choripán', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/FNrU5mQKFOE', 'Video de preparación', FALSE FROM nueva;

-- 53134 · Empanadas
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Intermedio'),
           'Empanadas argentinas',
           'Empanadas horneadas rellenas de carne molida, cebolla, huevo y aceitunas. Fuente: TheMealDB.',
           80, 12, 'API', '53134', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53134')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Harina', 'g', 500, NULL),
         ('Mantequilla', 'g', 200, NULL),
         ('Agua', 'ml', 120, NULL),
         ('Sal', 'cdta', 1, NULL),
         ('Carne molida', 'g', 500, NULL),
         ('Cebolla', 'u', 2, 'picado'),
         ('Huevo', 'u', 4, 'picado'),
         ('Aceitunas verdes', 'g', 100, NULL),
         ('Comino', 'cdta', 1, NULL),
         ('Paprika', 'cdta', 1, NULL),
         ('Pimienta', 'ag', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Prepara la masa: mezcla la harina y la sal en un tazón grande. Agrega la mantequilla y mézclala con los dedos hasta obtener una textura arenosa. Añade el agua poco a poco, mezclando hasta formar una masa. Envuelve y refrigera 30 minutos.'),
         (2, 'Prepara el relleno: cocina las cebollas en una sartén hasta que estén transparentes. Agrega la carne molida y cocina hasta que se dore. Incorpora las especias y retira del fuego. Una vez fría, mezcla con los huevos y las aceitunas.'),
         (3, 'Arma las empanadas: estira la masa y corta círculos. Coloca una cucharada de relleno en cada uno, dobla y sella los bordes.'),
         (4, 'Cocina: hornea a 200 °C (400 °F) de 20 a 25 minutos, o hasta que estén doradas.'),
         (5, 'Consejos: para una masa más hojaldrada, agrega una cucharada de vinagre a la mezcla; así se evita que se forme el gluten. Sella los bordes con un tenedor para que no se abran al hornear o freír.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/q99te31763075494.jpg', 'Empanadas argentinas', TRUE FROM nueva
UNION ALL
SELECT receta_id, 'VIDEO', 'https://www.youtube.com/embed/RRNVLC6IAv4', 'Video de preparación', FALSE FROM nueva;

-- 53521 · Porotos Granados (Chilean Bean Stew)
WITH nueva AS (
    INSERT INTO recetas (usuario_id, categoria_id, dificultad_id, titulo, descripcion,
                         tiempo_prep_min, porciones_base, origen, api_referencia_id, estado)
    SELECT (SELECT usuario_id FROM usuarios WHERE correo = 'admin@recetas.com'),
           (SELECT categoria_id FROM categorias WHERE nombre = 'Platos fuertes'),
           (SELECT dificultad_id FROM niveles_dificultad WHERE nombre = 'Fácil'),
           'Porotos granados (guiso chileno de frijoles)',
           'Guiso espeso chileno de frijoles, calabaza y maíz con albahaca. Fuente: TheMealDB.',
           60, 4, 'API', '53521', 'PUBLICADA'
    WHERE NOT EXISTS (SELECT 1 FROM recetas WHERE api_referencia_id = '53521')
    RETURNING receta_id
), ingr AS (
    INSERT INTO receta_ingredientes (receta_id, ingrediente_id, unidad_id, cantidad, nota)
    SELECT n.receta_id, i.ingrediente_id, u.unidad_id, v.cantidad::numeric(8,2), v.nota
    FROM nueva n
    CROSS JOIN (VALUES
         ('Aceite de oliva', 'cda', 1, NULL),
         ('Cebolla', 'u', 1, NULL),
         ('Calabaza', 'tz', 2, NULL),
         ('Frijoles blancos', 'u', 1, 'en lata'),
         ('Frijoles lima', 'tz', 1, NULL),
         ('Caldo de pollo', 'tz', 3, NULL),
         ('Maíz dulce', 'tz', 2, NULL),
         ('Albahaca', 'cda', 2, 'picado'),
         ('Chile banana', 'u', 1, NULL)
    ) AS v(nombre, unidad, cantidad, nota)
    JOIN ingredientes i    ON LOWER(i.nombre) = LOWER(v.nombre)
    JOIN unidades_medida u ON u.abreviatura = v.unidad
    RETURNING 1
), pasos AS (
    INSERT INTO receta_pasos (receta_id, numero_paso, descripcion)
    SELECT n.receta_id, v.numero::smallint, v.descripcion
    FROM nueva n
    CROSS JOIN (VALUES
         (1, 'Calienta el aceite de oliva en una olla a fuego medio; agrega la cebolla y tapa. Cocina hasta que esté suave y transparente, unos 5 minutos. Añade la calabaza, los frijoles blancos y los frijoles lima, y vierte el caldo de pollo encima. Tapa y cocina hasta que la calabaza esté tierna y empiece a deshacerse, de 30 a 45 minutos.'),
         (2, 'Incorpora el maíz y la albahaca; cocina hasta que el guiso tenga la consistencia de masa de panqueques, unos 10 minutos más. Espolvorea el chile banana picado sobre cada porción al servir.')
    ) AS v(numero, descripcion)
    RETURNING 1
)
INSERT INTO receta_multimedia (receta_id, tipo, ruta, descripcion, es_portada)
SELECT receta_id, 'IMAGEN', 'https://www.themealdb.com/images/media/meals/nimml81782855200.jpg', 'Porotos granados (guiso chileno de frijoles)', TRUE FROM nueva;

-- Verificación: debe devolver 40
-- SELECT count(*) FROM recetas WHERE origen = 'API';
