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

    public static async Task<bool> ActivoEnEmpresaDeJornadaAsync(
        IApplicationDbContext db,
        int idActivo,
        int idHistoricoInventario,
        CancellationToken cancellationToken)
    {
        var idEmpresaJornada = await (
            from h in db.HistoricosInventario.AsNoTracking()
            join s in db.Sedes.AsNoTracking() on h.IdSede equals s.Id
            where h.Id == idHistoricoInventario
            select (int?)s.IdEmpresa
        ).FirstOrDefaultAsync(cancellationToken);

        var idEmpresaActivo = await (
            from a in db.Activos.AsNoTracking()
            join u in db.Ubicaciones.AsNoTracking() on a.IdUbicacion equals u.Id
            join s in db.Sedes.AsNoTracking() on u.IdSede equals s.Id
            where a.Id == idActivo
            select (int?)s.IdEmpresa
        ).FirstOrDefaultAsync(cancellationToken);

        if (!idEmpresaJornada.HasValue || !idEmpresaActivo.HasValue)
        {
            return true;
        }

        return idEmpresaJornada.Value == idEmpresaActivo.Value;
    }

    public static int? ParseIdUbicacionDelimite(string? observaciones)
    {
        if (string.IsNullOrWhiteSpace(observaciones))
        {
            return null;
        }

        const string prefix = "[id_ubicacion=";
        var start = observaciones.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
        {
            return null;
        }

        var from = start + prefix.Length;
        var end = observaciones.IndexOf(']', from);
        if (end < 0)
        {
            return null;
        }

        return int.TryParse(observaciones[from..end], out var id) && id > 0 ? id : null;
    }

    public static string ConUbicacionDelimite(int? idUbicacion, string? observaciones)
    {
        var resto = observaciones?.Trim() ?? string.Empty;
        if (idUbicacion is not > 0)
        {
            return resto;
        }

        var texto = $"[id_ubicacion={idUbicacion.Value}] {resto}".Trim();
        return texto.Length <= 300 ? texto : texto[..300];
    }

    public static bool EsOtraUbicacion(string? observaciones) =>
        !string.IsNullOrWhiteSpace(observaciones)
        && observaciones.Contains("Otra ubicacion", StringComparison.OrdinalIgnoreCase);
}
