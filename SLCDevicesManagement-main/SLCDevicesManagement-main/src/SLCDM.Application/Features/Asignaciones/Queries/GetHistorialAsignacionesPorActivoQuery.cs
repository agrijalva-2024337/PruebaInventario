using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Asignaciones.Queries;

public sealed record GetHistorialAsignacionesPorActivoQuery(int IdActivo);

public sealed class GetHistorialAsignacionesPorActivoQueryValidator
    : AbstractValidator<GetHistorialAsignacionesPorActivoQuery>
{
    public GetHistorialAsignacionesPorActivoQueryValidator()
    {
        RuleFor(x => x.IdActivo).RequiredId("id activo");
    }
}

public sealed class GetHistorialAsignacionesPorActivoQueryHandler
    : IQueryHandler<GetHistorialAsignacionesPorActivoQuery, IReadOnlyList<AsignacionHistorialDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetHistorialAsignacionesPorActivoQuery> _validator;

    public GetHistorialAsignacionesPorActivoQueryHandler(
        IApplicationDbContext db,
        IValidator<GetHistorialAsignacionesPorActivoQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<AsignacionHistorialDto>> HandleAsync(
        GetHistorialAsignacionesPorActivoQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var existeActivo = await _db.Activos.AnyAsync(a => a.Id == query.IdActivo, cancellationToken);
        if (!existeActivo)
        {
            throw new NotFoundException("Activo", query.IdActivo);
        }

        return await _db.Asignaciones
            .AsNoTracking()
            .Where(a => a.IdActivo == query.IdActivo)
            .OrderByDescending(a => a.FechaAsignacion)
            .Select(a => new AsignacionHistorialDto(
                a.Id,
                a.IdActivo,
                a.IdUsuario,
                a.Usuario != null ? ((a.Usuario.Nombres + " " + a.Usuario.Apellidos).Trim()) : string.Empty,
                a.IdResponsable,
                a.Responsable != null ? a.Responsable.NombreCompleto : string.Empty,
                a.IdUbicacion,
                a.Ubicacion != null ? a.Ubicacion.Nombre : string.Empty,
                a.IdTipoAsignacion,
                a.TipoAsignacion != null ? a.TipoAsignacion.Nombre : string.Empty,
                a.FechaAsignacion,
                a.FechaDevolucion,
                a.Activa,
                a.Observaciones))
            .ToListAsync(cancellationToken);
    }
}
