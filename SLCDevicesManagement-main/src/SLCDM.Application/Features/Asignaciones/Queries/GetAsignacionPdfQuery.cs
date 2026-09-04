using FluentValidation;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Application.Features.Activos;

namespace SLCDM.Application.Features.Asignaciones.Queries;

public sealed record GetAsignacionPdfQuery(int Id, string? PublicAppUrl = null);

public sealed class GetAsignacionPdfQueryValidator : AbstractValidator<GetAsignacionPdfQuery>
{
    public GetAsignacionPdfQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id asignacion");
    }
}

public sealed class GetAsignacionPdfQueryHandler : IQueryHandler<GetAsignacionPdfQuery, AsignacionPdfFileDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetAsignacionPdfQuery> _validator;

    static GetAsignacionPdfQueryHandler()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public GetAsignacionPdfQueryHandler(IApplicationDbContext db, IValidator<GetAsignacionPdfQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<AsignacionPdfFileDto> HandleAsync(
        GetAsignacionPdfQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var asignacion = await _db.Asignaciones
            .AsNoTracking()
            .Include(a => a.Activo)
            .Include(a => a.Responsable)
            .Include(a => a.Ubicacion)
            .Include(a => a.TipoAsignacion)
            .Include(a => a.Estado)
            .FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Asignacion", query.Id);

        var usuarioEntrega = await _db.Usuarios
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == asignacion.IdUsuario, cancellationToken);

        var tipo = asignacion.TipoAsignacion?.Nombre ?? "Movimiento";
        var esBaja = TipoAsignacionNombres.EsNombre(tipo, TipoAsignacionNombres.Baja);
        var titulo = esBaja ? "Acta de baja de activo" : "Acta de asignación de activo";
        var quienEntrega = usuarioEntrega is null
            ? $"Usuario #{asignacion.IdUsuario}"
            : $"{usuarioEntrega.Nombres} {usuarioEntrega.Apellidos}".Trim();
        var quienRecibe = asignacion.Responsable?.NombreCompleto ?? $"Responsable #{asignacion.IdResponsable}";
        var fileName = esBaja
            ? $"acta-baja-{asignacion.Id}.pdf"
            : $"acta-asignacion-{asignacion.Id}.pdf";
        var qrUrl = ActivoQr.Url(query.PublicAppUrl, asignacion.IdActivo);
        var qrPng = ActivoQr.Png(qrUrl);
        var codigoInterno = ActivoDtoFactory.CodigoInterno(asignacion.IdActivo);

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.Letter);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(header =>
                {
                    header.Item().Text("DERCAS · SLCDevicesManagement").FontSize(9).FontColor(Colors.Grey.Darken1);
                    header.Item().Text(titulo).Bold().FontSize(18);
                    header.Item().Text($"Folio interno #{asignacion.Id}").FontSize(10).FontColor(Colors.Grey.Darken2);
                });

                page.Content().PaddingTop(16).Column(col =>
                {
                    col.Spacing(8);
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(datos =>
                        {
                            datos.Spacing(4);
                            datos.Item().Text("Datos del activo").Bold();
                            datos.Item().Text($"Código: {codigoInterno}");
                            datos.Item().Text($"Nombre: {asignacion.Activo?.Nombre ?? $"#{asignacion.IdActivo}"}");
                            datos.Item().Text($"Serie: {asignacion.Activo?.NumeroSerie ?? "—"}");
                            datos.Item().Text($"Ubicación: {asignacion.Ubicacion?.Nombre ?? "—"}");
                            datos.Item().Text($"Estado: {asignacion.Estado?.Nombre ?? "—"}");
                            datos.Item().Text($"Tipo: {tipo}");
                            datos.Item().Text($"Fecha: {asignacion.FechaAsignacion:yyyy-MM-dd}");
                        });
                        row.ConstantItem(120).Column(qr =>
                        {
                            qr.Item().AlignCenter().Width(110).Image(qrPng).FitArea();
                            qr.Item().AlignCenter().Text("QR del activo").FontSize(8).FontColor(Colors.Grey.Darken2);
                            qr.Item().AlignCenter().Text(codigoInterno).FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(asignacion.Observaciones))
                    {
                        col.Item().PaddingTop(8).Text(esBaja ? "Motivo de baja" : "Observaciones").Bold();
                        col.Item().Text(asignacion.Observaciones);
                    }

                    col.Item().PaddingTop(24).Row(row =>
                    {
                        row.RelativeItem().PaddingRight(12).Column(left =>
                        {
                            left.Item().Text(esBaja ? "Firma de quien registra" : "Firma de quien entrega").Bold();
                            left.Item().Text(quienEntrega).FontSize(10);
                            DrawFirma(left, asignacion.FirmaEntrega);
                        });
                        row.RelativeItem().PaddingLeft(12).Column(right =>
                        {
                            right.Item().Text(esBaja ? "Firma de quien autoriza" : "Firma de quien recibe").Bold();
                            right.Item().Text(quienRecibe).FontSize(10);
                            DrawFirma(right, asignacion.FirmaRecibe);
                        });
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Documento generado el ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    text.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        }).GeneratePdf();

        if (asignacion.DocumentoPdfGenerardoEn is null)
        {
            var tracked = await _db.Asignaciones.FirstAsync(a => a.Id == asignacion.Id, cancellationToken);
            tracked.DocumentoPdfUrl ??= AsignacionDocumento.PdfRelativeUrl(asignacion.Id);
            tracked.DocumentoPdfGenerardoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new AsignacionPdfFileDto(pdf, fileName);
    }

    private static void DrawFirma(ColumnDescriptor column, byte[]? firma)
    {
        column.Item().PaddingTop(8).Border(1).BorderColor(Colors.Grey.Lighten1).Height(90).Padding(6).Element(box =>
        {
            if (AsignacionDocumento.TieneTinta(firma))
            {
                box.Image(firma!).FitArea();
            }
            else
            {
                box.AlignMiddle().AlignCenter().Text("Sin firma").FontColor(Colors.Grey.Medium).Italic();
            }
        });
    }
}
