-- Demo de produccion DERCAS. Idempotente por NIT, correo y numero_serie.
-- No modifica el diagrama. Requiere Scripts/SeedCatalogos.sql ya ejecutado.

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

-- Contrasenas (Argon2id del PasswordHashService):
-- Admin general:  CambiarYa_Dercas1!
-- Admin empresa:  Empresa_Dercas1!
-- Operador:       Operador_Dercas1!
-- Consulta:       Consulta_Dercas1!

DECLARE @hashAdminGeneral varchar(255) = N'$argon2id$v=19$m=65536,t=2,p=1$RjvcOLoeJMAlXqSBZpx3Dw==$2hFAcukbByeHVb9wW4gvK1ywxnqjTP37MeSaNBWMAio=';
DECLARE @hashAdminEmpresa varchar(255) = N'$argon2id$v=19$m=65536,t=2,p=1$wZ7WdmP9SnXCwct9J2eu8A==$2QtNbRMD3rSoemOKQ5PFlUAnyPO4H6zqgn9GSYRO5zw=';
DECLARE @hashOperador varchar(255) = N'$argon2id$v=19$m=65536,t=2,p=1$stwPnnT1wwTTax7XwET13w==$IY8rDsnACxG4FBwYzqhH8KsT42b+2Q9EGNci9rdNoLQ=';
DECLARE @hashConsulta varchar(255) = N'$argon2id$v=19$m=65536,t=2,p=1$QC79GCStwZs/DlOtoCY+Mw==$/YITCKngoOz3LvzoPTnyaHWHn74W6RjoJ6U8zHg2tjE=';

DECLARE @idPais int = (SELECT TOP 1 id_pais FROM pais WHERE codigo_iso2 = N'GT');
IF @idPais IS NULL
BEGIN
    INSERT INTO pais (nombre, codigo_iso2, codigo_iso3, codigo_telefonico)
    VALUES (N'Guatemala', N'GT', N'GTM', N'+502');
    SET @idPais = SCOPE_IDENTITY();
END;

IF NOT EXISTS (SELECT 1 FROM categoria_activo WHERE nombre = N'Impresora')
    INSERT INTO categoria_activo (nombre, descripcion, habilitado) VALUES (N'Impresora', N'Impresoras y multifuncionales', 1);
IF NOT EXISTS (SELECT 1 FROM categoria_activo WHERE nombre = N'Telefono')
    INSERT INTO categoria_activo (nombre, descripcion, habilitado) VALUES (N'Telefono', N'Telefonia IP y celulares corporativos', 1);
IF NOT EXISTS (SELECT 1 FROM categoria_activo WHERE nombre = N'Servidor')
    INSERT INTO categoria_activo (nombre, descripcion, habilitado) VALUES (N'Servidor', N'Servidores y NAS', 1);
IF NOT EXISTS (SELECT 1 FROM categoria_activo WHERE nombre = N'Mobiliario')
    INSERT INTO categoria_activo (nombre, descripcion, habilitado) VALUES (N'Mobiliario', N'Escritorios, sillas y archiveros', 1);

DECLARE @catLaptop int = (SELECT id_categoria FROM categoria_activo WHERE nombre = N'Laptop');
DECLARE @catMonitor int = (SELECT id_categoria FROM categoria_activo WHERE nombre = N'Monitor');
DECLARE @catImpresora int = (SELECT id_categoria FROM categoria_activo WHERE nombre = N'Impresora');
DECLARE @catTelefono int = (SELECT id_categoria FROM categoria_activo WHERE nombre = N'Telefono');
DECLARE @catServidor int = (SELECT id_categoria FROM categoria_activo WHERE nombre = N'Servidor');
DECLARE @catMobiliario int = (SELECT id_categoria FROM categoria_activo WHERE nombre = N'Mobiliario');

UPDATE empresa SET
    direccion = N'Calzada Roosevelt 14-46, zona 11, Guatemala',
    telefono = N'2208-9000'
WHERE nit_codigo = N'1234567-8';

UPDATE empresa SET
    direccion = N'Km 16.5 Carretera a El Salvador, Fraijanes',
    telefono = N'2473-1000'
WHERE nit_codigo = N'8578484';

UPDATE empresa SET
    direccion = N'Avenida Las Americas 10-50, zona 13',
    telefono = N'2379-4000'
