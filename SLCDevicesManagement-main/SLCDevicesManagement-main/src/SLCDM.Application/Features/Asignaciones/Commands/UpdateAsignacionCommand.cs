using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Asignaciones.Commands;

public sealed record UpdateAsignacionCommand(
    int Id,
    int IdEstado,
    int IdTipoAsignacion,
    string? Observaciones,
    string? DocumentoPdfUrl,
    DateTime? DocumentoPdfGenerardoEn);

public sealed class UpdateAsignacionCommandValidator : AbstractValidator<UpdateAsignacionCommand>
{
    public UpdateAsignacionCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id asignacion");

        RuleFor(x => x.IdEstado)
            .RequiredId("id estado")
            .MustAsync(async (id, ct) => await db.Estados.AnyAsync(e => e.Id == id, ct))
            .WithMessage("No se encontro un estado con el id informado.");

        RuleFor(x => x.IdTipoAsignacion)
            .RequiredId("id tipo asignacion")
            .MustAsync(async (id, ct) => await db.TiposAsignacion.AnyAsync(t => t.Id == id, ct))
            .WithMessage("No se encontro un tipo de asignacion con el id informado.");

        RuleFor(x => x.Observaciones)
            .MaximumLength(300).WithMessage("El campo observaciones no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        RuleFor(x => x.DocumentoPdfUrl)
            .MaximumLength(300).WithMessage("El campo documento pdf url no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.DocumentoPdfUrl));
    }
}

public sealed class UpdateAsignacionCommandHandler : ICommandHandler<UpdateAsignacionCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateAsignacionCommand> _validator;

    public UpdateAsignacionCommandHandler(IApplicationDbContext db, IValidator<UpdateAsignacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateAsignacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Asignaciones.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Asignacion", command.Id);

        command.Adapt(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
