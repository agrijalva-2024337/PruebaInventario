using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones;

/// <summary>
/// Nombres canonicos de <c>estado</c> ligados a cada tipo de movimiento.
/// El API los resuelve solo; el cliente no elige el estado.
/// Catalogo: <c>Scripts/SeedCatalogos.sql</c>.
/// </summary>
public static class EstadoNombres
{
    public const string Disponible = "Disponible";
    public const string Asignado = "Asignado";
    public const string Traslado = "Traslado";
    public const string Mantenimiento = "Mantenimiento";
    public const string Baja = "Baja";

    public static string ParaTipo(string? tipoAsignacion)
    {
        if (TipoAsignacionNombres.EsNombre(tipoAsignacion, TipoAsignacionNombres.Asignacion))
        {
            return Asignado;
        }

        if (TipoAsignacionNombres.EsNombre(tipoAsignacion, TipoAsignacionNombres.Traslado))
        {
            return Traslado;
        }

        if (TipoAsignacionNombres.EsNombre(tipoAsignacion, TipoAsignacionNombres.Mantenimiento))
        {
            return Mantenimiento;
        }

        if (TipoAsignacionNombres.EsNombre(tipoAsignacion, TipoAsignacionNombres.Baja))
        {
            return Baja;
        }

        throw new ConflictException(
            $"No hay un estado automatico para el tipo de movimiento '{tipoAsignacion}'.");
    }

    public static async Task<Estado> ObtenerRequeridoAsync(
        IApplicationDbContext db,
        string nombre,
        CancellationToken cancellationToken)
    {
        var estados = await db.Estados.AsNoTracking().ToListAsync(cancellationToken);
        var estado = estados.FirstOrDefault(e => TipoAsignacionNombres.EsNombre(e.Nombre, nombre));
        if (estado is null)
        {
            throw new ConflictException(
                $"No existe el estado '{nombre}' en el catalogo. Ejecute Scripts/SeedCatalogos.sql o creelo en Estados.");
        }

        return estado;
    }

    public static Task<Estado> ObtenerParaTipoAsync(
        IApplicationDbContext db,
        string? tipoAsignacion,
        CancellationToken cancellationToken) =>
        ObtenerRequeridoAsync(db, ParaTipo(tipoAsignacion), cancellationToken);
}
