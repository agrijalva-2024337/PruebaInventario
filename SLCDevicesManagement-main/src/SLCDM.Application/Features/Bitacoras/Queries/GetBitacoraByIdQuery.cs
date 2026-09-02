using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Bitacoras.Queries;

public sealed record GetBitacoraByIdQuery(int Id);

public sealed class GetBitacoraByIdQueryValidator : AbstractValidator<GetBitacoraByIdQuery>
{
    public GetBitacoraByIdQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id bitacora");
    }
}

public sealed class GetBitacoraByIdQueryHandler : IQueryHandler<GetBitacoraByIdQuery, BitacoraDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetBitacoraByIdQuery> _validator;

    public GetBitacoraByIdQueryHandler(IApplicationDbContext db, IValidator<GetBitacoraByIdQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<BitacoraDto> HandleAsync(GetBitacoraByIdQuery query, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var entity = await _db.Bitacoras.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Bitacora", query.Id);

        var usuario = await _db.Usuarios.AsNoTracking()
            .Where(u => u.Id == entity.IdUsuario)
            .Select(u => (u.Nombres + " " + u.Apellidos).Trim())
            .FirstOrDefaultAsync(cancellationToken);

        return new BitacoraDto(
            entity.Id,
            entity.IdUsuario,
            entity.FechaHora,
            entity.TipoOperacion,
            entity.EntidadAfectada,
            entity.Descripcion,
            entity.InformacionAnterior,
            entity.InformacionNueva,
            usuario);
    }
}
