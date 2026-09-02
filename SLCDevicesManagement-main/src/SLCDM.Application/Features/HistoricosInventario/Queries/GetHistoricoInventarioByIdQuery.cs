using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.HistoricosInventario.Queries;

public sealed record GetHistoricoInventarioByIdQuery(int Id);

public sealed class GetHistoricoInventarioByIdQueryValidator : AbstractValidator<GetHistoricoInventarioByIdQuery>
{
    public GetHistoricoInventarioByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id historico inventario");
    }
}

public sealed class GetHistoricoInventarioByIdQueryHandler
    : IQueryHandler<GetHistoricoInventarioByIdQuery, HistoricoInventarioDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetHistoricoInventarioByIdQuery> _validator;

    public GetHistoricoInventarioByIdQueryHandler(
        IApplicationDbContext db,
        IValidator<GetHistoricoInventarioByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<HistoricoInventarioDto> HandleAsync(
        GetHistoricoInventarioByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.HistoricosInventario.AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("HistoricoInventario", query.Id);

        return entity.Adapt<HistoricoInventarioDto>();
    }
}
