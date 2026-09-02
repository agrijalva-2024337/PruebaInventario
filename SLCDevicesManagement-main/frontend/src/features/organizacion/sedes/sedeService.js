import { sedes } from '@/features/organizacion/mocks/sedes';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.sedes,
  seed: sedes,
});
