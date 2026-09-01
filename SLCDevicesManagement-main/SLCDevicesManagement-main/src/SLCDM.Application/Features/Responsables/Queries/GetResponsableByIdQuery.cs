using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Responsables.Queries;

public sealed record GetResponsableByIdQuery(int Id);

public sealed class GetResponsableByIdQueryValidator : AbstractValidator<GetResponsableByIdQuery>
{
    public GetResponsableByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id responsable");
    }
}

public sealed class GetResponsableByIdQueryHandler : IQueryHandler<GetResponsableByIdQuery, ResponsableDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetResponsableByIdQuery> _validator;

    public GetResponsableByIdQueryHandler(IApplicationDbContext db, IValidator<GetResponsableByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<ResponsableDto> HandleAsync(GetResponsableByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Responsables.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Responsable", query.Id);

        return entity.Adapt<ResponsableDto>();
    }
}
