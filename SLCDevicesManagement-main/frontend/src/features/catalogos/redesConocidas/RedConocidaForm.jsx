import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

const BSSID_PATTERN = /^[0-9a-fA-F]{2}(:[0-9a-fA-F]{2}){5}$/;

function validateRedConocida(values) {
  const errors = {};
  enforceRequired(errors, values, 'bssid', 'bssid');
  enforceMaxLength(errors, values, 'bssid', 'bssid', 17);
  enforceRequired(errors, values, 'idUbicacion', 'id ubicacion');

  if (values.bssid && !BSSID_PATTERN.test(values.bssid.trim())) {
    errors.bssid = 'El campo bssid debe tener el formato aa:bb:cc:dd:ee:ff.';
  }

  return errors;
}

export function RedConocidaForm({ initialValues, ubicacionOptions, onSubmit, onCancel, isSubmitting }) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateRedConocida,
  });

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <TextField
        label="BSSID"
        name="bssid"
        value={values.bssid}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.bssid ? errors.bssid : undefined}
        placeholder="aa:bb:cc:dd:ee:ff"
        required
      />
      <SelectField
        label="Ubicación"
        name="idUbicacion"
        value={values.idUbicacion}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idUbicacion ? errors.idUbicacion : undefined}
        options={ubicacionOptions}
        required
      />
      <FormActions onCancel={onCancel} submitLabel="Guardar" isSubmitting={isSubmitting} />
    </form>
  );
}
