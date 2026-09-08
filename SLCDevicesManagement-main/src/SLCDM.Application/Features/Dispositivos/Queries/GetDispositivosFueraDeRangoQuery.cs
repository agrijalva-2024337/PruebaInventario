using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Features.Dispositivos.Queries;

public sealed record GetDispositivosFueraDeRangoQuery;

public sealed record DispositivoFueraDeRangoDto(
    int IdActivo,
    string NombreActivo,
    int? IdUbicacionAsignada,
    int? IdUbicacionDetectada,
    DateTime? UltimoUsoEn,
    string? UltimoBssid,
    string? OrigenCoordenada,
    decimal? UltimaLatitud,
    decimal? UltimaLongitud);

public sealed class GetDispositivosFueraDeRangoQueryHandler
    : IQueryHandler<GetDispositivosFueraDeRangoQuery, IReadOnlyList<DispositivoFueraDeRangoDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetDispositivosFueraDeRangoQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<DispositivoFueraDeRangoDto>> HandleAsync(
        GetDispositivosFueraDeRangoQuery query,
        CancellationToken cancellationToken = default)
    {
        var items = _db.DispositivosToken.AsNoTracking()
            .IgnoreQueryFilters()
            .Where(d => d.FueraDeRango && !d.Revocado && d.Activo != null);

        if (!_currentUser.IsAdministradorGeneral && _currentUser.EmpresaId is int idEmpresa)
        {
            items = items.Where(d =>
                _db.Proveedores.IgnoreQueryFilters().Any(p =>
                    p.Id == d.Activo!.IdProveedor && p.IdEmpresa == idEmpresa));
        }

        return await items
            .Select(d => new DispositivoFueraDeRangoDto(
                d.IdActivo,
                d.Activo!.Nombre,
                d.Activo.IdUbicacion,
                d.UltimaUbicacionDetectadaId,
                d.UltimoUsoEn,
                d.UltimoBssid,
                d.OrigenCoordenada,
                d.UltimaLatitud,
                d.UltimaLongitud))
            .ToListAsync(cancellationToken);
    }
}
