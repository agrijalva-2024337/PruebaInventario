using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.Activos.Commands;

public sealed record UpdateActivoCommand(
    int Id,
    int IdCategoriaActivo,
    int IdProveedor,
    int IdUbicacion,
    string Nombre,
    string? Descripcion,
    string? Marca,
    string? Modelo,
    string? NumeroSerie,
    DateTime FechaCompra,
    decimal CostoAdquisicion,
    string? Moneda,
    string? NumeroFactura,
    DateTime FechaVencimientoGarantia,
    string? Observaciones);

public sealed class UpdateActivoCommandValidator : AbstractValidator<UpdateActivoCommand>
{
    public UpdateActivoCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id activo");

        RuleFor(x => x.IdCategoriaActivo)
            .RequiredId("id categoria activo")
            .MustAsync(async (id, ct) => await db.CategoriasActivo.AnyAsync(c => c.Id == id, ct))
            .WithMessage("No se encontro una categoria de activo con el id informado.");

        RuleFor(x => x.IdProveedor)
            .RequiredId("id proveedor")
            .MustAsync(async (id, ct) => await db.Proveedores.AnyAsync(p => p.Id == id, ct))
            .WithMessage("No se encontro un proveedor con el id informado.");

        RuleFor(x => x.IdUbicacion)
            .RequiredId("id ubicacion")
            .MustAsync(async (id, ct) => await db.Ubicaciones.AnyAsync(u => u.Id == id, ct))
            .WithMessage("No se encontro una ubicacion con el id informado.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El campo nombre no debe superar los 150 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("El campo descripcion no debe superar los 300 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

        RuleFor(x => x.Marca)
            .MaximumLength(100).WithMessage("El campo marca no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Marca));

        RuleFor(x => x.Modelo)
            .MaximumLength(100).WithMessage("El campo modelo no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Modelo));

        RuleFor(x => x.NumeroSerie)
            .MaximumLength(100).WithMessage("El campo numero serie no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroSerie));

        RuleFor(x => x.CostoAdquisicion)
            .GreaterThanOrEqualTo(0).WithMessage("El campo costo adquisicion debe ser mayor o igual a 0.");

        RuleFor(x => x.Moneda)
            .MaximumLength(10).WithMessage("El campo moneda no debe superar los 10 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Moneda));

        RuleFor(x => x.NumeroFactura)
            .MaximumLength(50).WithMessage("El campo numero factura no debe superar los 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroFactura));

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("El campo observaciones no debe superar los 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));
    }
}

public sealed class UpdateActivoCommandHandler : ICommandHandler<UpdateActivoCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateActivoCommand> _validator;

    public UpdateActivoCommandHandler(IApplicationDbContext db, IValidator<UpdateActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateActivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.Activos.FirstOrDefaultAsync(a => a.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("Activo", command.Id);

        command.Adapt(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
