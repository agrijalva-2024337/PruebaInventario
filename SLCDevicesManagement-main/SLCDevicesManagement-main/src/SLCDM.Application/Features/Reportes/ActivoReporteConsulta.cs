using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Features.Asignaciones;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Reportes;

internal static class ActivoEstadoOperativo
{
    public const string Disponible = "disponible";
    public const string Asignado = "asignado";
    public const string Mantenimiento = "mantenimiento";
    public const string Baja = "baja";

    public static bool EsEstadoValido(string? raw) =>
        TryNormalizar(raw, out _);

    public static bool TryNormalizar(string? raw, out string estado)
    {
        var n = TipoAsignacionNombres.Normalizar(raw).ToLowerInvariant();
        estado = n switch
        {
            Disponible or "disponibles" => Disponible,
            Asignado or "asignados" => Asignado,
            Mantenimiento or "enmantenimiento" => Mantenimiento,
            Baja or "dadosdebaja" or "dadodebaja" => Baja,
            _ => string.Empty
        };
        return estado.Length > 0;
    }
}

internal sealed record ActivoInventarioRow(
    Activo Activo,
    int IdSede,
    string NombreSede,
    int IdEmpresa,
    string NombreEmpresa,
    string NombreCategoria,
    string EstadoOperativo,
    int? IdResponsable);

internal static class ActivoReporteConsulta
{
    public static int? EmpresaEfectiva(ICurrentUserService user, int? idEmpresaSolicitada)
    {
        if (user.IsAdministradorGeneral)
        {
            return idEmpresaSolicitada is > 0 ? idEmpresaSolicitada : null;
        }

        return user.EmpresaId;
    }

    public static async Task<IReadOnlyList<ActivoInventarioRow>> CargarAsync(
        IApplicationDbContext db,
        int? idEmpresa,
        CancellationToken cancellationToken)
    {
        var query =
            from a in db.Activos.AsNoTracking()
            join u in db.Ubicaciones.AsNoTracking() on a.IdUbicacion equals u.Id
            join s in db.Sedes.AsNoTracking() on u.IdSede equals s.Id
            join e in db.Empresas.AsNoTracking() on s.IdEmpresa equals e.Id
            join c in db.CategoriasActivo.AsNoTracking() on a.IdCategoriaActivo equals c.Id
            select new
            {
                Activo = a,
                s.Id,
                NombreSede = s.Nombre,
                IdEmpresa = e.Id,
                NombreEmpresa = e.Nombre,
                NombreCategoria = c.Nombre
            };

        if (idEmpresa.HasValue)
        {
            query = query.Where(x => x.IdEmpresa == idEmpresa.Value);
        }

        var filas = await query.ToListAsync(cancellationToken);
        var estados = await ResolverEstadosAsync(db, cancellationToken);

        return filas
            .Select(x =>
            {
                var info = estados.GetValueOrDefault(x.Activo.Id);
                return new ActivoInventarioRow(
                    x.Activo,
                    x.Id,
                    x.NombreSede,
                    x.IdEmpresa,
                    x.NombreEmpresa,
                    x.NombreCategoria,
                    info?.Estado ?? ActivoEstadoOperativo.Disponible,
                    info?.IdResponsable);
            })
            .ToList();
    }

    private static async Task<Dictionary<int, EstadoAsignado>> ResolverEstadosAsync(
        IApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var tipos = await db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsBaja = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Baja))
            .Select(t => t.Id)
            .ToHashSet();
        var idsMantenimiento = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Mantenimiento))
            .Select(t => t.Id)
            .ToHashSet();
        var idsAsignacion = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Asignacion))
            .Select(t => t.Id)
            .ToHashSet();

        var activas = await db.Asignaciones
            .AsNoTracking()
            .Where(a => a.Activa)
            .Select(a => new { a.IdActivo, a.IdTipoAsignacion, a.IdResponsable, a.FechaAsignacion })
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<int, EstadoAsignado>();
        foreach (var grupo in activas.GroupBy(a => a.IdActivo))
        {
            if (grupo.Any(a => idsBaja.Contains(a.IdTipoAsignacion)))
            {
                resultado[grupo.Key] = new EstadoAsignado(ActivoEstadoOperativo.Baja, null);
                continue;
            }

            if (grupo.Any(a => idsMantenimiento.Contains(a.IdTipoAsignacion)))
            {
                resultado[grupo.Key] = new EstadoAsignado(ActivoEstadoOperativo.Mantenimiento, null);
                continue;
            }

            var asignacion = grupo
                .Where(a => idsAsignacion.Contains(a.IdTipoAsignacion))
                .OrderByDescending(a => a.FechaAsignacion)
                .FirstOrDefault();

            if (asignacion is not null)
            {
                resultado[grupo.Key] = new EstadoAsignado(ActivoEstadoOperativo.Asignado, asignacion.IdResponsable);
            }
        }

        return resultado;
    }

    private sealed record EstadoAsignado(string Estado, int? IdResponsable);
}
