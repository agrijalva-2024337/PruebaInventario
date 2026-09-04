using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;

namespace SLCDM.Application.Features.Asignaciones;

public sealed class AsignacionCorreoService : IAsignacionCorreoService
{
    private readonly IApplicationDbContext _db;
    private readonly IAsignacionPdfService _pdf;
    private readonly IEmailSender _email;
    private readonly SmtpOptions _smtp;
    private readonly ILogger<AsignacionCorreoService> _logger;

    public AsignacionCorreoService(
        IApplicationDbContext db,
        IAsignacionPdfService pdf,
        IEmailSender email,
        IOptions<SmtpOptions> smtp,
        ILogger<AsignacionCorreoService> logger)
    {
        _db = db;
        _pdf = pdf;
        _email = email;
        _smtp = smtp.Value;
        _logger = logger;
    }

    public async Task NotificarResponsableAsync(int idAsignacion, CancellationToken cancellationToken = default)
    {
        try
        {
            var asignacion = await _db.Asignaciones
                .AsNoTracking()
                .Include(a => a.TipoAsignacion)
                .FirstOrDefaultAsync(a => a.Id == idAsignacion, cancellationToken);

            if (asignacion is null)
            {
                _logger.LogWarning("No se envio el acta {Id}: asignacion no encontrada.", idAsignacion);
                return;
            }

            var responsable = await _db.Responsables
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == asignacion.IdResponsable, cancellationToken);

            if (responsable is null)
            {
                _logger.LogWarning("No se envio el acta {Id}: sin responsable.", idAsignacion);
                return;
            }

            var to = responsable.Correo?.Trim();
            if (string.IsNullOrWhiteSpace(to))
            {
                _logger.LogWarning(
                    "No se envio el acta {Id}: el responsable {Nombre} no tiene correo.",
                    idAsignacion,
                    responsable.NombreCompleto);
                return;
            }

            var esBaja = TipoAsignacionNombres.EsNombre(
                asignacion.TipoAsignacion?.Nombre,
                TipoAsignacionNombres.Baja);
            var pdf = await _pdf.GenerarAsync(idAsignacion, _smtp.PublicAppUrl, cancellationToken);
            var activo = await _db.Activos.AsNoTracking().IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == asignacion.IdActivo, cancellationToken);
            var activoNombre = activo?.Nombre ?? $"#{asignacion.IdActivo}";
            var asunto = esBaja
                ? $"Baja de activo {activoNombre}"
                : $"Asignación de activo {activoNombre}";
            var cuerpo = esBaja
                ? $"<p>Hola {responsable.NombreCompleto},</p><p>Se registró la <strong>baja</strong> del activo <strong>{activoNombre}</strong>. Adjunto encontrará el acta en PDF.</p>"
                : $"<p>Hola {responsable.NombreCompleto},</p><p>Se le asignó el activo <strong>{activoNombre}</strong>. Adjunto encontrará el acta de entrega en PDF.</p>";

            await _email.SendAsync(
                to,
                asunto,
                cuerpo,
                [new EmailAttachment(pdf.FileName, "application/pdf", pdf.Content)],
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo enviar el PDF de la asignacion {Id} al responsable.", idAsignacion);
        }
    }
}
