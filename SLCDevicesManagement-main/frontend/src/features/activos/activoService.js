import { activos } from '@/features/activos/mocks/activos';
import { apiPaths } from '@/shared/api/paths';
import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

const crud = createMockCrudService({
  endpoint: apiPaths.activos,
  seed: activos,
});

export const { getAll, getById, create, update } = crud;

export async function remove(id) {
  if (env.useApiMock) {
    return crud.remove(id);
  }

  await httpClient.delete(`${apiPaths.activos}/${id}`);
  return { id: Number(id) };
}

export async function getHistorial(id) {
  if (env.useApiMock) {
    return [];
  }

  const response = await httpClient.get(`${apiPaths.activos}/${id}/asignaciones`);
  return response.data ?? [];
}
