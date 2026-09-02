export const ESTADO_OPERATIVO = {
  disponible: 'disponible',
  asignado: 'asignado',
  mantenimiento: 'mantenimiento',
  baja: 'baja',
};

const LABELS = {
  disponible: 'Disponible',
  asignado: 'Asignado',
  mantenimiento: 'En mantenimiento',
  baja: 'Baja',
};

const BADGE_VARIANT = {
  disponible: 'success',
  asignado: 'primary',
  mantenimiento: 'warning',
  baja: 'danger',
};

export function estadoOperativo(activo) {
  return String(activo?.estadoOperativo ?? ESTADO_OPERATIVO.disponible).toLowerCase();
}

export function estadoNombre(activo) {
  const codigo = estadoOperativo(activo);
  return activo?.estadoNombre || LABELS[codigo] || LABELS.disponible;
}

export function estadoBadgeVariant(activo) {
  return BADGE_VARIANT[estadoOperativo(activo)] ?? 'secondary';
}

export function puedeMoverActivo(activo, kind) {
  const estado = estadoOperativo(activo);
  if (estado === ESTADO_OPERATIVO.baja) {
    return false;
  }

  if (kind === 'asignacion' || kind === 'mantenimiento') {
    return estado === ESTADO_OPERATIVO.disponible;
  }

  if (kind === 'baja') {
    return estado === ESTADO_OPERATIVO.asignado || estado === ESTADO_OPERATIVO.disponible;
  }

  if (kind === 'traslado') {
    return estado !== ESTADO_OPERATIVO.mantenimiento;
  }

  return true;
}

export function activosParaMovimiento(activos, kind) {
  return (activos ?? []).filter((activo) => puedeMoverActivo(activo, kind));
}
