using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Paises.Commands;

public sealed record UpdatePaisCommand(
    int Id,
    string Nombre,
    string CodigoIso2,
    string CodigoIso3,
    string? CodigoTelefonico);

public sealed class UpdatePaisCommandValidator : AbstractValidator<UpdatePaisCommand>
{
    public UpdatePaisCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id pais");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El campo nombre no debe superar los 100 caracteres.");

        RuleFor(x => x.CodigoIso2)
            .NotEmpty().WithMessage("El campo codigo iso2 es obligatorio.")
            .Length(2).WithMessage("El campo codigo iso2 debe tener 2 caracteres.")
            .MustAsync(async (cmd, iso2, ct) =>
                !await db.Paises.AnyAsync(p => p.CodigoIso2 == iso2.ToUpper() && p.Id != cmd.Id, ct))
            .WithMessage("Ya existe un pais con el mismo codigo iso2.");

        RuleFor(x => x.CodigoIso3)
            .NotEmpty().WithMessage("El campo codigo iso3 es obligatorio.")
            .Length(3).WithMessage("El campo codigo iso3 debe tener 3 caracteres.")
            .MustAsync(async (cmd, iso3, ct) =>
                !await db.Paises.AnyAsync(p => p.CodigoIso3 == iso3.ToUpper() && p.Id != cmd.Id, ct))
            .WithMessage("Ya existe un pais con el mismo codigo iso3.");

        RuleFor(x => x.CodigoTelefonico)
            .MaximumLength(5).WithMessage("El campo codigo telefonico no debe superar los 5 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoTelefonico));
    }
}

public sealed class UpdatePaisCommandHandler : ICommandHandler<UpdatePaisCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdatePaisCommand> _validator;

    public UpdatePaisCommandHandler(IApplicationDbContext db, IValidator<UpdatePaisCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdatePaisCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Paises.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Pais", command.Id);

        command.Adapt(entity);
        entity.CodigoIso2 = command.CodigoIso2.ToUpperInvariant();
        entity.CodigoIso3 = command.CodigoIso3.ToUpperInvariant();

        await _db.SaveChangesAsync(cancellationToken);
    }
}
