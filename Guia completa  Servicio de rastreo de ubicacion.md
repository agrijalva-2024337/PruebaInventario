# Guia completa: Servicio de rastreo de ubicacion de activos

Este documento junta todo lo que se diseno en la conversacion: el problema, las
decisiones que se tomaron y por que, y el codigo completo listo para copiar en
el repo SLCDevicesManagement (SLCDM). Esta escrito para que lo puedas leer de
principio a fin y entender cada pieza, no solo copiarla.

## 1. El problema que estamos resolviendo

Tienen activos (laptops, desktops) repartidos en sedes de varios paises
(Guatemala, Miami, Costa Rica, Panama, Honduras) y kioscos en malls. Quieren
saber automaticamente en que ubicacion fisica esta cada equipo, sin GPS
(la mayoria de laptops no trae) y sin pedirle permisos de localizacion al
sistema operativo.

La idea que se armo: cada equipo tiene un pequeno servicio de Windows que,
cada cierto tiempo, detecta a que red Wi-Fi esta conectado y se lo reporta al
backend. El backend ya sabe que ubicacion (oficina, sede, kiosco) corresponde
a esa red, asi que puede inferir donde esta el equipo sin coordenadas.

## 2. Como encaja con lo que ya existe (y por que no se duplica nada)

Antes de agregar tablas nuevas, revisamos el modelo de datos que ya tienen
(el ERD de DERCAS) para reusar lo que ya sirve. Esto es clave para entender
el diseno completo:

`Activo` ya tiene un campo `IdUbicacion` (a que ubicacion pertenece
actualmente) y un campo `NumeroSerie` (el numero de serie que capturaron al
comprar el equipo). Los dos se reusan tal cual.

`HistorialActivo` ya es la tabla donde se registra cualquier cambio
importante que le pasa a un activo (traslados, mantenimientos, bajas). Se
reusa para dejar constancia de cada cambio de ubicacion detectado
automaticamente, exactamente con el mismo patron que ya usa
`CreateTrasladoCommand` para traslados manuales.

`Sede` ya tiene `IdPais` e `IdEmpresa`, y `Ubicacion` ya cuelga de `Sede`. O
sea que la jerarquia multi-pais y multi-empresa que necesitan ya existe:
Guatemala, Miami, Costa Rica, etc. ya son sedes distintas en el modelo actual.
No hace falta tocar nada ahi.

Lo unico que de verdad no tiene donde reusarse, y por eso se agrega como
tablas nuevas, son dos conceptos que hoy no existen en el sistema:

Una credencial de equipo (`DispositivoToken`): hoy todo lo que autentica al
sistema es un usuario humano con JWT. Un servicio de Windows que corre solo,
sin nadie logueado, necesita su propia forma de autenticarse.

Un catalogo de redes conocidas (`RedConocida`): hoy no hay ningun lugar donde
decir "esta red Wi-Fi (este BSSID) pertenece a esta ubicacion". Y como una
sede real tiene varios puntos de acceso Wi-Fi (varios BSSID distintos
transmitiendo la misma red), esto tiene que ser una tabla (varios BSSID por
una ubicacion), no una columna.

Con esas dos piezas nuevas, y dos campos adicionales en `DispositivoToken`
para la alerta (ver seccion 7.9), todo lo demas se resuelve sin inventar
conceptos que no encajen con el resto del sistema. Un matiz importante que
se ajusto durante el diseno: el rastreo automatico NO actualiza
`Activo.IdUbicacion`. Ese campo sigue siendo, como ya era antes de este
addendum, la ubicacion asignada por un traslado autorizado (un proceso
humano). El rastreo solo compara la ubicacion detectada contra esa asignada
y marca una alerta si no coinciden -- ver la seccion 7.8 para el porque de
esta separacion.

## 3. El flujo completo, de principio a fin

Antes de ver codigo, esto es lo que pasa en orden:

Un tecnico da de alta el activo en el inventario como siempre (con su
`NumeroSerie` bien capturado, eso es importante).

Se instala el agente (el servicio de Windows) en ese equipo. En su primer
arranque, el agente lee el numero de serie real del hardware (no el que
alguien escribio a mano, el que el propio Windows reporta) y se lo manda al
backend junto con una "llave de instalacion" (una clave compartida del lote
de despliegue, no un secreto por equipo).

El backend busca en `Activo` cual tiene ese mismo `NumeroSerie` catalogado.
Si lo encuentra, genera un `DispositivoToken` (la credencial del equipo) y se
lo devuelve. El agente lo guarda en el Credential Manager de Windows (no en
un archivo de texto) y ya no necesita hacer este paso de nuevo.

De ahi en adelante, cada cierto tiempo (cada 15 minutos, por ejemplo), el
agente detecta el BSSID de la red Wi-Fi conectada y hace un POST al backend
con ese dato, autenticandose con su token.

El backend busca en `RedConocida` a que `Ubicacion` pertenece ese BSSID, y la
compara contra la ubicacion que el activo tiene asignada (la que fijo el
ultimo traslado autorizado, no la detectada). Si coinciden, no pasa nada. Si
no coinciden -- o el BSSID no esta catalogado -- se marca el equipo como
"fuera de rango" y se escribe una entrada en `HistorialActivo` solo en el
momento en que cambia ese estado (entra o sale de la alerta). El rastreo
automatico nunca cambia la ubicacion asignada del activo; eso solo lo hace
un traslado autorizado por una persona.

Si el servicio de Windows se cae por cualquier razon, el propio Windows lo
vuelve a levantar solo (esto se configura en la instalacion, no hace falta
programarlo).

## 4. Conceptos que vale la pena entender antes del codigo

**Por que dos formas distintas de autenticacion (JWT y DeviceToken).** El
JWT que ya tienen identifica a una persona que inicio sesion (tiene rol,
tiene empresa). Un servicio de Windows no es una persona logueada, asi que
no puede usar JWT. Por eso se agrega un segundo "esquema de autenticacion"
en ASP.NET Core, aparte del JWT, que valida un header custom
(`X-Device-Token`) contra la tabla `DispositivoToken`. Los dos esquemas
conviven: los endpoints normales piden JWT con rol, el endpoint de ping pide
DeviceToken.

