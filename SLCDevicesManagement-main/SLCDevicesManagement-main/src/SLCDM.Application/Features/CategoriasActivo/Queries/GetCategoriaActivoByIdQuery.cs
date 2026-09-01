using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.CategoriasActivo.Queries;

public sealed record GetCategoriaActivoByIdQuery(int Id);

public sealed class GetCategoriaActivoByIdQueryValidator : AbstractValidator<GetCategoriaActivoByIdQuery>
{
    public GetCategoriaActivoByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id categoria activo");
    }
}

public sealed class GetCategoriaActivoByIdQueryHandler : IQueryHandler<GetCategoriaActivoByIdQuery, CategoriaActivoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetCategoriaActivoByIdQuery> _validator;

    public GetCategoriaActivoByIdQueryHandler(
        IApplicationDbContext db,
        IValidator<GetCategoriaActivoByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<CategoriaActivoDto> HandleAsync(
        GetCategoriaActivoByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.CategoriasActivo.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("CategoriaActivo", query.Id);

        return entity.Adapt<CategoriaActivoDto>();
    }
}