WHERE nit_codigo = N'324';

DECLARE @idSlc int = (SELECT id_empresa FROM empresa WHERE nit_codigo = N'1234567-8');
DECLARE @idPin int = (SELECT id_empresa FROM empresa WHERE nit_codigo = N'8578484');
DECLARE @idSam int = (SELECT id_empresa FROM empresa WHERE nit_codigo = N'324');

IF NOT EXISTS (SELECT 1 FROM sede WHERE nombre = N'Bodega Mixco' AND id_empresa = @idSlc)
    INSERT INTO sede (id_empresa, id_pais, nombre, direccion, ciudad, habilitado)
    VALUES (@idSlc, @idPais, N'Bodega Mixco', N'Km 16.5 Carretera Interamericana', N'Mixco', 1);

IF NOT EXISTS (SELECT 1 FROM sede WHERE nombre = N'Planta Villa Nueva' AND id_empresa = @idPin)
    INSERT INTO sede (id_empresa, id_pais, nombre, direccion, ciudad, habilitado)
    VALUES (@idPin, @idPais, N'Planta Villa Nueva', N'4a calle 8-21 zona 4', N'Villa Nueva', 1);

IF NOT EXISTS (SELECT 1 FROM sede WHERE nombre = N'Oficinas Zona 10' AND id_empresa = @idPin)
    INSERT INTO sede (id_empresa, id_pais, nombre, direccion, ciudad, habilitado)
    VALUES (@idPin, @idPais, N'Oficinas Zona 10', N'15 avenida 5-10 zona 10', N'Guatemala', 1);

IF NOT EXISTS (SELECT 1 FROM sede WHERE nombre = N'Campus Cayala' AND id_empresa = @idSam)
    INSERT INTO sede (id_empresa, id_pais, nombre, direccion, ciudad, habilitado)
    VALUES (@idSam, @idPais, N'Campus Cayala', N'Blvd. Austriaco 40-05 zona 16', N'Guatemala', 1);

DECLARE @sedeCentral int = (SELECT id_sede FROM sede WHERE nombre = N'Central' AND id_empresa = @idSlc);
DECLARE @sedeAvia int = (SELECT id_sede FROM sede WHERE nombre = N'AVIA' AND id_empresa = @idSlc);
DECLARE @sedeMixco int = (SELECT id_sede FROM sede WHERE nombre = N'Bodega Mixco' AND id_empresa = @idSlc);
DECLARE @sedePlanta int = (SELECT id_sede FROM sede WHERE nombre = N'Planta Villa Nueva' AND id_empresa = @idPin);
DECLARE @sedeZ10 int = (SELECT id_sede FROM sede WHERE nombre = N'Oficinas Zona 10' AND id_empresa = @idPin);
DECLARE @sedeCayala int = (SELECT id_sede FROM sede WHERE nombre = N'Campus Cayala' AND id_empresa = @idSam);

IF NOT EXISTS (SELECT 1 FROM area WHERE nombre = N'Finanzas' AND id_sede = @sedeCentral)
    INSERT INTO area (id_sede, nombre, descripcion, habilitado) VALUES (@sedeCentral, N'Finanzas', N'Contabilidad y tesoreria', 1);
IF NOT EXISTS (SELECT 1 FROM area WHERE nombre = N'Logistica' AND id_sede = @sedeMixco)
    INSERT INTO area (id_sede, nombre, descripcion, habilitado) VALUES (@sedeMixco, N'Logistica', N'Bodega y despacho', 1);
IF NOT EXISTS (SELECT 1 FROM area WHERE nombre = N'Produccion' AND id_sede = @sedePlanta)
    INSERT INTO area (id_sede, nombre, descripcion, habilitado) VALUES (@sedePlanta, N'Produccion', N'Lineas de envasado', 1);
IF NOT EXISTS (SELECT 1 FROM area WHERE nombre = N'Mantenimiento planta' AND id_sede = @sedePlanta)
    INSERT INTO area (id_sede, nombre, descripcion, habilitado) VALUES (@sedePlanta, N'Mantenimiento planta', N'Taller electrico y mecanico', 1);
IF NOT EXISTS (SELECT 1 FROM area WHERE nombre = N'Administracion' AND id_sede = @sedeZ10)
    INSERT INTO area (id_sede, nombre, descripcion, habilitado) VALUES (@sedeZ10, N'Administracion', N'Gerencia general Pinulito', 1);
