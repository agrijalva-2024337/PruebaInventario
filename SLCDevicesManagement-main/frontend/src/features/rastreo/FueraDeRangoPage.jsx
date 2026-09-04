import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { DataTable } from '@/shared/components/DataTable';
import { PageHeader } from '@/shared/components/PageHeader';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import * as ubicacionService from '@/features/catalogos/ubicaciones/ubicacionService';
import * as dispositivoService from '@/features/rastreo/dispositivoService';

function formatDateTime(value) {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return String(value);
  }

  return date.toLocaleString('es-GT');
}

export function FueraDeRangoPage() {
  const [rows, setRows] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [banner, setBanner] = useState(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      try {
        const [alertas, ubicaciones] = await Promise.all([
          dispositivoService.getFueraDeRango(),
          ubicacionService.getAll(),
        ]);
        if (cancelled) {
          return;
        }

        const ubicacionById = new Map(ubicaciones.map((item) => [item.id, item.nombre]));
        setRows(
          (alertas ?? []).map((row) => ({
            ...row,
            ubicacionAsignadaNombre: row.idUbicacionAsignada
              ? (ubicacionById.get(row.idUbicacionAsignada) ?? `#${row.idUbicacionAsignada}`)
              : 'Sin asignar',
            ubicacionDetectadaNombre: row.idUbicacionDetectada
              ? (ubicacionById.get(row.idUbicacionDetectada) ?? `#${row.idUbicacionDetectada}`)
              : 'Red desconocida',
            ultimoUsoFmt: formatDateTime(row.ultimoUsoEn),
            posicionFmt:
              row.ultimaLatitud != null && row.ultimaLongitud != null
                ? `${row.ultimaLatitud}, ${row.ultimaLongitud}`
                : '—',
          })),
        );
        setErrorMessage(null);
      } catch (error) {
        if (!cancelled) {
          setErrorMessage(getErrorMessage(error));
          setBanner({ variant: 'error', message: getErrorMessage(error) });
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, []);

  const columns = [
    {
      key: 'nombreActivo',
      header: 'Activo',
      render: (row) => (
        <Link className="text-slate-900 underline decoration-slate-300" to={`/activos/${row.idActivo}`}>
          {row.nombreActivo}
        </Link>
      ),
    },
    { key: 'ubicacionAsignadaNombre', header: 'Ubicación asignada' },
    { key: 'ubicacionDetectadaNombre', header: 'Ubicación detectada' },
    { key: 'posicionFmt', header: 'Última posición' },
    { key: 'ultimoUsoFmt', header: 'Último ping' },
    {
      key: 'alerta',
      header: 'Estado',
      render: () => <Badge variant="danger">Fuera de rango</Badge>,
    },
  ];

  return (
    <section className="space-y-6">
      <PageHeader
        title="Fuera de rango"
        description="Equipos cuyo último ping Wi-Fi no coincide con la ubicación asignada por un traslado autorizado."
      />

      {banner ? (
        <AlertBanner
          variant={banner.variant}
          message={banner.message}
          onDismiss={() => setBanner(null)}
        />
      ) : null}

      <DataTable
        columns={columns}
        rows={rows}
        keyField="idActivo"
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage="Ningún equipo está fuera de rango en este momento."
        searchPlaceholder="Buscar activos..."
      />
    </section>
  );
}
