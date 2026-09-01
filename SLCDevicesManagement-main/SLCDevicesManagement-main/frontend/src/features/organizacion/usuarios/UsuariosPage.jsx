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
import { canWriteEmpresa, isAdministradorGeneral } from '@/features/auth/permissions';
import { RolUsuario, rolUsuarioLabel } from '@/shared/api/contracts';
import * as empresaService from '@/features/organizacion/empresas/empresaService';
import { UsuarioForm } from '@/features/organizacion/usuarios/UsuarioForm';
import * as usuarioService from '@/features/organizacion/usuarios/usuarioService';

const EMPTY_USUARIO = {
  idEmpresa: '',
  nombres: '',
  apellidos: '',
  correo: '',
  username: '',
  password: '',
  rol: RolUsuario.Consulta,
};

function toUsuarioFormValues(row) {
  return {
    idEmpresa: row.idEmpresa ?? '',
    nombres: row.nombres ?? '',
    apellidos: row.apellidos ?? '',
    correo: row.correo ?? '',
    username: row.username ?? '',
    password: '',
    rol: row.rol ?? RolUsuario.Consulta,
  };
}

function toCreatePayload(values) {
  return {
    idEmpresa: values.idEmpresa === '' || values.idEmpresa == null ? null : Number(values.idEmpresa),
    nombres: values.nombres,
    apellidos: values.apellidos,
    correo: values.correo,
    username: values.username,
    password: values.password,
    rol: Number(values.rol),
  };
}

function toUpdatePayload(row, values) {
  const payload = {
    id: row.id,
    idEmpresa: values.idEmpresa === '' || values.idEmpresa == null ? null : Number(values.idEmpresa),
    nombres: values.nombres,
    apellidos: values.apellidos,
    correo: values.correo,
    username: values.username,
    rol: Number(values.rol),
    habilitado: row.habilitado,
  };

  if (values.password) {
    payload.password = values.password;
  }

  return payload;
}

export function UsuariosPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteEmpresa(usuario);
  const allowAdminGeneral = isAdministradorGeneral(usuario);
  const { visibleRows, isLoading, errorMessage, filter, setFilter, banner, setBanner, reload } =
    useCatalogCollection(usuarioService.getAll);
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
    nombreCompleto: `${row.nombres ?? ''} ${row.apellidos ?? ''}`.trim(),
    empresaNombre: row.idEmpresa == null ? 'Todas' : (empresaById.get(row.idEmpresa) ?? row.idEmpresa),
    rolNombre: rolUsuarioLabel[row.rol] ?? row.rol,
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
      if (editing) {
        await usuarioService.update(editing.id, toUpdatePayload(editing, values));
        setBanner({ variant: 'success', message: 'El usuario se actualizó correctamente.' });
        setFormOpen(false);
        setEditing(null);
      } else {
        await usuarioService.create(toCreatePayload(values));
        setFormOpen(false);
        setEditing(null);
        setBanner({ variant: 'success', message: 'El usuario se creó correctamente.' });
      }

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
        await usuarioService.remove(confirmRow.id);
        setBanner({ variant: 'success', message: 'El usuario se inactivó correctamente.' });
      } else {
        await usuarioService.update(confirmRow.id, { ...confirmRow, habilitado: true });
        setBanner({ variant: 'success', message: 'El usuario se reactivó correctamente.' });
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
    { key: 'correo', header: 'Correo' },
    { key: 'username', header: 'Usuario' },
    { key: 'rolNombre', header: 'Rol' },
    { key: 'empresaNombre', header: 'Empresa' },
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
        title="Usuarios"
        description="Cuentas de acceso. La contraseña se hashea con Argon2id al guardar."
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
        emptyMessage="No hay usuarios para mostrar."
        searchPlaceholder="Buscar usuarios..."
        rowClassName={(row) => (row.habilitado ? '' : 'bg-slate-50 opacity-70')}
      />

      <Modal
        isOpen={formOpen}
        onClose={closeForm}
        title={editing ? 'Editar usuario' : 'Nuevo usuario'}
      >
        <UsuarioForm
          key={editing ? `edit-${editing.id}` : 'new'}
          isCreate={!editing}
          allowAdminGeneral={allowAdminGeneral}
          initialValues={editing ? toUsuarioFormValues(editing) : EMPTY_USUARIO}
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
        title={confirmRow?.habilitado ? 'Inactivar usuario' : 'Reactivar usuario'}
        message={
          confirmRow?.habilitado
            ? `¿Inactivar "${confirmRow?.nombres} ${confirmRow?.apellidos}"? El registro seguirá visible como inactivo.`
            : `¿Reactivar "${confirmRow?.nombres} ${confirmRow?.apellidos}"?`
        }
        confirmLabel={confirmRow?.habilitado ? 'Inactivar' : 'Reactivar'}
        variant={confirmRow?.habilitado ? 'danger' : 'primary'}
        isConfirming={confirming}
      />
    </section>
  );
}
