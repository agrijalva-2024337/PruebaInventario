using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Estados.Queries;

public sealed record GetEstadoByIdQuery(int Id);

public sealed class GetEstadoByIdQueryValidator : AbstractValidator<GetEstadoByIdQuery>
{
    public GetEstadoByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id estado");
    }
}

public sealed class GetEstadoByIdQueryHandler : IQueryHandler<GetEstadoByIdQuery, EstadoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetEstadoByIdQuery> _validator;

    public GetEstadoByIdQueryHandler(IApplicationDbContext db, IValidator<GetEstadoByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<EstadoDto> HandleAsync(GetEstadoByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Estados.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Estado", query.Id);

        return entity.Adapt<EstadoDto>();
    }
}
