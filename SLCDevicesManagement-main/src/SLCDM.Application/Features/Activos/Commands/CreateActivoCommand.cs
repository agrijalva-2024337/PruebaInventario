using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Activos.Commands;

public sealed record CreateActivoCommand(
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

public sealed class CreateActivoCommandValidator : AbstractValidator<CreateActivoCommand>
{
    public CreateActivoCommandValidator(IApplicationDbContext db)
    {
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

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await SerieUnicaEnEmpresaAsync(db, cmd, ct))
            .WithMessage("Ya existe un activo con ese numero de serie en la misma empresa.")
            .When(x => !string.IsNullOrWhiteSpace(x.NumeroSerie));

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await ProveedorYUbicacionMismaEmpresaAsync(db, cmd, ct))
            .WithMessage("El proveedor y la ubicacion deben pertenecer a la misma empresa.");

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

    private static async Task<bool> SerieUnicaEnEmpresaAsync(
        IApplicationDbContext db,
        CreateActivoCommand command,
        CancellationToken cancellationToken)
    {
        var idEmpresa = await db.Proveedores
            .AsNoTracking()
            .Where(p => p.Id == command.IdProveedor)
            .Select(p => (int?)p.IdEmpresa)
            .FirstOrDefaultAsync(cancellationToken);

        if (!idEmpresa.HasValue)
        {
            return true;
        }

        var serie = command.NumeroSerie!.Trim();
        return !await (
            from a in db.Activos.AsNoTracking()
            join p in db.Proveedores.AsNoTracking() on a.IdProveedor equals p.Id
            where p.IdEmpresa == idEmpresa.Value
                  && a.NumeroSerie != null
                  && a.NumeroSerie == serie
            select a.Id
        ).AnyAsync(cancellationToken);
    }

    private static async Task<bool> ProveedorYUbicacionMismaEmpresaAsync(
        IApplicationDbContext db,
        CreateActivoCommand command,
        CancellationToken cancellationToken)
    {
        var idEmpresaProveedor = await db.Proveedores
            .AsNoTracking()
            .Where(p => p.Id == command.IdProveedor)
            .Select(p => (int?)p.IdEmpresa)
            .FirstOrDefaultAsync(cancellationToken);

        var idEmpresaUbicacion = await (
            from u in db.Ubicaciones.AsNoTracking()
            join s in db.Sedes.AsNoTracking() on u.IdSede equals s.Id
            where u.Id == command.IdUbicacion
            select (int?)s.IdEmpresa
        ).FirstOrDefaultAsync(cancellationToken);

        return idEmpresaProveedor.HasValue
            && idEmpresaUbicacion.HasValue
            && idEmpresaProveedor.Value == idEmpresaUbicacion.Value;
    }
}

public sealed class CreateActivoCommandHandler : ICommandHandler<CreateActivoCommand, int>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateActivoCommand> _validator;

    public CreateActivoCommandHandler(IApplicationDbContext db, IValidator<CreateActivoCommand> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<int> HandleAsync(CreateActivoCommand command, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(command, cancellationToken);

        var entity = command.Adapt<Activo>();
        _db.Activos.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
