import { useEffect, useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Button } from '@/shared/components/Button';
import { CatalogRowActions } from '@/shared/components/CatalogRowActions';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable } from '@/shared/components/DataTable';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { useApiCollection } from '@/shared/hooks/useApiCollection';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { formatDate, toDateInput, toIsoDate, todayInputValue } from '@/shared/utils/dates';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import { ActivoForm } from '@/features/activos/ActivoForm';
import * as activoService from '@/features/activos/activoService';
import * as categoriaService from '@/features/catalogos/categorias/categoriaService';
import * as proveedorService from '@/features/catalogos/proveedores/proveedorService';
import * as ubicacionService from '@/features/catalogos/ubicaciones/ubicacionService';

const EMPTY_ACTIVO = {
  idCategoriaActivo: '',
  idProveedor: '',
  idUbicacion: '',
  nombre: '',
  descripcion: '',
  marca: '',
  modelo: '',
  numeroSerie: '',
  fechaCompra: todayInputValue(),
  costoAdquisicion: '0',
  moneda: 'GTQ',
  numeroFactura: '',
  fechaVencimientoGarantia: todayInputValue(),
  observaciones: '',
};

function toFormValues(row) {
  return {
    idCategoriaActivo: row.idCategoriaActivo ?? '',
    idProveedor: row.idProveedor ?? '',
    idUbicacion: row.idUbicacion ?? '',
    nombre: row.nombre ?? '',
    descripcion: row.descripcion ?? '',
    marca: row.marca ?? '',
    modelo: row.modelo ?? '',
    numeroSerie: row.numeroSerie ?? '',
    fechaCompra: toDateInput(row.fechaCompra) || todayInputValue(),
    costoAdquisicion: row.costoAdquisicion ?? '0',
    moneda: row.moneda ?? 'GTQ',
    numeroFactura: row.numeroFactura ?? '',
    fechaVencimientoGarantia: toDateInput(row.fechaVencimientoGarantia) || todayInputValue(),
    observaciones: row.observaciones ?? '',
  };
}

function toPayload(values) {
  return {
    idCategoriaActivo: Number(values.idCategoriaActivo),
    idProveedor: Number(values.idProveedor),
    idUbicacion: Number(values.idUbicacion),
    nombre: values.nombre,
    descripcion: values.descripcion || null,
    marca: values.marca || null,
    modelo: values.modelo || null,
    numeroSerie: values.numeroSerie || null,
    fechaCompra: toIsoDate(values.fechaCompra),
    costoAdquisicion: Number(values.costoAdquisicion),
    moneda: values.moneda || null,
    numeroFactura: values.numeroFactura || null,
    fechaVencimientoGarantia: toIsoDate(values.fechaVencimientoGarantia),
    observaciones: values.observaciones || null,
  };
}

function toOptions(items) {
  return items
    .filter((item) => item.habilitado !== false)
    .map((item) => ({ value: item.id, label: item.nombre }));
}

export function ActivosPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteOperativa(usuario);
  const { rows, isLoading, errorMessage, banner, setBanner, reload } =
    useApiCollection(activoService.getAll);
  const [categorias, setCategorias] = useState([]);
  const [proveedores, setProveedores] = useState([]);
  const [ubicaciones, setUbicaciones] = useState([]);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirmRow, setConfirmRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadLookups() {
      try {
        const [categoriaRows, proveedorRows, ubicacionRows] = await Promise.all([
          categoriaService.getAll(),
          proveedorService.getAll(),
          ubicacionService.getAll(),
        ]);
        if (!cancelled) {
          setCategorias(categoriaRows);
          setProveedores(proveedorRows);
          setUbicaciones(ubicacionRows);
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

  const categoriaById = new Map(categorias.map((item) => [item.id, item.nombre]));
  const proveedorById = new Map(proveedores.map((item) => [item.id, item.nombre]));
  const ubicacionById = new Map(ubicaciones.map((item) => [item.id, item.nombre]));
  const tableRows = rows.map((row) => ({
    ...row,
    categoriaNombre: categoriaById.get(row.idCategoriaActivo) ?? row.idCategoriaActivo,
    proveedorNombre: proveedorById.get(row.idProveedor) ?? row.idProveedor,
    ubicacionNombre: ubicacionById.get(row.idUbicacion) ?? row.idUbicacion,
    fechaCompraFmt: formatDate(row.fechaCompra),
  }));

  async function handleSave(values) {
    setSaving(true);
    try {
      const payload = toPayload(values);
      if (editing) {
        await activoService.update(editing.id, { id: editing.id, ...payload });
        setBanner({ variant: 'success', message: 'El activo se actualizó correctamente.' });
      } else {
        await activoService.create(payload);
        setBanner({ variant: 'success', message: 'El activo se creó correctamente.' });
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
      await activoService.remove(confirmRow.id);
      setBanner({ variant: 'success', message: 'El activo se eliminó correctamente.' });
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
    { key: 'numeroSerie', header: 'Serie' },
    { key: 'categoriaNombre', header: 'Categoría' },
    { key: 'ubicacionNombre', header: 'Ubicación' },
    { key: 'fechaCompraFmt', header: 'Compra' },
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
        title="Activos"
        description="Inventario de bienes. La ubicación determina la empresa y la sede."
        actions={
          puedeEditar ? (
            <Button
              onClick={() => {
                setEditing(null);
                setFormOpen(true);
              }}
              type="button"
            >
              Nuevo
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
        emptyMessage="No hay activos para mostrar."
        searchPlaceholder="Buscar activos..."
      />
      <Modal isOpen={formOpen} onClose={() => (saving ? null : setFormOpen(false))} title={editing ? 'Editar activo' : 'Nuevo activo'}>
        <ActivoForm
          key={editing ? `edit-${editing.id}` : 'new'}
          initialValues={editing ? toFormValues(editing) : EMPTY_ACTIVO}
          categoriaOptions={toOptions(categorias)}
          proveedorOptions={toOptions(proveedores)}
          ubicacionOptions={toOptions(ubicaciones)}
          onSubmit={handleSave}
          onCancel={() => setFormOpen(false)}
          isSubmitting={saving}
        />
      </Modal>
      <ConfirmDialog
        isOpen={Boolean(confirmRow)}
        onClose={() => (confirming ? null : setConfirmRow(null))}
        onConfirm={handleConfirm}
        title="Eliminar activo"
        message={`¿Eliminar "${confirmRow?.nombre}"? Esta acción no se puede deshacer.`}
        confirmLabel="Eliminar"
        variant="danger"
        isConfirming={confirming}
      />
    </section>
  );
}
