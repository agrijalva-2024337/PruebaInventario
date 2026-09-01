using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Ubicaciones.Queries;

public sealed record GetUbicacionByIdQuery(int Id);

public sealed class GetUbicacionByIdQueryValidator : AbstractValidator<GetUbicacionByIdQuery>
{
    public GetUbicacionByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id ubicacion");
    }
}

public sealed class GetUbicacionByIdQueryHandler : IQueryHandler<GetUbicacionByIdQuery, UbicacionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetUbicacionByIdQuery> _validator;

    public GetUbicacionByIdQueryHandler(IApplicationDbContext db, IValidator<GetUbicacionByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<UbicacionDto> HandleAsync(GetUbicacionByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Ubicaciones.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Ubicacion", query.Id);

        return entity.Adapt<UbicacionDto>();
    }
}