**Por que el hash del token de dispositivo es distinto al de las
contrasenas.** Las contrasenas de usuario se hashean con Argon2 (lo que ya
usa `PasswordHashService`), que es lento a proposito para que a un atacante
le cueste probar millones de contrasenas. Pero Argon2 no sirve para este
caso: para validar un login normal comparas la contrasena escrita contra
UN hash ya conocido (el del usuario que dice ser). Aqui, en cambio, el
servidor recibe un token y necesita **encontrar** cual dispositivo es sin
saber de antemano cual — eso exige poder buscarlo por igualdad en la base de
datos, y Argon2 genera un hash distinto cada vez aunque el texto sea el
mismo (por el salt aleatorio), asi que no se puede indexar ni buscar por
igualdad. Por eso aqui se usa HMAC-SHA256 con una llave secreta del servidor
(un "pepper"): es deterministico (el mismo token siempre da el mismo hash,
se puede buscar por igualdad), y como el token no es una contrasena elegida
por un humano sino 32 bytes aleatorios generados por el sistema, no hace
falta que el hash sea lento — es imposible de adivinar por fuerza bruta de
todas formas.

**Por que `IgnoreQueryFilters()` aparece en varios lugares.** Este backend ya
tiene un sistema multiempresa: cuando un usuario esta logueado, las consultas
se filtran automaticamente para que solo vea datos de su propia empresa (lo
hace `ApplyEmpresaQueryFilters` en el `ApplicationDbContext`). Ese filtro
depende de que haya un usuario con una claim de empresa en el JWT. Pero un
DeviceToken no tiene usuario ni empresa detras — es un equipo, no una
persona. Si no se usa `IgnoreQueryFilters()` en esos puntos especificos, las
consultas devolverian vacio siempre y el sistema pareceria roto. Se usa con
cuidado: solo en los pasos donde la operacion ya esta acotada a un unico
activo especifico (por el token o por el numero de serie), asi que saltarse
el filtro ahi no expone datos de otras empresas.

**Por que RedConocida es una tabla y no una columna.** Una ubicacion real
(una oficina, un piso, un kiosco) casi nunca tiene un solo punto de acceso
Wi-Fi — tiene varios routers/APs dando cobertura, cada uno con su propio
BSSID aunque todos transmitan el mismo nombre de red. La relacion real es
"muchos BSSID pertenecen a una ubicacion", y eso en una base de datos
relacional se modela con una tabla aparte, no con una columna (una columna
solo podria guardar un valor a la vez).

**Por que Windows ya resuelve el "reinicio automatico" sin programarlo.**
Todo servicio de Windows tiene una configuracion nativa llamada Recovery
Actions (se ve en la pestana "Recovery" de las propiedades del servicio, o
se configura con `sc.exe failure`). Le dice al sistema operativo que, si el
servicio se cae, lo reinicie solo despues de X segundos, hasta N veces. Esto
es mas confiable que programar un segundo servicio que vigile al primero,
porque ese segundo servicio tambien podria fallar y nadie lo estaria
vigilando a el.

**El riesgo del Wi-Fi de kioscos en malls.** Si el kiosco se conecta al
Wi-Fi publico o de cortesia del mall (no a una red propia de la empresa), dos
cosas pueden salir mal: primero, ambiguedad, si hay varios puntos de la
empresa en el mismo mall compartiendo la misma red, el sistema podria
confundir cual es cual. Segundo, y mas grave: la mayoria de esas redes
publicas tienen "portal cautivo" (esa pantalla del navegador donde aceptas
terminos antes de tener internet real). Un servicio corriendo en segundo
plano, sin navegador ni usuario interactuando, no puede pasar esa pantalla,
asi que el agente jamas lograria mandar sus pings aunque el resto funcione
perfecto. La recomendacion para kioscos especificamente es que tengan su
propio punto de acceso dedicado (un router chico con datos moviles), no que
dependan del Wi-Fi del mall.

## 5. Cambios en el modelo de datos (Domain)

### 5.1 Entidad nueva: DispositivoToken

Archivo: `src/SLCDM.Domain/Entities/DispositivoToken.cs`

Es la credencial de un equipo. Un token por activo. Solo se guarda el hash,
nunca el valor real del token (igual que con las contrasenas).

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Credencial de un agente de rastreo instalado en un equipo. Un token por
/// activo. El valor crudo del token solo existe en el momento en que se
/// genera (RegistrarDispositivoCommand / AutoRegistrarDispositivoCommand);
/// aqui solo se guarda su hash.
/// </summary>
public class DispositivoToken : SLCDM.Domain.Common.BaseEntity
{
    [Required(ErrorMessage = "El campo id activo es obligatorio")]
    public int IdActivo { get; set; }

    [ForeignKey("IdActivo")]
    public Activo? Activo { get; set; }

    [Required]
    [MaxLength(200, ErrorMessage = "El campo token hash no debe superar los 200 caracteres")]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreadoEn { get; set; }

    public DateTime? ExpiraEn { get; set; }

    public DateTime? UltimoUsoEn { get; set; }

    public bool Revocado { get; set; }
}
```

### 5.2 Entidad nueva: RedConocida

Archivo: `src/SLCDM.Domain/Entities/RedConocida.cs`

El catalogo de BSSID conocidos, cada uno apuntando a que ubicacion cubre.

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SLCDM.Domain.Entities;

/// <summary>
/// Un punto de acceso Wi-Fi fisico (BSSID) conocido, mapeado a la Ubicacion
/// que cubre. Una Ubicacion puede tener varios BSSID (varios APs dando
/// cobertura a la misma sede), por eso esto es una tabla y no una columna
/// en Ubicacion.
/// </summary>
public class RedConocida : SLCDM.Domain.Common.BaseEntity
{
    [Required]
    [MaxLength(17, ErrorMessage = "El campo bssid no debe superar los 17 caracteres")]
    public string Bssid { get; set; } = string.Empty;

    [Required(ErrorMessage = "El campo id ubicacion es obligatorio")]
    public int IdUbicacion { get; set; }

    [ForeignKey("IdUbicacion")]
    public Ubicacion? Ubicacion { get; set; }
}
```

## 6. Cambios en Persistence (mapeo a la base de datos)

### 6.1 Configuracion de DispositivoToken

