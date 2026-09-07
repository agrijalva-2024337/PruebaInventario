import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { Button } from '@/shared/components/Button';
import { DataTable } from '@/shared/components/DataTable';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { formatDate, formatDateTime } from '@/shared/utils/dates';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import { QrCodeCard } from '@/features/activos/QrCodeCard';
import { estadoBadgeVariant, estadoNombre, puedeMoverActivo } from '@/features/activos/activoEstado';
import * as activoService from '@/features/activos/activoService';
import { MovimientoForm } from '@/features/asignaciones/MovimientoForm';
import * as asignacionService from '@/features/asignaciones/asignacionService';
import {
  KIND_META,
  buildMovimientoPayload,
  emptyMovimientoForm,
  esTipo,
} from '@/features/asignaciones/movimientoPayload';
import * as areaService from '@/features/organizacion/areas/areaService';
import * as categoriaService from '@/features/catalogos/categorias/categoriaService';
import * as proveedorService from '@/features/catalogos/proveedores/proveedorService';
import * as ubicacionService from '@/features/catalogos/ubicaciones/ubicacionService';
import * as responsableService from '@/features/organizacion/responsables/responsableService';
import * as tipoAsignacionService from '@/features/organizacion/tiposAsignacion/tipoAsignacionService';
import * as dispositivoService from '@/features/rastreo/dispositivoService';

const MOVIMIENTO_KINDS = ['asignacion', 'traslado', 'mantenimiento', 'baja'];

function origenCoordenadaLabel(origen) {
  if (origen === 'gps') {
    return 'GPS del equipo';
  }
  if (origen === 'wifi') {
    return 'Red Wi-Fi catalogada';
  }
  return '—';
}

function toOptions(items, labelKey = 'nombre') {
  return (items ?? [])
    .filter((item) => item.habilitado !== false)
    .map((item) => ({ value: item.id, label: item[labelKey] ?? `#${item.id}` }));
}

