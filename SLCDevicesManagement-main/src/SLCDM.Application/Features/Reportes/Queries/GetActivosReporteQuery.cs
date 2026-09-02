using FluentValidation;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Application.Features.Activos;

namespace SLCDM.Application.Features.Reportes.Queries;

public sealed record GetActivosReporteQuery(
    string? Estado = null,
    int? IdEmpresa = null,
    int? IdSede = null,
    int? IdCategoriaActivo = null,
    int? IdResponsable = null,
    int Skip = 0,
    int Take = 100);

public sealed class GetActivosReporteQueryValidator : AbstractValidator<GetActivosReporteQuery>
{
    public GetActivosReporteQueryValidator()
    {
        RuleFor(x => x.Estado)
            .Must(estado => string.IsNullOrWhiteSpace(estado) || ActivoEstadoOperativo.EsEstadoValido(estado))
            .WithMessage("El campo estado debe ser disponible, asignado, mantenimiento o baja.");

        RuleFor(x => x.IdEmpresa).OptionalId("id empresa");
        RuleFor(x => x.IdSede).OptionalId("id sede");
        RuleFor(x => x.IdCategoriaActivo).OptionalId("id categoria activo");
        RuleFor(x => x.IdResponsable).OptionalId("id responsable");

        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("El campo skip no puede ser negativo.");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 500).WithMessage("El campo take debe estar entre 1 y 500.");
    }
}

public sealed class GetActivosReporteQueryHandler
    : IQueryHandler<GetActivosReporteQuery, IReadOnlyList<ActivoReporteDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<GetActivosReporteQuery> _validator;

    public GetActivosReporteQueryHandler(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IValidator<GetActivosReporteQuery> validator)
    {
        _db = db;
        _currentUser = currentUser;
        _validator = validator;
    }

    public async Task<IReadOnlyList<ActivoReporteDto>> HandleAsync(
        GetActivosReporteQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var idEmpresa = ActivoReporteConsulta.EmpresaEfectiva(_currentUser, query.IdEmpresa);
        IEnumerable<ActivoInventarioRow> filas = await ActivoReporteConsulta.CargarAsync(
            _db, idEmpresa, cancellationToken);

        if (ActivoEstadoOperativo.TryNormalizar(query.Estado, out var estado))
        {
            filas = filas.Where(f => f.EstadoOperativo == estado);
        }

        if (query.IdSede is > 0)
        {
            filas = filas.Where(f => f.IdSede == query.IdSede.Value);
        }

        if (query.IdCategoriaActivo is > 0)
        {
            filas = filas.Where(f => f.Activo.IdCategoriaActivo == query.IdCategoriaActivo.Value);
        }

        if (query.IdResponsable is > 0)
        {
            filas = filas.Where(f => f.IdResponsable == query.IdResponsable.Value);
        }

        return filas
            .OrderBy(f => f.Activo.Nombre)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(f => new ActivoReporteDto(
                ActivoDtoFactory.CreateFromRow(f),
                f.EstadoOperativo,
                f.IdSede,
                f.NombreSede,
                f.IdResponsable))
            .ToList();
    }
}
