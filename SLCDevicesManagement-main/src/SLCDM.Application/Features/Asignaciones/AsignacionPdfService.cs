using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Options;
using SLCDM.Domain.Enums;

namespace SLCDM.Application.Features.Asignaciones;

public sealed class AsignacionPdfService : IAsignacionPdfService
{
    private readonly IApplicationDbContext _db;
    private readonly BrandingOptions _branding;

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

        string? areaNombre = null;
        if (asignacion.Responsable is not null)
        {
            var area = await _db.Areas.AsNoTracking().IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == asignacion.Responsable.IdArea, cancellationToken);
            areaNombre = area?.Nombre;
        }

        var tipo = asignacion.TipoAsignacion?.Nombre ?? "Movimiento";
        var esBaja = TipoAsignacionNombres.EsNombre(tipo, TipoAsignacionNombres.Baja);
        var quienEntrega = usuarioEntrega is null
            ? $"Usuario #{asignacion.IdUsuario}"
            : $"{usuarioEntrega.Nombres} {usuarioEntrega.Apellidos}".Trim();
        var quienRecibe = asignacion.Responsable?.NombreCompleto ?? $"Responsable #{asignacion.IdResponsable}";
        var fileName = esBaja
            ? $"acta-baja-{asignacion.Id}.pdf"
            : $"acta-asignacion-{asignacion.Id}.pdf";

        var plantillaPath = ResolverPlantilla();
        if (plantillaPath is null)
        {
            throw new InvalidOperationException(
                "No está la plantilla Word del acta (wwwroot/templates/ActaAsignacion.docx).");
        }

        var datos = new ActaWordDatos
        {
            Para = quienRecibe,
            Departamento = string.IsNullOrWhiteSpace(areaNombre) ? "—" : areaNombre,
            Fecha = asignacion.FechaAsignacion.ToString("dd/MM/yyyy"),
            TipoEquipo = Texto(asignacion.Activo?.CategoriaActivo?.Nombre ?? asignacion.Activo?.Nombre),
            Marca = Texto(asignacion.Activo?.Marca),
            Modelo = Texto(asignacion.Activo?.Modelo),
            Serie = Texto(asignacion.Activo?.NumeroSerie),
            Especificaciones = Texto(asignacion.Activo?.Descripcion),
            Perifericos = Texto(asignacion.Activo?.PerifericosAdicionales),
            Estado = Texto(asignacion.Estado?.Nombre),
            Motivo = string.IsNullOrWhiteSpace(asignacion.Observaciones)
                ? "—"
                : asignacion.Observaciones.Trim(),
            NombreResponsable = quienRecibe,
            Dpi = asignacion.Responsable?.Dpi ?? string.Empty,
            NombreEntrega = quienEntrega,
            CargoEntrega = usuarioEntrega is null ? "—" : CargoRol(usuarioEntrega.Rol),
            NombreRecibe = quienRecibe,
            CargoRecibe = string.IsNullOrWhiteSpace(asignacion.Responsable?.Cargo)
                ? "Quien recibe"
                : asignacion.Responsable!.Cargo.Trim(),
            FirmaEntrega = asignacion.FirmaEntrega,
            FirmaRecibe = asignacion.FirmaRecibe
        };

        var docx = ActaWordDocumento.Rellenar(await File.ReadAllBytesAsync(plantillaPath, cancellationToken), datos);
        var pdf = await Task.Run(() => DocxPdfConverter.Convertir(docx), cancellationToken);
        return new AsignacionPdfFileDto(pdf, fileName);
    }

    private string? ResolverPlantilla()
    {
        var names = new[]
        {
            _branding.ActaTemplatePath,
            Path.Combine(AppContext.BaseDirectory, "wwwroot", "templates", "ActaAsignacion.docx"),
            Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "ActaAsignacion.docx"),
        };

        return names.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p));
    }

    private static string Texto(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string CargoRol(RolUsuario rol) => rol switch
    {
        RolUsuario.AdministradorGeneral => "Administrador general",
        RolUsuario.AdministradorEmpresa => "Administrador de empresa",
        RolUsuario.OperadorInventario => "Operador de inventario",
        _ => "Consulta"
    };
}
