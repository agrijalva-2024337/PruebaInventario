using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Sedes.Queries;

public sealed record GetSedesQuery(bool IncluirInhabilitados = false, int? IdEmpresa = null);

public sealed class GetSedesQueryValidator : AbstractValidator<GetSedesQuery>
{
    public GetSedesQueryValidator()
    {
        RuleFor(x => x.IdEmpresa).OptionalId("id empresa");
    }
}

public sealed class GetSedesQueryHandler : IQueryHandler<GetSedesQuery, IReadOnlyList<SedeDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetSedesQuery> _validator;

    public GetSedesQueryHandler(IApplicationDbContext db, IValidator<GetSedesQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<SedeDto>> HandleAsync(
        GetSedesQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var q = _db.Sedes.AsNoTracking();
        if (!query.IncluirInhabilitados)
        {
            q = q.Where(s => s.Habilitado);
        }

        if (query.IdEmpresa.HasValue)
        {
            q = q.Where(s => s.IdEmpresa == query.IdEmpresa.Value);
        }

        var items = await q
            .OrderBy(s => s.Nombre)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<SedeDto>>();
    }
}
