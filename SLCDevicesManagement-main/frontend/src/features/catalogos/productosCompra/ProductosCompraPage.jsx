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
import { formatDate, toDateInput, toIsoDate, todayInputValue } from '@/shared/utils/dates';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import { ProductoCompraForm } from '@/features/catalogos/productosCompra/ProductoCompraForm';
import * as productoCompraService from '@/features/catalogos/productosCompra/productoCompraService';
import * as proveedorService from '@/features/catalogos/proveedores/proveedorService';
import * as categoriaService from '@/features/catalogos/categorias/categoriaService';

const EMPTY_PRODUCTO = {
  idProveedor: '',
  idCategoriaActivo: '',
  nombre: '',
  marca: '',
  modelo: '',
  descripcion: '',
  costoUnitario: '0',
  moneda: 'GTQ',
  numeroFactura: '',
  fechaCompra: todayInputValue(),
  fechaVencimientoGarantia: todayInputValue(),
};

function toFormValues(row) {
  return {
    idProveedor: row.idProveedor ?? '',
    idCategoriaActivo: row.idCategoriaActivo ?? '',
    nombre: row.nombre ?? '',
    marca: row.marca ?? '',
    modelo: row.modelo ?? '',
    descripcion: row.descripcion ?? '',
    costoUnitario: row.costoUnitario ?? '0',
    moneda: row.moneda ?? 'GTQ',
    numeroFactura: row.numeroFactura ?? '',
    fechaCompra: toDateInput(row.fechaCompra) || todayInputValue(),
    fechaVencimientoGarantia: toDateInput(row.fechaVencimientoGarantia) || todayInputValue(),
  };
}

function toPayload(values) {
  return {
    idProveedor: Number(values.idProveedor),
    idCategoriaActivo: values.idCategoriaActivo ? Number(values.idCategoriaActivo) : null,
    nombre: values.nombre,
    marca: values.marca || null,
    modelo: values.modelo || null,
    descripcion: values.descripcion || null,
    costoUnitario: Number(values.costoUnitario),
    moneda: values.moneda || null,
    numeroFactura: values.numeroFactura || null,
    fechaCompra: toIsoDate(values.fechaCompra),
    fechaVencimientoGarantia: toIsoDate(values.fechaVencimientoGarantia),
  };
}

export function ProductosCompraPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteOperativa(usuario);
  const { visibleRows, isLoading, errorMessage, filter, setFilter, banner, setBanner, reload } =
    useCatalogCollection(productoCompraService.getAll);
  const [proveedores, setProveedores] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [saving, setSaving] = useState(false);
  const [confirmRow, setConfirmRow] = useState(null);
  const [confirming, setConfirming] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadLookups() {
      try {
        const [proveedorRows, categoriaRows] = await Promise.all([
          proveedorService.getAll(),
          categoriaService.getAll(),
        ]);
        if (!cancelled) {
          setProveedores(proveedorRows);
          setCategorias(categoriaRows);
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

  const proveedorById = new Map(proveedores.map((item) => [item.id, item.nombre]));
  const tableRows = visibleRows.map((row) => ({
    ...row,
    proveedorNombre: proveedorById.get(row.idProveedor) ?? row.idProveedor,
    fechaCompraFmt: formatDate(row.fechaCompra),
    garantiaFmt: formatDate(row.fechaVencimientoGarantia),
    costoFmt: `${row.moneda || 'GTQ'} ${row.costoUnitario}`,
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
      const payload = toPayload(values);
      if (editing) {
        await productoCompraService.update(editing.id, { ...editing, ...payload });
        setBanner({ variant: 'success', message: 'El producto se actualizó correctamente.' });
      } else {
        await productoCompraService.create({ ...payload, habilitado: true });
        setBanner({ variant: 'success', message: 'El producto se registró correctamente.' });
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
        await productoCompraService.remove(confirmRow.id);
        setBanner({ variant: 'success', message: 'El producto se inactivó correctamente.' });
      } else {
        await productoCompraService.update(confirmRow.id, { ...confirmRow, habilitado: true });
        setBanner({ variant: 'success', message: 'El producto se reactivó correctamente.' });
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
    { key: 'nombre', header: 'Producto' },
    { key: 'proveedorNombre', header: 'Proveedor' },
    { key: 'marca', header: 'Marca' },
    { key: 'modelo', header: 'Modelo' },
    { key: 'numeroFactura', header: 'Factura' },
    { key: 'fechaCompraFmt', header: 'Compra' },
    { key: 'garantiaFmt', header: 'Garantía' },
    { key: 'costoFmt', header: 'Costo' },
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
        title="Productos de compra"
        description="Catálogo de lo que cada proveedor vendió (factura, marca, modelo y costo). Al crear un activo se rellenan esos datos."
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
        emptyMessage="No hay productos de compra. Registre el stock o la factura del proveedor."
        searchPlaceholder="Buscar productos..."
        rowClassName={(row) => (row.habilitado ? '' : 'bg-slate-50 opacity-70')}
      />

      <Modal
        isOpen={formOpen}
        onClose={closeForm}
        title={editing ? 'Editar producto' : 'Nuevo producto de compra'}
        wide
      >
        <ProductoCompraForm
          key={editing ? `edit-${editing.id}` : 'new'}
          initialValues={editing ? toFormValues(editing) : EMPTY_PRODUCTO}
          proveedorOptions={proveedores
            .filter((item) => item.habilitado !== false)
            .map((item) => ({ value: item.id, label: item.nombre }))}
          categoriaOptions={categorias
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
        title={confirmRow?.habilitado ? 'Inactivar producto' : 'Reactivar producto'}
        message={
          confirmRow?.habilitado
            ? `¿Inactivar "${confirmRow?.nombre}"? Seguirá visible como inactivo.`
            : `¿Reactivar "${confirmRow?.nombre}"?`
        }
        confirmLabel={confirmRow?.habilitado ? 'Inactivar' : 'Reactivar'}
        variant={confirmRow?.habilitado ? 'danger' : 'primary'}
        isConfirming={confirming}
      />
    </section>
  );
}
