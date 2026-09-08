using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones;

internal static class AsignacionActivaRules
{
    public const string MensajeTrasladoOtraPersona =
        "El activo tiene una asignacion activa. El traslado solo mueve a la misma persona a otra ubicacion. Para entregarlo a otra persona, registre primero la devolucion.";

    public static async Task<Asignacion?> ObtenerEntregaActivaAsync(
        IApplicationDbContext db,
        int idActivo,
        CancellationToken cancellationToken)
    {
        var tipos = await db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsAsignacion = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Asignacion))
            .Select(t => t.Id)
            .ToList();

        if (idsAsignacion.Count == 0)
        {
            return null;
        }

        return await db.Asignaciones
            .Where(a => a.IdActivo == idActivo && a.Activa && idsAsignacion.Contains(a.IdTipoAsignacion))
            .OrderByDescending(a => a.FechaAsignacion)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
