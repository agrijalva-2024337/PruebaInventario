using Microsoft.EntityFrameworkCore;
using SLCDM.Domain.Entities;

namespace SLCDM.Application.Common.Interfaces;

/// <summary>
/// Abstraccion del DbContext que usan los casos de uso de Application
/// (IQueryHandler / ICommandHandler). Application depende de esta interfaz,
/// nunca de EF Core directamente — la implementacion real (ApplicationDbContext)
/// vive en Persistence.
///
/// OJO Gerardo: este archivo lo vamos a tocar los dos (BE-04 / BE-05). Yo
/// agrego los DbSet de mi grupo (Pais incluido, porque Sede depende de el y
/// nadie mas lo tenia asignado en Persistence todavia); vos agregas los
/// tuyos (Categoria_Activo, Proveedor, Ubicacion, Activo, Asignacion,
/// Historico_Inventario, Detalle_Activo, Historial_Activo) en un PR aparte.
/// Si da conflicto al mergear, es solo porque los dos agregamos lineas en el
/// mismo bloque — se resuelve dejando las dos, no se descarta ninguna.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Pais> Paises { get; }
    DbSet<Empresa> Empresas { get; }
    DbSet<Sede> Sedes { get; }
    DbSet<Area> Areas { get; }
    DbSet<Usuario> Usuarios { get; }
    DbSet<Responsable> Responsables { get; }
    DbSet<Bitacora> Bitacoras { get; }
    DbSet<Estado> Estados { get; }
    DbSet<TipoAsignacion> TiposAsignacion { get; }
    DbSet<CategoriaActivo> CategoriasActivo { get; }
    DbSet<Proveedor> Proveedores { get; }
    DbSet<Ubicacion> Ubicaciones { get; }
    DbSet<Activo> Activos { get; }
    DbSet<Asignacion> Asignaciones { get; }
    DbSet<HistoricoInventario> HistoricosInventario { get; }
    DbSet<DetalleActivo> DetallesActivos { get; }
    DbSet<HistorialActivo> HistorialActivos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
