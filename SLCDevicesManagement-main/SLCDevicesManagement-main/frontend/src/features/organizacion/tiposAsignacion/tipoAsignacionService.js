import { tiposAsignacion } from '@/features/organizacion/mocks/tiposAsignacion';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.tiposAsignacion,
  seed: tiposAsignacion,
});
