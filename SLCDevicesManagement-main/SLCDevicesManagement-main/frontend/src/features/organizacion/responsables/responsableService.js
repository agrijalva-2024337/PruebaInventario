import { responsables } from '@/features/organizacion/mocks/responsables';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.responsables,
  seed: responsables,
});
