using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Usuarios.Queries;

public sealed record GetUsuarioByIdQuery(int Id);

public sealed class GetUsuarioByIdQueryValidator : AbstractValidator<GetUsuarioByIdQuery>
{
    public GetUsuarioByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id usuario");
    }
}

public sealed class GetUsuarioByIdQueryHandler : IQueryHandler<GetUsuarioByIdQuery, UsuarioDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetUsuarioByIdQuery> _validator;

    public GetUsuarioByIdQueryHandler(IApplicationDbContext db, IValidator<GetUsuarioByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<UsuarioDto> HandleAsync(GetUsuarioByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Usuario", query.Id);

        return entity.Adapt<UsuarioDto>();
    }
}
