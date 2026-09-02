using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Usuarios.Queries;

public sealed record GetUsuariosQuery(bool IncluirInhabilitados = false, int? IdEmpresa = null);

public sealed class GetUsuariosQueryValidator : AbstractValidator<GetUsuariosQuery>
{
    public GetUsuariosQueryValidator()
    {
        RuleFor(x => x.IdEmpresa).OptionalId("id empresa");
    }
}

public sealed class GetUsuariosQueryHandler : IQueryHandler<GetUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetUsuariosQuery> _validator;

    public GetUsuariosQueryHandler(IApplicationDbContext db, IValidator<GetUsuariosQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<IReadOnlyList<UsuarioDto>> HandleAsync(
        GetUsuariosQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var q = _db.Usuarios.AsNoTracking();
        if (!query.IncluirInhabilitados)
        {
            q = q.Where(u => u.Habilitado);
        }

        if (query.IdEmpresa.HasValue)
        {
            q = q.Where(u => u.IdEmpresa == query.IdEmpresa.Value);
        }

        var items = await q
            .OrderBy(u => u.Apellidos)
            .ThenBy(u => u.Nombres)
            .ToListAsync(cancellationToken);

        return items.Adapt<List<UsuarioDto>>();
    }
}
