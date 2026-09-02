using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.DetallesActivos.Queries;

public sealed record GetDetalleActivoByIdQuery(int Id);

public sealed class GetDetalleActivoByIdQueryValidator : AbstractValidator<GetDetalleActivoByIdQuery>
{
    public GetDetalleActivoByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id detalle activo");
    }
}

public sealed class GetDetalleActivoByIdQueryHandler : IQueryHandler<GetDetalleActivoByIdQuery, DetalleActivoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetDetalleActivoByIdQuery> _validator;

    public GetDetalleActivoByIdQueryHandler(
        IApplicationDbContext db,
        IValidator<GetDetalleActivoByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<DetalleActivoDto> HandleAsync(
        GetDetalleActivoByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.DetallesActivos.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("DetalleActivo", query.Id);

        return entity.Adapt<DetalleActivoDto>();
    }
}
