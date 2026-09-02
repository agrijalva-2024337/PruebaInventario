using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.TiposAsignacion.Queries;

public sealed record GetTipoAsignacionByIdQuery(int Id);

public sealed class GetTipoAsignacionByIdQueryValidator : AbstractValidator<GetTipoAsignacionByIdQuery>
{
    public GetTipoAsignacionByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id tipo asignacion");
    }
}

public sealed class GetTipoAsignacionByIdQueryHandler : IQueryHandler<GetTipoAsignacionByIdQuery, TipoAsignacionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetTipoAsignacionByIdQuery> _validator;

    public GetTipoAsignacionByIdQueryHandler(IApplicationDbContext db, IValidator<GetTipoAsignacionByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<TipoAsignacionDto> HandleAsync(GetTipoAsignacionByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.TiposAsignacion.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("TipoAsignacion", query.Id);

        return entity.Adapt<TipoAsignacionDto>();
    }
}
