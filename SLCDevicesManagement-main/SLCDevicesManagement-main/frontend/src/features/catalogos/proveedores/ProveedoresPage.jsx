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
import { ProveedorForm } from '@/features/catalogos/proveedores/ProveedorForm';
import * as proveedorService from '@/features/catalogos/proveedores/proveedorService';
import * as empresaService from '@/features/organizacion/empresas/empresaService';

const EMPTY_PROVEEDOR = {
  idEmpresa: '',
  nombre: '',
  nit: '',
  nombreContacto: '',
  telefono: '',
  correo: '',
};

function toProveedorFormValues(row) {
  return {
    idEmpresa: row.idEmpresa ?? '',
    nombre: row.nombre ?? '',
    nit: row.nit ?? '',
    nombreContacto: row.nombreContacto ?? '',
    telefono: row.telefono ?? '',
    correo: row.correo ?? '',
  };
}

export function ProveedoresPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteEmpresa(usuario);
  const { visibleRows, isLoading, errorMessage, filter, setFilter, banner, setBanner, reload } =
    useCatalogCollection(proveedorService.getAll);
  const [empresas, setEmpresas] = useState([]);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirmRow, setConfirmRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadLookups() {
      try {
        const empresaRows = await empresaService.getAll();
        if (!cancelled) {
          setEmpresas(empresaRows);
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
  const tableRows = visibleRows.map((row) => ({
    ...row,
    empresaNombre: empresaById.get(row.idEmpresa) ?? row.idEmpresa,
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
      const payload = { ...values, idEmpresa: Number(values.idEmpresa) };

      if (editing) {
        await proveedorService.update(editing.id, { ...editing, ...payload });
        setBanner({ variant: 'success', message: 'El proveedor se actualizó correctamente.' });
      } else {
        await proveedorService.create({ ...payload, habilitado: true });
        setBanner({ variant: 'success', message: 'El proveedor se creó correctamente.' });
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
        await proveedorService.remove(confirmRow.id);
        setBanner({ variant: 'success', message: 'El proveedor se inactivó correctamente.' });
      } else {
        await proveedorService.update(confirmRow.id, { ...confirmRow, habilitado: true });
        setBanner({ variant: 'success', message: 'El proveedor se reactivó correctamente.' });
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
    { key: 'nit', header: 'NIT' },
    { key: 'nombreContacto', header: 'Contacto' },
    { key: 'telefono', header: 'Teléfono' },
    { key: 'correo', header: 'Correo' },
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
        title="Proveedores"
        description="Proveedores de compra y mantenimiento asociados a una empresa."
        actions={
          puedeEditar ? (
            <Button onClick={openCreate} type="button">
              Nuevo
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
        emptyMessage="No hay proveedores para mostrar."
        searchPlaceholder="Buscar proveedores..."
        rowClassName={(row) => (row.habilitado ? '' : 'bg-slate-50 opacity-70')}
      />

      <Modal
        isOpen={formOpen}
        onClose={closeForm}
        title={editing ? 'Editar proveedor' : 'Nuevo proveedor'}
      >
        <ProveedorForm
          key={editing ? `edit-${editing.id}` : 'new'}
          initialValues={editing ? toProveedorFormValues(editing) : EMPTY_PROVEEDOR}
          empresaOptions={empresas
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
        title={confirmRow?.habilitado ? 'Inactivar proveedor' : 'Reactivar proveedor'}
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
