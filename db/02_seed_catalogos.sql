-- =====================================================================
--  Usuarios de prueba (hash bcrypt real, costo 10):
--    admin@recetas.com  / Admin123*    (ADMIN)
--    maria@correo.com   / Maria123*    (USUARIO)
--    jose@correo.com    / Jose123*     (USUARIO)
--  Son credenciales de desarrollo: cambiarlas si la BD se publica.
-- =====================================================================

SET timezone = 'America/Costa_Rica';

INSERT INTO roles (nombre, descripcion) VALUES
('ADMIN',   'Administra recetas y noticias, y accede a reportería'),
('USUARIO', 'Consulta recetas, da me gusta, califica, comparte y genera listas de compras');

INSERT INTO categorias (nombre, descripcion) VALUES
('Desayunos','Recetas para empezar el día'),
('Platos fuertes','Almuerzos y cenas'),
('Postres','Dulces y repostería'),
('Bebidas','Jugos, batidos y cocteles sin alcohol'),
('Vegetariano','Recetas sin carne'),
('Comida típica','Recetas tradicionales costarricenses');

INSERT INTO niveles_dificultad (nombre) VALUES ('Fácil'),('Intermedio'),('Difícil');

-- "al gusto" no se multiplica al cambiar las porciones (escalable = FALSE)
INSERT INTO unidades_medida (nombre, abreviatura, escalable) VALUES
('gramos','g',TRUE),('kilogramos','kg',TRUE),('mililitros','ml',TRUE),('litros','l',TRUE),
('tazas','tz',TRUE),('cucharadas','cda',TRUE),('cucharaditas','cdta',TRUE),('unidades','u',TRUE),
('al gusto','ag',FALSE);

INSERT INTO redes_sociales (nombre) VALUES ('Facebook'),('WhatsApp'),('X'),('Instagram');

INSERT INTO usuarios (rol_id, nombre, apellidos, correo, password_hash) VALUES
(1,'Admin','Sistema','admin@recetas.com','$2b$10$tY.7bqlx.mAft2B1QEBm5eJlrc14TCJ9CQc2ZOcFV5BVBjsRL2p/S'),
(2,'María','Rodríguez','maria@correo.com','$2b$10$B3yrBRU8lGDHe3fqDeZpr.huDZfbkFgA.5bHu6e/8ZsGqRK8EYPOO'),
(2,'José','Vargas','jose@correo.com','$2b$10$XE4hxpqTAIhP0a1Y0FauFO9x70FUpqiYUjL3xZxpVpbkyf0sdkncK');
