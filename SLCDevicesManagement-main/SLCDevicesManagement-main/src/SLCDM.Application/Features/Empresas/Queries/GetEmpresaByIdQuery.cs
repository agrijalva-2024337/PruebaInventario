using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Empresas.Queries;

public sealed record GetEmpresaByIdQuery(int Id);

public sealed class GetEmpresaByIdQueryValidator : AbstractValidator<GetEmpresaByIdQuery>
{
    public GetEmpresaByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id empresa");
    }
}

public sealed class GetEmpresaByIdQueryHandler : IQueryHandler<GetEmpresaByIdQuery, EmpresaDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetEmpresaByIdQuery> _validator;

    public GetEmpresaByIdQueryHandler(IApplicationDbContext db, IValidator<GetEmpresaByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<EmpresaDto> HandleAsync(GetEmpresaByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Empresas.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Empresa", query.Id);

        return entity.Adapt<EmpresaDto>();
    }
}
