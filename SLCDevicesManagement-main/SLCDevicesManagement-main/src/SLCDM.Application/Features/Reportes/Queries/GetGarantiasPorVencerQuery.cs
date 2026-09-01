using FluentValidation;
using Mapster;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Features.Activos;

namespace SLCDM.Application.Features.Reportes.Queries;

public sealed record GetGarantiasPorVencerQuery(int? IdEmpresa = null, int Dias = 30);

public sealed class GetGarantiasPorVencerQueryValidator : AbstractValidator<GetGarantiasPorVencerQuery>
{
    public GetGarantiasPorVencerQueryValidator()
    {
        RuleFor(x => x.Dias)
            .InclusiveBetween(1, 365).WithMessage("El campo dias debe estar entre 1 y 365.");
    }
}

public sealed class GetGarantiasPorVencerQueryHandler
    : IQueryHandler<GetGarantiasPorVencerQuery, IReadOnlyList<GarantiaPorVencerDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetGarantiasPorVencerQuery> _validator;

    public GetGarantiasPorVencerQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IValidator<GetGarantiasPorVencerQuery> validator)
    {
        _db = db;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<IReadOnlyList<GarantiaPorVencerDto>> HandleAsync(
        GetGarantiasPorVencerQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var idEmpresa = ActivoReporteConsulta.EmpresaEfectiva(_currentUser, query.IdEmpresa);
        var filas = await ActivoReporteConsulta.CargarAsync(_db, idEmpresa, cancellationToken);

        var desde = DateTime.UtcNow.Date;
        var hasta = desde.AddDays(query.Dias);

        return filas
            .Where(f => f.EstadoOperativo != ActivoEstadoOperativo.Baja)
            .Where(f =>
            {
                var fecha = f.Activo.FechaVencimientoGarantia.Date;
                return fecha >= desde && fecha <= hasta;
            })
            .OrderBy(f => f.Activo.FechaVencimientoGarantia)
            .Select(f => new GarantiaPorVencerDto(
                f.Activo.Adapt<ActivoDto>(),
                f.Activo.FechaVencimientoGarantia,
                (f.Activo.FechaVencimientoGarantia.Date - desde).Days,
                f.IdSede,
                f.NombreSede))
            .ToList();
    }
}
