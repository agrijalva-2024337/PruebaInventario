using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.HistorialActivos.Queries;

public sealed record GetHistorialActivoByIdQuery(int Id);

public sealed class GetHistorialActivoByIdQueryValidator : AbstractValidator<GetHistorialActivoByIdQuery>
{
    public GetHistorialActivoByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id historial activo");
    }
}

public sealed class GetHistorialActivoByIdQueryHandler : IQueryHandler<GetHistorialActivoByIdQuery, HistorialActivoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetHistorialActivoByIdQuery> _validator;

    public GetHistorialActivoByIdQueryHandler(
        IApplicationDbContext db,
        IValidator<GetHistorialActivoByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<HistorialActivoDto> HandleAsync(
        GetHistorialActivoByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.HistorialActivos.AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("HistorialActivo", query.Id);

        return entity.Adapt<HistorialActivoDto>();
    }
}
