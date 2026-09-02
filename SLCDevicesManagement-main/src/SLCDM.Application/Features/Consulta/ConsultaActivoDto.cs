namespace SLCDM.Application.Features.Consulta;

/// <summary>
/// Ficha pública al escanear un QR. Sin costos, facturas ni historial.
/// </summary>
public sealed record ConsultaActivoDto(
    int Id,
    string CodigoInterno,
    string Nombre,
    string? Descripcion,
    string? Marca,
    string? Modelo,
    string? NumeroSerie,
    string? NombreCategoria,
    string? NombreEmpresa,
    string? NombreSede,
    string? NombreUbicacion,
    string? NombreArea,
    string? NombreResponsable,
    string EstadoOperativo,
    string EstadoNombre,
    DateTime? FechaVencimientoGarantia);
