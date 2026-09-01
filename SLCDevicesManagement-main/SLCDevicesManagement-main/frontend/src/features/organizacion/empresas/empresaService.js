import { empresas } from '@/features/organizacion/mocks/empresas';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.empresas,
  seed: empresas,
});
