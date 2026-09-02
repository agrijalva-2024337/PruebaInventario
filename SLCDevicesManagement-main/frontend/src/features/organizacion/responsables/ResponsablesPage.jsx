import { useEffect, useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { Button } from '@/shared/components/Button';
import { CatalogRowActions } from '@/shared/components/CatalogRowActions';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable } from '@/shared/components/DataTable';
import { HabilitadoFilter } from '@/shared/components/HabilitadoFilter';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { useCatalogCollection } from '@/shared/hooks/useCatalogCollection';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import { ResponsableForm } from '@/features/organizacion/responsables/ResponsableForm';
import * as responsableService from '@/features/organizacion/responsables/responsableService';
import * as areaService from '@/features/organizacion/areas/areaService';

const EMPTY = {
  idArea: '',
  nombreCompleto: '',
  cargo: '',
  correo: '',
  telefono: '',
};

export function ResponsablesPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteOperativa(usuario);
  const { visibleRows, isLoading, errorMessage, filter, setFilter, banner, setBanner, reload } =
    useCatalogCollection(responsableService.getAll);
  const [areas, setAreas] = useState([]);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirmRow, setConfirmRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  useEffect(() => {
    let cancelled = false;
    areaService
      .getAll()
      .then((rows) => {
        if (!cancelled) setAreas(rows);
      })
      .catch((error) => {
        if (!cancelled) setBanner({ variant: 'error', message: getErrorMessage(error) });
      });
    return () => {
      cancelled = true;
    };
  }, [setBanner]);

  const areaById = new Map(areas.map((item) => [item.id, item.nombre]));
  const tableRows = visibleRows.map((row) => ({
    ...row,
    areaNombre: areaById.get(row.idArea) ?? row.idArea,
  }));

  async function handleSave(values) {
    setSaving(true);
    try {
      const payload = {
        idArea: Number(values.idArea),
        nombreCompleto: values.nombreCompleto,
        cargo: values.cargo || null,
        correo: values.correo || null,
        telefono: values.telefono || null,
      };
      if (editing) {
        await responsableService.update(editing.id, { ...editing, ...payload });
        setBanner({ variant: 'success', message: 'El responsable se actualizó correctamente.' });
      } else {
        await responsableService.create({ ...payload, habilitado: true });
        setBanner({ variant: 'success', message: 'El responsable se creó correctamente.' });
      }
      setFormOpen(false);
      setEditing(null);
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setSaving(false);
    }
  }

  async function handleConfirm() {
    if (!confirmRow) return;
    setConfirming(true);
    try {
      if (confirmRow.habilitado) {
        await responsableService.remove(confirmRow.id);
        setBanner({ variant: 'success', message: 'El responsable se inactivó correctamente.' });
      } else {
        await responsableService.update(confirmRow.id, { ...confirmRow, habilitado: true });
        setBanner({ variant: 'success', message: 'El responsable se reactivó correctamente.' });
      }
      setConfirmRow(null);
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setConfirming(false);
    }
  }

  const columns = [
    { key: 'nombreCompleto', header: 'Nombre' },
    { key: 'cargo', header: 'Cargo' },
    { key: 'areaNombre', header: 'Área' },
    { key: 'correo', header: 'Correo' },
    {
      key: 'habilitado',
      header: 'Estado',
      render: (row) => (
        <Badge variant={row.habilitado ? 'success' : 'ghost'}>{row.habilitado ? 'Activo' : 'Inactivo'}</Badge>
      ),
    },
    ...(puedeEditar
      ? [
          {
            key: 'acciones',
            header: '',
            align: 'right',
            sortable: false,
            render: (row) => (
              <CatalogRowActions
                row={row}
                onEdit={() => {
                  setEditing(row);
                  setFormOpen(true);
                }}
                onInactivate={setConfirmRow}
                onReactivate={setConfirmRow}
              />
            ),
          },
        ]
      : []),
  ];

  return (
    <section className="space-y-6">
      <PageHeader
        title="Responsables"
        description="Personas que reciben o autorizan movimientos de activos."
        actions={
          puedeEditar ? (
            <Button
              type="button"
              onClick={() => {
                setEditing(null);
                setFormOpen(true);
              }}
            >
              Nuevo
            </Button>
          ) : null
        }
      />
      {banner ? (
        <AlertBanner variant={banner.variant} message={banner.message} onDismiss={() => setBanner(null)} />
      ) : null}
      <HabilitadoFilter value={filter} onChange={setFilter} />
      <DataTable
        columns={columns}
        rows={tableRows}
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage="No hay responsables para mostrar."
        searchPlaceholder="Buscar responsables..."
        rowClassName={(row) => (row.habilitado ? '' : 'bg-slate-50 opacity-70')}
      />
      <Modal
        isOpen={formOpen}
        onClose={() => (saving ? null : setFormOpen(false))}
        title={editing ? 'Editar responsable' : 'Nuevo responsable'}
      >
        <ResponsableForm
          key={editing ? `edit-${editing.id}` : 'new'}
          initialValues={
            editing
              ? {
                  idArea: editing.idArea ?? '',
                  nombreCompleto: editing.nombreCompleto ?? '',
                  cargo: editing.cargo ?? '',
                  correo: editing.correo ?? '',
                  telefono: editing.telefono ?? '',
                }
              : EMPTY
          }
          areaOptions={areas
            .filter((item) => item.habilitado !== false)
            .map((item) => ({ value: item.id, label: item.nombre }))}
          onSubmit={handleSave}
          onCancel={() => setFormOpen(false)}
          isSubmitting={saving}
        />
      </Modal>
      <ConfirmDialog
        isOpen={Boolean(confirmRow)}
        onClose={() => (confirming ? null : setConfirmRow(null))}
        onConfirm={handleConfirm}
        title={confirmRow?.habilitado ? 'Inactivar responsable' : 'Reactivar responsable'}
        message={
          confirmRow?.habilitado
            ? `¿Inactivar "${confirmRow?.nombreCompleto}"?`
            : `¿Reactivar "${confirmRow?.nombreCompleto}"?`
        }
        confirmLabel={confirmRow?.habilitado ? 'Inactivar' : 'Reactivar'}
        variant={confirmRow?.habilitado ? 'danger' : 'primary'}
        isConfirming={confirming}
      />
    </section>
  );
}
