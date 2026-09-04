import { redesConocidas } from '@/features/catalogos/mocks/redesConocidas';
import { apiPaths } from '@/shared/api/paths';
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

let items = clone(redesConocidas);

export async function getAll(params) {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    const idUbicacion = params?.idUbicacion;
    const rows = idUbicacion
      ? items.filter((item) => item.idUbicacion === Number(idUbicacion))
      : items;
    return clone(rows);
  }

  const response = await httpClient.get(apiPaths.redesConocidas, { params });
  return response.data;
}

export async function getById(id) {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    const found = items.find((item) => item.id === Number(id));
    if (!found) {
      const error = new Error('No se encontró el registro solicitado.');
      error.status = 404;
      throw error;
    }
    return clone(found);
  }

  const response = await httpClient.get(`${apiPaths.redesConocidas}/${id}`);
  return response.data;
}

export async function create(data) {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    const nextId = items.reduce((max, item) => Math.max(max, item.id), 0) + 1;
    const created = { ...data, id: nextId };
    items = [...items, created];
    return clone(created);
  }

  const response = await httpClient.post(apiPaths.redesConocidas, data);
  return response.data;
}

export async function update(id, data) {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    const numericId = Number(id);
    items = items.map((item) => (item.id === numericId ? { ...item, ...data, id: numericId } : item));
    return clone(items.find((item) => item.id === numericId));
  }

  const response = await httpClient.put(`${apiPaths.redesConocidas}/${id}`, data);
  return response.data;
}

export async function remove(id) {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    items = items.filter((item) => item.id !== Number(id));
    return { id: Number(id) };
  }

  const response = await httpClient.delete(`${apiPaths.redesConocidas}/${id}`);
  return response.data ?? { id: Number(id) };
}