Archivo: `src/SLCDM.Persistence/Configurations/DispositivoTokenConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class DispositivoTokenConfiguration : IEntityTypeConfiguration<DispositivoToken>
{
    public void Configure(EntityTypeBuilder<DispositivoToken> builder)
    {
        builder.ToTable("dispositivo_token");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id_dispositivo_token");

        builder.Property(d => d.IdActivo).HasColumnName("id_activo").IsRequired();

        builder.HasOne(d => d.Activo)
            .WithMany()
            .HasForeignKey(d => d.IdActivo)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(d => d.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(d => d.ExpiraEn).HasColumnName("expira_en");
        builder.Property(d => d.UltimoUsoEn).HasColumnName("ultimo_uso_en");

        builder.Property(d => d.Revocado)
            .HasColumnName("revocado")
            .HasDefaultValue(false);

        builder.HasIndex(d => d.TokenHash).IsUnique();
        builder.HasIndex(d => d.IdActivo).IsUnique().HasFilter("[revocado] = 0");
    }
}
```

El indice unico en `TokenHash` es lo que hace rapida la busqueda "que
dispositivo tiene este token" en cada peticion. El segundo indice
(`IdActivo` unico donde `revocado = 0`) asegura a nivel de base de datos que
un activo no pueda tener dos tokens activos al mismo tiempo, aunque el
codigo de validacion falle o alguien inserte directo en la base.

### 6.2 Configuracion de RedConocida

Archivo: `src/SLCDM.Persistence/Configurations/RedConocidaConfiguration.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence.Configurations;

public class RedConocidaConfiguration : IEntityTypeConfiguration<RedConocida>
{
    public void Configure(EntityTypeBuilder<RedConocida> builder)
    {
        builder.ToTable("red_conocida");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id_red_conocida");

        builder.Property(r => r.Bssid)
            .HasColumnName("bssid")
            .HasColumnType("varchar(17)")
            .IsRequired();

        builder.Property(r => r.IdUbicacion).HasColumnName("id_ubicacion").IsRequired();

        builder.HasOne(r => r.Ubicacion)
            .WithMany()
            .HasForeignKey(r => r.IdUbicacion)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Bssid).IsUnique();
    }
}
```

### 6.3 Registrar las tablas nuevas en el DbContext

Archivo: `src/SLCDM.Application/Common/Interfaces/IApplicationDbContext.cs`

Agregar estas dos lineas junto a los demas `DbSet`:

```csharp
    DbSet<DispositivoToken> DispositivosToken { get; }
    DbSet<RedConocida> RedesConocidas { get; }
```

Archivo: `src/SLCDM.Persistence/ApplicationDbContext.cs`

Agregar junto a los demas `DbSet`:

```csharp
    public DbSet<DispositivoToken> DispositivosToken => Set<DispositivoToken>();
    public DbSet<RedConocida> RedesConocidas => Set<RedConocida>();
```

Y dentro del metodo `ApplyEmpresaQueryFilters`, siguiendo el mismo patron que
ya usan las demas entidades (filtrar por el activo o la ubicacion a la que
pertenecen):

```csharp
        modelBuilder.Entity<DispositivoToken>().HasQueryFilter(d =>
            IgnoreEmpresaFilter || Activos.Any(a => a.Id == d.IdActivo));

        modelBuilder.Entity<RedConocida>().HasQueryFilter(r =>
            IgnoreEmpresaFilter || Ubicaciones.Any(u => u.Id == r.IdUbicacion));
```

### 6.4 La migracion

Con las dos entidades y sus configuraciones ya en el proyecto, se genera la
migracion de Entity Framework (esto crea automaticamente el script SQL que
agrega las tablas nuevas a la base de datos):

```
dotnet ef migrations add AddDispositivoTokenYRedConocida --project src/SLCDM.Persistence --startup-project src/SLCDM.Api
dotnet ef database update --project src/SLCDM.Persistence --startup-project src/SLCDM.Api
```

## 7. Cambios en Application (la logica de negocio)

Esta es la capa donde vive la logica, sin depender de ASP.NET Core ni de
Entity Framework directamente (por eso todo usa `IApplicationDbContext`, la
interfaz, no la implementacion real).

### 7.1 Opciones de configuracion

Archivo: `src/SLCDM.Application/Common/Options/DeviceTrackingOptions.cs`

Sigue el mismo patron que ya usan con `JwtOptions`: una clase que se llena
desde `appsettings.json`.

```csharp
namespace SLCDM.Application.Common.Options;

public sealed class DeviceTrackingOptions
{
    public const string SectionName = "DeviceTracking";

    public string Pepper { get; set; } = string.Empty;

    public string InstallKey { get; set; } = string.Empty;

    public int TokenExpiryDays { get; set; } = 365;
}
```

### 7.2 Servicio de hash del token

Archivo: `src/SLCDM.Application/Common/Interfaces/IDeviceTokenHashService.cs`

```csharp
namespace SLCDM.Application.Common.Interfaces;

public interface IDeviceTokenHashService
{
    string GenerateRawToken();

    string Hash(string rawToken);
}
```

Archivo: `src/SLCDM.Application/Common/Security/DeviceTokenHashService.cs`

```csharp
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;

namespace SLCDM.Application.Common.Security;

/// <summary>
/// Hash de tokens de dispositivo. A diferencia de PasswordHashService
/// (Argon2, pensado para contrasenas de baja entropia elegidas por
/// humanos), un token de dispositivo es un secreto aleatorio de alta
/// entropia: aqui se necesita un hash deterministico (HMAC-SHA256 con
/// pepper del servidor) para poder buscarlo por igualdad en la base de
/// datos. Un token de 32 bytes aleatorios no es fuerza-bruteable aunque el
/// hash sea rapido de calcular.
/// </summary>
public sealed class DeviceTokenHashService : IDeviceTokenHashService
{
    private readonly byte[] _pepper;

    public DeviceTokenHashService(IOptions<DeviceTrackingOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Pepper) || options.Value.Pepper.Length < 32)
        {
            throw new InvalidOperationException(
                "DeviceTracking:Pepper debe tener al menos 32 caracteres. Use User Secrets o variables de entorno en produccion.");
        }

        _pepper = Encoding.UTF8.GetBytes(options.Value.Pepper);
    }

    public string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public string Hash(string rawToken)
    {
        using var hmac = new HMACSHA256(_pepper);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hash);
    }
}
```

Recuerda registrar `IDeviceTokenHashService` en el archivo donde el proyecto
ya registra `IPasswordHashService` (busca `AddScoped<IPasswordHashService`
o similar dentro de `SLCDM.Application` y agrega la linea equivalente).

### 7.3 El nombre de la claim del equipo autenticado

Archivo: `src/SLCDM.Application/Common/Security/DeviceClaimTypes.cs`

