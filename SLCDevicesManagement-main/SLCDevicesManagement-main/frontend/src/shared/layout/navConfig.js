export const NAV_SECTIONS = [
  {
    id: 'inicio',
    label: 'Inicio',
    items: [{ to: '/', label: 'Dashboard', end: true }],
  },
  {
    id: 'catalogos',
    label: 'Catálogos',
    items: [
      { to: '/catalogos/empresas', label: 'Empresas' },
      { to: '/catalogos/sedes', label: 'Sedes' },
      { to: '/catalogos/areas', label: 'Áreas' },
      { to: '/catalogos/responsables', label: 'Responsables' },
      { to: '/catalogos/categorias', label: 'Categorías' },
      { to: '/catalogos/proveedores', label: 'Proveedores' },
      { to: '/catalogos/ubicaciones', label: 'Ubicaciones' },
      { to: '/catalogos/usuarios', label: 'Usuarios', requiresEscrituraEmpresa: true },
      { to: '/catalogos/estados', label: 'Estados' },
      { to: '/catalogos/tipos-asignacion', label: 'Tipos de asignación' },
    ],
  },
  {
    id: 'activos',
    label: 'Activos',
    items: [{ to: '/activos', label: 'Activos' }],
  },
  {
    id: 'operaciones',
    label: 'Operaciones',
    items: [
      { to: '/operaciones/asignaciones', label: 'Asignaciones' },
      { to: '/operaciones/traslados', label: 'Traslados' },
      { to: '/operaciones/mantenimientos', label: 'Mantenimientos' },
      { to: '/operaciones/bajas', label: 'Bajas' },
    ],
  },
  {
    id: 'inventario',
    label: 'Inventario',
    items: [{ to: '/inventario', label: 'Jornadas' }],
  },
  {
    id: 'reportes',
    label: 'Reportes',
    items: [{ to: '/reportes', label: 'Reportes' }],
  },
];
