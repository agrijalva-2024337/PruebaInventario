import { useCallback, useEffect, useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { Button } from '@/shared/components/Button';
import { CatalogRowActions } from '@/shared/components/CatalogRowActions';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable } from '@/shared/components/DataTable';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { useApiCollection } from '@/shared/hooks/useApiCollection';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { formatDate } from '@/shared/utils/dates';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import * as activoService from '@/features/activos/activoService';
import { MovimientoForm } from '@/features/asignaciones/MovimientoForm';
import * as asignacionService from '@/features/asignaciones/asignacionService';
import {
  KIND_META,
  buildMovimientoPayload,
  emptyMovimientoForm,
  esTipo,
} from '@/features/asignaciones/movimientoPayload';
import { activosParaMovimiento } from '@/features/activos/activoEstado';
import * as areaService from '@/features/organizacion/areas/areaService';
import * as proveedorService from '@/features/catalogos/proveedores/proveedorService';
import * as responsableService from '@/features/organizacion/responsables/responsableService';
import * as tipoAsignacionService from '@/features/organizacion/tiposAsignacion/tipoAsignacionService';
import * as ubicacionService from '@/features/catalogos/ubicaciones/ubicacionService';
import { subscribeEmpresaActiva } from '@/shared/empresaActiva';
import { FormActions } from '@/shared/components/FormActions';
import { TextareaField } from '@/shared/components/TextareaField';

function toOptions(items, labelKey = 'nombre') {
  return (items ?? [])
    .filter((item) => item.habilitado !== false)
    .map((item) => ({ value: item.id, label: item[labelKey] ?? `#${item.id}` }));
}

