using Microsoft.EntityFrameworkCore;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Entities;

namespace SLCDM.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUser;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : this(options, new DesignTimeCurrentUserService())
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// Expuesto para que EF reevalue el filtro en cada query (no capturar el valor al compilar el modelo).
    /// </summary>
    public bool IgnoreEmpresaFilter => _currentUser.IsAdministradorGeneral;

    public int TenantEmpresaId => _currentUser.EmpresaId ?? -1;

    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Sede> Sedes => Set<Sede>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Responsable> Responsables => Set<Responsable>();
    public DbSet<Bitacora> Bitacoras => Set<Bitacora>();
    public DbSet<Estado> Estados => Set<Estado>();
    public DbSet<TipoAsignacion> TiposAsignacion => Set<TipoAsignacion>();
    public DbSet<CategoriaActivo> CategoriasActivo => Set<CategoriaActivo>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<Activo> Activos => Set<Activo>();
    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();
    public DbSet<HistoricoInventario> HistoricosInventario => Set<HistoricoInventario>();
    public DbSet<DetalleActivo> DetallesActivos => Set<DetalleActivo>();
    public DbSet<HistorialActivo> HistorialActivos => Set<HistorialActivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplyEmpresaQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Multiempresa: el Administrador general ve todo; el resto solo datos de su id_empresa.
    /// Pais, Estado, TipoAsignacion y CategoriaActivo son catalogos globales (sin filtro).
    /// </summary>
    private void ApplyEmpresaQueryFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>().HasQueryFilter(e =>
            IgnoreEmpresaFilter || e.Id == TenantEmpresaId);

        modelBuilder.Entity<Sede>().HasQueryFilter(s =>
            IgnoreEmpresaFilter || s.IdEmpresa == TenantEmpresaId);

        modelBuilder.Entity<Usuario>().HasQueryFilter(u =>
            IgnoreEmpresaFilter || u.IdEmpresa == TenantEmpresaId);

        modelBuilder.Entity<Proveedor>().HasQueryFilter(p =>
            IgnoreEmpresaFilter || p.IdEmpresa == TenantEmpresaId);

        modelBuilder.Entity<Area>().HasQueryFilter(a =>
            IgnoreEmpresaFilter || Sedes.Any(s => s.Id == a.IdSede));

        modelBuilder.Entity<Ubicacion>().HasQueryFilter(u =>
            IgnoreEmpresaFilter || Sedes.Any(s => s.Id == u.IdSede));

        modelBuilder.Entity<Responsable>().HasQueryFilter(r =>
            IgnoreEmpresaFilter || Areas.Any(a => a.Id == r.IdArea));

        modelBuilder.Entity<HistoricoInventario>().HasQueryFilter(h =>
            IgnoreEmpresaFilter || Sedes.Any(s => s.Id == h.IdSede));

        modelBuilder.Entity<Activo>().HasQueryFilter(a =>
            IgnoreEmpresaFilter || Proveedores.Any(p => p.Id == a.IdProveedor));

        modelBuilder.Entity<Asignacion>().HasQueryFilter(a =>
            IgnoreEmpresaFilter || Activos.Any(x => x.Id == a.IdActivo));

        modelBuilder.Entity<DetalleActivo>().HasQueryFilter(d =>
            IgnoreEmpresaFilter || Activos.Any(a => a.Id == d.IdActivo));

        modelBuilder.Entity<Bitacora>().HasQueryFilter(b =>
            IgnoreEmpresaFilter || Usuarios.Any(u => u.Id == b.IdUsuario));

        modelBuilder.Entity<HistorialActivo>().HasQueryFilter(h =>
            IgnoreEmpresaFilter
            || (h.IdAsignacion.HasValue && Asignaciones.Any(a => a.Id == h.IdAsignacion))
            || (h.IdDetalleActivo.HasValue && DetallesActivos.Any(d => d.Id == h.IdDetalleActivo)));
    }
}
