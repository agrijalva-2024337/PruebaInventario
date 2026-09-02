using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Features.Reportes;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Activos;

internal static class ActivoContextoConsulta
{
    public static async Task<IReadOnlyDictionary<int, ActivoContexto>> CargarAsync(
        IApplicationDbContext db,
        IReadOnlyCollection<Activo> activos,
        IReadOnlyDictionary<int, ActivoEstadoInfo> estados,
        CancellationToken cancellationToken)
    {
        if (activos.Count == 0)
        {
            return new Dictionary<int, ActivoContexto>();
        }

        var idUbicaciones = activos.Select(a => a.IdUbicacion).Distinct().ToList();
        var ubicaciones = await (
            from u in db.Ubicaciones.AsNoTracking()
            join s in db.Sedes.AsNoTracking() on u.IdSede equals s.Id
            join e in db.Empresas.AsNoTracking() on s.IdEmpresa equals e.Id
            where idUbicaciones.Contains(u.Id)
            select new
            {
                u.Id,
                u.Nombre,
                IdSede = s.Id,
                NombreSede = s.Nombre,
                IdEmpresa = e.Id,
                NombreEmpresa = e.Nombre
            }
        ).ToListAsync(cancellationToken);

        var ubicacionById = ubicaciones.ToDictionary(x => x.Id);

        var idsResponsable = estados.Values
            .Where(v => v.IdResponsable.HasValue)
            .Select(v => v.IdResponsable!.Value)
            .Distinct()
            .ToList();

        var responsables = await (
            from r in db.Responsables.AsNoTracking()
            join a in db.Areas.AsNoTracking() on r.IdArea equals a.Id
            where idsResponsable.Contains(r.Id)
            select new { r.Id, r.NombreCompleto, a.Nombre }
        ).ToListAsync(cancellationToken);

        var responsableById = responsables.ToDictionary(x => x.Id);

        var resultado = new Dictionary<int, ActivoContexto>();
        foreach (var activo in activos)
        {
            ubicacionById.TryGetValue(activo.IdUbicacion, out var ub);
            var info = estados.GetValueOrDefault(activo.Id);
            responsableById.TryGetValue(info?.IdResponsable ?? 0, out var resp);

            resultado[activo.Id] = new ActivoContexto(
                ub?.IdEmpresa,
                ub?.NombreEmpresa,
                ub?.IdSede,
                ub?.NombreSede,
                ub?.Nombre,
                resp?.Nombre,
                info?.IdResponsable,
                resp?.NombreCompleto);
        }

        return resultado;
    }
}