IF NOT EXISTS (SELECT 1 FROM area WHERE nombre = N'Comercial' AND id_sede = @sedeCayala)
    INSERT INTO area (id_sede, nombre, descripcion, habilitado) VALUES (@sedeCayala, N'Comercial', N'Ventas Samsung', 1);

DECLARE @areaTI int = (SELECT id_area FROM area WHERE nombre = N'TI' AND id_sede = @sedeCentral);
DECLARE @areaFinanzas int = (SELECT id_area FROM area WHERE nombre = N'Finanzas' AND id_sede = @sedeCentral);
DECLARE @areaLogistica int = (SELECT id_area FROM area WHERE nombre = N'Logistica' AND id_sede = @sedeMixco);
DECLARE @areaProd int = (SELECT id_area FROM area WHERE nombre = N'Produccion' AND id_sede = @sedePlanta);
DECLARE @areaMant int = (SELECT id_area FROM area WHERE nombre = N'Mantenimiento planta' AND id_sede = @sedePlanta);
DECLARE @areaAdmPin int = (SELECT id_area FROM area WHERE nombre = N'Administracion' AND id_sede = @sedeZ10);
DECLARE @areaComSam int = (SELECT id_area FROM area WHERE nombre = N'Comercial' AND id_sede = @sedeCayala);

IF NOT EXISTS (SELECT 1 FROM ubicacion WHERE nombre = N'Sala de servidores' AND id_sede = @sedeCentral)
    INSERT INTO ubicacion (id_sede, nombre, descripcion, latitud, longitud, habilitado)
    VALUES (@sedeCentral, N'Sala de servidores', N'Rack principal TI', 14.634900, -90.506900, 1);
IF NOT EXISTS (SELECT 1 FROM ubicacion WHERE nombre = N'Bodega A' AND id_sede = @sedeMixco)
    INSERT INTO ubicacion (id_sede, nombre, descripcion, latitud, longitud, habilitado)
    VALUES (@sedeMixco, N'Bodega A', N'Recepcion de mercaderia', 14.633200, -90.606400, 1);
IF NOT EXISTS (SELECT 1 FROM ubicacion WHERE nombre = N'Linea 1' AND id_sede = @sedePlanta)
    INSERT INTO ubicacion (id_sede, nombre, descripcion, latitud, longitud, habilitado)
    VALUES (@sedePlanta, N'Linea 1', N'Envasado primario', 14.526100, -90.587500, 1);
IF NOT EXISTS (SELECT 1 FROM ubicacion WHERE nombre = N'Taller electrico' AND id_sede = @sedePlanta)
    INSERT INTO ubicacion (id_sede, nombre, descripcion, latitud, longitud, habilitado)
    VALUES (@sedePlanta, N'Taller electrico', N'Repuestos y herramientas', 14.526400, -90.587800, 1);
IF NOT EXISTS (SELECT 1 FROM ubicacion WHERE nombre = N'Gerencia' AND id_sede = @sedeZ10)
    INSERT INTO ubicacion (id_sede, nombre, descripcion, latitud, longitud, habilitado)
    VALUES (@sedeZ10, N'Gerencia', N'Oficina direccion', 14.598600, -90.514800, 1);
IF NOT EXISTS (SELECT 1 FROM ubicacion WHERE nombre = N'Showroom' AND id_sede = @sedeCayala)
    INSERT INTO ubicacion (id_sede, nombre, descripcion, latitud, longitud, habilitado)
    VALUES (@sedeCayala, N'Showroom', N'Sala de exhibicion', 14.613400, -90.485900, 1);

DECLARE @ubOficina1 int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Oficina 1' AND id_sede = @sedeCentral);
DECLARE @ubServidores int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Sala de servidores' AND id_sede = @sedeCentral);
DECLARE @ubRh int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Oficinas de RH' AND id_sede = @sedeAvia);
DECLARE @ubBodegaA int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Bodega A' AND id_sede = @sedeMixco);
DECLARE @ubLinea1 int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Linea 1' AND id_sede = @sedePlanta);
DECLARE @ubTaller int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Taller electrico' AND id_sede = @sedePlanta);
DECLARE @ubGerencia int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Gerencia' AND id_sede = @sedeZ10);
DECLARE @ubShowroom int = (SELECT id_ubicacion FROM ubicacion WHERE nombre = N'Showroom' AND id_sede = @sedeCayala);

