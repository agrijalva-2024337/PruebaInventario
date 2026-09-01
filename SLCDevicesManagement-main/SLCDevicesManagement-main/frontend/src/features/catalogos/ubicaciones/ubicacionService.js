import { ubicaciones } from '@/features/catalogos/mocks/ubicaciones';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.ubicaciones,
  seed: ubicaciones,
});
