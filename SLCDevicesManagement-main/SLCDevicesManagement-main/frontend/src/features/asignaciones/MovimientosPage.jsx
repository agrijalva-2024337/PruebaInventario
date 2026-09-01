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
import { formatDate, toIsoDate, todayInputValue } from '@/shared/utils/dates';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import * as activoService from '@/features/activos/activoService';
import { MovimientoForm } from '@/features/asignaciones/MovimientoForm';
import * as asignacionService from '@/features/asignaciones/asignacionService';
import * as estadoService from '@/features/organizacion/estados/estadoService';
import * as responsableService from '@/features/organizacion/responsables/responsableService';
import * as tipoAsignacionService from '@/features/organizacion/tiposAsignacion/tipoAsignacionService';
import * as ubicacionService from '@/features/catalogos/ubicaciones/ubicacionService';

const KIND_META = {
  asignacion: {
    title: 'Asignaciones',
    description: 'Entrega de un activo a un responsable. Ocupa el bien hasta la devolución.',
    tipoNombre: 'Asignacion',
    actionLabel: 'Nueva entrega',
    submit: asignacionService.entregar,
    success: 'La entrega se registró correctamente.',
  },
  traslado: {
    title: 'Traslados',
    description: 'Cambio de ubicación. No ocupa el activo.',
    tipoNombre: 'Traslado',
    actionLabel: 'Nuevo traslado',
    submit: asignacionService.trasladar,
    success: 'El traslado se registró correctamente.',
  },
  mantenimiento: {
    title: 'Mantenimientos',
    description: 'Envío a mantenimiento. Ocupa el activo hasta finalizar.',
    tipoNombre: 'Mantenimiento',
    actionLabel: 'Iniciar mantenimiento',
    submit: asignacionService.iniciarMantenimiento,
    success: 'El mantenimiento se inició correctamente.',
  },
  baja: {
    title: 'Bajas',
    description: 'Baja operativa: el activo no se borra, queda fuera de circulación.',
    tipoNombre: 'Baja',
    actionLabel: 'Registrar baja',
    submit: asignacionService.darDeBaja,
    success: 'La baja se registró correctamente.',
  },
};

function normalizarTipo(nombre) {
  return String(nombre ?? '')
    .trim()
    .replaceAll('ó', 'o')
    .replaceAll('Ó', 'o');
}

function esTipo(nombre, esperado) {
  return normalizarTipo(nombre).toLowerCase() === normalizarTipo(esperado).toLowerCase();
}

function toOptions(items, labelKey = 'nombre') {
  return items
    .filter((item) => item.habilitado !== false)
    .map((item) => ({ value: item.id, label: item[labelKey] ?? `#${item.id}` }));
}

function emptyForm() {
  return {
    idActivo: '',
    idResponsable: '',
    idUbicacion: '',
    idEstado: '',
    fechaAsignacion: todayInputValue(),
    observaciones: '',
    motivo: '',
    documentoPdfUrl: '',
  };
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
    estados: [],
    tipos: [],
  });
  const [lookupsLoaded, setLookupsLoaded] = useState(false);
  const [formOpen, setFormOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [actionRow, setActionRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  useEffect(() => {
    let cancelled = false;
    Promise.all([
      activoService.getAll(),
      responsableService.getAll(),
      ubicacionService.getAll(),
      estadoService.getAll(),
      tipoAsignacionService.getAll(),
    ])
      .then(([activos, responsables, ubicaciones, estados, tipos]) => {
        if (!cancelled) {
          setLookups({ activos, responsables, ubicaciones, estados, tipos });
          setLookupsLoaded(true);
        }
      })
      .catch((error) => {
        if (!cancelled) setBanner({ variant: 'error', message: getErrorMessage(error) });
      });
    return () => {
      cancelled = true;
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
      const base = {
        idActivo: Number(values.idActivo),
        idUsuario: usuario.id,
        idResponsable: Number(values.idResponsable),
        idEstado: Number(values.idEstado),
        fechaAsignacion: toIsoDate(values.fechaAsignacion),
      };

      if (kind === 'baja') {
        await meta.submit({
          ...base,
          motivo: values.motivo,
          documentoPdfUrl: values.documentoPdfUrl,
        });
      } else if (kind === 'asignacion') {
        const tipo = lookups.tipos.find((item) => esTipo(item.nombre, 'Asignacion'));
        if (!tipo) {
          throw new Error('No existe el tipo Asignacion. Créalo en Catálogos → Tipos de asignación.');
        }

        await meta.submit({
          ...base,
          idUbicacion: Number(values.idUbicacion),
          idTipoAsignacion: tipo.id,
          observaciones: values.observaciones || null,
        });
      } else {
        await meta.submit({
          ...base,
          idUbicacion: Number(values.idUbicacion),
          observaciones: values.observaciones || null,
        });
      }

      setBanner({ variant: 'success', message: meta.success });
      setFormOpen(false);
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
        await asignacionService.finalizarMantenimiento(actionRow.id, { id: actionRow.id });
        setBanner({ variant: 'success', message: 'El mantenimiento se finalizó.' });
      } else {
        await asignacionService.devolver(actionRow.id, { id: actionRow.id });
        setBanner({ variant: 'success', message: 'El activo se devolvió.' });
      }
      setActionRow(null);
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setConfirming(false);
    }
  }

  const columns = [
    { key: 'activoNombre', header: 'Activo' },
    { key: 'responsableNombre', header: 'Responsable' },
    { key: 'fechaFmt', header: 'Fecha' },
    {
      key: 'estadoLabel',
      header: 'Estado',
      render: (row) => <Badge variant={row.activa ? 'success' : 'ghost'}>{row.estadoLabel}</Badge>,
    },
    ...(puedeEditar && (kind === 'asignacion' || kind === 'mantenimiento')
      ? [
          {
            key: 'acciones',
            header: '',
            align: 'right',
            sortable: false,
            render: (row) =>
              row.activa ? (
                <CatalogRowActions
                  extra={
                    <Button variant="secondary" onClick={() => setActionRow(row)}>
                      {kind === 'mantenimiento' ? 'Finalizar' : 'Devolver'}
                    </Button>
                  }
                />
              ) : null,
          },
        ]
      : []),
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
      <Modal isOpen={formOpen} onClose={() => (saving ? null : setFormOpen(false))} title={meta.actionLabel}>
        <MovimientoForm
          kind={kind}
          initialValues={emptyForm()}
          activoOptions={toOptions(lookups.activos)}
          responsableOptions={toOptions(lookups.responsables, 'nombreCompleto')}
          ubicacionOptions={toOptions(lookups.ubicaciones)}
          estadoOptions={toOptions(lookups.estados)}
          onSubmit={handleSave}
          onCancel={() => setFormOpen(false)}
          isSubmitting={saving}
        />
      </Modal>
      <ConfirmDialog
        isOpen={Boolean(actionRow)}
        onClose={() => (confirming ? null : setActionRow(null))}
        onConfirm={handleConfirm}
        title={kind === 'mantenimiento' ? 'Finalizar mantenimiento' : 'Devolver activo'}
        message={
          kind === 'mantenimiento'
            ? `¿Cerrar el mantenimiento del activo "${actionRow?.activoNombre}"?`
            : `¿Registrar la devolución de "${actionRow?.activoNombre}"?`
        }
        confirmLabel={kind === 'mantenimiento' ? 'Finalizar' : 'Devolver'}
        isConfirming={confirming}
      />
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