Cuando el backend valida el DeviceToken, necesita "recordar" durante esa
peticion cual es el `IdActivo` correspondiente. Eso se hace con una claim,
igual que el JWT lleva claims de usuario, rol y empresa.

```csharp
namespace SLCDM.Application.Common.Security;

/// <summary>
/// Nombre de la claim que porta el principal autenticado por DeviceToken
/// (esquema aparte del JWT de usuario: aqui no hay usuario logueado).
/// </summary>
public static class DeviceClaimTypes
{
    public const string IdActivo = "id_activo_dispositivo";
}
```

### 7.4 DTO de respuesta al emitir un token

Archivo: `src/SLCDM.Application/Features/Dispositivos/DispositivoTokenDto.cs`

```csharp
namespace SLCDM.Application.Features.Dispositivos;

public sealed record DispositivoTokenDto(
    int Id,
    int IdActivo,
    string TokenCrudo,
    DateTime CreadoEn,
    DateTime? ExpiraEn);
```

### 7.5 Comando: registro manual de dispositivo (lo hace un administrador)

Archivo: `src/SLCDM.Application/Features/Dispositivos/Commands/RegistrarDispositivoCommand.cs`

Este es el camino manual: un administrador, desde el sistema (con su JWT
normal), da de alta el dispositivo de un activo especifico y recibe el token
para instalarlo el mismo. Util para casos donde no quieren depender del
autoregistro por numero de serie.

```csharp
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Dispositivos.Commands;

public sealed record RegistrarDispositivoCommand(int IdActivo);

public sealed class RegistrarDispositivoCommandValidator : AbstractValidator<RegistrarDispositivoCommand>
{
    public RegistrarDispositivoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdActivo)
            .RequiredId("id activo")
            .MustAsync(async (id, ct) => await db.Activos.AnyAsync(a => a.Id == id, ct))
            .WithMessage("No se encontro un activo con el id informado.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                !await db.DispositivosToken.AnyAsync(d => d.IdActivo == cmd.IdActivo && !d.Revocado, ct))
            .WithMessage("El activo ya tiene un token de dispositivo activo. Revoquelo antes de generar uno nuevo.");
    }
}

public sealed class RegistrarDispositivoCommandHandler : ICommandHandler<RegistrarDispositivoCommand, DispositivoTokenDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<RegistrarDispositivoCommand> _validator;
    private readonly IDeviceTokenHashService _tokenHash;
    private readonly DeviceTrackingOptions _options;

    public RegistrarDispositivoCommandHandler(
        IApplicationDbContext db,
        IValidator<RegistrarDispositivoCommand> validator,
        IDeviceTokenHashService tokenHash,
        IOptions<DeviceTrackingOptions> options)
    {
        _db = db;
        _validator = validator;
        _tokenHash = tokenHash;
        _options = options.Value;
    }

    public async Task<DispositivoTokenDto> HandleAsync(RegistrarDispositivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var rawToken = _tokenHash.GenerateRawToken();
        var ahora = DateTime.UtcNow;

        var entity = new DispositivoToken
        {
            IdActivo = command.IdActivo,
            TokenHash = _tokenHash.Hash(rawToken),
            CreadoEn = ahora,
            ExpiraEn = ahora.AddDays(_options.TokenExpiryDays),
            Revocado = false
        };

        _db.DispositivosToken.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // El token crudo solo existe aqui: no se guarda en ningun lado. Se
        // muestra una sola vez para que el administrador lo copie.
        return new DispositivoTokenDto(entity.Id, entity.IdActivo, rawToken, entity.CreadoEn, entity.ExpiraEn);
    }
}
```

### 7.6 Comando: autoregistro por numero de serie (lo hace el agente solo)

Archivo: `src/SLCDM.Application/Features/Dispositivos/Commands/AutoRegistrarDispositivoCommand.cs`

Este es el camino automatico: el agente, en su primer arranque, se
autoregistra sin que nadie de TI lo toque, usando el numero de serie del
hardware mas una llave de instalacion compartida.

```csharp
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Dispositivos.Commands;

public sealed record AutoRegistrarDispositivoCommand(string NumeroSerie, string InstallKey);

public sealed class AutoRegistrarDispositivoCommandValidator : AbstractValidator<AutoRegistrarDispositivoCommand>
{
    private readonly DeviceTrackingOptions _options;

    public AutoRegistrarDispositivoCommandValidator(IOptions<DeviceTrackingOptions> options)
    {
        _options = options.Value;

        RuleFor(x => x.NumeroSerie).NotEmpty().WithMessage("El numero de serie es obligatorio.");

        RuleFor(x => x.InstallKey)
            .Must(EsInstallKeyValida)
            .WithMessage("Llave de instalacion invalida.");
    }

    private bool EsInstallKeyValida(string installKey) =>
        !string.IsNullOrEmpty(installKey)
        && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(installKey),
            System.Text.Encoding.UTF8.GetBytes(_options.InstallKey));
}

public sealed class AutoRegistrarDispositivoCommandHandler
    : ICommandHandler<AutoRegistrarDispositivoCommand, DispositivoTokenDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<AutoRegistrarDispositivoCommand> _validator;
    private readonly IDeviceTokenHashService _tokenHash;
    private readonly DeviceTrackingOptions _options;

    public AutoRegistrarDispositivoCommandHandler(
        IApplicationDbContext db,
        IValidator<AutoRegistrarDispositivoCommand> validator,
        IDeviceTokenHashService tokenHash,
        IOptions<DeviceTrackingOptions> options)
    {
        _db = db;
        _validator = validator;
        _tokenHash = tokenHash;
        _options = options.Value;
    }

    public async Task<DispositivoTokenDto> HandleAsync(AutoRegistrarDispositivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        // Sin JWT: esta es la llamada de arranque, antes de que el equipo
        // tenga cualquier credencial. Por eso IgnoreQueryFilters() -- no hay
        // usuario ni empresa detras todavia.
        var activo = await _db.Activos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.NumeroSerie == command.NumeroSerie, cancellationToken)
            ?? throw new NotFoundException("Activo con ese numero de serie", command.NumeroSerie);

        var tokenExistente = await _db.DispositivosToken.IgnoreQueryFilters()
            .AnyAsync(d => d.IdActivo == activo.Id && !d.Revocado, cancellationToken);

        if (tokenExistente)
        {
            throw new ConflictException("Este activo ya tiene un token de dispositivo activo.");
        }

        var rawToken = _tokenHash.GenerateRawToken();
        var ahora = DateTime.UtcNow;

        var entity = new DispositivoToken
        {
            IdActivo = activo.Id,
            TokenHash = _tokenHash.Hash(rawToken),
            CreadoEn = ahora,
            ExpiraEn = ahora.AddDays(_options.TokenExpiryDays),
            Revocado = false
        };

        _db.DispositivosToken.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new DispositivoTokenDto(entity.Id, entity.IdActivo, rawToken, entity.CreadoEn, entity.ExpiraEn);
    }
}
```