IF NOT EXISTS (SELECT 1 FROM proveedor WHERE nit = N'456789-1' AND id_empresa = @idSlc)
    INSERT INTO proveedor (id_empresa, nombre, nit, nombre_contacto, telefono, correo, habilitado)
    VALUES (@idSlc, N'Tecnologias GT', N'456789-1', N'Luis Pineda', N'2278-4411', N'ventas@tecgt.com', 1);
IF NOT EXISTS (SELECT 1 FROM proveedor WHERE nit = N'998877-3' AND id_empresa = @idPin)
    INSERT INTO proveedor (id_empresa, nombre, nit, nombre_contacto, telefono, correo, habilitado)
    VALUES (@idPin, N'Insumos Industriales', N'998877-3', N'Carmen Soto', N'2473-2200', N'compras@insumosind.gt', 1);
IF NOT EXISTS (SELECT 1 FROM proveedor WHERE nit = N'112233-7' AND id_empresa = @idPin)
    INSERT INTO proveedor (id_empresa, nombre, nit, nombre_contacto, telefono, correo, habilitado)
    VALUES (@idPin, N'HP Guatemala', N'112233-7', N'Roberto Diaz', N'2385-0100', N'hp@canal.gt', 1);
IF NOT EXISTS (SELECT 1 FROM proveedor WHERE nit = N'667788-0' AND id_empresa = @idSam)
    INSERT INTO proveedor (id_empresa, nombre, nit, nombre_contacto, telefono, correo, habilitado)
    VALUES (@idSam, N'Samsung Electronics CA', N'667788-0', N'Paola Ruiz', N'2379-4100', N'paola.ruiz@samsung.com', 1);

DECLARE @provDell1 int = (SELECT TOP 1 id_proveedor FROM proveedor WHERE id_empresa = @idSlc AND nombre = N'Dell' ORDER BY id_proveedor);
DECLARE @provTec int = (SELECT id_proveedor FROM proveedor WHERE nit = N'456789-1');
DECLARE @provIns int = (SELECT id_proveedor FROM proveedor WHERE nit = N'998877-3');
DECLARE @provHp int = (SELECT id_proveedor FROM proveedor WHERE nit = N'112233-7');
DECLARE @provSam int = (SELECT id_proveedor FROM proveedor WHERE nit = N'667788-0');

IF NOT EXISTS (SELECT 1 FROM responsable WHERE correo = N'marta.lopez@slctrade.com')
    INSERT INTO responsable (id_area, nombre_completo, cargo, correo, telefono, habilitado)
    VALUES (@areaFinanzas, N'Marta Lopez', N'Contadora general', N'marta.lopez@slctrade.com', N'5510-2201', 1);
IF NOT EXISTS (SELECT 1 FROM responsable WHERE correo = N'jose.ramirez@slctrade.com')
    INSERT INTO responsable (id_area, nombre_completo, cargo, correo, telefono, habilitado)
    VALUES (@areaLogistica, N'Jose Ramirez', N'Jefe de bodega', N'jose.ramirez@slctrade.com', N'5510-3344', 1);
IF NOT EXISTS (SELECT 1 FROM responsable WHERE correo = N'lucia.mejia@pinulito.com')
    INSERT INTO responsable (id_area, nombre_completo, cargo, correo, telefono, habilitado)
    VALUES (@areaProd, N'Lucia Mejia', N'Supervisora de linea', N'lucia.mejia@pinulito.com', N'5422-1100', 1);
IF NOT EXISTS (SELECT 1 FROM responsable WHERE correo = N'pedro.castillo@pinulito.com')
    INSERT INTO responsable (id_area, nombre_completo, cargo, correo, telefono, habilitado)
    VALUES (@areaMant, N'Pedro Castillo', N'Jefe de mantenimiento', N'pedro.castillo@pinulito.com', N'5422-1188', 1);
IF NOT EXISTS (SELECT 1 FROM responsable WHERE correo = N'sofia.herrera@pinulito.com')
    INSERT INTO responsable (id_area, nombre_completo, cargo, correo, telefono, habilitado)
    VALUES (@areaAdmPin, N'Sofia Herrera', N'Gerente administrativa', N'sofia.herrera@pinulito.com', N'2331-9090', 1);
