namespace SLCDM.Application.Features.Activos;

public sealed record ActivoDto(
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