### 7.7 Comando: revocar un dispositivo

Archivo: `src/SLCDM.Application/Features/Dispositivos/Commands/RevocarDispositivoCommand.cs`

Para cuando dan de baja un equipo, o el token se compromete y hay que
invalidarlo.

```csharp
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Dispositivos.Commands;

public sealed record RevocarDispositivoCommand(int Id);

public sealed class RevocarDispositivoCommandValidator : AbstractValidator<RevocarDispositivoCommand>
{
    public RevocarDispositivoCommandValidator()
    {
        RuleFor(x => x.Id).RequiredId("id dispositivo token");
    }
}

public sealed class RevocarDispositivoCommandHandler : ICommandHandler<RevocarDispositivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<RevocarDispositivoCommand> _validator;

    public RevocarDispositivoCommandHandler(IApplicationDbContext db, IValidator<RevocarDispositivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(RevocarDispositivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.DispositivosToken.FirstOrDefaultAsync(d => d.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("DispositivoToken", command.Id);

        entity.Revocado = true;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
```

### 7.8 Comando: registrar la ubicacion detectada (el ping)

Archivo: `src/SLCDM.Application/Features/Dispositivos/Commands/RegistrarUbicacionCommand.cs`

Importante: esto NO mueve al activo de ubicacion. `Activo.IdUbicacion` es la
ubicacion asignada (la que fijo un traslado autorizado por un humano via
`CreateTrasladoCommand`) y el rastreo automatico nunca la toca. Lo que hace
este comando es comparar donde esta realmente el equipo contra donde
deberia estar, y prender o apagar una bandera de alerta si no coinciden. Si
las mezclaramos (que el rastreo actualizara `IdUbicacion` directamente),
perderiamos la diferencia entre "se movio con autorizacion" y "aparecio en
un lugar que no le corresponde" -- que es exactamente la senal que se busca
para detectar un robo o un uso indebido.

```csharp
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Dispositivos.Commands;

/// <summary>
/// IdActivo llega desde la claim del DeviceToken (Api/Authentication), nunca
/// del cuerpo de la peticion: un dispositivo solo puede reportar su propia
/// ubicacion, jamas la de otro activo.
/// </summary>
public sealed record RegistrarUbicacionCommand(int IdActivo, string Bssid);

public sealed class RegistrarUbicacionCommandHandler : ICommandHandler<RegistrarUbicacionCommand>
{
    private readonly IApplicationDbContext _db;

    public RegistrarUbicacionCommandHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(RegistrarUbicacionCommand command, CancellationToken cancellationToken = default)
    {
        var activo = await _db.Activos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == command.IdActivo, cancellationToken)
            ?? throw new NotFoundException("Activo", command.IdActivo);

        var token = await _db.DispositivosToken.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.IdActivo == command.IdActivo, cancellationToken)
            ?? throw new NotFoundException("DispositivoToken", command.IdActivo);

        var redConocida = await _db.RedesConocidas.IgnoreQueryFilters()
            .Include(r => r.Ubicacion)
            .FirstOrDefaultAsync(r => r.Bssid == command.Bssid, cancellationToken);

        var ubicacionDetectada = redConocida?.Ubicacion;

        // Decision de negocio: si el activo TODAVIA no tiene ubicacion
        // asignada (nunca paso por un traslado formal), no evaluamos alerta
        // -- no tiene sentido avisar que esta "fuera de rango" de un rango
        // que nunca se definio. Si mas adelante prefieren lo contrario
        // (tratar "sin asignar" como alerta), este es el unico punto que
        // hay que cambiar.
        if (activo.IdUbicacion is null)
        {
            token.UltimaUbicacionDetectadaId = ubicacionDetectada?.Id;
            token.UltimoUsoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        // Fuera de rango: la red no esta catalogada, o esta catalogada pero
        // apunta a una ubicacion distinta de la asignada al activo.
        var estaFueraDeRango = ubicacionDetectada is null || ubicacionDetectada.Id != activo.IdUbicacion;
        var eraFueraDeRango = token.FueraDeRango;

        token.FueraDeRango = estaFueraDeRango;
        token.UltimaUbicacionDetectadaId = ubicacionDetectada?.Id;
        token.UltimoUsoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        // Solo se escribe en el historial cuando cambia el estado (entra o
        // sale de la alerta), no en cada ping -- si no, serian decenas de
        // renglones idénticos por dia mientras el equipo sigue fuera de rango.
        if (estaFueraDeRango == eraFueraDeRango)
        {
            return;
        }

        _db.HistorialActivos.Add(new HistorialActivo
        {
            FechaHora = DateTime.UtcNow,
            TipoOperacion = estaFueraDeRango ? "AlertaFueraDeRango" : "AlertaResuelta",
            Descripcion = estaFueraDeRango
                ? "El activo fue detectado fuera de la ubicacion asignada"
                : "El activo volvio a la ubicacion asignada",
            InformacionAnterior = $"id_ubicacion_asignada={activo.IdUbicacion}",
            InformacionNueva = ubicacionDetectada is not null
                ? $"id_ubicacion_detectada={ubicacionDetectada.Id}; bssid={command.Bssid}"
                : $"ubicacion_detectada=desconocida; bssid={command.Bssid}"
        });
        await _db.SaveChangesAsync(cancellationToken);
    }
}
```

Nota que este handler no tiene un `RegistrarUbicacionCommandValidator`
separado: el `IdActivo` no viene del usuario (viene de la claim, ya validada
por la autenticacion), y el `Bssid` no necesita validarse contra catalogos
externos, solo se busca si existe o no. Es intencional que sea simple.

### 7.9 Dos campos nuevos en DispositivoToken (para la alerta)

