import { usuarios } from '@/features/organizacion/mocks/usuarios';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.usuarios,
  seed: usuarios,
});
