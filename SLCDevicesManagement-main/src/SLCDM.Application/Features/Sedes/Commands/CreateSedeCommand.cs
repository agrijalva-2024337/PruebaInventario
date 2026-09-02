using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Sedes.Commands;

public sealed record CreateSedeCommand(
    int IdEmpresa,
    int IdPais,
    string Nombre,
    string? Direccion,
    string? Ciudad);

public sealed class CreateSedeCommandValidator : AbstractValidator<CreateSedeCommand>
{
    public CreateSedeCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdEmpresa)
            .RequiredId("id empresa")
            .MustAsync(async (id, ct) => await db.Empresas.AnyAsync(e => e.Id == id, ct))
            .WithMessage("El campo id empresa no corresponde a un registro existente.");

        RuleFor(x => x.IdPais)
            .RequiredId("id pais")
            .MustAsync(async (id, ct) => await db.Paises.AnyAsync(p => p.Id == id, ct))
            .WithMessage("El campo id pais no corresponde a un registro existente.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El campo nombre no debe superar los 100 caracteres.");

        RuleFor(x => x.Direccion)
            .MaximumLength(100).WithMessage("El campo direccion no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Direccion));

        RuleFor(x => x.Ciudad)
            .MaximumLength(100).WithMessage("El campo ciudad no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Ciudad));
    }
}

public sealed class CreateSedeCommandHandler : ICommandHandler<CreateSedeCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateSedeCommand> _validator;

    public CreateSedeCommandHandler(IApplicationDbContext db, IValidator<CreateSedeCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateSedeCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Sede>();
        entity.Habilitado = true;

        _db.Sedes.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
