using SLCDM.Application.Features.Reportes;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Activos;

internal sealed record ActivoContexto(
    int? IdEmpresa,
    string? NombreEmpresa,
    int? IdSede,
    string? NombreSede,
    string? NombreUbicacion,
    string? NombreArea,
    int? IdResponsable,
    string? NombreResponsable);

internal static class ActivoDtoFactory
{
    public static string CodigoInterno(int idActivo) => $"A-{idActivo:D5}";

    public static ActivoDto Create(
        Activo item,
        string? estadoOperativo,
        ActivoContexto? contexto = null)
    {
        var estado = string.IsNullOrWhiteSpace(estadoOperativo)
            ? ActivoEstadoOperativo.Disponible
            : estadoOperativo;

        return new ActivoDto(
            item.Id,
            item.IdCategoriaActivo,
            item.IdProveedor,
            item.IdUbicacion,
            item.Nombre,
            item.Descripcion,
            item.Marca,
            item.Modelo,
            item.NumeroSerie,
            item.FechaCompra,
            item.CostoAdquisicion,
            item.Moneda,
            item.NumeroFactura,
            item.FechaVencimientoGarantia,
            item.Observaciones,
            item.PerifericosAdicionales,
            estado,
            ActivoEstadoOperativo.NombreVisible(estado),
            CodigoInterno(item.Id),
            contexto?.IdEmpresa,
            contexto?.NombreEmpresa,
            contexto?.IdSede,
            contexto?.NombreSede,
            contexto?.NombreUbicacion,
            contexto?.NombreArea,
            contexto?.IdResponsable,
            contexto?.NombreResponsable);
    }

    public static ActivoDto CreateFromRow(ActivoInventarioRow row) =>
        Create(
            row.Activo,
            row.EstadoOperativo,
            new ActivoContexto(
                row.IdEmpresa,
                row.NombreEmpresa,
                row.IdSede,
                row.NombreSede,
                row.NombreUbicacion,
                null,
                row.IdResponsable,
                null));
}
