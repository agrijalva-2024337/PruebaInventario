using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PDFtoImage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Application.Features.Activos;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Features.Asignaciones;

public sealed class AsignacionPdfService : IAsignacionPdfService
{
    private static readonly Color Navy = Color.FromHex("#12344d");
    private static readonly Color Gold = Color.FromHex("#c9a227");
    private static readonly object MembreteLock = new();
    private static string? MembreteCachePath;
    private static byte[]? MembreteCachePng;

    private readonly IApplicationDbContext _db;
    private readonly BrandingOptions _branding;

    static AsignacionPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public AsignacionPdfService(IApplicationDbContext db, IOptions<BrandingOptions> branding)
    {
        _db = db;
        _branding = branding.Value;
    }

    public async Task<AsignacionPdfFileDto> GenerarAsync(
        int idAsignacion,
        string? publicAppUrl,
        CancellationToken cancellationToken = default)
    {
        var asignacion = await _db.Asignaciones
            .AsNoTracking()
            .Include(a => a.TipoAsignacion)
            .Include(a => a.Estado)
            .FirstOrDefaultAsync(a => a.Id == idAsignacion, cancellationToken)
            ?? throw new NotFoundException("Asignacion", idAsignacion);

        asignacion.Activo = await _db.Activos
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(a => a.CategoriaActivo)
            .FirstOrDefaultAsync(a => a.Id == asignacion.IdActivo, cancellationToken);

        asignacion.Responsable = await _db.Responsables
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == asignacion.IdResponsable, cancellationToken);

