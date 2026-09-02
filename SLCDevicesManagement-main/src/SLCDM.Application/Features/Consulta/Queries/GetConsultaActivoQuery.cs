using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Exceptions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Common.Validation;
using SLCDM.Application.Features.Activos;
using SLCDM.Application.Features.Asignaciones;
using SLCDM.Application.Features.Reportes;

namespace SLCDM.Application.Features.Consulta.Queries;

public sealed record GetConsultaActivoQuery(int Id);

public sealed class GetConsultaActivoQueryValidator : AbstractValidator<GetConsultaActivoQuery>
{
    public GetConsultaActivoQueryValidator()
    {
        RuleFor(x => x.Id).RequiredId("id activo");
    }
}

public sealed class GetConsultaActivoQueryHandler : IQueryHandler<GetConsultaActivoQuery, ConsultaActivoDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<GetConsultaActivoQuery> _validator;

    public GetConsultaActivoQueryHandler(IApplicationDbContext db, IValidator<GetConsultaActivoQuery> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<ConsultaActivoDto> HandleAsync(
        GetConsultaActivoQuery query,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(query, cancellationToken);

        var activo = await _db.Activos
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken)
            ?? throw new NotFoundException("Activo", query.Id);

        var contexto = await (
            from a in _db.Activos.IgnoreQueryFilters().AsNoTracking()
            join u in _db.Ubicaciones.IgnoreQueryFilters().AsNoTracking() on a.IdUbicacion equals u.Id
            join s in _db.Sedes.IgnoreQueryFilters().AsNoTracking() on u.IdSede equals s.Id
            join e in _db.Empresas.IgnoreQueryFilters().AsNoTracking() on s.IdEmpresa equals e.Id
            join c in _db.CategoriasActivo.AsNoTracking() on a.IdCategoriaActivo equals c.Id
            where a.Id == query.Id
            select new
            {
                NombreUbicacion = u.Nombre,
                NombreSede = s.Nombre,
                NombreEmpresa = e.Nombre,
                NombreCategoria = c.Nombre
            }
        ).FirstOrDefaultAsync(cancellationToken);

        var (estado, idResponsable) = await ResolverEstadoAsync(query.Id, cancellationToken);
        string? nombreResponsable = null;
        string? nombreArea = null;

        if (idResponsable.HasValue)
        {
            var responsable = await (
                from r in _db.Responsables.IgnoreQueryFilters().AsNoTracking()
                join ar in _db.Areas.IgnoreQueryFilters().AsNoTracking() on r.IdArea equals ar.Id
                where r.Id == idResponsable.Value
                select new { r.NombreCompleto, Area = ar.Nombre }
            ).FirstOrDefaultAsync(cancellationToken);

            nombreResponsable = responsable?.NombreCompleto;
            nombreArea = responsable?.Area;
        }

        return new ConsultaActivoDto(
            activo.Id,
            ActivoDtoFactory.CodigoInterno(activo.Id),
            activo.Nombre,
            activo.Descripcion,
            activo.Marca,
            activo.Modelo,
            activo.NumeroSerie,
            contexto?.NombreCategoria,
            contexto?.NombreEmpresa,
            contexto?.NombreSede,
            contexto?.NombreUbicacion,
            nombreArea,
            nombreResponsable,
            estado,
            ActivoEstadoOperativo.NombreVisible(estado),
            activo.FechaVencimientoGarantia);
    }

    private async Task<(string Estado, int? IdResponsable)> ResolverEstadoAsync(
        int idActivo,
        CancellationToken cancellationToken)
    {
        var tipos = await _db.TiposAsignacion.AsNoTracking().ToListAsync(cancellationToken);
        var idsBaja = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Baja))
            .Select(t => t.Id)
            .ToHashSet();
        var idsMantenimiento = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Mantenimiento))
            .Select(t => t.Id)
            .ToHashSet();
        var idsAsignacion = tipos
            .Where(t => TipoAsignacionNombres.EsNombre(t.Nombre, TipoAsignacionNombres.Asignacion))
            .Select(t => t.Id)
            .ToHashSet();

        var activas = await _db.Asignaciones
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(a => a.IdActivo == idActivo && a.Activa)
            .Select(a => new { a.IdTipoAsignacion, a.IdResponsable, a.FechaAsignacion })
            .ToListAsync(cancellationToken);

        if (activas.Any(a => idsBaja.Contains(a.IdTipoAsignacion)))
        {
            return (ActivoEstadoOperativo.Baja, null);
        }

        if (activas.Any(a => idsMantenimiento.Contains(a.IdTipoAsignacion)))
        {
            return (ActivoEstadoOperativo.Mantenimiento, null);
        }

        var asignacion = activas
            .Where(a => idsAsignacion.Contains(a.IdTipoAsignacion))
            .OrderByDescending(a => a.FechaAsignacion)
            .FirstOrDefault();

        if (asignacion is not null)
        {
            return (ActivoEstadoOperativo.Asignado, asignacion.IdResponsable);
        }

        return (ActivoEstadoOperativo.Disponible, null);
    }
}
