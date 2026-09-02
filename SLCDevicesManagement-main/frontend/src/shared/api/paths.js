/**
 * Rutas REST alineadas a [Route("api/[controller]")] de SLCDM.Api.
 */
export const apiPaths = {
  health: '/api/health',
  auth: {
    login: '/api/auth/login',
    profile: '/api/auth/profile',
  },
  paises: '/api/paises',
  empresas: '/api/empresas',
  sedes: '/api/sedes',
  areas: '/api/areas',
  usuarios: '/api/usuarios',
  responsables: '/api/responsables',
  bitacoras: '/api/bitacoras',
  estados: '/api/estados',
  tiposAsignacion: '/api/tiposAsignacion',
  categoriasActivo: '/api/categoriasActivo',
  proveedores: '/api/proveedores',
  ubicaciones: '/api/ubicaciones',
  activos: '/api/activos',
  consultaActivos: '/api/consulta/activos',
  asignaciones: '/api/asignaciones',
  historicosInventario: '/api/historicosInventario',
  detallesActivo: '/api/detallesActivos',
  historialActivos: '/api/historialActivos',
  reportes: {
    inventarioGeneral: '/api/reportes/inventario-general',
    activos: '/api/reportes/activos',
    activosPorSede: '/api/reportes/activos-por-sede',
    activosPorCategoria: '/api/reportes/activos-por-categoria',
    activosPorResponsable: '/api/reportes/activos-por-responsable',
    garantiasPorVencer: '/api/reportes/garantias-por-vencer',
    activosPorUbicacion: '/api/reportes/activos-por-ubicacion',
    diferenciasInventario: '/api/reportes/diferencias-inventario',
  },
};
