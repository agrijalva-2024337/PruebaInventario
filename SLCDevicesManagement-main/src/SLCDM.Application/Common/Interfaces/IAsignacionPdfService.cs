using SLCDM.Application.Features.Asignaciones;

namespace SLCDM.Application.Common.Interfaces;

public interface IAsignacionPdfService
{
    Task<AsignacionPdfFileDto> GenerarAsync(int idAsignacion, string? publicAppUrl, CancellationToken cancellationToken = default);
}

public interface IAsignacionCorreoService
{
    Task NotificarResponsableAsync(int idAsignacion, CancellationToken cancellationToken = default);
}
