import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';

const MOCK_DELAY_MS = 400;

function wait(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

function clone(value) {
  return JSON.parse(JSON.stringify(value));
}

export function createMockCrudService({ endpoint, seed, delayMs = MOCK_DELAY_MS }) {
  let items = clone(seed);
  const usesHabilitado = seed.some((item) => Object.hasOwn(item, 'habilitado'));

  async function getAll(params) {
    if (env.useApiMock) {
      await wait(delayMs);
      return clone(items);
    }

    const response = await httpClient.get(endpoint, {
      params: { incluirInhabilitados: true, ...params },
    });
    return response.data;
  }

  async function getById(id) {
    if (env.useApiMock) {
      await wait(delayMs);
      const found = items.find((item) => item.id === Number(id));

      if (!found) {
        const error = new Error('No se encontró el registro solicitado.');
        error.status = 404;
        throw error;
      }

      return clone(found);
    }

    const response = await httpClient.get(`${endpoint}/${id}`);
    return response.data;
  }

  async function create(data) {
    if (env.useApiMock) {
      await wait(delayMs);
      const nextId = items.reduce((max, item) => Math.max(max, item.id), 0) + 1;
      const created = { ...data, id: nextId };

      if (usesHabilitado) {
        created.habilitado = data.habilitado ?? true;
      }

      items = [...items, created];
      return clone(created);
    }

    const response = await httpClient.post(endpoint, data);
    return response.data;
  }

  async function update(id, data) {
    if (env.useApiMock) {
      await wait(delayMs);
      const numericId = Number(id);
      const current = items.find((item) => item.id === numericId);

      if (!current) {
        const error = new Error('No se encontró el registro solicitado.');
        error.status = 404;
        throw error;
      }

      const updated = { ...current, ...data, id: numericId };
      items = items.map((item) => (item.id === numericId ? updated : item));
      return clone(updated);
    }

    const response = await httpClient.put(`${endpoint}/${id}`, data);
    return response.data;
  }

  async function remove(id) {
    if (env.useApiMock) {
      await wait(delayMs);
      const numericId = Number(id);
      const current = items.find((item) => item.id === numericId);

      if (!current) {
        const error = new Error('No se encontró el registro solicitado.');
        error.status = 404;
        throw error;
      }

      const updated = usesHabilitado ? { ...current, habilitado: false } : current;
      items = items.map((item) => (item.id === numericId ? updated : item));
      return clone(updated);
    }

    const response = await httpClient.post(`${endpoint}/${id}/disable`);
    return response.data ?? { id: Number(id) };
  }

  return { getAll, getById, create, update, remove };
}

export function createReadService(options) {
  const { getAll, getById } = createMockCrudService(options);
  return { getAll, getById };
}
