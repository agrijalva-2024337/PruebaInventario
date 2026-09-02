using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Asignaciones;

internal static class ActivoBajaRules
{
    public const string MensajeActivoDadoDeBaja =
        "El activo esta dado de baja. No puede asignarse, trasladarse ni enviarse a mantenimiento.";

    public static async Task<bool> EstaDadoDeBajaAsync(
        IApplicationDbContext db,
        int idActivo,
        CancellationToken cancellationToken)
    {
        var tipos = await db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsBaja = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Baja))
            .Select(t => t.Id)
            .ToList();

        if (idsBaja.Count == 0)
        {
            return false;
        }

        return await db.Asignaciones.AnyAsync(
            a => a.IdActivo == idActivo && a.Activa && idsBaja.Contains(a.IdTipoAsignacion),
            cancellationToken);
    }

    public static async Task<HashSet<int>> IdsDadosDeBajaAsync(
        IApplicationDbContext db,
        CancellationToken cancellationToken)
    {
        var tipos = await db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsBaja = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Baja))
            .Select(t => t.Id)
            .ToList();

        if (idsBaja.Count == 0)
        {
            return [];
        }

        var ids = await db.Asignaciones
            .AsNoTracking()
            .Where(a => a.Activa && idsBaja.Contains(a.IdTipoAsignacion))
            .Select(a => a.IdActivo)
            .ToListAsync(cancellationToken);

        return ids.ToHashSet();
    }
}
