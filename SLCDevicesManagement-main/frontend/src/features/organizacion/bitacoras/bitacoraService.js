import { bitacoras } from '@/features/organizacion/mocks/bitacoras';
import { apiPaths } from '@/shared/api/paths';
import { createReadService } from '@/shared/services/createMockCrudService';

export const { getAll, getById } = createReadService({
  endpoint: apiPaths.bitacoras,
  seed: bitacoras,
});