        asignacion.Ubicacion = await _db.Ubicaciones
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == asignacion.IdUbicacion, cancellationToken);

        var usuarioEntrega = await _db.Usuarios
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == asignacion.IdUsuario, cancellationToken);

        Empresa? empresa = null;
        if (asignacion.Ubicacion is not null)
        {
            var sede = await _db.Sedes.AsNoTracking().IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == asignacion.Ubicacion.IdSede, cancellationToken);
            if (sede is not null)
            {
                empresa = await _db.Empresas.AsNoTracking().IgnoreQueryFilters()
                    .FirstOrDefaultAsync(e => e.Id == sede.IdEmpresa, cancellationToken);
            }
        }

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
        var qrUrl = ActivoQr.Url(publicAppUrl, asignacion.IdActivo);
        var qrPng = ActivoQr.Png(qrUrl);
        var codigoInterno = ActivoDtoFactory.CodigoInterno(asignacion.IdActivo);
        var membrete = LeerMembrete();
        var empresaNombre = string.IsNullOrWhiteSpace(empresa?.Nombre) ? "SLC" : empresa.Nombre;

        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Navy));

                if (membrete is { Length: > 0 })
                {
                    page.Background().Image(membrete).FitArea();
                    page.MarginTop(88);
                    page.MarginBottom(70);
                    page.MarginHorizontal(48);
                }
                else
                {
                    page.MarginTop(24);
                    page.MarginBottom(28);
                    page.MarginHorizontal(40);
                    page.Header().Column(col =>
                    {
                        col.Item().Background(Navy).Padding(12).Row(row =>
                        {
                            row.RelativeItem().Column(brand =>
                            {
                                brand.Item().Text(empresaNombre).FontColor(Colors.White).Bold().FontSize(16);
                                brand.Item().Text("Control de activos · Inventario").FontColor(Colors.White).FontSize(9);
                            });
                            row.ConstantItem(160).AlignRight().Column(meta =>
                            {
                                meta.Item().Text(titulo).FontColor(Gold).FontSize(9).Bold();
                                meta.Item().Text($"Folio #{asignacion.Id}").FontColor(Colors.White).FontSize(9);
                            });
                        });
                        col.Item().Height(4).Background(Gold);
                        if (!string.IsNullOrWhiteSpace(empresa?.Direccion) || !string.IsNullOrWhiteSpace(empresa?.Telefono))
                        {
                            col.Item().PaddingVertical(6).Text(text =>
                            {
                                text.Span(empresa?.Direccion ?? string.Empty).FontSize(8).FontColor(Colors.Grey.Darken2);
                                if (!string.IsNullOrWhiteSpace(empresa?.Telefono))
                                {
                                    text.Span(string.IsNullOrWhiteSpace(empresa.Direccion) ? empresa.Telefono : $"  ·  {empresa.Telefono}")
                                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                                }
                            });
                        }
                    });
                }

                page.Content().PaddingTop(membrete is { Length: > 0 } ? 28 : 12).Column(col =>
                {
                    col.Spacing(4);
                    if (membrete is { Length: > 0 })
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text(titulo).Bold().FontSize(14).FontColor(Navy);
                            row.ConstantItem(90).AlignRight().Text($"Folio #{asignacion.Id}")
                                .FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                    }

                    var tipoEquipo = asignacion.Activo?.CategoriaActivo?.Nombre
                        ?? asignacion.Activo?.Nombre
                        ?? $"#{asignacion.IdActivo}";
                    var motivo = string.IsNullOrWhiteSpace(asignacion.Observaciones)
                        ? null
                        : asignacion.Observaciones.Trim();

                    if (membrete is { Length: > 0 })
                    {
                        col.Item().Height(36);
                    }

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(izq =>
                        {
                            izq.Spacing(3);
                            Dato(izq, "Tipo de equipo", tipoEquipo);
                            Dato(izq, "Nombre", Texto(asignacion.Activo?.Nombre));
                            Dato(izq, "Marca", Texto(asignacion.Activo?.Marca));
                            Dato(izq, "Modelo", Texto(asignacion.Activo?.Modelo));
                        });
                        row.RelativeItem().PaddingLeft(16).Column(der =>
                        {
                            der.Spacing(3);
                            Dato(der, "Serie", Texto(asignacion.Activo?.NumeroSerie));
                            Dato(der, "Ubicación", Texto(asignacion.Ubicacion?.Nombre));
                            Dato(der, "Estado", Texto(asignacion.Estado?.Nombre));
                            Dato(der, "Fecha", asignacion.FechaAsignacion.ToString("yyyy-MM-dd"));
                        });
                        row.ConstantItem(78).Column(qr =>
                        {
                            qr.Item().AlignCenter().Width(72).Image(qrPng).FitArea();
                            qr.Item().AlignCenter().Text(codigoInterno).FontSize(7).FontColor(Colors.Grey.Darken1);
                        });
                    });

                    if (!string.IsNullOrWhiteSpace(asignacion.Activo?.Descripcion))
                    {
                        Dato(col, "Especificaciones", asignacion.Activo.Descripcion.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(motivo))
                    {
                        Dato(col, esBaja ? "Motivo de baja" : "Motivo de entrega", motivo);
                    }

                    if (membrete is { Length: > 0 })
                    {
                        col.Item().Height(330);
                    }

                    col.Item().PaddingTop(membrete is { Length: > 0 } ? 8 : 16)
                        .PaddingLeft(100)
                        .Element(c =>
                            DrawFirmas(c, esBaja, quienEntrega, quienRecibe, asignacion.FirmaEntrega, asignacion.FirmaRecibe));
                });

                if (membrete is { Length: > 0 })
                {
                    page.Foreground().Column(fg =>
                    {
                        fg.Item().Height(464);
                        fg.Item().PaddingLeft(43).PaddingRight(220).Text(quienRecibe).FontSize(10).FontColor(Navy);
                    });
                }

                if (membrete is not { Length: > 0 })
                {
                    page.Footer().Column(col =>
                    {
                        col.Item().Height(3).Background(Gold);
                        col.Item().Background(Navy).Padding(8).AlignCenter().Text(text =>
                        {
                            text.Span($"{empresaNombre} · documento interno de inventario  ·  ")
                                .FontSize(8).FontColor(Colors.White);
                            text.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(8).FontColor(Gold);
                        });
                    });
                }
            });
        }).GeneratePdf();

        return new AsignacionPdfFileDto(pdf, fileName);
    }

    private byte[]? LeerMembrete()
    {
        var path = ResolverRutaMembrete();
        if (path is null)
        {
            return null;
        }

        lock (MembreteLock)
        {
            if (MembreteCachePath == path && MembreteCachePng is { Length: > 0 })
            {
                return MembreteCachePng;
            }

            MembreteCachePath = path;
            MembreteCachePng = CargarMembreteComoPng(path);
            return MembreteCachePng;
        }
    }

    private string? ResolverRutaMembrete()
    {
        var configured = _branding.LetterheadPath;
        var root = string.IsNullOrWhiteSpace(configured)
            ? null
            : Path.GetDirectoryName(configured);

        var candidates = new List<string>();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            candidates.Add(configured);
        }

        if (!string.IsNullOrWhiteSpace(root))
        {
            candidates.Add(Path.Combine(root, "HojaMembrentadaSLC.pdf"));
            candidates.Add(Path.Combine(root, "branding", "hoja-membretada.png"));
            candidates.Add(Path.Combine(root, "branding", "hoja-membretada.jpg"));
        }

        return candidates.FirstOrDefault(File.Exists);
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static byte[] CargarMembreteComoPng(string path)
    {
        if (string.Equals(Path.GetExtension(path), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            var pdfBytes = File.ReadAllBytes(path);
            using var bitmap = Conversion.ToImage(pdfBytes, page: 0, options: new RenderOptions(Dpi: 150));
            using var encoded = bitmap.Encode(SKEncodedImageFormat.Png, 90);
            return encoded.ToArray();
        }

        return File.ReadAllBytes(path);
    }

    private static string Texto(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static void Dato(ColumnDescriptor column, string etiqueta, string valor)
    {
        column.Item().Text(text =>
        {
            text.Span($"{etiqueta}: ").Bold().FontSize(9);
            text.Span(valor).FontSize(9);
        });
    }

    private static void DrawFirmas(
        IContainer container,
        bool esBaja,
        string quienEntrega,
        string quienRecibe,
        byte[]? firmaEntrega,
        byte[]? firmaRecibe)
    {
        container.Row(firmas =>
        {
            firmas.RelativeItem().PaddingRight(18).Column(left =>
            {
                left.Item().Text(esBaja ? "Firma de quien registra" : "Firma de quien entrega").Bold().FontSize(8);
                left.Item().Text(quienEntrega).FontSize(8);
                DrawFirma(left, firmaEntrega);
            });
            firmas.RelativeItem().PaddingLeft(14).Column(right =>
            {
                right.Item().Text(esBaja ? "Firma de quien autoriza" : "Firma de quien recibe").Bold().FontSize(8);
                right.Item().Text(quienRecibe).FontSize(8);
                DrawFirma(right, firmaRecibe);
            });
        });
    }

    private static void DrawFirma(ColumnDescriptor column, byte[]? firma)
    {
        column.Item().Height(42).Element(box =>
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
