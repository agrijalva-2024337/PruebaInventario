import { apiPaths } from '@/shared/api/paths';
import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';

const MOCK_DELAY_MS = 400;

function wait(ms) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

export async function getFueraDeRango() {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    return [
      {
        idActivo: 1,
        nombreActivo: 'Laptop demo',
        idUbicacionAsignada: 1,
        idUbicacionDetectada: 2,
        ultimoUsoEn: new Date().toISOString(),
      },
    ];
  }

  const response = await httpClient.get(`${apiPaths.dispositivos}/fuera-de-rango`);
  return response.data;
}

export async function getMapa() {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    return [
      {
        idActivo: 1,
        nombreActivo: 'Laptop demo',
        fueraDeRango: true,
        ultimoUsoEn: new Date().toISOString(),
        ultimoBssid: 'aa:bb:cc:dd:ee:ff',
        origenCoordenada: 'wifi',
        ultimaLatitud: 14.6108,
        ultimaLongitud: -90.5133,
        ubicacionAsignada: {
          id: 1,
          nombre: 'Oficina Guatemala',
          latitud: 14.6349,
          longitud: -90.5069,
        },
        ubicacionDetectada: {
          id: 2,
          nombre: 'Kiosco Oakland',
          latitud: 14.6108,
          longitud: -90.5133,
        },
      },
    ];
  }

  const response = await httpClient.get(`${apiPaths.dispositivos}/mapa`);
  return response.data;
}

export async function getRastreoByActivo(idActivo) {
  if (env.useApiMock) {
    await wait(MOCK_DELAY_MS);
    return {
      idActivo,
      nombreActivo: 'Laptop demo',
      fueraDeRango: false,
      ultimoUsoEn: new Date().toISOString(),
      ultimoBssid: 'aa:bb:cc:dd:ee:ff',
      origenCoordenada: 'wifi',
      ultimaLatitud: 14.6349,
      ultimaLongitud: -90.5069,
      ubicacionAsignada: {
        id: 1,
        nombre: 'Oficina Guatemala',
        latitud: 14.6349,
        longitud: -90.5069,
      },
      ubicacionDetectada: {
        id: 1,
        nombre: 'Oficina Guatemala',
        latitud: 14.6349,
        longitud: -90.5069,
      },
    };
  }

  const response = await httpClient.get(`${apiPaths.dispositivos}/activos/${idActivo}`);
  return response.data;
}

export async function registrar(idActivo) {
  const response = await httpClient.post(apiPaths.dispositivos, { idActivo });
  return response.data;
}

export async function revocar(id) {
  await httpClient.post(`${apiPaths.dispositivos}/${id}/revocar`);
}
