import { apiPaths } from '@/shared/api/paths';
import { env } from '@/shared/config/env';
import httpClient from '@/shared/services/httpClient';
import { activos } from '@/features/activos/mocks/activos';

export async function getConsultaActivo(id) {
  const numericId = Number(id);

  if (env.useApiMock) {
    const found = activos.find((item) => item.id === numericId);
    if (!found) {
      const error = new Error('No se encontró el activo.');
      error.status = 404;
      throw error;
    }

    return {
      id: found.id,
      codigoInterno: `A-${String(found.id).padStart(5, '0')}`,
      nombre: found.nombre,
      descripcion: found.descripcion,
      marca: found.marca,
      modelo: found.modelo,
      numeroSerie: found.numeroSerie,
      nombreCategoria: null,
      nombreEmpresa: null,
      nombreSede: null,
      nombreUbicacion: null,
      nombreArea: null,
      nombreResponsable: null,
      estadoOperativo: found.estadoOperativo ?? 'disponible',
      estadoNombre: found.estadoNombre ?? 'Disponible',
      fechaVencimientoGarantia: found.fechaVencimientoGarantia,
    };
  }

  const response = await httpClient.get(`${apiPaths.consultaActivos}/${numericId}`);
  return response.data;
}
