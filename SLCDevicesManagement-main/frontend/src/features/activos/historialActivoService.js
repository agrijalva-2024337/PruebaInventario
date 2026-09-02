import { historialActivos } from '@/features/activos/mocks/historialActivos';
import { apiPaths } from '@/shared/api/paths';
import { createReadService } from '@/shared/services/createMockCrudService';

export const { getAll, getById } = createReadService({
  endpoint: apiPaths.historialActivos,
  seed: historialActivos,
});
