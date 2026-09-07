import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { TextareaField } from '@/shared/components/TextareaField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired, requiredError } from '@/shared/utils/fieldErrors';

function enforceRequiredDecimal(errors, values, field, fieldLabel) {
  const value = values[field];
  if (value === '' || value == null || Number.isNaN(Number(value))) {
    errors[field] = requiredError(fieldLabel);
  }
}

function validateUbicacion(values) {
  const errors = {};
  enforceRequired(errors, values, 'idSede', 'id sede');
  enforceRequired(errors, values, 'nombre', 'nombre');
  enforceMaxLength(errors, values, 'nombre', 'nombre', 100);
  enforceMaxLength(errors, values, 'descripcion', 'descripcion', 200);
  enforceRequiredDecimal(errors, values, 'latitud', 'latitud');
  enforceRequiredDecimal(errors, values, 'longitud', 'longitud');
  const lat = Number(values.latitud);
  const lng = Number(values.longitud);
  if (!Number.isNaN(lat) && (lat < -90 || lat > 90)) {
    errors.latitud = 'La latitud debe estar entre -90 y 90.';
  }
  if (!Number.isNaN(lng) && (lng < -180 || lng > 180)) {
    errors.longitud = 'La longitud debe estar entre -180 y 180.';
  }
  if (!Number.isNaN(lat) && !Number.isNaN(lng) && Math.abs(lat) < 0.05 && Math.abs(lng) < 0.05) {
    errors.latitud = '0, 0 cae en el océano. Usa las coordenadas reales (ej. 14.63, -90.51).';
    errors.longitud = '0, 0 cae en el océano. Usa las coordenadas reales (ej. 14.63, -90.51).';
  }
  return errors;
}

export function UbicacionForm({ initialValues, sedeOptions, onSubmit, onCancel, isSubmitting }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateUbicacion,
  });

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <SelectField
        label="Sede"
        name="idSede"
        value={values.idSede}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idSede ? errors.idSede : undefined}
        options={sedeOptions}
        required
      />
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
      <TextField
        label="Latitud"
        name="latitud"
        type="number"
        step="any"
        value={values.latitud}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.latitud ? errors.latitud : undefined}
        required
      />
      <TextField
        label="Longitud"
        name="longitud"
        type="number"
        step="any"
        value={values.longitud}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.longitud ? errors.longitud : undefined}
        required
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
