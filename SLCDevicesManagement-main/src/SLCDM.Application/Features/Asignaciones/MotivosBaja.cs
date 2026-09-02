using SLCDM.Application.Common.Exceptions;

namespace SLCDM.Application.Features.Asignaciones;

/// <summary>
/// Motivos de baja del requerimiento. No hay tabla en el ERD:
/// se guardan en <c>asignacion.observaciones</c>.
/// </summary>
public static class MotivosBaja
{
    public static readonly string[] Canonicos =
    [
        "Venta",
        "Desecho",
        "Donacion",
        "Perdida",
        "Robo",
        "Dano irreparable",
        "Otro"
    ];

    public static bool EsCanonico(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return false;
        }

        var normalizado = TipoAsignacionNombres.Normalizar(motivo);
        return Canonicos.Any(c =>
            TipoAsignacionNombres.Normalizar(c).Equals(normalizado, StringComparison.OrdinalIgnoreCase));
    }

    public static void AsegurarCanonico(string? motivo)
    {
        if (!EsCanonico(motivo))
        {
            throw new ConflictException(
                "El motivo de baja debe ser: Venta, Desecho, Donacion, Perdida, Robo, Dano irreparable u Otro.");
        }
    }
}
