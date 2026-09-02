import { useCallback, useEffect, useState } from 'react';
import { AlertBanner } from '@/shared/components/AlertBanner';
import { Badge } from '@/shared/components/Badge';
import { Button } from '@/shared/components/Button';
import { CheckboxField } from '@/shared/components/CheckboxField';
import { ConfirmDialog } from '@/shared/components/ConfirmDialog';
import { DataTable } from '@/shared/components/DataTable';
import { FormActions } from '@/shared/components/FormActions';
import { Modal } from '@/shared/components/Modal';
import { PageHeader } from '@/shared/components/PageHeader';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { TextareaField } from '@/shared/components/TextareaField';
import { subscribeEmpresaActiva } from '@/shared/empresaActiva';
import { useApiCollection } from '@/shared/hooks/useApiCollection';
import { useForm } from '@/shared/hooks/useForm';
import { getErrorMessage } from '@/shared/utils/getErrorMessage';
import { formatDate, toIsoDate, todayInputValue } from '@/shared/utils/dates';
import { enforceRequired } from '@/shared/utils/fieldErrors';
import { useAuth } from '@/features/auth/AuthContext';
import { canWriteOperativa } from '@/features/auth/permissions';
import * as activoService from '@/features/activos/activoService';
import * as ubicacionService from '@/features/catalogos/ubicaciones/ubicacionService';
import * as detalleActivoService from '@/features/inventario/detalleActivoService';
import * as historicoService from '@/features/inventario/historicoInventarioService';
import * as sedeService from '@/features/organizacion/sedes/sedeService';

function parseIdUbicacionJornada(observaciones) {
  const match = String(observaciones ?? '').match(/\[id_ubicacion=(\d+)\]/i);
  return match ? Number(match[1]) : null;
}

function JornadaForm({ initialValues, sedeOptions, ubicaciones, onSubmit, onCancel, isSubmitting }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit, patchValues } = useForm({
    initialValues,
    validate: (next) => {
      const result = {};
      enforceRequired(result, next, 'idSede', 'id sede');
      enforceRequired(result, next, 'fechaInicio', 'fecha inicio');
      return result;
    },
  });

  const ubicacionesSede = (ubicaciones ?? []).filter(
    (item) => !values.idSede || Number(item.idSede) === Number(values.idSede),
  );

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <SelectField
        label="Sede"
        name="idSede"
        value={values.idSede}
        onChange={(field) => {
          patchValues({ idSede: field.value, idUbicacion: '' });
        }}
        onBlur={handleBlur}
        error={touched.idSede ? errors.idSede : undefined}
        options={sedeOptions}
        required
      />
      <SelectField
        label="Delimitar por ubicación (opcional)"
        name="idUbicacion"
        value={values.idUbicacion}
        onChange={handleChange}
        onBlur={handleBlur}
        options={ubicacionesSede
          .filter((item) => item.habilitado !== false)
          .map((item) => ({ value: item.id, label: item.nombre }))}
        placeholder="Toda la sede"
      />
      <TextField
        label="Responsable de la jornada"
        name="responsable"
        value={values.responsable}
        onChange={handleChange}
        onBlur={handleBlur}
      />
      <TextField
        label="Fecha de inicio"
        name="fechaInicio"
        type="date"
        value={values.fechaInicio}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.fechaInicio ? errors.fechaInicio : undefined}
        required
      />
      <TextareaField
        label="Observaciones"
        name="observaciones"
        value={values.observaciones}
        onChange={handleChange}
        onBlur={handleBlur}
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}