export function ActivoDetallePage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { usuario } = useAuth();
  const puedeMover = canWriteOperativa(usuario);
  const activoId = Number(id);

  const [activo, setActivo] = useState(null);
  const [historial, setHistorial] = useState([]);
  const [lookups, setLookups] = useState({
    categorias: [],
    proveedores: [],
    ubicaciones: [],
    responsables: [],
    tipos: [],
    areas: [],
  });
  const [isLoading, setIsLoading] = useState(true);
  const [banner, setBanner] = useState(null);
  const [movimientoKind, setMovimientoKind] = useState(null);
  const [saving, setSaving] = useState(false);
  const [rastreo, setRastreo] = useState(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      try {
        const [item, history, categorias, proveedores, ubicaciones, responsables, tipos, areas, rastreoItem] =
          await Promise.all([
            activoService.getById(activoId),
            activoService.getHistorial(activoId),
            categoriaService.getAll(),
            proveedorService.getAll(),
            ubicacionService.getAll(),
            responsableService.getAll(),
            tipoAsignacionService.getAll(),
            areaService.getAll(),
            dispositivoService.getRastreoByActivo(activoId).catch(() => null),
          ]);
        if (!cancelled) {
          setActivo(item);
          setHistorial(Array.isArray(history) ? history : []);
          setLookups({ categorias, proveedores, ubicaciones, responsables, tipos, areas });
          setRastreo(rastreoItem);
        }
      } catch (error) {
        if (!cancelled) {
          setBanner({ variant: 'error', message: getErrorMessage(error) });
          setActivo(null);
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    if (Number.isFinite(activoId) && activoId > 0) {
      load();
    }

    return () => {
      cancelled = true;
    };
  }, [activoId]);

  const categoriaNombre = lookups.categorias.find((item) => item.id === activo?.idCategoriaActivo)?.nombre;
  const proveedorNombre = lookups.proveedores.find((item) => item.id === activo?.idProveedor)?.nombre;
  const ubicacionNombre = lookups.ubicaciones.find((item) => item.id === activo?.idUbicacion)?.nombre;

  async function handleSave(values) {
    setSaving(true);
    try {
      await KIND_META[movimientoKind].submit(
        buildMovimientoPayload(movimientoKind, values, usuario, lookups.tipos, toOptions(lookups.proveedores)),
      );
      setBanner({ variant: 'success', message: KIND_META[movimientoKind].success });
      setMovimientoKind(null);
      const [item, history] = await Promise.all([
        activoService.getById(activoId),
        activoService.getHistorial(activoId),
      ]);
      setActivo(item);
      setHistorial(Array.isArray(history) ? history : []);
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setSaving(false);
    }
  }

  async function handlePdf(row) {
    try {
      await asignacionService.downloadPdf(row.id);
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    }
  }

  const historyRows = historial.map((row) => ({
    ...row,
    fechaFmt: formatDate(row.fechaAsignacion),
    estadoLabel: row.activa ? 'Activa' : 'Cerrada',
  }));

  return (
    <section className="space-y-6">
      <PageHeader
        title={activo?.nombre ?? 'Activo'}
        description={
          puedeMover
            ? 'Detalle, QR e historial. Puedes registrar un movimiento desde aquí.'
            : 'Consulta de detalle e historial del activo. No puedes registrar movimientos.'
        }
        actions={
          <div className="flex flex-wrap gap-2">
            <Button variant="secondary" type="button" onClick={() => navigate('/activos/escanear')}>
              Escanear QR
            </Button>
            <Button variant="secondary" type="button" onClick={() => navigate('/activos')}>
              Volver al listado
            </Button>
          </div>
        }
      />
      {banner ? (
        <AlertBanner variant={banner.variant} message={banner.message} onDismiss={() => setBanner(null)} />
      ) : null}

      {isLoading ? <p className="text-sm text-slate-500">Cargando activo...</p> : null}
      {!isLoading && !activo ? (
        <p className="text-sm text-slate-600">
          No se encontró el activo.{' '}
          <Link className="underline" to="/activos">
            Ir al listado
          </Link>
        </p>
      ) : null}

      {activo ? (
        <>
          <div className="grid gap-6 lg:grid-cols-[1fr_16rem]">
            <dl className="grid gap-3 rounded-lg border border-slate-200 bg-white p-4 sm:grid-cols-2">
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Código interno</dt>
                <dd>{activo.codigoInterno || `A-${String(activo.id).padStart(5, '0')}`}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Serie / etiqueta</dt>
                <dd>{activo.numeroSerie || '—'}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Empresa</dt>
                <dd>{activo.nombreEmpresa || '—'}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Sede</dt>
                <dd>{activo.nombreSede || '—'}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Categoría</dt>
                <dd>{categoriaNombre ?? activo.idCategoriaActivo}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Proveedor</dt>
                <dd>{proveedorNombre ?? activo.idProveedor}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Ubicación asignada</dt>
                <dd>{activo.nombreUbicacion || ubicacionNombre || activo.idUbicacion}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Área</dt>
                <dd>{activo.nombreArea || '—'}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Responsable actual</dt>
                <dd>{activo.nombreResponsable || '—'}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Estado</dt>
                <dd>
                  <Badge variant={estadoBadgeVariant(activo)}>{estadoNombre(activo)}</Badge>
                </dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Marca / modelo</dt>
                <dd>
                  {activo.marca || '—'} {activo.modelo || ''}
                </dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-slate-500">Compra</dt>
                <dd>
                  {formatDate(activo.fechaCompra)} · {activo.moneda} {activo.costoAdquisicion}
                </dd>
              </div>
              <div className="sm:col-span-2">
                <dt className="text-xs uppercase tracking-wide text-slate-500">Observaciones</dt>
                <dd>{activo.observaciones || '—'}</dd>
              </div>
            </dl>
            <div className="rounded-lg border border-slate-200 bg-white p-3">
              <QrCodeCard activoId={activo.id} nombre={activo.nombre} />
            </div>
          </div>

          <div className="rounded-lg border border-slate-200 bg-white p-4">
            <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
              <h3 className="text-sm font-semibold">Última ubicación del equipo</h3>
              {rastreo ? (
                <Badge variant={rastreo.fueraDeRango ? 'danger' : 'success'}>
                  {rastreo.fueraDeRango ? 'Fuera de rango' : 'En ubicación'}
                </Badge>
              ) : null}
            </div>
            {rastreo ? (
              <dl className="grid gap-3 sm:grid-cols-2">
                <div>
                  <dt className="text-xs uppercase tracking-wide text-slate-500">Detectada</dt>
                  <dd>{rastreo.ubicacionDetectada?.nombre || 'Red desconocida'}</dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-slate-500">Coordenadas</dt>
                  <dd>
                    {rastreo.ultimaLatitud != null && rastreo.ultimaLongitud != null
                      ? `${rastreo.ultimaLatitud}, ${rastreo.ultimaLongitud}`
                      : '—'}
                  </dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-slate-500">Origen</dt>
                  <dd>{origenCoordenadaLabel(rastreo.origenCoordenada)}</dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-slate-500">BSSID</dt>
                  <dd>{rastreo.ultimoBssid || '—'}</dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-slate-500">Último ping</dt>
                  <dd>{formatDateTime(rastreo.ultimoUsoEn)}</dd>
                </div>
                <div>
                  <dt className="text-xs uppercase tracking-wide text-slate-500">Mapa</dt>
                  <dd>
                    <Link className="underline" to="/rastreo/mapa">
                      Ver en el mapa
                    </Link>
                  </dd>
                </div>
              </dl>
            ) : (
              <p className="text-sm text-slate-500">
                Este activo aún no reporta ubicación. Instalá el agente para ver dónde está el
                equipo.
              </p>
            )}
          </div>

          {puedeMover ? (
            <div className="flex flex-wrap gap-2">
              {MOVIMIENTO_KINDS.filter((kind) => puedeMoverActivo(activo, kind)).map((kind) => (
                <Button key={kind} type="button" onClick={() => setMovimientoKind(kind)}>
                  {KIND_META[kind].actionLabel}
                </Button>
              ))}
              {MOVIMIENTO_KINDS.every((kind) => !puedeMoverActivo(activo, kind)) ? (
                <p className="rounded-md bg-slate-50 px-3 py-2 text-sm text-slate-600">
                  Este activo no admite movimientos en su estado actual.
                </p>
              ) : null}
            </div>
          ) : (
            <p className="rounded-md bg-slate-50 px-3 py-2 text-sm text-slate-600">
              Tu perfil de consulta solo muestra el detalle de este activo.
            </p>
          )}

          <div>
            <h3 className="mb-2 text-sm font-semibold">Historial de movimientos</h3>
            <DataTable
              columns={[
                { key: 'tipoAsignacion', header: 'Tipo' },
                { key: 'responsableRecibe', header: 'Responsable' },
                { key: 'usuarioEntrega', header: 'Registró' },
                { key: 'fechaFmt', header: 'Fecha' },
                {
                  key: 'estadoLabel',
                  header: 'Estado',
                  render: (row) => (
                    <Badge variant={row.activa ? 'success' : 'ghost'}>{row.estadoLabel}</Badge>
                  ),
                },
                {
                  key: 'acciones',
                  header: '',
                  align: 'right',
                  sortable: false,
                  render: (row) =>
                    esTipo(row.tipoAsignacion, 'Asignacion') || esTipo(row.tipoAsignacion, 'Baja') ? (
                      <Button variant="secondary" onClick={() => handlePdf(row)}>
                        Descargar PDF
                      </Button>
                    ) : null,
                },
              ]}
              rows={historyRows}
              emptyMessage="Aún no hay movimientos para este activo."
              pageSize={8}
            />
          </div>
        </>
      ) : null}

      <Modal
        isOpen={Boolean(movimientoKind)}
        onClose={() => (saving ? null : setMovimientoKind(null))}
        title={movimientoKind ? KIND_META[movimientoKind].actionLabel : ''}
        wide={movimientoKind === 'asignacion' || movimientoKind === 'baja'}
      >
        {movimientoKind ? (
          <MovimientoForm
            key={movimientoKind}
            kind={movimientoKind}
            initialValues={emptyMovimientoForm(activoId)}
            activoOptions={toOptions(activo ? [activo] : [])}
            responsableOptions={toOptions(lookups.responsables, 'nombreCompleto')}
            ubicacionOptions={toOptions(lookups.ubicaciones)}
            areaOptions={toOptions(lookups.areas)}
            responsables={lookups.responsables}
            activos={activo ? [activo] : []}
            proveedorOptions={toOptions(lookups.proveedores)}
            onSubmit={handleSave}
            onCancel={() => setMovimientoKind(null)}
            isSubmitting={saving}
            activoLocked
          />
        ) : null}
      </Modal>
    </section>
  );
}
