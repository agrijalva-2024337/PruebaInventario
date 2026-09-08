using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;

namespace SLCDM.Application.Features.ProductosCompra.Commands;

public sealed record UpdateProductoCompraCommand(
    int Id,
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
    DateTime FechaVencimientoGarantia,
    bool Habilitado);

public sealed class UpdateProductoCompraCommandValidator : AbstractValidator<UpdateProductoCompraCommand>
{
    public UpdateProductoCompraCommandValidator(IApplicationDbContext db)
    {
        RuleFor(x => x.Id).RequiredId("id producto compra");

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

public sealed class UpdateProductoCompraCommandHandler : ICommandHandler<UpdateProductoCompraCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<UpdateProductoCompraCommand> _validator;

    public UpdateProductoCompraCommandHandler(IApplicationDbContext db, IValidator<UpdateProductoCompraCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(UpdateProductoCompraCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = await _db.ProductosCompra.FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException("ProductoCompra", command.Id);

        command.Adapt(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
