import { useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Button } from '@/shared/components/Button';
import { CatalogRowActions } from '@/shared/components/CatalogRowActions';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable } from '@/shared/components/DataTable';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { FormActions } from '@/shared/components/FormActions';
import { TextField } from '@/shared/components/TextField';
import { TextareaField } from '@/shared/components/TextareaField';
import { useApiCollection } from '@/shared/hooks/useApiCollection';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { useAuth } from '@/features/auth/AuthContext';
import { isAdministradorGeneral } from '@/features/auth/permissions';
import httpClient from '@/shared/services/httpClient';
import { env } from '@/shared/config/env';
import * as estadoService from '@/features/organizacion/estados/estadoService';
import * as tipoAsignacionService from '@/features/organizacion/tiposAsignacion/tipoAsignacionService';
import { apiPaths } from '@/shared/api/paths';

function NamedForm({ initialValues, onSubmit, onCancel, isSubmitting, maxNombre }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: (next) => {
      const result = {};
      enforceRequired(result, next, 'nombre', 'nombre');
      enforceMaxLength(result, next, 'nombre', 'nombre', maxNombre);
      enforceMaxLength(result, next, 'descripcion', 'descripcion', 150);
      return result;
    },
  });

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <TextField
        label="Nombre"
        name="nombre"
        value={values.nombre}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.nombre ? errors.nombre : undefined}
        required
      />
      <TextareaField
        label="Descripción"
        name="descripcion"
        value={values.descripcion}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.descripcion ? errors.descripcion : undefined}
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}

function NamedCatalogPage({ title, description, service, endpoint, hint }) {
  const { usuario } = useAuth();
  const puedeEditar = isAdministradorGeneral(usuario);
  const { rows, isLoading, errorMessage, banner, setBanner, reload } = useApiCollection(service.getAll);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirmRow, setConfirmRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  async function handleSave(values) {
    setSaving(true);
    try {
      if (editing) {
        await service.update(editing.id, { id: editing.id, ...values });
        setBanner({ variant: 'success', message: 'El registro se actualizó correctamente.' });
      } else {
        await service.create(values);
        setBanner({ variant: 'success', message: 'El registro se creó correctamente.' });
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

  async function handleDelete() {
    if (!confirmRow) return;
    setConfirming(true);
    try {
      if (env.useApiMock) {
        await service.remove(confirmRow.id);
      } else {
        await httpClient.delete(`${endpoint}/${confirmRow.id}`);
      }
      setBanner({ variant: 'success', message: 'El registro se eliminó.' });
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
    { key: 'descripcion', header: 'Descripción' },
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
                onDelete={setConfirmRow}
              />
            ),
          },
        ]
      : []),
  ];

  return (
    <section className="space-y-6">
      <PageHeader
        title={title}
        description={description}
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
      {hint ? <p className="text-sm text-slate-500">{hint}</p> : null}
      {banner ? (
        <AlertBanner variant={banner.variant} message={banner.message} onDismiss={() => setBanner(null)} />
      ) : null}
      <DataTable
        columns={columns}
        rows={rows}
        isLoading={isLoading}
        errorMessage={errorMessage}
        emptyMessage="No hay registros."
        searchPlaceholder="Buscar..."
      />
      <Modal isOpen={formOpen} onClose={() => (saving ? null : setFormOpen(false))} title={editing ? 'Editar' : 'Nuevo'}>
        <NamedForm
          key={editing ? `edit-${editing.id}` : 'new'}
          initialValues={{
            nombre: editing?.nombre ?? '',
            descripcion: editing?.descripcion ?? '',
          }}
          maxNombre={50}
          onSubmit={handleSave}
          onCancel={() => setFormOpen(false)}
          isSubmitting={saving}
        />
      </Modal>
      <ConfirmDialog
        isOpen={Boolean(confirmRow)}
        onClose={() => (confirming ? null : setConfirmRow(null))}
        onConfirm={handleDelete}
        title="Eliminar"
        message={`¿Eliminar "${confirmRow?.nombre}"?`}
        confirmLabel="Eliminar"
        variant="danger"
        isConfirming={confirming}
      />
    </section>
  );
}

export function EstadosPage() {
  return (
    <NamedCatalogPage
      title="Estados"
      description="Estados operativos usados en asignaciones, traslados y mantenimientos."
      service={estadoService}
      endpoint={apiPaths.estados}
    />
  );
}

export function TiposAsignacionPage() {
  return (
    <NamedCatalogPage
      title="Tipos de asignación"
      description="Catálogo canónico. Deben existir Asignacion, Traslado, Mantenimiento y Baja."
      hint="Si falta alguno, las operaciones devolverán 409. Nombres: Asignacion, Traslado, Mantenimiento, Baja."
      service={tipoAsignacionService}
      endpoint={apiPaths.tiposAsignacion}
    />
  );
}
