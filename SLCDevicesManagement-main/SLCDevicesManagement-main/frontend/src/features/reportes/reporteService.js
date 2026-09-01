import { apiPaths } from '@/shared/api/paths';
import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';

async function get(path, params) {
  if (env.useApiMock) {
    return [];
  }

  const response = await httpClient.get(path, { params });
  return response.data ?? [];
}

export function inventarioGeneral(params) {
  return get(apiPaths.reportes.inventarioGeneral, params);
}

export function activos(params) {
  return get(apiPaths.reportes.activos, params);
}

export function activosPorSede(params) {
  return get(apiPaths.reportes.activosPorSede, params);
}

export function activosPorCategoria(params) {
  return get(apiPaths.reportes.activosPorCategoria, params);
}

export function activosPorResponsable(params) {
  return get(apiPaths.reportes.activosPorResponsable, params);
}

export function garantiasPorVencer(params) {
  return get(apiPaths.reportes.garantiasPorVencer, params);
}
