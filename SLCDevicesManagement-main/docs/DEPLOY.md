# DERCAS — despliegue de SLCDevicesManagement API

API REST de inventario de activos multiempresa. No hay captcha, SMTP ni envelope `{success,message}` global. Las rutas van bajo `/api/...` (sin `api/v1`).

## Runtime

Los csproj declaran **`net10.0`** (`src/SLCDM.Api/SLCDM.Api.csproj` y el resto de la solución). Compilar y publicar con el SDK de .NET 10. Los paquetes de EF Core y JWT Bearer son 8.0.x; el TargetFramework del host es 10.

```bash
dotnet build src/SLCDM.Api/SLCDM.Api.csproj
```

Perfil HTTP local: `http://localhost:5139` (`src/SLCDM.Api/Properties/launchSettings.json`). HTTPS de desarrollo: `https://localhost:7062`.

## SQL Server y cadena de conexión

La API lee **`ConnectionStrings:DefaultConnection`**. Valor de `appsettings.json` (Windows / SSPI):

```
Server=localhost;Database=DercasInventario;Trusted_Connection=True;TrustServerCertificate=True;
```

Si SQL Server es Express con nombre (`SLCGTAGRIJALVA\SQLEXPRESS` en SSMS), `localhost` apunta a la instancia por defecto y falla. En este equipo **SQL Server Browser está detenido** y TCP/IP de Express está apagado, así que `Microsoft.Data.SqlClient` no resuelve `.\SQLEXPRESS` (SSMS sí, por memoria compartida).

La cadena local en `appsettings.json` usa el named pipe:

```
Server=np:\\.\pipe\MSSQL$SQLEXPRESS\sql\query;Database=DercasInventario;Trusted_Connection=True;TrustServerCertificate=True;
```

En JSON: `Server=np:\\\\.\\pipe\\MSSQL$SQLEXPRESS\\sql\\query;...`.

Alternativa más limpia: en SQL Server Configuration Manager habilita TCP/IP de `SQLEXPRESS`, arranca el servicio **SQL Server Browser**, y usa `Server=.\\SQLEXPRESS;...`.

En producción no uses `Trusted_Connection` del desarrollador: pon usuario SQL o identidad administrada en **User Secrets**, variables de entorno o el almacén de secretos del host.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=DercasInventario;User Id=...;Password=...;TrustServerCertificate=True;" --project src/SLCDM.Api
```

Sobrescritura por entorno: `ConnectionStrings__DefaultConnection`.

## Esquema

Preferido (EF Core 8, proyecto Persistence, startup Api):

```bash
dotnet ef database update --project src/SLCDM.Persistence --startup-project src/SLCDM.Api
```

Si no se usa EF, ejecutar en este orden sobre la base vacía:

1. `Scripts/InitialSchema.sql` (tablas + historial de migraciones).
2. `Scripts/AsignacionUbicacionUso.sql` si esa migración no quedó aplicada (columna de ubicación/uso en asignación).
3. `Scripts/SeedCatalogos.sql` (paso siguiente).

## Catálogos mínimos

El API **no** siembra catálogos. Sin ellos, traslado / mantenimiento / baja responden **409**.

Ejecutar `Scripts/SeedCatalogos.sql` (idempotente):

| Tabla | Nombres |
| --- | --- |
| `tipo_asignacion` | `Asignacion`, `Traslado`, `Mantenimiento`, `Baja` (exactos; ver `TipoAsignacionNombres`) |
| `estado` | al menos `Activo` (`Asignacion.IdEstado` es obligatorio) |

También se pueden crear como `AdministradorGeneral` vía `POST /api/tiposAsignacion` y `POST /api/estados`.

## JWT

Sección **`JwtSettings`** (no existe `Jwt:SigningKey`):

| Clave | Notas |
| --- | --- |
| `SecretKey` | ≥ 32 caracteres. En producción: User Secrets o variable `JwtSettings__SecretKey`. |
| `Issuer` | Debe coincidir al emitir y validar (dev: `SLCDevicesManagement`). |
| `Audience` | Igual (dev: `SLCDevicesManagement`). |
| `ExpiryInMinutes` | Dev: 480. |

Validación: `ClockSkew = TimeSpan.Zero`, `MapInboundClaims = false`, claim de rol = `role`.

## CORS

Política **`ReactClient`**: orígenes de `Cors:Origins` (por defecto `http://localhost:5173` y `https://localhost:5173`), `AllowAnyHeader`, `AllowAnyMethod`, **`AllowCredentials`**.

Ajusta `Cors:Origins` al origen real del frontend en cada ambiente.

## Frontend (Vite)

Puerto de desarrollo: **5173**. Copiar `frontend/.env.example` a `frontend/.env`:

```
VITE_API_URL=http://localhost:5139
VITE_USE_API_MOCK=false
```

Health real: `GET /api/health` (ya no se usa `/weatherforecast`).

## Primer usuario (no hay seeder en código)

`POST /api/usuarios` exige JWT de `AdministradorEmpresa` o `AdministradorGeneral`. El primer usuario se inserta a mano en SQL y luego el resto por Swagger.

1. `password_hash` es **Argon2id** (`PasswordHashService`: `$argon2id$v=19$m=65536,t=2,p=1$...`). No insertar texto plano. Generar el hash en un scratch C# con `new PasswordHashService().HashPassword("tu-clave")` o un programa que referencie `SLCDM.Application`.
2. `rol` se guarda como string del enum: `AdministradorGeneral` (valores: `Consulta`, `OperadorInventario`, `AdministradorEmpresa`, `AdministradorGeneral`).
3. `id_empresa` puede ser `NULL` para el bootstrap general.

```sql
INSERT INTO [usuario] (
    [id_empresa], [nombres], [apellidos], [correo], [username],
    [password_hash], [rol], [fecha_creacion], [habilitado]
)
VALUES (
    NULL, N'Admin', N'General', N'admin@localhost', N'admin.general',
    N'<ARGON2ID_HASH>', N'AdministradorGeneral', SYSUTCDATETIME(), 1
);
```

Login: `POST /api/auth/login` con `{ "emailOrUsername": "admin@localhost", "password": "<clave en claro>" }`. El API hashea Argon2id al crear/actualizar usuarios; el login compara el claro contra el hash.

Después del primer JWT, crear el resto con `POST /api/usuarios` (body: `password` en claro, no `passwordHash`).

## Smoke

Sin token:

```bash
curl -s http://localhost:5139/api/health
curl -s -X POST http://localhost:5139/api/auth/login -H "Content-Type: application/json" -d "{\"emailOrUsername\":\"admin@localhost\",\"password\":\"...\"}"
```

Con `Authorization: Bearer <accessToken>`:

```bash
curl -s http://localhost:5139/api/auth/profile -H "Authorization: Bearer <token>"
curl -s http://localhost:5139/api/reportes/inventario-general -H "Authorization: Bearer <token>"
```

`GET /api/health` y `POST /api/auth/login` son anónimos. El resto de controladores hereda `[Authorize]`.

## Tag sugerido (no aplicado)

```
v1.0.0 — DERCAS API: inventario, movimientos, jornadas, reportes, JWT por perfil.
```
