import { Button } from '@/shared/components/Button';

export function CatalogRowActions({ row, onEdit, onInactivate, onReactivate, onDelete, extra }) {
  return (
    <div className="flex flex-wrap justify-end gap-1" onClick={(event) => event.stopPropagation()}>
      {extra}
      {onEdit ? (
        <Button variant="ghost" onClick={() => onEdit(row)}>
          Editar
        </Button>
      ) : null}
      {onDelete ? (
        <Button variant="danger" onClick={() => onDelete(row)}>
          Eliminar
        </Button>
      ) : null}
      {onInactivate && row.habilitado ? (
        <Button variant="danger" onClick={() => onInactivate(row)}>
          Inactivar
        </Button>
      ) : null}
      {onReactivate && row.habilitado === false ? (
        <Button variant="secondary" onClick={() => onReactivate(row)}>
          Reactivar
        </Button>
      ) : null}
    </div>
  );
}
