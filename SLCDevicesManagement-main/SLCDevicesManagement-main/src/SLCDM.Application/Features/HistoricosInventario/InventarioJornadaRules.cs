using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.HistoricosInventario;

internal static class InventarioJornadaRules
{
    public static async Task<int?> SedeTeoricaDelActivoAsync(
        IApplicationDbContext db,
        int idActivo,
        CancellationToken cancellationToken)
    {
        return await db.Activos
            .AsNoTracking()
            .Where(a => a.Id == idActivo)
            .Join(
                db.Ubicaciones.AsNoTracking(),
                a => a.IdUbicacion,
                u => u.Id,
                (_, u) => (int?)u.IdSede)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<bool> ActivoEnSedeDeJornadaAsync(
        IApplicationDbContext db,
        int idActivo,
        int idHistoricoInventario,
        CancellationToken cancellationToken)
    {
        var idSedeJornada = await db.HistoricosInventario
            .AsNoTracking()
            .Where(h => h.Id == idHistoricoInventario)
            .Select(h => (int?)h.IdSede)
            .FirstOrDefaultAsync(cancellationToken);

        if (!idSedeJornada.HasValue)
        {
            return true;
        }

        var idSedeActivo = await SedeTeoricaDelActivoAsync(db, idActivo, cancellationToken);
        if (!idSedeActivo.HasValue)
        {
            return true;
        }

        return idSedeActivo.Value == idSedeJornada.Value;
    }
}
