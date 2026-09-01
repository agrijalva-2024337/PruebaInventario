import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { TextareaField } from '@/shared/components/TextareaField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

export function MovimientoForm({
  kind,
  initialValues,
  activoOptions,
  responsableOptions,
  ubicacionOptions,
  estadoOptions,
  onSubmit,
  onCancel,
  isSubmitting,
}) {
  const isBaja = kind === 'baja';
  const isMantenimiento = kind === 'mantenimiento';
  const needsUbicacion = !isBaja;

  function validate(values) {
    const errors = {};
    enforceRequired(errors, values, 'idActivo', 'id activo');
    enforceRequired(errors, values, 'idResponsable', 'id responsable');
    enforceRequired(errors, values, 'idEstado', 'id estado');
    enforceRequired(errors, values, 'fechaAsignacion', 'fecha');
    if (needsUbicacion) {
      enforceRequired(errors, values, 'idUbicacion', 'id ubicacion');
    }
    if (isMantenimiento) {
      enforceRequired(errors, values, 'observaciones', 'observaciones');
    }
    if (isBaja) {
      enforceRequired(errors, values, 'motivo', 'motivo');
      enforceRequired(errors, values, 'documentoPdfUrl', 'documento');
    }
    enforceMaxLength(errors, values, 'observaciones', 'observaciones', 300);
    enforceMaxLength(errors, values, 'motivo', 'motivo', 300);
    enforceMaxLength(errors, values, 'documentoPdfUrl', 'documento', 300);
    return errors;
  }

  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate,
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
      <SelectField
        label={isBaja ? 'Autoriza (responsable)' : 'Responsable que recibe'}
        name="idResponsable"
        value={values.idResponsable}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idResponsable ? errors.idResponsable : undefined}
        options={responsableOptions}
        required
      />
      {needsUbicacion ? (
        <SelectField
          label="Ubicación de uso"
          name="idUbicacion"
          value={values.idUbicacion}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.idUbicacion ? errors.idUbicacion : undefined}
          options={ubicacionOptions}
          required
        />
      ) : null}
      <SelectField
        label="Estado"
        name="idEstado"
        value={values.idEstado}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idEstado ? errors.idEstado : undefined}
        options={estadoOptions}
        required
      />
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
          <TextareaField
            label="Motivo"
            name="motivo"
            value={values.motivo}
            onChange={handleChange}
            onBlur={handleBlur}
            error={touched.motivo ? errors.motivo : undefined}
            required
          />
          <TextField
            label="Documento de referencia"
            name="documentoPdfUrl"
            value={values.documentoPdfUrl}
            onChange={handleChange}
            onBlur={handleBlur}
            error={touched.documentoPdfUrl ? errors.documentoPdfUrl : undefined}
            required
            placeholder="Folio, URL o número de acta"
          />
        </>
      ) : (
        <TextareaField
          label="Observaciones"
          name="observaciones"
          value={values.observaciones}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.observaciones ? errors.observaciones : undefined}
          required={isMantenimiento}
        />
      )}
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
