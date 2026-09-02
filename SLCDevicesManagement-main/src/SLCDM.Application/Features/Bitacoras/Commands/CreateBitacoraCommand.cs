using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;
using SLCDM.Domain.Enums;

namespace SLCDM.Application.Features.Bitacoras.Commands;

public sealed record CreateBitacoraCommand(
    int IdUsuario,
    TipoOperacionBitacora TipoOperacion,
    string EntidadAfectada,
    string? Descripcion,
    string? InformacionAnterior,
    string? InformacionNueva);

public sealed class CreateBitacoraCommandValidator : AbstractValidator<CreateBitacoraCommand>
{
    public CreateBitacoraCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdUsuario)
            .RequiredId("id usuario")
            .MustAsync(async (id, ct) => await db.Usuarios.AnyAsync(u => u.Id == id, ct))
            .WithMessage("El campo id usuario no corresponde a un registro existente.");

        RuleFor(x => x.TipoOperacion)
            .IsInEnum().WithMessage("El campo tipo operacion no es un valor valido.");

        RuleFor(x => x.EntidadAfectada)
            .NotEmpty().WithMessage("El campo entidad afectada es obligatorio.")
            .MaximumLength(100).WithMessage("El campo entidad afectada no debe superar los 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("El campo descripcion no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class CreateBitacoraCommandHandler : ICommandHandler<CreateBitacoraCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateBitacoraCommand> _validator;

    public CreateBitacoraCommandHandler(IApplicationDbContext db, IValidator<CreateBitacoraCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateBitacoraCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Bitacora>();
        entity.FechaHora = DateTime.UtcNow;

        _db.Bitacoras.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
