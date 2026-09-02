import { Button } from '@/shared/components/Button';

export function FormActions({
  onCancel,
  submitLabel = 'Guardar',
  cancelLabel = 'Cancelar',
  isSubmitting = false,
  submitDisabled = false,
}) {
  return (
    <div className="flex justify-end gap-2">
      <Button variant="secondary" onClick={onCancel} disabled={isSubmitting} type="button">
        {cancelLabel}
      </Button>
      <Button type="submit" disabled={isSubmitting || submitDisabled}>
        {isSubmitting ? 'Guardando...' : submitLabel}
      </Button>
    </div>
  );
}