function HallazgoForm({ initialValues, activoOptions, ubicacionOptions, onSubmit, onCancel, isSubmitting }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: (next) => {
      const result = {};
      enforceRequired(result, next, 'idActivo', 'id activo');
      if (next.encontradoEnOtraUbicacion) {
        enforceRequired(result, next, 'idUbicacionObservada', 'ubicacion observada');
      }
      return result;
    },
  });

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <SelectField
        label="Activo"
        name="idActivo"
        value={values.idActivo}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idActivo ? errors.idActivo : undefined}
        options={activoOptions}
        required
      />
      <CheckboxField
        label="Encontrado"
        name="encontrado"
        value={values.encontrado}
        onChange={handleChange}
      />
      <CheckboxField
        label="Buen estado físico"
        name="buenEstado"
        value={values.buenEstado}
        onChange={handleChange}
      />
      <CheckboxField
        label="Encontrado en otra ubicación"
        name="encontradoEnOtraUbicacion"
        value={values.encontradoEnOtraUbicacion}
        onChange={handleChange}
      />
      {values.encontradoEnOtraUbicacion ? (
        <SelectField
          label="Ubicación donde se encontró"
          name="idUbicacionObservada"
          value={values.idUbicacionObservada}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.idUbicacionObservada ? errors.idUbicacionObservada : undefined}
          options={ubicacionOptions}
          required
        />
      ) : null}
      <TextareaField
        label="Observaciones"
        name="observaciones"
        value={values.observaciones}
        onChange={handleChange}
        onBlur={handleBlur}
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} submitLabel="Registrar" />
    </form>
  );
}