Como el rastreo ya no toca `Activo.IdUbicacion`, necesita donde guardar "la
ultima ubicacion que detecto" y "si esta actualmente fuera de rango" -- eso
va en la misma tabla `DispositivoToken` que ya se agrego para la credencial,
no hace falta otra tabla nueva. Se agrega esto a la clase de la seccion 5.1:

```csharp
    public int? UltimaUbicacionDetectadaId { get; set; }

    [ForeignKey("UltimaUbicacionDetectadaId")]
    public Ubicacion? UltimaUbicacionDetectada { get; set; }

    public bool FueraDeRango { get; set; }
```

Y en `DispositivoTokenConfiguration.cs` (seccion 6.1), agregar antes de los
indices:

```csharp
        builder.Property(d => d.UltimaUbicacionDetectadaId).HasColumnName("ultima_ubicacion_detectada_id");

        builder.HasOne(d => d.UltimaUbicacionDetectada)
            .WithMany()
            .HasForeignKey(d => d.UltimaUbicacionDetectadaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.FueraDeRango)
            .HasColumnName("fuera_de_rango")
            .HasDefaultValue(false);
```

### 7.10 Consulta: quien esta fuera de rango ahora mismo

Archivo: `src/SLCDM.Application/Features/Dispositivos/Queries/GetDispositivosFueraDeRangoQuery.cs`

Esta es la que en la practica van a usar para monitorear -- un reporte o un
panel que consultan para ver, en cualquier momento, que equipos estan
actualmente fuera de la ubicacion que les corresponde.

```csharp
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Dispositivos.Queries;

public sealed record GetDispositivosFueraDeRangoQuery;

public sealed record DispositivoFueraDeRangoDto(
    int IdActivo,
    string NombreActivo,
    int? IdUbicacionAsignada,
    int? IdUbicacionDetectada,
    DateTime? UltimoUsoEn);

public sealed class GetDispositivosFueraDeRangoQueryHandler
    : IQueryHandler<GetDispositivosFueraDeRangoQuery, IReadOnlyList<DispositivoFueraDeRangoDto>>
{
    private readonly IApplicationDbContext _db;

    public GetDispositivosFueraDeRangoQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DispositivoFueraDeRangoDto>> HandleAsync(
        GetDispositivosFueraDeRangoQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = await _db.DispositivosToken.AsNoTracking()
            .Where(d => d.FueraDeRango && !d.Revocado)
            .Include(d => d.Activo)
            .Select(d => new DispositivoFueraDeRangoDto(
                d.IdActivo,
                d.Activo!.Nombre,
                d.Activo.IdUbicacion,
                d.UltimaUbicacionDetectadaId,
                d.UltimoUsoEn))
            .ToListAsync(cancellationToken);

        return items;
    }
}
```

Y el endpoint correspondiente en el controller (seccion 8.4), agregando el
query handler al constructor igual que los demas:

```csharp
    [HttpGet("fuera-de-rango")]
    [Authorize(Roles = Roles.Lectura)]
    public async Task<ActionResult<IReadOnlyList<DispositivoFueraDeRangoDto>>> FueraDeRango(
        CancellationToken cancellationToken) =>
        Ok(await _fueraDeRango.HandleAsync(new GetDispositivosFueraDeRangoQuery(), cancellationToken));
```

## 8. Cambios en Api (la capa web: autenticacion y el controller)

### 8.1 El nombre del esquema de autenticacion

Archivo: `src/SLCDM.Api/Authentication/DeviceTokenDefaults.cs`

```csharp
namespace SLCDM.Api.Authentication;

public static class DeviceTokenDefaults
{
    public const string AuthenticationScheme = "DeviceToken";

    public const string HeaderName = "X-Device-Token";
}
```

### 8.2 El handler que valida el token en cada peticion

Archivo: `src/SLCDM.Api/Authentication/DeviceTokenAuthenticationHandler.cs`

Esto es lo que ASP.NET Core llama automaticamente en cada peticion que pide
`[Authorize(AuthenticationSchemes = "DeviceToken")]`, antes de que el
controller reciba la peticion. Lee el header, busca el token en la base de
datos, y si es valido, arma un `ClaimsPrincipal` (la identidad autenticada)
con la claim del `IdActivo`.

```csharp
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;

namespace SLCDM.Api.Authentication;

public sealed class DeviceTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IApplicationDbContext _db;
    private readonly IDeviceTokenHashService _tokenHash;

    public DeviceTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IApplicationDbContext db,
        IDeviceTokenHashService tokenHash)
        : base(options, logger, encoder)
    {
        _db = db;
        _tokenHash = tokenHash;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(DeviceTokenDefaults.HeaderName, out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        var rawToken = headerValue.ToString();
        if (string.IsNullOrWhiteSpace(rawToken))
        {
            return AuthenticateResult.Fail("Token de dispositivo vacio.");
        }

        var hash = _tokenHash.Hash(rawToken);
        var ahora = DateTime.UtcNow;

        var token = await _db.DispositivosToken.IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.TokenHash == hash && !d.Revocado
                && (d.ExpiraEn == null || d.ExpiraEn > ahora));

        if (token is null)
        {
            return AuthenticateResult.Fail("Token de dispositivo invalido, revocado o expirado.");
        }

        token.UltimoUsoEn = ahora;
        await _db.SaveChangesAsync(CancellationToken.None);

        var claims = new[] { new Claim(DeviceClaimTypes.IdActivo, token.IdActivo.ToString()) };
        var identity = new ClaimsIdentity(claims, DeviceTokenDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, DeviceTokenDefaults.AuthenticationScheme);

        return AuthenticateResult.Success(ticket);
    }
}
```

### 8.3 Registrar el esquema nuevo

Archivo: `src/SLCDM.Api/Authentication/AuthenticationExtensions.cs`

Agregar este metodo a la clase que ya existe (junto a `AddJwtAuthentication`,
sin modificar ese metodo):

```csharp
    public static IServiceCollection AddDeviceTokenAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication()
            .AddScheme<AuthenticationSchemeOptions, DeviceTokenAuthenticationHandler>(
                DeviceTokenDefaults.AuthenticationScheme, _ => { });

        return services;
    }
```

Archivo: `src/SLCDM.Api/Program.cs`

Agregar esta linea despues de `builder.Services.AddJwtAuthentication(...)`:

```csharp
builder.Services.AddDeviceTokenAuthentication();
```

### 8.4 El controller

Archivo: `src/SLCDM.Api/Controllers/DispositivosController.cs`

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SLCDM.Api.Authentication;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Security;
using SLCDM.Application.Features.Dispositivos;
using SLCDM.Application.Features.Dispositivos.Commands;

