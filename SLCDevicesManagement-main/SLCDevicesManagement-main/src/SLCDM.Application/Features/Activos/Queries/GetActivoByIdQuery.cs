using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Activos.Queries;

public sealed record GetActivoByIdQuery(int Id);

public sealed class GetActivoByIdQueryValidator : AbstractValidator<GetActivoByIdQuery>
{
    public GetActivoByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id activo");
    }
}

public sealed class GetActivoByIdQueryHandler : IQueryHandler<GetActivoByIdQuery, ActivoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetActivoByIdQuery> _validator;

    public GetActivoByIdQueryHandler(IApplicationDbContext db, IValidator<GetActivoByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<ActivoDto> HandleAsync(GetActivoByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Activos.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Activo", query.Id);

        return entity.Adapt<ActivoDto>();
    }
}