export function InventarioPage() {
  const { usuario } = useAuth();
  const puedeEditar = canWriteOperativa(usuario);
  const load = useCallback(() => historicoService.getAll(), []);
  const { rows, isLoading, errorMessage, banner, setBanner, reload } = useApiCollection(load);
  const [sedes, setSedes] = useState([]);
  const [activos, setActivos] = useState([]);
  const [ubicaciones, setUbicaciones] = useState([]);
  const [formOpen, setFormOpen] = useState(false);
  const [saving, setSaving] = useState(false);
  const [jornada, setJornada] = useState(null);
  const [detalles, setDetalles] = useState([]);
  const [hallazgoOpen, setHallazgoOpen] = useState(false);
  const [cerrarRow, setCerrarRow] = useState(null);
  const [confirming, setConfirming] = useState(false);
  const [diferencias, setDiferencias] = useState(null);

  useEffect(() => {
    let cancelled = false;

    function loadLookups() {
      Promise.all([sedeService.getAll(), activoService.getAll(), ubicacionService.getAll()])
        .then(([sedeRows, activoRows, ubicacionRows]) => {
          if (!cancelled) {
            setSedes(sedeRows);
            setActivos(activoRows);
            setUbicaciones(ubicacionRows);
          }
        })
        .catch((error) => {
          if (!cancelled) setBanner({ variant: 'error', message: getErrorMessage(error) });
        });
    }

    loadLookups();
    const unsub = subscribeEmpresaActiva(() => loadLookups());
    return () => {
      cancelled = true;
      unsub();
    };
  }, [setBanner]);

  const sedeById = new Map(sedes.map((item) => [item.id, item.nombre]));
  const activoById = new Map(activos.map((item) => [item.id, item.nombre]));
  const ubicacionById = new Map(ubicaciones.map((item) => [item.id, item]));
  const activosDeJornada = jornada
    ? activos.filter((item) => {
        const ubicacion = ubicacionById.get(item.idUbicacion);
        if (ubicacion?.idSede !== jornada.idSede) {
          return false;
        }
        const idUbicacionFiltro = parseIdUbicacionJornada(jornada.observaciones);
        return !idUbicacionFiltro || Number(item.idUbicacion) === idUbicacionFiltro;
      })
    : activos;
  const idsRegistrados = new Set(detalles.map((item) => item.idActivo));
  const esperadosSinRegistrar = activosDeJornada.filter((item) => !idsRegistrados.has(item.id));
  const tableRows = rows.map((row) => ({
    ...row,
    sedeNombre: sedeById.get(row.idSede) ?? row.idSede,
    inicioFmt: formatDate(row.fechaInicio),
    cierreFmt: formatDate(row.fechaCierre),
  }));

  async function openJornada(row) {
    setJornada(row);
    try {
      const items = await detalleActivoService.getAll({ idHistoricoInventario: row.id });
      setDetalles(items);
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
      setDetalles([]);
    }
  }

  async function handleCreate(values) {
    setSaving(true);
    try {
      await historicoService.create({
        idSede: Number(values.idSede),
        responsable: values.responsable || null,
        fechaInicio: toIsoDate(values.fechaInicio),
        observaciones: values.idUbicacion
          ? `[id_ubicacion=${values.idUbicacion}] ${values.observaciones || ''}`.trim()
          : values.observaciones || null,
      });
      setBanner({ variant: 'success', message: 'La jornada se abrió correctamente.' });
      setFormOpen(false);
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setSaving(false);
    }
  }

  async function handleHallazgo(values) {
    setSaving(true);
    try {
      let observaciones = values.observaciones || '';
      if (values.encontradoEnOtraUbicacion) {
        const ubicacion = ubicaciones.find((item) => item.id === Number(values.idUbicacionObservada));
        observaciones = `[Otra ubicacion: ${ubicacion?.nombre ?? values.idUbicacionObservada}] ${observaciones}`.trim();
      }
      await historicoService.registrarDetalle(jornada.id, {
        idActivo: Number(values.idActivo),
        encontrado: Boolean(values.encontrado),
        buenEstado: Boolean(values.buenEstado),
        observaciones: observaciones || null,
        fechaVerificacion: toIsoDate(todayInputValue()),
      });
      setBanner({ variant: 'success', message: 'Hallazgo registrado.' });
      setHallazgoOpen(false);
      await openJornada(jornada);
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setSaving(false);
    }
  }

  async function handleCerrar() {
    if (!cerrarRow) return;
    setConfirming(true);
    try {
      await historicoService.cerrar(cerrarRow.id, { id: cerrarRow.id });
      setBanner({ variant: 'success', message: 'La jornada se cerró. Ya no se pueden registrar hallazgos.' });
      setCerrarRow(null);
      if (jornada?.id === cerrarRow.id) {
        setJornada({ ...jornada, cerrado: true });
      }
      await reload();
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    } finally {
      setConfirming(false);
    }
  }

  async function verDiferencias(row) {
    try {
      const data = await historicoService.getDiferencias(row.id, !row.cerrado);
      setDiferencias(data);
    } catch (error) {
      setBanner({ variant: 'error', message: getErrorMessage(error) });
    }
  }

  const columns = [
    { key: 'sedeNombre', header: 'Sede' },
    { key: 'responsable', header: 'Responsable' },
    { key: 'inicioFmt', header: 'Inicio' },
    { key: 'cierreFmt', header: 'Cierre' },
    {
      key: 'cerrado',
      header: 'Estado',
      render: (row) => (
        <Badge variant={row.cerrado ? 'ghost' : 'success'}>{row.cerrado ? 'Cerrada' : 'Abierta'}</Badge>
      ),
    },
    {
      key: 'acciones',
      header: '',
      align: 'right',
      sortable: false,
      render: (row) => (
        <div className="flex flex-wrap justify-end gap-1" onClick={(event) => event.stopPropagation()}>
          <Button variant="ghost" onClick={() => openJornada(row)}>
            Ver
          </Button>
          <Button variant="ghost" onClick={() => verDiferencias(row)}>
            Diferencias
          </Button>
          {puedeEditar && !row.cerrado ? (
            <Button variant="danger" onClick={() => setCerrarRow(row)}>
              Cerrar
            </Button>
          ) : null}
        </div>
      ),
    },
  ];

  const detalleRows = detalles.map((row) => ({
    ...row,
    activoNombre: activoById.get(row.idActivo) ?? row.idActivo,
    encontradoLabel: row.encontrado ? 'Sí' : 'No',
    estadoLabel: row.buenEstado ? 'Bueno' : 'No',
  }));

  return (
    <section className="space-y-6">
      <PageHeader
        title="Inventario físico"
        description="Jornadas por sede o ubicación. Muestre teóricos, registre hallazgos, identifique faltantes y cierre para congelar el resultado."
        actions={
          puedeEditar ? (
            <Button type="button" onClick={() => setFormOpen(true)}>
              Nueva jornada
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
        emptyMessage="No hay jornadas de inventario."
        searchPlaceholder="Buscar jornadas..."
      />

      {jornada ? (
        <div className="rounded-lg border border-slate-200 bg-white p-4">
          <div className="mb-3 flex items-center justify-between gap-2">
            <h3 className="text-sm font-semibold">
              Hallazgos · {sedeById.get(jornada.idSede) ?? jornada.idSede}{' '}
              {jornada.cerrado ? '(cerrada)' : '(abierta)'}
            </h3>
            {puedeEditar && !jornada.cerrado ? (
              <Button type="button" onClick={() => setHallazgoOpen(true)}>
                Registrar hallazgo
              </Button>
            ) : null}
          </div>
          <p className="mb-3 text-sm text-slate-600">
            Teóricos en el alcance: {activosDeJornada.length}. Registrados: {detalles.length}. Pendientes de
            verificar: {esperadosSinRegistrar.length}.
          </p>
          {esperadosSinRegistrar.length > 0 ? (
            <p className="mb-3 text-sm text-amber-800">
              Sin registrar: {esperadosSinRegistrar.map((item) => item.nombre).join(', ')}
            </p>
          ) : null}
          <DataTable
            columns={[
              { key: 'activoNombre', header: 'Activo' },
              { key: 'encontradoLabel', header: 'Encontrado' },
              { key: 'estadoLabel', header: 'Buen estado' },
              { key: 'observaciones', header: 'Observaciones' },
            ]}
            rows={detalleRows}
            emptyMessage="Aún no hay hallazgos en esta jornada."
            pageSize={5}
          />
        </div>
      ) : null}

      <Modal isOpen={formOpen} onClose={() => (saving ? null : setFormOpen(false))} title="Nueva jornada">
        <JornadaForm
          initialValues={{
            idSede: '',
            idUbicacion: '',
            responsable: usuario?.nombre ?? '',
            fechaInicio: todayInputValue(),
            observaciones: '',
          }}
          sedeOptions={sedes
            .filter((item) => item.habilitado !== false)
            .map((item) => ({ value: item.id, label: item.nombre }))}
          ubicaciones={ubicaciones}
          onSubmit={handleCreate}
          onCancel={() => setFormOpen(false)}
          isSubmitting={saving}
        />
      </Modal>

      <Modal
        isOpen={hallazgoOpen}
        onClose={() => (saving ? null : setHallazgoOpen(false))}
        title="Registrar hallazgo"
      >
        <HallazgoForm
          initialValues={{
            idActivo: '',
            encontrado: true,
            buenEstado: true,
            encontradoEnOtraUbicacion: false,
            idUbicacionObservada: '',
            observaciones: '',
          }}
          activoOptions={activos.map((item) => ({
            value: item.id,
            label: `${item.codigoInterno || item.nombre} · ${item.nombreUbicacion || 'sin ubicación'}`,
          }))}
          ubicacionOptions={ubicaciones
            .filter((item) => item.habilitado !== false)
            .map((item) => ({ value: item.id, label: item.nombre }))}
          onSubmit={handleHallazgo}
          onCancel={() => setHallazgoOpen(false)}
          isSubmitting={saving}
        />
      </Modal>

      <Modal isOpen={Boolean(diferencias)} onClose={() => setDiferencias(null)} title="Diferencias de inventario">
        {diferencias ? (
          <div className="space-y-4 text-sm">
            {[
              ['Encontrados', diferencias.encontradosEnSede],
              ['No encontrados', diferencias.noEncontrados],
              ['Sobrantes', diferencias.sobrantes],
              ['En otra ubicación', diferencias.encontradosEnOtraUbicacion],
              ['Faltantes sin registrar', diferencias.faltantesSinRegistrar],
            ].map(([label, items]) => (
              <div key={label}>
                <p className="font-medium text-slate-800">
                  {label} ({items?.length ?? 0})
                </p>
                <ul className="mt-1 list-disc pl-5 text-slate-600">
                  {(items ?? []).length === 0 ? (
                    <li>Ninguno</li>
                  ) : (
                    items.map((item) => (
                      <li key={item.idActivo}>
                        {item.nombreActivo} · {item.nombreUbicacion}
                      </li>
                    ))
                  )}
                </ul>
              </div>
            ))}
          </div>
        ) : null}
      </Modal>

      <ConfirmDialog
        isOpen={Boolean(cerrarRow)}
        onClose={() => (confirming ? null : setCerrarRow(null))}
        onConfirm={handleCerrar}
        title="Cerrar jornada"
        message="El cierre es irreversible. Después no se podrán registrar más hallazgos."
        confirmLabel="Cerrar jornada"
        variant="danger"
        isConfirming={confirming}
      />
    </section>
  );
}
