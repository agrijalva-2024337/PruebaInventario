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
import { canWriteEmpresa } from '@/features/auth/permissions';
import * as paisService from '@/features/catalogos/paises/paisService';
import * as empresaService from '@/features/organizacion/empresas/empresaService';
import { SedeForm } from '@/features/organizacion/sedes/SedeForm';
import * as sedeService from '@/features/organizacion/sedes/sedeService';

const EMPTY_SEDE = {
  idEmpresa: '',
  idPais: '',
  nombre: '',
  direccion: '',
  ciudad: '',
};

function toSedeFormValues(row) {
  return {
    idEmpresa: row.idEmpresa ?? '',
    idPais: row.idPais ?? '',
    nombre: row.nombre ?? '',
    direccion: row.direccion ?? '',
    ciudad: row.ciudad ?? '',
  };
}

function toOptions(items) {
  return items
    .filter((item) => item.habilitado !== false)
    .map((item) => ({ value: item.id, label: item.nombre }));
}

export function SedesPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteEmpresa(usuario);
  const { visibleRows, isLoading, errorMessage, filter, setFilter, banner, setBanner, reload } =
    useCatalogCollection(sedeService.getAll);
  const [empresas, setEmpresas] = useState([]);
  const [paises, setPaises] = useState([]);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirmRow, setConfirmRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadLookups() {
      try {
        const [empresaRows, paisRows] = await Promise.all([
          empresaService.getAll(),
          paisService.getAll(),
        ]);

        if (!cancelled) {
          setEmpresas(empresaRows);
          setPaises(paisRows);
        }
      } catch (error) {
        if (!cancelled) {
          setBanner({ variant: 'error', message: getErrorMessage(error) });
        }
      }
    }

    loadLookups();
    return () => {
      cancelled = true;
    };
  }, [setBanner]);

  const empresaById = new Map(empresas.map((item) => [item.id, item.nombre]));
  const paisById = new Map(paises.map((item) => [item.id, item.nombre]));
  const tableRows = visibleRows.map((row) => ({
    ...row,
    empresaNombre: empresaById.get(row.idEmpresa) ?? row.idEmpresa,
    paisNombre: paisById.get(row.idPais) ?? row.idPais,
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
        ...values,
        idEmpresa: Number(values.idEmpresa),
        idPais: Number(values.idPais),
      };

      if (editing) {
        await sedeService.update(editing.id, { ...editing, ...payload });
        setBanner({ variant: 'success', message: 'La sede se actualizó correctamente.' });
      } else {
        await sedeService.create({ ...payload, habilitado: true });
        setBanner({ variant: 'success', message: 'La sede se creó correctamente.' });
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
      if (confirmRow.habilitado) {
        await sedeService.remove(confirmRow.id);
        setBanner({ variant: 'success', message: 'La sede se inactivó correctamente.' });
      } else {
        await sedeService.update(confirmRow.id, { ...confirmRow, habilitado: true });
        setBanner({ variant: 'success', message: 'La sede se reactivó correctamente.' });
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
    { key: 'nombre', header: 'Nombre' },
    { key: 'empresaNombre', header: 'Empresa' },
    { key: 'paisNombre', header: 'País' },
    { key: 'ciudad', header: 'Ciudad' },
    { key: 'direccion', header: 'Dirección' },
    {
      key: 'habilitado',
      header: 'Estado',
      render: (row) => (
        <Badge variant={row.habilitado ? 'success' : 'ghost'}>
          {row.habilitado ? 'Activo' : 'Inactivo'}
        </Badge>
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
                onEdit={openEdit}
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
        title="Sedes"
        description="Sucursales y centros de operación asociados a una empresa y un país."
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

      <HabilitadoFilter value={filter} onChange={setFilter} />

      <DataTable
        columns={columns}
        rows={tableRows}
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage="No hay sedes para mostrar."
        searchPlaceholder="Buscar sedes..."
        rowClassName={(row) => (row.habilitado ? '' : 'bg-slate-50 opacity-70')}
      />

      <Modal isOpen={formOpen} onClose={closeForm} title={editing ? 'Editar sede' : 'Nueva sede'}>
        <SedeForm
          key={editing ? `edit-${editing.id}` : 'new'}
          initialValues={editing ? toSedeFormValues(editing) : EMPTY_SEDE}
          empresaOptions={toOptions(empresas)}
          paisOptions={paises.map((item) => ({ value: item.id, label: item.nombre }))}
          onSubmit={handleSave}
          onCancel={closeForm}
          isSubmitting={saving}
        />
      </Modal>

      <ConfirmDialog
        isOpen={Boolean(confirmRow)}
        onClose={() => (confirming ? null : setConfirmRow(null))}
        onConfirm={handleConfirm}
        title={confirmRow?.habilitado ? 'Inactivar sede' : 'Reactivar sede'}
        message={
          confirmRow?.habilitado
            ? `¿Inactivar "${confirmRow?.nombre}"? El registro seguirá visible como inactivo.`
            : `¿Reactivar "${confirmRow?.nombre}"?`
        }
        confirmLabel={confirmRow?.habilitado ? 'Inactivar' : 'Reactivar'}
        variant={confirmRow?.habilitado ? 'danger' : 'primary'}
        isConfirming={confirming}
      />
    </section>
  );
}
