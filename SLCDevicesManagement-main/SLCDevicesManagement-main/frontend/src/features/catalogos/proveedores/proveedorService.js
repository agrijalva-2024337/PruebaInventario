import { proveedores } from '@/features/catalogos/mocks/proveedores';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.proveedores,
  seed: proveedores,
});
