import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { Button } from '@/shared/components/Button';
import { CatalogRowActions } from '@/shared/components/CatalogRowActions';
import { DataTable } from '@/shared/components/DataTable';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { subscribeEmpresaActiva } from '@/shared/empresaActiva';
import { useApiCollection } from '@/shared/hooks/useApiCollection';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { formatDate, toDateInput, toIsoDate, todayInputValue } from '@/shared/utils/dates';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import { ActivoForm } from '@/features/activos/ActivoForm';
import { estadoBadgeVariant, estadoNombre } from '@/features/activos/activoEstado';
import { QrCodeCard, QrThumb } from '@/features/activos/QrCodeCard';
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
  const navigate = useNavigate();
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
  const [qrActivo, setQrActivo] = useState(null);

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
    const unsub = subscribeEmpresaActiva(() => loadLookups());
    return () => {
      cancelled = true;
      unsub();
    };
  }, [setBanner]);

  const categoriaById = new Map(categorias.map((item) => [item.id, item.nombre]));
  const proveedorById = new Map(proveedores.map((item) => [item.id, item.nombre]));
  const ubicacionById = new Map(ubicaciones.map((item) => [item.id, item.nombre]));
  const tableRows = rows.map((row) => ({
    ...row,
    categoriaNombre: categoriaById.get(row.idCategoriaActivo) ?? row.idCategoriaActivo,
    proveedorNombre: proveedorById.get(row.idProveedor) ?? row.idProveedor,
    ubicacionNombre: ubicacionById.get(row.idUbicacion) ?? row.nombreUbicacion ?? row.idUbicacion,
    sedeNombre: row.nombreSede ?? '—',
    areaNombre: row.nombreArea ?? '—',
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
        const created = await activoService.create(payload);
        const newId = created?.id;
        setBanner({ variant: 'success', message: 'El activo se creó correctamente. Imprime o descarga su QR.' });
        if (newId) {
          setQrActivo({ id: newId, nombre: values.nombre });
        }
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

  const columns = [
    {
      key: 'qr',
      header: 'QR',
      sortable: false,
      render: (row) => (
        <button type="button" className="rounded border border-slate-200 p-0.5" onClick={() => setQrActivo(row)}>
          <QrThumb activoId={row.id} size={48} alt={`QR de ${row.nombre}`} />
        </button>
      ),
    },
    { key: 'nombre', header: 'Nombre' },
    { key: 'codigoInterno', header: 'Código' },
    { key: 'numeroSerie', header: 'Serie' },
    { key: 'categoriaNombre', header: 'Categoría' },
    { key: 'sedeNombre', header: 'Sede' },
    { key: 'ubicacionNombre', header: 'Ubicación' },
    { key: 'areaNombre', header: 'Área' },
    {
      key: 'estadoNombre',
      header: 'Estado',
      render: (row) => <Badge variant={estadoBadgeVariant(row)}>{estadoNombre(row)}</Badge>,
    },
    { key: 'nombreResponsable', header: 'Responsable' },
    { key: 'fechaCompraFmt', header: 'Compra' },
    {
      key: 'acciones',
      header: '',
      align: 'right',
      sortable: false,
      render: (row) => (
        <CatalogRowActions
          row={row}
          extra={
            <>
              <Button variant="ghost" onClick={() => navigate(`/activos/${row.id}`)}>
                Ver
              </Button>
              <Button variant="ghost" onClick={() => setQrActivo(row)}>
                QR
              </Button>
            </>
          }
          onEdit={
            puedeEditar
              ? () => {
                  setEditing(row);
                  setFormOpen(true);
                }
              : undefined
          }
          onDelete={undefined}
        />
      ),
    },
  ];

  return (
    <section className="space-y-6">
      <PageHeader
        title="Activos"
        description="Cada activo tiene QR permanente. Escanéelo para ver el detalle o registrar un movimiento. El código interno es único; para sacarlo de circulación use Baja."
        actions={
          <div className="flex flex-wrap gap-2">
            <Button variant="secondary" type="button" onClick={() => navigate('/activos/escanear')}>
              Escanear QR
            </Button>
            {puedeEditar ? (
              <Button
                onClick={() => {
                  setEditing(null);
                  setFormOpen(true);
                }}
                type="button"
              >
                Nuevo
              </Button>
            ) : null}
          </div>
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
      <Modal isOpen={Boolean(qrActivo)} onClose={() => setQrActivo(null)} title="Código QR del activo">
        {qrActivo ? <QrCodeCard activoId={qrActivo.id} nombre={qrActivo.nombre} /> : null}
      </Modal>
    </section>
  );
}
