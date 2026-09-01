using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones;

/// <summary>
/// Nombres canonicos de <c>tipo_asignacion</c>. El API no los siembra:
/// hay que cargarlos (idempotente: <c>Scripts/SeedCatalogos.sql</c>) o
/// crearlos como AdministradorGeneral. Si falta el registro, BE-16/17/18
/// (<see cref="ObtenerRequeridoAsync"/>) responden 409.
/// Solo Asignacion y Mantenimiento ocupan el activo (un proceso a la vez).
/// Traslado no ocupa; Baja bloquea entrega/traslado/mantenimiento.
/// </summary>
public static class TipoAsignacionNombres
{
    /// <summary>Entrega a responsable. Ocupa el activo. BE-14/15.</summary>
    public const string Asignacion = "Asignacion";

    /// <summary>Cambio de ubicacion. No ocupa. BE-16.</summary>
    public const string Traslado = "Traslado";

    /// <summary>Envio a mantenimiento. Ocupa el activo. BE-17.</summary>
    public const string Mantenimiento = "Mantenimiento";

    /// <summary>Baja operativa (el activo no se borra). BE-18.</summary>
    public const string Baja = "Baja";

    public static string Normalizar(string? nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return string.Empty;
        }

        return nombre.Trim()
            .Replace("ó", "o", StringComparison.OrdinalIgnoreCase)
            .Replace("Ó", "o", StringComparison.OrdinalIgnoreCase);
    }

    public static bool EsNombre(string? actual, string esperado) =>
        Normalizar(actual).Equals(Normalizar(esperado), StringComparison.OrdinalIgnoreCase);

    public static bool EsTipoQueOcupaActivo(string? nombre) =>
        EsNombre(nombre, Asignacion) || EsNombre(nombre, Mantenimiento);

    public static async Task<TipoAsignacion> ObtenerRequeridoAsync(
        IApplicationDbContext db,
        string nombre,
        CancellationToken cancellationToken)
    {
        var tipos = await db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var tipo = tipos.FirstOrDefault(t => EsNombre(t.Nombre, nombre));
        if (tipo is null)
        {
            throw new ConflictException(
                $"No existe el tipo de asignacion '{nombre}' en el catalogo. Cree el registro antes de continuar.");
        }

        return tipo;
    }
}
