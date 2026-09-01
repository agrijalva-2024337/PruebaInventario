using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Empresas.Commands;

public sealed record UpdateEmpresaCommand(
    int Id,
    string Nombre,
    string NitCodigo,
    string? Direccion,
    string? Telefono,
    bool Habilitado);

public sealed class UpdateEmpresaCommandValidator : AbstractValidator<UpdateEmpresaCommand>
{
    public UpdateEmpresaCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id empresa");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El campo nombre no debe superar los 150 caracteres.");

        RuleFor(x => x.NitCodigo)
            .NotEmpty().WithMessage("El campo nit codigo es obligatorio.")
            .MaximumLength(50).WithMessage("El campo nit codigo no debe superar los 50 caracteres.")
            .MustAsync(async (cmd, nit, ct) =>
                !await db.Empresas.AnyAsync(e => e.NitCodigo == nit && e.Id != cmd.Id, ct))
            .WithMessage("Ya existe una empresa con el mismo nit codigo.");

        RuleFor(x => x.Direccion)
            .MaximumLength(150).WithMessage("El campo direccion no debe superar los 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Direccion));

        RuleFor(x => x.Telefono)
            .MaximumLength(30).WithMessage("El campo telefono no debe superar los 30 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

public sealed class UpdateEmpresaCommandHandler : ICommandHandler<UpdateEmpresaCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateEmpresaCommand> _validator;

    public UpdateEmpresaCommandHandler(IApplicationDbContext db, IValidator<UpdateEmpresaCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateEmpresaCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Empresas.FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Empresa", command.Id);

        command.Adapt(entity);

        await _db.SaveChangesAsync(cancellationToken);
    }
}
