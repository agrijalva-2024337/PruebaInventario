using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Areas.Queries;

public sealed record GetAreaByIdQuery(int Id);

public sealed class GetAreaByIdQueryValidator : AbstractValidator<GetAreaByIdQuery>
{
    public GetAreaByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id area");
    }
}

public sealed class GetAreaByIdQueryHandler : IQueryHandler<GetAreaByIdQuery, AreaDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetAreaByIdQuery> _validator;

    public GetAreaByIdQueryHandler(IApplicationDbContext db, IValidator<GetAreaByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<AreaDto> HandleAsync(GetAreaByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Areas.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Area", query.Id);

        return entity.Adapt<AreaDto>();
    }
}
