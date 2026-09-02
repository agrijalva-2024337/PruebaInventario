using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.TiposAsignacion.Commands;

public sealed record UpdateTipoAsignacionCommand(
    int Id,
    string Nombre,
    string? Descripcion);

public sealed class UpdateTipoAsignacionCommandValidator : AbstractValidator<UpdateTipoAsignacionCommand>
{
    public UpdateTipoAsignacionCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id tipo asignacion");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(50).WithMessage("El campo nombre no debe superar los 50 caracteres.")
            .MustAsync(async (cmd, nombre, ct) =>
                !await db.TiposAsignacion.AnyAsync(t => t.Nombre == nombre && t.Id != cmd.Id, ct))
            .WithMessage("Ya existe un tipo de asignacion con el mismo nombre.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(150).WithMessage("El campo descripcion no debe superar los 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class UpdateTipoAsignacionCommandHandler : ICommandHandler<UpdateTipoAsignacionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateTipoAsignacionCommand> _validator;

    public UpdateTipoAsignacionCommandHandler(IApplicationDbContext db, IValidator<UpdateTipoAsignacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateTipoAsignacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.TiposAsignacion.FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("TipoAsignacion", command.Id);

        command.Adapt(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
