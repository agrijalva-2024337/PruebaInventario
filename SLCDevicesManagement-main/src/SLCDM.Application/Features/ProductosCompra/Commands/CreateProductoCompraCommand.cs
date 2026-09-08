using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.ProductosCompra.Commands;

public sealed record CreateProductoCompraCommand(
    int IdProveedor,
    int? IdCategoriaActivo,
    string Nombre,
    string? Marca,
    string? Modelo,
    string? Descripcion,
    decimal CostoUnitario,
    string? Moneda,
    string? NumeroFactura,
    DateTime FechaCompra,
    DateTime FechaVencimientoGarantia);

public sealed class CreateProductoCompraCommandValidator : AbstractValidator<CreateProductoCompraCommand>
{
    public CreateProductoCompraCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.IdProveedor)
            .RequiredId("id proveedor")
            .MustAsync(async (id, ct) => await db.Proveedores.AnyAsync(p => p.Id == id, ct))
            .WithMessage("No se encontro un proveedor con el id informado.");

        RuleFor(x => x.IdCategoriaActivo)
            .MustAsync(async (id, ct) => !id.HasValue || await db.CategoriasActivo.AnyAsync(c => c.Id == id.Value, ct))
            .WithMessage("No se encontro una categoria de activo con el id informado.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El campo nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El campo nombre no debe superar los 150 caracteres.");

        RuleFor(x => x.Marca)
            .MaximumLength(100).WithMessage("El campo marca no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Marca));

        RuleFor(x => x.Modelo)
            .MaximumLength(100).WithMessage("El campo modelo no debe superar los 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Modelo));

        RuleFor(x => x.Descripcion)
            .MaximumLength(500).WithMessage("El campo descripcion no debe superar los 500 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

        RuleFor(x => x.CostoUnitario)
            .GreaterThanOrEqualTo(0).WithMessage("El campo costo unitario debe ser mayor o igual a 0.");

        RuleFor(x => x.Moneda)
            .MaximumLength(10).WithMessage("El campo moneda no debe superar los 10 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Moneda));

        RuleFor(x => x.NumeroFactura)
            .MaximumLength(50).WithMessage("El campo numero factura no debe superar los 50 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroFactura));
    }
}

public sealed class CreateProductoCompraCommandHandler : ICommandHandler<CreateProductoCompraCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateProductoCompraCommand> _validator;

    public CreateProductoCompraCommandHandler(IApplicationDbContext db, IValidator<CreateProductoCompraCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateProductoCompraCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<ProductoCompra>();
        entity.Habilitado = true;
        if (entity.FechaCompra == default)
        {
            entity.FechaCompra = DateTime.UtcNow.Date;
        }

        if (entity.FechaVencimientoGarantia == default)
        {
            entity.FechaVencimientoGarantia = entity.FechaCompra.AddYears(1);
        }

        _db.ProductosCompra.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
