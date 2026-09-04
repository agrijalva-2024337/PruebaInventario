using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Dispositivos.Commands;

public sealed record RegistrarDispositivoCommand(int IdActivo);

public sealed class RegistrarDispositivoCommandValidator : AbstractValidator<RegistrarDispositivoCommand>
{
    public RegistrarDispositivoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdActivo)
            .RequiredId("id activo")
            .MustAsync(async (id, ct) => await db.Activos.AnyAsync(a => a.Id == id, ct))
            .WithMessage("No se encontro un activo con el id informado.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) =>
                !await db.DispositivosToken.AnyAsync(d => d.IdActivo == cmd.IdActivo && !d.Revocado, ct))
            .WithMessage("El activo ya tiene un token de dispositivo activo. Revoquelo antes de generar uno nuevo.");
    }
}

public sealed class RegistrarDispositivoCommandHandler : ICommandHandler<RegistrarDispositivoCommand, DispositivoTokenDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<RegistrarDispositivoCommand> _validator;
    private readonly IDeviceTokenHashService _tokenHash;
    private readonly DeviceTrackingOptions _options;

    public RegistrarDispositivoCommandHandler(
        IApplicationDbContext db,
        IValidator<RegistrarDispositivoCommand> validator,
        IDeviceTokenHashService tokenHash,
        IOptions<DeviceTrackingOptions> options)
    {
        _db = db;
        _validator = validator;
        _tokenHash = tokenHash;
        _options = options.Value;
    }

    public async Task<DispositivoTokenDto> HandleAsync(RegistrarDispositivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var rawToken = _tokenHash.GenerateRawToken();
        var ahora = DateTime.UtcNow;

        var entity = new DispositivoToken
        {
            IdActivo = command.IdActivo,
            TokenHash = _tokenHash.Hash(rawToken),
            CreadoEn = ahora,
            ExpiraEn = ahora.AddDays(_options.TokenExpiryDays),
            Revocado = false
        };

        _db.DispositivosToken.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // El token crudo solo existe aqui: no se guarda en ningun lado. Se
        // muestra una sola vez para que el administrador lo copie.
        return new DispositivoTokenDto(entity.Id, entity.IdActivo, rawToken, entity.CreadoEn, entity.ExpiraEn);
    }
}
