export const NAV_SECTIONS = [
  {
    id: 'inventario',
    label: 'Inventario',
    defaultOpen: true,
    items: [
      { to: '/activos', label: 'Activos' },
      { to: '/operaciones/asignaciones', label: 'Asignaciones' },
      { to: '/operaciones/traslados', label: 'Traslados' },
      { to: '/operaciones/mantenimientos', label: 'Mantenimientos' },
      { to: '/operaciones/bajas', label: 'Bajas' },
      { to: '/inventario', label: 'Inventario físico' },
    ],
  },
  {
    id: 'consultas',
    label: 'Consultas',
    defaultOpen: true,
    items: [{ to: '/reportes', label: 'Reportes' }],
  },
  {
    id: 'organizacion',
    label: 'Organización',
    defaultOpen: false,
    items: [
      { to: '/catalogos/empresas', label: 'Empresas' },
      { to: '/catalogos/sedes', label: 'Sedes' },
      { to: '/catalogos/areas', label: 'Áreas' },
      { to: '/catalogos/ubicaciones', label: 'Ubicaciones' },
      { to: '/catalogos/responsables', label: 'Responsables' },
      { to: '/catalogos/proveedores', label: 'Proveedores' },
    ],
  },
  {
    id: 'administracion',
    label: 'Administración',
    defaultOpen: false,
    items: [
      { to: '/catalogos/usuarios', label: 'Usuarios', requiresEscrituraEmpresa: true },
      { to: '/catalogos/categorias', label: 'Categorías' },
      { to: '/catalogos/estados', label: 'Estados', requiresAdminGeneral: true },
      { to: '/catalogos/tipos-asignacion', label: 'Tipos de asignación', requiresAdminGeneral: true },
      { to: '/catalogos/bitacora', label: 'Bitácora' },
    ],
  },
];
