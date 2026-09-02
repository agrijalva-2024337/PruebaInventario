import { useEffect, useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Button } from '@/shared/components/Button';
import { DataTable } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { SelectField } from '@/shared/components/SelectField';
import { subscribeEmpresaActiva } from '@/shared/empresaActiva';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { formatDate } from '@/shared/utils/dates';
import * as historialActivoService from '@/features/activos/historialActivoService';
import * as asignacionService from '@/features/asignaciones/asignacionService';
import { esTipo } from '@/features/asignaciones/movimientoPayload';
import * as tipoAsignacionService from '@/features/organizacion/tiposAsignacion/tipoAsignacionService';
import * as reporteService from '@/features/reportes/reporteService';

const TABS = [
  { id: 'general', label: 'Inventario general' },
  { id: 'activos', label: 'Activos' },
  { id: 'sede', label: 'Por sede' },
  { id: 'ubicacion', label: 'Por ubicación' },
  { id: 'categoria', label: 'Por categoría' },
  { id: 'responsable', label: 'Por responsable' },
  { id: 'garantias', label: 'Garantías' },
  { id: 'movimientos', label: 'Historial movimientos' },
  { id: 'asignaciones', label: 'Historial asignaciones' },
  { id: 'mantenimientos', label: 'Historial mantenimientos' },
  { id: 'diferencias', label: 'Diferencias inventario' },
];

const ESTADO_OPTIONS = [
  { value: '', label: 'Todos' },
  { value: 'disponible', label: 'Disponible' },
  { value: 'asignado', label: 'Asignado' },
  { value: 'mantenimiento', label: 'Mantenimiento' },
  { value: 'baja', label: 'Baja' },
];

async function asignacionesPorTipo(nombreTipo) {
  const [rows, tipos] = await Promise.all([asignacionService.getAll(), tipoAsignacionService.getAll()]);
  const tipoById = new Map((tipos ?? []).map((item) => [item.id, item.nombre]));
  return (rows ?? [])
    .filter((row) => esTipo(tipoById.get(row.idTipoAsignacion), nombreTipo))
    .map((row) => ({
      ...row,
      tipoNombre: tipoById.get(row.idTipoAsignacion) ?? row.idTipoAsignacion,
      fechaFmt: formatDate(row.fechaAsignacion),
      estadoLabel: row.activa ? 'Activa' : 'Cerrada',
    }));
}

