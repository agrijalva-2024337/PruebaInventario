namespace SLCDM.Application.Features.ProductosCompra;

public sealed record ProductoCompraDto(
    int Id,
    bool Habilitado,
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
