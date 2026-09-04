using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.RedesConocidas.Commands;

public sealed record UpdateRedConocidaCommand(int Id, string Bssid, int IdUbicacion);

public sealed class UpdateRedConocidaCommandValidator : AbstractValidator<UpdateRedConocidaCommand>
{
    public UpdateRedConocidaCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id red conocida");

        RuleFor(x => x.Bssid)
            .NotEmpty().WithMessage("El campo bssid es obligatorio.")
            .MaximumLength(17).WithMessage("El campo bssid no debe superar los 17 caracteres.")
            .Matches(@"^[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5}$")
            .WithMessage("El campo bssid debe tener el formato aa:bb:cc:dd:ee:ff.");

        RuleFor(x => x.IdUbicacion)
            .RequiredId("id ubicacion")
            .MustAsync(async (id, ct) => await db.Ubicaciones.AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro una ubicacion con el id informado.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                !await db.RedesConocidas.AnyAsync(
                    r => r.Id != cmd.Id && r.Bssid == cmd.Bssid.Trim().ToLowerInvariant(),
                    ct))
            .WithMessage("Ya existe una red conocida con ese BSSID.");
    }
}

public sealed class UpdateRedConocidaCommandHandler : ICommandHandler<UpdateRedConocidaCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateRedConocidaCommand> _validator;

    public UpdateRedConocidaCommandHandler(IApplicationDbContext db, IValidator<UpdateRedConocidaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateRedConocidaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.RedesConocidas.FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("RedConocida", command.Id);

        entity.Bssid = command.Bssid.Trim().ToLowerInvariant();
        entity.IdUbicacion = command.IdUbicacion;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
