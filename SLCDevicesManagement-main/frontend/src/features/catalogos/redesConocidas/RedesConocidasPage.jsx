import { useEffect, useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Button } from '@/shared/components/Button';
import { CatalogRowActions } from '@/shared/components/CatalogRowActions';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable } from '@/shared/components/DataTable';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import { RedConocidaForm } from '@/features/catalogos/redesConocidas/RedConocidaForm';
import * as redConocidaService from '@/features/catalogos/redesConocidas/redConocidaService';
import * as ubicacionService from '@/features/catalogos/ubicaciones/ubicacionService';

const EMPTY_RED = { bssid: '', idUbicacion: '' };

export function RedesConocidasPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteOperativa(usuario);
  const [rows, setRows] = useState([]);
  const [ubicaciones, setUbicaciones] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState(null);
  const [banner, setBanner] = useState(null);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirmRow, setConfirmRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  async function reload() {
    setIsLoading(true);
    try {
      const [redes, ubicacionRows] = await Promise.all([
        redConocidaService.getAll(),
        ubicacionService.getAll(),
      ]);
      setRows(redes);
      setUbicaciones(ubicacionRows);
      setErrorMessage(null);
    } catch (error) {
      setErrorMessage(getErrorMessage(error));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    reload();
  }, []);

  const ubicacionById = new Map(ubicaciones.map((item) => [item.id, item.nombre]));
  const tableRows = rows.map((row) => ({
    ...row,
    ubicacionNombre: ubicacionById.get(row.idUbicacion) ?? row.idUbicacion,
  }));

  function openCreate() {
    setEditing(null);
    setFormOpen(true);
  }

  function openEdit(row) {
    setEditing(row);
    setFormOpen(true);
  }

  function closeForm() {
    if (!saving) {
      setFormOpen(false);
      setEditing(null);
    }
  }

  async function handleSave(values) {
    setSaving(true);

    try {
      const payload = {
        bssid: values.bssid.trim().toLowerCase(),
        idUbicacion: Number(values.idUbicacion),
      };

      if (editing) {
        await redConocidaService.update(editing.id, { id: editing.id, ...payload });
        setBanner({ variant: 'success', message: 'La red conocida se actualizó correctamente.' });
      } else {
        await redConocidaService.create(payload);
        setBanner({ variant: 'success', message: 'La red conocida se creó correctamente.' });
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
    if (!confirmRow) {
      return;
    }

    setConfirming(true);

    try {
      await redConocidaService.remove(confirmRow.id);
      setBanner({ variant: 'success', message: 'La red conocida se eliminó correctamente.' });
      setConfirmRow(null);
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setConfirming(false);
    }
  }

  const columns = [
    { key: 'bssid', header: 'BSSID' },
    { key: 'ubicacionNombre', header: 'Ubicación' },
    ...(puedeEditar
      ? [
          {
            key: 'acciones',
            header: '',
            align: 'right',
            sortable: false,
            render: (row) => (
              <CatalogRowActions row={row} onEdit={openEdit} onDelete={setConfirmRow} />
            ),
          },
        ]
      : []),
  ];

  return (
    <section className="space-y-6">
      <PageHeader
        title="Redes conocidas"
        description="Puntos de acceso Wi-Fi (BSSID) asociados a una ubicación. El agente de rastreo usa este catálogo para inferir dónde está cada equipo."
        actions={
          puedeEditar ? (
            <Button onClick={openCreate} type="button">
              Nueva
            </Button>
          ) : null
        }
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
        rows={tableRows}
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage="No hay redes conocidas. Catalogá los BSSID de cada sede antes de activar el rastreo."
        searchPlaceholder="Buscar por BSSID..."
      />

      <Modal
        isOpen={formOpen}
        onClose={closeForm}
        title={editing ? 'Editar red conocida' : 'Nueva red conocida'}
      >
        <RedConocidaForm
          key={editing ? `edit-${editing.id}` : 'new'}
          initialValues={
            editing
              ? { bssid: editing.bssid ?? '', idUbicacion: editing.idUbicacion ?? '' }
              : EMPTY_RED
          }
          ubicacionOptions={ubicaciones
            .filter((item) => item.habilitado !== false)
            .map((item) => ({ value: item.id, label: item.nombre }))}
          onSubmit={handleSave}
          onCancel={closeForm}
          isSubmitting={saving}
        />
      </Modal>

      <ConfirmDialog
        isOpen={Boolean(confirmRow)}
        onClose={() => (confirming ? null : setConfirmRow(null))}
        onConfirm={handleConfirm}
        title="Eliminar red conocida"
        message={`¿Eliminar el BSSID "${confirmRow?.bssid}"? Los equipos en esa red quedarán como ubicación desconocida.`}
        confirmLabel="Eliminar"
        variant="danger"
        isConfirming={confirming}
      />
    </section>
  );
}
