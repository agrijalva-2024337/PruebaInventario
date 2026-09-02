using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Areas.Queries;

public sealed record GetAreasQuery(bool IncluirInhabilitados = false, int? IdSede = null);

public sealed class GetAreasQueryValidator : AbstractValidator<GetAreasQuery>
{
    public GetAreasQueryValidator()
    {
        RuleFor(x => x.IdSede).OptionalId("id sede");
    }
}

public sealed class GetAreasQueryHandler : IQueryHandler<GetAreasQuery, IReadOnlyList<AreaDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetAreasQuery> _validator;

    public GetAreasQueryHandler(IApplicationDbContext db, IValidator<GetAreasQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<AreaDto>> HandleAsync(
        GetAreasQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var q = _db.Areas.AsNoTracking();
        if (!query.IncluirInhabilitados)
        {
            q = q.Where(a => a.Habilitado);
        }

        if (query.IdSede.HasValue)
        {
            q = q.Where(a => a.IdSede == query.IdSede.Value);
        }

        var items = await q
            .OrderBy(a => a.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<AreaDto>>();
    }
}
