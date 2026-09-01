import { useId } from 'react';
import { Button } from '@/shared/components/Button';
import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { useForm } from '@/shared/hooks/useForm';
import { RolUsuario, rolUsuarioLabel } from '@/shared/api/contracts';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';
import { generatePassword } from '@/features/organizacion/usuarios/generatePassword';

const ROL_OPTIONS = [
  { value: RolUsuario.Consulta, label: rolUsuarioLabel[RolUsuario.Consulta] },
  { value: RolUsuario.OperadorInventario, label: rolUsuarioLabel[RolUsuario.OperadorInventario] },
  { value: RolUsuario.AdministradorEmpresa, label: rolUsuarioLabel[RolUsuario.AdministradorEmpresa] },
  { value: RolUsuario.AdministradorGeneral, label: rolUsuarioLabel[RolUsuario.AdministradorGeneral] },
];

function isAdminGeneral(rol) {
  return Number(rol) === RolUsuario.AdministradorGeneral;
}

function validateUsuario(values, { isCreate }) {
  const errors = {};
  enforceRequired(errors, values, 'nombres', 'nombres');
  enforceMaxLength(errors, values, 'nombres', 'nombres', 100);
  enforceRequired(errors, values, 'apellidos', 'apellidos');
  enforceMaxLength(errors, values, 'apellidos', 'apellidos', 100);
  enforceRequired(errors, values, 'correo', 'correo');
  enforceMaxLength(errors, values, 'correo', 'correo', 150);
  enforceRequired(errors, values, 'username', 'username');
  enforceMaxLength(errors, values, 'username', 'username', 50);
  enforceRequired(errors, values, 'rol', 'rol');

  if (!isAdminGeneral(values.rol)) {
    enforceRequired(errors, values, 'idEmpresa', 'id empresa');
  }

  if (isCreate) {
    enforceRequired(errors, values, 'password', 'password');
  }

  if (values.password && String(values.password).length < 8) {
    errors.password = 'El campo password debe tener al menos 8 caracteres';
  }

  enforceMaxLength(errors, values, 'password', 'password', 128);
  return errors;
}

export function UsuarioForm({
  initialValues,
  empresaOptions,
  isCreate,
  allowAdminGeneral,
  onSubmit,
  onCancel,
  isSubmitting,
}) {
  const passwordId = useId();
  const rolOptions = allowAdminGeneral
    ? ROL_OPTIONS
    : ROL_OPTIONS.filter((option) => option.value !== RolUsuario.AdministradorGeneral);

  const { values, errors, touched, handleChange, handleBlur, handleSubmit, setFieldValue } = useForm({
    initialValues,
    validate: (next) => validateUsuario(next, { isCreate }),
  });

  function fillGeneratedPassword() {
    setFieldValue('password', generatePassword());
  }

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid gap-4 sm:grid-cols-2">
        <TextField
          label="Nombres"
          name="nombres"
          value={values.nombres}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.nombres ? errors.nombres : undefined}
          required
        />
        <TextField
          label="Apellidos"
          name="apellidos"
          value={values.apellidos}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.apellidos ? errors.apellidos : undefined}
          required
        />
      </div>
      <TextField
        label="Correo"
        name="correo"
        type="email"
        value={values.correo}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.correo ? errors.correo : undefined}
        required
        autoComplete="off"
      />
      <TextField
        label="Usuario"
        name="username"
        value={values.username}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.username ? errors.username : undefined}
        required
        autoComplete="off"
      />
      <SelectField
        label="Rol"
        name="rol"
        value={values.rol}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.rol ? errors.rol : undefined}
        options={rolOptions}
        required
      />
      <SelectField
        label="Empresa"
        name="idEmpresa"
        value={values.idEmpresa}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.idEmpresa ? errors.idEmpresa : undefined}
        options={empresaOptions}
        required={!isAdminGeneral(values.rol)}
        placeholder={isAdminGeneral(values.rol) ? 'Sin empresa (todas)' : 'Seleccionar...'}
      />
      <div>
        <label className="block text-sm font-medium text-slate-700" htmlFor={passwordId}>
          {isCreate ? 'Contraseña' : 'Nueva contraseña (vacío = no cambiar)'}
          {isCreate ? <span className="text-red-600"> *</span> : null}
        </label>
        <div className="mt-1 flex gap-2">
          <input
            id={passwordId}
            name="password"
            type="text"
            value={values.password}
            onChange={handleChange}
            onBlur={handleBlur}
            required={isCreate}
            autoComplete="new-password"
            spellCheck={false}
            className="w-full rounded-md border border-slate-200 bg-white px-3 py-2 font-mono text-sm"
            aria-invalid={Boolean(touched.password && errors.password)}
          />
          <Button type="button" variant="secondary" className="shrink-0" onClick={fillGeneratedPassword}>
            Generar
          </Button>
        </div>
        {touched.password && errors.password ? (
          <p className="mt-1 text-xs text-red-600">{errors.password}</p>
        ) : (
          <p className="mt-1 text-xs text-slate-500">
            Generar rellena el campo. Cópiala ahora: al guardar solo queda el hash.
          </p>
        )}
      </div>
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
