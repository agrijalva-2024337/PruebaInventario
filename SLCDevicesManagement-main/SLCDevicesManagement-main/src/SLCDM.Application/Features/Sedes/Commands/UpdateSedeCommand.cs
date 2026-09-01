using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Sedes.Commands;

public sealed record UpdateSedeCommand(
    int Id,
    int IdEmpresa,
    int IdPais,
    string Nombre,
    string? Direccion,
    string? Ciudad,
    bool Habilitado);

public sealed class UpdateSedeCommandValidator : AbstractValidator<UpdateSedeCommand>
{
    public UpdateSedeCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id sede");

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

public sealed class UpdateSedeCommandHandler : ICommandHandler<UpdateSedeCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateSedeCommand> _validator;

    public UpdateSedeCommandHandler(IApplicationDbContext db, IValidator<UpdateSedeCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateSedeCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Sedes.FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Sede", command.Id);

        command.Adapt(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
