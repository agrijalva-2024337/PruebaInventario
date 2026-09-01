import { useEffect, useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { DataTable } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { SelectField } from '@/shared/components/SelectField';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import * as reporteService from '@/features/reportes/reporteService';

const TABS = [
  { id: 'general', label: 'Inventario general' },
  { id: 'activos', label: 'Activos' },
  { id: 'sede', label: 'Por sede' },
  { id: 'categoria', label: 'Por categoría' },
  { id: 'responsable', label: 'Por responsable' },
  { id: 'garantias', label: 'Garantías' },
];

const ESTADO_OPTIONS = [
  { value: '', label: 'Todos' },
  { value: 'disponible', label: 'Disponible' },
  { value: 'asignado', label: 'Asignado' },
  { value: 'mantenimiento', label: 'Mantenimiento' },
  { value: 'baja', label: 'Baja' },
];

export function ReportesPage() {
  const [tab, setTab] = useState('general');
  const [estado, setEstado] = useState('');
  const [rows, setRows] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [banner, setBanner] = useState(null);

  useEffect(() => {
    let cancelled = false;
    setIsLoading(true);

    const loaders = {
      general: () => reporteService.inventarioGeneral(),
      activos: () => reporteService.activos({ estado: estado || undefined, take: 200 }),
      sede: () => reporteService.activosPorSede(),
      categoria: () => reporteService.activosPorCategoria(),
      responsable: () => reporteService.activosPorResponsable(),
      garantias: () => reporteService.garantiasPorVencer({ dias: 30 }),
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
  }, [tab, estado]);

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
      { key: 'nombre', header: 'Activo' },
      { key: 'estadoOperativo', header: 'Estado' },
      { key: 'nombreSede', header: 'Sede' },
    ],
    sede: [
      { key: 'nombreSede', header: 'Sede' },
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
  };

  const tableRows =
    tab === 'activos'
      ? rows.map((row) => ({
          ...row,
          nombre: row.activo?.nombre ?? row.nombre,
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
        description="Resumen operativo filtrado por la empresa de tu sesión (el administrador general ve todas)."
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
