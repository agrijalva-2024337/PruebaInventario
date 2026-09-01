using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Ubicaciones.Commands;

public sealed record CreateUbicacionCommand(
    int IdSede,
    string Nombre,
    string? Descripcion,
    decimal Latitud,
    decimal Longitud);

public sealed class CreateUbicacionCommandValidator : AbstractValidator<CreateUbicacionCommand>
{
    public CreateUbicacionCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdSede)
            .RequiredId("id sede")
            .MustAsync(async (id, ct) => await db.Sedes.AnyAsync(s => s.Id == id, ct))
            .WithMessage("No se encontro una sede con el id informado.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El campo nombre no debe superar los 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(200).WithMessage("El campo descripcion no debe superar los 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

        RuleFor(x => x.Latitud)
            .InclusiveBetween(-90, 90).WithMessage("El campo latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Longitud)
            .InclusiveBetween(-180, 180).WithMessage("El campo longitud debe estar entre -180 y 180.");
    }
}

public sealed class CreateUbicacionCommandHandler : ICommandHandler<CreateUbicacionCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateUbicacionCommand> _validator;

    public CreateUbicacionCommandHandler(IApplicationDbContext db, IValidator<CreateUbicacionCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateUbicacionCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Ubicacion>();
        entity.Habilitado = true;

        _db.Ubicaciones.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
