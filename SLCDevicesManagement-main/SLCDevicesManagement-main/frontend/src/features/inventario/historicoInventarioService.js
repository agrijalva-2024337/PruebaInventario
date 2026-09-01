import { historicosInventario } from '@/features/inventario/mocks/historicosInventario';
import { apiPaths } from '@/shared/api/paths';
import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

const crud = createMockCrudService({
  endpoint: apiPaths.historicosInventario,
  seed: historicosInventario,
});

export const { getAll, getById, create } = crud;

export async function cerrar(id, data) {
  if (env.useApiMock) {
    return crud.update(id, { cerrado: true, ...data });
  }

  await httpClient.post(`${apiPaths.historicosInventario}/${id}/cerrar`, { id, ...data });
}

export async function getDiferencias(id, incluirAbierta = false) {
  if (env.useApiMock) {
    return {
      idHistoricoInventario: id,
      idSede: 0,
      cerrado: true,
      encontradosEnSede: [],
      noEncontrados: [],
      sobrantes: [],
      faltantesSinRegistrar: [],
    };
  }

  const response = await httpClient.get(`${apiPaths.historicosInventario}/${id}/diferencias`, {
    params: { incluirAbierta },
  });
  return response.data;
}

export async function registrarDetalle(id, data) {
  if (env.useApiMock) {
    return { id: Date.now(), ...data, idHistoricoInventario: id };
  }

  const response = await httpClient.post(`${apiPaths.historicosInventario}/${id}/detalles`, {
    ...data,
    idHistoricoInventario: id,
  });
  return response.data;
}