export function ReportesPage() {
  const [tab, setTab] = useState('general');
  const [estado, setEstado] = useState('');
  const [empresaTick, setEmpresaTick] = useState(0);
  const [rows, setRows] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [banner, setBanner] = useState(null);

  useEffect(() => subscribeEmpresaActiva(() => setEmpresaTick((n) => n + 1)), []);

  useEffect(() => {
    let cancelled = false;
    setIsLoading(true);

    const loaders = {
      general: () => reporteService.inventarioGeneral(),
      activos: () => reporteService.activos({ estado: estado || undefined, take: 200 }),
      sede: () => reporteService.activosPorSede(),
      ubicacion: () => reporteService.activosPorUbicacion(),
      categoria: () => reporteService.activosPorCategoria(),
      responsable: () => reporteService.activosPorResponsable(),
      garantias: () => reporteService.garantiasPorVencer({ dias: 30 }),
      movimientos: async () =>
        (await historialActivoService.getAll()).map((row) => ({
          ...row,
          fechaFmt: formatDate(row.fechaHora),
        })),
      asignaciones: () => asignacionesPorTipo('Asignacion'),
      mantenimientos: () => asignacionesPorTipo('Mantenimiento'),
      diferencias: async () =>
        (await reporteService.diferenciasInventario()).map((row) => ({
          ...row,
          fechaFmt: formatDate(row.fechaInicio),
        })),
    };

    loaders[tab]()
      .then((data) => {
        if (!cancelled) {
          setRows(Array.isArray(data) ? data : []);
          setErrorMessage(null);
        }
      })
      .catch((error) => {
        if (!cancelled) {
          setErrorMessage(getErrorMessage(error));
          setRows([]);
          setBanner({ variant: 'error', message: getErrorMessage(error) });
        }
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [tab, estado, empresaTick]);

  const columnsByTab = {
    general: [
      { key: 'nombreEmpresa', header: 'Empresa' },
      { key: 'totalActivos', header: 'Total' },
      { key: 'disponibles', header: 'Disponibles' },
      { key: 'asignados', header: 'Asignados' },
      { key: 'enMantenimiento', header: 'Mantenimiento' },
      { key: 'dadosDeBaja', header: 'Baja' },
    ],
    activos: [
      { key: 'codigoInterno', header: 'Código' },
      { key: 'nombre', header: 'Activo' },
      { key: 'estadoOperativo', header: 'Estado' },
      { key: 'nombreSede', header: 'Sede' },
      { key: 'nombreUbicacion', header: 'Ubicación' },
    ],
    sede: [
      { key: 'nombreSede', header: 'Sede' },
      { key: 'totalActivos', header: 'Total' },
      { key: 'disponibles', header: 'Disponibles' },
      { key: 'asignados', header: 'Asignados' },
      { key: 'enMantenimiento', header: 'Mantenimiento' },
      { key: 'dadosDeBaja', header: 'Baja' },
    ],
    ubicacion: [
      { key: 'nombreSede', header: 'Sede' },
      { key: 'nombreUbicacion', header: 'Ubicación' },
      { key: 'totalActivos', header: 'Total' },
      { key: 'disponibles', header: 'Disponibles' },
      { key: 'asignados', header: 'Asignados' },
    ],
    categoria: [
      { key: 'nombreCategoria', header: 'Categoría' },
      { key: 'totalActivos', header: 'Total' },
      { key: 'disponibles', header: 'Disponibles' },
      { key: 'asignados', header: 'Asignados' },
    ],
    responsable: [
      { key: 'nombreResponsable', header: 'Responsable' },
      { key: 'totalAsignados', header: 'Asignados' },
    ],
    garantias: [
      { key: 'nombre', header: 'Activo' },
      { key: 'diasRestantes', header: 'Días' },
      { key: 'nombreSede', header: 'Sede' },
    ],
    movimientos: [
      { key: 'fechaFmt', header: 'Fecha' },
      { key: 'tipoOperacion', header: 'Tipo' },
      { key: 'descripcion', header: 'Descripción' },
      { key: 'informacionAnterior', header: 'Anterior' },
      { key: 'informacionNueva', header: 'Nueva' },
    ],
    asignaciones: [
      { key: 'idActivo', header: 'Activo' },
      { key: 'idResponsable', header: 'Responsable' },
      { key: 'fechaFmt', header: 'Fecha' },
      { key: 'estadoLabel', header: 'Estado' },
      { key: 'observaciones', header: 'Observaciones' },
    ],
    mantenimientos: [
      { key: 'idActivo', header: 'Activo' },
      { key: 'idResponsable', header: 'Responsable' },
      { key: 'fechaFmt', header: 'Fecha' },
      { key: 'estadoLabel', header: 'Estado' },
      { key: 'observaciones', header: 'Detalle' },
    ],
    diferencias: [
      { key: 'nombreSede', header: 'Sede' },
      { key: 'fechaFmt', header: 'Jornada' },
      { key: 'tipo', header: 'Diferencia' },
      { key: 'nombreActivo', header: 'Activo' },
      { key: 'nombreUbicacion', header: 'Ubicación' },
    ],
  };

  const tableRows =
    tab === 'activos'
      ? rows.map((row) => ({
          ...row,
          nombre: row.activo?.nombre ?? row.nombre,
          codigoInterno: row.activo?.codigoInterno ?? row.codigoInterno,
          nombreUbicacion: row.activo?.nombreUbicacion ?? row.nombreUbicacion,
        }))
      : tab === 'garantias'
        ? rows.map((row) => ({
            ...row,
            nombre: row.activo?.nombre ?? row.nombre,
          }))
        : rows;

  return (
    <section className="space-y-6">
      <PageHeader
        title="Reportes"
        description="Filtrados por la empresa activa. Inventario, estados, historial de movimientos y diferencias de inventario físico."
        actions={
          <Button
            type="button"
            variant="secondary"
            onClick={() => {
              const columns = columnsByTab[tab] ?? [];
              const header = columns.map((c) => c.header).join(',');
              const lines = tableRows.map((row) =>
                columns.map((c) => `"${String(row[c.key] ?? '').replaceAll('"', '""')}"`).join(','),
              );
              const blob = new Blob([[header, ...lines].join('\n')], { type: 'text/csv;charset=utf-8;' });
              const url = URL.createObjectURL(blob);
              const link = document.createElement('a');
              link.href = url;
              link.download = `reporte-${tab}.csv`;
              link.click();
              URL.revokeObjectURL(url);
            }}
          >
            Exportar CSV
          </Button>
        }
      />
      {banner ? (
        <AlertBanner variant={banner.variant} message={banner.message} onDismiss={() => setBanner(null)} />
      ) : null}
      <div className="flex flex-wrap gap-2">
        {TABS.map((item) => (
          <button
            key={item.id}
            type="button"
            className={[
              'rounded-md px-3 py-1.5 text-sm',
              tab === item.id ? 'bg-slate-900 text-white' : 'border border-slate-200 bg-white text-slate-700',
            ].join(' ')}
            onClick={() => setTab(item.id)}
          >
            {item.label}
          </button>
        ))}
      </div>
      {tab === 'activos' ? (
        <div className="max-w-xs">
          <SelectField
            label="Estado operativo"
            name="estado"
            value={estado}
            onChange={(field) => setEstado(field.value ?? '')}
            options={ESTADO_OPTIONS.filter((item) => item.value)}
            placeholder="Todos"
          />
        </div>
      ) : null}
      <DataTable
        columns={columnsByTab[tab]}
        rows={tableRows}
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage="No hay datos para este reporte."
        searchPlaceholder="Buscar..."
      />
    </section>
  );
}
