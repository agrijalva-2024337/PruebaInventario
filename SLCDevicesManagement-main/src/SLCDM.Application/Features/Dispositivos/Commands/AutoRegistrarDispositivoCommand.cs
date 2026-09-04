using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Dispositivos.Commands;

public sealed record AutoRegistrarDispositivoCommand(string NumeroSerie, string InstallKey);

public sealed class AutoRegistrarDispositivoCommandValidator : AbstractValidator<AutoRegistrarDispositivoCommand>
{
    private readonly DeviceTrackingOptions _options;

    public AutoRegistrarDispositivoCommandValidator(IOptions<DeviceTrackingOptions> options)
    {
        _options = options.Value;

        RuleFor(x => x.NumeroSerie).NotEmpty().WithMessage("El numero de serie es obligatorio.");

        RuleFor(x => x.InstallKey)
            .Must(EsInstallKeyValida)
            .WithMessage("Llave de instalacion invalida.");
    }

    private bool EsInstallKeyValida(string installKey)
    {
        if (string.IsNullOrEmpty(installKey) || string.IsNullOrEmpty(_options.InstallKey))
        {
            return false;
        }

        var left = Encoding.UTF8.GetBytes(installKey);
        var right = Encoding.UTF8.GetBytes(_options.InstallKey);
        if (left.Length != right.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(left, right);
    }
}

public sealed class AutoRegistrarDispositivoCommandHandler
    : ICommandHandler<AutoRegistrarDispositivoCommand, DispositivoTokenDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<AutoRegistrarDispositivoCommand> _validator;
    private readonly IDeviceTokenHashService _tokenHash;
    private readonly DeviceTrackingOptions _options;

    public AutoRegistrarDispositivoCommandHandler(
        IApplicationDbContext db,
        IValidator<AutoRegistrarDispositivoCommand> validator,
        IDeviceTokenHashService tokenHash,
        IOptions<DeviceTrackingOptions> options)
    {
        _db = db;
        _validator = validator;
        _tokenHash = tokenHash;
        _options = options.Value;
    }

    public async Task<DispositivoTokenDto> HandleAsync(AutoRegistrarDispositivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        // Sin JWT: esta es la llamada de arranque, antes de que el equipo
        // tenga cualquier credencial. Por eso IgnoreQueryFilters() -- no hay
        // usuario ni empresa detras todavia.
        var activo = await _db.Activos.IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.NumeroSerie == command.NumeroSerie, cancellationToken)
            ?? throw new NotFoundException("Activo con ese numero de serie", command.NumeroSerie);

        var tokenExistente = await _db.DispositivosToken.IgnoreQueryFilters()
            .AnyAsync(d => d.IdActivo == activo.Id && !d.Revocado, cancellationToken);

        if (tokenExistente)
        {
            throw new ConflictException("Este activo ya tiene un token de dispositivo activo.");
        }

        var rawToken = _tokenHash.GenerateRawToken();
        var ahora = DateTime.UtcNow;

        var entity = new DispositivoToken
        {
            IdActivo = activo.Id,
            TokenHash = _tokenHash.Hash(rawToken),
            CreadoEn = ahora,
            ExpiraEn = ahora.AddDays(_options.TokenExpiryDays),
            Revocado = false
        };

        _db.DispositivosToken.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new DispositivoTokenDto(entity.Id, entity.IdActivo, rawToken, entity.CreadoEn, entity.ExpiraEn);
    }
}
