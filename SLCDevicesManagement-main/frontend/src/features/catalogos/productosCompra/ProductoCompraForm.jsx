import { FormActions } from '@/shared/components/FormActions';
import { SelectField } from '@/shared/components/SelectField';
import { TextField } from '@/shared/components/TextField';
import { TextareaField } from '@/shared/components/TextareaField';
import { useForm } from '@/shared/hooks/useForm';
import { enforceMaxLength, enforceRequired } from '@/shared/utils/fieldErrors';

function validateProducto(values) {
  const errors = {};
  enforceRequired(errors, values, 'idProveedor', 'id proveedor');
  enforceRequired(errors, values, 'nombre', 'nombre');
  enforceMaxLength(errors, values, 'nombre', 'nombre', 150);
  enforceMaxLength(errors, values, 'marca', 'marca', 100);
  enforceMaxLength(errors, values, 'modelo', 'modelo', 100);
  enforceMaxLength(errors, values, 'descripcion', 'descripcion', 500);
  enforceRequired(errors, values, 'fechaCompra', 'fecha compra');
  enforceRequired(errors, values, 'fechaVencimientoGarantia', 'fecha vencimiento garantia');
  enforceMaxLength(errors, values, 'moneda', 'moneda', 10);
  enforceMaxLength(errors, values, 'numeroFactura', 'numero factura', 50);
  if (values.costoUnitario === '' || Number.isNaN(Number(values.costoUnitario))) {
    errors.costoUnitario = 'El campo costo unitario es obligatorio';
  }
  return errors;
}

export function ProductoCompraForm({
  initialValues,
  proveedorOptions,
  categoriaOptions,
  onSubmit,
  onCancel,
  isSubmitting,
}) {
  const { values, errors, touched, handleChange, handleBlur, handleSubmit } = useForm({
    initialValues,
    validate: validateProducto,
  });

  return (
    <form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
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
        label="Categoría (opcional)"
        name="idCategoriaActivo"
        value={values.idCategoriaActivo}
        onChange={handleChange}
        onBlur={handleBlur}
        options={categoriaOptions}
        placeholder="Sin categoría"
      />
      <TextField
        label="Nombre del producto"
        name="nombre"
        value={values.nombre}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.nombre ? errors.nombre : undefined}
        required
      />
      <div className="grid gap-4 sm:grid-cols-2">
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
          label="Costo unitario"
          name="costoUnitario"
          type="number"
          step="0.01"
          min="0"
          value={values.costoUnitario}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.costoUnitario ? errors.costoUnitario : undefined}
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
          label="Número de factura"
          name="numeroFactura"
          value={values.numeroFactura}
          onChange={handleChange}
          onBlur={handleBlur}
          error={touched.numeroFactura ? errors.numeroFactura : undefined}
        />
      </div>
      <TextareaField
        label="Especificaciones (opcional)"
        name="descripcion"
        value={values.descripcion}
        onChange={handleChange}
        onBlur={handleBlur}
        error={touched.descripcion ? errors.descripcion : undefined}
        placeholder="RAM, disco, procesador…"
      />
      <FormActions onCancel={onCancel} isSubmitting={isSubmitting} />
    </form>
  );
}
