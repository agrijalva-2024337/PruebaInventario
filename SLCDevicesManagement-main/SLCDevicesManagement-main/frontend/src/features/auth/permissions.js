import { RolUsuario, rolUsuarioLabel } from '@/shared/api/contracts';

const ROLE_NAME_TO_VALUE = {
  Consulta: RolUsuario.Consulta,
  OperadorInventario: RolUsuario.OperadorInventario,
  AdministradorEmpresa: RolUsuario.AdministradorEmpresa,
  AdministradorGeneral: RolUsuario.AdministradorGeneral,
};

export function normalizeRol(usuario) {
  if (usuario == null) {
    return null;
  }

  if (typeof usuario.rol === 'number') {
    return usuario.rol;
  }

  const name = usuario.role ?? usuario.rol;
  return ROLE_NAME_TO_VALUE[name] ?? null;
}

export function rolLabel(usuario) {
  const rol = normalizeRol(usuario);
  return rol == null ? '' : rolUsuarioLabel[rol] ?? '';
}

export function isAdministradorGeneral(usuario) {
  return normalizeRol(usuario) === RolUsuario.AdministradorGeneral;
}

/** Sedes, áreas, usuarios, proveedores, categorías. */
export function canWriteEmpresa(usuario) {
  const rol = normalizeRol(usuario);
  return rol === RolUsuario.AdministradorEmpresa || rol === RolUsuario.AdministradorGeneral;
}

/** Ubicaciones, activos, asignaciones, jornadas. */
export function canWriteOperativa(usuario) {
  return (
    normalizeRol(usuario) === RolUsuario.OperadorInventario || canWriteEmpresa(usuario)
  );
}

export function canCreateEmpresa(usuario) {
  return isAdministradorGeneral(usuario);
}
