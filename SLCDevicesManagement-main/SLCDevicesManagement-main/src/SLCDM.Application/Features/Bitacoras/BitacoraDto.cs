using SLCDM.Domain.Enums;

namespace SLCDM.Application.Features.Bitacoras;

public sealed record BitacoraDto(
    int Id,
    int IdUsuario,
    DateTime FechaHora,
    TipoOperacionBitacora TipoOperacion,
    string EntidadAfectada,
    string? Descripcion,
    string? InformacionAnterior,
    string? InformacionNueva);
