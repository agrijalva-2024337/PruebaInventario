import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

function validateResponsable(values) {
  const errors = {};
  enforceRequired(errors, values, 'idArea', 'id area');
  enforceRequired(errors, values, 'nombreCompleto', 'nombre completo');
  enforceMaxLength(errors, values, 'nombreCompleto', 'nombre completo', 150);
  enforceMaxLength(errors, values, 'cargo', 'cargo', 100);
  enforceMaxLength(errors, values, 'correo', 'correo', 150);
  enforceMaxLength(errors, values, 'telefono', 'telefono', 30);
  return errors;
}

export function ResponsableForm({ initialValues, areaOptions, onSubmit, onCancel, isSubmitting }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateResponsable,
  });

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <SelectField
        label="Área"
        name="idArea"
        value={values.idArea}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idArea ? errors.idArea : undefined}
        options={areaOptions}
        required
      />
      <TextField
        label="Nombre completo"
        name="nombreCompleto"
        value={values.nombreCompleto}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.nombreCompleto ? errors.nombreCompleto : undefined}
        required
      />
      <TextField
        label="Cargo"
        name="cargo"
        value={values.cargo}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.cargo ? errors.cargo : undefined}
      />
      <TextField
        label="Correo"
        name="correo"
        type="email"
        value={values.correo}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.correo ? errors.correo : undefined}
      />
      <TextField
        label="Teléfono"
        name="telefono"
        value={values.telefono}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.telefono ? errors.telefono : undefined}
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
