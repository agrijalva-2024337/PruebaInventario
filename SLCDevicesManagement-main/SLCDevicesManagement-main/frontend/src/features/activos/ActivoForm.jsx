import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { TextareaField } from '@/shared/components/TextareaField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

function validateActivo(values) {
  const errors = {};
  enforceRequired(errors, values, 'idCategoriaActivo', 'id categoria activo');
  enforceRequired(errors, values, 'idProveedor', 'id proveedor');
  enforceRequired(errors, values, 'idUbicacion', 'id ubicacion');
  enforceRequired(errors, values, 'nombre', 'nombre');
  enforceMaxLength(errors, values, 'nombre', 'nombre', 150);
  enforceMaxLength(errors, values, 'descripcion', 'descripcion', 300);
  enforceMaxLength(errors, values, 'marca', 'marca', 100);
  enforceMaxLength(errors, values, 'modelo', 'modelo', 100);
  enforceMaxLength(errors, values, 'numeroSerie', 'numero serie', 100);
  enforceRequired(errors, values, 'fechaCompra', 'fecha compra');
  enforceRequired(errors, values, 'fechaVencimientoGarantia', 'fecha vencimiento garantia');
  enforceMaxLength(errors, values, 'moneda', 'moneda', 10);
  enforceMaxLength(errors, values, 'numeroFactura', 'numero factura', 50);
  enforceMaxLength(errors, values, 'observaciones', 'observaciones', 500);
  if (values.costoAdquisicion === '' || Number.isNaN(Number(values.costoAdquisicion))) {
    errors.costoAdquisicion = 'El campo costo adquisicion es obligatorio';
  }
  return errors;
}

export function ActivoForm({
  initialValues,
  categoriaOptions,
  proveedorOptions,
  ubicacionOptions,
  onSubmit,
  onCancel,
  isSubmitting,
}) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateActivo,
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
      <div className="grid gap-4 sm:grid-cols-3">
        <SelectField
          label="Categoría"
          name="idCategoriaActivo"
          value={values.idCategoriaActivo}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.idCategoriaActivo ? errors.idCategoriaActivo : undefined}
          options={categoriaOptions}
          required
        />
        <SelectField
          label="Proveedor"
          name="idProveedor"
          value={values.idProveedor}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.idProveedor ? errors.idProveedor : undefined}
          options={proveedorOptions}
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
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        <TextField
          label="Marca"
          name="marca"
          value={values.marca}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.marca ? errors.marca : undefined}
        />
        <TextField
          label="Modelo"
          name="modelo"
          value={values.modelo}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.modelo ? errors.modelo : undefined}
        />
        <TextField
          label="Número de serie"
          name="numeroSerie"
          value={values.numeroSerie}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.numeroSerie ? errors.numeroSerie : undefined}
        />
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        <TextField
          label="Fecha de compra"
          name="fechaCompra"
          type="date"
          value={values.fechaCompra}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.fechaCompra ? errors.fechaCompra : undefined}
          required
        />
        <TextField
          label="Vence garantía"
          name="fechaVencimientoGarantia"
          type="date"
          value={values.fechaVencimientoGarantia}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.fechaVencimientoGarantia ? errors.fechaVencimientoGarantia : undefined}
          required
        />
      </div>
      <div className="grid gap-4 sm:grid-cols-3">
        <TextField
          label="Costo"
          name="costoAdquisicion"
          type="number"
          step="0.01"
          min="0"
          value={values.costoAdquisicion}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.costoAdquisicion ? errors.costoAdquisicion : undefined}
          required
        />
        <TextField
          label="Moneda"
          name="moneda"
          value={values.moneda}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.moneda ? errors.moneda : undefined}
          placeholder="GTQ"
        />
        <TextField
          label="Factura"
          name="numeroFactura"
          value={values.numeroFactura}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.numeroFactura ? errors.numeroFactura : undefined}
        />
      </div>
      <TextareaField
        label="Descripción"
        name="descripcion"
        value={values.descripcion}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.descripcion ? errors.descripcion : undefined}
      />
      <TextareaField
        label="Observaciones"
        name="observaciones"
        value={values.observaciones}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.observaciones ? errors.observaciones : undefined}
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
