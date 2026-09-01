import { FormActions } from '@/shared/components/FormActions';
import { TextField } from '@/shared/components/TextField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

function validateEmpresa(values) {
  const errors = {};
  enforceRequired(errors, values, 'nombre', 'nombre');
  enforceMaxLength(errors, values, 'nombre', 'nombre', 150);
  enforceRequired(errors, values, 'nitCodigo', 'nit codigo');
  enforceMaxLength(errors, values, 'nitCodigo', 'nit codigo', 50);
  enforceMaxLength(errors, values, 'direccion', 'direccion', 150);
  enforceMaxLength(errors, values, 'telefono', 'telefono', 30);
  return errors;
}

export function EmpresaForm({ initialValues, onSubmit, onCancel, isSubmitting }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateEmpresa,
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
      <TextField
        label="NIT / código"
        name="nitCodigo"
        value={values.nitCodigo}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.nitCodigo ? errors.nitCodigo : undefined}
        required
      />
      <TextField
        label="Dirección"
        name="direccion"
        value={values.direccion}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.direccion ? errors.direccion : undefined}
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
