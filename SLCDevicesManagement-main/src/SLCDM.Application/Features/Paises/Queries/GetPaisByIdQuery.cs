using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Paises.Queries;

public sealed record GetPaisByIdQuery(int Id);

public sealed class GetPaisByIdQueryValidator : AbstractValidator<GetPaisByIdQuery>
{
    public GetPaisByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id pais");
    }
}

public sealed class GetPaisByIdQueryHandler : IQueryHandler<GetPaisByIdQuery, PaisDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetPaisByIdQuery> _validator;

    public GetPaisByIdQueryHandler(IApplicationDbContext db, IValidator<GetPaisByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<PaisDto> HandleAsync(GetPaisByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Paises.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Pais", query.Id);

        return entity.Adapt<PaisDto>();
    }
}
