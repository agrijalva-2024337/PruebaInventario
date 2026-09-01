import { Button } from '@/shared/components/Button';
import { Modal } from '@/shared/components/Modal';

export function ConfirmDialog({
  isOpen,
  onClose,
  onConfirm,
  title,
  message,
  confirmLabel = 'Eliminar',
  cancelLabel = 'Cancelar',
  variant = 'danger',
  isConfirming = false,
}) {
  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={title}
      footer={
        <>
          <Button variant="secondary" onClick={onClose} disabled={isConfirming}>
            {cancelLabel}
          </Button>
          <Button variant={variant} onClick={onConfirm} disabled={isConfirming}>
            {isConfirming ? 'Procesando...' : confirmLabel}
          </Button>
        </>
      }
    >
      <p className="text-sm text-slate-600">{message}</p>
    </Modal>
  );
}
