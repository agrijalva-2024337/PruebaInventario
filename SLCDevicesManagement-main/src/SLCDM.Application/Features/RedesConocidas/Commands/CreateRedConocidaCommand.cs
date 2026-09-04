using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.RedesConocidas.Commands;

public sealed record CreateRedConocidaCommand(string Bssid, int IdUbicacion);

public sealed class CreateRedConocidaCommandValidator : AbstractValidator<CreateRedConocidaCommand>
{
    public CreateRedConocidaCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Bssid)
            .NotEmpty().WithMessage("El campo bssid es obligatorio.")
            .MaximumLength(17).WithMessage("El campo bssid no debe superar los 17 caracteres.")
            .Matches(@"^[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5}$")
            .WithMessage("El campo bssid debe tener el formato aa:bb:cc:dd:ee:ff.");

        RuleFor(x => x.IdUbicacion)
            .RequiredId("id ubicacion")
            .MustAsync(async (id, ct) => await db.Ubicaciones.AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro una ubicacion con el id informado.");

        RuleFor(x => x.Bssid)
            .MustAsync(async (bssid, ct) =>
                !await db.RedesConocidas.AnyAsync(r => r.Bssid == bssid.Trim().ToLowerInvariant(), ct))
            .WithMessage("Ya existe una red conocida con ese BSSID.");
    }
}

public sealed class CreateRedConocidaCommandHandler : ICommandHandler<CreateRedConocidaCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateRedConocidaCommand> _validator;

    public CreateRedConocidaCommandHandler(IApplicationDbContext db, IValidator<CreateRedConocidaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateRedConocidaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<RedConocida>();
        entity.Bssid = command.Bssid.Trim().ToLowerInvariant();

        _db.RedesConocidas.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
