using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Proveedores.Queries;

public sealed record GetProveedorByIdQuery(int Id);

public sealed class GetProveedorByIdQueryValidator : AbstractValidator<GetProveedorByIdQuery>
{
    public GetProveedorByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id proveedor");
    }
}

public sealed class GetProveedorByIdQueryHandler : IQueryHandler<GetProveedorByIdQuery, ProveedorDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetProveedorByIdQuery> _validator;

    public GetProveedorByIdQueryHandler(IApplicationDbContext db, IValidator<GetProveedorByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<ProveedorDto> HandleAsync(GetProveedorByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Proveedores.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Proveedor", query.Id);

        return entity.Adapt<ProveedorDto>();
    }
}
