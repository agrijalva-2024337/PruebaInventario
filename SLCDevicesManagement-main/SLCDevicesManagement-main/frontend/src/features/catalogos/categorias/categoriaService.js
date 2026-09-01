import { categorias } from '@/features/catalogos/mocks/categorias';
import { apiPaths } from '@/shared/api/paths';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

export const { getAll, getById, create, update, remove } = createMockCrudService({
  endpoint: apiPaths.categoriasActivo,
  seed: categorias,
});
