using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.CategoriasActivo.Commands;

public sealed record CreateCategoriaActivoCommand(
    string Nombre,
    string? Descripcion);

public sealed class CreateCategoriaActivoCommandValidator : AbstractValidator<CreateCategoriaActivoCommand>
{
    public CreateCategoriaActivoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El campo nombre no debe superar los 100 caracteres.")
            .MustAsync(async (nombre, ct) =>
                !await db.CategoriasActivo.AnyAsync(c => c.Nombre == nombre, ct))
            .WithMessage("Ya existe una categoria de activo con el mismo nombre.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("El campo descripcion no debe superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

public sealed class CreateCategoriaActivoCommandHandler : ICommandHandler<CreateCategoriaActivoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateCategoriaActivoCommand> _validator;

    public CreateCategoriaActivoCommandHandler(
        IApplicationDbContext db,
        IValidator<CreateCategoriaActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(
        CreateCategoriaActivoCommand command,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<CategoriaActivo>();
        entity.Habilitado = true;

        _db.CategoriasActivo.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
