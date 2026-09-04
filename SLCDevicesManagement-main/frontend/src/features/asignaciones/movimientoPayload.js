import { toIsoDate, todayInputValue } from '@/shared/utils/dates';
import { dataUrlToBase64 } from '@/shared/utils/qr';
import * as asignacionService from '@/features/asignaciones/asignacionService';

export const KIND_META = {
  asignacion: {
    title: 'Asignaciones',
    description: 'Entrega de un activo a una persona o al responsable de un área. Ocupa el bien hasta la devolución.',
    tipoNombre: 'Asignacion',
    actionLabel: 'Nueva entrega',
    submit: asignacionService.entregar,
    success: 'La entrega se registró correctamente.',
  },
  traslado: {
    title: 'Traslados',
    description: 'Cambio de ubicación. No ocupa el activo.',
    tipoNombre: 'Traslado',
    actionLabel: 'Nuevo traslado',
    submit: asignacionService.trasladar,
    success: 'El traslado se registró correctamente.',
  },
  mantenimiento: {
    title: 'Mantenimientos',
    description: 'Envío a mantenimiento. Ocupa el activo hasta finalizar.',
    tipoNombre: 'Mantenimiento',
    actionLabel: 'Iniciar mantenimiento',
    submit: asignacionService.iniciarMantenimiento,
    success: 'El mantenimiento se inició correctamente.',
  },
  baja: {
    title: 'Bajas',
    description: 'Baja operativa (venta, desecho, donación, pérdida, robo, daño u otro). El activo no se borra.',
    tipoNombre: 'Baja',
    actionLabel: 'Registrar baja',
    submit: asignacionService.darDeBaja,
    success: 'La baja se registró correctamente.',
  },
};

export function normalizarTipo(nombre) {
  return String(nombre ?? '')
    .trim()
    .replaceAll('ó', 'o')
    .replaceAll('Ó', 'o');
}

export function esTipo(nombre, esperado) {
  return normalizarTipo(nombre).toLowerCase() === normalizarTipo(esperado).toLowerCase();
}

export function emptyMovimientoForm(idActivo = '') {
  return {
    idActivo,
    idResponsable: '',
    idUbicacion: '',
    fechaAsignacion: todayInputValue(),
    observaciones: '',
    motivo: '',
    tipoMantenimiento: 'Preventivo',
    idArea: '',
    idProveedorTrabajo: '',
    costo: '',
    numeroFactura: '',
    documentoPdfUrl: '',
    firmaEntrega: '',
    firmaRecibe: '',
  };
}

export function buildMovimientoPayload(kind, values, usuario, tipos, proveedorOptions = []) {
  const idUsuario = Number(usuario?.id ?? usuario?.Id);
  if (!Number.isInteger(idUsuario) || idUsuario <= 0) {
    throw new Error('No hay un usuario de sesión para registrar quien entrega. Cierra sesión y vuelve a entrar.');
  }

  const base = {
    idActivo: Number(values.idActivo),
    idUsuario,
    idResponsable: Number(values.idResponsable),
    idUbicacion: Number(values.idUbicacion),
    fechaAsignacion: toIsoDate(values.fechaAsignacion),
  };

  if (kind === 'baja') {
    return {
      ...base,
      motivo: values.motivo,
      documentoPdfUrl: values.documentoPdfUrl || null,
      firmaEntrega: dataUrlToBase64(values.firmaEntrega),
      firmaRecibe: dataUrlToBase64(values.firmaRecibe),
    };
  }

  if (kind === 'asignacion') {
    const tipo = tipos.find((item) => esTipo(item.nombre, 'Asignacion'));
    if (!tipo) {
      throw new Error('No existe el tipo Asignacion. Créalo en Catálogos → Tipos de asignación.');
    }

    return {
      ...base,
      idUbicacion: Number(values.idUbicacion),
      idTipoAsignacion: tipo.id,
      observaciones: values.observaciones || null,
      firmaEntrega: dataUrlToBase64(values.firmaEntrega),
      firmaRecibe: dataUrlToBase64(values.firmaRecibe),
    };
  }

  if (kind === 'mantenimiento') {
    const proveedor = proveedorOptions.find(
      (item) => String(item.value) === String(values.idProveedorTrabajo),
    );
    return {
      ...base,
      idUbicacion: Number(values.idUbicacion),
      observaciones: values.observaciones || null,
      tipoMantenimiento: values.tipoMantenimiento || null,
      costo: values.costo || null,
      numeroFactura: values.numeroFactura || null,
      proveedorTrabajo: proveedor?.label || null,
    };
  }

  return {
    ...base,
    idUbicacion: Number(values.idUbicacion),
    observaciones: values.observaciones || null,
  };
}
