import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

function validateProveedor(values) {
  const errors = {};
  enforceRequired(errors, values, 'idEmpresa', 'id empresa');
  enforceRequired(errors, values, 'nombre', 'nombre');
  enforceMaxLength(errors, values, 'nombre', 'nombre', 150);
  enforceRequired(errors, values, 'nit', 'nit');
  enforceMaxLength(errors, values, 'nit', 'nit', 50);
  enforceMaxLength(errors, values, 'nombreContacto', 'nombre contacto', 100);
  enforceMaxLength(errors, values, 'telefono', 'telefono', 30);
  enforceMaxLength(errors, values, 'correo', 'correo', 150);
  return errors;
}

export function ProveedorForm({ initialValues, empresaOptions, onSubmit, onCancel, isSubmitting }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateProveedor,
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
        label="NIT"
        name="nit"
        value={values.nit}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.nit ? errors.nit : undefined}
        required
      />
      <TextField
        label="Nombre de contacto"
        name="nombreContacto"
        value={values.nombreContacto}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.nombreContacto ? errors.nombreContacto : undefined}
      />
      <TextField
        label="Teléfono"
        name="telefono"
        value={values.telefono}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.telefono ? errors.telefono : undefined}
      />
      <TextField
        label="Correo"
        name="correo"
        type="text"
        value={values.correo}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.correo ? errors.correo : undefined}
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
