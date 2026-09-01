import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

function validateSede(values) {
  const errors = {};
  enforceRequired(errors, values, 'idEmpresa', 'id empresa');
  enforceRequired(errors, values, 'idPais', 'id pais');
  enforceRequired(errors, values, 'nombre', 'nombre');
  enforceMaxLength(errors, values, 'nombre', 'nombre', 100);
  enforceMaxLength(errors, values, 'direccion', 'direccion', 100);
  enforceMaxLength(errors, values, 'ciudad', 'ciudad', 100);
  return errors;
}

export function SedeForm({
  initialValues,
  empresaOptions,
  paisOptions,
  onSubmit,
  onCancel,
  isSubmitting,
}) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateSede,
  });

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <SelectField
        label="Empresa"
        name="idEmpresa"
        value={values.idEmpresa}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idEmpresa ? errors.idEmpresa : undefined}
        options={empresaOptions}
        required
      />
      <SelectField
        label="País"
        name="idPais"
        value={values.idPais}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idPais ? errors.idPais : undefined}
        options={paisOptions}
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
      <TextField
        label="Dirección"
        name="direccion"
        value={values.direccion}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.direccion ? errors.direccion : undefined}
      />
      <TextField
        label="Ciudad"
        name="ciudad"
        value={values.ciudad}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.ciudad ? errors.ciudad : undefined}
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
