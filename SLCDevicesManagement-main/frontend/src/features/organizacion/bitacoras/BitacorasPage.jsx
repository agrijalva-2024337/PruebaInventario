import { AlertBanner } from '@/shared/components/AlertBanner';
import { DataTable } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { useApiCollection } from '@/shared/hooks/useApiCollection';
import { formatDate } from '@/shared/utils/dates';
import * as bitacoraService from '@/features/organizacion/bitacoras/bitacoraService';

const TIPO_LABEL = {
  0: 'Creación',
  1: 'Modificación',
  2: 'Eliminación',
  Creacion: 'Creación',
  Modificacion: 'Modificación',
  Eliminacion: 'Eliminación',
};

export function BitacorasPage() {
  const { rows, isLoading, errorMessage, banner, setBanner } = useApiCollection(bitacoraService.getAll);

  const tableRows = rows.map((row) => ({
    ...row,
    fechaFmt: formatDate(row.fechaHora),
    tipoLabel: TIPO_LABEL[row.tipoOperacion] ?? row.tipoOperacion,
  }));

  return (
    <section className="space-y-6">
      <PageHeader
        title="Bitácora"
        description="Auditoría de operaciones: usuario, fecha, tipo e información anterior/nueva. Filtrada por la empresa activa."
      />
      {banner ? (
        <AlertBanner variant={banner.variant} message={banner.message} onDismiss={() => setBanner(null)} />
      ) : null}
      <DataTable
        columns={[
          { key: 'fechaFmt', header: 'Fecha' },
          { key: 'nombreUsuario', header: 'Usuario' },
          { key: 'idUsuario', header: 'Id usuario' },
          { key: 'tipoLabel', header: 'Operación' },
          { key: 'entidadAfectada', header: 'Entidad' },
          { key: 'descripcion', header: 'Descripción' },
          { key: 'informacionAnterior', header: 'Anterior' },
          { key: 'informacionNueva', header: 'Nueva' },
        ]}
        rows={tableRows}
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage="No hay registros de bitácora."
        searchPlaceholder="Buscar en bitácora..."
        pageSize={15}
      />
    </section>
  );
}
