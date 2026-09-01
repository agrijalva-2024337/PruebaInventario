using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Bitacoras.Queries;

public sealed record GetBitacorasQuery(int? IdUsuario = null, string? EntidadAfectada = null);

public sealed class GetBitacorasQueryValidator : AbstractValidator<GetBitacorasQuery>
{
    public GetBitacorasQueryValidator()
    {
        RuleFor(x => x.IdUsuario).OptionalId("id usuario");

        RuleFor(x => x.EntidadAfectada)
            .MaximumLength(100).WithMessage("El campo entidad afectada no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.EntidadAfectada));
    }
}

public sealed class GetBitacorasQueryHandler : IQueryHandler<GetBitacorasQuery, IReadOnlyList<BitacoraDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetBitacorasQuery> _validator;

    public GetBitacorasQueryHandler(IApplicationDbContext db, IValidator<GetBitacorasQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<BitacoraDto>> HandleAsync(
        GetBitacorasQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var q = _db.Bitacoras.AsNoTracking();

        if (query.IdUsuario.HasValue)
        {
            q = q.Where(b => b.IdUsuario == query.IdUsuario.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.EntidadAfectada))
        {
            q = q.Where(b => b.EntidadAfectada == query.EntidadAfectada);
        }

        var items = await q
            .OrderByDescending(b => b.FechaHora)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<BitacoraDto>>();
    }
}
