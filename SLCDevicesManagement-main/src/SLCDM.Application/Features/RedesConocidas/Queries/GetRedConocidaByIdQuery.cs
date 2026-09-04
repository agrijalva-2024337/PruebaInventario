using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.RedesConocidas.Queries;

public sealed record GetRedConocidaByIdQuery(int Id);

public sealed class GetRedConocidaByIdQueryValidator : AbstractValidator<GetRedConocidaByIdQuery>
{
    public GetRedConocidaByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id red conocida");
    }
}

public sealed class GetRedConocidaByIdQueryHandler : IQueryHandler<GetRedConocidaByIdQuery, RedConocidaDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetRedConocidaByIdQuery> _validator;

    public GetRedConocidaByIdQueryHandler(IApplicationDbContext db, IValidator<GetRedConocidaByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<RedConocidaDto> HandleAsync(GetRedConocidaByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.RedesConocidas.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("RedConocida", query.Id);

        return entity.Adapt<RedConocidaDto>();
    }
}
