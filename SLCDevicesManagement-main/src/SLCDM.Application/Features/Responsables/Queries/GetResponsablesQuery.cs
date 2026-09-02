using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Responsables.Queries;

public sealed record GetResponsablesQuery(bool IncluirInhabilitados = false, int? IdArea = null);

public sealed class GetResponsablesQueryValidator : AbstractValidator<GetResponsablesQuery>
{
    public GetResponsablesQueryValidator()
    {
        RuleFor(x => x.IdArea).OptionalId("id area");
    }
}

public sealed class GetResponsablesQueryHandler : IQueryHandler<GetResponsablesQuery, IReadOnlyList<ResponsableDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetResponsablesQuery> _validator;

    public GetResponsablesQueryHandler(IApplicationDbContext db, IValidator<GetResponsablesQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<ResponsableDto>> HandleAsync(
        GetResponsablesQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var q = _db.Responsables.AsNoTracking();
        if (!query.IncluirInhabilitados)
        {
            q = q.Where(r => r.Habilitado);
        }

        if (query.IdArea.HasValue)
        {
            q = q.Where(r => r.IdArea == query.IdArea.Value);
        }

        var items = await q
            .OrderBy(r => r.NombreCompleto)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<ResponsableDto>>();
    }
}