IF NOT EXISTS (SELECT 1 FROM responsable WHERE correo = N'andres.choj@samsung.com')
    INSERT INTO responsable (id_area, nombre_completo, cargo, correo, telefono, habilitado)
    VALUES (@areaComSam, N'Andres Choj', N'Ejecutivo comercial', N'andres.choj@samsung.com', N'4010-7788', 1);

DECLARE @respAngel int = (SELECT TOP 1 id_responsable FROM responsable WHERE id_area = @areaTI ORDER BY id_responsable);
DECLARE @respMarta int = (SELECT id_responsable FROM responsable WHERE correo = N'marta.lopez@slctrade.com');
DECLARE @respJose int = (SELECT id_responsable FROM responsable WHERE correo = N'jose.ramirez@slctrade.com');
DECLARE @respLucia int = (SELECT id_responsable FROM responsable WHERE correo = N'lucia.mejia@pinulito.com');
DECLARE @respPedro int = (SELECT id_responsable FROM responsable WHERE correo = N'pedro.castillo@pinulito.com');
DECLARE @respSofia int = (SELECT id_responsable FROM responsable WHERE correo = N'sofia.herrera@pinulito.com');
DECLARE @respAndres int = (SELECT id_responsable FROM responsable WHERE correo = N'andres.choj@samsung.com');

UPDATE usuario SET password_hash = @hashAdminGeneral, habilitado = 1 WHERE correo IN (N'admin@localhost', N'ana@empresa.com');
UPDATE usuario SET password_hash = @hashAdminEmpresa, habilitado = 1 WHERE correo IN (N'admin.empresa@slctrade.com', N'bargueta@slc.com.gt');
UPDATE usuario SET password_hash = @hashOperador, habilitado = 1 WHERE correo = N'operador@slctrade.com';
UPDATE usuario SET password_hash = @hashConsulta, habilitado = 1 WHERE correo = N'jmendez@slc.com.gt';

IF NOT EXISTS (SELECT 1 FROM usuario WHERE correo = N'consulta@slctrade.com')
    INSERT INTO usuario (id_empresa, nombres, apellidos, correo, username, password_hash, rol, fecha_creacion, habilitado)
    VALUES (@idSlc, N'Karla', N'Morales', N'consulta@slctrade.com', N'kmorales', @hashConsulta, N'Consulta', SYSUTCDATETIME(), 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE correo = N'admin.empresa@pinulito.com')
    INSERT INTO usuario (id_empresa, nombres, apellidos, correo, username, password_hash, rol, fecha_creacion, habilitado)
    VALUES (@idPin, N'Diego', N'Barrios', N'admin.empresa@pinulito.com', N'dbarrios', @hashAdminEmpresa, N'AdministradorEmpresa', SYSUTCDATETIME(), 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE correo = N'operador@pinulito.com')
    INSERT INTO usuario (id_empresa, nombres, apellidos, correo, username, password_hash, rol, fecha_creacion, habilitado)
    VALUES (@idPin, N'Elena', N'Ruano', N'operador@pinulito.com', N'eruano', @hashOperador, N'OperadorInventario', SYSUTCDATETIME(), 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE correo = N'consulta@pinulito.com')
    INSERT INTO usuario (id_empresa, nombres, apellidos, correo, username, password_hash, rol, fecha_creacion, habilitado)
    VALUES (@idPin, N'Julio', N'Mendez', N'consulta@pinulito.com', N'jmendez.pin', @hashConsulta, N'Consulta', SYSUTCDATETIME(), 1);
IF NOT EXISTS (SELECT 1 FROM usuario WHERE correo = N'operador@samsung.com')
    INSERT INTO usuario (id_empresa, nombres, apellidos, correo, username, password_hash, rol, fecha_creacion, habilitado)
    VALUES (@idSam, N'Paola', N'Ruiz', N'operador@samsung.com', N'pruiz', @hashOperador, N'OperadorInventario', SYSUTCDATETIME(), 1);

DECLARE @usrAdminSlc int = (SELECT id_usuario FROM usuario WHERE correo = N'admin.empresa@slctrade.com');
DECLARE @usrOpSlc int = (SELECT id_usuario FROM usuario WHERE correo = N'operador@slctrade.com');
DECLARE @usrOpPin int = (SELECT id_usuario FROM usuario WHERE correo = N'operador@pinulito.com');
DECLARE @usrOpSam int = (SELECT id_usuario FROM usuario WHERE correo = N'operador@samsung.com');

