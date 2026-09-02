import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { SignaturePad } from '@/shared/components/SignaturePad';
import { TextField } from '@/shared/components/TextField';
import { TextareaField } from '@/shared/components/TextareaField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

function idAreaDe(item) {
  const raw = item?.idArea ?? item?.IdArea ?? item?.id_area;
  if (raw === null || raw === undefined || raw === '') return null;
  const parsed = Number(raw);
  return Number.isNaN(parsed) ? null : parsed;
}

export const MOTIVOS_BAJA = [
  { value: 'Venta', label: 'Venta' },
  { value: 'Desecho', label: 'Desecho' },
  { value: 'Donacion', label: 'Donación' },
  { value: 'Perdida', label: 'Pérdida' },
  { value: 'Robo', label: 'Robo' },
  { value: 'Dano irreparable', label: 'Daño irreparable' },
  { value: 'Otro', label: 'Otro motivo autorizado' },
];

export const TIPOS_MANTENIMIENTO = [
  { value: 'Preventivo', label: 'Preventivo' },
  { value: 'Correctivo', label: 'Correctivo' },
];

export function MovimientoForm({
  kind,
  initialValues,
  activoOptions,
  responsableOptions,
  ubicacionOptions,
  areaOptions = [],
  responsables = [],
  activos = [],
  proveedorOptions = [],
  onSubmit,
  onCancel,
  isSubmitting,
  activoLocked = false,
}) {
  const isBaja = kind === 'baja';
  const isMantenimiento = kind === 'mantenimiento';
  const isTraslado = kind === 'traslado';
  const isAsignacion = kind === 'asignacion';
  const needsUbicacion = !isBaja;
  const needsFirmas = kind === 'asignacion' || kind === 'baja';
  const emptyHint = {
    asignacion:
      'No hay activos disponibles. Un activo ya asignado, en mantenimiento o de baja no aparece en esta lista.',
    traslado:
      'No hay activos para trasladar. Los que están en mantenimiento o de baja no aparecen.',
    mantenimiento:
      'No hay activos disponibles para mantenimiento. Debe estar libre (no asignado ni de baja).',
    baja: 'No hay activos para dar de baja. No aparecen los que ya están de baja o en mantenimiento.',
  }[kind];

  function validate(values) {
    const errors = {};
    enforceRequired(errors, values, 'idActivo', 'id activo');
    enforceRequired(errors, values, 'idResponsable', 'id responsable');
    enforceRequired(errors, values, 'fechaAsignacion', 'fecha');
    if (needsUbicacion) {
      enforceRequired(errors, values, 'idUbicacion', 'id ubicacion');
    }
    if (isMantenimiento) {
      enforceRequired(errors, values, 'observaciones', 'descripcion del problema');
      enforceRequired(errors, values, 'tipoMantenimiento', 'tipo de mantenimiento');
    }
    if (isTraslado) {
      enforceRequired(errors, values, 'observaciones', 'motivo del traslado');
    }
    if (isBaja) {
      enforceRequired(errors, values, 'motivo', 'motivo');
    }
    if (needsFirmas) {
      enforceRequired(errors, values, 'firmaEntrega', 'firma de entrega');
      enforceRequired(errors, values, 'firmaRecibe', 'firma de recepcion');
    }
    enforceMaxLength(errors, values, 'observaciones', 'observaciones', 300);
    enforceMaxLength(errors, values, 'motivo', 'motivo', 300);
    enforceMaxLength(errors, values, 'documentoPdfUrl', 'documento', 300);
    return errors;
  }

  const { values, errors, touched, handleChange, handleBlur, handleSubmit, patchValues } = useForm({
    initialValues,
    validate,
  });

  const activoSeleccionado = (activos ?? []).find((item) => Number(item.id) === Number(values.idActivo));
  const areaSeleccionada = values.idArea !== '' && values.idArea != null;
  const habilitados = (responsables ?? []).filter((item) => item.habilitado !== false);
  const responsablesDeArea = areaSeleccionada
    ? habilitados.filter((item) => idAreaDe(item) === Number(values.idArea))
    : habilitados;
  const responsablesVisibles = isAsignacion
    ? responsablesDeArea.map((item) => ({
        value: item.id,
        label: item.cargo ? `${item.nombreCompleto} — ${item.cargo}` : (item.nombreCompleto ?? `#${item.id}`),
      }))
    : responsableOptions;

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
        disabled={activoLocked}
      />
      {activoOptions.length === 0 ? (
        <p className="text-sm text-amber-800">{emptyHint}</p>
      ) : null}
      {isTraslado && values.idActivo ? (
        <p className="rounded-md bg-slate-50 px-3 py-2 text-sm text-slate-700">
          Ubicación de origen:{' '}
          <strong>{activoSeleccionado?.nombreUbicacion || activoSeleccionado?.idUbicacion || 'actual del activo'}</strong>
        </p>
      ) : null}
      {isAsignacion && areaOptions.length > 0 ? (
        <SelectField
          label="Área"
          name="idArea"
          value={values.idArea}
          onChange={(field) => {
            patchValues({ idArea: field.value, idResponsable: '' });
          }}
          onBlur={handleBlur}
          options={areaOptions}
          placeholder="Todas las áreas"
        />
      ) : null}
      <SelectField
        label={isBaja ? 'Autoriza (responsable)' : isAsignacion ? 'Persona del área' : 'Responsable que recibe'}
        name="idResponsable"
        value={values.idResponsable}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idResponsable ? errors.idResponsable : undefined}
        options={responsablesVisibles}
        required
        placeholder={
          isAsignacion && areaSeleccionada ? 'Seleccionar persona del área' : 'Seleccionar...'
        }
      />
      {isAsignacion && areaSeleccionada && responsablesVisibles.length === 0 ? (
        <p className="text-sm text-amber-800">
          No hay personas habilitadas en esta área. Elija otra área o registre un responsable en Catálogos.
        </p>
      ) : null}
      {needsUbicacion ? (
        <SelectField
          label={isTraslado ? 'Ubicación destino' : 'Ubicación de uso'}
          name="idUbicacion"
          value={values.idUbicacion}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.idUbicacion ? errors.idUbicacion : undefined}
          options={ubicacionOptions}
          required
        />
      ) : null}
      <TextField
        label="Fecha"
        name="fechaAsignacion"
        type="date"
        value={values.fechaAsignacion}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.fechaAsignacion ? errors.fechaAsignacion : undefined}
        required
      />
      {isBaja ? (
        <>
          <SelectField
            label="Motivo de baja"
            name="motivo"
            value={values.motivo}
            onChange={handleChange}
            onBlur={handleBlur}
            error={touched.motivo ? errors.motivo : undefined}
            options={MOTIVOS_BAJA}
            required
          />
          <TextField
            label="Documento de referencia (opcional)"
            name="documentoPdfUrl"
            value={values.documentoPdfUrl}
            onChange={handleChange}
            onBlur={handleBlur}
            error={touched.documentoPdfUrl ? errors.documentoPdfUrl : undefined}
            placeholder="Folio, factura o acta"
          />
        </>
      ) : (
        <>
          {isMantenimiento ? (
            <SelectField
              label="Tipo de mantenimiento"
              name="tipoMantenimiento"
              value={values.tipoMantenimiento}
              onChange={handleChange}
              onBlur={handleBlur}
              error={touched.tipoMantenimiento ? errors.tipoMantenimiento : undefined}
            options={TIPOS_MANTENIMIENTO}
            required
          />
        ) : null}
        {isMantenimiento && proveedorOptions.length > 0 ? (
          <SelectField
            label="Proveedor del trabajo (opcional)"
            name="idProveedorTrabajo"
            value={values.idProveedorTrabajo}
            onChange={handleChange}
            onBlur={handleBlur}
            options={proveedorOptions}
            placeholder="Usar el responsable indicado"
          />
        ) : null}
        <TextareaField
            label={isTraslado ? 'Motivo del traslado' : isMantenimiento ? 'Descripción del problema' : 'Observaciones'}
            name="observaciones"
            value={values.observaciones}
            onChange={handleChange}
            onBlur={handleBlur}
            error={touched.observaciones ? errors.observaciones : undefined}
            required={isMantenimiento || isTraslado}
          />
          {isMantenimiento ? (
            <div className="grid gap-4 sm:grid-cols-2">
              <TextField
                label="Costo (se guarda en historial)"
                name="costo"
                value={values.costo}
                onChange={handleChange}
                onBlur={handleBlur}
              />
              <TextField
                label="Factura del trabajo"
                name="numeroFactura"
                value={values.numeroFactura}
                onChange={handleChange}
                onBlur={handleBlur}
              />
            </div>
          ) : null}
        </>
      )}
      {needsFirmas ? (
        <div className="grid gap-4 sm:grid-cols-2">
          <SignaturePad
            label={isBaja ? 'Firma de quien registra' : 'Firma de quien entrega'}
            name="firmaEntrega"
            value={values.firmaEntrega}
            onChange={handleChange}
            error={touched.firmaEntrega ? errors.firmaEntrega : undefined}
            required
          />
          <SignaturePad
            label={isBaja ? 'Firma de quien autoriza' : 'Firma de quien recibe'}
            name="firmaRecibe"
            value={values.firmaRecibe}
            onChange={handleChange}
            error={touched.firmaRecibe ? errors.firmaRecibe : undefined}
            required
          />
        </div>
      ) : null}
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