namespace SLCDM.Api.Controllers;

public sealed class DispositivosController : ApiControllerBase
{
    private readonly ICommandHandler<RegistrarDispositivoCommand, DispositivoTokenDto> _registrar;
    private readonly ICommandHandler<AutoRegistrarDispositivoCommand, DispositivoTokenDto> _autoRegistrar;
    private readonly ICommandHandler<RevocarDispositivoCommand> _revocar;
    private readonly ICommandHandler<RegistrarUbicacionCommand> _ping;

    public DispositivosController(
        ICommandHandler<RegistrarDispositivoCommand, DispositivoTokenDto> registrar,
        ICommandHandler<AutoRegistrarDispositivoCommand, DispositivoTokenDto> autoRegistrar,
        ICommandHandler<RevocarDispositivoCommand> revocar,
        ICommandHandler<RegistrarUbicacionCommand> ping)
    {
        _registrar = registrar;
        _autoRegistrar = autoRegistrar;
        _revocar = revocar;
        _ping = ping;
    }

    [HttpPost]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistrarDispositivoCommand command,
        CancellationToken cancellationToken)
    {
        var dto = await _registrar.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [HttpPost("auto-registro")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> AutoRegistrar(
        [FromBody] AutoRegistrarDispositivoCommand command,
        CancellationToken cancellationToken)
    {
        var dto = await _autoRegistrar.HandleAsync(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, dto);
    }

    [HttpPost("{id:int}/revocar")]
    [Authorize(Roles = Roles.EscrituraOperativa)]
    public async Task<IActionResult> Revocar(int id, CancellationToken cancellationToken)
    {
        await _revocar.HandleAsync(new RevocarDispositivoCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("ping")]
    [Authorize(AuthenticationSchemes = DeviceTokenDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Ping(
        [FromBody] DevicePingRequest body,
        CancellationToken cancellationToken)
    {
        var idActivoClaim = User.FindFirstValue(DeviceClaimTypes.IdActivo);
        if (!int.TryParse(idActivoClaim, out var idActivo))
        {
            return Unauthorized();
        }

        await _ping.HandleAsync(new RegistrarUbicacionCommand(idActivo, body.Bssid), cancellationToken);
        return NoContent();
    }
}

public sealed record DevicePingRequest(string Bssid);
```

Nota que `auto-registro` es el unico endpoint de todo el sistema marcado
`[AllowAnonymous]`. Es intencional: es el punto de entrada antes de que el
equipo tenga cualquier credencial. Su proteccion no es el JWT, es la
combinacion de `InstallKey` + que el numero de serie exista en el catalogo
de activos. Por ser el unico endpoint sin autenticacion fuerte, conviene
aplicarle una politica de rate limiting mas estricta que al resto (el
proyecto ya tiene `AddRateLimitingPolicies()` configurado, ahi se agregaria
una regla especifica para esta ruta).

### 8.5 Configuracion nueva en appsettings.json

Archivo: `src/SLCDM.Api/appsettings.json`

Agregar esta seccion (usa valores reales, no los de ejemplo, en produccion
via User Secrets o variables de entorno, igual que ya hacen con
`JwtSettings:SecretKey`):

```json
  "DeviceTracking": {
    "Pepper": "CAMBIAR_ESTO_USAR_USER_SECRETS_EN_PRODUCCION_32+",
    "InstallKey": "CAMBIAR_ESTO_ES_LA_LLAVE_DEL_LOTE_DE_INSTALACION",
    "TokenExpiryDays": 365
  }
```

## 9. El agente (el programa que corre en cada equipo)

Este es un proyecto de .NET aparte, fuera de la solucion SLCDM (no vive en
el backend, corre en las computadoras de los usuarios).

### 9.1 El archivo de proyecto

Archivo: `SLCDM.Agent.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Worker">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <SelfContained>true</SelfContained>
    <PublishSingleFile>true</PublishSingleFile>
    <UseWindowsService>true</UseWindowsService>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting.WindowsServices" Version="8.0.0" />
    <PackageReference Include="System.Management" Version="8.0.0" />
  </ItemGroup>
</Project>
```

`SelfContained` + `PublishSingleFile` hacen que el resultado sea un solo
`.exe` que no depende de que el equipo tenga el runtime de .NET instalado
por separado.

### 9.2 Lectura de la huella de hardware

Archivo: `HuellaHardware.cs`

Usa WMI (Windows Management Instrumentation), la forma estandar de leer
informacion de hardware en Windows.

```csharp
using System.Management;

namespace SLCDM.Agent;

public static class HuellaHardware
{
    public static string? LeerNumeroSerieBios()
    {
        using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
        foreach (ManagementObject item in searcher.Get())
        {
            return item["SerialNumber"]?.ToString()?.Trim();
        }
        return null;
    }

    public static string? LeerNumeroSerieMotherboard()
    {
        using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
        foreach (ManagementObject item in searcher.Get())
        {
            return item["SerialNumber"]?.ToString()?.Trim();
        }
        return null;
    }
}
```

### 9.3 El worker (el ciclo principal del servicio)

Archivo: `Worker.cs`

```csharp
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace SLCDM.Agent;

public sealed class Worker : BackgroundService
{
    private readonly HttpClient _http;
    private readonly string _installKey;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(15);

    public Worker(HttpClient http, string installKey)
    {
        _http = http;
        _installKey = installKey;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = CredencialAlmacen.LeerToken();

        if (token is null)
        {
            token = await AutoRegistrarseAsync(stoppingToken);
            CredencialAlmacen.GuardarToken(token);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var bssid = ObtenerBssidConectado();
            if (bssid is not null)
            {
                await EnviarPingAsync(token, bssid, stoppingToken);
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task<string> AutoRegistrarseAsync(CancellationToken cancellationToken)
    {
        var numeroSerie = HuellaHardware.LeerNumeroSerieBios()
            ?? throw new InvalidOperationException("No se pudo leer el numero de serie del equipo.");

        var response = await _http.PostAsJsonAsync("api/dispositivos/auto-registro", new
        {
            NumeroSerie = numeroSerie,
            InstallKey = _installKey
        }, cancellationToken);

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<RespuestaAutoRegistro>(cancellationToken: cancellationToken);
        return dto!.TokenCrudo;
    }

    private static string? ObtenerBssidConectado()
    {
        var psi = new ProcessStartInfo("netsh", "wlan show interfaces")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proceso = Process.Start(psi);
        if (proceso is null)
        {
            return null;
        }

        var salida = proceso.StandardOutput.ReadToEnd();
        proceso.WaitForExit();

        var match = Regex.Match(salida, @"BSSID\s*:\s*([0-9a-fA-F:]{17})");
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    private async Task EnviarPingAsync(string token, string bssid, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/dispositivos/ping");
        request.Headers.Add("X-Device-Token", token);
        request.Content = JsonContent.Create(new { Bssid = bssid });

        try
        {
            using var response = await _http.SendAsync(request, cancellationToken);
        }
        catch
        {
            // Sin conexion al backend: se reintenta en el siguiente ciclo.
            // No se detiene el servicio por un fallo de red puntual.
        }
    }

    private sealed record RespuestaAutoRegistro(int Id, int IdActivo, string TokenCrudo, DateTime CreadoEn, DateTime? ExpiraEn);
}
```

### 9.4 Guardar el token de forma segura

Archivo: `CredencialAlmacen.cs`

No se guarda el token en un archivo de texto plano (cualquiera con acceso al
disco lo veria). Se usa el Credential Manager de Windows, el mismo lugar
donde Windows guarda contrasenas de redes y de aplicaciones.

```csharp
using CredentialManagement;

namespace SLCDM.Agent;

public static class CredencialAlmacen
{
    private const string Target = "SLCDM-DeviceToken";

    public static string? LeerToken()
    {
        using var cred = new Credential { Target = Target };
        return cred.Load() ? cred.Password : null;
    }

    public static void GuardarToken(string token)
    {
        using var cred = new Credential
        {
            Target = Target,
            Username = "device",
            Password = token,
            PersistanceType = PersistanceType.LocalComputer
        };
        cred.Save();
    }
}
```

Necesita el paquete NuGet `CredentialManagement` (o el equivalente
`CredentialManagement.Standard` si el original no soporta .NET 8).

### 9.5 El arranque del programa

Archivo: `Program.cs`

```csharp
using SLCDM.Agent;

var builder = Host.CreateApplicationBuilder(args);

var backendUrl = builder.Configuration["Backend:BaseUrl"]
    ?? throw new InvalidOperationException("Falta Backend:BaseUrl en la configuracion.");
var installKey = builder.Configuration["Backend:InstallKey"]
    ?? throw new InvalidOperationException("Falta Backend:InstallKey en la configuracion.");

builder.Services.AddHttpClient<Worker>(client =>
{
    client.BaseAddress = new Uri(backendUrl);
});

builder.Services.AddHostedService<Worker>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SLCDM Agente de Rastreo";
});

var host = builder.Build();
host.Run();
```

## 10. Como se instala en cada equipo

### 10.1 Publicar el ejecutable

Desde la carpeta del proyecto del agente:

```
dotnet publish -c Release -r win-x64 -o ./publish
```

Esto genera un solo `.exe` en `./publish`, listo para copiar a cualquier
equipo Windows sin instalar nada mas.

### 10.2 Script de instalacion

Archivo: `instalar.ps1` (se corre como administrador en el equipo destino)

```powershell
param(
    [Parameter(Mandatory=$true)][string]$BackendUrl,
    [Parameter(Mandatory=$true)][string]$InstallKey
)

$installPath = "C:\Program Files\SLCDM"
New-Item -ItemType Directory -Force -Path $installPath | Out-Null
Copy-Item -Path ".\publish\*" -Destination $installPath -Recurse -Force

$config = @{
    "Backend" = @{
        "BaseUrl" = $BackendUrl
        "InstallKey" = $InstallKey
    }
} | ConvertTo-Json
Set-Content -Path "$installPath\appsettings.Production.json" -Value $config

New-Service -Name "SLCDMAgente" `
  -BinaryPathName "$installPath\SLCDMAgente.exe" `
  -DisplayName "SLCDM Agente de Rastreo" `
  -StartupType Automatic

# Recovery: si el servicio se cae, Windows lo reinicia solo. No hace falta
# un segundo servicio "vigilante" -- esto ya lo hace el sistema operativo.
sc.exe failure SLCDMAgente reset= 86400 actions= restart/60000/restart/60000/restart/60000

Start-Service -Name "SLCDMAgente"
```

Se corre asi, pasandole la URL del backend y la llave de instalacion del
lote (la misma para todos los equipos de ese despliegue):

```
.\instalar.ps1 -BackendUrl "https://api.tuempresa.com/" -InstallKey "la-llave-del-lote"
```

El equipo, en su primer arranque del servicio, lee su propio numero de
serie y se autoregistra solo contra el backend. No hace falta que nadie de
TI central genere ni copie un token por equipo.

### 10.3 Desinstalar o actualizar

Para actualizar el binario a una version nueva:

```powershell
Stop-Service -Name "SLCDMAgente"
Copy-Item -Path ".\publish\*" -Destination "C:\Program Files\SLCDM" -Recurse -Force
Start-Service -Name "SLCDMAgente"
```

Para desinstalar por completo:

```powershell
Stop-Service -Name "SLCDMAgente"
sc.exe delete SLCDMAgente
Remove-Item -Recurse -Force "C:\Program Files\SLCDM"
```

## 11. Cosas pendientes antes de construir esto en serio

Verificar que `Activo.NumeroSerie` este bien capturado y sea unico en el
inventario actual. El autoregistro depende de eso: si hay series vacias o
duplicadas, esos equipos no van a poder autoregistrarse.

Decidir el mecanismo de conectividad para los kioscos de mall (idealmente un
punto de acceso propio de la empresa, no el Wi-Fi publico del mall, por el
problema del portal cautivo que se explico en la seccion 4).

Catalogar los BSSID de cada sede en la tabla `RedConocida` antes de activar
el rastreo ahi (si una ubicacion no tiene ningun BSSID cargado, el sistema
simplemente no va a poder ubicar los equipos que esten ahi, aunque el resto
funcione bien).

Definir una politica de uso que los empleados firmen, dejando claro que el
equipo de la empresa reporta su ubicacion por red (no pantalla, no teclas,
no navegacion) para efectos de inventario y control de activos. Esto es mas
por transparencia y respaldo legal que por un requisito tecnico.

Asignar un numero de sprint/ticket (BE-XX) a este trabajo dentro del
roadmap, para que quede planificado igual que el resto de funcionalidades.