DECLARE @tipoAsig int = (SELECT id_tipo_asignacion FROM tipo_asignacion WHERE nombre = N'Asignacion');
DECLARE @tipoTras int = (SELECT id_tipo_asignacion FROM tipo_asignacion WHERE nombre = N'Traslado');
DECLARE @tipoMant int = (SELECT id_tipo_asignacion FROM tipo_asignacion WHERE nombre = N'Mantenimiento');
DECLARE @tipoBaja int = (SELECT id_tipo_asignacion FROM tipo_asignacion WHERE nombre = N'Baja');
DECLARE @estAsig int = (SELECT id_estado FROM estado WHERE nombre = N'Asignado');
DECLARE @estTras int = (SELECT id_estado FROM estado WHERE nombre = N'Traslado');
DECLARE @estMant int = (SELECT id_estado FROM estado WHERE nombre = N'Mantenimiento');
DECLARE @estBaja int = (SELECT id_estado FROM estado WHERE nombre = N'Baja');

IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SLC-LT-1044')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catLaptop, @provDell1, @ubOficina1, N'Laptop Dell Latitude 5540', N'Equipo de gerencia financiera', N'Dell', N'Latitude 5540', N'SLC-LT-1044', '2024-03-12', 9850.00, N'GTQ', N'F-44120', '2027-03-12', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SLC-MON-2201')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catMonitor, @provTec, @ubOficina1, N'Monitor Dell 27 P2723D', N'Monitor dual finanzas', N'Dell', N'P2723D', N'SLC-MON-2201', '2024-03-12', 2100.00, N'GTQ', N'F-44121', '2027-03-12', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SLC-PR-3308')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catImpresora, @provTec, @ubOficina1, N'Impresora HP LaserJet Pro', N'Impresora compartida piso 1', N'HP', N'M404dn', N'SLC-PR-3308', '2023-11-02', 3200.00, N'GTQ', N'F-39811', '2026-11-02', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SLC-SV-010')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catServidor, @provDell1, @ubServidores, N'Servidor Dell PowerEdge R750', N'Hypervisor de aplicativos', N'Dell', N'PowerEdge R750', N'SLC-SV-010', '2023-06-20', 48500.00, N'GTQ', N'F-30102', '2026-06-20', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SLC-TEL-551')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catTelefono, @provTec, @ubBodegaA, N'Telefono IP Yealink T46U', N'Sin asignar, stock bodega', N'Yealink', N'T46U', N'SLC-TEL-551', '2025-01-18', 890.00, N'GTQ', N'F-51002', '2027-01-18', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SLC-ESC-77')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catMobiliario, @provTec, @ubBodegaA, N'Escritorio operativo 160cm', N'Mobiliario disponible', N'OffiMax', N'OP-160', N'SLC-ESC-77', '2024-08-01', 1450.00, N'GTQ', N'F-48011', '2026-08-01', N'Demo produccion');

IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'PIN-LT-200')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catLaptop, @provHp, @ubGerencia, N'Laptop HP EliteBook 840', N'Equipo de gerencia Pinulito', N'HP', N'EliteBook 840 G10', N'PIN-LT-200', '2024-05-09', 11200.00, N'GTQ', N'P-2201', '2027-05-09', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'PIN-SCAN-01')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catImpresora, @provIns, @ubLinea1, N'Impresora industrial Zebra', N'Etiquetado de linea 1', N'Zebra', N'ZT411', N'PIN-SCAN-01', '2022-09-15', 15800.00, N'GTQ', N'P-1888', '2025-09-15', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'PIN-MON-88')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catMonitor, @provHp, @ubTaller, N'Monitor industrial 24', N'Puesto de taller', N'HP', N'P24h G5', N'PIN-MON-88', '2024-02-01', 1750.00, N'GTQ', N'P-2104', '2027-02-01', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'PIN-TEL-12')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catTelefono, @provHp, @ubGerencia, N'Smartphone corporativo A35', N'Stock gerencia', N'Samsung', N'Galaxy A35', N'PIN-TEL-12', '2025-04-22', 2100.00, N'GTQ', N'P-2409', '2026-10-22', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'PIN-BAJA-9')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catLaptop, @provHp, @ubTaller, N'Laptop HP ProBook 450 (baja)', N'Equipo dado de baja por dano', N'HP', N'ProBook 450 G8', N'PIN-BAJA-9', '2021-03-10', 6400.00, N'GTQ', N'P-0901', '2023-03-10', N'Demo produccion');

IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SAM-TAB-01')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catTelefono, @provSam, @ubShowroom, N'Tablet Galaxy Tab S9', N'Demo de piso showroom', N'Samsung', N'Tab S9', N'SAM-TAB-01', '2024-10-01', 7800.00, N'GTQ', N'S-1001', '2026-10-01', N'Demo produccion');
IF NOT EXISTS (SELECT 1 FROM activo WHERE numero_serie = N'SAM-LT-77')
    INSERT INTO activo (id_categoria, id_proveedor, id_ubicacion, nombre, descripcion, marca, modelo, numero_serie, fecha_compra, costo_adquisicion, moneda, numero_factura, fecha_vencimiento_garantia, observaciones)
    VALUES (@catLaptop, @provSam, @ubShowroom, N'Laptop Galaxy Book4', N'Equipo de ejecutivo comercial', N'Samsung', N'Galaxy Book4', N'SAM-LT-77', '2024-11-12', 9500.00, N'GTQ', N'S-1044', '2027-11-12', N'Demo produccion');

DECLARE @aLat int = (SELECT id_activo FROM activo WHERE numero_serie = N'SLC-LT-1044');
DECLARE @aMon int = (SELECT id_activo FROM activo WHERE numero_serie = N'SLC-MON-2201');
DECLARE @aPr int = (SELECT id_activo FROM activo WHERE numero_serie = N'SLC-PR-3308');
DECLARE @aSv int = (SELECT id_activo FROM activo WHERE numero_serie = N'SLC-SV-010');
DECLARE @aPinLt int = (SELECT id_activo FROM activo WHERE numero_serie = N'PIN-LT-200');
DECLARE @aPinScan int = (SELECT id_activo FROM activo WHERE numero_serie = N'PIN-SCAN-01');
DECLARE @aPinMon int = (SELECT id_activo FROM activo WHERE numero_serie = N'PIN-MON-88');
DECLARE @aPinBaja int = (SELECT id_activo FROM activo WHERE numero_serie = N'PIN-BAJA-9');
DECLARE @aSamLt int = (SELECT id_activo FROM activo WHERE numero_serie = N'SAM-LT-77');

IF @aLat IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aLat AND id_tipo_asignacion = @tipoAsig)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, activa, observaciones)
    VALUES (@aLat, @usrOpSlc, @respMarta, @ubOficina1, @estAsig, @tipoAsig, '2025-09-01', 1, N'Entrega a Finanzas');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), SYSUTCDATETIME(), N'Asignacion', N'Entrega a responsable', N'', N'id_responsable=' + CAST(@respMarta AS varchar(12)));
END;

IF @aMon IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aMon)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, fecha_devolucion, activa, observaciones)
    VALUES (@aMon, @usrOpSlc, @respMarta, @ubOficina1, @estAsig, @tipoAsig, '2025-06-10', '2025-08-20', 0, N'Devolución por cambio de puesto');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), '2025-08-20', N'Asignacion', N'Devolucion', N'activa=true', N'activa=false');
END;

IF @aPr IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aPr AND id_tipo_asignacion = @tipoMant)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, activa, observaciones)
    VALUES (@aPr, @usrOpSlc, @respAngel, @ubOficina1, @estMant, @tipoMant, '2026-08-20', 1, N'[Correctivo] Atasco reiterado de papel');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), SYSUTCDATETIME(), N'Mantenimiento', N'Inicio de mantenimiento', N'id_estado=', N'tipo=Correctivo; costo=450; factura=M-882');
END;

IF @aSv IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aSv AND id_tipo_asignacion = @tipoTras)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, fecha_devolucion, activa, observaciones)
    VALUES (@aSv, @usrAdminSlc, @respAngel, @ubServidores, @estTras, @tipoTras, '2025-12-02', '2025-12-02', 0, N'Reubicacion a sala de servidores');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), '2025-12-02', N'Traslado', N'Traslado de activo', N'id_ubicacion=' + CAST(@ubOficina1 AS varchar(12)), N'id_ubicacion=' + CAST(@ubServidores AS varchar(12)));
