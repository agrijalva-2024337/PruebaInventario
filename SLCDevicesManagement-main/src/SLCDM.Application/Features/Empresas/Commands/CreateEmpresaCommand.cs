using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Empresas.Commands;

public sealed record CreateEmpresaCommand(
    string Nombre,
    string NitCodigo,
    string? Direccion,
    string? Telefono);

public sealed class CreateEmpresaCommandValidator : AbstractValidator<CreateEmpresaCommand>
{
    public CreateEmpresaCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El campo nombre no debe superar los 150 caracteres.");

        RuleFor(x => x.NitCodigo)
            .NotEmpty().WithMessage("El campo nit codigo es obligatorio.")
            .MaximumLength(50).WithMessage("El campo nit codigo no debe superar los 50 caracteres.")
            .MustAsync(async (nit, ct) =>
                !await db.Empresas.AnyAsync(e => e.NitCodigo == nit, ct))
            .WithMessage("Ya existe una empresa con el mismo nit codigo.");

        RuleFor(x => x.Direccion)
            .MaximumLength(150).WithMessage("El campo direccion no debe superar los 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Direccion));

        RuleFor(x => x.Telefono)
            .MaximumLength(30).WithMessage("El campo telefono no debe superar los 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

public sealed class CreateEmpresaCommandHandler : ICommandHandler<CreateEmpresaCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateEmpresaCommand> _validator;

    public CreateEmpresaCommandHandler(IApplicationDbContext db, IValidator<CreateEmpresaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateEmpresaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Empresa>();
        entity.Habilitado = true;

        _db.Empresas.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
