using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Sedes.Queries;

public sealed record GetSedeByIdQuery(int Id);

public sealed class GetSedeByIdQueryValidator : AbstractValidator<GetSedeByIdQuery>
{
    public GetSedeByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id sede");
    }
}

public sealed class GetSedeByIdQueryHandler : IQueryHandler<GetSedeByIdQuery, SedeDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetSedeByIdQuery> _validator;

    public GetSedeByIdQueryHandler(IApplicationDbContext db, IValidator<GetSedeByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<SedeDto> HandleAsync(GetSedeByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Sedes.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Sede", query.Id);

        return entity.Adapt<SedeDto>();
    }
}
