import { asignaciones } from '@/features/asignaciones/mocks/asignaciones';
import { apiPaths } from '@/shared/api/paths';
import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';
import { createMockCrudService } from '@/shared/services/createMockCrudService';

const crud = createMockCrudService({
  endpoint: apiPaths.asignaciones,
  seed: asignaciones,
});

export const { getAll, getById } = crud;

async function post(path, data) {
  if (env.useApiMock) {
    return crud.create(data);
  }

  const response = await httpClient.post(path, data);
  return response.data;
}

export function entregar(data) {
  return post(`${apiPaths.asignaciones}/entrega`, data);
}

export function trasladar(data) {
  return post(`${apiPaths.asignaciones}/traslado`, data);
}

export function iniciarMantenimiento(data) {
  return post(`${apiPaths.asignaciones}/mantenimiento`, data);
}

export function darDeBaja(data) {
  return post(`${apiPaths.asignaciones}/baja`, data);
}

export async function devolver(id, data) {
  if (env.useApiMock) {
    return crud.update(id, { ...data, activa: false });
  }

  await httpClient.post(`${apiPaths.asignaciones}/${id}/devolver`, data);
}

export async function finalizarMantenimiento(id, data) {
  if (env.useApiMock) {
    return crud.update(id, { ...data, activa: false });
  }

  await httpClient.post(`${apiPaths.asignaciones}/${id}/finalizar-mantenimiento`, data);
}

async function readBlobError(error) {
  const data = error?.response?.data;
  if (!(data instanceof Blob)) {
    return error;
  }

  try {
    const text = await data.text();
    error.response.data = JSON.parse(text);
  } catch {
    error.message = 'No se pudo generar el PDF.';
  }

  return error;
}

export async function downloadPdf(id) {
  if (env.useApiMock) {
    throw new Error('El PDF no está disponible en modo mock.');
  }

  try {
    const response = await httpClient.get(`${apiPaths.asignaciones}/${id}/pdf`, {
      responseType: 'blob',
      timeout: 60000,
    });
    const blob = new Blob([response.data], { type: 'application/pdf' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `acta-${id}.pdf`;
    link.click();
    URL.revokeObjectURL(url);
  } catch (error) {
    throw await readBlobError(error);
  }
}
