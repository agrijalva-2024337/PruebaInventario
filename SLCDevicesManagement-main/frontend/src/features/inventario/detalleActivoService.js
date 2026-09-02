import { detallesActivo } from '@/features/inventario/mocks/detallesActivo';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.detallesActivo,
  seed: detallesActivo,
});