export function MovimientosPage({ kind }) {
  const meta = KIND_META[kind];
  const { usuario } = useAuth();
  const puedeEditar = canWriteOperativa(usuario);
  const loadAsignaciones = useCallback(() => asignacionService.getAll(), []);
  const { rows, isLoading, errorMessage, banner, setBanner, reload } = useApiCollection(loadAsignaciones);
  const [lookups, setLookups] = useState({
    activos: [],
    responsables: [],
    ubicaciones: [],
    tipos: [],
    areas: [],
    proveedores: [],
  });
  const [lookupsLoaded, setLookupsLoaded] = useState(false);
  const [formOpen, setFormOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [actionRow, setActionRow] = useState(null);
  const [confirming, setConfirming] = useState(false);
  const [trabajoRealizado, setTrabajoRealizado] = useState('');

  useEffect(() => {
    let cancelled = false;

    function loadLookups() {
      Promise.all([
        activoService.getAll(),
        responsableService.getAll(),
        ubicacionService.getAll(),
        tipoAsignacionService.getAll(),
        areaService.getAll(),
        proveedorService.getAll(),
      ])
        .then(([activos, responsables, ubicaciones, tipos, areas, proveedores]) => {
          if (!cancelled) {
            setLookups({ activos, responsables, ubicaciones, tipos, areas, proveedores });
            setLookupsLoaded(true);
          }
        })
        .catch((error) => {
          if (!cancelled) setBanner({ variant: 'error', message: getErrorMessage(error) });
        });
    }

    loadLookups();
    const unsub = subscribeEmpresaActiva(() => loadLookups());
    return () => {
      cancelled = true;
      unsub();
    };
  }, [setBanner]);

  const tipoById = new Map(lookups.tipos.map((item) => [item.id, item.nombre]));
  const activoById = new Map(lookups.activos.map((item) => [item.id, item.nombre]));
  const responsableById = new Map(lookups.responsables.map((item) => [item.id, item.nombreCompleto]));

  const tableRows = rows
    .filter((row) => !lookupsLoaded || esTipo(tipoById.get(row.idTipoAsignacion), meta.tipoNombre))
    .map((row) => ({
      ...row,
      activoNombre: activoById.get(row.idActivo) ?? row.idActivo,
      responsableNombre: responsableById.get(row.idResponsable) ?? row.idResponsable,
      tipoNombre: tipoById.get(row.idTipoAsignacion) ?? row.idTipoAsignacion,
      fechaFmt: formatDate(row.fechaAsignacion),
      estadoLabel: row.activa ? 'Activa' : 'Cerrada',
    }));

  async function handleSave(values) {
    setSaving(true);
    try {
      await meta.submit(buildMovimientoPayload(kind, values, usuario, lookups.tipos, toOptions(lookups.proveedores)));
      setBanner({ variant: 'success', message: meta.success });
      setFormOpen(false);
      const activos = await activoService.getAll();
      setLookups((prev) => ({ ...prev, activos }));
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setSaving(false);
    }
  }

  async function handleConfirm() {
    if (!actionRow) return;
    setConfirming(true);
    try {
      if (kind === 'mantenimiento') {
        if (!String(trabajoRealizado ?? '').trim()) {
          setBanner({ variant: 'error', message: 'Indique el trabajo realizado antes de finalizar.' });
          return;
        }
        await asignacionService.finalizarMantenimiento(actionRow.id, {
          id: actionRow.id,
          observaciones: trabajoRealizado.trim(),
        });
        setBanner({ variant: 'success', message: 'El mantenimiento se finalizó.' });
        setTrabajoRealizado('');
      } else {
        await asignacionService.devolver(actionRow.id, { id: actionRow.id });
        setBanner({ variant: 'success', message: 'El activo se devolvió.' });
      }
      setActionRow(null);
      const activos = await activoService.getAll();
      setLookups((prev) => ({ ...prev, activos }));
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setConfirming(false);
    }
  }

  async function handlePdf(row) {
    try {
      await asignacionService.downloadPdf(row.id);
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    }
  }

  const needsPdf = kind === 'asignacion' || kind === 'baja';
  const needsClose = kind === 'asignacion' || kind === 'mantenimiento';

  const columns = [
    { key: 'activoNombre', header: 'Activo' },
    { key: 'responsableNombre', header: 'Responsable' },
    { key: 'fechaFmt', header: 'Fecha' },
    {
      key: 'estadoLabel',
      header: 'Estado',
      render: (row) => <Badge variant={row.activa ? 'success' : 'ghost'}>{row.estadoLabel}</Badge>,
    },
    {
      key: 'acciones',
      header: '',
      align: 'right',
      sortable: false,
      render: (row) => (
        <CatalogRowActions
          extra={
            <>
              {needsPdf ? (
                <Button variant="secondary" onClick={() => handlePdf(row)}>
                  Descargar PDF
                </Button>
              ) : null}
              {puedeEditar && needsClose && row.activa ? (
                <Button variant="secondary" onClick={() => setActionRow(row)}>
                  {kind === 'mantenimiento' ? 'Finalizar' : 'Devolver'}
                </Button>
              ) : null}
            </>
          }
        />
      ),
    },
  ];

  return (
    <section className="space-y-6">
      <PageHeader
        title={meta.title}
        description={meta.description}
        actions={
          puedeEditar ? (
            <Button type="button" onClick={() => setFormOpen(true)}>
              {meta.actionLabel}
            </Button>
          ) : null
        }
      />
      {banner ? (
        <AlertBanner variant={banner.variant} message={banner.message} onDismiss={() => setBanner(null)} />
      ) : null}
      <DataTable
        columns={columns}
        rows={tableRows}
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage={`No hay ${meta.title.toLowerCase()} para mostrar.`}
        searchPlaceholder="Buscar..."
      />
      <Modal
        isOpen={formOpen}
        onClose={() => (saving ? null : setFormOpen(false))}
        title={meta.actionLabel}
        wide={kind === 'asignacion' || kind === 'baja'}
      >
        <MovimientoForm
          kind={kind}
          initialValues={emptyMovimientoForm()}
          activoOptions={toOptions(activosParaMovimiento(lookups.activos, kind))}
          responsableOptions={toOptions(lookups.responsables, 'nombreCompleto')}
          ubicacionOptions={toOptions(lookups.ubicaciones)}
          areaOptions={toOptions(lookups.areas)}
          responsables={lookups.responsables}
          activos={lookups.activos}
          proveedorOptions={toOptions(lookups.proveedores)}
          onSubmit={handleSave}
          onCancel={() => setFormOpen(false)}
          isSubmitting={saving}
        />
      </Modal>
      {kind === 'mantenimiento' ? (
        <Modal
          isOpen={Boolean(actionRow)}
          onClose={() => (confirming ? null : setActionRow(null))}
          title="Finalizar mantenimiento"
        >
          <form
            className="space-y-4"
            onSubmit={(event) => {
              event.preventDefault();
              handleConfirm();
            }}
          >
            <p className="text-sm text-slate-600">
              Cierre del mantenimiento de &quot;{actionRow?.activoNombre}&quot;. El estado del activo vuelve al anterior.
            </p>
            <TextareaField
              label="Trabajo realizado"
              name="trabajoRealizado"
              value={trabajoRealizado}
              onChange={(event) => setTrabajoRealizado(event.target.value)}
              required
            />
            <FormActions
              onCancel={() => setActionRow(null)}
              isSubmitting={confirming}
              submitLabel="Finalizar"
            />
          </form>
        </Modal>
      ) : (
        <ConfirmDialog
          isOpen={Boolean(actionRow)}
          onClose={() => (confirming ? null : setActionRow(null))}
          onConfirm={handleConfirm}
          title="Devolver activo"
          message={`¿Registrar la devolución de "${actionRow?.activoNombre}"?`}
          confirmLabel="Devolver"
          isConfirming={confirming}
        />
      )}
    </section>
  );
}

export function AsignacionesPage() {
  return <MovimientosPage kind="asignacion" />;
}

export function TrasladosPage() {
  return <MovimientosPage kind="traslado" />;
}

export function MantenimientosPage() {
  return <MovimientosPage kind="mantenimiento" />;
}

export function BajasPage() {
  return <MovimientosPage kind="baja" />;
}