END;

IF @aPinLt IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aPinLt AND id_tipo_asignacion = @tipoAsig)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, activa, observaciones)
    VALUES (@aPinLt, @usrOpPin, @respSofia, @ubGerencia, @estAsig, @tipoAsig, '2025-11-15', 1, N'Asignada a gerencia administrativa');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), '2025-11-15', N'Asignacion', N'Entrega a responsable', N'', N'id_responsable=' + CAST(@respSofia AS varchar(12)));
END;

IF @aPinScan IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aPinScan AND id_tipo_asignacion = @tipoMant)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, activa, observaciones)
    VALUES (@aPinScan, @usrOpPin, @respPedro, @ubLinea1, @estMant, @tipoMant, '2026-08-28', 1, N'[Preventivo] Limpieza de cabezal y calibracion');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), SYSUTCDATETIME(), N'Mantenimiento', N'Inicio de mantenimiento', N'', N'tipo=Preventivo; proveedor=Insumos Industriales');
END;

IF @aPinBaja IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aPinBaja AND id_tipo_asignacion = @tipoBaja)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, activa, observaciones, documento_pdf_url)
    VALUES (@aPinBaja, @usrOpPin, @respSofia, @ubTaller, @estBaja, @tipoBaja, '2026-04-12', 1, N'Dano irreparable', N'Acta-baja-PIN-BAJA-9');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), '2026-04-12', N'Baja', N'Baja de activo', N'', N'motivo=Dano irreparable');
END;

IF @aSamLt IS NOT NULL AND NOT EXISTS (SELECT 1 FROM asignacion WHERE id_activo = @aSamLt AND id_tipo_asignacion = @tipoAsig)
BEGIN
    INSERT INTO asignacion (id_activo, id_usuario, id_responsable, id_ubicacion, id_estado, id_tipo_asignacion, fecha_asignacion, activa, observaciones)
    VALUES (@aSamLt, @usrOpSam, @respAndres, @ubShowroom, @estAsig, @tipoAsig, '2026-01-08', 1, N'Equipo de campo comercial');
    INSERT INTO historial_activo (id_asignacion, fecha_hora, tipo_operacion, descripcion, informacion_anterior, informacion_nueva)
    VALUES (SCOPE_IDENTITY(), '2026-01-08', N'Asignacion', N'Entrega a responsable', N'', N'id_responsable=' + CAST(@respAndres AS varchar(12)));
END;

IF NOT EXISTS (SELECT 1 FROM historico_inventario WHERE id_sede = @sedeCentral AND responsable = N'Karla Morales')
BEGIN
    INSERT INTO historico_inventario (id_sede, cerrado, responsable, fecha_inicio, fecha_cierre, observaciones)
    VALUES (@sedeCentral, 1, N'Karla Morales', '2026-07-15', '2026-07-16', N'Inventario semestral sede Central');
    DECLARE @idInv int = SCOPE_IDENTITY();
    IF @aLat IS NOT NULL
        INSERT INTO detalle_activo (id_activo, id_inventario, encontrado, buen_estado, observaciones, fecha_verificacion)
        VALUES (@aLat, @idInv, 1, 1, N'En oficina 1', '2026-07-15');
    IF @aSv IS NOT NULL
        INSERT INTO detalle_activo (id_activo, id_inventario, encontrado, buen_estado, observaciones, fecha_verificacion)
        VALUES (@aSv, @idInv, 1, 1, N'Rack 2', '2026-07-15');
    IF @aPr IS NOT NULL
        INSERT INTO detalle_activo (id_activo, id_inventario, encontrado, buen_estado, observaciones, fecha_verificacion)
        VALUES (@aPr, @idInv, 0, 0, N'No encontrado en recuento', '2026-07-16');
END;

IF NOT EXISTS (SELECT 1 FROM historico_inventario WHERE id_sede = @sedePlanta AND responsable = N'Elena Ruano')
BEGIN
    INSERT INTO historico_inventario (id_sede, cerrado, responsable, fecha_inicio, observaciones)
    VALUES (@sedePlanta, 0, N'Elena Ruano', '2026-09-01', N'Jornada abierta planta Villa Nueva');
END;

COMMIT TRANSACTION;
GO
