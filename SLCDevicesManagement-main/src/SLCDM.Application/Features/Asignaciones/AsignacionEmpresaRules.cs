using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Asignaciones;

internal static class AsignacionEmpresaRules
{
    public static async Task<int?> EmpresaIdDeUbicacionAsync(
        IApplicationDbContext db,
        int idUbicacion,
        CancellationToken cancellationToken)
    {
        return await db.Ubicaciones
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(u => u.Id == idUbicacion)
            .Join(
                db.Sedes.AsNoTracking().IgnoreQueryFilters(),
                u => u.IdSede,
                s => s.Id,
                (_, s) => (int?)s.IdEmpresa)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<int?> EmpresaIdDeActivoAsync(
        IApplicationDbContext db,
        int idActivo,
        CancellationToken cancellationToken)
    {
        var idUbicacion = await db.Activos
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(a => a.Id == idActivo)
            .Select(a => (int?)a.IdUbicacion)
            .FirstOrDefaultAsync(cancellationToken);

        if (!idUbicacion.HasValue)
        {
            return null;
        }

        return await EmpresaIdDeUbicacionAsync(db, idUbicacion.Value, cancellationToken);
    }

    public static async Task<bool> MismaEmpresaAsync(
        IApplicationDbContext db,
        int idActivo,
        int idUbicacionDestino,
        CancellationToken cancellationToken)
    {
        var empresaActivo = await EmpresaIdDeActivoAsync(db, idActivo, cancellationToken);
        var empresaDestino = await EmpresaIdDeUbicacionAsync(db, idUbicacionDestino, cancellationToken);

        if (!empresaActivo.HasValue || !empresaDestino.HasValue)
        {
            return true;
        }

        return empresaActivo.Value == empresaDestino.Value;
    }
}
