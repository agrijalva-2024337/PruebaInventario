using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.CategoriasActivo.Commands;

public sealed record UpdateCategoriaActivoCommand(
    int Id,
    string Nombre,
    string? Descripcion,
    bool Habilitado);

public sealed class UpdateCategoriaActivoCommandValidator : AbstractValidator<UpdateCategoriaActivoCommand>
{
    public UpdateCategoriaActivoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id categoria activo");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El campo nombre no debe superar los 100 caracteres.")
            .MustAsync(async (cmd, nombre, ct) =>
                !await db.CategoriasActivo.AnyAsync(c => c.Nombre == nombre && c.Id != cmd.Id, ct))
            .WithMessage("Ya existe una categoria de activo con el mismo nombre.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("El campo descripcion no debe superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class UpdateCategoriaActivoCommandHandler : ICommandHandler<UpdateCategoriaActivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateCategoriaActivoCommand> _validator;

    public UpdateCategoriaActivoCommandHandler(
        IApplicationDbContext db,
        IValidator<UpdateCategoriaActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(
        UpdateCategoriaActivoCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.CategoriasActivo
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("CategoriaActivo", command.Id);

        command.Adapt(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
